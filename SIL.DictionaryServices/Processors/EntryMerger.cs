using System.Linq;
using SIL.DictionaryServices.Model;
using SIL.Progress;

namespace SIL.DictionaryServices.Processors
{
	public class EntryMerger
	{
		public static bool TryMergeEntries(LexEntry entry1, LexEntry entry2, string[] traitsWithMultiplicity, IProgress progress)
		{
			if (!entry1.LexicalForm.CanBeUnifiedWith(entry2.LexicalForm))
			{
				progress.WriteMessageWithColor("gray","Attempting to merge entries, but could not because their Lexical Forms clash in some writing system.");
				return false;
			}

			if (!SenseMerger.TryMergeProperties(entry1, entry2, traitsWithMultiplicity, "entries for "+entry1.ToString(), progress))
				return false;

			// at this point, we're committed to doing the merge

			entry1.LexicalForm.MergeIn(entry2.LexicalForm);

			var senses = entry2.Senses.ToArray();
			foreach (var sense in senses)
			{
				MergeOrAddSense(entry1, sense, traitsWithMultiplicity, progress);
			}

			if (entry2.ModificationTime > entry1.ModificationTime)
			{
				entry1.ModificationTime = entry2.ModificationTime;
			}

			entry1.IsDirty = true;
			return true;
		}


		private static void MergeOrAddSense(LexEntry targetEntry, LexSense incomingSense, string[] traitsWithMultiplicity, IProgress progress)
		{
			if (targetEntry.Senses.Count == 0)
			{
				targetEntry.Senses.Add(incomingSense);//no problem!
			}
			else
			{
				if (targetEntry.Senses.Count == 1)
				{
					var targetSense = targetEntry.Senses[0];
					if (SenseMerger.TryMergeSenseWithSomeExistingSense(targetSense, incomingSense, traitsWithMultiplicity, progress))
					{
						//it was merged in
						return;
					}
				}
			}
			//it needs to be added
			targetEntry.Senses.Add(incomingSense);
		}



		/*        private void MergeMultiText(PalasoDataObject first, PalasoDataObject second, string propertyName)
				{
					if (first.GetOrCreateProperty<MultiText>(propertyName).Empty && !second.GetOrCreateProperty<MultiText>(propertyName).Empty)
						ChangeMultiText(first, second, propertyName);
				}

				private void ChangeMultiText(PalasoDataObject target, PalasoDataObject source , string propertyName)
				{
					KeyValuePair<string, object> prop = target.Properties.Find(p => p.Key == propertyName);
					if (prop.Key != null)
					{
						target.Properties.Remove(prop);
						MultiText newText = source.GetOrCreateProperty<MultiText>(propertyName);
						var newGuy = new KeyValuePair<string, object>(propertyName, newText);
						//source.Properties.Remove(prop);//detach else it gets deleted
						target.Properties.Add(newGuy);
					}
				}
				*/
	}
}
