using System.Collections.Generic;
using System.Linq;

namespace SIL.WritingSystems
{
	public static class WritingSystemRepositoryExtensions
	{
		public static IEnumerable<WritingSystemDefinition> TextWritingSystems(this IWritingSystemRepository repo)
		{
			return repo.AllWritingSystems.Where(ws => !ws.IsVoice);
		}

		public static IEnumerable<WritingSystemDefinition> VoiceWritingSystems(this IWritingSystemRepository repo)
		{
			return repo.AllWritingSystems.Where(ws => ws.IsVoice);
		}

		public static IEnumerable<string> FilterForTextIetfLanguageTags(this IWritingSystemRepository repo, IEnumerable<string> langTagsToFilter)
		{
			var set = new HashSet<string>(repo.TextWritingSystems().Select(ws => ws.IetfLanguageTag));
			return langTagsToFilter.Where(set.Contains);
		}
	}
}
