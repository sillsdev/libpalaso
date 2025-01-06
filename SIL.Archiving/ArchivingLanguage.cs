using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using SIL.WritingSystems;

namespace SIL.Archiving
{
	/// <summary></summary>
	public class ArchivingLanguage : IComparable
	{
		private string m_iso3Code;    // ex. "eng"
		private string m_englishName; // ex. "English"

		/// <summary></summary>
		public ArchivingLanguage(string iso3Code)
		{
			Iso3Code = iso3Code;
		}

		/// <summary></summary>
		public ArchivingLanguage(string iso3Code, string languageName)
		{
			LanguageName = languageName;
			Iso3Code = iso3Code;
		}

		/// <summary></summary>
		[PublicAPI]
		public ArchivingLanguage(string iso3Code, string languageName, string englishName)
		{
			LanguageName = languageName;
			Iso3Code = iso3Code;
			EnglishName = englishName;
		}

		/// <summary></summary>
		public string LanguageName { get; set; }

		/// <summary></summary>
		public string Iso3Code
		{
			get => m_iso3Code;
			set => m_iso3Code = value.ToLower();
		}

		/// <summary></summary>
		public string EnglishName
		{
			get
			{
				if (string.IsNullOrEmpty(m_englishName))
				{
					var returnVal = string.Empty;

					// look for the language name in culture info
					var culture = CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(
						c => c.ThreeLetterISOLanguageName == m_iso3Code);

					if (culture != null)
					{
						returnVal = culture.EnglishName;

						// The name returned first might be something like "English (United States)," so
						// search the parent cultures for the base culture which will return "English."
						while ((culture.Parent.ThreeLetterISOLanguageName == m_iso3Code)
							&& (culture.Parent.ThreeLetterISOLanguageName != "ivl"))
						{
							culture = culture.Parent;
							returnVal = culture.EnglishName;
						}
					}

					m_englishName = returnVal;
				}

				if (string.IsNullOrEmpty(m_englishName))
				{
					// Not very efficient, but this is not very performance-critical.
					// And it's undesirable to crash if someone uses a language that's not windows-standard.
					// It's not guaranteed that DesiredName is an English name, but it's the best we can do AFAIK.
					var lookup = new LanguageLookup();
					var lang = lookup.GetLanguageFromCode(m_iso3Code);
					m_englishName = lang?.DesiredName;
				}

				// throw an exception if no name is found
				if (string.IsNullOrEmpty(m_englishName))
				{
					throw new CultureNotFoundException(
						$"Could not determine the language for ISO3 code \"{m_iso3Code}.\" " +
						$"Perhaps you need to set the {nameof(EnglishName)} value explicitly.");
				}

				return m_englishName;
			}
			set => m_englishName = value;
		}

		/// <summary>The script used for the Subject Language. Ex. "Latn:Latin"</summary>
		public string Script { get; set; }

		/// <summary>The dialect used for the language. Ex. "03035:Standard; deu"</summary>
		public string Dialect { get; set; }

		/// <summary>Compare an ArchivingLanguage to an object. They are identical if the object
		/// is an ArchivingLanguage with the same Iso3Code and LanguageName</summary>
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			if (!(obj is ArchivingLanguage other))
				throw new ArgumentException();

			// first compare the Iso3Code
			int result = string.Compare(Iso3Code, other.Iso3Code, StringComparison.Ordinal);
			if (result != 0) return result;

			// if the same Iso3Code, compare the LanguageName
			return String.Compare(LanguageName, other.LanguageName, StringComparison.Ordinal);
		}

		/// <summary>Compare 2 ArchivingLanguage objects. They are identical if they have the same
		/// Iso3Code and LanguageName</summary>
		public static int Compare(ArchivingLanguage langA, ArchivingLanguage langB)
		{
			return langA.CompareTo(langB);
		}
	}

	/// <summary>Compare 2 LanguageString objects. They are identical if they have the same Iso3LanguageId</summary>
	public class ArchivingLanguageComparer : IEqualityComparer<ArchivingLanguage>
	{
		public bool Equals(ArchivingLanguage x, ArchivingLanguage y)
		{
			return (x.CompareTo(y) == 0);
		}

		public int GetHashCode(ArchivingLanguage obj)
		{
			return (obj.Iso3Code + obj.LanguageName).GetHashCode();
		}
	}

	/// <summary>Simplify creating and managing ArchivingLanguage collections</summary>
	public class ArchivingLanguageCollection : HashSet<ArchivingLanguage>
	{
		/// <summary>Default constructor</summary>
		public ArchivingLanguageCollection()
			: base(new ArchivingLanguageComparer())
		{
			// additional constructor code can go here
		}
	}

}
