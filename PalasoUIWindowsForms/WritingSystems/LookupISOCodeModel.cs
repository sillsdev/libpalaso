using System.Collections.Generic;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	///<summary>
	/// Basically just handles selecting from likely options, based on what you type
	///</summary>
	public class LookupIsoCodeModel
	{
		private readonly IList<WritingSystemDefinition.LanguageCode> _languageCodes;

		public LookupIsoCodeModel()
		{
			_languageCodes = WritingSystemDefinition.LanguageCodes;
		}

		public IEnumerable<WritingSystemDefinition.LanguageCode> GetMatchingWritingSystems(string typedText)
		{
			/* This works, but the results are satisfactory yet (they could be with some enancement to the matcher
			 We would need it to favor exact prefix matches... currently an exact match could be several items down the list.

			var d = new ApproximateMatcher.GetStringDelegate<WritingSystemDefinition.LanguageCode>(c => c.Name);
			var languages = ApproximateMatcher.FindClosestForms(_languageCodes, d, s, ApproximateMatcherOptions.IncludePrefixedAndNextClosestForms);
			*/

			typedText = typedText.ToLowerInvariant();

			foreach (WritingSystemDefinition.LanguageCode lang in _languageCodes)
			{
				if (string.IsNullOrEmpty(typedText) // in which case, show all of them
					|| (lang.Code.ToLowerInvariant().StartsWith(typedText)
						|| lang.Name.ToLowerInvariant().StartsWith(typedText)))
				{
					yield return lang;
				}
			}
		}
	}
}
