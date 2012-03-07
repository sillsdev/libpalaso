using System;
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

		public static void Merge(LiftLexEntryRepository repo, string writingSystemIdForMatching, string[] traitsWithMultiplicity, StringBuilderProgress progress)
		{
			var alreadyProcessed = new List<RepositoryId>();

			var ids = new List<RepositoryId>(repo.GetAllItems());
			for (int i = 0; i < ids.Count; i++)
			{
				if (progress.CancelRequested)
				{
					throw new OperationCanceledException("User cancelled");
				}
				if (alreadyProcessed.Contains(ids[i]))
					continue;
				alreadyProcessed.Add(ids[i]);
				var entry = repo.GetItem(ids[i]);
				var writingSystemForMatching = WritingSystemDefinition.Parse(writingSystemIdForMatching);
				var matches = repo.GetEntriesWithMatchingLexicalForm(
					entry.LexicalForm.GetExactAlternative(writingSystemIdForMatching), writingSystemForMatching
				);

				//at this point we have entries which match along a single ws axis. We may or may not be able to merge them...

				var lexicalForm = entry.LexicalForm.GetExactAlternative(writingSystemForMatching.Id);
				if (matches.Count > 1) //>1 becuase each will match itself
				{
					progress.WriteMessageWithColor("gray", "Found {0} homograph(s) for {1}", matches.Count, lexicalForm);
				}
				var mergeCount = 0;
				var matchAlreadyProcessed = new List<RepositoryId>();
				foreach (RecordToken<LexEntry> incomingMatch in matches)
				{
					if (incomingMatch.Id == ids[i])
						continue; // The entry will match itself at least this time.
					if (matchAlreadyProcessed.Contains(incomingMatch.Id))
						continue; //we'll be here at least as each element matches itself

					matchAlreadyProcessed.Add(incomingMatch.Id);
					if (EntryMerger.TryMergeEntries(entry, incomingMatch.RealObject, traitsWithMultiplicity, progress))
					{
						mergeCount++;
						alreadyProcessed.Add(incomingMatch.Id);
						repo.DeleteItem(incomingMatch.RealObject);
						repo.SaveItem(entry);
					}
				}
				if (matches.Count > 1)
				{
					if (mergeCount == 0)
					{
						//progress.WriteMessageWithColor("gray", "Not merged.");
					}
					else
					{
						progress.WriteMessageWithColor("black", "Merged {0} homographs of {1}.", 1 + mergeCount,
													   lexicalForm);
					}
					progress.WriteMessage(""); //blank line
				}

			}

			MergeSensesWithinEntries(repo, traitsWithMultiplicity, progress);
		}

		/// <summary>
		/// it can happen that within a single entry, you can have mergable senses.
		/// </summary>
		private static void MergeSensesWithinEntries(LiftLexEntryRepository repo, string[] traitsWithMultiplicity, IProgress progress)
		{
			var ids = new List<RepositoryId>(repo.GetAllItems());
			foreach (var id in ids)
			{
				if (progress.CancelRequested)
				{
					throw new OperationCanceledException("User cancelled");
				}
				var entry = repo.GetItem(id);
				var senses = entry.Senses.ToArray();
				if(senses.Length < 2)
				{
					continue;
				}
				var sensesToRemove = new List<LexSense>();
				foreach (var sense in entry.Senses)
				{
					if (sensesToRemove.Contains(sense))
						continue;
					foreach (var otherSense in entry.Senses)
					{
						if (otherSense == sense) // Don't try and compare with ourself.
							continue;
						if (sensesToRemove.Contains(otherSense))
							continue;
						if (!SenseMerger.TryMergeSenseWithSomeExistingSense(sense, otherSense, traitsWithMultiplicity, progress))
							continue;
						sensesToRemove.Add(otherSense);
					}
				}
				foreach (var sense in sensesToRemove)
				{
					entry.Senses.Remove(sense);
					entry.IsDirty = true;
				}
				if (entry.IsDirty)
				{
					repo.SaveItem(entry);
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
			progress.WriteMessage("Looking at 1st 1000 entries to determine which Writing System to use for matching...");
			var choices = new Dictionary<string, Counter>();

			var ids = repo.GetAllItems();
			for (int i = 0; i < 1000 && i < ids.Length; i++)
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
			progress.WriteMessage("Will use '{0}' for matching.", z.Value.Id);

			return z.Value.Id;
		}



		public override string ToString()
		{
			return "Merge Homographs";
		}

	}
}