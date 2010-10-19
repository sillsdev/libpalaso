using System.Collections.Generic;
using System.Linq;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;

namespace Palaso.DictionaryServices.Processors
{
	public class SenseMerger
	{
		public static bool TryMergeSenseWithSomeExistingSense(LexSense targetSense, LexSense incomingSense)
		{
			if (!targetSense.Gloss.Empty)
			{
				if (!incomingSense.Gloss.Empty && !targetSense.Gloss.CanBeUnifiedWith(incomingSense.Gloss))
					return false;
			}

			if (!targetSense.Definition.Empty)
			{
				if (!incomingSense.Definition.Empty && !targetSense.Definition.CanBeUnifiedWith(incomingSense.Definition))
					return false;
			}

			//can we unify the properites?
			if (!TryMergeProperties(targetSense, incomingSense))
			{
				return false;
			}

			//at this point, we're committed

			foreach (var lexExampleSentence in incomingSense.ExampleSentences)
			{
				targetSense.ExampleSentences.Add(lexExampleSentence);
			}
			targetSense.Gloss.MergeIn(incomingSense.Gloss);
			targetSense.Definition.MergeIn(incomingSense.Definition);
			return true;
		}

		public static bool TryMergeProperties(PalasoDataObject targetItem, PalasoDataObject incomingItem)
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

					if (targetProperty.Value != incomingProperty.Value)
					{
						return false; //clashing properties
					}
				}
			}

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
