using System;
using System.Collections.Generic;
using SIL.Data;
using SIL.DictionaryServices.Model;
using SIL.Lift;
using SIL.Progress;

namespace SIL.DictionaryServices.Processors
{
	/// <summary>
	/// This is used by the "LiftTools" application. It currently finds all the
	/// senses which look like they were merged into a single entry during CAWL
	/// gathering, and splits them into separate entries.
	/// </summary>
	public class EntrySplitter
	{

		public static void Run(LiftLexEntryRepository repo, IProgress progress)
		{
			var ids = new List<RepositoryId>(repo.GetAllItems());
			for (int i = 0; i < ids.Count; i++)
			{
				if (progress.CancelRequested)
				{
					throw new OperationCanceledException("User cancelled");
				}
				var entry = repo.GetItem(ids[i]);
				bool foundFirstCawl = false;
				List<LexSense> sensesToSplitOff = new List<LexSense>();
				foreach (var sense in entry.Senses)
				{
					var cawl = sense.GetProperty<MultiText>("SILCAWL");
					if (cawl == null)
						continue;
					if(foundFirstCawl)
					{
						sensesToSplitOff.Add(sense);
						continue;
					}
					foundFirstCawl = true;
				}

				foreach (var lexSense in sensesToSplitOff)
				{
					SpinSenseOffToItsOwnEntry(repo, lexSense,progress);
				}
			}
		}

		/// <summary>
		/// Note, this isn't very ambitious. The only thing the new entry will have is the lexeme form and the new sense, not any other traits/fields
		/// </summary>
		/// <param name="repo"> </param>
		/// <param name="sense"></param>
		private static void SpinSenseOffToItsOwnEntry(LiftLexEntryRepository repo, LexSense sense, IProgress progress)
		{
			var existingEntry = (LexEntry) sense.Parent;
			progress.WriteMessage("Splitting off {0} ({1}) into its own entry", existingEntry.LexicalForm.GetFirstAlternative(), sense.Definition.GetFirstAlternative());
			LexEntry newEntry = repo.CreateItem();
			newEntry.LexicalForm.MergeIn(existingEntry.LexicalForm);
			existingEntry.Senses.Remove(sense);
			newEntry.Senses.Add(sense);
			sense.Parent = newEntry;
			repo.SaveItem(existingEntry);
			repo.SaveItem(newEntry);
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



		public override string ToString()
		{
			return "Entry Splitter";
		}

	}
}