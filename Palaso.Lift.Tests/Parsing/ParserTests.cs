using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Palaso.Lift.Merging;
using Palaso.Lift.Parsing;
using Palaso.Lift.Validation;
using NMock2;
using NUnit.Framework;
using Has = NMock2.Has;
using Is = NMock2.Is;

namespace Palaso.Lift.Tests.Parsing
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

		[Test, ExpectedException(typeof(LiftFormatException))]
		public void ReadLiftFile_OldVersion_Throws()
		{
			using (TempFile f = new TempFile(string.Format("<lift version='{0}'></lift>", /*Validator.LiftVersion*/ "0.0")))
			{
				_parser.ReadLiftFile(f.Path);
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
			string when = now.ToString(Extensible.LiftTimeFormatNoTimeZone);
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
					<field name='cvPattern'>
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
			string when= new DateTime(2000,1,1).ToUniversalTime().ToString(Extensible.LiftTimeFormatNoTimeZone);
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
			string when= new DateTime(2000,1,1).ToUniversalTime().ToString(Extensible.LiftTimeFormatNoTimeZone);
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
				"<entry><field name='color'><form lang='en'><text>red</text></form><form lang='es'><text>roco</text></form></field></entry>");
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
				"<entry><sense><field name='color'><form lang='en'><text>red</text></form><form lang='es'><text>roco</text></form></field></sense></entry>");
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
				"<entry><sense><example><field name='color'><form lang='en'><text>red</text></form><form lang='es'><text>roco</text></form></field></example></sense></entry>");
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
				"<entry><field name='color'><form lang='en'><text>red</text></form><form lang='es'><text>roco</text></form></field><field name='special'><form lang='en'><text>free</text></form></field></entry>");
		}




		[Test]
		public void DatesOnFields()
		{

			ExpectEmptyEntry();
			DateTime creat = new DateTime(2000,1,1).ToUniversalTime();
			string createdTime = creat.ToString(Extensible.LiftTimeFormatNoTimeZone);
			DateTime mod = new DateTime(2000, 1, 2).ToUniversalTime();
			string modifiedTime = mod.ToString(Extensible.LiftTimeFormatNoTimeZone);
			ExpectMergeInField(
				Is.EqualTo("color"),
				Is.EqualTo(creat),
				Is.EqualTo(mod),
				Is.Anything,
				Has.Property("Count", Is.EqualTo(0))
				);
			ParseEntryAndCheck(String.Format("<entry><field name='color' dateCreated='{0}'  dateModified='{1}' ></field></entry>",
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
			SimpleCheckWithHeader(InsertVersion("<lift V><header><fields><field-definition name='custom'/></fields></header><entry/></lift>"), 0,1,1);
		}
		[Test]
		public void TwoFields()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><fields><field-definition name='special'/><field-definition name='custom'></field-definition></fields></header><entry/></lift>"), 0, 2, 1);
		}

		[Test]
		public void EmptyFieldNoEntriesOk()
		{
			SimpleCheckWithHeader(InsertVersion("<lift V><header><fields><field-definition name='custom'/></fields></header></lift>"), 0, 1, 0);
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
			File.WriteAllText(path, @"<lift><entry></entry>
				<entry id='foo'/><entry/></lift>");
			int count = LiftParser<DummyBase, Dummy, Dummy, Dummy>.GetEstimatedNumberOfEntriesInFile(path);
			Assert.AreEqual(3, count);
		}

		[Test]
		public void SimpleFieldDefinition()
		{
			string content = "<field-definition name='tone'><description><form lang='en'><text>the tone information for a pronunciation</text></form></description></field-definition>";
			Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
				.With(Is.EqualTo("tone"),
					  Is.EqualTo(null),
					  Is.EqualTo(null),
					  Is.EqualTo(null),
					  Is.EqualTo(null),
					  Is.EqualTo(new LiftMultiText("en", "the tone information for a pronunciation")),
					  Is.EqualTo(new LiftMultiText()));
			_doc.LoadXml(content);
			_parser.ReadFieldDefinition(_doc.FirstChild);
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void SimpleEtymology()
		{
			string content = "<entry><etymology source='Greek' type='borrowed'><form lang='bam'><text>alphabeta</text></form><gloss lang='en'><text>letters</text></gloss><field name='comment'><form lang='en'><text>this etymology is nonsense</text></form></field></etymology></entry>";
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

#if MONO
		const string NewLine = "\r\n";
#else
		const string NewLine = "\n";
		// string NewLine = Environment.NewLine;
		/* review: I (CP) am not clear why there is a difference in line endings between linux and windows.
		 * Linux expects the windows convention \r\n (correctly) where as windows interprets \n to match the \r\n
		 * used in the file.  Environment.NewLine also gives \r\n.  I would expect \r\n to work everywhere.
		 */
#endif

		[Test]
		public void ReadExternalLiftFile()
		{
			// For this test to work, the files test20080407.lift and test20080407.lift-ranges MUST
			// be copied to the current working directory.
			using (_mocks.Ordered)	// Ordered may be too strong if parse details change.
			{
				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("etymology"), Is.EqualTo("borrowed"), Is.Null, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "The word is borrowed from another language")),
						  Is.EqualTo(new LiftMultiText("en", "borrowed")),
						  Is.EqualTo(new LiftMultiText()),
						  Is.EqualTo("<range-element id=\"borrowed\">" + NewLine +
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
						  Is.EqualTo("<range-element id=\"proto\">" + NewLine +
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
						  Is.EqualTo("<range-element guid=\"c528ee72-31a5-423d-833d-0c8454f345d3\" id=\"Adverb\">" + NewLine +
								"      <label><form lang=\"en\"><text>Adverb</text></form></label>" + NewLine +
								"      <abbrev><form lang=\"en\"><text>adv</text></form></abbrev>" + NewLine +
								"      <description><form lang=\"en\"><text>modify verbs</text></form></description>" + NewLine +
								"    </range-element>"));
				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("grammatical-info"), Is.EqualTo("Noun"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "substantives and nominals")),
						  Is.EqualTo(new LiftMultiText("en", "Noun")),
						  Is.EqualTo(new LiftMultiText("en", "n")),
						  Is.EqualTo("<range-element guid=\"0fae9a91-36c0-429f-9a31-fbef1292da6a\" id=\"Noun\">" + NewLine +
								"      <label><form lang=\"en\"><text>Noun</text></form></label>" + NewLine +
								"      <abbrev><form lang=\"en\"><text>n</text></form></abbrev>" + NewLine +
								"      <description><form lang=\"en\"><text>substantives and nominals</text></form></description>" + NewLine +
								"    </range-element>"));
				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("grammatical-info"), Is.EqualTo("Verb"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "signal events and actions")),
						  Is.EqualTo(new LiftMultiText("en", "Verb")),
						  Is.EqualTo(new LiftMultiText("en", "v")),
						  Is.EqualTo("<range-element guid=\"4812abf3-31e5-450c-a15f-a830dfc7f223\" id=\"Verb\">" + NewLine +
								"      <label><form lang=\"en\"><text>Verb</text></form></label>" + NewLine +
								"      <abbrev><form lang=\"en\"><text>v</text></form></abbrev>" + NewLine +
								"      <description><form lang=\"en\"><text>signal events and actions</text></form></description>" + NewLine +
								"    </range-element>"));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("cv-pattern"),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(new LiftMultiText("en", "the syllable pattern for a pronunciation")),
						  Is.EqualTo(new LiftMultiText()));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("tone"),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(new LiftMultiText("en", "the tone information for a pronunciation")),
						  Is.EqualTo(new LiftMultiText()));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("import-residue"),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(new LiftMultiText("en", "residue left over from importing")),
						  Is.EqualTo(new LiftMultiText()));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("literal-meaning"),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(new LiftMultiText("en", "literal meaning of an entry")),
						  Is.EqualTo(new LiftMultiText()));
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
			_parser.ReadLiftFile("test20080407.lift");
			_mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void MigrateAndReadExternalFile()
		{
			// For this test to work, the files test20111130.lift and test20111130.lift-ranges MUST
			// be copied to the current working directory.
			var needed = Lift.Migration.Migrator.IsMigrationNeeded("test20111130.lift");
			Assert.IsTrue(needed, "test20111130.lift needs to be migrated!");
			var path = Lift.Migration.Migrator.MigrateToLatestVersion("test20111130.lift");
			using (_mocks.Ordered)	// Ordered may be too strong if parse details change.
			{
				var mtLex = new LiftMultiText("fng", "tehst");
				mtLex.Add("en", "test");

				var mtGloss = new LiftMultiText("en", "trial");
				mtGloss.Add("pt", "Portuguese trial");

				var mtField = new LiftMultiText("fng", "testier");
				mtField.Add("en", "testingly");
				mtField.Add("pt", "Portuguese something");

				var mtText = new LiftMultiText();
				mtText.AddSpan("en", "en", null, null, 58);
				mtText.AddOrAppend("en", "This is a long text of vernacular data, believe it or not.\u2029", "");
				mtText.AddSpan("en", "en", null, null, 35);
				mtText.AddOrAppend("en", "I don't personally believe, myself.", "");

				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("grammatical-info"), Is.EqualTo("Noun"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "A noun is a broad classification of parts of speech which include substantives and nominals.")),
						  Is.EqualTo(new LiftMultiText("en", "Noun")),
						  Is.EqualTo(new LiftMultiText("en", "n")),
						  Is.EqualTo("<range-element id=\"Noun\" guid=\"c1e5f352-aece-4473-a726-aadae20520a1\">" + NewLine +
								 "<label><form lang=\"en\"><text>Noun</text></form></label>" + NewLine +
								 "<abbrev><form lang=\"en\"><text>n</text></form></abbrev>" + NewLine +
								 "<description><form lang=\"en\"><text>A noun is a broad classification of parts of speech which include substantives and nominals.</text></form></description>" + NewLine +
								 "</range-element>"));

				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("grammatical-info"), Is.EqualTo("Verb"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "A verb is a part of speech whose members typically signal events and actions.")),
						  Is.EqualTo(new LiftMultiText("en", "Verb")),
						  Is.EqualTo(new LiftMultiText("en", "v")),
						  Is.EqualTo("<range-element id=\"Verb\" guid=\"907d0751-1ae5-45dc-a6ef-7f90b1c8f446\">" + NewLine +
								 "<label><form lang=\"en\"><text>Verb</text></form></label>" + NewLine +
								 "<abbrev><form lang=\"en\"><text>v</text></form></abbrev>" + NewLine +
								 "<description><form lang=\"en\"><text>A verb is a part of speech whose members typically signal events and actions.</text></form></description>" + NewLine +
								 "</range-element>"));

				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("morph-type"), Is.EqualTo("prefix"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "A prefix is an affix that is joined before a root or stem.")),
						  Is.EqualTo(new LiftMultiText("en", "prefix")),
						  Is.EqualTo(new LiftMultiText("en", "pfx")),
						  Is.EqualTo("<range-element id=\"prefix\" guid=\"d7f713db-e8cf-11d3-9764-00c04f186933\">" + NewLine +
								 "<label><form lang=\"en\"><text>prefix</text></form></label>" + NewLine +
								 "<abbrev><form lang=\"en\"><text>pfx</text></form></abbrev>" + NewLine +
								 "<description><form lang=\"en\"><text>A prefix is an affix that is joined before a root or stem.</text></form></description>" + NewLine +
								 "<trait name=\"trailing-symbol\" value=\"-\" />" + NewLine +
								 "</range-element>"));

				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("morph-type"), Is.EqualTo("suffix"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "A suffix is an affix that is attached to the end of a root or stem.")),
						  Is.EqualTo(new LiftMultiText("en", "suffix")),
						  Is.EqualTo(new LiftMultiText("en", "sfx")),
						  Is.EqualTo("<range-element id=\"suffix\" guid=\"d7f713dd-e8cf-11d3-9764-00c04f186933\">" + NewLine +
								 "<label><form lang=\"en\"><text>suffix</text></form></label>" + NewLine +
								 "<abbrev><form lang=\"en\"><text>sfx</text></form></abbrev>" + NewLine +
								 "<description><form lang=\"en\"><text>A suffix is an affix that is attached to the end of a root or stem.</text></form></description>" + NewLine +
								 "<trait name=\"leading-symbol\" value=\"-\" />" + NewLine +
								 "</range-element>"));

				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("morph-type"), Is.EqualTo("root"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "A root is the portion of a word that is not further analyzable and that carries the principle portion of meaning.")),
						  Is.EqualTo(new LiftMultiText("en", "root")),
						  Is.EqualTo(new LiftMultiText("en", "ubd root")),
						  Is.EqualTo("<range-element id=\"root\" guid=\"d7f713e5-e8cf-11d3-9764-00c04f186933\">" + NewLine +
								 "<label><form lang=\"en\"><text>root</text></form></label>" + NewLine +
								 "<abbrev><form lang=\"en\"><text>ubd root</text></form></abbrev>" + NewLine +
								 "<description><form lang=\"en\"><text>A root is the portion of a word that is not further analyzable and that carries the principle portion of meaning.</text></form></description>" + NewLine +
								 "</range-element>"));

				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("genres"), Is.EqualTo("Monologue"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "A text with a single participant, typically a narrator addressing an audience")),
						  Is.EqualTo(new LiftMultiText("en", "Monologue")),
						  Is.EqualTo(new LiftMultiText("en", "mnlg")),
						  Is.EqualTo("<range-element id=\"Monologue\" guid=\"c076f554-ea5e-11de-9793-0013722f8dec\">" + NewLine +
								 "<label><form lang=\"en\"><text>Monologue</text></form></label>" + NewLine +
								 "<abbrev><form lang=\"en\"><text>mnlg</text></form></abbrev>" + NewLine +
								 "<description><form lang=\"en\"><text>A text with a single participant, typically a narrator addressing an audience</text></form></description>" + NewLine +
								 "</range-element>"));

				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("genres"), Is.EqualTo("Narrative"), Is.NotNull, Is.EqualTo("Monologue"),
						  Is.EqualTo(new LiftMultiText("en", "An account of events, a text that describes or projects a contingent succession of actions")),
						  Is.EqualTo(new LiftMultiText("en", "Narrative")),
						  Is.EqualTo(new LiftMultiText("en", "nar")),
						  Is.EqualTo("<range-element id=\"Narrative\" guid=\"c082e102-ea5e-11de-92f2-0013722f8dec\" parent=\"Monologue\">" + NewLine +
								 "<label><form lang=\"en\"><text>Narrative</text></form></label>" + NewLine +
								 "<abbrev><form lang=\"en\"><text>nar</text></form></abbrev>" + NewLine +
								 "<description><form lang=\"en\"><text>An account of events, a text that describes or projects a contingent succession of actions</text></form></description>" + NewLine +
								 "</range-element>"));

				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("genres"), Is.EqualTo("Dialogue"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText("en", "Literal speech between individuals or quoted text that uses quotation formulas (typically using the verb 'to say')")),
						  Is.EqualTo(new LiftMultiText("en", "Dialogue")),
						  Is.EqualTo(new LiftMultiText("en", "dia")),
						  Is.EqualTo("<range-element id=\"Dialogue\" guid=\"c13f39e2-ea5e-11de-9645-0013722f8dec\">" + NewLine +
								 "<label><form lang=\"en\"><text>Dialogue</text></form></label>" + NewLine +
								 "<abbrev><form lang=\"en\"><text>dia</text></form></abbrev>" + NewLine +
								 "<description><form lang=\"en\"><text>Literal speech between individuals or quoted text that uses quotation formulas (typically using the verb 'to say')</text></form></description>" + NewLine +
								 "</range-element>"));

				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("education"), Is.EqualTo("No Formal"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText()),
						  Is.EqualTo(new LiftMultiText("en", "No Formal")),
						  Is.EqualTo(new LiftMultiText("en", "No")),
						  Is.EqualTo("<range-element id=\"No Formal\" guid=\"bda22e02-ea5e-11de-9452-0013722f8dec\">" + NewLine +
								 "<label><form lang=\"en\"><text>No Formal</text></form></label>" + NewLine +
								 "<abbrev><form lang=\"en\"><text>No</text></form></abbrev>" + NewLine +
								 "</range-element>"));

				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("education"), Is.EqualTo("High School"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText()),
						  Is.EqualTo(new LiftMultiText("en", "High School")),
						  Is.EqualTo(new LiftMultiText("en", "HS")),
						  Is.EqualTo("<range-element id=\"High School\" guid=\"bdc38ec6-ea5e-11de-9902-0013722f8dec\">" + NewLine +
								 "<label><form lang=\"en\"><text>High School</text></form></label>" + NewLine +
								 "<abbrev><form lang=\"en\"><text>HS</text></form></abbrev>" + NewLine +
								 "</range-element>"));

				Expect.Exactly(1).On(_merger).Method("ProcessRangeElement")
					.With(Is.EqualTo("education"), Is.EqualTo("Professor"), Is.NotNull, Is.Null,
						  Is.EqualTo(new LiftMultiText()),
						  Is.EqualTo(new LiftMultiText("en", "Professor")),
						  Is.EqualTo(new LiftMultiText("en", "Prof")),
						  Is.EqualTo("<range-element id=\"Professor\" guid=\"bde4ef8a-ea5e-11de-8623-0013722f8dec\">" + NewLine +
								 "<label><form lang=\"en\"><text>Professor</text></form></label>" + NewLine +
								 "<abbrev><form lang=\"en\"><text>Prof</text></form></abbrev>" + NewLine +
								 "</range-element>"));

				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("import-residue"),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(new LiftMultiText("en", "This records residue left over from importing a standard format file into FieldWorks (or LinguaLinks).")),
						  Is.EqualTo(new LiftMultiText()));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("Entry String"),
						  Is.EqualTo("entry"),
						  Is.EqualTo("string"),
						  Is.EqualTo(null),
						  Is.EqualTo("en"),
						  Is.EqualTo(new LiftMultiText("en", "This is a test.")),
						  Is.EqualTo(new LiftMultiText()));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("Entry Text"),
						  Is.EqualTo("entry"),
						  Is.EqualTo("multitext"),
						  Is.EqualTo(null),
						  Is.EqualTo("fng"),
						  Is.EqualTo(new LiftMultiText("en", "This is a test.")),
						  Is.EqualTo(new LiftMultiText()));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("Entry List Item"),
						  Is.EqualTo("entry"),
						  Is.EqualTo("option"),
						  Is.EqualTo("genres"),
						  Is.EqualTo(null),
						  Is.EqualTo(new LiftMultiText("en", "This references the Genres list.")),
						  Is.EqualTo(new LiftMultiText()));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("Sense List Items"),
						  Is.EqualTo("sense"),
						  Is.EqualTo("option-collection"),
						  Is.EqualTo("education"),
						  Is.EqualTo(null),
						  Is.EqualTo(new LiftMultiText("en", "The education levels using this sense implies.")),
						  Is.EqualTo(new LiftMultiText()));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("Sense multilingual string"),
						  Is.EqualTo("sense"),
						  Is.EqualTo("multistring"),
						  Is.EqualTo(null),
						  Is.EqualTo("fng en pt"),
						  Is.EqualTo(new LiftMultiText("en", "all vernacular and analysis writing systems")),
						  Is.EqualTo(new LiftMultiText()));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("Sense date"),
						  Is.EqualTo("sense"),
						  Is.EqualTo("gendate"),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(new LiftMultiText("en", "dating something or other")),
						  Is.EqualTo(new LiftMultiText()));
				Expect.Exactly(1).On(_merger).Method("ProcessFieldDefinition")
					.With(Is.EqualTo("Allomorph Number"),
						  Is.EqualTo("variant"),
						  Is.EqualTo("integer"),
						  Is.EqualTo(null),
						  Is.EqualTo(null),
						  Is.EqualTo(new LiftMultiText("en", "numbering away")),
						  Is.EqualTo(new LiftMultiText()));

				ExpectGetOrMakeEntry(new ExtensibleMatcher("tist_b4e4618c-9c89-4afb-96b3-9e2ebea01ffb",
														   new Guid("b4e4618c-9c89-4afb-96b3-9e2ebea01ffb"),
														   new DateTime(2011, 11, 22, 22, 25, 44, DateTimeKind.Utc),
														   new DateTime(2011, 11, 30, 16,  5, 51, DateTimeKind.Utc)));
				Expect.Exactly(1).On(_merger).Method("MergeInLexemeForm")
					.With(Is.Anything,
						  Is.EqualTo(mtLex));
				Expect.Exactly(1).On(_merger).Method("MergeInCitationForm")
					.With(Is.Anything,
						  Is.EqualTo(new LiftMultiText("fng", "tist")));
				Expect.Exactly(1).On(_merger).Method("MergeInNote")
					.With(Is.Anything,
						  Is.EqualTo("summary-definition"),
						  Is.EqualTo(new LiftMultiText("en", "summarily speaking ...")),
						  Is.EqualTo("<note type=\"summary-definition\">" + NewLine +
									 "<form lang=\"en\"><text>summarily speaking ...</text></form>" + NewLine +
									 "</note>"));
				ExpectGetOrMakeSense();
				Expect.Exactly(1).On(_merger).Method("MergeInGrammaticalInfo")
					.With(Is.Anything,
						  Is.EqualTo("Noun"), Is.NotNull);
				Expect.Exactly(1).On(_merger).Method("MergeInGloss")
					.With(Is.Anything,
						  Is.EqualTo(mtGloss));
				Expect.Exactly(1).On(_merger).Method("MergeInDefinition")
					.With(Is.Anything,
						  Is.EqualTo(new LiftMultiText("en", "an attempt to do something")));
				Expect.Exactly(1).On(_merger).Method("MergeInNote")
					.With(Is.Anything,
						  Is.EqualTo("scientific-name"),
						  Is.EqualTo(new LiftMultiText("en", "trialate")),
						  Is.EqualTo("<note type=\"scientific-name\">" + NewLine +
									 "<form lang=\"en\"><text>trialate</text></form>" + NewLine +
									 "</note>"));
				Expect.Exactly(1).On(_merger).Method("MergeInField")
					.With(Is.Anything,
						  Is.EqualTo("Sense multilingual string"),
						  Is.Anything, Is.Anything,
						  Is.EqualTo(mtField),
						  Is.EqualTo(new List<Trait>()));
				Expect.Exactly(1).On(_merger).Method("MergeInTrait")
					.With(Is.Anything,
						  Is.EqualTo(new Trait("Sense List Items", "No Formal")));
				Expect.Exactly(1).On(_merger).Method("MergeInTrait")
					.With(Is.Anything,
						  Is.EqualTo(new Trait("Sense List Items", "Professor")));
				Expect.Exactly(1).On(_merger).Method("MergeInVariant")
					.With(Is.Anything,
						  Is.EqualTo(new LiftMultiText("fng", "tess")),
						  Is.EqualTo("<variant>" + NewLine +
									 "<form lang=\"fng\"><text>tess</text></form>" + NewLine +
									 "<trait name=\"environment\" value=\"/_d\" />" + NewLine +
									 "<trait name=\"morph-type\" value=\"root\" />" + NewLine +
									 "<trait name=\"Allomorph Number\" value=\"42\" />" + NewLine +
									 "</variant>")
									 )
					.Will(Return.Value(new Dummy()));
				Expect.Exactly(1).On(_merger).Method("MergeInTrait")
					.With(Is.Anything, Is.EqualTo(new Trait("environment", "/_d")));
				Expect.Exactly(1).On(_merger).Method("MergeInTrait")
					.With(Is.Anything, Is.EqualTo(new Trait("morph-type", "root")));
				Expect.Exactly(1).On(_merger).Method("MergeInTrait")
					.With(Is.Anything, Is.EqualTo(new Trait("Allomorph Number", "42")));
				Expect.Exactly(1).On(_merger).Method("MergeInField")
					.With(Is.Anything,
						  Is.EqualTo("Entry String"),
						  Is.Anything, Is.Anything,
						  Is.EqualTo(new LiftMultiText("en", "test")),
						  Is.EqualTo(new List<Trait>()));
				Expect.Exactly(1).On(_merger).Method("MergeInField")
					.With(Is.Anything,
						  Is.EqualTo("Entry Text"),
						  Is.Anything, Is.Anything,
						  Is.EqualTo(mtText),
						  Is.EqualTo(new List<Trait>()));
				Expect.Exactly(1).On(_merger).Method("MergeInTrait")
					.With(Is.Anything, Is.EqualTo(new Trait("morph-type", "root")));
				Expect.Exactly(1).On(_merger).Method("MergeInTrait")
					.With(Is.Anything, Is.EqualTo(new Trait("Entry List Item", "Monologue")));
				ExpectFinishEntry();
			}
			var count = _parser.ReadLiftFile(path);
			Assert.AreEqual(1, count, "test20111130.lift has one entry.");
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
