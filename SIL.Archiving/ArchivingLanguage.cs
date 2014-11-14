using System.Globalization;
using System.Linq;

namespace SIL.Archiving
{
	/// ------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ------------------------------------------------------------------------------------
	public class ArchivingLanguage
	{
		protected string _iso3Code;    // ex. "eng"
		protected string _englishName; // ex. "English"

		/// ------------------------------------------------------------------------------------
		public ArchivingLanguage(string iso3Code)
		{
			_iso3Code = iso3Code;
		}

		/// ------------------------------------------------------------------------------------
		public ArchivingLanguage(string iso3Code, string englishName)
		{
			_iso3Code = iso3Code;
			_englishName = englishName;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The ISO3 code for the language. Ex. "eng"
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Iso3Code
		{
			get { return _iso3Code; }
			set { _iso3Code = value.ToLower(); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The English name of the language. Used for searching, human-readable. If not
		/// provided by the host software, an attempt will be made to determine the name
		/// based on the ISO3 code. Ex. "English"
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string EnglishName
		{
			get
			{
				if (string.IsNullOrEmpty(_englishName))
				{
					var returnVal = string.Empty;

					// look for the language name in culture info
					var culture = CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(
						c => c.ThreeLetterISOLanguageName == _iso3Code);

					if (culture != null)
					{
						returnVal = culture.EnglishName;

						// The name returned first might be something like "English (United States)," so
						// search the parent cultures for the base culture which will return "English."
						while ((culture.Parent.ThreeLetterISOLanguageName == _iso3Code)
							&& (culture.Parent.ThreeLetterISOLanguageName != "ivl"))
						{
							culture = culture.Parent;
							returnVal = culture.EnglishName;
						}
					}

					_englishName = returnVal;
				}

				// throw an exception if no name is found
				if (string.IsNullOrEmpty(_englishName))
					throw new CultureNotFoundException(string.Format("Could not determine the language for ISO3 code \"{0}.\" Perhaps you need to set the EnglishName value explicitly.", _iso3Code));

				return _englishName;
			}
			set { _englishName = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The script used for the Subject Language. Ex. "Latn:Latin"
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Script { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The dialect used for the language. Ex. "03035:Standard; deu"
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Dialect { get; set; }
	}
}
