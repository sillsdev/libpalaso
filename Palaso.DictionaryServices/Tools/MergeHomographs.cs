using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palaso.Code;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.Progress.LogBox;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Tools
{
	public class MergeHomographs : Tool
	{
		private IProgress _progress;
		public override void Run(string inputLiftPath, string outputLiftPath, IProgress progress)
		{
			_progress = progress;
			File.Copy(inputLiftPath,outputLiftPath,true);
			RequireThat.File(outputLiftPath).Exists();

			using (var repo = new Palaso.DictionaryServices.LiftLexEntryRepository(outputLiftPath))
			{
				var writingSystemForMatching = new WritingSystemDefinition("en");

				progress.WriteMessage("Starting with {0} entries...", repo.Count);
				Merge(repo, writingSystemForMatching);
				progress.WriteMessage("Ended with {0} entries...", repo.Count);
			}
		}

		private void Merge(LiftLexEntryRepository repo, WritingSystemDefinition writingSystemForMatching)
		{
		   var alreadyProcessed = new List<RepositoryId>();

			var ids = new List<RepositoryId>(repo.GetAllItems());
			for (int i = 0; i < ids.Count; i++)
			{
				if (alreadyProcessed.Contains(ids[i]))
					continue;
				alreadyProcessed.Add(ids[i]);
				var entry = repo.GetItem(ids[i]);
				var matches = repo.GetEntriesWithMatchingLexicalForm(entry.LexicalForm.GetExactAlternative(writingSystemForMatching.Id), writingSystemForMatching);

				//at this point we have entries which match along a single ws axis. We may or may not be able to merge them...

				foreach (RecordToken<LexEntry> incomingMatch in matches)
				{
					if (alreadyProcessed.Contains(incomingMatch.Id))
						continue;//we'll be here at least as each element matches itself

					alreadyProcessed.Add(incomingMatch.Id);
					if (EntryMerger.TryMergeEntries(entry, incomingMatch.RealObject, _progress))
					{
						repo.DeleteItem(incomingMatch.RealObject);
					}
					repo.SaveItem(entry);
				}
			}
		}

		public override string ToString()
		{
			return "Merge Homographs (Brutal)";
		}
	}
}