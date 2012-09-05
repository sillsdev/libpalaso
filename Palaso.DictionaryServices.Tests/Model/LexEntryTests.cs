using System;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;

namespace Palaso.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexEntryTests
	{
		private LexEntry _entry;
		private LexSense _sense;
		private LexExampleSentence _examples;
		private bool _removed;

		[SetUp]
		public void Setup()
		{
			_entry = new LexEntry();
			_sense = new LexSense();
			_entry.Senses.Add(_sense);
#if GlossMeaning
			this._sense.Gloss["th"] = "sense";
#else
			_sense.Definition["th"] = "sense";
#endif
			MultiText customFieldInSense =
				_sense.GetOrCreateProperty<MultiText>("customFieldInSense");
			customFieldInSense["th"] = "custom";
			_examples = new LexExampleSentence();
			_sense.ExampleSentences.Add(_examples);
			_examples.Sentence["th"] = "example";
			_examples.Translation["en"] = "translation";
			MultiText customFieldInExample =
				_examples.GetOrCreateProperty<MultiText>("customFieldInExample");
			customFieldInExample["th"] = "custom";
			_entry.EmptyObjectsRemoved += _entry_EmptyObjectsRemoved;
			_entry.PropertyChanged += _entry_PropertyChanged;
			_removed = false;
		}

		[Test]
		public void ExampleSentencePropertiesInUse_HasPropertyProp1_ReturnsProp1()
		{
			var ex = new LexExampleSentence();
			ex.GetOrCreateProperty<MultiText>("Prop1");
			Assert.That(ex.PropertiesInUse, Contains.Item("Prop1"));
			Assert.That(ex.PropertiesInUse.Count(), Is.EqualTo(3));
		}

		[Test]
		public void ExampleSentencePropertiesInUse_HasMultipleProperties_ReturnsProperties()
		{
			var ex = new LexExampleSentence();
			ex.GetOrCreateProperty<MultiText>("Prop1");
			ex.GetOrCreateProperty<MultiText>("Prop2");
			Assert.That(ex.PropertiesInUse, Contains.Item("Prop1"));
			Assert.That(ex.PropertiesInUse, Contains.Item("Prop2"));
			Assert.That(ex.PropertiesInUse.Count(), Is.EqualTo(4));
		}

		[Test]
		public void LexSensePropertiesInUse_HasPropertyProp1_ReturnsProp1()
		{
			var lexSense = new LexSense();
			lexSense.GetOrCreateProperty<MultiText>("Prop1");
			Assert.That(lexSense.PropertiesInUse, Contains.Item("Prop1"));
			Assert.That(lexSense.PropertiesInUse.Count(), Is.EqualTo(1));
		}

		[Test]
		public void LexSensePropertiesInUse_HasMultipleProperties_ReturnsProperties()
		{
			var lexSense = new LexSense();
			lexSense.GetOrCreateProperty<MultiText>("Prop1");
			lexSense.GetOrCreateProperty<MultiText>("Prop2");
			Assert.That(lexSense.PropertiesInUse, Contains.Item("Prop1"));
			Assert.That(lexSense.PropertiesInUse, Contains.Item("Prop2"));
			Assert.That(lexSense.PropertiesInUse.Count(), Is.EqualTo(2));
		}

		[Test]
		public void LexSensePropertiesInUse_SenseAndMulitipleExampleSentencesHaveMultipleProperties_ReturnsAllProperties()
		{
			var ex1 = new LexExampleSentence();
			ex1.GetOrCreateProperty<MultiText>("Ex1Prop1");
			ex1.GetOrCreateProperty<MultiText>("Ex1Prop2");
			var ex2 = new LexExampleSentence();
			ex2.GetOrCreateProperty<MultiText>("Ex2Prop1");
			ex2.GetOrCreateProperty<MultiText>("Ex2Prop2");
			var lexSense = new LexSense();
			lexSense.GetOrCreateProperty<MultiText>("Prop1");
			lexSense.GetOrCreateProperty<MultiText>("Prop2");
			lexSense.ExampleSentences.Add(ex1);
			lexSense.ExampleSentences.Add(ex2);
			Assert.That(lexSense.PropertiesInUse, Contains.Item("Ex1Prop1"));
			Assert.That(lexSense.PropertiesInUse, Contains.Item("Ex1Prop2"));
			Assert.That(lexSense.PropertiesInUse, Contains.Item("Ex2Prop1"));
			Assert.That(lexSense.PropertiesInUse, Contains.Item("Ex2Prop2"));
			Assert.That(lexSense.PropertiesInUse, Contains.Item("Prop1"));
			Assert.That(lexSense.PropertiesInUse, Contains.Item("Prop2"));
			Assert.That(lexSense.PropertiesInUse.Count(), Is.EqualTo(10));
		}

		[Test]
		public void LexEntryPropertiesInUse_HasPropertyProp1_ReturnsProp1()
		{
			var entry = new LexEntry();
			entry.GetOrCreateProperty<MultiText>("Prop1");
			Assert.That(entry.PropertiesInUse, Contains.Item("Prop1"));
			Assert.That(entry.PropertiesInUse.Count(), Is.EqualTo(1));
		}

		[Test]
		public void LexEntryPropertiesInUse_HasMultipleProperties_ReturnsProperties()
		{
			var entry = new LexEntry();
			entry.GetOrCreateProperty<MultiText>("Prop1");
			entry.GetOrCreateProperty<MultiText>("Prop2");
			Assert.That(entry.PropertiesInUse, Contains.Item("Prop1"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Prop2"));
			Assert.That(entry.PropertiesInUse.Count(), Is.EqualTo(2));
		}

		[Test]
		public void LexEntryPropertiesInUse_EntryandMultipleSensesAndExampleSentencesHaveMultipleProperties_ReturnsAllProperties()
		{
			var ex1 = new LexExampleSentence();
			ex1.GetOrCreateProperty<MultiText>("Ex1Prop1");
			ex1.GetOrCreateProperty<MultiText>("Ex1Prop2");

			var ex2 = new LexExampleSentence();
			ex2.GetOrCreateProperty<MultiText>("Ex2Prop1");
			ex2.GetOrCreateProperty<MultiText>("Ex2Prop2");

			var ex3 = new LexExampleSentence();
			ex3.GetOrCreateProperty<MultiText>("Ex3Prop1");
			ex3.GetOrCreateProperty<MultiText>("Ex3Prop2");

			var lexSense1 = new LexSense();
			lexSense1.GetOrCreateProperty<MultiText>("Se1Prop1");
			lexSense1.GetOrCreateProperty<MultiText>("Se1Prop2");

			var lexSense2 = new LexSense();
			lexSense2.GetOrCreateProperty<MultiText>("Se2Prop1");
			lexSense2.GetOrCreateProperty<MultiText>("Se2Prop2");

			var entry = new LexEntry();
			entry.GetOrCreateProperty<MultiText>("Prop1");
			entry.GetOrCreateProperty<MultiText>("Prop2");

			entry.Senses.Add(lexSense1);
			entry.Senses.Add(lexSense2);
			lexSense1.ExampleSentences.Add(ex1);
			lexSense1.ExampleSentences.Add(ex2);
			lexSense2.ExampleSentences.Add(ex3);

			Assert.That(entry.PropertiesInUse, Contains.Item("Ex1Prop1"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Ex1Prop2"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Ex2Prop1"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Ex2Prop2"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Ex3Prop1"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Ex3Prop2"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Se1Prop1"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Se1Prop2"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Se2Prop1"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Se2Prop2"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Prop1"));
			Assert.That(entry.PropertiesInUse, Contains.Item("Prop2"));
			Assert.That(entry.PropertiesInUse.Count(), Is.EqualTo(18));
		}

		[Test]
		public void Cleanup_HasBaseform_PropertyIsNotRemoved()
		{
			var target = new LexEntry();
			_entry = new LexEntry();
			_entry.LexicalForm["v"] = "hello";
			_entry.AddRelationTarget(LexEntry.WellKnownProperties.BaseForm, target.GetOrCreateId(true));
			_entry.CleanUpAfterEditting();
			Assert.IsNotNull(_entry.GetProperty<LexRelationCollection>(LexEntry.WellKnownProperties.BaseForm));
		}

		[Test]
		public void Cleanup_HasEmptyBaseform_PropertyIsRemoved()
		{
			var target = new LexEntry();
			_entry = new LexEntry();
			_entry.LexicalForm["v"] = "hello";
			_entry.AddRelationTarget(LexEntry.WellKnownProperties.BaseForm, string.Empty);
			Assert.IsNotNull(_entry.GetProperty<LexRelationCollection>(LexEntry.WellKnownProperties.BaseForm));
			_entry.CleanUpAfterEditting();
			Assert.IsNull(_entry.GetProperty<LexRelationCollection>(LexEntry.WellKnownProperties.BaseForm));
		}

		private void _entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			_entry.CleanUpEmptyObjects();
		}

		private void _entry_EmptyObjectsRemoved(object sender, EventArgs e)
		{
			_removed = true;
		}

		private void ClearExampleSentence()
		{
			_examples.Sentence["th"] = string.Empty;
		}

		private void ClearExampleTranslation()
		{
			_examples.Translation["en"] = string.Empty;
		}

		private void ClearExampleCustom()
		{
			MultiText customFieldInExample =
				_examples.GetOrCreateProperty<MultiText>("customFieldInExample");
			customFieldInExample["th"] = string.Empty;
			_entry.CleanUpAfterEditting();
		}

		private void ClearSenseMeaning()
		{
#if GlossMeaning
			this._sense.Gloss["th"] = string.Empty;
#else
			_sense.Definition["th"] = string.Empty;
#endif
		}

		private void ClearSenseExample()
		{
			ClearExampleSentence();
			ClearExampleTranslation();
			ClearExampleCustom();
			_removed = false;
		}

		private void ClearSenseCustom()
		{
			MultiText customFieldInSense =
				_sense.GetOrCreateProperty<MultiText>("customFieldInSense");
			customFieldInSense["th"] = string.Empty;
			_entry.CleanUpAfterEditting();
		}

		[Test]
		public void Example_Empty_False()
		{
			Assert.IsFalse(_examples.IsEmpty);
		}

		[Test]
		public void ExampleWithOnlySentence_Empty_False()
		{
			ClearExampleTranslation();
			ClearExampleCustom();
			Assert.IsFalse(_removed);
			Assert.IsFalse(_examples.IsEmpty);
		}

		[Test]
		public void ExampleWithOnlyTranslation_Empty_False()
		{
			ClearExampleSentence();
			ClearExampleCustom();
			Assert.IsFalse(_removed);
			Assert.IsFalse(_examples.IsEmpty);
		}

		[Test]
		public void ExampleWithOnlyCustomField_Empty_False()
		{
			ClearExampleSentence();
			ClearExampleTranslation();
			Assert.IsFalse(_removed);
			Assert.IsFalse(_examples.IsEmpty);
		}

		[Test]
		public void ExampleWithNoFields_Empty_True()
		{
			ClearExampleSentence();
			ClearExampleTranslation();
			ClearExampleCustom();
			Assert.IsTrue(_removed);
			Assert.IsTrue(_examples.IsEmpty);
		}

		[Test]
		public void EmptyExampleRemoved()
		{
			ClearExampleSentence();
			ClearExampleTranslation();
			ClearExampleCustom();
			Assert.IsTrue(_removed);
			Assert.AreEqual(0, _sense.ExampleSentences.Count);
		}

		[Test]
		public void SenseWithOnlyMeaning_Empty_False()
		{
			ClearSenseExample();
			ClearSenseCustom();
			Assert.IsFalse(_removed);
			Assert.IsFalse(_sense.IsEmpty);
		}

		[Test]
		public void SenseWithOnlyExample_Empty_False()
		{
			ClearSenseMeaning();
			ClearSenseCustom();
			Assert.IsFalse(_removed);
			Assert.IsFalse(_sense.IsEmpty);
		}

		[Test]
		public void SenseWithOnlyCustom_Empty_False()
		{
			ClearSenseMeaning();
			ClearSenseExample();
			Assert.IsFalse(_removed);
			Assert.IsFalse(_sense.IsEmpty);
		}

		[Test]
		public void SenseWithNoExampleOrField_Empty_True()
		{
			ClearSenseMeaning();
			ClearSenseExample();
			ClearSenseCustom();
			Assert.IsTrue(_removed);
			Assert.IsTrue(_sense.IsEmpty);
		}

		[Test]
		public void SenseWithOnlyPOS_ReadyForDeletion()
		{
			Assert.IsFalse(_sense.IsEmptyForPurposesOfDeletion);
			ClearSenseMeaning();
			ClearSenseExample();
			ClearSenseCustom();
			Assert.IsTrue(_sense.IsEmpty);
			OptionRef pos =
				_sense.GetOrCreateProperty<OptionRef>(LexSense.WellKnownProperties.PartOfSpeech);
			pos.Value = "noun";
			Assert.IsFalse(_sense.IsEmpty);
			Assert.IsTrue(_sense.IsEmptyForPurposesOfDeletion);
		}

		//Not fixed yet       [Test]
		public void SenseWithAPicture_ReadyForDeletion()
		{
			Assert.IsFalse(_sense.IsEmptyForPurposesOfDeletion);
			ClearSenseMeaning();
			ClearSenseExample();
			ClearSenseCustom();
			Assert.IsTrue(_sense.IsEmpty);
			PictureRef pict =
				_sense.GetOrCreateProperty<PictureRef>(LexSense.WellKnownProperties.Picture);
			pict.Value = "dummy.png";
			Assert.IsFalse(_sense.IsEmpty);
			Assert.IsTrue(_sense.IsEmptyForPurposesOfDeletion);
		}

		[Test]
		public void EmptySensesRemoved()
		{
			ClearSenseMeaning();
			ClearSenseExample();
			ClearSenseCustom();
			Assert.IsTrue(_removed);
			Assert.AreEqual(0, _entry.Senses.Count);
		}

		[Test]
		public void GetOrCreateSenseWithMeaning_SenseDoesNotExist_NewSenseWithMeaning()
		{
			MultiText meaning = new MultiText();
			meaning.SetAlternative("th", "new");

			LexSense sense = _entry.GetOrCreateSenseWithMeaning(meaning);
			Assert.AreNotSame(_sense, sense);
#if GlossMeaning
			Assert.AreEqual("new", sense.Gloss.GetExactAlternative("th"));
#else
			Assert.AreEqual("new", sense.Definition.GetExactAlternative("th"));
#endif
		}

		[Test]
		public void GetOrCreateSenseWithMeaning_SenseWithEmptyStringExists_ExistingSense()
		{
			ClearSenseMeaning();

			MultiText meaning = new MultiText();
			meaning.SetAlternative("th", string.Empty);

			LexSense sense = _entry.GetOrCreateSenseWithMeaning(meaning);
			Assert.AreSame(_sense, sense);
		}

		[Test]
		public void GetOrCreateSenseWithMeaning_SenseDoesExists_ExistingSense()
		{
			MultiText meaning = new MultiText();
			meaning.SetAlternative("th", "sense");

			LexSense sense = _entry.GetOrCreateSenseWithMeaning(meaning);
			Assert.AreSame(_sense, sense);
		}

		[Test]
		public void GetHeadword_EmptyEverything_ReturnsEmptyString()
		{
			LexEntry entry = new LexEntry();
			Assert.AreEqual(string.Empty, entry.GetHeadWordForm("a"));
		}

		[Test]
		public void GetHeadword_LexemeForm_ReturnsCorrectAlternative()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm.SetAlternative("c", "can");
			entry.LexicalForm.SetAlternative("a", "apple");
			entry.LexicalForm.SetAlternative("b", "bart");
			Assert.AreEqual("apple", entry.GetHeadWordForm("a"));
			Assert.AreEqual("bart", entry.GetHeadWordForm("b"));
			Assert.AreEqual(string.Empty, entry.GetHeadWordForm("donthave"));
		}

		[Test]
		public void GetHeadword_CitationFormHasAlternative_CorrectForm()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm.SetAlternative("a", "apple");
			MultiText citation =
				entry.GetOrCreateProperty<MultiText>(LexEntry.WellKnownProperties.Citation);
			citation.SetAlternative("b", "barter");
			citation.SetAlternative("a", "applishus");
			Assert.AreEqual("applishus", entry.GetHeadWordForm("a"));
			Assert.AreEqual("barter", entry.GetHeadWordForm("b"));
			Assert.AreEqual(string.Empty, entry.GetHeadWordForm("donthave"));
		}

		[Test]
		public void GetHeadword_CitationFormLacksAlternative_GetsFormFromLexemeForm()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm.SetAlternative("a", "apple");
			MultiText citation =
				entry.GetOrCreateProperty<MultiText>(LexEntry.WellKnownProperties.Citation);
			citation.SetAlternative("b", "bater");
			Assert.AreEqual("apple", entry.GetHeadWordForm("a"));
		}

		[Test]
		public void LexEntryConstructor_IsDirtyReturnsTrue()
		{
			LexEntry entry = new LexEntry();
			Assert.IsTrue(entry.IsDirty);
		}

		[Test]
		public void Clean_IsDirtyReturnsFalse()
		{
			LexEntry entry = new LexEntry();
			entry.Clean();
			Assert.IsFalse(entry.IsDirty);
		}

		[Test]
		public void LexEntryChanges_IsDirtyReturnsTrue()
		{
			LexEntry entry = new LexEntry();
			entry.Clean();
			entry.Senses.Add(new LexSense());
			Assert.IsTrue(entry.IsDirty);
		}
	}
}