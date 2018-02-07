using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Palaso.DictionaryServices.Model;
using Palaso.Lift.Validation;
using Palaso.TestUtilities;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.DictionaryServices.Lift;

using NUnit.Framework;

namespace Palaso.DictionaryServices.Tests.Lift
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
			AssertThatXmlIn.String(session.StringBuilder.ToString()).
				HasSpecifiedNumberOfMatchesForXpath(xpath,1);
		}

		private static void AssertHasAtLeastOneMatch(string xpath, LiftExportTestSessionBase session)
		{
			AssertThatXmlIn.String(session.StringBuilder.ToString()).HasAtLeastOneMatchForXpath(xpath);
		}

		[Obsolete("Use AssertThatXmlInXPath.String(...)")] // CP 2011-01
		private static string GetSenseElement(LexSense sense)
		{
			return string.Format("<sense id=\"{0}\">", sense.GetOrCreateId());
		}

		private static string GetStringAttributeOfTopElement(string attribute, LiftExportTestSessionBase session)
		{
			var doc = new XmlDocument();
			doc.LoadXml(session.StringBuilder.ToString());
			return doc.FirstChild.Attributes[attribute].ToString();
		}

		[Obsolete("Use AssertEqualsCanonicalString(string, string) instead")]
		private static void AssertEqualsCanonicalString(string expected, LiftExportTestSessionBase session)
		{
			string canonicalAnswer = CanonicalXml.ToCanonicalStringFragment(expected);
			Assert.AreEqual(canonicalAnswer, session.StringBuilder.ToString());
		}

		private static void AssertEqualsCanonicalString(string expected, string actual)
		{
			string canonicalAnswer = CanonicalXml.ToCanonicalStringFragment(expected);
			Assert.AreEqual(canonicalAnswer, actual);
		}

		[Test]
		public void LiftWriter_AddUsingWholeList_TwoEntries_HasTwoEntries()
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
		public void LiftWriter_AttributesWithProblematicCharacters()
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
		public void LiftWriter_BlankExample()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				session.LiftWriter.Add(new LexExampleSentence());
				AssertEqualsCanonicalString("<example />", session.OutputString());
			}
		}

		[Test]
		public void LiftWriter_BlankGrammi()
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
		public void LiftWriter_BlankMultiText()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				session.LiftWriter.AddMultitextForms(null, new MultiText());
				AssertEqualsCanonicalString("", session);
			}
		}

		[Test]
		public void LiftWriter_BlankSense()
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
		public void LiftWriter_Citation()
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
		public void LiftWriter_CitationWithStarredForm()
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
		public void LiftWriter_EntryWith2SimpleVariants()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var e = session.CreateItem();
				var variant = new LexVariant();
				variant.SetAlternative("etr","one");
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
		public void LiftWriter_EntryWithSimpleEtymology()
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
		public void LiftWriter_EntryWithFullEtymology()
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
		public void LiftWriter_EntryWithBorrowedWord()
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
		public void LiftWriter_EntryWithSimplePronunciation()
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
		public void LiftWriter_EntryWithRelationToDiacriticWord()
		{
			// WS-356 relation ref in lift file was NFD so it couldn't find the referenced entry
			using (var session = new LiftExportAsFullDocumentTestSession())
			{
				LexEntry diacritic_entry = session.CreateItem();
				diacritic_entry.LexicalForm["test"] = "më"; // NFC entry
				session.LiftWriter.Add(diacritic_entry);

				LexEntry entry_with_relation = session.CreateItem();
				entry_with_relation.LexicalForm["test"] = "men";
				entry_with_relation.AddRelationTarget("confer", diacritic_entry.Id);  // NFD id
				session.LiftWriter.Add(entry_with_relation);

				session.LiftWriter.End();
				// assert that the relation ref is NFC
				AssertHasOneMatch("lift/entry/relation[@ref='më_" + diacritic_entry.Guid + "']", session);
			}
		}

		[Test]
		public void LiftWriter_SenseWith2Notes()
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
		public void LiftWriter_SenseWith2Reversals()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var sense = new LexSense();
				var reversal = new LexReversal {Type = "revType"};
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
		public void LiftWriter_EntryWithTypedNote()
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
		public void LiftWriter_VariantWith2Traits()
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
		public void LiftWriter_VariantWith2SimpleFields()
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
		public void LiftWriter_FieldWithTraits()
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
		public void LiftWriter_CustomMultiTextOnEntry()
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
		public void LiftWriter_CustomMultiTextOnExample()
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
		public void LiftWriter_CustomMultiTextOnSense()
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
		public void LiftWriter_CustomOptionRefCollectionOnEntry()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				//_fieldToOptionListName.Add("flubs", "colors");
				LexEntry entry = session.CreateItem();

				var o = entry.GetOrCreateProperty<OptionRefCollection>("flubs");
				o.AddRange(new[] {"orange", "blue"});
				//_lexEntryRepository.SaveItem(entry);
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();
				AssertHasAtLeastOneMatch("entry/trait[@name='flubs' and @value='orange']", session);
				AssertHasAtLeastOneMatch("entry/trait[@name='flubs' and @value='blue']", session);
				AssertHasAtLeastOneMatch("entry[count(trait) =2]", session);
			}
		}

		[Test]
		public void LiftWriter_CustomOptionRefCollectionOnExample()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				//_fieldToOptionListName.Add("flubs", "colors");
				var example = new LexExampleSentence();
				var o = example.GetOrCreateProperty<OptionRefCollection>("flubs");
				o.AddRange(new[] {"orange", "blue"});
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
		public void LiftWriter_CustomOptionRefCollectionOnSense()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				//_fieldToOptionListName.Add("flubs", "colors");
				var sense = new LexSense();
				var o = sense.GetOrCreateProperty<OptionRefCollection>("flubs");
				o.AddRange(new[] {"orange", "blue"});
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				string expected = CanonicalXml.ToCanonicalStringFragment(
					GetSenseElement(sense) +
					"<trait name=\"flubs\" value=\"orange\" /><trait name=\"flubs\" value=\"blue\" /></sense>"
				);
				Assert.AreEqual(
					expected,
					session.StringBuilder.ToString());
			}
		}

		[Test]
		public void LiftWriter_CustomOptionRefOnEntry()
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
		public void LiftWriter_CustomOptionRefOnExample()
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
		public void LiftWriter_CustomOptionRefOnSense()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				//_fieldToOptionListName.Add("flub", "kindsOfFlubs");
				var sense = new LexSense();
				var o = sense.GetOrCreateProperty<OptionRef>("flub");
				o.Value = "orange";
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				string expected = CanonicalXml.ToCanonicalStringFragment(
					GetSenseElement(sense) + "<trait name=\"flub\" value=\"orange\" /></sense>"
				);
				Assert.AreEqual(
					expected,
					session.StringBuilder.ToString()
				);
			}
		}

		[Test]
		public void LiftWriter_CustomOptionRefOnSenseWithGrammi()
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
		public void LiftWriter_DefinitionOnSense_OutputAsDefinition()
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
		public void LiftWriter_DeletedEntry()
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
		public void LiftWriter_DocumentStart()
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
		public void LiftWriter_EmptyCustomMultiText()
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
		public void LiftWriter_EmptyCustomOptionRef()
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
		public void LiftWriter_EmptyCustomOptionRefCollection()
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
		public void LiftWriter_EmptyDefinitionOnSense_NotOutput()
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
		public void LiftWriter_EmptyExampleSource_NoAttribute()
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
		public void LiftWriter_EmptyNoteOnEntry_NoOutput()
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
		public void LiftWriter_Entry_EntryHasIdWithInvalidXMLCharacters_CharactersEscaped()
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
		public void LiftWriter_Entry_ScaryUnicodeCharacter_SafeXmlEmitted()
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
		public void LiftWriter_Entry_HasId_RemembersId()
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
		public void LiftWriter_Entry_NoId_GetsHumanReadableId()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				var entry = session.CreateItem();
				entry.LexicalForm["test"] = "lexicalForm";
				//_lexEntryRepository.SaveItem(entry);
				// make dateModified different than dateCreated
				session.LiftWriter.Add(entry);
				session.LiftWriter.End();

				string expectedId = LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(
					entry, new Dictionary<string, int>()
				);
				string result = session.StringBuilder.ToString();
				AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(String.Format("//entry[@id=\"{0}\"]", expectedId));
			}
		}

		[Test]
		public void LiftWriter_EntryGuid()
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
		public void LiftWriter_EmptyRelationNotOutput()
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
		public void LiftWriter_EntryHasDateCreated()
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
		public void LiftWriter_EntryHasDateModified()
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
		/// Regression: WS-34576
		/// </summary>
		[Test]
		public void LiftWriter_Add_CultureUsesPeriodForTimeSeparator_DateAttributesOutputWithColon()
		{
			var culture = new CultureInfo("en-US");
			culture.DateTimeFormat.TimeSeparator = ".";

			Thread.CurrentThread.CurrentCulture = culture;

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
					String.Format("//entry[@dateModified=\"{0}\"]",
								  entry.ModificationTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture))
					);
				Assert.IsTrue(result.Contains(":"), "should contain colons");
				Assert.IsFalse(result.Contains("."), "should not contain periods");
			}
		}

		[Test]
		public void LiftWriter_EntryWithSenses()
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
		public void LiftWriter_ExampleSentence()
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
		public void LiftWriter_ExampleSentenceWithTranslation()
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
		public void LiftWriter_ExampleSourceAsAttribute()
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
		public void LiftWriter_FlagCleared_NoOutput()
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
		public void LiftWriter_FlagOnEntry_OutputAsTrait()
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

		/* this is not relevant, as we are currently using form_guid as the id
		[Test]
		public void LiftWriter_DuplicateFormsGetHomographNumbers()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["blue"] = "ocean";
			session.LiftWriter.Add(entry);
			session.LiftWriter.Add(entry);
			session.LiftWriter.Add(entry);
		  session.LiftWriter.End();
		  Assert.IsTrue(_stringBuilder.ToString().Contains("\"ocean\""), "ocean not contained in {0}", _stringBuilder.ToString());
		  Assert.IsTrue(_stringBuilder.ToString().Contains("ocean_2"), "ocean_2 not contained in {0}", _stringBuilder.ToString());
		  Assert.IsTrue(_stringBuilder.ToString().Contains("ocean_3"), "ocean_3 not contained in {0}", _stringBuilder.ToString());
		}
		*/

		[Test]
		public void LiftWriter_GetHumanReadableId_EntryHasId_GivesId()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexEntry entry = session.CreateItem();
				entry.Id = "my id";
				//_lexEntryRepository.SaveItem(entry);
				Assert.AreEqual(
					"my id",
					LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, new Dictionary<string, int>())
				);
			}
		}

		/* this tests a particular implementation detail (idCounts), which isn't used anymore:
		[Test]
		public void GetHumanReadableId_EntryHasId_RegistersId()
		{
			LexEntry entry = new LexEntry("my id", Guid.NewGuid());
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts);
			Assert.AreEqual(1, idCounts["my id"]);
		}
		*/

		/* this is not relevant, as we are currently using form_guid as the id
		[Test]
		public void GetHumanReadableId_EntryHasAlreadyUsedId_GivesIncrementedId()
		{
			LexEntry entry = new LexEntry("my id", Guid.NewGuid());
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts);
			Assert.AreEqual("my id_2", LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts));
		}
		*/
		/* this is not relevant, as we are currently using form_guid as the id
		[Test]
		public void GetHumanReadableId_EntryHasAlreadyUsedId_IncrementsIdCount()
		{
			LexEntry entry = new LexEntry("my id", Guid.NewGuid());
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts);
			LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts);
			Assert.AreEqual(2, idCounts["my id"]);
		}
		*/
		/* this is not relevant, as we are currently using form_guid as the id
		[Test]
		public void GetHumanReadableId_EntryHasNoIdAndNoLexicalForms_GivesDefaultId()
		{
			LexEntry entry = new LexEntry();
			Assert.AreEqual("NoForm", LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, new Dictionary<string, int>()));
		}
		*/

		/*      this is not currently relevant, as we are now using form_guid as the id
		[Test]
		public void GetHumanReadableId_EntryHasNoIdAndNoLexicalFormsButAlreadyUsedId_GivesIncrementedDefaultId()
		{
			LexEntry entry = new LexEntry();
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts);
			Assert.AreEqual("NoForm_2", LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts));
		}
		*/

		/*      this is not currently relevant, as we are now using form_guid as the id
		[Test]
		public void GetHumanReadableId_EntryHasNoId_GivesIdMadeFromFirstLexicalForm()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["green"] = "grass";
			entry.LexicalForm["blue"] = "ocean";

			Assert.AreEqual("grass", LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, new Dictionary<string, int>()));
		}
		*/

		/*      this is not currently relevant, as we are now using form_guid as the id

		[Test]
		public void GetHumanReadableId_EntryHasNoId_RegistersIdMadeFromFirstLexicalForm()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["green"] = "grass";
			entry.LexicalForm["blue"] = "ocean";
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts);
			Assert.AreEqual(1, idCounts["grass"]);
		}
		*/
		/*      this is not currently relevant, as we are now using form_guid as the id
		[Test]
		public void GetHumanReadableId_EntryHasNoIdAndIsSameAsAlreadyEncountered_GivesIncrementedId()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["green"] = "grass";
			entry.LexicalForm["blue"] = "ocean";
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts);
			Assert.AreEqual("grass_2", LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts));
		}
		*/
		/*      this is not currently relevant, as we are now using form_guid as the id
		[Test]
		public void GetHumanReadableId_EntryHasNoIdAndIsSameAsAlreadyEncountered_IncrementsIdCount()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["green"] = "grass";
			entry.LexicalForm["blue"] = "ocean";
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts);
			LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, idCounts);
			Assert.AreEqual(2, idCounts["grass"]);
		}
		*/
		/*      this is not currently relevant, as we are now using form_guid as the id
		[Test]
		public void GetHumanReadableId_IdsDifferByWhiteSpaceTypeOnly_WhitespaceTreatedAsSpaces()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["green"] = "string\t1\n2\r3 4";
			Assert.AreEqual("string 1 2 3 4", LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, new Dictionary<string, int>()));
		}
		*/

		[Test]
		public void LiftWriter_GetHumanReadableId_IdIsSpace_NoForm()
		{
			var entry = new LexEntry(" ", Guid.NewGuid());
			Assert.IsTrue(
				LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(
					entry, new Dictionary<string, int>()
				).StartsWith("Id'dPrematurely_")
			);
		}

		[Test]
		public void LiftWriter_GetHumanReadableId_IdIsSpace_TreatedAsThoughNonExistentId()
		{
			var entry = new LexEntry(" ", Guid.NewGuid());
			entry.LexicalForm["green"] = "string";
			Assert.IsTrue(
				LiftWriter.GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry, new Dictionary<string, int>()).StartsWith
					("string"));
		}

		[Test]
		public void LiftWriter_Gloss()
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
		public void LiftWriter_Gloss_MultipleGlossesSplitIntoSeparateEntries()
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
		public void LiftWriter_GlossWithProblematicCharacters()
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
		public void LiftWriter_GlossWithStarredForm()
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
		public void LiftWriter_Grammi()
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
		public void LiftWriter_GrammiWithStarredForm()
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
		public void LiftWriter_LexemeForm_SingleWritingSystem()
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
		public void LiftWriter_LexEntry_becomes_entry()
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
		public void LiftWriter_LexicalUnit()
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
		public void LiftWriter_LexicalUnitWithStarredForm()
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
		public void LiftWriter_LexSense_becomes_sense()
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
		public void LiftWriter_MultiText()
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
		public void LiftWriter_NoteOnEntry_OutputAsNote()
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
		public void LiftWriter_NoteOnExample_OutputAsNote()
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
		public void LiftWriter_NoteOnSense_OutputAsNote()
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
		public void LiftWriter_Picture_OutputAsPictureURLRef()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexSense sense = new LexSense();
				PictureRef p = sense.GetOrCreateProperty<PictureRef>("Picture");
				p.Value = "bird.jpg";
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertEqualsCanonicalString(GetSenseElement(sense) + "<illustration href=\"bird.jpg\" /></sense>", session);
			}
		}

		[Test]
		public void LiftWriter_Picture_OutputAsPictureWithCaption()
		{
			using (var session = new LiftExportAsFragmentTestSession())
			{
				LexSense sense = new LexSense();
				PictureRef p = sense.GetOrCreateProperty<PictureRef>("Picture");
				p.Value = "bird.jpg";
				p.Caption = new MultiText();
				p.Caption["aa"] = "aCaption";
				session.LiftWriter.Add(sense);
				session.LiftWriter.End();
				AssertEqualsCanonicalString(
					GetSenseElement(sense) +
						"<illustration href=\"bird.jpg\"><label><form lang=\"aa\"><text>aCaption</text></form></label></illustration></sense>",
					session
				);
			}
		}

		[Test]
		public void LiftWriter_Sense_HasId_RemembersId()
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
		public void LiftWriter_Sense_NoId_GetsId()
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
		public void LiftWriter_SensesAreLastObjectsInEntry() // this helps conversions to sfm review: It would be great if this wasn't necessary CP 2011-01
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
		public void LiftWriter_SenseWithExample()
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
		public void LiftWriter_SenseWithSynonymRelations()
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
		public void LiftWriter_AddRelationTarget_SenseWithSynonymRelations()
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
		public void LiftWriter_SenseWithRelationWithEmbeddedXml()
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
				AssertThatXmlIn.String(session.StringBuilder.ToString()).HasSpecifiedNumberOfMatchesForXpath("//sense/relation/trait",1);
				AssertThatXmlIn.String(session.StringBuilder.ToString()).HasSpecifiedNumberOfMatchesForXpath("//sense/relation/field", 1);
			}
		}

		[Test]
		public void LiftWriter_WriteToFile()
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
		public void LiftWriter_Add_MultiTextWithWellFormedXML_IsExportedAsXML()
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
		public void LiftWriter_Add_MultiTextWithWellFormedXMLAndScaryCharacter_IsExportedAsXML()
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
		public void LiftWriter_Add_MultiTextWithScaryUnicodeChar_IsExported()
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
		public void LiftWriter_Add_MalformedXmlWithWithScaryUnicodeChar_IsExportedAsText()
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
		public void LiftWriter_Add_MultiTextWithMalFormedXML_IsExportedText()
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
		public void LiftWriter_Add_TextWithSpanAndMeaningfulWhiteSpace_FormattingAndWhitespaceIsUntouched()
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