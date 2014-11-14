﻿using System.Collections.Generic;
using System.Linq;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public class LookupIsoCodeModel
	{
		private readonly IList<Iso639LanguageCode> _languageCodes;
		private Iso639LanguageCode _selectedWritingSystem;
		private EthnologueLookup _ethnologueLookup;


		public LookupIsoCodeModel()
		{
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
				 _ethnologueLookup = new EthnologueLookup();

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
			foreach (var language in    _ethnologueLookup.SuggestLanguages(typedText))
			{
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
