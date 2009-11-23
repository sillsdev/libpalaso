using System;
using System.Collections.Generic;
using System.Linq;
using LiftIO.Parsing;
using Palaso.Data;
using Palaso.Lift;
using Palaso.Lift.Model;
using Palaso.Lift.Options;
using Palaso.Text;

namespace WeSay.LexicalModel
{
	/// <summary>
	/// This class is called by the LiftParser, as it encounters each element of a lift file.
	/// There is at least one other ILexiconMerger, used in FLEX.
	///
	/// NB: In WeSay, we don't really use this to "merge in" elements, since we start from
	/// scratch each time we read a file. But in FLEx it is currently used that way, hence
	/// we haven't renamed the interface (ILexiconMerger).
	/// </summary>
	internal class LexEntryFromLiftBuilder:
			ILexiconMerger<PalasoDataObject, LexEntry, LexSense, LexExampleSentence>,
			IDisposable
	{
		public class EntryCreatedEventArgs: EventArgs
		{
			public readonly LexEntry Entry;

			public EntryCreatedEventArgs(LexEntry entry)
			{
				this.Entry = entry;
			}
		}

		public event EventHandler<EntryCreatedEventArgs> EntryCreatedEvent = delegate { };
		private readonly IList<string> _expectedOptionCollectionTraits;
		private readonly MemoryDataMapper<LexEntry> _dataMapper;
		private readonly OptionsList _semanticDomainsList;

		public LexEntryFromLiftBuilder(MemoryDataMapper<LexEntry> dataMapper, OptionsList semanticDomainsList)
		{
			ExpectedOptionTraits = new List<string>();
			_expectedOptionCollectionTraits = new List<string>();
			_dataMapper = dataMapper;
			_semanticDomainsList = semanticDomainsList;
		}

		public LexEntry GetOrMakeEntry(Extensible eInfo, int order)
		{
			LexEntry entry = null;

			if (eInfo.CreationTime == default(DateTime))
			{
				eInfo.CreationTime = PreciseDateTime.UtcNow;
			}

			if (eInfo.ModificationTime == default(DateTime))
			{
				eInfo.ModificationTime = PreciseDateTime.UtcNow;
			}

			entry = _dataMapper.CreateItem();
			entry.Id = eInfo.Id;
			entry.Guid = eInfo.Guid;
			entry.CreationTime = eInfo.CreationTime;
			entry.ModificationTime = eInfo.ModificationTime;
			if (_dataMapper.LastModified < entry.ModificationTime)
			{
				_dataMapper.LastModified = entry.ModificationTime;
			}

			entry.ModifiedTimeIsLocked = true; //while we build it up
			entry.OrderForRoundTripping = order;
			return entry;
		}

		#region ILexiconMerger<PalasoDataObject,LexEntry,LexSense,LexExampleSentence> Members

		public void EntryWasDeleted(Extensible info, DateTime dateDeleted)
		{
			//there isn't anything we need to do; we just don't import it
			// since we always update file in place, the info will still stay in the lift file
			// even though we don't import it.
		}

		#endregion

#if merging
		private static bool CanSafelyPruneMerge(Extensible eInfo, LexEntry entry)
		{
			return entry != null
				&& entry.ModificationTime == eInfo.ModificationTime
				&& entry.ModificationTime.Kind != DateTimeKind.Unspecified
				 && eInfo.ModificationTime.Kind != DateTimeKind.Unspecified;
		}
#endif

		public LexSense GetOrMakeSense(LexEntry entry, Extensible eInfo, string rawXml)
		{
			//nb, has no guid or dates
			LexSense s = new LexSense(entry);
			s.Id = eInfo.Id;
			entry.Senses.Add(s);

			return s;
		}

		public LexSense GetOrMakeSubsense(LexSense sense, Extensible info, string rawXml)
		{
			sense.GetOrCreateProperty<EmbeddedXmlCollection>("subSense").Values.Add(rawXml);

			return null;
		}

		public LexExampleSentence GetOrMakeExample(LexSense sense, Extensible eInfo)
		{
			LexExampleSentence ex = new LexExampleSentence(sense);
			sense.ExampleSentences.Add(ex);
			return ex;
		}

		public void MergeInLexemeForm(LexEntry entry, LiftMultiText forms)
		{
			MergeIn(entry.LexicalForm, forms);
		}

		public void MergeInCitationForm(LexEntry entry, LiftMultiText contents)
		{
			AddOrAppendMultiTextProperty(entry,
										 contents,
										 LexEntry.WellKnownProperties.Citation,
										 null);
		}

		public PalasoDataObject MergeInPronunciation(LexEntry entry,
													LiftMultiText contents,
													string rawXml)
		{
			entry.GetOrCreateProperty<EmbeddedXmlCollection>("pronunciation").Values.Add(rawXml);
			return null;
		}

		public PalasoDataObject MergeInVariant(LexEntry entry, LiftMultiText contents, string rawXml)
		{
			entry.GetOrCreateProperty<EmbeddedXmlCollection>("variant").Values.Add(rawXml);
			return null;
		}

		public void MergeInGloss(LexSense sense, LiftMultiText forms)
		{
			sense.Gloss.MergeInWithAppend(MultiText.Create(forms.AsSimpleStrings), "; ");
			AddAnnotationsToMultiText(forms, sense.Gloss);
		}

		private static void AddAnnotationsToMultiText(LiftMultiText forms, MultiTextBase text)
		{
			foreach (Annotation annotation in forms.Annotations)
			{
				if (annotation.Name == "flag")
				{
					text.SetAnnotationOfAlternativeIsStarred(annotation.LanguageHint,
															 int.Parse(annotation.Value) > 0);
				}
				else
				{
					//log dropped
				}
			}
		}

		public void MergeInExampleForm(LexExampleSentence example, LiftMultiText forms)
				//, string optionalSource)
		{
			MergeIn(example.Sentence, forms);
		}

		public void MergeInTranslationForm(LexExampleSentence example,
										   string type,
										   LiftMultiText forms,
										   string rawXml)
		{
			bool alreadyHaveAPrimaryTranslation = example.Translation != null &&
												  !string.IsNullOrEmpty(
														   example.Translation.GetFirstAlternative());
		/*    bool typeIsCompatibleWithWeSayPrimaryTranslation = string.IsNullOrEmpty(type) ||
															   type.ToLower() == "free translation"; //this is the default style in FLEx
		 * */

			//WeSay's model only allows for one translation just grab the first translation
			if (!alreadyHaveAPrimaryTranslation /*&& typeIsCompatibleWithWeSayPrimaryTranslation*/)
			{
				MergeIn(example.Translation, forms);
				example.TranslationType = type;
			}
			else
			{
				example.GetOrCreateProperty<EmbeddedXmlCollection>(PalasoDataObject.GetEmbeddedXmlNameForProperty(LexExampleSentence.WellKnownProperties.Translation)).Values.Add(rawXml);
			}
		}

		public void MergeInSource(LexExampleSentence example, string source)
		{
			OptionRef o =
					example.GetOrCreateProperty<OptionRef>(
							LexExampleSentence.WellKnownProperties.Source);
			o.Value = source;
		}

		public void MergeInDefinition(LexSense sense, LiftMultiText contents)
		{
			AddOrAppendMultiTextProperty(sense,
										 contents,
										 LexSense.WellKnownProperties.Definition,
										 null);
		}

		public void MergeInPicture(LexSense sense, string href, LiftMultiText caption)
		{
			//nb 1:  we're limiting ourselves to one picture per sense, here:
			//nb 2: the name and case must match the fieldName
			PictureRef pict = sense.GetOrCreateProperty<PictureRef>("Picture");
			pict.Value = href;
			if (caption != null)
			{
				pict.Caption = MultiText.Create(caption.AsSimpleStrings);
			}
		}

		/// <summary>
		/// Handle LIFT's "note" entity
		/// </summary>
		/// <remarks>The difficult thing here is we don't handle anything but a default note.
		/// Any other kind, we put in the xml residue for round-tripping.</remarks>
		public void MergeInNote(PalasoDataObject extensible, string type, LiftMultiText contents, string rawXml)
		{
			var noteProperty = extensible.GetProperty<MultiText>(PalasoDataObject.WellKnownProperties.Note);
			bool alreadyHaveAOne= !MultiText.IsEmpty(noteProperty);

			bool weCanHandleThisType = string.IsNullOrEmpty(type) ||type == "general";

			if (!alreadyHaveAOne && weCanHandleThisType)
			{
				List<String> writingSystemAlternatives = new List<string>(contents.Count);
				foreach (KeyValuePair<string, string> pair in contents.AsSimpleStrings)
				{
					writingSystemAlternatives.Add(pair.Key);
				}
				noteProperty = extensible.GetOrCreateProperty<MultiText>(PalasoDataObject.WellKnownProperties.Note);
				MergeIn(noteProperty, contents);
			}
			else //residue
			{
				var residue = extensible.GetOrCreateProperty<EmbeddedXmlCollection>(PalasoDataObject.GetEmbeddedXmlNameForProperty(PalasoDataObject.WellKnownProperties.Note));
				residue.Values.Add(rawXml);
//                var r = extensible.GetProperty<EmbeddedXmlCollection>(PalasoDataObject.GetEmbeddedXmlNameForProperty(PalasoDataObject.WellKnownProperties.Note));
//                Debug.Assert(r != null);
			}
		}

		public PalasoDataObject GetOrMakeParentReversal(PalasoDataObject parent,
													   LiftMultiText contents,
													   string type)
		{
			return null; // we'll get what we need from the rawxml of MergeInReversal
		}

		public PalasoDataObject MergeInReversal(LexSense sense,
											   PalasoDataObject parent,
											   LiftMultiText contents,
											   string type,
											   string rawXml)
		{
			sense.GetOrCreateProperty<EmbeddedXmlCollection>("reversal").Values.Add(rawXml);
			return null;
		}

		public PalasoDataObject MergeInEtymology(LexEntry entry,
												string source,
												string type,
												LiftMultiText form,
												LiftMultiText gloss,
												string rawXml)
		{
			entry.GetOrCreateProperty<EmbeddedXmlCollection>("etymology").Values.Add(rawXml);

			return null;
		}

		public void ProcessRangeElement(string range,
										string id,
										string guid,
										string parent,
										LiftMultiText description,
										LiftMultiText label,
										LiftMultiText abbrev) {}

		public void ProcessFieldDefinition(string tag, LiftMultiText description) {}

		public void MergeInGrammaticalInfo(PalasoDataObject senseOrReversal,
										   string val,
										   List<Trait> traits)
		{
			LexSense sense = senseOrReversal as LexSense;
			if (sense == null)
			{
				return; //todo: preserve grammatical info on reversal, when we hand reversals
			}

			OptionRef o =
					sense.GetOrCreateProperty<OptionRef>(LexSense.WellKnownProperties.PartOfSpeech);
			o.Value = val;
			if (traits != null)
			{
				foreach (Trait trait in traits)
				{
					if (trait.Name == "flag" && int.Parse(trait.Value) > 0)
					{
						o.IsStarred = true;
					}
					else
					{
						//log skipping
					}
				}
			}
		}

		private static void AddOrAppendMultiTextProperty(PalasoDataObject dataObject,
														 LiftMultiText contents,
														 string propertyName,
														 string noticeToPrependIfNotEmpty)
		{
			MultiText mt = dataObject.GetOrCreateProperty<MultiText>(propertyName);
			mt.MergeInWithAppend(MultiText.Create(contents),
								 string.IsNullOrEmpty(noticeToPrependIfNotEmpty)
										 ? "; "
										 : noticeToPrependIfNotEmpty);
			AddAnnotationsToMultiText(contents, mt);

			//dataObject.GetOrCreateProperty<string>(propertyName) mt));
		}

		/*
		private static void AddMultiTextProperty(PalasoDataObject dataObject, LiftMultiText contents, string propertyName)
		{
			dataObject.Properties.Add(
				new KeyValuePair<string, object>(propertyName,
												 MultiText.Create(contents)));
		}
*/

		/// <summary>
		/// Handle LIFT's "field" entity which can be found on any subclass of "extensible"
		/// </summary>
		public void MergeInField(PalasoDataObject extensible,
								 string typeAttribute,
								 DateTime dateCreated,
								 DateTime dateModified,
								 LiftMultiText contents,
								 List<Trait> traits)
		{
			MultiText t = MultiText.Create(contents.AsSimpleStrings);

			//enchance: instead of KeyValuePair, make a LiftField class, so we can either keep the
			// other field stuff as xml (in order to round-trip it) or model it.

			extensible.Properties.Add(new KeyValuePair<string, object>(typeAttribute, t));

			if (traits != null)
			{
				foreach (var trait in traits)
				{
					t.EmbeddedXmlElements.Add(string.Format(@"<trait name='{0}' value='{1}'/>", trait.Name, trait.Value));
				}
			}
		}

		/// <summary>
		/// Handle LIFT's "trait" entity,
		/// which can be found on any subclass of "extensible", on any "field", and as
		/// a subclass of "annotation".
		/// </summary>
		public void MergeInTrait(PalasoDataObject extensible, Trait trait)
		{
			if (String.IsNullOrEmpty(trait.Name))
			{
				//"log skipping..."
				return;
			}
			if (ExpectedOptionTraits.Contains(trait.Name))
			{
				OptionRef o = extensible.GetOrCreateProperty<OptionRef>(trait.Name);
				o.Value = trait.Value.Trim();
			}
			else if (trait.Name.StartsWith("flag-"))
			{
				extensible.SetFlag(trait.Name);
			}
					// if it is unknown assume it is a collection.
			else //if (ExpectedOptionCollectionTraits.Contains(trait.Name))
			{
				var key = trait.Value.Trim();
				OptionRefCollection refs =
						extensible.GetOrCreateProperty<OptionRefCollection>(trait.Name);
				if(trait.Name == LexSense.WellKnownProperties.SemanticDomainDdp4)
				{
					if(_semanticDomainsList.GetOptionFromKey(key) == null)
					{
						var match =_semanticDomainsList.Options.FirstOrDefault(option => option.Key.StartsWith(key));
						if(match !=null)
						{
							refs.Add(match.Key);
							return;
						}
					}
				}
				refs.Add(key);
			}
			//else
			//{
			//    //"log skipping..."
			//}
		}

		public void MergeInRelation(PalasoDataObject extensible,
									string relationFieldId,
									string targetId,
									string rawXml)
		{
			if (String.IsNullOrEmpty(relationFieldId))
			{
				return; //"log skipping..."
			}

			//the "field name" of a relation equals the name of the relation type
			LexRelationCollection collection =
					extensible.GetOrCreateProperty<LexRelationCollection>(relationFieldId);
			LexRelation relation = new LexRelation(relationFieldId, targetId, extensible);
			collection.Relations.Add(relation);
		}

		public IEnumerable<string> ExpectedOptionTraits
		{
			get; set;
		}

		public IList<string> ExpectedOptionCollectionTraits
		{
			get { return _expectedOptionCollectionTraits; }
		}

		//        public ProgressState ProgressState
		//        {
		//            set
		//            {
		//                _progressState = value;
		//            }
		//        }

		private static void MergeIn(MultiTextBase multiText, LiftMultiText forms)
		{
			multiText.MergeIn(MultiText.Create(forms.AsSimpleStrings));
			AddAnnotationsToMultiText(forms, multiText);
		}

		public void Dispose()
		{
			//_entries.Dispose();
		}

		#region ILexiconMerger<PalasoDataObject,LexEntry,LexSense,LexExampleSentence> Members

		public void FinishEntry(LexEntry entry)
		{
			PostProcessSenses(entry);
			//_dataMapper.FinishCreateEntry(entry);
			entry.GetOrCreateId(false);
			entry.ModifiedTimeIsLocked = false;
			entry.Clean();
		}

		/// <summary>
		/// We do this because in linguist tools, there is a distinction that we don't want to
		/// normally make in WeSay.  There, "gloss" is the first pass at a definition, but its
		/// really just for interlinearlization.  That isn't a distinction we want our user
		/// to bother with.
		/// </summary>
		/// <param name="entry"></param>
		private void PostProcessSenses(LexEntry entry)
		{
			foreach (LexSense sense in entry.Senses)
			{
				CopyOverGlossesIfDefinitionsMissing(sense);
				FixUpOldLiteralMeaningMistake(entry, sense);
			}
		}

		/// <summary>
		/// we initially, mistakenly put literal meaning on sense. This moves
		/// it and switches to newer naming style.
		/// </summary>
		internal void FixUpOldLiteralMeaningMistake(LexEntry entry, LexSense sense)
		{
			KeyValuePair<string, object> prop = sense.Properties.Find(p => p.Key == "LiteralMeaning");
			if (prop.Key != null)
			{
				sense.Properties.Remove(prop);
				//change the label and move it up to lex entry
				var newGuy = new KeyValuePair<string, object>("literal-meaning",  prop.Value);
				entry.Properties.Add(newGuy);
			}
		}

		private void CopyOverGlossesIfDefinitionsMissing(LexSense sense)
		{
			foreach(LanguageForm form in sense.Gloss.Forms)
			{
				if(!sense.Definition.ContainsAlternative(form.WritingSystemId))
				{
					sense.Definition.SetAlternative(form.WritingSystemId,form.Form);
				}
			}
		}

		public void MergeInMedia(PalasoDataObject pronunciation, string href, LiftMultiText caption)
		{
			// review: Currently ignore media. See WS-1128
		}

		#endregion
	}
}