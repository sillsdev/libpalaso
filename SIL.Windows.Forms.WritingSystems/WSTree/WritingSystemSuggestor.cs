using System;
using System.Collections.Generic;
using System.Linq;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems.WSTree
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

		public IEnumerable<Tuple<string, string>> OtherKnownWritingSystems { get; set; }

		private readonly IWritingSystemFactory _writingSystemFactory;

		public WritingSystemSuggestor(IWritingSystemFactory writingSystemFactory)
		{
			OtherKnownWritingSystems =
				new WritingSystemFromWindowsLocaleProvider(writingSystemFactory).Union(new List<Tuple<string, string>>()
			{
				new Tuple<string, string>("tpi", string.Empty)
			});

			_writingSystemFactory = writingSystemFactory;
			SuppressSuggestionsForMajorWorldLanguages = true;
			SuggestIpa = true;
			SuggestDialects = true;
			SuggestOther = true;
			SuggestVoice = false;
		}

		/// <summary>
		/// When true, no suggestions will be made some languages which may be supplied by the OS
		/// but which are unlikely to be the study of language documentation efforst
		/// </summary>
		public bool SuppressSuggestionsForMajorWorldLanguages { get; set; }

		public IEnumerable<IWritingSystemDefinitionSuggestion> GetSuggestions(WritingSystemDefinition primary, IEnumerable<WritingSystemDefinition> existingWritingSystemsForLanguage)
		{
			if (primary.Language == null && primary.Variants.Any(v => !v.IsPrivateUse))
				yield break;

			if (SuppressSuggestionsForMajorWorldLanguages
			   && new[]{"en", "th", "es", "fr", "de", "hi", "id", "vi","my","pt", "fi", "ar", "it","sv", "ja", "ko", "ch", "nl", "ru"}.Contains((string) primary.Language))
				yield break;

			WritingSystemDefinition[] existingWSs = existingWritingSystemsForLanguage.ToArray();

			if (SuggestIpa && IpaSuggestion.ShouldSuggest(existingWSs))
			{
				yield return new IpaSuggestion(_writingSystemFactory, primary);
			}

			if (SuggestVoice && VoiceSuggestion.ShouldSuggest(existingWSs))
			{
				yield return new VoiceSuggestion(_writingSystemFactory, primary);
			}

			if (SuggestDialects)
			{
				yield return new DialectSuggestion(_writingSystemFactory, primary);
			}

			if (SuggestOther)
			{
				yield return new OtherSuggestion(_writingSystemFactory, primary, existingWSs);
			}
		}


		public IEnumerable<IWritingSystemDefinitionSuggestion> GetOtherLanguageSuggestions(IEnumerable<WritingSystemDefinition> existingDefinitions)
		{
			if (OtherKnownWritingSystems != null)
			{
				WritingSystemDefinition[] wsArray = existingDefinitions.ToArray();
				foreach (var tuple in OtherKnownWritingSystems)
				{
					if (wsArray.All(def => def.LanguageTag != tuple.Item1))
						yield return new LanguageSuggestion(_writingSystemFactory, tuple.Item1, tuple.Item2);
				}
			}
		}
	}
}