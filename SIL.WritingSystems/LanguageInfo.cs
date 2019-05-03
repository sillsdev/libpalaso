using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SIL.WritingSystems
{

	/// <summary>
	/// If you add a new data field to LanguageInfo:
	///    add it to the LanguageInfo in LanguageData.LanguageDataIndex()
	///       that is used to create LanguageDataIndex.txt
	///    add the field to WriteIndex and WriteJson to get it into LanguageDataIndex.txt and
	///       LanguageDataIndex.json respectively
	///
	/// This is required for web apps to use the same language data as libpalaso consumers
	///
	/// LanguageLookup uses the LanguageInfo to search for languages
	/// LanguageLookup() needs to read and add the new field from LanguageDataIndex to the LanguageInfo
	/// change the check for number of items in a record from LanguageDataIndex
	/// </summary>
	[DebuggerDisplay("{LanguageTag}")]
	public class LanguageInfo
	{
		private readonly List<string> _names = new List<string>();
		private readonly HashSet<string> _countries = new HashSet<string>();
		private string _desiredName;

		public string LanguageTag { get; set; }
		public string ThreeLetterTag { get; set; }

		/// <summary>
		/// This allows apps to filter out macro languages
		/// </summary>
		public bool IsMacroLanguage { get; set; }


		/// <summary>
		/// List of alternative names for the language
		/// </summary>
		public IList<string> Names
		{
			get { return _names; }
		}


		/// <summary>
		/// List of countries that the language is natively spoken in
		/// </summary>
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
