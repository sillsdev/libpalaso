using System.Collections.Generic;
using System.Linq;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.Progress.LogBox;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Processors
{
	/// <summary>
	/// This is used by the "LiftTools" application. It finds all the homographs in
	/// a lexicon, and merges them if it can. It also merges their senses, if it can.
	/// </summary>
	public class HomographMerger
	{
		public static void Merge(LiftLexEntryRepository repo, string writingSystemIdForMatching, IProgress progress)
		{
		   var alreadyProcessed = new List<RepositoryId>();

			var ids = new List<RepositoryId>(repo.GetAllItems());
			for (int i = 0; i < ids.Count; i++)
			{
				if (alreadyProcessed.Contains(ids[i]))
					continue;
				alreadyProcessed.Add(ids[i]);
				var entry = repo.GetItem(ids[i]);
				var writingSystemForMatching = new WritingSystemDefinition(writingSystemIdForMatching);
				var matches = repo.GetEntriesWithMatchingLexicalForm(entry.LexicalForm.GetExactAlternative(writingSystemIdForMatching), writingSystemForMatching);

				//at this point we have entries which match along a single ws axis. We may or may not be able to merge them...

				foreach (RecordToken<LexEntry> incomingMatch in matches)
				{
					if (alreadyProcessed.Contains(incomingMatch.Id))
						continue;//we'll be here at least as each element matches itself

					alreadyProcessed.Add(incomingMatch.Id);
					if (EntryMerger.TryMergeEntries(entry, incomingMatch.RealObject, progress))
					{
						progress.WriteMessage("Merged two instances of {0}.", entry.GetSimpleFormForLogging());
						repo.DeleteItem(incomingMatch.RealObject);
						repo.SaveItem(entry);
					}
					else
					{
						progress.WriteMessage("Not merging two instances of {0}.", entry.GetSimpleFormForLogging());
					}
				}
			}



		}

		private class Counter
		{
			public Counter(string id)
			{
				Id = id;
				count = 1;
			}
			public string Id;
			public int count;
		}

		public static string GuessPrimarLexicalFormWritingSystem(LiftLexEntryRepository repo, IProgress progress)
		{
			var choices = new Dictionary<string, Counter>();

			var ids = repo.GetAllItems();
			for (int i = 0; i < 10 && i < ids.Length; i++)
			{
				var entry = repo.GetItem(ids[i]);
				foreach (var languageForm in entry.LexicalForm.Forms)
				{
					Counter counter;
					if (choices.TryGetValue(languageForm.WritingSystemId, out counter))
					{
						++counter.count;
					}
					else
					{
						choices.Add(languageForm.WritingSystemId, new Counter(languageForm.WritingSystemId));
					}
				}
			}
			if (choices.Count == 0)
			{
				progress.WriteError("Could not determine a primary writing system for matching entries.");
				return null;
			}
			var z = choices.OrderByDescending(p => p.Value.count).FirstOrDefault();

			return z.Value.Id;
		}



		public override string ToString()
		{
			return "Merge Homographs";
		}
	}
}