using System;
using System.Collections.Generic;
using NUnit.Framework;
using SIL.Data;
using SIL.DictionaryServices.Lift;
using SIL.DictionaryServices.Model;
using SIL.Lift;
using SIL.Lift.Options;
using SIL.Lift.Parsing;

namespace SIL.DictionaryServices.Tests.Lift
{
	[TestFixture]
	public class LexEntryFromLiftBuilderTests: ILiftMergerTestSuite
	{
		private LexEntryFromLiftBuilder _builder;
		private MemoryDataMapper<LexEntry> _dataMapper;
//        private string _tempFile;

		[SetUp]
		public void Setup()
		{
			_dataMapper = new MemoryDataMapper<LexEntry>();
			OptionsList pretendSemanticDomainList = new OptionsList();
			pretendSemanticDomainList.Options.Add(new Option("4.2.7 Play, fun", new MultiText()));
			_builder = new LexEntryFromLiftBuilder(_dataMapper, pretendSemanticDomainList);
		}

		[TearDown]
		public void TearDown()
		{
			_builder.Dispose();
			_dataMapper.Dispose();
		}


		/// <summary>
		/// Test the form we get from FLEx 5.4 (it was changed in FLEx 6.0)
		/// </summary>
		[Test]
		public void NewEntry_HasSemanticDomainWithTextualLabel_CorrectlyAddsSemanticDomain()
		{
			Extensible extensibleInfo = new Extensible();
			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			LexSense s = _builder.GetOrMakeSense(e, new Extensible(), string.Empty);

			var t = new Trait("semantic-domain-ddp4", //the name has migrated up to this already
								"4.2.7");
			_builder.MergeInTrait(s, t);
			_builder.FinishEntry(e);
			var property = e.Senses[0].GetProperty<OptionRefCollection>(LexSense.WellKnownProperties.SemanticDomainDdp4);
			Assert.AreEqual("4.2.7 Play, fun", property.KeyAtIndex(0));
		}

		/// <summary>
		/// Flex allows you to add domains. So make sure that doesn't break us.
		/// </summary>
		[Test]
		public void NewEntry_HasSemanticDomainWeDontKnowAbout_AddedAnyways()
		{
			Extensible extensibleInfo = new Extensible();
			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			LexSense s = _builder.GetOrMakeSense(e, new Extensible(), string.Empty);

			var t = new Trait("semantic-domain-ddp4",
								"9.9.9.9.9.9 Computer Gadgets" );
			_builder.MergeInTrait(s, t);
			_builder.FinishEntry(e);
			var property = e.Senses[0].GetProperty<OptionRefCollection>(LexSense.WellKnownProperties.SemanticDomainDdp4);
			Assert.AreEqual("9.9.9.9.9.9 Computer Gadgets", property.KeyAtIndex(0));
		}

		[Test]
		public void NewEntryGetsId()
		{
			Extensible extensibleInfo = new Extensible();
			extensibleInfo.Id = "foo";
			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			Assert.AreEqual(extensibleInfo.Id, e.Id);
			_builder.FinishEntry(e);
			Assert.AreEqual(1, _dataMapper.CountAllItems());
		}

		[Test]
		public void NewEntryGetsGuid()
		{
			Extensible extensibleInfo = new Extensible();
			extensibleInfo.Guid = Guid.NewGuid();
			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			Assert.AreEqual(extensibleInfo.Guid, e.Guid);
			_builder.FinishEntry(e);
			Assert.AreEqual(1, _dataMapper.CountAllItems());
		}

		[Test]
		public void NewEntryGetsDates()
		{
			Extensible extensibleInfo = new Extensible();
			extensibleInfo.CreationTime = DateTime.Parse("2/2/1969  12:15:12").ToUniversalTime();
			extensibleInfo.ModificationTime =
					DateTime.Parse("10/11/1968  12:15:12").ToUniversalTime();
			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			Assert.AreEqual(extensibleInfo.CreationTime, e.CreationTime);
			Assert.AreEqual(extensibleInfo.ModificationTime, e.ModificationTime);
			_builder.FinishEntry(e);
			Assert.AreEqual(1, _dataMapper.CountAllItems());
		}

		[Test, Ignore("TODO: move to wesay")]
		public void NewEntry_NoDefYesGloss_GlossCopiedToDefinition()
		{
		   // _builder.AfterEntryRead += _builder.ApplyWeSayPolicyToParsedEntry;

			Extensible extensibleInfo = new Extensible();
			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			LexSense s = _builder.GetOrMakeSense(e, new Extensible(), string.Empty);
			_builder.MergeInGloss(s, new LiftMultiText("x","x meaning"));
			_builder.FinishEntry(e);
			Assert.AreEqual("x meaning",e.Senses[0].Gloss.GetExactAlternative("x"));
			Assert.AreEqual("x meaning",e.Senses[0].Definition.GetExactAlternative("x"));
		}

		[Test, Ignore("TODO: move to wesay")]
		public void NewEntry_OldLiteralMeaning_GetsMoved()
		{
		   // _builder.AfterEntryRead += _builder.ApplyWeSayPolicyToParsedEntry;

			Extensible extensibleInfo = new Extensible();
			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			LexSense s = _builder.GetOrMakeSense(e, new Extensible(), string.Empty);
			LiftMultiText t = new LiftMultiText("en", "test");
			_builder.MergeInField(s, "LiteralMeaning", default(DateTime), default(DateTime), t, null);
			_builder.FinishEntry(e);
			Assert.IsNull(e.Senses[0].GetProperty<MultiText>("LiteralMeaning"));
			Assert.IsNotNull(e.GetProperty<MultiText>(LexEntry.WellKnownProperties.LiteralMeaning));
			Assert.AreEqual("test", e.GetProperty<MultiText>(LexEntry.WellKnownProperties.LiteralMeaning).GetExactAlternative("en"));
		}

		[Test, Ignore("TODO: move to wesay")]
		public void NewEntry_HasDefGlossHasAnotherWSAlternative_CopiedToDefinition()
		{
		  //  _builder.AfterEntryRead += _builder.ApplyWeSayPolicyToParsedEntry;

			Extensible extensibleInfo = new Extensible();
			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			LexSense s = _builder.GetOrMakeSense(e, new Extensible(), string.Empty);
			_builder.MergeInDefinition(s, new LiftMultiText("x", "x def"));
			_builder.MergeInGloss(s, new LiftMultiText("x", "x meaning"));
			_builder.MergeInGloss(s, new LiftMultiText("y", "y meaning"));
			_builder.FinishEntry(e);
			Assert.AreEqual("x meaning", e.Senses[0].Gloss.GetExactAlternative("x"));
			Assert.AreEqual("x def", e.Senses[0].Definition.GetExactAlternative("x"));
			Assert.AreEqual("y meaning", e.Senses[0].Definition.GetExactAlternative("y"));
		}

		[Test]
		public void NewEntry_YesDefYesGloss_BothUnchanged()
		{
			Extensible extensibleInfo = new Extensible();
			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			LexSense s = _builder.GetOrMakeSense(e, new Extensible(), string.Empty);
			_builder.MergeInGloss(s, new LiftMultiText("x", "x gloss"));
			_builder.MergeInDefinition(s, new LiftMultiText("x", "x def"));
			_builder.FinishEntry(e);
			Assert.AreEqual("x gloss", e.Senses[0].Gloss.GetExactAlternative("x"));
			Assert.AreEqual("x def", e.Senses[0].Definition.GetExactAlternative("x"));
		}
		[Test]
		public void NewEntry_NoGlossNoDef_GetNeitherInTheSense()
		{
			Extensible extensibleInfo = new Extensible();
			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			LexSense s = _builder.GetOrMakeSense(e, new Extensible(), string.Empty);
			_builder.FinishEntry(e);
			Assert.That(e.Senses[0], Is.EqualTo(s));
			Assert.That(s.Gloss, Is.Empty);
			Assert.That(s.Definition, Is.Empty);
		}

		[Test]
		public void NewEntryWithTextIdIgnoresIt()
		{
			Extensible extensibleInfo = new Extensible();
			extensibleInfo.Id = "hello";
			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			//no attempt is made to use that id
			Assert.IsNotNull(e.Guid);
			Assert.AreNotSame(Guid.Empty, e.Guid);
		}

		[Test]
		public void NewEntryTakesGivenDates()
		{
			Extensible extensibleInfo = new Extensible();
			extensibleInfo = AddDates(extensibleInfo);

			LexEntry e = _builder.GetOrMakeEntry(extensibleInfo, 0);
			Assert.AreEqual(extensibleInfo.CreationTime, e.CreationTime);
			Assert.AreEqual(extensibleInfo.ModificationTime, e.ModificationTime);
		}

		[Test]
		public void NewEntryNoDatesUsesNow()
		{
			LexEntry e = MakeSimpleEntry();
			Assert.IsTrue(TimeSpan.FromTicks(DateTime.UtcNow.Ticks - e.CreationTime.Ticks).Seconds <
						  2);
			Assert.IsTrue(
					TimeSpan.FromTicks(DateTime.UtcNow.Ticks - e.ModificationTime.Ticks).Seconds < 2);
		}

		private LexEntry MakeSimpleEntry()
		{
			Extensible extensibleInfo = new Extensible();
			return _builder.GetOrMakeEntry(extensibleInfo, 0);
		}

		[Test]
		public void EntryGetsEmptyLexemeForm()
		{
			LexEntry e = MakeSimpleEntry();
			_builder.MergeInLexemeForm(e, new LiftMultiText());
			Assert.AreEqual(0, e.LexicalForm.Count);
		}



		[Test]
		public void MergeInNote_NoteHasNoType_Added()
		{
			LexEntry e = MakeSimpleEntry();
			_builder.MergeInNote(e, string.Empty, MakeBasicLiftMultiText(), string.Empty);
			MultiText mt = e.GetProperty<MultiText>(PalasoDataObject.WellKnownProperties.Note);
			Assert.AreEqual("uno", mt["ws-one"]);
			Assert.AreEqual("dos", mt["ws-two"]);
		}
		[Test]
		public void MergeInNote_NoteHasTypeOfGeneral_Added()
		{
			LexEntry e = MakeSimpleEntry();
			_builder.MergeInNote(e, "general", MakeBasicLiftMultiText(), string.Empty);
			MultiText mt = e.GetProperty<MultiText>(PalasoDataObject.WellKnownProperties.Note);
			Assert.AreEqual("uno", mt["ws-one"]);
			Assert.AreEqual("dos", mt["ws-two"]);
		}

		[Test]
		public void MergeInNote_NoteHasTypeOtherThanGeneral_AllGoesToRoundTripResidue()
		{
			LexEntry e = MakeSimpleEntry();
			_builder.MergeInNote(e, "red", MakeBasicLiftMultiText(), "<pretendXmlOfNote/>");
			MultiText mt = e.GetProperty<MultiText>(PalasoDataObject.WellKnownProperties.Note);
			Assert.IsNull(mt);
			var residue =e.GetProperty<EmbeddedXmlCollection>(PalasoDataObject.GetEmbeddedXmlNameForProperty(PalasoDataObject.WellKnownProperties.Note));
			Assert.AreEqual(1,residue.Values.Count);
			Assert.AreEqual("<pretendXmlOfNote/>", residue.Values[0]);
		}

		[Test]
		public void MergeInNote_NoType_AfterFirstTheyGoesToRoundTripResidue()
		{
			LexEntry e = MakeSimpleEntry();
			_builder.MergeInNote(e, string.Empty, MakeBasicLiftMultiText("first"), "pretend xml one");
			_builder.MergeInNote(e, string.Empty, MakeBasicLiftMultiText("second"), "<pretend xml two/>");
			_builder.MergeInNote(e, string.Empty, MakeBasicLiftMultiText("third"), "<pretend xml three/>");

			MultiText mt = e.GetProperty<MultiText>(PalasoDataObject.WellKnownProperties.Note);
			Assert.AreEqual("first", mt["ws-one"]);

			var residue = e.GetProperty<EmbeddedXmlCollection>(PalasoDataObject.GetEmbeddedXmlNameForProperty(PalasoDataObject.WellKnownProperties.Note));
			Assert.AreEqual(2, residue.Values.Count);
			Assert.AreEqual("<pretend xml two/>", residue.Values[0]);
			Assert.AreEqual("<pretend xml three/>", residue.Values[1]);

		}


		[Test]
		public void SenseGetsGrammi()
		{
			LexSense sense = new LexSense();
			_builder.MergeInGrammaticalInfo(sense, "red", null);
			OptionRef optionRef =
					sense.GetProperty<OptionRef>(LexSense.WellKnownProperties.PartOfSpeech);
			Assert.IsNotNull(optionRef);
			Assert.AreEqual("red", optionRef.Value);
		}

		[Test]
		public void GrammiGetsFlagTrait()
		{
			LexSense sense = new LexSense();
			_builder.MergeInGrammaticalInfo(sense,
										   "red",
										   new List<Trait>(new Trait[] {new Trait("flag", "1")}));
			OptionRef optionRef =
					sense.GetProperty<OptionRef>(LexSense.WellKnownProperties.PartOfSpeech);
			Assert.IsTrue(optionRef.IsStarred);
		}

		[Test]
		public void SenseGetsExample()
		{
			LexSense sense = new LexSense();
			Extensible x = new Extensible();
			LexExampleSentence ex = _builder.GetOrMakeExample(sense, x);
			Assert.IsNotNull(ex);
			_builder.MergeInExampleForm(ex, MakeBasicLiftMultiText());
			Assert.AreEqual(2, ex.Sentence.Forms.Length);
			Assert.AreEqual("dos", ex.Sentence["ws-two"]);
		}

		[Test]
		public void SenseGetsRelation()
		{
			LexSense sense = new LexSense();
			_builder.MergeInRelation(sense, "synonym", "foo", null);
			LexRelationCollection synonyms = sense.GetProperty<LexRelationCollection>("synonym");
			LexRelation relation = synonyms.Relations[0];
			Assert.AreEqual("synonym", relation.FieldId);
			Assert.AreEqual("foo", relation.Key);
		}


		[Test]
		public void MergeInRelation_RelationHasEmbeddedTraits_RelationGetsEmbeddedXmlForLaterRoundTrip()
		{
			LexEntry e = MakeSimpleEntry();
			var xml = @"<relation type='component-lexeme' ref='bodzi_d333f64f-d388-431f-bb2b-7dd9b7f3fe3c'>
							<trait name='complex-form-type' value='Composto'></trait>
							<trait name='is-primary' value='true'/>
						</relation>";
			_builder.MergeInRelation(e, "component-lexeme", "someId", xml);

			LexRelationCollection collection =
				   e.GetOrCreateProperty<LexRelationCollection>("component-lexeme");
			Assert.AreEqual(2, collection.Relations[0].EmbeddedXmlElements.Count);
		}

		[Test]
		public void ExampleSourcePreserved()
		{
			LexExampleSentence ex = new LexExampleSentence();
			_builder.MergeInSource(ex, "fred");

			Assert.AreEqual("fred",
							ex.GetProperty<OptionRef>(LexExampleSentence.WellKnownProperties.Source)
									.Value);
		}

		[Test]
		public void SenseGetsDef()
		{
			LexSense sense = new LexSense();
			_builder.MergeInDefinition(sense, MakeBasicLiftMultiText());
			AssertPropertyHasExpectedMultiText(sense, LexSense.WellKnownProperties.Definition);
		}

		[Test]
		public void SenseGetsNote()
		{
			LexSense sense = new LexSense();
			_builder.MergeInNote(sense, null, MakeBasicLiftMultiText(), string.Empty);
			AssertPropertyHasExpectedMultiText(sense, PalasoDataObject.WellKnownProperties.Note);
		}

		[Test]
		public void SenseGetsId()
		{
			Extensible extensibleInfo = new Extensible();
			extensibleInfo.Id = "foo";
			LexSense s = _builder.GetOrMakeSense(new LexEntry(), extensibleInfo, string.Empty);
			Assert.AreEqual(extensibleInfo.Id, s.Id);
		}



		//        [Test]
		//        public void MergingIntoEmptyMultiTextWithFlags()
		//        {
		//            LiftMultiText lm = new LiftMultiText();
		//            lm.Add("one", "uno");
		//            lm.Add("two", "dos");
		//            lm.Traits.Add(new Trait("one", "flag", "1"));
		//
		//            MultiText m = new MultiText();
		//            MultiText.Create(lm as System.Collections.Generic.Dictionary<string,string>, List<)
		//            LexSense sense = new LexSense();
		//            LiftMultiText text = MakeBasicLiftMultiText();
		//            text.Traits.Add(new Trait("ws-one", "flag", "1"));
		//            _builder.MergeInGloss(sense, text);
		//
		//            Assert.IsTrue(sense.Gloss.GetAnnotationOfAlternativeIsStarred("ws-one"));
		//        }

		[Test]
		public void GlossGetsFlag()
		{
			LexSense sense = new LexSense();
			LiftMultiText text = MakeBasicLiftMultiText();
			AddAnnotationToLiftMultiText(text, "ws-one", "flag", "1");
			_builder.MergeInGloss(sense, text);
			Assert.IsTrue(sense.Gloss.GetAnnotationOfAlternativeIsStarred("ws-one"));
			Assert.IsFalse(sense.Gloss.GetAnnotationOfAlternativeIsStarred("ws-two"));

			text = MakeBasicLiftMultiText();
			AddAnnotationToLiftMultiText(text, "ws-one", "flag", "0");
			_builder.MergeInGloss(sense, text);
			Assert.IsFalse(sense.Gloss.GetAnnotationOfAlternativeIsStarred("ws-one"));
		}

		[Test]
		public void LexicalUnitGetsFlag()
		{
			LexEntry entry = MakeSimpleEntry();
			LiftMultiText text = MakeBasicLiftMultiText();
			AddAnnotationToLiftMultiText(text, "ws-one", "flag", "1");
			_builder.MergeInLexemeForm(entry, text);
			Assert.IsTrue(entry.LexicalForm.GetAnnotationOfAlternativeIsStarred("ws-one"));
			Assert.IsFalse(entry.LexicalForm.GetAnnotationOfAlternativeIsStarred("ws-two"));
		}

		private static void AddAnnotationToLiftMultiText(LiftMultiText text,
														 string languageHint,
														 string name,
														 string value)
		{
			Annotation annotation = new Annotation(name, value, default(DateTime), null);
			annotation.LanguageHint = languageHint;
			text.Annotations.Add(annotation);
		}

		[Test]
		public void MultipleGlossesCombined()
		{
			LexSense sense = new LexSense();
			_builder.MergeInGloss(sense, MakeBasicLiftMultiText());
			LiftMultiText secondGloss = new LiftMultiText();
			secondGloss.Add("ws-one", "UNO");
			secondGloss.Add("ws-three", "tres");
			_builder.MergeInGloss(sense, secondGloss);

			//MultiText mt = sense.GetProperty<MultiText>(LexSense.WellKnownProperties.Note);
			Assert.AreEqual(3, sense.Gloss.Forms.Length);
			Assert.AreEqual("uno; UNO", sense.Gloss["ws-one"]);
		}

		private static void AssertPropertyHasExpectedMultiText(PalasoDataObject dataObject,
															   string name)
		{
			//must match what is created by MakeBasicLiftMultiText()
			MultiText mt = dataObject.GetProperty<MultiText>(name);
			Assert.AreEqual(2, mt.Forms.Length);
			Assert.AreEqual("dos", mt["ws-two"]);
		}

		private static LiftMultiText MakeBasicLiftMultiText(string text)
		{
			LiftMultiText forms = new LiftMultiText();
			forms.Add("ws-one", text);
			forms.Add("ws-two", text+"-in-two");
			return forms;
		}

		private static LiftMultiText MakeBasicLiftMultiText()
		{
			LiftMultiText forms = new LiftMultiText();
			forms.Add("ws-one", "uno");
			forms.Add("ws-two", "dos");
			return forms;
		}

		#region ILiftMergerTestSuite Members

		[Test]
		[Ignore("not yet")]
		public void NewWritingSystemAlternativeHandled() {}

		#endregion

		[Test]
		public void EntryGetsLexemeFormWithUnheardOfLanguage()
		{
			LexEntry e = MakeSimpleEntry();
			LiftMultiText forms = new LiftMultiText();
			forms.Add("x99", "hello");
			_builder.MergeInLexemeForm(e, forms);
			Assert.AreEqual("hello", e.LexicalForm["x99"]);
		}

		[Test]
		public void NewEntryGetsLexemeForm()
		{
			LexEntry e = MakeSimpleEntry();
			LiftMultiText forms = new LiftMultiText();
			forms.Add("x", "hello");
			forms.Add("y", "bye");
			_builder.MergeInLexemeForm(e, forms);
			Assert.AreEqual(2, e.LexicalForm.Count);
		}

		[Test]
		public void EntryWithCitation()
		{
			LexEntry entry = MakeSimpleEntry();
			LiftMultiText forms = new LiftMultiText();
			forms.Add("x", "hello");
			forms.Add("y", "bye");
			_builder.MergeInCitationForm(entry, forms);

			MultiText citation = entry.GetProperty<MultiText>(LexEntry.WellKnownProperties.Citation);
			Assert.AreEqual(2, citation.Forms.Length);
			Assert.AreEqual("hello", citation["x"]);
			Assert.AreEqual("bye", citation["y"]);
		}

		[Test]
		public void EntryWithChildren()
		{
			Extensible extensibleInfo = new Extensible();
			LexEntry e = MakeSimpleEntry();
			LexSense s = _builder.GetOrMakeSense(e, extensibleInfo, string.Empty);

			LexExampleSentence ex = _builder.GetOrMakeExample(s, new Extensible());
			ex.Sentence["foo"] = "this is a sentence";
			ex.Translation["aa"] = "aaaa";
			_builder.FinishEntry(e);
			CheckCompleteEntry(e);

			RepositoryId[] entries = _dataMapper.GetAllItems();
			Assert.AreEqual(1, entries.Length);

			//now check it again, from the list
			CheckCompleteEntry(_dataMapper.GetItem(entries[0]));
		}

		[Test]
		public void MergeInTranslationForm_TypeFree_GetContentsAndSavesType()
		{
			LexExampleSentence ex = new LexExampleSentence();
			LiftMultiText translation = new LiftMultiText();
			translation.Add("aa", "aaaa");
			_builder.MergeInTranslationForm(ex, "Free translation", translation, "bogus raw xml");
			Assert.AreEqual("aaaa", ex.Translation["aa"]);
			Assert.AreEqual("Free translation", ex.TranslationType);
		}

		[Test]
		public void MergeInTranslationForm_UnheardOfType_StillBecomesTranslation()
		{
			LexExampleSentence ex = new LexExampleSentence();
			LiftMultiText translation = new LiftMultiText();
			translation.Add("aa", "aaaa");
			_builder.MergeInTranslationForm(ex, "madeUpType", translation, "bogus raw xml");
			Assert.AreEqual("aaaa", ex.Translation["aa"]);
			Assert.AreEqual("madeUpType",ex.TranslationType);
		}

		[Test]
		public void MergeInTranslationForm_NoType_GetContents()
		{
			LexExampleSentence ex = new LexExampleSentence();
			LiftMultiText translation = new LiftMultiText();
			translation.Add("aa", "aaaa");
			_builder.MergeInTranslationForm(ex, "", translation, "bogus raw xml");
			Assert.AreEqual("aaaa", ex.Translation["aa"]);
			Assert.IsTrue(string.IsNullOrEmpty(ex.TranslationType));
		}

		private static void CheckCompleteEntry(LexEntry entry)
		{
			Assert.AreEqual(1, entry.Senses.Count);
			LexSense sense = entry.Senses[0];
			Assert.AreEqual(1, sense.ExampleSentences.Count);
			LexExampleSentence example = sense.ExampleSentences[0];
			Assert.AreEqual("this is a sentence", example.Sentence["foo"]);
			Assert.AreEqual("aaaa", example.Translation["aa"]);
			Assert.AreEqual(entry, sense.Parent);
			Assert.AreEqual(entry, example.Parent.Parent);
		}

		[Test]
		[Ignore(
				"Haven't implemented protecting modified dates of, e.g., the entry as you add/merge its children."
				)]
		public void ModifiedDatesRetained() {}

		[Test]
		public void ChangedEntryFound()
		{
#if merging
			Guid g = Guid.NewGuid();
			Extensible extensibleInfo = CreateFullExtensibleInfo(g);

			LexEntry e = _repository.CreateItem();
			LexSense sense1 = new LexSense();
			LexSense sense2 = new LexSense();
			e.Senses.Add(sense1);
			e.Senses.Add(sense2);
			e.CreationTime = extensibleInfo.CreationTime;
			e.ModificationTime = new DateTime(e.CreationTime.Ticks + 100, DateTimeKind.Utc);

			LexEntry found = _merger.GetOrMakeEntry(extensibleInfo, 0);
			_merger.FinishEntry(found);
			Assert.AreSame(found, e);
			Assert.AreEqual(2, found.Senses.Count);

			//this is a temp side track
			Assert.AreEqual(1, _repository.CountAllItems());
			Extensible xInfo = CreateFullExtensibleInfo(Guid.NewGuid());
			LexEntry x = _merger.GetOrMakeEntry(xInfo, 1);
			_merger.FinishEntry(x);
			Assert.AreEqual(2, _repository.CountAllItems());
#endif
		}

		[Test]
		public void UnchangedEntryPruned()
		{
#if merging
			Guid g = Guid.NewGuid();
			Extensible extensibleInfo = CreateFullExtensibleInfo( g);

			LexEntry e = _repository.CreateItem();
			e.CreationTime = extensibleInfo.CreationTime;
			e.ModificationTime = extensibleInfo.ModificationTime;

			LexEntry found = _merger.GetOrMakeEntry(extensibleInfo, 0);
			Assert.IsNull(found);
#endif
		}

		[Test]
		[Ignore("This test is defective. found is always true CJP 2008-07-14")]
		public void EntryWithIncomingUnspecifiedModTimeNotPruned()
		{
			Guid g = Guid.NewGuid();
			Extensible eInfo = CreateFullExtensibleInfo(g);
			LexEntry item = _dataMapper.CreateItem();
			item.Guid = eInfo.Guid;
			item.Id = eInfo.Id;
			item.ModificationTime = eInfo.ModificationTime;
			item.CreationTime = eInfo.CreationTime;
			_dataMapper.SaveItem(item);

			//strip out the time
			eInfo.ModificationTime = Extensible.ParseDateTimeCorrectly("2005-01-01");
			Assert.AreEqual(DateTimeKind.Utc, eInfo.ModificationTime.Kind);

			LexEntry found = _builder.GetOrMakeEntry(eInfo, 0);
			Assert.IsNotNull(found);
		}

		[Test]
		public void ParseDateTimeCorrectly()
		{
			Assert.AreEqual(DateTimeKind.Utc,
							Extensible.ParseDateTimeCorrectly("2003-08-07T08:42:42Z").Kind);
			Assert.AreEqual(DateTimeKind.Utc,
							Extensible.ParseDateTimeCorrectly("2005-01-01T01:11:11+8:00").Kind);
			Assert.AreEqual(DateTimeKind.Utc, Extensible.ParseDateTimeCorrectly("2005-01-01").Kind);
			Assert.AreEqual("00:00:00",
							Extensible.ParseDateTimeCorrectly("2005-01-01").TimeOfDay.ToString());
		}

		[Test]
		[Ignore("Haven't implemented this.")]
		public void MergingSameEntryLackingGuidId_TwiceFindMatch() {}

		private static Extensible AddDates(Extensible extensibleInfo)
		{
			extensibleInfo.CreationTime = Extensible.ParseDateTimeCorrectly("2003-08-07T08:42:42Z");
			extensibleInfo.ModificationTime =
					Extensible.ParseDateTimeCorrectly("2005-01-01T01:11:11+8:00");
			return extensibleInfo;
		}

		private static Extensible CreateFullExtensibleInfo(Guid g)
		{
			Extensible extensibleInfo = new Extensible();
			extensibleInfo.Guid = g;
			extensibleInfo = AddDates(extensibleInfo);
			return extensibleInfo;
		}

		[Test]
		public void ExpectedAtomicTraitOnEntry()
		{
			_builder.ExpectedOptionTraits = new[]{"flub"};
			LexEntry e = MakeSimpleEntry();
			_builder.MergeInTrait(e, new Trait("flub", "dub"));
			Assert.AreEqual(1, e.Properties.Count);
			Assert.AreEqual("flub", e.Properties[0].Key);
			OptionRef option = e.GetProperty<OptionRef>("flub");
			Assert.AreEqual("dub", option.Value);
		}

		[Test]
		public void UnexpectedAtomicTraitRetained()
		{
			LexEntry e = MakeSimpleEntry();
			_builder.MergeInTrait(e, new Trait("flub", "dub"));
			Assert.AreEqual(1, e.Properties.Count);
			Assert.AreEqual("flub", e.Properties[0].Key);
			OptionRefCollection option = e.GetProperty<OptionRefCollection>("flub");
			Assert.IsTrue(option.Contains("dub"));
		}

		[Test]
		public void ExpectedCollectionTrait()
		{
			_builder.ExpectedOptionCollectionTraits.Add("flub");
			LexEntry e = MakeSimpleEntry();
			_builder.MergeInTrait(e, new Trait("flub", "dub"));
			_builder.MergeInTrait(e, new Trait("flub", "stub"));
			Assert.AreEqual(1, e.Properties.Count);
			Assert.AreEqual("flub", e.Properties[0].Key);
			OptionRefCollection options = e.GetProperty<OptionRefCollection>("flub");
			Assert.AreEqual(2, options.Count);
			Assert.IsTrue(options.Contains("dub"));
			Assert.IsTrue(options.Contains("stub"));
		}

		[Test]
		public void UnexpectedAtomicCollectionRetained()
		{
			LexEntry e = MakeSimpleEntry();
			_builder.MergeInTrait(e, new Trait("flub", "dub"));
			_builder.MergeInTrait(e, new Trait("flub", "stub"));
			Assert.AreEqual(1, e.Properties.Count);
			Assert.AreEqual("flub", e.Properties[0].Key);
			OptionRefCollection option = e.GetProperty<OptionRefCollection>("flub");
			Assert.IsTrue(option.Contains("dub"));
			Assert.IsTrue(option.Contains("stub"));
		}

		[Test]
		public void ExpectedCustomField()
		{
			LexEntry e = MakeSimpleEntry();
			LiftMultiText t = new LiftMultiText();
			t["z"] = new LiftString("dub");
			_builder.MergeInField(e, "flub", default(DateTime), default(DateTime), t, null);
			Assert.AreEqual(1, e.Properties.Count);
			Assert.AreEqual("flub", e.Properties[0].Key);
			MultiText mt = e.GetProperty<MultiText>("flub");
			Assert.AreEqual("dub", mt["z"]);
		}

		[Test]
		public void UnexpectedCustomFieldRetained()
		{
			LexEntry e = MakeSimpleEntry();
			LiftMultiText t = new LiftMultiText();
			t["z"] = new LiftString("dub");
			_builder.MergeInField(e, "flub", default(DateTime), default(DateTime), t, null);
			Assert.AreEqual(1, e.Properties.Count);
			Assert.AreEqual("flub", e.Properties[0].Key);
			MultiText mt = e.GetProperty<MultiText>("flub");
			Assert.AreEqual("dub", mt["z"]);
		}

		[Test]
		public void EntryGetsFlag()
		{
			LexEntry e = MakeSimpleEntry();
			_builder.MergeInTrait(e, new Trait(LexEntry.WellKnownProperties.FlagSkipBaseForm, null));
			Assert.IsTrue(e.GetHasFlag(LexEntry.WellKnownProperties.FlagSkipBaseForm));
		}

		[Test]
		public void SenseGetsPictureNoCaption()
		{
			Extensible extensibleInfo = new Extensible();
			LexEntry e = MakeSimpleEntry();
			LexSense s = _builder.GetOrMakeSense(e, extensibleInfo, string.Empty);

			_builder.MergeInPicture(s, "testPicture.png", null);
			PictureRef pict = s.GetProperty<PictureRef>("Picture");
			Assert.AreEqual("testPicture.png", pict.Value);
			Assert.IsNull(pict.Caption);
		}

		[Test]
		public void SenseGetsPictureWithCaption()
		{
			Extensible extensibleInfo = new Extensible();
			LexEntry e = MakeSimpleEntry();
			LexSense s = _builder.GetOrMakeSense(e, extensibleInfo, string.Empty);

			LiftMultiText caption = new LiftMultiText();
			caption["aa"] = new LiftString("acaption");
			_builder.MergeInPicture(s, "testPicture.png", caption);
			PictureRef pict = s.GetProperty<PictureRef>("Picture");
			Assert.AreEqual("testPicture.png", pict.Value);
			Assert.AreEqual("acaption", pict.Caption["aa"]);
		}

		[Test]
		public void GetOrMakeEntry_ReturnedLexEntryIsDirty()
		{
			Extensible extensibleInfo = new Extensible();
			LexEntry entry = _builder.GetOrMakeEntry(extensibleInfo, 0);
			Assert.IsTrue(entry.IsDirty);
		}
	}
}