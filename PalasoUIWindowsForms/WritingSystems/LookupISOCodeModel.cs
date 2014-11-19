using System;
using System.Collections.Generic;
using System.Linq;
using L10NSharp;
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
			if (_ethnologueLookup == null)
				_ethnologueLookup = new EthnologueLookup { Force3LetterCodes = Force3LetterCodes };

			/* This works, but the results are satisfactory yet (they could be with some enancement to the matcher
			 We would need it to favor exact prefix matches... currently an exact match could be several items down the list.

			var d = new ApproximateMatcher.GetStringDelegate<WritingSystemDefinition.Iso639LanguageCode>(c => c.Name);
			var languages = ApproximateMatcher.FindClosestForms(_languageCodes, d, s, ApproximateMatcherOptions.IncludePrefixedAndNextClosestForms);
			*/

			//            typedText = typedText.ToLowerInvariant();
			//
			//            foreach (Iso639LanguageCode lang in _languageCodes)
			//            {
			//                if (string.IsNullOrEmpty(typedText) // in which case, show all of them
			//                    || (lang.InvariantLowerCaseCode.StartsWith(typedText)
			//                        || lang.Name.ToLowerInvariant().StartsWith(typedText)))
			//                {
			//                    yield return lang;
			//                }
			//            }

			// Users were having problems when they looked up things like "English" and were presented with "United Arab Emirates"
			// and such, as these colonial languages are spoken in so many countries. So this just displays the number of countries.
			foreach (var language in _ethnologueLookup.SuggestLanguages(typedText))
			{
				if (language.CountryCount > 2) // 3 or more was chosen because generally 2 languages fit in the space allowed
				{
					var msg = LocalizationManager.GetString("LanguageLookup.CountryCount", "{0} Countries", "Shown when there are multiple countries and it is just confusing to list them all.");
					language.Country = string.Format(msg, language.CountryCount);
				}
				yield return language;
			}
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