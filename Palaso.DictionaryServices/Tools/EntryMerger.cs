using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.DictionaryServices.Model;
using Palaso.Progress.LogBox;

namespace Palaso.DictionaryServices.Tools
{
	public class EntryMerger
	{
		public static bool TryMergeEntries(LexEntry entry1, LexEntry entry2, IProgress progress)
		{

			if (!SenseMerger.TryMergeProperties(entry1, entry2))
				return false;

			// at this point, we're committed to doing the merge

			foreach (var property in entry2.Properties)
			{
				//absorb it only if we don't have a matching one
				if (entry1.Properties.Any(k => k.Key == property.Key))
				{
					progress.WriteWarning("{0}: Clashing values of {1}, merging anyways", entry1.GetSimpleFormForLogging(), property.Key);
				}
				else
				{
					entry1.Properties.Add(property);
				}
			}


			var senses = entry2.Senses.ToArray();
			foreach (var sense in senses)
			{
				MergeOrAddSense(entry1, sense);
			}

			if (entry2.ModificationTime > entry1.ModificationTime)
			{
				entry1.ModificationTime = entry2.ModificationTime;
			}
			entry1.IsDirty = true;
			return true;
		}

		private static void MergeOrAddSense(LexEntry targetEntry, LexSense incomingSense)
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
					if (SenseMerger.TryMergeSenseWithSomeExistingSense(targetSense, incomingSense))
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
						target.Properties.Add(newGuy); ;
					}
				}
				*/
	}
}
