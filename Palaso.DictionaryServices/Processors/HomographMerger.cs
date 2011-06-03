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
		public static void Merge(LiftLexEntryRepository repo, string writingSystemIdForMatching, IProgress progress)
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
				var matches =
					repo.GetEntriesWithMatchingLexicalForm(
						entry.LexicalForm.GetExactAlternative(writingSystemIdForMatching), writingSystemForMatching);

				//at this point we have entries which match along a single ws axis. We may or may not be able to merge them...

				var lexicalForm = entry.LexicalForm.GetExactAlternative(writingSystemForMatching.Id);
				if (matches.Count > 1) //>1 becuase each will match itself
				{
					progress.WriteMessageWithColor("gray", "Found {0} homograph(s) for {1}", matches.Count, lexicalForm);
				}
				var mergeCount = 0;
				foreach (RecordToken<LexEntry> incomingMatch in matches)
				{
					if (alreadyProcessed.Contains(incomingMatch.Id))
						continue; //we'll be here at least as each element matches itself

					alreadyProcessed.Add(incomingMatch.Id);
					if (EntryMerger.TryMergeEntries(entry, incomingMatch.RealObject, progress))
					{
						mergeCount++;
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

			MergeSensesWithinEntries(repo, progress);
		}

		/// <summary>
		/// it can happen that within a single entry, you can have mergable senses.
		///
		/// NB!!!! this only thinks about merging the first 2 senses. (this was written as an emergency cleanup for a FLEx bug).
		/// </summary>
		private static void MergeSensesWithinEntries(LiftLexEntryRepository repo, IProgress progress)
		{
			var ids = new List<RepositoryId>(repo.GetAllItems());
			for (int i = 0; i < ids.Count; i++)
			{
				if (progress.CancelRequested)
				{
					throw new OperationCanceledException("User cancelled");
				}
				var entry = repo.GetItem(ids[i]);
				var senses = entry.Senses.ToArray();
				if(senses.Length < 2)
				{
					continue;
				}
				var targetSense = senses[0];
				var sense2 = senses[1];

				{
					if(SenseMerger.TryMergeSenseWithSomeExistingSense(targetSense, sense2, progress))
					{
						entry.Senses.Remove(sense2);
						entry.IsDirty = true;
						repo.SaveItem(entry);
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
			progress.WriteMessage("Looking at 1st 50 entries to determine which Writing System to use for matching...");
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
			progress.WriteMessage("Will use '{0}' for matching.", z.Value.Id);

			return z.Value.Id;
		}



		public override string ToString()
		{
			return "Merge Homographs";
		}
	}
}