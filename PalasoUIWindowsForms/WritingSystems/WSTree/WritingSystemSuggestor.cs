using System.Collections.Generic;
using System.Linq;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems.WSTree
{
	public class WritingSystemSuggestor
	{
		/// <summary>
		/// Consider setting this to true in linguistic applications
		/// </summary>
		public bool SuggestIpa { get; set; }
		/// <summary>
		/// Consider setting this to true in linguistic applications
		/// </summary>
		public bool SuggestDialects { get; set; }

		public bool SuggestVoice { get; set; }

		public bool SuggestOther { get; set; }

		public IEnumerable<WritingSystemDefinition> OtherKnownWritingSystems { get; set; }


		public WritingSystemSuggestor()
		{
			OtherKnownWritingSystems = new WritingSystemFromWindowsLocaleProvider();
			SuppressSuggestionsForMajorWorldLanguages=true;
			SuggestIpa=true;
			SuggestDialects=true;
			SuggestOther = true;
			SuggestVoice=false;
		}

		/// <summary>
		/// When true, no suggestions will be made some languages which may be supplied by the OS
		/// but which are unlikely to be the study of language documentation efforst
		/// </summary>
		public bool SuppressSuggestionsForMajorWorldLanguages { get; set; }

		public IEnumerable<IWritingSystemDefinitionSuggestion> GetSuggestions(WritingSystemDefinition primary, IEnumerable<WritingSystemDefinition> existingWritingSystemsForLanguage)
		{
			if(string.IsNullOrEmpty(primary.ISO))
				yield break;

			if(SuppressSuggestionsForMajorWorldLanguages
			   && new[]{"en", "th", "es", "fr", "de", "hi", "id", "vi","my","pt", "fi", "ar", "it","sv", "ja", "ko", "ch", "nl", "ru"}.Contains(primary.ISO))
				yield break;

			if (SuggestIpa && IpaSuggestion.ShouldSuggest(existingWritingSystemsForLanguage))
			{
				yield return new IpaSuggestion(primary);
			}

			if (SuggestVoice && VoiceSuggestion.ShouldSuggest(existingWritingSystemsForLanguage))
			{
				yield return new VoiceSuggestion(primary);
			}

			if (SuggestDialects)
			{
				yield return new DialectSuggestion(primary);
			}

			if (SuggestOther)
			{
				yield return new OtherSuggestion(primary);
			}
		}


		public IEnumerable<IWritingSystemDefinitionSuggestion> GetOtherLanguageSuggestions(IEnumerable<WritingSystemDefinition> existingDefinitions)
		{
			if (OtherKnownWritingSystems != null)
			{
				foreach (WritingSystemDefinition language in OtherKnownWritingSystems)
				{
					if (!existingDefinitions.Any(def => def.RFC5646 == language.RFC5646))
						yield return new LanguageSuggestion(language);
				}
			}
		}
	}
}