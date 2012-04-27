using System;
using System.Collections.Generic;
using System.Linq;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Progress.LogBox;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Processors
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



		public override string ToString()
		{
			return "Entry Splitter";
		}

	}
}