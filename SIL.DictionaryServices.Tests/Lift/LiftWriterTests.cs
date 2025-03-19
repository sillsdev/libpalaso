using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using NUnit.Framework;
using SIL.DictionaryServices.Lift;
using SIL.DictionaryServices.Model;
using SIL.Lift;
using SIL.Lift.Options;
using SIL.Lift.Validation;
using SIL.TestUtilities;

namespace SIL.DictionaryServices.Tests.Lift
{
	[TestFixture]
	public class LiftWriterTests
	{
		class LiftExportTestSessionBase : IDisposable
		{
			protected LiftWriter _liftWriter;

			private readonly StringBuilder _stringBuilder;
			protected readonly string _filePath;

			protected LiftExportTestSessionBase()
			{
				_filePath = Path.GetTempFileName();
				_stringBuilder = new StringBuilder();
			}

			public void Dispose()
			{
				if (_liftWriter != null)
				{
					LiftWriter.Dispose();
				}
				File.Delete(_filePath);
			}

			public string FilePath
			{
				get { return _filePath; }
			}

			public StringBuilder StringBuilder
			{
				get { return _stringBuilder; }
			}

			public LiftWriter LiftWriter
			{
				get { return _liftWriter; }
			}

			public LexEntry CreateItem()
			{
				return new LexEntry();
			}

			public string OutputString()
			{
				LiftWriter.End();
				return StringBuilder.ToString();
			}

			private void AddTestLexEntry(string lexicalForm)
			{
				LexEntry entry = CreateItem();
				entry.LexicalForm["test"] = lexicalForm;
				LiftWriter.Add(entry);
			}

			public void AddTwoTestLexEntries()
			{
				AddTestLexEntry("sunset");
				AddTestLexEntry("flower");
				LiftWriter.End();
			}
		}

		class LiftExportAsFragmentTestSession : LiftExportTestSessionBase
		{
			public LiftExportAsFragmentTestSession()
			{
				_liftWriter = new LiftWriter(StringBuilder, true);
			}
		}

		class LiftExportAsFullDocumentTestSession : LiftExportTestSessionBase
		{
			public LiftExportAsFullDocumentTestSession()
			{
				_liftWriter = new LiftWriter(StringBuilder, false);
			}
		}

		class LiftExportAsFileTestSession : LiftExportTestSessionBase
		{
			public LiftExportAsFileTestSession()
			{
				_liftWriter = new LiftWriter(_filePath, LiftWriter.ByteOrderStyle.BOM);
			}
		}

		[SetUp]
		public void Setup()
		{
		}

		[TearDown]
		public void TearDown()
		{
			GC.Collect();
		}

		private static void AssertHasOneMatch(string xpath, LiftExportTestSessionBase session)
		{
			AssertThatXmlIn.String(session.StringBuilder.ToString())
				.HasSpecifiedNumberOfMatchesForXpath(xpath, 1);
		}

		private static void AssertHasMatches(
			string xpath, LiftExportTestSessionBase session, int count)
		{
			AssertThatXmlIn.String(session.StringBuilder.ToString())
				.HasSpecifiedNumberOfMatchesForXpath(xpath, count);
		}

		private static void AssertHasAtLeastOneMatch(
			string xpath, LiftExportTestSessionBase session)
		{
			AssertThatXmlIn.String(session.StringBuilder.ToString())
				.HasAtLeastOneMatchForXpath(xpath);
		}

		private static string GetSenseElement(LexSense sense, string innerXml)
		{
			return string.Format("<sense id=\"{0}\">{1}</sense>", sense.GetOrCreateId(), innerXml);
		}

		private static string GetStringAttributeOfTopElement(string attribute, LiftExportTestSessionBase session)
		{
			var doc = new XmlDocument();
			doc.LoadXml(session.StringBuilder.ToString());
			return doc.FirstChild.Attributes[attribute].ToString();
		}

		private static void AssertEqualsCanonicalString(string expected, string actual)
		{
			string canonicalAnswer = CanonicalXml.ToCanonicalStringFragment(expected);
			Assert.AreEqual(canonicalAnswer, actual);
		}

		[Test]
		public void AddUsingWholeList_TwoEntries_HasTwoEntries()
		{
			using (var session = new LiftExportAsFullDocumentTestSession())
			{
				session.AddTwoTestLexEntries();
				var doc = new XmlDocument();
				doc.LoadXml(session.StringBuilder.ToString());
				Assert.AreEqual(2, doc.SelectNodes("lift/entry").Count);
			}
		}

		[Test]
		public void AttributesWithProblematicCharacters()
		{
			const string expected = "lang=\"x&quot;y\">";
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				sense.Gloss["x\"y"] = "test";
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				string result = session.OutputString();
				Assert.IsTrue(result.Contains(expected));
			}
		}

		[Test]
		public void BlankExample()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				session.LiftWriter.Add(new LexExampleSentence());
				AssertEqualsCanonicalString("<example />", session.OutputString());
			}
		}

		[Test]
		public void BlankGrammi()
		{
			var sense = new LexSense();
			var o = sense.GetOrCreateProperty<OptionRef>(
				LexSense.WellKnownProperties.PartOfSpeech
			);
			o.Value = string.Empty;
			using (var session = new LiftExportAsFragmentTestSession())
			{
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense[not(grammatical-info)]", session);
				AssertHasAtLeastOneMatch("sense[not(trait)]", session);
			}
		}

		[Test]
		public void BlankMultiText()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				session.LiftWriter.AddMultitextForms(null, new MultiText());
				AssertEqualsCanonicalString("", session.OutputString());
			}
		}

		[Test]
		public void BlankSense()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				session.LiftWriter.Add(sense);
				AssertEqualsCanonicalString(
					String.Format("<sense id=\"{0}\" />", sense.GetOrCreateId()),
					session.OutputString()
				);
			}
		}

		[Test]
		public void Citation()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				var citation = entry.GetOrCreateProperty<MultiText>(
					LexEntry.WellKnownProperties.Citation
				);
				citation["zz"] = "orange";
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("entry/citation/form[@lang='zz']/text[text()='orange']", session);
				AssertHasAtLeastOneMatch("entry/citation/form[@lang='zz'][not(trait)]", session);
				AssertHasAtLeastOneMatch("entry[not(field)]", session);
			}
		}

		[Test]
		public void CitationWithStarredForm()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var e = session.CreateItem();
				var citation = e.GetOrCreateProperty<MultiText>(
					LexEntry.WellKnownProperties.Citation
				);
				citation.SetAlternative("x", "orange");
				citation.SetAnnotationOfAlternativeIsStarred("x", true);
				// _lexEntryRepository.SaveItem(e);
				session.LiftWriter.Add(e);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch(
					"entry/citation/form[@lang='x']/annotation[@name='flag' and @value='1']",
					session
				);
			}
		}

		[Test]
		public void EntryWith2SimpleVariants()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var e = session.CreateItem();
				var variant = new LexVariant();
				variant.SetAlternative("etr", "one");
				e.Variants.Add(variant);
				variant = new LexVariant();
				variant.SetAlternative("etr", "two");
				e.Variants.Add(variant);
				session.LiftWriter.Add(e);
				session.LiftWriter.End();
				AssertHasOneMatch("entry/variant/form[@lang='etr' and text='one']", session);
				AssertHasOneMatch("entry/variant/form[@lang='etr' and text='two']", session);
			}
		}

		[Test]
		public void EntryWithSimpleEtymology()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var e = session.CreateItem();
				var etymology = new LexEtymology("theType", "theSource");
				etymology.SetAlternative("etr", "one");
				e.Etymologies.Add(etymology);
				session.LiftWriter.Add(e);
				session.LiftWriter.End();
				AssertHasOneMatch("entry/etymology/form[@lang='etr' and text='one']", session);
				AssertHasOneMatch("entry/etymology[@type='theType' and @source='theSource']", session);
			}
		}

		[Test]
		public void EntryWithFullEtymology()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var e = session.CreateItem();
				var etymology = new LexEtymology("theType", "theSource");
				etymology.SetAlternative("etr", "theProtoform");
				etymology.Gloss.SetAlternative("en", "engloss");
				etymology.Gloss.SetAlternative("fr", "frgloss");
				etymology.Comment.SetAlternative("en", "metathesis?");
				e.Etymologies.Add(etymology);
				session.LiftWriter.Add(e);
				session.LiftWriter.End();
				AssertHasOneMatch("entry/etymology/form[@lang='etr' and text='theProtoform']", session);
				AssertHasOneMatch("entry/etymology[@type='theType' and @source='theSource']", session);

				//handling of comments may change, the issue has been raised on the LIFT google group
				AssertHasOneMatch("entry/etymology/field[@type='comment']/form[@lang='en' and text='metathesis?']", session);
			}
		}

		[Test]
		public void EntryWith2SimpleFields()
		{
			using var session = new LiftExportAsFragmentTestSession();
			var e = session.CreateItem();
			var field1 = new LexField("theType");
			field1.SetAlternative("en", "one");
			e.Fields.Add(field1);
			var field2 = new LexField("theType");
			field2.SetAlternative("es", "dos");
			e.Fields.Add(field2);
			session.LiftWriter.Add(e);
			session.LiftWriter.End();

			AssertHasMatches("entry/field[@type='theType']", session, 2);
			AssertHasOneMatch("entry/field/form[@lang='en' and text='one']", session);
			AssertHasOneMatch("entry/field/form[@lang='es' and text='dos']", session);
		}

		[Test]
		public void EntryWithFullField()
		{
			using var session = new LiftExportAsFragmentTestSession();
			var e = session.CreateItem();
			var field = new LexField("theType");
			field.Traits.Add(new LexTrait("givenName", "Joe"));
			field.Traits.Add(new LexTrait("surname", "DiMaggio"));
			field.SetAlternative("etr", "theProtoform");
			field.SetAlternative("en", "enForm");
			field.SetAlternative("fr", "frForm");
			e.Fields.Add(field);
			session.LiftWriter.Add(e);
			session.LiftWriter.End();

			AssertHasOneMatch("entry/field[@type='theType']", session);
			AssertHasMatches("entry/field/trait", session, 2);
			AssertHasOneMatch("entry/field/trait[@name='givenName' and @value='Joe']", session);
			AssertHasOneMatch("entry/field/trait[@name='surname' and @value='DiMaggio']", session);
			AssertHasMatches("entry/field/form", session, 3);
			AssertHasOneMatch("entry/field/form[@lang='etr' and text='theProtoform']", session);
			AssertHasOneMatch("entry/field/form[@lang='en' and text='enForm']", session);
			AssertHasOneMatch("entry/field/form[@lang='fr' and text='frForm']", session);
		}

		[Test]
		public void EntryWithBorrowedWord()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var e = session.CreateItem();
				//notice, no form given
				e.Etymologies.Add(new LexEtymology("theType", "theSource"));
				session.LiftWriter.Add(e);
				session.LiftWriter.End();
				AssertHasOneMatch("entry/etymology[@type='theType' and @source='theSource']", session);
			}
		}

		[Test]
		public void EntryWithSimplePronunciation()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var e = session.CreateItem();
				var phonetic = new LexPhonetic();
				phonetic.SetAlternative("ipa", "one");
				e.Pronunciations.Add(phonetic);
				session.LiftWriter.Add(e);
				session.LiftWriter.End();
				AssertHasOneMatch("entry/pronunciation/form[@lang='ipa' and text='one']", session);
			}
		}

		[Test]
		public void SenseWith2Notes()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				var note = new LexNote("grammar");
				note.SetAlternative("etr", "one");
				sense.Notes.Add(note);
				var note2 = new LexNote("comment");
				note2.SetAlternative("etr", "blah");
				sense.Notes.Add(note2);
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasOneMatch("sense/note/form[@lang='etr' and text='one']", session);
				AssertHasOneMatch("sense/note[@type='grammar']", session);
				AssertHasOneMatch("sense/note[@type='comment']", session);
			}
		}

		[Test]
		public void SenseWith2Reversals()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				var reversal = new LexReversal { Type = "revType" };
				reversal.SetAlternative("en", "one");
				sense.Reversals.Add(reversal);
				var reversal2 = new LexReversal();
				reversal2.SetAlternative("en", "two");
				sense.Reversals.Add(reversal2);
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasOneMatch("sense/reversal/form[@lang='en' and text='one']", session);
				AssertHasOneMatch("sense/reversal/form[@lang='en' and text='two']", session);
				AssertHasOneMatch("sense/reversal[@type='revType']", session);
				AssertHasOneMatch("sense/reversal/@type", session); //only one had a type
			}
		}

		[Test]
		public void EntryWithTypedNote()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				var note = new LexNote("comic");
				note.SetAlternative("etr", "one");
				sense.Notes.Add(note);
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasOneMatch("sense/note/form[@lang='etr' and text='one']", session);
				AssertHasOneMatch("sense/note[@type='comic']", session);
			}
		}

		[Test]
		public void VariantWith2Traits()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var variant = new LexVariant();
				variant.SetAlternative("etr", "one");
				variant.Traits.Add(new LexTrait("a", "A"));
				variant.Traits.Add(new LexTrait("b", "B"));
				session.LiftWriter.AddVariant(variant);
				session.LiftWriter.End();
				AssertHasOneMatch("variant/trait[@name='a' and @value='A']", session);
				AssertHasOneMatch("variant/trait[@name='b' and @value='B']", session);
			}
		}

		[Test]
		public void VariantWith2SimpleFields()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var variant = new LexVariant();
				variant.SetAlternative("etr", "one");
				var fieldA = new LexField("a");
				fieldA.SetAlternative("en", "aaa");
				variant.Fields.Add(fieldA);
				var fieldB = new LexField("b");
				fieldB.SetAlternative("en", "bbb");
				variant.Fields.Add(fieldB);
				session.LiftWriter.AddVariant(variant);
				session.LiftWriter.End();
				AssertHasOneMatch("variant/field[@type='a']/form[@lang='en' and text = 'aaa']", session);
				AssertHasOneMatch("variant/field[@type='b']/form[@lang='en' and text = 'bbb']", session);
			}
		}

		[Test]
		public void FieldWithTraits()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var variant = new LexVariant();
				variant.SetAlternative("etr", "one");
				var fieldA = new LexField("a");
				fieldA.SetAlternative("en", "aaa");
				fieldA.Traits.Add(new LexTrait("one", "1"));
				variant.Fields.Add(fieldA);
				session.LiftWriter.AddVariant(variant);
				session.LiftWriter.End();
				AssertHasOneMatch("variant/field[@type='a']/trait[@name='one' and @value='1']", session);
			}
		}

		[Test]
		public void CustomMultiTextOnEntry()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var entry = session.CreateItem();

				var m = entry.GetOrCreateProperty<MultiText>("flubadub");
				m["zz"] = "orange";
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("entry/field[@type='flubadub']/form[@lang='zz' and text='orange']", session);
			}
		}

		[Test]
		public void CustomMultiTextOnExample()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var example = new LexExampleSentence();
				var m = example.GetOrCreateProperty<MultiText>("flubadub");
				m["zz"] = "orange";
				session.LiftWriter.Add(example);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("example/field[@type='flubadub']/form[@lang='zz' and text='orange']", session);
			}
		}

		[Test]
		public void CustomMultiTextOnSense()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				var m = sense.GetOrCreateProperty<MultiText>("flubadub");
				m["zz"] = "orange";
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense/field[@type='flubadub']/form[@lang='zz' and text='orange']", session);
			}
		}

		[Test]
		public void CustomOptionRefCollectionOnEntry()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				//_fieldToOptionListName.Add("flubs", "colors");
				LexEntry entry = session.CreateItem();

				var o = entry.GetOrCreateProperty<OptionRefCollection>("flubs");
				o.AddRange(new[] { "orange", "blue" });
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("entry/trait[@name='flubs' and @value='orange']", session);
				AssertHasAtLeastOneMatch("entry/trait[@name='flubs' and @value='blue']", session);
				AssertHasAtLeastOneMatch("entry[count(trait) =2]", session);
			}
		}

		[Test]
		public void CustomOptionRefCollectionOnExample()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				//_fieldToOptionListName.Add("flubs", "colors");
				var example = new LexExampleSentence();
				var o = example.GetOrCreateProperty<OptionRefCollection>("flubs");
				o.AddRange(new[] { "orange", "blue" });
				session.LiftWriter.Add(example);
				session.LiftWriter.End();
				string expected = CanonicalXml.ToCanonicalStringFragment(
					"<example><trait name=\"flubs\" value=\"orange\" /><trait name=\"flubs\" value=\"blue\" /></example>"
				);
				Assert.AreEqual(
					expected,
					session.StringBuilder.ToString()
				);
			}
		}

		[Test]
		public void CustomOptionRefCollectionOnSense()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				//_fieldToOptionListName.Add("flubs", "colors");
				var sense = new LexSense();
				var o = sense.GetOrCreateProperty<OptionRefCollection>("flubs");
				o.AddRange(new[] { "orange", "blue" });
				session.LiftWriter.Add(sense);
				string expected = CanonicalXml.ToCanonicalStringFragment(
					GetSenseElement(sense, "<trait name=\"flubs\" value=\"orange\" /><trait name=\"flubs\" value=\"blue\" />")
				);
				Assert.AreEqual(expected, session.OutputString());
			}
		}

		[Test]
		public void CustomOptionRefOnEntry()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				//_fieldToOptionListName.Add("flub", "kindsOfFlubs");
				LexEntry entry = session.CreateItem();

				var o = entry.GetOrCreateProperty<OptionRef>("flub");
				o.Value = "orange";
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch(
					"entry/trait[@name='flub' and @value='orange']",
					session
				);
			}
		}

		[Test]
		public void CustomOptionRefOnExample()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				//_fieldToOptionListName.Add("flub", "kindsOfFlubs");
				var example = new LexExampleSentence();
				var o = example.GetOrCreateProperty<OptionRef>("flub");
				o.Value = "orange";
				session.LiftWriter.Add(example);
				session.LiftWriter.End();
				string expected = CanonicalXml.ToCanonicalStringFragment(
					"<example><trait name=\"flub\" value=\"orange\" /></example>"
				);
				Assert.AreEqual(
					expected,
					session.StringBuilder.ToString()
				);
			}
		}

		[Test]
		public void CustomOptionRefOnSense()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				//_fieldToOptionListName.Add("flub", "kindsOfFlubs");
				var sense = new LexSense();
				var o = sense.GetOrCreateProperty<OptionRef>("flub");
				o.Value = "orange";
				session.LiftWriter.Add(sense);
				string expected = CanonicalXml.ToCanonicalStringFragment(
					GetSenseElement(sense, "<trait name=\"flub\" value=\"orange\" />")
				);
				Assert.AreEqual(expected, session.OutputString());
			}
		}

		[Test]
		public void CustomOptionRefOnSenseWithGrammi()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				//_fieldToOptionListName.Add("flub", "kindsOfFlubs");
				var sense = new LexSense();
				var grammi = sense.GetOrCreateProperty<OptionRef>(
					LexSense.WellKnownProperties.PartOfSpeech
				);
				grammi.Value = "verb";

				var o = sense.GetOrCreateProperty<OptionRef>("flub");
				o.Value = "orange";
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense/trait[@name='flub' and @value='orange']", session);
				AssertHasAtLeastOneMatch("sense[count(trait)=1]", session);
			}
		}

		[Test]
		public void DefinitionOnSense_OutputAsDefinition()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				var m = sense.GetOrCreateProperty<MultiText>(
					LexSense.WellKnownProperties.Definition
				);
				m["zz"] = "orange";
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense/definition/form[@lang='zz']/text[text()='orange']", session);
				AssertHasAtLeastOneMatch("sense[not(field)]", session);
			}
		}

		[Test]
		public void DeletedEntry()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var entry = new LexEntry();
				session.LiftWriter.AddDeletedEntry(entry);
				session.LiftWriter.End();
				Assert.IsNotNull(GetStringAttributeOfTopElement("dateDeleted", session));
			}
		}

		[Test]
		public void DocumentStart()
		{
			using (var session = new LiftExportAsFullDocumentTestSession())
			{
				//NOTE: the utf-16 here is an artifact of the xmlwriter when writing to a stringbuilder,
				//which is what we use for tests.  The file version puts out utf-8
				//CheckAnswer("<?xml version=\"1.0\" encoding=\"utf-16\"?><lift producer=\"WeSay.1Pt0Alpha\"/>");// xmlns:flex=\"http://fieldworks.sil.org\" />");
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch(string.Format("lift[@version='{0}']", Validator.LiftVersion), session);
				AssertHasAtLeastOneMatch(string.Format("lift[@producer='{0}']", LiftWriter.ProducerString), session);
			}
		}

		[Test]
		public void EmptyCustomMultiText()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				sense.GetOrCreateProperty<MultiText>("flubadub");
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense[not(field)]", session);
			}
		}

		[Test]
		public void EmptyCustomOptionRef()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				sense.GetOrCreateProperty<OptionRef>("flubadub");
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense[not(trait)]", session);
			}
		}

		[Test]
		public void EmptyCustomOptionRefCollection()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				sense.GetOrCreateProperty<OptionRefCollection>("flubadub");
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense[not(trait)]", session);
			}
		}

		[Test]
		public void EmptyDefinitionOnSense_NotOutput()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				sense.GetOrCreateProperty<MultiText>(LexSense.WellKnownProperties.Definition);
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense[not(definition)]", session);
				AssertHasAtLeastOneMatch("sense[not(field)]", session);
			}
		}

		[Test]
		public void EmptyExampleSource_NoAttribute()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var ex = new LexExampleSentence();
				ex.GetOrCreateProperty<OptionRef>(
					LexExampleSentence.WellKnownProperties.Source
				);
				session.LiftWriter.Add(ex);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("example[not(@source)]", session);
			}
		}

		[Test]
		public void EmptyNoteOnEntry_NoOutput()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				entry.GetOrCreateProperty<MultiText>(PalasoDataObject.WellKnownProperties.Note);
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("entry[not(note)]", session);
				AssertHasAtLeastOneMatch("entry[not(field)]", session);
			}
		}

		[Test]
		public void Entry_EntryHasIdWithInvalidXMLCharacters_CharactersEscaped()
		{
			const string expected = "id=\"&lt;&gt;&amp;&quot;'\"";
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var entry = session.CreateItem();
				// technically the only invalid characters in an attribute are & < and " (when surrounded by ")
				entry.Id = "<>&\"\'";
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				string result = session.OutputString();
				Assert.IsTrue(result.Contains(expected));
			}
		}

		[Test]
		public void Entry_ScaryUnicodeCharacter_SafeXmlEmitted()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				entry.LexicalForm["test"] = '\u001F'.ToString();
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();

				var doc = new XmlDocument();
				//this next line will crash if things aren't safe
				doc.LoadXml(session.StringBuilder.ToString());
			}
		}

		[Test]
		public void Entry_HasId_RemembersId()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				entry.Id = "my id";
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				string result = session.StringBuilder.ToString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath("//entry[@id='my id']");
			}
		}

		[Test]
		public void Entry_NoId_GetsHumanReadableId()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var entry = session.CreateItem();
				entry.LexicalForm["test"] = "lexicalForm";
				//_lexEntryRepository.SaveItem(entry);
				// make dateModified different than dateCreated
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();

				string expectedId =
					LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry);
				string result = session.StringBuilder.ToString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					string.Format("//entry[@id=\"{0}\"]", expectedId));
			}
		}

		[Test]
		public void EntryGuid()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				string result = session.StringBuilder.ToString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(String.Format("//entry[@guid=\"{0}\"]", entry.Guid));
			}
		}

		[Test]
		public void EmptyRelationNotOutput()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				entry.AddRelationTarget(LexEntry.WellKnownProperties.BaseForm, string.Empty);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				Assert.IsFalse(session.StringBuilder.ToString().Contains("relation"));
			}
		}

		[Test]
		public void EntryHasDateCreated()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				string result = session.StringBuilder.ToString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					String.Format("//entry[@dateCreated=\"{0}\"]", entry.CreationTime.ToString("yyyy-MM-ddTHH:mm:ssZ"))
				);
			}
		}

		[Test]
		public void EntryHasDateModified()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				entry.LexicalForm["test"] = "lexicalForm";
				// make dateModified different than dateCreated
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				string result = session.StringBuilder.ToString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					String.Format("//entry[@dateModified=\"{0}\"]", entry.ModificationTime.ToString("yyyy-MM-ddTHH:mm:ssZ"))
				);
			}
		}

		/// <summary>
		/// Regression: WS-34576, LT-20698
		/// </summary>
		[Test]
		public void Add_CultureUsesPeriodForTimeSeparator_DateAttributesOutputWithColon([Values("en-US", "de-DE")] string culture)
		{
			Thread.CurrentThread.CurrentCulture =
				new CultureInfo(culture) { DateTimeFormat = { TimeSeparator = "." } };

			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				entry.LexicalForm["test"] = "lexicalForm";
				// make dateModified different than dateCreated
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				string result = session.StringBuilder.ToString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					$"//entry[@dateModified=\"{entry.ModificationTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}\"]"
				);
				Assert.IsTrue(result.Contains(":"), "should contain colons");
				Assert.IsFalse(result.Contains("."), "should not contain periods");
			}
		}

		[Test]
		public void EntryWithSenses()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				entry.LexicalForm["blue"] = "ocean";
				LexSense sense1 = new LexSense();
				sense1.Gloss["a"] = "aaa";
				entry.Senses.Add(sense1);
				LexSense sense2 = new LexSense();
				sense2.Gloss["b"] = "bbb";
				entry.Senses.Add(sense2);
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				string result = session.StringBuilder.ToString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath("//entry/sense/gloss[@lang='a']/text[text()='aaa']");
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath("//entry/sense/gloss[@lang='b']/text[text()='bbb']");
				AssertHasAtLeastOneMatch("entry[count(sense)=2]", session);
			}
		}

		[Test]
		public void ExampleSentence()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var example = new LexExampleSentence();
				example.Sentence["blue"] = "ocean's eleven";
				example.Sentence["red"] = "red sunset tonight";
				session.LiftWriter.Add(example);
				AssertEqualsCanonicalString(
					"<example><form lang=\"blue\"><text>ocean's eleven</text></form><form lang=\"red\"><text>red sunset tonight</text></form></example>",
					session.OutputString()
				);
			}
		}

		[Test]
		public void ExampleSentenceWithTranslation()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var example = new LexExampleSentence();
				example.Sentence["blue"] = "ocean's eleven";
				example.Sentence["red"] = "red sunset tonight";
				example.Translation["green"] = "blah blah";
				session.LiftWriter.Add(example);
				var outPut = session.OutputString();
				AssertEqualsCanonicalString(
					"<example><form lang=\"blue\"><text>ocean's eleven</text></form><form lang=\"red\"><text>red sunset tonight</text></form><translation><form lang=\"green\"><text>blah blah</text></form></translation></example>", outPut);
			}
		}

		[Test]
		public void ExampleSourceAsAttribute()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexExampleSentence ex = new LexExampleSentence();
				OptionRef z = ex.GetOrCreateProperty<OptionRef>(
					LexExampleSentence.WellKnownProperties.Source
				);
				z.Value = "hearsay";

				session.LiftWriter.Add(ex);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("example[@source='hearsay']", session);
			}
		}

		[Test]
		public void FlagCleared_NoOutput()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();

				entry.SetFlag("ATestFlag");
				entry.ClearFlag("ATestFlag");
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("entry[not(trait)]", session);
			}
		}

		[Test]
		public void FlagOnEntry_OutputAsTrait()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();

				entry.SetFlag("ATestFlag");
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("entry/trait[@name='ATestFlag' and @value]", session);
			}
		}

		[Test]
		public void GetHumanReadableId_EntryHasId_GivesId()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				entry.Id = "my id";
				Assert.That(
					LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry),
					Is.EqualTo("my id")
				);
			}
		}

		[Test]
		public void GetHumanReadableId_IdIsSpace_NoForm()
		{
			var entry = new LexEntry(" ", Guid.NewGuid());
			Assert.That(
				LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry),
				Does.StartWith("Id'dPrematurely_")
			);
		}

		[Test]
		public void GetHumanReadableId_IdIsSpace_TreatedAsThoughNonExistentId()
		{
			var entry = new LexEntry(" ", Guid.NewGuid());
			entry.LexicalForm["green"] = "string";
			Assert.That(
				LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry),
				Does.StartWith("string")
			);
		}

		[Test]
		public void Gloss()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				sense.Gloss["blue"] = "ocean";
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch(
					"sense/gloss[@lang='blue']/text[text()='ocean']",
					session
				);
			}
		}

		[Test] // Ummmm no it shouldn't CP 2013-05.  Flex expects the opposite of this.
		public void Gloss_MultipleGlossesSplitIntoSeparateEntries()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				sense.Gloss["a"] = "aaa; bbb; ccc";
				sense.Gloss["x"] = "xx";
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense[count(gloss)=2]", session);
				AssertHasAtLeastOneMatch("sense/gloss[@lang='a' and text='aaa; bbb; ccc']", session);
				AssertHasAtLeastOneMatch("sense/gloss[@lang='x' and text='xx']", session);
			}
		}

		[Test]
		public void GlossWithProblematicCharacters()
		{
			const string expected = "<text>LessThan&lt;GreaterThan&gt;Ampersan&amp;</text>";
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				sense.Gloss["blue"] = "LessThan<GreaterThan>Ampersan&";
				session.LiftWriter.Add(sense);
				string result = session.OutputString();
				Assert.IsTrue(result.Contains(expected));
			}
		}

		[Test]
		public void GlossWithStarredForm()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexSense sense = new LexSense();
				sense.Gloss.SetAlternative("x", "orange");
				sense.Gloss.SetAnnotationOfAlternativeIsStarred("x", true);
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense/gloss[@lang='x']/annotation[@name='flag' and @value='1']", session);
			}
		}

		[Test]
		public void Grammi()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				var o = sense.GetOrCreateProperty<OptionRef>(
					LexSense.WellKnownProperties.PartOfSpeech
				);
				o.Value = "orange";
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense/grammatical-info[@value='orange']", session);
				AssertHasAtLeastOneMatch("sense[not(trait)]", session);
			}
		}

		[Test]
		public void GrammiWithStarredForm()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexSense sense = new LexSense();
				OptionRef o = sense.GetOrCreateProperty<OptionRef>(
					LexSense.WellKnownProperties.PartOfSpeech
				);
				o.Value = "orange";
				o.IsStarred = true;
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch(
					"sense/grammatical-info[@value='orange']/annotation[@name='flag' and @value='1']",
					session
				);
			}
		}

		[Test]
		public void LexemeForm_SingleWritingSystem()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry e = session.CreateItem();
				e.LexicalForm["xx"] = "foo";
				//_lexEntryRepository.SaveItem(e);
				session.LiftWriter.Add(e);
				session.LiftWriter.End();

				AssertHasAtLeastOneMatch("//lexical-unit/form[@lang='xx']", session);
			}
		}

		[Test]
		public void LexEntry_becomes_entry()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();

				Assert.IsTrue(session.StringBuilder.ToString().StartsWith("<entry"));
			}
		}

		[Test]
		public void LexicalUnit()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry e = session.CreateItem();
				e.LexicalForm.SetAlternative("x", "orange");
				//_lexEntryRepository.SaveItem(e);
				session.LiftWriter.Add(e);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("entry/lexical-unit/form[@lang='x']/text[text()='orange']", session);
				AssertHasAtLeastOneMatch("entry/lexical-unit/form[@lang='x'][not(trait)]", session);
			}
		}

		[Test]
		public void LexicalUnitWithStarredForm()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry e = session.CreateItem();

				e.LexicalForm.SetAlternative("x", "orange");
				e.LexicalForm.SetAnnotationOfAlternativeIsStarred("x", true);
				//_lexEntryRepository.SaveItem(e);
				session.LiftWriter.Add(e);
				session.LiftWriter.End();

				AssertHasAtLeastOneMatch(
					"entry/lexical-unit/form[@lang='x']/annotation[@name='flag' and @value='1']",
					session
				);
			}
		}

		[Test]
		public void LexSense_becomes_sense()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				Assert.IsTrue(session.StringBuilder.ToString().StartsWith("<sense"));
			}
		}

		[Test]
		public void MultiText()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var text = new MultiText();
				text["blue"] = "ocean";
				text["red"] = "sunset";
				session.LiftWriter.AddMultitextForms(null, text);
				AssertEqualsCanonicalString(
					"<form lang=\"blue\"><text>ocean</text></form><form lang=\"red\"><text>sunset</text></form>",
					session.OutputString()
				);
			}
		}

		[Test]
		public void NoteOnEntry_OutputAsNote()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();

				MultiText m =
					entry.GetOrCreateProperty<MultiText>(PalasoDataObject.WellKnownProperties.Note);
				m["zz"] = "orange";
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("entry/note/form[@lang='zz' and text='orange']", session);
				AssertHasAtLeastOneMatch("entry[not(field)]", session);
			}
		}

		[Test]
		public void NoteOnExample_OutputAsNote()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexExampleSentence example = new LexExampleSentence();
				MultiText m =
					example.GetOrCreateProperty<MultiText>(PalasoDataObject.WellKnownProperties.Note);
				m["zz"] = "orange";
				session.LiftWriter.Add(example);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("example/note/form[@lang='zz' and text='orange']", session);
				AssertHasAtLeastOneMatch("example[not(field)]", session);
			}
		}

		[Test]
		public void NoteOnSense_OutputAsNote()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexSense sense = new LexSense();
				MultiText m =
					sense.GetOrCreateProperty<MultiText>(PalasoDataObject.WellKnownProperties.Note);
				m["zz"] = "orange";
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("sense/note/form[@lang='zz' and text='orange']", session);
				AssertHasAtLeastOneMatch("sense[not(field)]", session);
			}
		}

		[Test]
		public void Picture_OutputAsPictureURLRef()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexSense sense = new LexSense();
				PictureRef p = sense.GetOrCreateProperty<PictureRef>("Picture");
				p.Value = "bird.jpg";
				session.LiftWriter.Add(sense);
				AssertEqualsCanonicalString(GetSenseElement(sense, "<illustration href=\"bird.jpg\" />"), session.OutputString());
			}
		}

		[Test]
		public void Picture_OutputAsPictureWithCaption()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexSense sense = new LexSense();
				PictureRef p = sense.GetOrCreateProperty<PictureRef>("Picture");
				p.Value = "bird.jpg";
				p.Caption = new MultiText();
				p.Caption["aa"] = "aCaption";
				session.LiftWriter.Add(sense);
				AssertEqualsCanonicalString(
					GetSenseElement(sense, "<illustration href=\"bird.jpg\"><label><form lang=\"aa\"><text>aCaption</text></form></label></illustration>"),
					session.OutputString()
				);
			}
		}

		[Test]
		public void Sense_HasId_RemembersId()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexSense s = new LexSense();
				s.Id = "my id";
				session.LiftWriter.Add(s);
				session.LiftWriter.End();
				string result = session.StringBuilder.ToString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath("//sense[@id='my id']");
			}
		}

		[Test]
		public void Sense_NoId_GetsId()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexSense sense = new LexSense();
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				string result = session.StringBuilder.ToString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					String.Format("//sense[@id='{0}']", sense.Id)
				);
			}
		}

		[Test]
		public void SensesAreLastObjectsInEntry() // this helps conversions to sfm review: It would be great if this wasn't necessary CP 2011-01
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();

				entry.LexicalForm["blue"] = "ocean";

				LexSense sense1 = new LexSense();
				sense1.Gloss["a"] = "aaa";
				entry.Senses.Add(sense1);
				LexSense sense2 = new LexSense();
				sense2.Gloss["b"] = "bbb";
				entry.Senses.Add(sense2);

				MultiText citation =
					entry.GetOrCreateProperty<MultiText>(LexEntry.WellKnownProperties.Citation);
				citation["zz"] = "orange";

				MultiText note =
					entry.GetOrCreateProperty<MultiText>(PalasoDataObject.WellKnownProperties.Note);
				note["zz"] = "orange";

				MultiText field = entry.GetOrCreateProperty<MultiText>("custom");
				field["zz"] = "orange";

				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				string result = session.StringBuilder.ToString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath("//gloss[@lang='a']/text[text()='aaa']");
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath("//gloss[@lang='b']/text[text()='bbb']");
				AssertThatXmlIn.String(result).HasNoMatchForXpath("/entry/sense[2]/following-sibling::*");
			}
		}

		[Test]
		public void SenseWithExample()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				var example = new LexExampleSentence();
				example.Sentence["red"] = "red sunset tonight";
				sense.ExampleSentences.Add(example);
				session.LiftWriter.Add(sense);
				string result = session.OutputString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					"/sense/example/form[@lang='red']/text[text()='red sunset tonight']"
				);
			}
		}

		[Test]
		public void SenseWithSynonymRelations()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();

				var synonymRelationType = new LexRelationType(
					"synonym",
					LexRelationType.Multiplicities.Many,
					LexRelationType.TargetTypes.Sense
				);

				var antonymRelationType = new LexRelationType(
					"antonym",
					LexRelationType.Multiplicities.Many,
					LexRelationType.TargetTypes.Sense
				);

				var relations = new LexRelationCollection();
				sense.Properties.Add(new KeyValuePair<string, IPalasoDataObjectProperty>("relations", relations));

				relations.Relations.Add(new LexRelation(synonymRelationType.ID, "one", sense));
				relations.Relations.Add(new LexRelation(synonymRelationType.ID, "two", sense));
				relations.Relations.Add(new LexRelation(antonymRelationType.ID, "bee", sense));

				session.LiftWriter.Add(sense);
				string result = session.OutputString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					"/sense/relation[@type='synonym' and @ref='one']"
				);
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					"/sense/relation[@type='synonym' and @ref='two']"
				);
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					"/sense/relation[@type='antonym' and @ref='bee']"
				);
			}
		}

		[Test]
		public void AddRelationTarget_SenseWithSynonymRelations()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				sense.AddRelationTarget("synonym", "one");
				sense.AddRelationTarget("synonym", "two");
				sense.AddRelationTarget("antonym", "bee");

				session.LiftWriter.Add(sense);
				string result = session.OutputString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					"/sense/relation[@type='synonym' and @ref='one']"
				);
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					"/sense/relation[@type='synonym' and @ref='two']"
				);
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
					"/sense/relation[@type='antonym' and @ref='bee']"
				);
			}
		}

		[Test]
		public void SenseWithRelationWithEmbeddedXml()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();

				var synonymRelationType = new LexRelationType(
					"synonym",
					LexRelationType.Multiplicities.Many,
					LexRelationType.TargetTypes.Sense
				);

				var relations = new LexRelationCollection();
				sense.Properties.Add(new KeyValuePair<string, IPalasoDataObjectProperty>("relations", relations));

				var lexRelation = new LexRelation(synonymRelationType.ID, "one", sense);
				lexRelation.EmbeddedXmlElements.Add("<trait name='x' value='X'/>");
				lexRelation.EmbeddedXmlElements.Add("<field id='z'><text>hello</text></field>");
				relations.Relations.Add(lexRelation);

				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertThatXmlIn.String(session.StringBuilder.ToString())
					.HasSpecifiedNumberOfMatchesForXpath("//sense/relation/trait", 1);
				AssertThatXmlIn.String(session.StringBuilder.ToString())
					.HasSpecifiedNumberOfMatchesForXpath("//sense/relation/field", 1);
			}
		}

		[Test]
		public void WriteToFile()
		{
			using (var session = new LiftExportAsFileTestSession())
			{
				session.AddTwoTestLexEntries();
				var doc = new XmlDocument();
				doc.Load(session.FilePath);
				Assert.AreEqual(2, doc.SelectNodes("lift/entry").Count);
			}
		}

		[Test]
		public void Add_MultiTextWithWellFormedXML_IsExportedAsXML()
		{
			const string expected =
				"<form\r\n\tlang=\"de\">\r\n\t<text>This <span href=\"reference\">is well formed</span> XML!</text>\r\n</form>";
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var multiText = new MultiText();
				multiText.SetAlternative("de", "This <span href=\"reference\">is well formed</span> XML!");
				session.LiftWriter.AddMultitextForms(null, multiText);
				session.LiftWriter.End();
				Assert.AreEqual(expected, session.OutputString());
			}
		}

		[Test]
		public void Add_MultiTextWithWellFormedXMLAndScaryCharacter_IsExportedAsXML()
		{
			const string expected =
			"<form\r\n\tlang=\"de\">\r\n\t<text>This <span href=\"reference\">is well &#x1F; formed</span> XML!</text>\r\n</form>";
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var multiText = new MultiText();
				multiText.SetAlternative("de", "This <span href=\"reference\">is well \u001F formed</span> XML!");
				session.LiftWriter.AddMultitextForms(null, multiText);
				session.LiftWriter.End();
				Assert.AreEqual(expected, session.OutputString());
			}
		}

		[Test]
		public void Add_MultiTextWithScaryUnicodeChar_IsExported()
		{
			const string expected =
				"<form\r\n\tlang=\"de\">\r\n\t<text>This has a segment separator character at the end&#x1F;</text>\r\n</form>";
			//  1F is the character for "Segment Separator" and you can insert it by right-clicking in windows
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var multiText = new MultiText();
				multiText.SetAlternative("de", "This has a segment separator character at the end\u001F");
				session.LiftWriter.AddMultitextForms(null, multiText);
				session.LiftWriter.End();
				Assert.AreEqual(expected, session.OutputString());
			}
		}

		[Test]
		public void Add_MalformedXmlWithWithScaryUnicodeChar_IsExportedAsText()
		{
			const string expected = "<form\r\n\tlang=\"de\">\r\n\t<text>This &lt;span href=\"reference\"&gt;is not well &#x1F; formed&lt;span&gt; XML!</text>\r\n</form>";
			//  1F is the character for "Segment Separator" and you can insert it by right-clicking in windows
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var multiText = new MultiText();
				multiText.SetAlternative("de", "This <span href=\"reference\">is not well \u001F formed<span> XML!");
				session.LiftWriter.AddMultitextForms(null, multiText);
				session.LiftWriter.End();
				Assert.AreEqual(expected, session.OutputString());
			}
		}

		[Test]
		public void Add_MultiTextWithMalFormedXML_IsExportedText()
		{
			const string expected =
				"<form\r\n\tlang=\"de\">\r\n\t<text>This &lt;span href=\"reference\"&gt;is not well formed&lt;span&gt; XML!</text>\r\n</form>";
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var multiText = new MultiText();
				multiText.SetAlternative("de", "This <span href=\"reference\">is not well formed<span> XML!");
				session.LiftWriter.AddMultitextForms(null, multiText);
				session.LiftWriter.End();
				Assert.AreEqual(expected, session.OutputString());
			}
		}

		[Test]
		public void Add_TextWithSpanAndMeaningfulWhiteSpace_FormattingAndWhitespaceIsUntouched()
		{
			// REVIEW (EberhardB): does it really make sense to preserve the line endings? It seems
			// that for a text node line endings should be standardized.
			const string formattedText = "\rThis's <span href=\"reference\">\n is a\t\t\n\r\t span</span> with annoying whitespace!\r\n";
			const string expected = "<form\r\n\tlang=\"de\">\r\n\t<text>" + formattedText + "</text>\r\n</form>";
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var multiText = new MultiText();
				multiText.SetAlternative("de", formattedText);
				session.LiftWriter.AddMultitextForms(null, multiText);
				session.LiftWriter.End();
				Assert.AreEqual(expected, session.OutputString());
			}
		}
	}
}
