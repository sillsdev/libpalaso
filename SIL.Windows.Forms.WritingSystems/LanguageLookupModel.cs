using System.Collections.Generic;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems
{
	public class LanguageLookupModel
	{
		private LanguageLookup _languageLookup;

		/// <summary>
		/// Maybe this doesn't belong here... using it in Bloom which is more concerned with language than writing system
		/// </summary>
		/// <param name="iso639Code"></param>
		/// <returns></returns>
		public LanguageSubtag GetExactLanguageMatch(string iso639Code)
		{
			LanguageSubtag language;
			if (StandardSubtags.TryGetLanguageFromIso3Code(iso639Code, out language) && language.Code != language.Iso3Code)
				return language;
			return null;
		}

		public IEnumerable<LanguageInfo> GetMatchingLanguages(string typedText)
		{
			if (_languageLookup == null)
				_languageLookup = new LanguageLookup();

			return _languageLookup.SuggestLanguages(typedText);
		}

		public LanguageInfo LanguageInfo { get; set; }

		public string LanguageTag
		{
			get
			{
				if (LanguageInfo == null)
					return string.Empty;
				return LanguageInfo.LanguageTag;
			}
		}
	}
}