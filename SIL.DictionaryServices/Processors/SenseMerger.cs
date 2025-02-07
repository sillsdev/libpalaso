using System.Collections.Generic;
using System.Linq;
using SIL.DictionaryServices.Model;
using SIL.Lift;
using SIL.Lift.Options;
using SIL.Progress;

namespace SIL.DictionaryServices.Processors
{
	public class SenseMerger
	{
		public static bool TryMergeSenseWithSomeExistingSense(LexSense targetSense, LexSense incomingSense, string[] traitsWithMultiplicity, IProgress progress)
		{
			//can we unify the properties?
			if (!TryMergeProperties(targetSense, incomingSense, traitsWithMultiplicity, "senses of " + targetSense.Parent.ToString(), progress))
			{
				return false;
			}

			progress.WriteMessageWithColor("blue", "Merged two senses of {0} together: {1} into {2}", targetSense.Parent.ToString(), incomingSense.Id, targetSense.Id);

			//at this point, we're committed);

			foreach (var lexExampleSentence in incomingSense.ExampleSentences)
			{
				targetSense.ExampleSentences.Add(lexExampleSentence);
			}

			return true;
		}

		public static bool TryMergeProperties(PalasoDataObject targetItem, PalasoDataObject incomingItem, string[] traitsWithMultiplicity, string itemDescriptionForMessage, IProgress progress)
		{
			var knownMergableOptionCollectionTraits = new[] { LexSense.WellKnownProperties.SemanticDomainDdp4 };
			if (traitsWithMultiplicity == null)
			{
				traitsWithMultiplicity = new string[0];
			}

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

				   //NB: Some of the complexity here is that we can't really tell from here if what the multiplicity is of the <trait>, unless we
					//recognize the trait (e.d. semantic domains) or we see that one side has > 1 values already.

					 if (incomingProperty.Value is OptionRefCollection && targetProperty.Value is OptionRefCollection)
					{
						var targetCollection = ((OptionRefCollection)targetProperty.Value);
						var incomingCollection = ((OptionRefCollection)incomingProperty.Value);
						var clearlyHasMultiplicityGreaterThan1 = (incomingCollection.Count > 1 || targetCollection.Count > 1);
						var atLeastOneSideIsEmpty = (incomingCollection.Count == 0 || targetCollection.Count == 0);
						if (knownMergableOptionCollectionTraits.Contains(targetProperty.Key)
							|| traitsWithMultiplicity.Contains(targetProperty.Key)
							|| clearlyHasMultiplicityGreaterThan1
							|| atLeastOneSideIsEmpty)
						{
							var mergeSuccess = targetCollection.MergeByKey(
								(OptionRefCollection) incomingProperty.Value);
							if (mergeSuccess)
								continue;
							else
							{
								progress.WriteMessageWithColor("gray",
															   "Attempting to merge " + itemDescriptionForMessage +
															   ", but could not due to inability to merge contents of the property '{0}' (possibly due to incompatible embedded xml).",
															   targetProperty.Key, targetProperty.Value,
															   incomingProperty.Value);
								return false;
							}
						}
						else
							//at this point, we know that both sides have a count of 1, a common thing for a <trait> with multiplicity of 1
							if (((OptionRef) incomingCollection.Members[0]).Key == ((OptionRef) targetCollection.Members[0]).Key &&
								((OptionRef) incomingCollection.Members[0]).Value == ((OptionRef) targetCollection.Members[0]).Value)
							{
								continue; //same, single value
							}
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

			//I (JH) once saw this foreach break saying the collection was modified, so I'm makinkg this safeProperties thing
			var safeProperties = new List<KeyValuePair<string, IPalasoDataObjectProperty>>(incomingItem.Properties.ToArray());
			foreach (var pair in safeProperties)
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
					targetItem.MergeProperty(pair);
				}
			}
			return true;
		}
	}
}
