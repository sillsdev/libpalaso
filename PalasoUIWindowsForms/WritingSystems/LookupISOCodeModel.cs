using System.Collections.Generic;
using System.Linq;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public class LookupIsoCodeModel
	{
		private readonly IList<Iso639LanguageCode> _languageCodes;
		private Iso639LanguageCode _selectedWritingSystem;
		private EthnologueLookup _ethnologueLookup;

		/// <summary>Force the dialog to return 3 letter iso codes even if a 2 letter code is available</summary>
		public bool Force3LetterCodes { get; set; }

		public LookupIsoCodeModel()
		{
			Force3LetterCodes = false;
			_languageCodes = StandardTags.ValidIso639LanguageCodes;
		}

		/// <summary>
		/// Maybe this doesn't belong here... using it in Bloom which is more concerned with language than writing system
		/// </summary>
		/// <param name="iso639Code"></param>
		/// <returns></returns>
		public Iso639LanguageCode GetExactLanguageMatch(string iso639Code)
		{
			iso639Code = iso639Code.ToLowerInvariant();
			return _languageCodes.FirstOrDefault(
				code => code.InvariantLowerCaseCode == iso639Code
				);
		}

		public IEnumerable<LanguageInfo> GetMatchingLanguages(string typedText)
		{
			if(_ethnologueLookup==null)
				_ethnologueLookup = new EthnologueLookup { Force3LetterCodes = Force3LetterCodes };

			return _ethnologueLookup.SuggestLanguages(typedText);
		}


		public LanguageInfo LanguageInfo;

		public string ISOCode
		{
			get
			{
				if (LanguageInfo == null)
					return string.Empty;
				return LanguageInfo.Code;
			}
		}
	}
}
