using System.Collections.Generic;
using System.Linq;

namespace SIL.WritingSystems
{
	public class LanguageInfo
	{
		private readonly List<string> _names = new List<string>();
		private readonly HashSet<string> _countries = new HashSet<string>();
		private string _desiredName;

		public string LanguageTag { get; set; }
		public string ThreeLetterTag { get; set; }
		public bool IsMacroLanguage { get; set; }

		public IList<string> Names
		{
			get { return _names; }
		}

		public ISet<string> Countries
		{
			get { return _countries; }
		}

		/// <summary>
		/// The primary country where the language is spoken. In a LanguageInfo retrieved from
		/// LanguageLookup, this should match the main title in the
		/// Ethnologue website, where it says things like Romanian//A language of Romania
		/// or English//A language of United Kingdom
		/// </summary>
		public string PrimaryCountry { get; set; }

		/// <summary>
		/// Used by some apps (e.g., Bloom) as a convenient place to save the name a particular
		/// user wants to call the language in a particular context. Also serves as a shortcut
		/// for the first name.
		/// </summary>
		public string DesiredName
		{
			get
			{
				if (string.IsNullOrEmpty(_desiredName))
					return Names.FirstOrDefault();
				return _desiredName;
			}
			set { _desiredName = value; }
		}
	}
}
