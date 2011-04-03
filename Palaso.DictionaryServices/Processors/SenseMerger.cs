using System.Collections.Generic;
using System.Linq;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.Progress.LogBox;

namespace Palaso.DictionaryServices.Processors
{
	public class SenseMerger
	{
		public static bool TryMergeSenseWithSomeExistingSense(LexSense targetSense, LexSense incomingSense, IProgress progress)
		{
			//can we unify the properites?
			if (!TryMergeProperties(targetSense, incomingSense, "senses", progress))
			{
				return false;
			}

			progress.WriteMessageWithColor("blue", "Merged two {0} senses of {1} together: {2} into {3}", targetSense.Parent.ToString(), targetSense.Gloss.GetFirstAlternative(), incomingSense.Id, targetSense.Id);

			//at this point, we're committed);

			foreach (var lexExampleSentence in incomingSense.ExampleSentences)
			{
				targetSense.ExampleSentences.Add(lexExampleSentence);
			}

			return true;
		}

		public static bool TryMergeProperties(PalasoDataObject targetItem, PalasoDataObject incomingItem, string itemDescriptionForMessage, IProgress progress)
		{
			foreach (var incomingProperty in incomingItem.Properties)
			{
				if (targetItem.Properties.Any(p => p.Key == incomingProperty.Key))
				{
					var targetProperty = targetItem.Properties.First(p => p.Key == incomingProperty.Key);

					if (incomingProperty.Value is MultiText && targetProperty.Value is MultiText &&
						((MultiText)incomingProperty.Value).CanBeUnifiedWith(((MultiText)targetProperty.Value)))
					{
						continue;
					}
					//NB: we are only attempting to merge the normal, single value OptionRefCollection, which is actuall
					//a <trait> in LIFT land (<field> too?  I dunno)
					if (incomingProperty.Value is OptionRefCollection && targetProperty.Value is OptionRefCollection &&

						((OptionRefCollection)incomingProperty.Value).Count == 1 &&
						((OptionRefCollection)targetProperty.Value).Count == 1 &&
						((OptionRef)((OptionRefCollection)incomingProperty.Value).Members[0]).Key == ((OptionRef)((OptionRefCollection)targetProperty.Value).Members[0]).Key &&
						((OptionRef)((OptionRefCollection)incomingProperty.Value).Members[0]).Value == ((OptionRef)((OptionRefCollection)targetProperty.Value).Members[0]).Value)
					{
						continue;
					}

					if (incomingProperty.Value is OptionRef && targetProperty.Value is OptionRef &&
						(((OptionRef)incomingProperty.Value).Value == ((OptionRef)targetProperty.Value).Value))
					{
						continue;
					}

					if (targetProperty.Value != incomingProperty.Value)
					{
						progress.WriteMessageWithColor("gray","Attempting to merge "+itemDescriptionForMessage+", but could not because of the property '{0}' ('{1}' vs. '{2}')", targetProperty.Key, targetProperty.Value, incomingProperty.Value);
						return false; //clashing properties
					}
				}
			}

			//at this point, we're committed

			foreach (var pair in incomingItem.Properties)
			{
				var match = targetItem.Properties.FirstOrDefault(p => p.Key == pair.Key);

				if (match.Key != default(KeyValuePair<string, object>).Key &&
					pair.Value is MultiText && match.Value is MultiText &&
					((MultiText)pair.Value).CanBeUnifiedWith(((MultiText)match.Value)))
				{
					((MultiText)match.Value).MergeIn(((MultiText)pair.Value));
				}
				else
				{
					targetItem.CopyProperty(pair);
				}
			}
			return true;
		}
	}
}
