using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NMock2;
using NUnit.Framework;
using SIL.IO;
using SIL.Lift.Merging;
using SIL.Lift.Parsing;
using SIL.Lift.Tests.Properties;
using SIL.Lift.Validation;
using Unit = NUnit.Framework;
using Has = NMock2.Has;
using Is = NMock2.Is;

namespace SIL.Lift.Tests.Parsing
{
	[TestFixture]
	public class ParserTests
	{
		private ILexiconMerger<DummyBase, Dummy, Dummy, Dummy> _merger;
		private LiftParser<DummyBase, Dummy, Dummy, Dummy> _parser;
		private XmlDocument _doc;
		public StringBuilder _results;
		private Mockery _mocks;
		private List<LiftParser<DummyBase, Dummy, Dummy, Dummy>.ErrorArgs> _parsingWarnings;


		[SetUp]
		public void Setup()
		{
			//_parsingErrors = new List<Exception>();
			_doc = new XmlDocument();
			//_doc.DocumentElement.SetAttribute("xmlns:flex", "http://fieldworks.sil.org");

			_mocks = new Mockery();
			_merger = _mocks.NewMock<ILexiconMerger<DummyBase, Dummy, Dummy, Dummy>>();
			_parser = new LiftParser<DummyBase, Dummy, Dummy, Dummy>(_merger);
			_parsingWarnings = new List<LiftParser<DummyBase, Dummy, Dummy, Dummy>.ErrorArgs>();
			_parser.ParsingWarning += OnParsingWarning;
		}

		void OnParsingWarning(object sender, LiftParser<DummyBase, Dummy, Dummy, Dummy>.ErrorArgs e)
		{
			_parsingWarnings.Add(e);
		}

		[TearDown]
		public void TearDown()
		{


		}

		[Test]
		public void ReadLiftFile_SuppliedChangeDetector_SkipsUnchangedEntries()
		{
			_parser.ChangeReport = new DummyChangeReport();

			_doc.LoadXml("<entry id='old'/>");
			_parser.ReadEntry(_doc.FirstChild);
			_mocks.VerifyAllExpectationsHaveBeenMet();

			ExpectGetOrMakeEntry(new ExtensibleMatcher("changed"));
			ExpectFinishEntry();

			_doc.LoadXml("<entry id='changed'/>");
			_parser.ReadEntry(_doc.FirstChild);
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void ReadLiftFile_OldVersion_ThrowsLiftFormatException()
		{
			using (TempFile f = new TempFile(string.Format("<lift version='{0}'></lift>", /*Validator.LiftVersion*/ "0.0")))
			{
				Assert.Throws<LiftFormatException>(() =>_parser.ReadLiftFile(f.Path));
			}
		}
		[Test]
		public void ReadLiftFile_CurrentVersion_Happy()
		{
			using (TempFile f = new TempFile(string.Format("<lift version='{0}'></lift>", Validator.LiftVersion)))
			{
				_parser.ReadLiftFile(f.Path);
			}
		}


		[Test]
		public void MultipleFormsInOneLangAreCombined()
		{
			_doc.LoadXml("<foobar><form lang='x'><text>one</text></form><form lang='z'><text>zzzz</text></form><form lang='x'><text>two</text></form></foobar>");
			LiftMultiText t = _parser.ReadMultiText(_doc.FirstChild);
			Assert.AreEqual("one; two", t["x"].Text);
			Assert.AreEqual("zzzz", t["z"].Text);
		}


		[Test]
		public void SpanContentsIncludedInForm()
		{
			_doc.LoadXml("<foobar><form lang='x'><text>one <span class='emphasis'>inner text</span> node</text></form></foobar>");
			LiftMultiText t = _parser.ReadMultiText(_doc.FirstChild);
			Assert.AreEqual("one inner text node", t["x"].Text);
			Assert.AreEqual(1, t["x"].Spans.Count);
			Assert.AreEqual("emphasis", t["x"].Spans[0].Class);
			Assert.AreEqual(null, t["x"].Spans[0].Lang);
			Assert.AreEqual(null, t["x"].Spans[0].LinkURL);
			Assert.AreEqual(4, t["x"].Spans[0].Index);
			Assert.AreEqual(10, t["x"].Spans[0].Length);
		}

		[Test]
		public void MultiTextWithTwoInternalSpans()
		{
			// Note that isolated whitespace tends to be swallowed up and ignored when reading XML files.
			// Thus, a single space between two span elements must be represented by a character entity.
			_doc.LoadXml("<foobar><form lang='x'><text>one <span class='emphasis'>inner text</span>&#32;<span class='vernacular' lang='y' href='this is a test'>node</span></text></form></foobar>");
			LiftMultiText t = _parser.ReadMultiText(_doc.FirstChild);
			Assert.AreEqual("one inner text node", t["x"].Text);
			Assert.AreEqual(2, t["x"].Spans.Count);
			Assert.AreEqual("emphasis", t["x"].Spans[0].Class);
			Assert.AreEqual(null, t["x"].Spans[0].Lang);
			Assert.AreEqual(null, t["x"].Spans[0].LinkURL);
			Assert.AreEqual(4, t["x"].Spans[0].Index);
			Assert.AreEqual(10, t["x"].Spans[0].Length);
			Assert.AreEqual("vernacular", t["x"].Spans[1].Class);
			Assert.AreEqual("y", t["x"].Spans[1].Lang);
			Assert.AreEqual("this is a test", t["x"].Spans[1].LinkURL);
			Assert.AreEqual(15, t["x"].Spans[1].Index);
			Assert.AreEqual(4, t["x"].Spans[1].Length);
		}

		[Test]
		public void MultiTextWithNestedSpan()
		{
			_doc.LoadXml("<foobar><form lang='x'><text>one <span class='emphasis'>inner <span class='vernacular' lang='y'>text</span></span> node</text></form></foobar>");
			var t = _parser.ReadMultiText(_doc.FirstChild);
			var tx = t["x"];
			Assert.IsNotNull(tx);
			Assert.AreEqual("one inner text node", tx.Text);
			Assert.AreEqual(1, tx.Spans.Count);
			var span = tx.Spans[0];
			Assert.IsNotNull(span);
			Assert.AreEqual("emphasis", span.Class);
			Assert.AreEqual(null, span.Lang);
			Assert.AreEqual(null, span.LinkURL);
			Assert.AreEqual(4, span.Index);
			Assert.AreEqual(10, span.Length);
			Assert.AreEqual(1, span.Spans.Count);
			var subspan = span.Spans[0];
			Assert.IsNotNull(subspan);
			Assert.AreEqual("vernacular", subspan.Class);
			Assert.AreEqual("y", subspan.Lang);
			Assert.AreEqual(null, subspan.LinkURL);
			Assert.AreEqual(10, subspan.Index);
			Assert.AreEqual(4, subspan.Length);
			Assert.AreEqual(0, subspan.Spans.Count);
		}

		[Test]
		public void MultiTextWithEmptyNestedSpan()
		{
			_doc.LoadXml("<foobar><form lang='x'><text><span lang='y'></span></text></form></foobar>");
			_parser.ParsingWarning += (sender, args) => { Assert.Fail("Could not parse empty span in an empty multitext.");};
			var t = _parser.ReadMultiText(_doc.FirstChild);
			var tx = t["x"];
			Assert.That(tx, Unit.Is.Not.Null);
			Assert.That(tx.Text, Unit.Is.Null);
			Assert.That(tx.Spans.Count, Unit.Is.EqualTo(1));
			var span = tx.Spans[0];
			Assert.That(span, Unit.Is.Not.Null);
			Assert.That(span.Class, Unit.Is.Null);
			Assert.That(span.Lang, Unit.Is.EqualTo("y"));
			Assert.That(span.Length, Unit.Is.EqualTo(0));
		}

		[Test]
		public void FirstValueOfSimpleMultiText()
		{
			LiftMultiText t = new LiftMultiText();
			LiftString s1 = new LiftString();
			s1.Text = "1";
			t.Add("x", s1);
			LiftString s2 = new LiftString();
			s2.Text = "2";
			t.Add("y", s2);
			Assert.AreEqual("x", t.FirstValue.Key);
			Assert.AreEqual("1", t.FirstValue.Value.Text);
		}

		[Test]
		public void EmptyLiftOk()
		{
			SimpleCheckGetOrMakeEntry_InsertVersion("<lift V />", 0);
		}



		[Test]
		public void EntryMissingIdNotFatal()
		{
			SimpleCheckGetOrMakeEntry_InsertVersion("<lift V><entry/></lift>", 1);
		}

		[Test]
		public void EmptyEntriesOk()
		{
			SimpleCheckGetOrMakeEntry_InsertVersion("<lift V><entry/><entry/></lift>", 2);
		}
		[Test]
		public void NotifyOfDeletedEntry()
		{
			DateTime now = DateTime.UtcNow;
			string when = now.ToString(Extensible.LiftTimeFormatWithUTC);
			ExpectEntryWasDeleted();            //todo expect more!
			_doc.LoadXml(String.Format("<entry dateDeleted='{0}'/>", when));
			_parser.ReadEntry(_doc.FirstChild);
			_mocks.VerifyAllExpectationsHaveBeenMet();

		}
		private void SimpleCheckGetOrMakeEntry_InsertVersion(string content, int times)
		{
			content = InsertVersion(content);

			_doc.LoadXml(content);
			using (_mocks.Ordered)
			{
				Expect.Exactly(times).On(_merger)
					.Method("GetOrMakeEntry")
					.WithAnyArguments()
					.Will(Return.Value(null));
			}
			using (TempFile f = new TempFile(string.Format(content)))
			{
				_parser.ReadLiftFile(f.Path);
			}
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}

		private static string InsertVersion(string content) {
			content = content.Replace("<lift V", string.Format("<lift version='{0}' ", Validator.LiftVersion));
			return content;
		}

		[Test]
		public void EntryWithGuid()
		{
			Guid g = Guid.NewGuid();
//            ExpectMergeInLexemeForm(Is.Anything);
			ParseEntryAndCheck(string.Format("<entry guid=\"{0}\" />", g),
							   new ExtensibleMatcher(g));
		}
		[Test]
		public void EntryWithId()
		{
			ParseEntryAndCheck("<entry id='-foo-' />", new ExtensibleMatcher("-foo-"));
		}

		private void ParseEntryAndCheck(string content, Matcher extensibleMatcher)
		{
			ExpectGetOrMakeEntry(extensibleMatcher);
			ExpectFinishEntry();

			_doc.LoadXml(content);
			_parser.ReadEntry(_doc.FirstChild);
			_mocks.VerifyAllExpectationsHaveBeenMet();

		}


		private void ParseEntryAndCheck(string content)
		{
			ExpectFinishEntry();
			_doc.LoadXml(content);
			_parser.ReadEntry(_doc.FirstChild);
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}

		private void ExpectGetOrMakeEntry(Matcher extensibleMatcher)
		{
			Expect.Exactly(1).On(_merger)
				.Method("GetOrMakeEntry")
				//.With(Is.Anything)
				.With(extensibleMatcher, Is.EqualTo(0))
				.Will(Return.Value(new Dummy()));
		}

		private void ExpectEmptyEntry()
		{
			ExpectGetOrMakeEntry();
			//ExpectMergeInLexemeForm(Is.Anything);
		}


		private void ExpectGetOrMakeEntry()
		{
			Expect.Exactly(1).On(_merger)
				.Method("GetOrMakeEntry")
				.Will(Return.Value(new Dummy()));
		}

		private void ExpectGetOrMakeSense()
		{
			Expect.Exactly(1).On(_merger)
				.Method("GetOrMakeSense")
				.Will(Return.Value(new Dummy()));
		}
		private void ExpectMergeInGrammi(string value, Matcher traitListMatcher)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeInGrammaticalInfo")
				.With(Is.Anything, Is.EqualTo(value), traitListMatcher);
		}

		private void ExpectGetOrMakeExample()
		{
			Expect.Exactly(1).On(_merger)
				.Method("GetOrMakeExample")
				.Will(Return.Value(new Dummy()));
		}


		private void ExpectMergeInLexemeForm(Matcher matcher)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeInLexemeForm")
				.With(Is.Anything,matcher);
		}

		//private void ExpectMergeInCitationForm(Matcher matcher)
		//{
		//    Expect.Exactly(1).On(_merger)
		//        .Method("MergeInCitationForm")
		//        .With(Is.Anything, matcher);
		//}

		private void ExpectFinishEntry()
		{
			Expect.Exactly(1).On(_merger)
				.Method("FinishEntry");
		}
		//private void ExpectMergeGloss()
		//{
		//    Expect.Exactly(1).On(_merger)
		//        .Method("MergeInGloss");
		//}
		//private void ExpectMergeDefinition()
		//{
		//    Expect.Exactly(1).On(_merger)
		//        .Method("MergeInDefinition");
		//}


		private void ExpectMergeInField(Matcher tagMatcher, Matcher dateCreatedMatcher, Matcher dateModifiedMatcher, Matcher multiTextMatcher, Matcher traitsMatcher)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeInField").With(Is.Anything, tagMatcher,
											 dateCreatedMatcher, dateModifiedMatcher, multiTextMatcher, traitsMatcher);
			//  .Method("MergeInField").With(matchers);
		}


//        private void ExpectMergeInField(params object[] matchers)
//        {
//            Expect.Exactly(1).On(_merger)
//                .Method("MergeInField").With(Is.Anything, Is.Anything, Is.Anything, Is.Anything, Is.Anything);
//              //  .Method("MergeInField").With(matchers);
//        }

		private void ExpectMergeInTrait(Matcher traitMatcher)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeInTrait")
				.With(Is.Anything, traitMatcher);
		}
		private void ExpectMergeInRelation(string relationType, string targetId)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeInRelation")
				.With(Is.Anything, Is.EqualTo(relationType), Is.EqualTo(targetId), Is.Anything);
		}

		private void ExpectMergeInPicture(string href)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeInPicture")
				.With(Is.Anything, Is.EqualTo(href), Is.Null);
		}

		private void ExpectMergeInPictureWithCaption(string href)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeInPicture")
				.With(Is.Anything, Is.EqualTo(href), Is.NotNull);
		}

		private void ExpectMergeInMediaWithCaption(string href, string caption)
		{
			Expect.Exactly(1).On(_merger)
							.Method("MergeInMedia")
							.With(Is.Anything, Is.EqualTo(href), Has.ToString(Is.EqualTo(caption)));
		}

		private void ExpectEntryWasDeleted()
		{
			Expect.Exactly(1).On(_merger)
				.Method("EntryWasDeleted");
			//todo expect more!
		}

		private void ExpectMergeInNote(string value)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeInNote")
				.With(Is.Anything, Is.Anything/*todo type*/, Has.ToString(Is.EqualTo(value)), Is.Anything);
		}

		private void ExpectTypedMergeInNote(string type)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeInNote")
				.With(Is.Anything, Is.EqualTo(type), Is.Anything, Is.Anything);
		}





		[Test]
		public void EntryWithoutId()
		{
//            ExpectMergeInLexemeForm(Is.Anything);
			ParseEntryAndCheck("<entry/>", new ExtensibleMatcher());
		}

		[Test]
		public void EntryWithReadableIdPlusGuid()
		{
//            ExpectMergeInLexemeForm(Is.Anything);
			Guid g = Guid.NewGuid();
//            string s = String.Format("<lift xmlns:flex='http://fieldworks.sil.org'><entry  id='-foo' flex:guid='{0}'/></lift>", g);
//
//            _doc.LoadXml(s);
//            _parser.ReadFile(_doc);
//

			// string s = String.Format("<entry xmlns:flex='http://fieldworks.sil.org' id='-foo' flex:guid='{0}'/>", g);
			string s = String.Format("<entry id='-foo' guid='{0}'/>", g);
			ParseEntryAndCheck(s, new ExtensibleMatcher("-foo", g));
		}

		[Test]
		public void FormMissingLangGeneratesNonFatalError()
		{
			ExpectGetOrMakeEntry();
//            ExpectMergeInLexemeForm(Is.Anything);
			ParseEntryAndCheck("<entry><lexical-unit><form/></lexical-unit></entry>");
			Assert.AreEqual(1, _parsingWarnings.Count);
		}


		[Test]
		public void EmptyFormOk()
		{
			using (_mocks.Ordered)
			{
				ExpectGetOrMakeEntry(/*";;;"*/);
				ExpectMergeInLexemeForm(Is.Anything);
			}
			ParseEntryAndCheck("<entry><lexical-unit><form lang='x'/></lexical-unit></entry>");
		}

//        [Test]
//        public void SpacesTrimmedFromLexicalUnit()
//        {
//            ExpectGetOrMakeEntry();
//            ExpectMultiTextMergeIn("LexemeForm", Has.Property("Count", Is.EqualTo(2)));
//            //            ExpectMergeInCitationForm(Is.Anything);
//            string content ="<entry><lexical-unit><form lang='x'><text> hello </text></form></lexical-unit></entry>";
//            ExpectFinishEntry();
//            _doc.LoadXml(content);
//            Dummy d = _parser.ReadEntry(_doc.FirstChild);
//            d
//        }

		[Test]
		public void EntryWithLexicalUnit()
		{
			ExpectGetOrMakeEntry();
			ExpectMultiTextMergeIn("LexemeForm", Has.Property("Count", Is.EqualTo(2)));
//            ExpectMergeInCitationForm(Is.Anything);
			ParseEntryAndCheck("<entry><lexical-unit><form lang='x'><text>hello</text></form><form lang='y'><text>bye</text></form></lexical-unit></entry>");
			//           ParseEntryAndCheck("<entry><lexical-unit><form lang='x'><text>hello</text></form><form lang='y'>bye</form></lexical-unit></entry>", "GetOrMakeEntry(;;;)MergeInLexemeForm(m,x=hello|y=bye|)");
		}

		[Test]
		public void EntryWithCitationForm()
		{
			ExpectGetOrMakeEntry();
			//          ExpectMergeInLexemeForm(Is.Anything);
			ExpectMultiTextMergeIn("CitationForm", Has.Property("Count", Is.EqualTo(2)));
			ParseEntryAndCheck("<entry><citation><form lang='x'><text>hello</text></form><form lang='y'><text>bye</text></form></citation></entry>");
		}

		[Test]
		public void EntryWithPronunciation()
		{
			ExpectGetOrMakeEntry();
			ExpectMergeInPronunciation("en__IPA=ai|");
			ParseEntryAndCheck("<entry><pronunciation><form lang='en__IPA'><text>ai</text></form></pronunciation></entry>");
		}

		[Test]
		public void EntryWithPronunciationWithFields()
		{
			ExpectGetOrMakeEntry();
			ExpectMergeInPronunciation("");
			ExpectMergeInField(
				Is.EqualTo("cvPattern"),
				Is.EqualTo(default(DateTime)),
				Is.EqualTo(default(DateTime)),
				Has.Property("Count", Is.EqualTo(1)),//multitext
				Has.Property("Count", Is.EqualTo(0))//traits
				);
			ParseEntryAndCheck(@"<entry><pronunciation>
					<field type='cvPattern'>
						<form lang='en'>
							<text>acvpattern</text>
						</form>
					</field>
				</pronunciation></entry>");
		}

		[Test]
		public void EntryWithPronunciationWithMedia()
		{
			ExpectGetOrMakeEntry();
			ExpectMergeInPronunciation("en__IPA=ai|");
			ExpectMergeInMediaWithCaption("blah.mp3", "en=This is a test|");
			ParseEntryAndCheck("<entry><pronunciation><form lang='en__IPA'><text>ai</text></form><media href='blah.mp3'><label><form lang='en'><text>This is a test</text></form></label></media></pronunciation></entry>");
		}

		private void ExpectMergeInPronunciation(string value)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeInPronunciation")
				.With(Is.Anything, Has.ToString(Is.EqualTo(value)), Is.Anything)
				.Will(Return.Value(new Dummy()));
		}

		[Test]
		public void EntryWithVariant()
		{
			ExpectGetOrMakeEntry();
			ExpectMergeInVariant("en=-d|");
			ParseEntryAndCheck("<entry><variant><form lang='en'><text>-d</text></form></variant></entry>");
		}

		private void ExpectMergeInVariant(string value)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeInVariant")
				.With(Is.Anything, Has.ToString(Is.EqualTo(value)), Is.Anything)
				.Will(Return.Value(new Dummy()));
		}

		// private void ExpectEmptyMultiTextMergeIn(string MultiTextPropertyName)
		//{
		//    Expect.Exactly(1).On(_merger)
		//                    .Method("MergeIn" + MultiTextPropertyName)
		//                    .With(Is.Anything, Has.Property("Count",Is.EqualTo(0)));

		//}

		private void ExpectValueOfMergeIn(string MultiTextPropertyName, string value)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeIn" + MultiTextPropertyName)
				.With(Is.Anything, Has.ToString(Is.EqualTo(value)));
		}

		private void ExpectValueOfMergeInTranslationForm(string type, string value)
		{
			if (type == null)
				Expect.Exactly(1).On(_merger)
					.Method("MergeInTranslationForm")
					.With(Is.Anything, Is.Null, Has.ToString(Is.EqualTo(value)), Is.Anything);
			else
				Expect.Exactly(1).On(_merger)
					.Method("MergeInTranslationForm")
					.With(Is.Anything, Has.ToString(Is.EqualTo(type)), Has.ToString(Is.EqualTo(value)), Is.Anything);
		}

		//        private void ExpectMultiTextMergeIn(string MultiTextPropertyName, Matcher matcher)
//        {
//             Expect.Exactly(1).On(_merger)
//                            .Method("MergeIn" + MultiTextPropertyName)
//                            .With(Is.Anything, Has.Property("Traits",  matcher));
//       }

		private void ExpectMultiTextMergeIn(string MultiTextPropertyName, Matcher multiTextMatcher)
		{
			Expect.Exactly(1).On(_merger)
				.Method("MergeIn" + MultiTextPropertyName)
				.With(Is.Anything, multiTextMatcher);
		}


		[Test]
		public void NonLiftDateError()
		{
			TryDateFormat("last tuesday");
			TryDateFormat("2005-01-01T01:11:11");
			TryDateFormat("1/2/2003");
			Assert.AreEqual(3, _parsingWarnings.Count);
		}

		private void TryDateFormat(string created)
		{
			ExpectGetOrMakeEntry();
//            ExpectMergeInLexemeForm(Is.Anything);
			ParseEntryAndCheck(
				string.Format("<entry id='foo' dateCreated='{0}'></entry>", created));
		}

		[Test]
		public void DateWithoutTimeOk()
		{
			ExpectGetOrMakeEntry();
//            ExpectMergeInLexemeForm(Is.Anything);
			ParseEntryAndCheck("<entry id='foo' dateCreated='2005-01-01'></entry>");
			Assert.AreEqual(0, _parsingWarnings.Count);
		}

		[Test]
		public void EntryWithDates()
		{
			string createdIn = "2003-08-07T08:42:42+07:00";
			string modIn = "2005-01-01T01:11:11+07:00";
			DateTime createdOut = new DateTime(2003, 08, 07, 01, 42, 42, DateTimeKind.Utc);//"2003-08-07T01:42:42Z"  has to be UTC (in - 7 hours)
			DateTime modOut = new DateTime(2004, 12, 31, 18, 11, 11, DateTimeKind.Utc); //"2004-12-31T18:11:11Z" has to be UTC (in - 7 hours)
			ExpectGetOrMakeEntry(new ExtensibleMatcher("foo", createdOut, modOut));

//            ExpectEmptyMultiTextMergeIn("LexemeForm");
			ParseEntryAndCheck(
				string.Format("<entry id='foo' dateCreated='{0}' dateModified='{1}'></entry>", createdIn, modIn));

		}


		[Test]
		public void EntryWithNote()
		{
			ExpectGetOrMakeEntry();
			//        ExpectMergeInLexemeForm(Is.Anything);
			ExpectMergeInNote("x=hello|");

			ParseEntryAndCheck(string.Format("<entry><note><form lang='x'><text>hello</text></form></note></entry>"));
		}

		[Test]
		public void EntryWithTwoNotes()
		{
			ExpectGetOrMakeEntry();
			ExpectTypedMergeInNote("typeone");
			ExpectTypedMergeInNote("typetwo");

			ParseEntryAndCheck(string.Format("<entry><note type='typeone'><form lang='x'><text>one</text></form></note><note type='typetwo'><form lang='x'><text>two</text></form></note></entry>"));
		}



		[Test]
		public void EntryWithSense()
		{
			ExpectGetOrMakeEntry();
			//    ExpectMergeInLexemeForm(Is.Anything);
			ExpectGetOrMakeSense();
			//  ExpectMergeGloss();
			//  ExpectMergeDefinition();
			ParseEntryAndCheck(string.Format("<entry><sense></sense></entry>"));
		}

		[Test]
		public void SenseWithGloss()
		{
			ExpectGetOrMakeEntry();
//            ExpectMergeInLexemeForm(Is.Anything);
			ExpectGetOrMakeSense();
			ExpectValueOfMergeIn("Gloss","x=hello|");
//            ExpectMergeDefinition();

			ParseEntryAndCheck(string.Format("<entry><sense><gloss lang='x'><text>hello</text></gloss></sense></entry>"));
		}



		[Test]
		public void LexicalUnitWithAnnotation()
		{
			ExpectGetOrMakeEntry();
			ExpectMergeInLexemeForm(new LiftMultiTextAnnotationMatcher(1, "x", "flag", "1", null, default(DateTime)));
			ParseEntryAndCheck(string.Format("<entry><lexical-unit><form lang='x'><text>blah blah</text><annotation name='flag' value='1'/></form></lexical-unit></entry>"));
		}

		[Test]
		public void DefinitionWithAnnotation()
		{
			ExpectGetOrMakeEntry();
			//ExpectMergeInLexemeForm(Is.Anything);
			ExpectGetOrMakeSense();
			string when= new DateTime(2000,1,1).ToUniversalTime().ToString(Extensible.LiftTimeFormatWithUTC);
			ExpectMultiTextMergeIn("Definition", new LiftMultiTextAnnotationMatcher(1, "x", "flag", "1", "john", DateTime.Parse(when).ToUniversalTime()));

			ParseEntryAndCheck(string.Format(@"
			<entry>
				<sense>
					<definition>
						<form lang='z'>
							<text>hello</text>
							<annotation name='flag' value='1' who='john' when='{0}'>
								<form lang='x'>
									<text>blah blah</text>
								</form>
							</annotation>
						</form>
					</definition></sense></entry>", when));
		}

		[Test]
		public void SenseWithTraitWithAnnotations()
		{
			ExpectGetOrMakeEntry();
			//ExpectMergeInLexemeForm(Is.Anything);
			ExpectGetOrMakeSense();
			string when= new DateTime(2000,1,1).ToUniversalTime().ToString(Extensible.LiftTimeFormatWithUTC);
			ExpectMergeInTrait(new TraitMatcher("dummy", "blah", 2));
			//ExpectMergeDefinition();

			ParseEntryAndCheck(string.Format(@"
			<entry>
				<sense>
					<trait name='dummy' value ='blah'>
						<annotation name='first'/>
						<annotation name='second'/>
				</trait></sense></entry>", when));
		}

		[Test]
		public void GrammiWithTwoTraits()
		{
			ExpectGetOrMakeEntry();
			//ExpectMergeInLexemeForm(Is.Anything);
			ExpectGetOrMakeSense();
			//ExpectMultiTextMergeIn("Gloss", Is.Anything);
			//ExpectMergeDefinition();
			ExpectMergeInGrammi("x", Has.Property("Count", Is.EqualTo(2)));

			ParseEntryAndCheck(string.Format("<entry><sense><grammatical-info value='x'><trait name='one' value='1'/><trait name='two' value='2'/></grammatical-info></sense></entry>"));
		}

		[Test]
		public void GlossWithTwoLanguages()
		{
			ExpectGetOrMakeEntry();
			ExpectGetOrMakeSense();
			ExpectValueOfMergeIn("Gloss", "x=hello|y=bye|");

			ParseEntryAndCheck(string.Format("<entry><sense><gloss lang='x'><text>hello</text></gloss><gloss lang='y'><text>bye</text></gloss></sense></entry>"));
		}

		[Test]
		public void GlossWithTwoFormsInSameLanguageAreCombined()
		{
			ExpectGetOrMakeEntry();
			//ExpectMergeInLexemeForm(Is.Anything);
			ExpectGetOrMakeSense();
			ExpectValueOfMergeIn("Gloss", "x=hello; bye|");
			//ExpectMergeDefinition();

			ParseEntryAndCheck(string.Format("<entry><sense><gloss lang='x'><text>hello</text></gloss><gloss lang='x'><text>bye</text></gloss></sense></entry>"));
		}
		[Test]
		public void SenseWithDefintition()
		{
			ExpectEmptyEntry();
			ExpectGetOrMakeSense();
			//ExpectMergeGloss();
			ExpectValueOfMergeIn("Definition", "x=hello|");

			ParseEntryAndCheck(string.Format("<entry><sense><definition><form lang='x'><text>hello</text></form></definition></sense></entry>"));
		}

		[Test]
		public void SenseWithNote()
		{
			ExpectEmptyEntry();
			ExpectGetOrMakeSense();
			//ExpectMergeGloss();
			//ExpectMergeDefinition();
			ExpectMergeInNote("x=hello|");

			ParseEntryAndCheck(string.Format("<entry><sense><note><form lang='x'><text>hello</text></form></note></sense></entry>"));
		}

		[Test]
		public void FieldOnEntries()
		{
			ExpectEmptyEntry();
			ExpectMergeInField(
				Is.EqualTo("color"),
				Is.EqualTo(default(DateTime)),
				Is.EqualTo(default(DateTime)),
				Has.Property("Count", Is.EqualTo(2)),
				Has.Property("Count", Is.EqualTo(0))
				);
			ParseEntryAndCheck(
				"<entry><field type='color'><form lang='en'><text>red</text></form><form lang='es'><text>roco</text></form></field></entry>");
		}

		[Test]
		public void FieldOnSenses()
		{
			ExpectEmptyEntry();
			ExpectGetOrMakeSense();
			ExpectMergeInField(
				Is.EqualTo("color"),
				Is.EqualTo(default(DateTime)),
				Is.EqualTo(default(DateTime)),
				Has.Property("Count", Is.EqualTo(2)),
				Has.Property("Count", Is.EqualTo(0))
				);
			ParseEntryAndCheck(
				"<entry><sense><field type='color'><form lang='en'><text>red</text></form><form lang='es'><text>roco</text></form></field></sense></entry>");
		}

		[Test]
		public void FieldOnExamples()
		{
			ExpectEmptyEntry();
			ExpectGetOrMakeSense();
			ExpectGetOrMakeExample();
			ExpectMergeInField(
				Is.EqualTo("color"),
				Is.EqualTo(default(DateTime)),
				Is.EqualTo(default(DateTime)),
				Has.Property("Count", Is.EqualTo(2)),
				Has.Property("Count", Is.EqualTo(0))
				);
			ParseEntryAndCheck(
				"<entry><sense><example><field type='color'><form lang='en'><text>red</text></form><form lang='es'><text>roco</text></form></field></example></sense></entry>");
		}


		[Test]
		public void MultipleFieldsOnEntries()
		{
			ExpectEmptyEntry();
			ExpectMergeInField(
				Is.EqualTo("color"),
				Is.EqualTo(default(DateTime)),
				Is.EqualTo(default(DateTime)),
				Has.Property("Count", Is.EqualTo(2)),
				Has.Property("Count", Is.EqualTo(0))
				);
			ExpectMergeInField(
				Is.EqualTo("special"),
				Is.EqualTo(default(DateTime)),
				Is.EqualTo(default(DateTime)),
				Has.Property("Count", Is.EqualTo(1)),
				Has.Property("Count", Is.EqualTo(0))
				);
			ParseEntryAndCheck(
				"<entry><field type='color'><form lang='en'><text>red</text></form><form lang='es'><text>roco</text></form></field><field type='special'><form lang='en'><text>free</text></form></field></entry>");
		}




		[Test]
		public void DatesOnFields()
		{

			ExpectEmptyEntry();
			DateTime creat = new DateTime(2000,1,1).ToUniversalTime();
			string createdTime = creat.ToString(Extensible.LiftTimeFormatWithUTC);
			DateTime mod = new DateTime(2000, 1, 2).ToUniversalTime();
			string modifiedTime = mod.ToString(Extensible.LiftTimeFormatWithUTC);
			ExpectMergeInField(
				Is.EqualTo("color"),
				Is.EqualTo(creat),
				Is.EqualTo(mod),
				Is.Anything,
				Has.Property("Count", Is.EqualTo(0))
				);
			ParseEntryAndCheck(String.Format("<entry><field type='color' dateCreated='{0}'  dateModified='{1}' ></field></entry>",
											 createdTime,
											 modifiedTime));
		}

		[Test]
		public void TraitsOnEntries()
		{
			ExpectEmptyEntry();
			ExpectMergeInTrait(new NMock2.Matchers.AndMatcher(
								   Has.Property("Name", Is.EqualTo("color")), Has.Property("Value", Is.EqualTo("red"))));
			ExpectMergeInTrait(new NMock2.Matchers.AndMatcher(
								   Has.Property("Name", Is.EqualTo("shape")), Has.Property("Value", Is.EqualTo("square"))));
			ParseEntryAndCheck(string.Format("<entry><trait name='color' value='red'/><trait name='shape' value='square'/></entry>"));
		}


		[Test]
		public void TraitsOnEntries_MultipleOfSameType_Okay()
		{
			ExpectEmptyEntry();
			ExpectMergeInTrait(new NMock2.Matchers.AndMatcher(
								   Has.Property("Name", Is.EqualTo("color")), Has.Property("Value", Is.EqualTo("red"))));
			ExpectMergeInTrait(new NMock2.Matchers.AndMatcher(
								   Has.Property("Name", Is.EqualTo("color")), Has.Property("Value", Is.EqualTo("blue"))));
			ParseEntryAndCheck(string.Format("<entry><trait name='color' value='red'/><trait name='color' value='blue'/></entry>"));
		}


		[Test]
		public void TraitsOnSenses()
		{
			ExpectEmptyEntry();
			ExpectGetOrMakeSense();
			ExpectMergeInTrait(new NMock2.Matchers.AndMatcher(
								   Has.Property("Name", Is.EqualTo("color")), Has.Property("Value", Is.EqualTo("red"))));
			ExpectMergeInTrait(new NMock2.Matchers.AndMatcher(
								   Has.Property("Name", Is.EqualTo("shape")), Has.Property("Value", Is.EqualTo("square"))));
			ParseEntryAndCheck(string.Format("<entry><sense><trait name='color' value='red'/><trait name='shape' value='square'/></sense></entry>"));
		}

		[Test]
		public void TraitsOnExamples()
		{
			ExpectEmptyEntry();
			ExpectGetOrMakeSense();
			ExpectGetOrMakeExample();
			ExpectMergeInTrait(new NMock2.Matchers.AndMatcher(
								   Has.Property("Name", Is.EqualTo("color")), Has.Property("Value", Is.EqualTo("red"))));
			ExpectMergeInTrait(new NMock2.Matchers.AndMatcher(
								   Has.Property("Name", Is.EqualTo("shape")), Has.Property("Value", Is.EqualTo("square"))));
			ParseEntryAndCheck(string.Format("<entry><sense><example><trait name='color' value='red'/><trait name='shape' value='square'/></example></sense></entry>"));
		}


		[Test]
		public void SenseWithGrammi()
		{
			ExpectEmptyEntry();
			ExpectGetOrMakeSense();
			//ExpectMergeGloss();
			//ExpectMergeDefinition();
			ExpectMergeInGrammi("blue", Is.Anything);
			ParseEntryAndCheck("<entry><sense><grammatical-info value='blue'/></sense></entry>");
		}

		[Test]
		public void SenseWithExample()
		{
			ExpectGetOrMakeEntry();
			//ExpectMergeInLexemeForm(Is.Anything);
			ExpectGetOrMakeSense();
			//ExpectMergeGloss();
			//ExpectMergeDefinition();
			ExpectGetOrMakeExample();
			ExpectValueOfMergeIn("ExampleForm", "x=hello|");
//            ExpectValueOfMergeIn("TranslationForm", "");

			ParseEntryAndCheck(
				string.Format("<entry><sense><example><form lang='x'><text>hello</text></form></example></sense></entry>"));
		}

		[Test]
		public void SenseWithRelation()
		{
			ExpectGetOrMakeEntry();
			ExpectGetOrMakeSense();
			ExpectMergeInRelation("synonym", "one");

			ParseEntryAndCheck(
				string.Format("<entry><sense><relation type=\"synonym\" ref=\"one\" /></sense></entry>"));
		}

		[Test]
		public void SenseWithPicture()
		{
			ExpectGetOrMakeEntry();
			ExpectGetOrMakeSense();
			ExpectMergeInPicture("bird.jpg");

			ParseEntryAndCheck(
				string.Format("<entry><sense><illustration href=\"bird.jpg\" /></sense></entry>"));
		}


		[Test]
		public void SenseWithPictureAndCaption()
		{
			ExpectGetOrMakeEntry();
			ExpectGetOrMakeSense();
			ExpectMergeInPictureWithCaption("bird.jpg");

			ParseEntryAndCheck(
				string.Format("<entry><sense><illustration href=\"bird.jpg\" ><label><form lang='en'><text>bird</text></form></label></illustration></sense></entry>"));
		}

		[Test]
		public void ExampleWithTranslation()
		{
			ExpectGetOrMakeEntry();
			//ExpectMergeInLexemeForm(Is.Anything);
			ExpectGetOrMakeSense();
			//ExpectMergeGloss();
			//ExpectMergeDefinition();
			ExpectGetOrMakeExample();
			//          ExpectValueOfMergeIn("ExampleForm", "");
			ExpectValueOfMergeInTranslationForm("Free Translation", "x=hello|");

			ParseEntryAndCheck("<entry><sense><example><translation type='Free Translation'><form lang='x'><text>hello</text></form></translation></example></sense></entry>");
			//    "GetOrMakeEntry(;;;)GetOrMakeSense(m,)GetOrMakeExample(m,)MergeInTranslationForm(m,x=hello|)");
		}

		[Test]
		public void ExampleWithSource()
		{
			ExpectGetOrMakeEntry();
			//ExpectMergeInLexemeForm(Is.Anything);
			ExpectGetOrMakeSense();
			//ExpectMergeGloss();
			//ExpectMergeDefinition();
			ExpectGetOrMakeExample();
//            ExpectValueOfMergeIn("ExampleForm", "");
			ExpectValueOfMergeInTranslationForm(null, "x=hello|");

			ExpectValueOfMergeIn("Source", "test");

			ParseEntryAndCheck("<entry><sense><example source='test'><translation><form lang='x'><text>hello</text></form></translation></example></sense></entry>");
			//    "GetOrMakeEntry(;;;)GetOrMakeSense(m,)GetOrMakeExample(m,)MergeInTranslationForm(m,x=hello|)");
		}

		[Test]
		public void ExampleWithNote()
		{
			ExpectEmptyEntry();
			ExpectGetOrMakeSense();
			//ExpectMergeGloss();
			//ExpectMergeDefinition();
			ExpectGetOrMakeExample();
			ExpectMergeInNote("x=hello|");

			ParseEntryAndCheck(string.Format("<entry><sense><example><note><form lang='x'><text>hello</text></form></note></example></sense></entry>"));
		}

		[Test]
		public void EmptyHeaderOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header/><entry/></lift>"), 0, 0, 1);
		}

		[Test]
		public void EmptyHeaderNoEntriesOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header/></lift>"), 0,0,0);
		}

		[Test]
		public void EmptyFieldsOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><fields/></header><entry/></lift>"), 0,0,1);
		}

		[Test]
		public void EmptyFieldsNoEntriesOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><fields/></header></lift>"), 0, 0, 0);
		}
		[Test]
		public void EmptyFieldOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><fields><field tag='custom'/></fields></header><entry/></lift>"), 0,1,1);
		}
		[Test]
		public void TwoFields()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><fields><field tag='special'/><field tag='custom'></field></fields></header><entry/></lift>"), 0, 2, 1);
		}

		[Test]
		public void EmptyFieldNoEntriesOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><fields><field tag='custom'/></fields></header></lift>"), 0, 1, 0);
		}


		[Test]
		public void EmptyRangesOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><ranges/></header><entry/></lift>"), 0,0,1);
		}
		[Test]
		public void EmptyRangesNoEntriesOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><ranges/></header></lift>"), 0, 0, 0);
		}



		[Test]
		public void EmptyRangeOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><ranges><range/></ranges></header><entry/></lift>"), 0,0,1);
		}

		[Test]
		public void EmptyRangeNoEntriesOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><ranges><range/></ranges></header></lift>"), 0, 0, 0);
		}

		[Test]
		public void EmptyLiftHeaderSectionsFieldsBeforeRangesOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><fields/><ranges/></header><entry/></lift>"), 0, 0, 1);
		}

		[Test]
		public void EmptyLiftHeaderSectionsOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><ranges/><fields/></header><entry/></lift>"), 0, 0, 1);
		}
		[Test]
		public void EmptyLiftHeaderSectionsNoEntriesOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><ranges/><fields/></header></lift>"), 0, 0, 0);
		}

		[Test]
		public void SimpleRangeElement()
		{
			string content = InsertVersion("<lift V><header><ranges><range id='dialect'><range-element id='en'><label><form lang='en'><text>English</text></form></label><abbrev><form lang='en'><text>Eng</text></form></abbrev><description><form lang='en'><text>Standard English</text></form></description></range-element></range></ranges></header><entry/></lift>");
			Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
				.With(Is.EqualTo("dialect"), Is.EqualTo("en"), Is.Null, Is.Null,
					  Is.EqualTo(new LiftMultiText("en", "Standard English")),
					  Is.EqualTo(new LiftMultiText("en", "English")),
					  Is.EqualTo(new LiftMultiText("en", "Eng")),
					  Is.EqualTo("<range-element id=\"en\"><label><form lang=\"en\"><text>English</text></form></label><abbrev><form lang=\"en\"><text>Eng</text></form></abbrev><description><form lang=\"en\"><text>Standard English</text></form></description></range-element>"));
			ExpectGetOrMakeEntry();
			ExpectFinishEntry();
			using (TempFile f = new TempFile(string.Format(content)))
			{
				_parser.ReadLiftFile(f.Path);
			}
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}


		private void SimpleCheckWithHeader(string content, int rangeElementCount, int fieldCount, int entryCount)
		{
			using (_mocks.Unordered)
			{
				Expect.Exactly(rangeElementCount).On(_merger).Method("ProcessRangeElement")
					.WithAnyArguments();
				Expect.Exactly(fieldCount).On(_merger).Method("ProcessFieldDefinition")
					.WithAnyArguments();
				Expect.Exactly(entryCount).On(_merger).Method("GetOrMakeEntry").WithAnyArguments().Will(Return.Value(null));
			}
			using (TempFile f = new TempFile(string.Format(content)))
			{
				_parser.ReadLiftFile(f.Path);
			}
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void GetNumberOfEntriesInFile_0Entries_Returns0()
		{
			using(TempFile f = new TempFile( "<lift></lift>"))
			{
				int count = LiftParser<DummyBase, Dummy, Dummy, Dummy>.GetEstimatedNumberOfEntriesInFile(f.Path);
				Assert.AreEqual(0, count);
			}
		}

		[Test]
		public void GetNumberOfEntriesInFile_3Entries_Returns3()
		{
			string path = Path.GetTempFileName();
			try
			{
				File.WriteAllText(path, @"<lift><entry></entry>
				<entry id='foo'/><entry/></lift>");
				int count = LiftParser<DummyBase, Dummy, Dummy, Dummy>.GetEstimatedNumberOfEntriesInFile(path);
				Assert.AreEqual(3, count);
			}
			finally
			{
				File.Delete(path);
			}
		}

		[Test]
		public void SimpleFieldDefinition()
		{
			string content = "<field tag='tone'><form lang='en'><text>the tone information for a pronunciation</text></form></field>";
			Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
				.With(Is.EqualTo("tone"), Is.EqualTo(new LiftMultiText("en", "the tone information for a pronunciation")));
			_doc.LoadXml(content);
			_parser.ReadFieldDefinition(_doc.FirstChild);
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void SimpleEtymology()
		{
			string content = "<entry><etymology source='Greek' type='borrowed'><form lang='bam'><text>alphabeta</text></form><gloss lang='en'><text>letters</text></gloss><field type='comment'><form lang='en'><text>this etymology is nonsense</text></form></field></etymology></entry>";
			_doc.LoadXml(content);
			using (_mocks.Ordered)
			{
				ExpectGetOrMakeEntry();
				Expect.Exactly(1).On(_merger).Method("MergeInEtymology")
					.With(Is.Anything, Is.EqualTo("Greek"), Is.EqualTo("borrowed"),
						  Is.EqualTo(new LiftMultiText("bam", "alphabeta")),
						  Is.EqualTo(new LiftMultiText("en", "letters")), Is.Anything)
					.Will(Return.Value(new Dummy()));
				Expect.Exactly(1).On(_merger).Method("MergeInField")
					.With(Is.Anything, Is.EqualTo("comment"), Is.EqualTo(DateTime.MinValue), Is.EqualTo(DateTime.MinValue),
						  Is.EqualTo(new LiftMultiText("en", "this etymology is nonsense")), Is.Anything);
				ExpectFinishEntry();
			}
			_parser.ReadEntry(_doc.FirstChild);
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void SimpleReversal()
		{
			string content = "<entry><sense><reversal><form lang='en'><text>sorghum</text></form></reversal></sense></entry>";
			_doc.LoadXml(content);
			using (_mocks.Ordered)
			{
				ExpectGetOrMakeEntry();
				ExpectGetOrMakeSense();
				Expect.Exactly(1).On(_merger).Method("MergeInReversal")
					.With(Is.Anything, Is.Null, Is.EqualTo(new LiftMultiText("en", "sorghum")), Is.Null, Is.Anything);
				ExpectFinishEntry();
			}
			_parser.ReadEntry(_doc.FirstChild);
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void NestedReversal()
		{
			string content = "<entry><sense><reversal type='test'><form lang='en'><text>apple</text></form><main><form lang='en'><text>fruit</text></form></main></reversal></sense></entry>";
			_doc.LoadXml(content);
			using (_mocks.Ordered)
			{
				ExpectGetOrMakeEntry();
				ExpectGetOrMakeSense();
				Expect.Exactly(1).On(_merger).Method("GetOrMakeParentReversal")
					.With(Is.Null, Is.EqualTo(new LiftMultiText("en", "fruit")), Is.EqualTo("test"));
				Expect.Exactly(1).On(_merger).Method("MergeInReversal")
					.With(Is.Anything, Is.Null, Is.EqualTo(new LiftMultiText("en", "apple")), Is.EqualTo("test"), Is.Anything);
				ExpectFinishEntry();
			}
			_parser.ReadEntry(_doc.FirstChild);
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void ReadSubSense()
		{
			string content = "<entry><sense><gloss lang='en'><text>destroy</text></gloss><subsense><gloss lang='en'><text>unmake</text></gloss></subsense></sense></entry>";
			_doc.LoadXml(content);
			using (_mocks.Ordered)
			{
				ExpectGetOrMakeEntry();
				ExpectGetOrMakeSense();
				Expect.Exactly(1).On(_merger).Method("MergeInGloss")
					.With(Is.NotNull, Is.EqualTo(new LiftMultiText("en", "destroy")));
				Expect.Exactly(1).On(_merger).Method("GetOrMakeSubsense")
					.Will(Return.Value(new Dummy()));
				Expect.Exactly(1).On(_merger).Method("MergeInGloss")
					.With(Is.Anything, Is.EqualTo(new LiftMultiText("en", "unmake")));
				ExpectFinishEntry();
			}
			_parser.ReadEntry(_doc.FirstChild);
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}

		private class NewLineAgnosticEqualMatcher: NMock2.Matcher
		{
			public NewLineAgnosticEqualMatcher(string expected)
			{
				Expected = expected.Replace("\r\n", "\n");
			}

			private string Expected { get; set; }

			public override void DescribeTo(TextWriter writer)
			{
				writer.Write("equal to ");
				writer.Write(Expected);
			}

			public override bool Matches(object o)
			{
				var str = o as string;
				if (str == null)
					return false;

				return Expected.Equals(str.Replace("\r\n", "\n"));
			}
		}

		public static Matcher IsEqualToIgnoreNl(string expected)
		{
			return new NewLineAgnosticEqualMatcher(expected);
		}

		[Test]
		public void ReadExternalLiftFile()
		{
			const string NewLine = "\n";

			using (_mocks.Ordered)	// Ordered may be too strong if parse details change.
			{
				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("etymology"), Is.EqualTo("borrowed"), Is.Null, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "The word is borrowed from another language")),
						  Is.EqualTo(new LiftMultiText("en", "borrowed")),
						  Is.EqualTo(new LiftMultiText()),
						  IsEqualToIgnoreNl("<range-element id=\"borrowed\">" + NewLine +
								"          <label>" + NewLine +
								"            <form lang=\"en\"><text>borrowed</text></form>" + NewLine +
								"          </label>" + NewLine +
								"          <description>" + NewLine +
								"            <form lang=\"en\"><text>The word is borrowed from another language</text></form>" + NewLine +
								"          </description>" + NewLine +
								"        </range-element>"));
				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("etymology"), Is.EqualTo("proto"), Is.Null, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "The proto form of the word in another language")),
						  Is.EqualTo(new LiftMultiText("en", "proto")),
						  Is.EqualTo(new LiftMultiText()),
						  IsEqualToIgnoreNl("<range-element id=\"proto\">" + NewLine +
								"          <label>" + NewLine +
								"            <form lang=\"en\"><text>proto</text></form>" + NewLine +
								"          </label>" + NewLine +
								"          <description>" + NewLine +
								"            <form lang=\"en\"><text>The proto form of the word in another language</text></form>" + NewLine +
								"          </description>" + NewLine +
								"        </range-element>"));
				// The following range elements are from an external range file.
				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("grammatical-info"), Is.EqualTo("Adverb"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "modify verbs")),
						  Is.EqualTo(new LiftMultiText("en", "Adverb")),
						  Is.EqualTo(new LiftMultiText("en", "adv")),
						  IsEqualToIgnoreNl("<range-element guid=\"c528ee72-31a5-423d-833d-0c8454f345d3\" id=\"Adverb\">" + NewLine +
								"      <label><form lang=\"en\"><text>Adverb</text></form></label>" + NewLine +
								"      <abbrev><form lang=\"en\"><text>adv</text></form></abbrev>" + NewLine +
								"      <description><form lang=\"en\"><text>modify verbs</text></form></description>" + NewLine +
								"    </range-element>"));
				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("grammatical-info"), Is.EqualTo("Noun"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "substantives and nominals")),
						  Is.EqualTo(new LiftMultiText("en", "Noun")),
						  Is.EqualTo(new LiftMultiText("en", "n")),
						  IsEqualToIgnoreNl("<range-element guid=\"0fae9a91-36c0-429f-9a31-fbef1292da6a\" id=\"Noun\">" + NewLine +
								"      <label><form lang=\"en\"><text>Noun</text></form></label>" + NewLine +
								"      <abbrev><form lang=\"en\"><text>n</text></form></abbrev>" + NewLine +
								"      <description><form lang=\"en\"><text>substantives and nominals</text></form></description>" + NewLine +
								"    </range-element>"));
				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("grammatical-info"), Is.EqualTo("Verb"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "signal events and actions")),
						  Is.EqualTo(new LiftMultiText("en", "Verb")),
						  Is.EqualTo(new LiftMultiText("en", "v")),
						  IsEqualToIgnoreNl("<range-element guid=\"4812abf3-31e5-450c-a15f-a830dfc7f223\" id=\"Verb\">" + NewLine +
								"      <label><form lang=\"en\"><text>Verb</text></form></label>" + NewLine +
								"      <abbrev><form lang=\"en\"><text>v</text></form></abbrev>" + NewLine +
								"      <description><form lang=\"en\"><text>signal events and actions</text></form></description>" + NewLine +
								"    </range-element>"));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("cv-pattern"),
						  Is.EqualTo(new LiftMultiText("en", "the syllable pattern for a pronunciation")));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("tone"),
						  Is.EqualTo(new LiftMultiText("en", "the tone information for a pronunciation")));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("import-residue"),
						  Is.EqualTo(new LiftMultiText("en", "residue left over from importing")));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("literal-meaning"),
						  Is.EqualTo(new LiftMultiText("en", "literal meaning of an entry")));
				ExpectGetOrMakeEntry(new ExtensibleMatcher("bird_6db30a98-530e-4614-86d4-237f6984db71",
														   new Guid("6db30a98-530e-4614-86d4-237f6984db71"),
														   new DateTime(2008, 3, 31, 8, 4, 9, DateTimeKind.Utc),
														   new DateTime(2008, 3, 31, 8, 4, 9, DateTimeKind.Utc)));
				Expect.Exactly(1).On(_merger).Method("MergeInLexemeForm")
					.With(Is.Anything, Is.EqualTo(new LiftMultiText("x-rtl", "bird")));
				ExpectGetOrMakeSense();
				Expect.Exactly(1).On(_merger).Method("MergeInGrammaticalInfo")
					.With(Is.Anything, Is.EqualTo("Noun"), Is.NotNull);
				Expect.Exactly(1).On(_merger).Method("MergeInGloss")
					.With(Is.Anything, Is.EqualTo(new LiftMultiText("en", "bird")));
				Expect.Exactly(1).On(_merger).Method("MergeInTrait")
					.With(Is.Anything, Is.EqualTo(new Trait("morph-type", "stem")));
				Expect.Exactly(1).On(_merger).Method("MergeInTrait")
					.With(Is.Anything, Is.EqualTo(new Trait("entry-type", "Main Entry")));
				ExpectFinishEntry();
			}

			var cwd = Environment.CurrentDirectory;
			var liftFilePath = Path.Combine(cwd, "test20080407.lift");
			var liftRangesFilePath = Path.Combine(cwd, "test20080407.lift-ranges");
			try
			{
				File.WriteAllBytes(liftFilePath, Resources.test20080407_lift);
				File.WriteAllBytes(liftRangesFilePath, Resources.test20080407_lift_ranges);
				_parser.ReadLiftFile("test20080407.lift");
				_mocks.VerifyAllExpectationsHaveBeenMet();
			}
			finally
			{
				RobustFile.Delete(liftFilePath);
				RobustFile.Delete(liftRangesFilePath);
			}
		}

		[Test]
		public void ReadLiftRangesFileWithEmptyRange()
		{
			using (_mocks.Ordered)
			{
				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("list"), Is.EqualTo("1"), Is.Null, Is.Null,
						Is.EqualTo(new LiftMultiText()),
						Is.EqualTo(new LiftMultiText("en", "first")),
						Is.EqualTo(new LiftMultiText("en", "1st")),
						IsEqualToIgnoreNl("<range-element id=\"1\"><label><form lang=\"en\"><text>first</text></form></label><abbrev><form lang=\"en\"><text>1st</text></form></abbrev></range-element>"));
				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("list"), Is.EqualTo("2"), Is.Null, Is.Null,
						Is.EqualTo(new LiftMultiText()),
						Is.EqualTo(new LiftMultiText("en", "second")),
						Is.EqualTo(new LiftMultiText("en", "2nd")),
						IsEqualToIgnoreNl("<range-element id=\"2\"><label><form lang=\"en\"><text>second</text></form></label><abbrev><form lang=\"en\"><text>2nd</text></form></abbrev></range-element>"));
			}
			using (TempFile r = new TempFile("<lift-ranges>" +
				"<range id=\"empty\"/>" +
				"<range id=\"list\">" +
				"<range-element id=\"1\"><label><form lang=\"en\"><text>first</text></form></label><abbrev><form lang=\"en\"><text>1st</text></form></abbrev></range-element>" +
				"<range-element id=\"2\"><label><form lang=\"en\"><text>second</text></form></label><abbrev><form lang=\"en\"><text>2nd</text></form></abbrev></range-element>" +
				"</range>" +
				"</lift-ranges>"))
			using (TempFile f = new TempFile(String.Format("<lift version='{0}'><header><ranges><range href=\"file://{1}\" id=\"empty\"/><range href=\"file://{1}\" id=\"list\"/></ranges></header></lift>",
				Validator.LiftVersion, r.Path)))
			{
				_parser.ReadLiftFile(f.Path);
			}
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}
		/*
		 *
		/// <summary>
		/// when I wrote the flex exporter, lift did not yet implement semantic domain
		/// </summary>
		[Test, Ignore("Not yet implemented in WeSay")]
		public void SemanticDomainTraitIsBroughtInCorrectly()
		{
			_doc.LoadXml("<trait range=\"semantic-domain\" value=\"6.5.1.1\"/>");
			//TODO   _importer.ReadTrait(_doc.SelectSingleNode("wrap"));
		}

		/// <summary>
		/// when I wrote the flex exporter, lift did not yet implement part of speech
		/// </summary>
		[Test, Ignore("Not yet implemented in WeSay")]
		public void GrammiWithTextLabel()
		{
			_doc.LoadXml("<sense><grammi type=\"conc\"/></sense>");
			//TODO   _importer.ReadSense(_doc.SelectSingleNode("sense"));
		}

		/// <summary>
		/// when I wrote the flex exporter, lift did not yet implement part of speech
		/// </summary>
		[Test, Ignore("Not yet implemented in WeSay")]
		public void GrammiWithEmptyLabel()
		{
			_doc.LoadXml("<sense><grammi type=\"\"/></sense>");
			//TODO   _importer.ReadSense(_doc.SelectSingleNode("sense"));
		}


		 * */

		//private void ParseAndCheck(string content, string expectedResults)
		//{
		//    _doc.LoadXml(content);
		//    _parser.ReadFile(_doc);
		//    Assert.AreEqual(expectedResults, _results.ToString());
		//}

//        private void ParseEntryAndCheck(string content, string expectedResults)
//        {
//            _doc.LoadXml(content);
//            _parser.ReadEntry(_doc.FirstChild);
//            Assert.AreEqual(expectedResults, _results.ToString());
//        }
	}

	public class DummyBase
	{
	}

	public class Dummy : DummyBase
	{
		public override string ToString()
		{
			return "m";
		}
	}

	public class DummyLiftChangeDetector : ILiftChangeDetector
	{
		private bool _haveCache = false;

		public DummyLiftChangeDetector()
		{
			Reset();
		}
		public void Reset()
		{
			_haveCache = true;
		}

		public void ClearCache()
		{
			_haveCache = false;
		}

		public bool CanProvideChangeRecord
		{
			get { return _haveCache; }
		}

		public ILiftChangeReport GetChangeReport(IProgress progress)
		{
			return new DummyChangeReport();
		}
	}

	class DummyChangeReport : ILiftChangeReport
	{
		public LiftChangeReport.ChangeType GetChangeType(string entryId)
		{
			switch (entryId)
			{
				case "new":
					return LiftChangeReport.ChangeType.New;

				case "old":
					return LiftChangeReport.ChangeType.None;

				case "changed":
					return LiftChangeReport.ChangeType.Editted;
				default:
					return LiftChangeReport.ChangeType.None;
			}
		}

		public IList<string> IdsOfDeletedEntries
		{
			get { throw new System.NotImplementedException(); }
		}
	}
}
