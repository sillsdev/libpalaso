using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Palaso.TestUtilities;
using SIL.Data;
using SIL.IO;
using SIL.Keyboarding;
using SIL.WritingSystems.Migration;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using SIL.Xml;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class LdmlDataMapperTests
	{
		private WritingSystemDefinition _ws;
		private static readonly XNamespace Sil = "urn://www.sil.org/ldml/0.1";

		private class TestEnvironment : IDisposable
		{
			public TestEnvironment()
			{
				FolderContainingLdml = new TemporaryFolder("LdmlDataMapperTests");
				NamespaceManager = new XmlNamespaceManager(new NameTable());
				NamespaceManager.AddNamespace("sil", "urn://www.sil.org/ldml/0.1");
			}

			public XmlNamespaceManager NamespaceManager { get; private set; }

			private TemporaryFolder FolderContainingLdml { get; set; }

			public void Dispose()
			{
				FolderContainingLdml.Dispose();
			}

			public string FilePath(string fileName)
			{
				return Path.Combine(FolderContainingLdml.Path, fileName);
			}
		}

		[SetUp]
		public void SetUp()
		{
			_ws = new WritingSystemDefinition("en", "Latn", "US", string.Empty, "eng", false);
		}

		[Test]
		public void ReadFromFile_NullFileName_Throws()
		{
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			Assert.Throws<ArgumentNullException>(
				() => adaptor.Read((string)null, _ws)
			);
		}

		[Test]
		public void ReadFromFile_NullWritingSystem_Throws()
		{
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			Assert.Throws<ArgumentNullException>(
				() => adaptor.Read("foo.ldml", null)
			);
		}

		[Test]
		public void ReadFromXmlReader_NullXmlReader_Throws()
		{
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			Assert.Throws<ArgumentNullException>(
				() => adaptor.Read((XmlReader)null, _ws)
			);
		}

		[Test]
		public void ReadFromXmlReader_NullWritingSystem_Throws()
		{
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			Assert.Throws<ArgumentNullException>(
				() => adaptor.Read(XmlReader.Create(new StringReader("<ldml/>")), null)
			);
		}

		[Test]
		public void WriteToFile_NullFileName_Throws()
		{
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			Assert.Throws<ArgumentNullException>(
				() => adaptor.Write((string)null, _ws, null)
			);
		}

		[Test]
		public void WriteToFile_NullWritingSystem_Throws()
		{
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			Assert.Throws<ArgumentNullException>(
				() => adaptor.Write("foo.ldml", null, null)
			);
		}

		[Test]
		public void WriteToXmlWriter_NullXmlReader_Throws()
		{
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			Assert.Throws<ArgumentNullException>(
				() => adaptor.Write((XmlWriter)null, _ws, null)
			);
		}

		[Test]
		public void WriteToXmlWriter_NullWritingSystem_Throws()
		{
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			Assert.Throws<ArgumentNullException>(
				() => adaptor.Write(XmlWriter.Create(new MemoryStream()), null, null)
			);
		}

		[Test]
		public void WriteSetsRequiresValidTagToTrue()
		{
			var ws = new WritingSystemDefinition();
			ws.RequiresValidLanguageTag = false;
			ws.Language = "InvalidLanguage";
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			var sw = new StringWriter();
			var writer = XmlWriter.Create(sw, CanonicalXmlSettings.CreateXmlWriterSettings());
			Assert.Throws<ValidationException>(() => adaptor.Write(writer, ws, null));
		}

		[Test]
		public void ExistingUnusedLdml_Write_PreservesData()
		{
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			var sw = new StringWriter();
			var ws = new WritingSystemDefinition("en");
			var writer = XmlWriter.Create(sw, CanonicalXmlSettings.CreateXmlWriterSettings());
			adaptor.Write(writer, ws, XmlReader.Create(new StringReader("<ldml><!--Comment--><dates/><special>hey</special></ldml>")));
			writer.Close();
			AssertThatXmlIn.String(sw.ToString()).HasAtLeastOneMatchForXpath("/ldml/special[text()=\"hey\"]");
		}

		#region Roundtrip
		[Test]
		public void RoundtripSimpleCustomSortRules_WS33715()
		{
			var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());

			const string sortRules = "(A̍ a̍)";
			var cd = new SimpleCollationDefinition("standard") {SimpleRules = sortRules};
			var wsWithSimpleCustomSortRules = new WritingSystemDefinition();
			wsWithSimpleCustomSortRules.Collations.Add(cd);

			var wsFromLdml = new WritingSystemDefinition();
			using (var tempFile = new TempFile())
			{
				ldmlAdaptor.Write(tempFile.Path, wsWithSimpleCustomSortRules, null);
				ldmlAdaptor.Read(tempFile.Path, wsFromLdml);
			}

			var cdFromLdml = (SimpleCollationDefinition) wsFromLdml.Collations.First();
			Assert.AreEqual(sortRules, cdFromLdml.SimpleRules);
		}

		[Test]
		public void RoundtripKnownKeyboards()
		{
			var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());

			//Keyboard.Controller = new MyKeyboardController();

			var wsWithKnownKeyboards = new WritingSystemDefinition();
			// ID name layout local available
			var keyboard1 = new DefaultKeyboardDefinition("en-US_MyFavoriteKeyboard", "MyFavoriteKeyboard", "MyFavoriteKeyboard", "en-US", true);
			keyboard1.Format = KeyboardFormat.Msklc;
			wsWithKnownKeyboards.KnownKeyboards.Add(keyboard1);

			var keyboard2 = new DefaultKeyboardDefinition("en-GB_SusannasFavoriteKeyboard", "SusannasFavoriteKeyboard", "SusannasFavoriteKeyboard", "en-GB", true);
			keyboard2.Format = KeyboardFormat.Msklc;
			wsWithKnownKeyboards.KnownKeyboards.Add(keyboard2);

			var wsFromLdml = new WritingSystemDefinition();
			using (var tempFile = new TempFile())
			{
				ldmlAdaptor.Write(tempFile.Path, wsWithKnownKeyboards, null);
				ldmlAdaptor.Read(tempFile.Path, wsFromLdml);
			}

			var knownKeyboards = wsFromLdml.KnownKeyboards.ToList();
			Assert.That(knownKeyboards, Has.Count.EqualTo(2), "restored WS should have known keyboards");
			var keyboard1FromLdml = knownKeyboards[0];
			Assert.That(keyboard1FromLdml.Layout, Is.EqualTo("MyFavoriteKeyboard"));
			Assert.That(keyboard1FromLdml.Locale, Is.EqualTo("en-US"));

			var keyboard2FromLdml = knownKeyboards[1];
			Assert.That(keyboard2FromLdml.Layout, Is.EqualTo("SusannasFavoriteKeyboard"));
			Assert.That(keyboard2FromLdml.Locale, Is.EqualTo("en-GB"));
		}

		[Test]
		public void Roundtrip_LdmlIdentity()
		{
			using (var environment = new TestEnvironment())
			{
				var wsToLdml = new WritingSystemDefinition("en", "Latn", "GB", "x-test")
				{
					VersionNumber = "$Revision$",
					VersionDescription = "Identity version description",
					WindowsLcid = "1036",
					DefaultRegion = "US"
				};
				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());

				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='GB']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-test']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/version[@number='$Revision$' and text()='Identity version description']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@windowsLCID='1036' and @defaultRegion='US']", environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.VersionNumber, Is.EqualTo("$Revision$"));
				Assert.That(wsFromLdml.VersionDescription, Is.EqualTo("Identity version description"));
				Assert.That(wsFromLdml.IetfLanguageTag, Is.EqualTo("en-GB-x-test"));
				Assert.That(wsFromLdml.WindowsLcid, Is.EqualTo("1036"));
				Assert.That(wsFromLdml.DefaultRegion, Is.EqualTo("US"));
			}
		}

		[Test]
		public void Roundtrip_LdmlLayout()
		{
			using (var environment = new TestEnvironment())
			{
				// Write/Read RightToLeftScript is false
				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "")
				{
					RightToLeftScript = false
				};
				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/layout/orientation/characterOrder[text()='left-to-right']");

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.RightToLeftScript, Is.False);

				// Write/Read RightToLeftScript is true
				wsToLdml.RightToLeftScript = true;
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/layout/orientation/characterOrder[text()='right-to-left']");

				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.RightToLeftScript, Is.True);
			}
		}

		// This tests characters and numbers elements
		[Test]
		public void Roundtrip_LdmlCharacters()
		{
			using (var environment = new TestEnvironment())
			{
				var index = new CharacterSetDefinition("index");
				for (int i = 'A'; i <= (int) 'Z'; i++)
					index.Characters.Add(((char) i).ToString(CultureInfo.InvariantCulture));
				index.Characters.Add("AZ");

				var main = new CharacterSetDefinition("main");
				for (int i = 'a'; i <= (int) 'z'; i++)
					main.Characters.Add(((char) i).ToString(CultureInfo.InvariantCulture));
				main.Characters.Add("az");

				var footnotes = new CharacterSetDefinition("footnotes");
				const string footnotesString = "- ‐ – — , ; : ! ? . … ' ‘ ’ \" “ ” ( ) [ ] § @ * / & # † ‡ ′ ″";
				foreach (string str in footnotesString.Split(' '))
					footnotes.Characters.Add(str);

				var numeric = new CharacterSetDefinition("numeric");
				const string numericString = "๐ ๑ ๒ ๓ ๔ ๕ ๖ ๗ ๘ ๙";
				foreach (string str in numericString.Split(' '))
					numeric.Characters.Add(str);

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.CharacterSets.Add(index);
				wsToLdml.CharacterSets.Add(main);
				wsToLdml.CharacterSets.Add(footnotes);
				wsToLdml.CharacterSets.Add(numeric);

				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/characters/exemplarCharacters[@type='index' and text()='[A-Z{AZ}]']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/characters/exemplarCharacters[text()='[a-z{az}]']", environment.NamespaceManager);
				// Character set in XPath is escaped differently from the actual file
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/characters/special/sil:exemplarCharacters[@type='footnotes' and text()='[!-#\\&-*,-/\\:;?@\\[\\]\\u00A7\\u2010\\u2013\\u2014\\u2018\\u2019\\u201C\\u201D\\u2020\\u2021\\u2026\\u2032\\u2033]']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/numbers/defaultNumberingSystem[text()='standard']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/numbers/numberingSystem[@id='standard' and @type='numeric' and @digits='๐๑๒๓๔๕๖๗๘๙']", environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.CharacterSets["index"].ValueEquals(index));
				Assert.That(wsFromLdml.CharacterSets["main"].ValueEquals(main));
				Assert.That(wsFromLdml.CharacterSets["footnotes"].ValueEquals(footnotes));
				Assert.That(wsFromLdml.CharacterSets["numeric"].ValueEquals(numeric));
			}
		}

		[Test]
		public void Roundtrip_LdmlAlternateCharacters()
		{
			using (var environment = new TestEnvironment())
			{
				var altMain = new CharacterSetDefinition("main\ufdd0capital");
				for (int i = 'A'; i <= (int) 'Z'; i++)
					altMain.Characters.Add(((char) i).ToString(CultureInfo.InvariantCulture));
				altMain.Characters.Add("AZ");

				var main = new CharacterSetDefinition("main");
				for (int i = 'a'; i <= (int) 'z'; i++)
					main.Characters.Add(((char) i).ToString(CultureInfo.InvariantCulture));
				main.Characters.Add("az");

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.CharacterSets.Add(altMain);
				wsToLdml.CharacterSets.Add(main);

				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/characters/exemplarCharacters[@alt='capital' and text()='[A-Z{AZ}]']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/characters/exemplarCharacters[text()='[a-z{az}]']", environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.CharacterSets["main\ufdd0capital"].ValueEquals(altMain));
				Assert.That(wsFromLdml.CharacterSets["main"].ValueEquals(main));
			}
		}

		[Test]
		public void Roundtrip_LdmlDelimiters()
		{
			using (var environment = new TestEnvironment())
			{
				var mp = new MatchedPair("mpOpen1", "mpClose2", false);
				var pp = new PunctuationPattern("pattern1", PunctuationPatternContext.Medial);
				// Quotation Marks:
				// Level 1 normal quotation marks (quotationStart and quotationEnd)
				// Level 2 normal quotation marks (alternateQuotationStart and alternateQuotationEnd)
				// Level 3 normal quotation marks (special: sil:quotation-marks)
				// Level 1 narrative quotation marks (special: sil:quotation-marks)
				var qm1 = new QuotationMark("\"", "\"", null, 1, QuotationMarkingSystemType.Normal);
				var qm2 = new QuotationMark("{", "}", null, 2, QuotationMarkingSystemType.Normal);
				var qm3 = new QuotationMark("open1", "close2", "cont3", 3, QuotationMarkingSystemType.Normal);
				var qm4 = new QuotationMark("", null, null, 1, QuotationMarkingSystemType.Narrative);

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.MatchedPairs.Add(mp);
				wsToLdml.PunctuationPatterns.Add(pp);
				wsToLdml.QuotationMarks.Add(qm1);
				wsToLdml.QuotationMarks.Add(qm2);
				wsToLdml.QuotationMarks.Add(qm3);
				wsToLdml.QuotationMarks.Add(qm4);
				wsToLdml.QuotationParagraphContinueType = QuotationParagraphContinueType.Outermost;
	
				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/delimiters/special/sil:matched-pairs/sil:matched-pair[@open='mpOpen1' and @close='mpClose2' and @paraClose='false']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/delimiters/special/sil:punctuation-patterns/sil:punctuation-pattern[@pattern='pattern1' and @context='medial']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/delimiters/quotationStart[text()='\"']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/delimiters/quotationEnd[text()='\"']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/delimiters/alternateQuotationStart[text()='{']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/delimiters/alternateQuotationEnd[text()='}']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/delimiters/special/sil:quotation-marks/sil:quotation[@open='open1' and @close='close2' and @continue='cont3' and @level='3']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/delimiters/special/sil:quotation-marks/sil:quotation[@open and string-length(@open)=0 and @level='1' and @type='narrative']", environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.MatchedPairs.FirstOrDefault(), Is.EqualTo(mp));
				Assert.That(wsFromLdml.PunctuationPatterns.FirstOrDefault(), Is.EqualTo(pp));
				Assert.That(wsFromLdml.QuotationParagraphContinueType, Is.EqualTo(QuotationParagraphContinueType.Outermost));
				Assert.That(wsFromLdml.QuotationMarks[0], Is.EqualTo(qm1));
				Assert.That(wsFromLdml.QuotationMarks[1], Is.EqualTo(qm2));
				Assert.That(wsFromLdml.QuotationMarks[2], Is.EqualTo(qm3));
				Assert.That(wsFromLdml.QuotationMarks[3], Is.EqualTo(qm4));
			}
		}

		//WS-33992 : Test removed since empty collations are removed

		// For the collation tests below, we use XElement load to verify the LDML write
		// of icu rules because Xpath is inadequate for checking CDATA sections

		[Test]
		public void Roundtrip_LdmlStandardCollation()
		{
			using (var environment = new TestEnvironment())
			{
				const string icuRules =
					"&B<t<<<T<s<<<S<e<<<E\r\n\t\t\t\t&C<k<<<K<x<<<X<i<<<I\r\n\t\t\t\t&D<q<<<Q<r<<<R\r\n\t\t\t\t&G<o<<<O\r\n\t\t\t\t&W<h<<<H";
				var cd = new IcuCollationDefinition("standard")
				{
					IcuRules = icuRules,
					CollationRules = icuRules,
					IsValid = true
				};

				var wsToLdml = new WritingSystemDefinition("aa", "Latn", "", "");
				wsToLdml.Collations.Add(cd);
				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);

				XElement ldmlElem = XElement.Load(environment.FilePath("test.ldml"));
				XElement collationsElem = ldmlElem.Element("collations");
				XElement defaultCollationElem = collationsElem.Element("defaultCollation");
				XElement collationElem = collationsElem.Element("collation");
				Assert.That((string)defaultCollationElem, Is.EqualTo("standard"));
				Assert.That((string) collationElem.Attribute("type"), Is.EqualTo("standard"));
				Assert.That((string) collationElem, Is.EqualTo(icuRules.Replace("\r\n", "\n")));
				
				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.Collations.First().ValueEquals(cd));
			}
		}

		[Test]
		public void Roundtrip_LdmlAlternateCollation()
		{
			using (var environment = new TestEnvironment())
			{
				const string icuRules =
					"&B<t<<<T<s<<<S<e<<<E\r\n\t\t\t\t&C<k<<<K<x<<<X<i<<<I\r\n\t\t\t\t&D<q<<<Q<r<<<R\r\n\t\t\t\t&G<o<<<O\r\n\t\t\t\t&W<h<<<H";
				var cd = new IcuCollationDefinition("standard")
				{
					IcuRules = icuRules,
					CollationRules = icuRules,
					IsValid = true
				};

				const string altIcuRules =
					"&B<t<<<T<s<<<S<e<<<E";
				var altCD = new IcuCollationDefinition("standard\ufdd0short")
				{
					IcuRules = altIcuRules,
					CollationRules = altIcuRules,
					IsValid = true
				};

				var wsToLdml = new WritingSystemDefinition("aa", "Latn", "", "");
				wsToLdml.Collations.Add(cd);
				wsToLdml.Collations.Add(altCD);
				wsToLdml.DefaultCollation = cd;
				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);

				XElement ldmlElem = XElement.Load(environment.FilePath("test.ldml"));
				XElement collationsElem = ldmlElem.Element("collations");
				XElement defaultCollationElem = collationsElem.Element("defaultCollation");
				Assert.That((string) defaultCollationElem, Is.EqualTo("standard"));

				XElement[] collationElems = collationsElem.Elements("collation").ToArray();
				Assert.That((string) collationElems[0].Attribute("type"), Is.EqualTo("standard"));
				Assert.That((string) collationElems[0].Attribute("alt"), Is.Null);
				Assert.That((string) collationElems[0], Is.EqualTo(icuRules.Replace("\r\n", "\n")));

				Assert.That((string) collationElems[1].Attribute("type"), Is.EqualTo("standard"));
				Assert.That((string) collationElems[1].Attribute("alt"), Is.EqualTo("short"));
				Assert.That((string) collationElems[1], Is.EqualTo(altIcuRules.Replace("\r\n", "\n")));
				
				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.Collations[0].ValueEquals(cd), Is.True);
				Assert.That(wsFromLdml.Collations[1].ValueEquals(altCD), Is.True);
			}
		}

		[Test]
		public void Roundtrip_LdmlSimpleCollation()
		{
			using (var environment = new TestEnvironment())
			{
				const string simpleRules =
					"\r\n\t\t\t\t\ta/A\r\n\t\t\t\t\tb/B\r\n\t\t\t\t\tt/T\r\n\t\t\t\t\ts/S\r\n\t\t\t\t\tc/C\r\n\t\t\t\t\tk/K\r\n\t\t\t\t\tx/X\r\n\t\t\t\t\ti/I\r\n\t\t\t\t\td/D\r\n\t\t\t\t\tq/Q\r\n\t\t\t\t\tr/R\r\n\t\t\t\t\te/E\r\n\t\t\t\t\tf/F\r\n\t\t\t\t\tg/G\r\n\t\t\t\t\to/O\r\n\t\t\t\t\tj/J\r\n\t\t\t\t\tl/L\r\n\t\t\t\t\tm/M\r\n\t\t\t\t\tn/N\r\n\t\t\t\t\tp/P\r\n\t\t\t\t\tu/U\r\n\t\t\t\t\tv/V\r\n\t\t\t\t\tw/W\r\n\t\t\t\t\th/H\r\n\t\t\t\t\ty/Y\r\n\t\t\t\t\tz/Z\r\n\t\t\t\t";
				const string icuRules =
					"&[before 1] [first regular]  < a\\/A < b\\/B < t\\/T < s\\/S < c\\/C < k\\/K < x\\/X < i\\/I < d\\/D < q\\/Q < r\\/R < e\\/E < f\\/F < g\\/G < o\\/O < j\\/J < l\\/L < m\\/M < n\\/N < p\\/P < u\\/U < v\\/V < w\\/W < h\\/H < y\\/Y < z\\/Z";
				var cd = new SimpleCollationDefinition("standard")
				{
					SimpleRules = simpleRules,
					CollationRules = icuRules,
					IsValid = true
				};
				var wsToLdml = new WritingSystemDefinition("aa", "Latn", "", "");
				wsToLdml.Collations.Add(cd);

				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				XElement ldmlElem = XElement.Load(environment.FilePath("test.ldml"));
				XElement collationsElem = ldmlElem.Element("collations");
				XElement defaultCollationElem = collationsElem.Element("defaultCollation");
				XElement collationElem = collationsElem.Element("collation");
				XElement crElem = collationElem.Element("cr");
				XElement simpleElem = collationElem.Element("special").Element(Sil + "simple");
				Assert.That((string)defaultCollationElem, Is.EqualTo("standard"));
				Assert.That((string)collationElem.Attribute("type"), Is.EqualTo("standard"));
				Assert.That((string)crElem, Is.EqualTo(icuRules.Replace("\r\n", "\n")));
				Assert.That((string)simpleElem, Is.EqualTo(simpleRules.Replace("\r\n", "\n")));

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);

				Assert.That(wsFromLdml.Collations.First().ValueEquals(cd));
				Assert.That(wsFromLdml.DefaultCollation.ValueEquals(cd));
			}
		}

		[Test]
		public void Roundtrip_LdmlSimpleCollationNeedsCompiling()
		{
			using (var environment = new TestEnvironment())
			{
				const string simpleRules =
					"\r\n\t\t\t\t\ta/A\r\n\t\t\t\t\tb/B\r\n\t\t\t\t\tt/T\r\n\t\t\t\t\ts/S\r\n\t\t\t\t\tc/C\r\n\t\t\t\t\tk/K\r\n\t\t\t\t\tx/X\r\n\t\t\t\t\ti/I\r\n\t\t\t\t\td/D\r\n\t\t\t\t\tq/Q\r\n\t\t\t\t\tr/R\r\n\t\t\t\t\te/E\r\n\t\t\t\t\tf/F\r\n\t\t\t\t\tg/G\r\n\t\t\t\t\to/O\r\n\t\t\t\t\tj/J\r\n\t\t\t\t\tl/L\r\n\t\t\t\t\tm/M\r\n\t\t\t\t\tn/N\r\n\t\t\t\t\tp/P\r\n\t\t\t\t\tu/U\r\n\t\t\t\t\tv/V\r\n\t\t\t\t\tw/W\r\n\t\t\t\t\th/H\r\n\t\t\t\t\ty/Y\r\n\t\t\t\t\tz/Z\r\n\t\t\t\t";
				const string icuRules =
					"&[before 1] [first regular]  < a\\/A < b\\/B < t\\/T < s\\/S < c\\/C < k\\/K < x\\/X < i\\/I < d\\/D < q\\/Q < r\\/R < e\\/E < f\\/F < g\\/G < o\\/O < j\\/J < l\\/L < m\\/M < n\\/N < p\\/P < u\\/U < v\\/V < w\\/W < h\\/H < y\\/Y < z\\/Z";
				var cd = new SimpleCollationDefinition("standard")
				{
					SimpleRules = simpleRules,
				};
				var wsToLdml = new WritingSystemDefinition("aa", "Latn", "", "");
				wsToLdml.Collations.Add(cd);

				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				XElement ldmlElem = XElement.Load(environment.FilePath("test.ldml"));
				XElement collationsElem = ldmlElem.Element("collations");
				XElement defaultCollationElem = collationsElem.Element("defaultCollation");
				XElement collationElem = collationsElem.Element("collation");
				XElement specialElem = collationElem.Element("special");
				XElement simpleElem = specialElem.Element(Sil + "simple");
				Assert.That((string)defaultCollationElem, Is.EqualTo("standard"));
				Assert.That((string)collationElem.Attribute("type"), Is.EqualTo("standard"));
				Assert.That((string)specialElem.Attribute(Sil + "needsCompiling"), Is.EqualTo("true"));
				Assert.That((string)simpleElem, Is.EqualTo(simpleRules.Replace("\r\n", "\n")));

				var validatedCd = new SimpleCollationDefinition("standard")
				{
					SimpleRules = simpleRules,
					CollationRules = icuRules,
					IsValid = true
				};
				// When the LDML reader parses the invalid rules, it will validate and regenerate icu rules
				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);

				Assert.That(wsFromLdml.Collations.First().ValueEquals(validatedCd));
				Assert.That(wsFromLdml.DefaultCollation.ValueEquals(validatedCd));
			}
		}

		[Test]
		public void Roundtrip_LdmlIcuCollationWithImports()
		{
			using (var environment = new TestEnvironment())
			{
				const string icuRules =
					"&B<t<<<T<s<<<S<e<<<E\n\t\t\t\t&C<k<<<K<x<<<X<i<<<I\n\t\t\t\t&D<q<<<Q<r<<<R\n\t\t\t\t&G<o<<<O\n\t\t\t\t&W<h<<<H";
				var cd = new IcuCollationDefinition("standard")
				{
					Imports = {new IcuCollationImport("my", "standard")},
					CollationRules = icuRules,
					IsValid = true
				};

				var wsToLdml = new WritingSystemDefinition("aa", "Latn", "", "");
				wsToLdml.Collations.Add(cd);

				var wsFactory = new TestWritingSystemFactory {WritingSystems =
				{
					new WritingSystemDefinition("my")
					{
						Collations = {new IcuCollationDefinition("standard") {IcuRules = icuRules, CollationRules = icuRules, IsValid = true}}
					}
				}};
				var ldmlAdaptor = new LdmlDataMapper(wsFactory);
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				XElement ldmlElem = XElement.Load(environment.FilePath("test.ldml"));
				XElement collationsElem = ldmlElem.Element("collations");
				XElement defaultCollationElem = collationsElem.Element("defaultCollation");
				XElement collationElem = collationsElem.Element("collation");
				XElement crElem = collationElem.Element("cr");
				XElement importElem = collationElem.Element("import");
				Assert.That((string) defaultCollationElem, Is.EqualTo("standard"));
				Assert.That((string) collationElem.Attribute("type"), Is.EqualTo("standard"));
				Assert.That(crElem, Is.Null);
				Assert.That((string) importElem.Attribute("source"), Is.EqualTo("my"));
				Assert.That((string) importElem.Attribute("type"), Is.EqualTo("standard"));

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);

				Assert.That(wsFromLdml.Collations.First().ValueEquals(cd));
				Assert.That(wsFromLdml.DefaultCollation.ValueEquals(cd));
			}
		}

		[Test]
		public void Roundtrip_LdmlFont()
		{
			using (var environment = new TestEnvironment())
			{
				var fd = new FontDefinition("Padauk")
				{
					RelativeSize = 2.1f,
					MinVersion = "3.1.4",
					Features = "order=3 children=2 color=red createDate=1996",
					Language = "en",
					Engines = FontEngines.Graphite | FontEngines.OpenType,
					OpenTypeLanguage = "abcd",
					Roles = FontRoles.Default | FontRoles.Emphasis,
					Subset = "unknown"
				};
				fd.Urls.Add("http://wirl.scripts.sil.org/padauk");
				fd.Urls.Add("http://scripts.sil.org/cms/scripts/page.php?item_id=padauk");

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.Fonts.Add(fd);

				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/special/sil:external-resources/sil:font[@name='Padauk' and @types='default emphasis' and @size='2.1' and @minversion='3.1.4' and @features='order=3 children=2 color=red createDate=1996' and @lang='en' and @otlang='abcd' and @subset='unknown']/sil:url[text()='http://wirl.scripts.sil.org/padauk']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/special/sil:external-resources/sil:font[@name='Padauk' and @types='default emphasis' and @size='2.1' and @minversion='3.1.4' and @features='order=3 children=2 color=red createDate=1996' and @lang='en' and @otlang='abcd' and @subset='unknown']/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=padauk']", environment.NamespaceManager);


				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);

				Assert.That(wsFromLdml.Fonts.First().ValueEquals(fd));
			}
		}

		[Test]
		public void Roundtrip_LdmlSpellChecker()
		{
			using (var environment = new TestEnvironment())
			{
				var scd = new SpellCheckDictionaryDefinition(SpellCheckDictionaryFormat.Hunspell);
				scd.Urls.Add("http://wirl.scripts.sil.org/hunspell");
				scd.Urls.Add("http://scripts.sil.org/cms/scripts/page.php?item_id=hunspell");

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.SpellCheckDictionaries.Add(scd);

				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/special/sil:external-resources/sil:spellcheck[@type='hunspell']/sil:url[text()='http://wirl.scripts.sil.org/hunspell']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/special/sil:external-resources/sil:spellcheck[@type='hunspell']/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=hunspell']", environment.NamespaceManager);


				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);

				Assert.That(wsFromLdml.SpellCheckDictionaries.First().ValueEquals(scd));
			}
		}

		[Test]
		public void Roundtrip_LdmlKeyboard()
		{
			using (var environment = new TestEnvironment())
			{
				List<string> urls = new List<string>();
				urls.Add("http://wirl.scripts.sil.org/keyman");
				urls.Add("http://scripts.sil.org/cms/scripts/page.php?item_id=keyman9");
				IKeyboardDefinition kbd = Keyboard.Controller.CreateKeyboard("Compiled Keyman9", KeyboardFormat.CompiledKeyman, urls);

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.KnownKeyboards.Add(kbd);
				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman9' and @type='kmx']/sil:url[text()='http://wirl.scripts.sil.org/keyman']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman9' and @type='kmx']/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=keyman9']", environment.NamespaceManager);


				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.KnownKeyboards.First(), Is.EqualTo(kbd));
			}
		}
		#endregion

		[Test]
		public void Read_LdmlContainsOnlyPrivateUse_IsoAndprivateUseSetCorrectly()
		{
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			var wsFromLdml = new WritingSystemDefinition();
			using (var tempFile = new TempFile())
			{
				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(LdmlContentForTests.Version3Identity("", "", "", "x-private-use", "abcdefg", "123456", "", ""));
				}
				adaptor.Read(tempFile.Path, wsFromLdml);
				Assert.That(wsFromLdml.Language, Is.EqualTo((LanguageSubtag) "private"));
				Assert.That(wsFromLdml.Variants, Is.EqualTo(new VariantSubtag[] {"use"}));
			}
		}

		#region Write_Ldml
		[Test]
		public void Write_LdmlIsNicelyFormatted()
		{
			using (var file = new TempFile())
			{
				//Create an ldml file to read
				var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
				adaptor.Write(file.Path, ws, null);

				//change the read writing system and write it out again
				var ws2 = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws2);
				ws2.Region = "US";
				adaptor.Write(file.Path, ws2, new MemoryStream(File.ReadAllBytes(file.Path)));
				Assert.That(XElement.Load(file.Path), Is.EqualTo(XElement.Parse(
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='' />
		<generation date='0001-01-01T00:00:00Z' />
		<language type='en' />
		<script type='Zxxx' />
		<territory type='US' />
		<variant type='x-audio' />
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity variantName='Audio' />
		</special>
	</identity>
	<layout>
		<orientation>
			<characterOrder>left-to-right</characterOrder>
		</orientation>
	</layout>
</ldml>"
)).Using((IEqualityComparer<XNode>) new XNodeEqualityComparer()));
			}
		}

		[Test]
		public void Write_WritingSystemWasloadedFromLdmlThatContainedLayoutInfo_LayoutInfoIsOnlyWrittenOnce()
		{
			using (var file = new TempFile())
			{
				//create an ldml file to read that contains layout info
				var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
				ws.RightToLeftScript = true;
				adaptor.Write(file.Path, ws, null);

				//read the file and write it out unchanged
				var ws2 = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws2);
				adaptor.Write(file.Path, ws2, new MemoryStream(File.ReadAllBytes(file.Path)));

				AssertThatXmlIn.File(file.Path).HasNoMatchForXpath("/ldml/layout[2]");
			}
		}

		#endregion
		
// Add these when FlexPrivateUseFormat is migrated
#if WS_FIX
		[Test]
		public void WriteNoRoundTrip_LdmlIsFlexPrivateUseFormatLanguageOnly_LdmlIsChanged()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var adaptor = new LdmlDataMapper();
				var ws = new WritingSystemDefinition();
				adaptor.Read(file.Path,ws);
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path), false), WritingSystemCompatibility.Strict);
				AssertThatLdmlMatches("", "", "", "x-en", file);
			}
		}

		[Test]
		public void WriteNoRoundTrip_LdmlIsFlexPrivateUseFormatlanguageAndScript_LdmlIsChanged()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "", "", file);
				var adaptor = new LdmlDataMapper();
				var ws = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws);
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path), false), WritingSystemCompatibility.Strict);
				AssertThatLdmlMatches("qaa", "Zxxx", "", "x-en", file);
			}
		}

		[Test]
		public void WriteRoundTrip_LdmlIsFlexPrivateUseFormat_LdmlIsUnchanged()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var adaptor = new LdmlDataMapper();
				var ws = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws);
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path), true), WritingSystemCompatibility.Flex7V0Compatible);
				AssertThatLdmlMatches("x-en", "", "", "", file);
			}
		}
#endif

		[Test]
		public void WriteRoundTrip_LdmlIsValidLanguageStartingWithX_LdmlIsUnchanged()
		{
			using (var file = new TempFile())
			{
				WriteCurrentVersionLdml("xh", "", "", "", file);
				var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				var ws = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws);
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path), true));
				AssertThatLdmlMatches("xh", "Latn", "", "", file);
				var versionReader = new WritingSystemLdmlVersionGetter();
				Assert.That(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, Is.EqualTo(versionReader.GetFileVersion(file.Path)));
			}
		}

#if WS_FIX
		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageIsPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "", "", "", "x-en");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageAndScriptPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "", "", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "Zxxx", "", "x-en");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageAndTerritoryPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "US", "", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "", "US", "x-en");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageAndVariantPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "fonipa", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "", "", "fonipa-x-en");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageIsOnlyX_AllFieldsPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "Zxxx", "US", "1901-x-audio");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_AllFieldsPopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "qaa", "Zxxx", "US", "1901-x-en-audio");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}

		[Test]
		public void RoundTripFlexPrivateUseWritingSystem_LanguageAndPrivateUsePopulated()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "x-private", file);
				var ws = new WritingSystemDefinition();
				string originalLdml = File.ReadAllText(file.Path);
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, "", "", "", "x-en-private");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)), WritingSystemCompatibility.Flex7V0Compatible);
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(originalLdml));
			}
		}
#endif


		[Test]
		public void Read_NonDescriptLdml_WritingSystemIdIsSameAsRfc5646Tag()
		{
			using (var file = new TempFile())
			{
				WriteCurrentVersionLdml("en", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper(new TestWritingSystemFactory()).Read(file.Path, ws);
				Assert.That(ws.IetfLanguageTag, Is.EqualTo("en-Zxxx-US-1901-x-audio"));
			}
		}

		[Test]
		public void Read_V0Ldml_ThrowFriendlyException()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper(new TestWritingSystemFactory());
				Assert.That(() => dataMapper.Read(file.Path, ws),
								Throws.Exception.TypeOf<ApplicationException>()
										.With.Property("Message")
										.EqualTo(String.Format("The LDML tag 'en' is version 0.  Version {0} was expected.",
										WritingSystemDefinition.LatestWritingSystemDefinitionVersion)));
			}
		}

		[Test]
		public void Read_V1Ldml_ThrowFriendlyException()
		{
			using (var file = new TempFile())
			{
				// Note: Version 1 Ldml is written with Version 2
				WriteVersion1Ldml("en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper(new TestWritingSystemFactory());
				Assert.That(() => dataMapper.Read(file.Path, ws),
								Throws.Exception.TypeOf<ApplicationException>()
										.With.Property("Message")
										.EqualTo(String.Format("The LDML tag 'en' is version 2.  Version {0} was expected.",
										WritingSystemDefinition.LatestWritingSystemDefinitionVersion)));
			}
		}

		[Test]
		public void RoundTrippingLdmlDoesNotDuplicateSections()
		{
			using (var roundTripOut2 = new TempFile())
			using (var roundTripOut = new TempFile())
			using (var tempFile = new TempFile())
			{

				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(LdmlContentForTests.CurrentVersion("qaa", "", "", "x-lel"));
				}
				var ws = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper(new TestWritingSystemFactory());

				dataMapper.Read(tempFile.Path, ws);
				var keyboard1 = new DefaultKeyboardDefinition("MyFavoriteKeyboard", string.Empty);
				ws.KnownKeyboards.Add(keyboard1);
				using (var fileStream = new FileStream(tempFile.Path, FileMode.Open))
				{
					dataMapper.Write(roundTripOut.Path, ws, fileStream);
				}
				AssertThatXmlIn.File(roundTripOut.Path).HasSpecifiedNumberOfMatchesForXpath("/ldml/special/*[local-name()='external-resources']", 1);
				var secondTripMapper = new LdmlDataMapper(new TestWritingSystemFactory());
				var secondTripWs = new WritingSystemDefinition();
				secondTripMapper.Read(roundTripOut.Path, secondTripWs);
				secondTripWs.KnownKeyboards.Add(new DefaultKeyboardDefinition("x-tel", string.Empty));
				using (var fileStream = new FileStream(roundTripOut.Path, FileMode.Open))
				{
					secondTripMapper.Write(roundTripOut2.Path, secondTripWs, fileStream);
				}
				AssertThatXmlIn.File(roundTripOut2.Path).HasSpecifiedNumberOfMatchesForXpath("/ldml/special/*[local-name()='external-resources']", 1);
			}
		}

		private static void WriteVersion0Ldml(string language, string script, string territory, string variant, TempFile file)
		{
			//using a writing system V0 here because the real writing system can't cope with the way
			//flex encodes private-use language and shouldn't. But using a writing system + ldml adaptor
			//is the quickest way to generate ldml so I'm using it here.
			var ws = new WritingSystemDefinitionV0()
				{ISO639 = language, Script = script, Region = territory, Variant = variant};
			new LdmlAdaptorV0().Write(file.Path, ws, null);
		}

		private static void WriteVersion1Ldml(string language, string script, string territory, string variant, TempFile file)
		{
			var ws = new WritingSystemDefinitionV1();
			ws.SetAllComponents(language, script, territory, variant);
			new LdmlAdaptorV1().Write(file.Path, ws, null);
		}

		private static void WriteCurrentVersionLdml(string language, string script, string territory, string variant, TempFile file)
		{
			var ws = new WritingSystemDefinition();
			ws.SetIetfLanguageTagComponents(language, script, territory, variant);
			new LdmlDataMapper(new TestWritingSystemFactory()).Write(file.Path, ws, null);
		}

		private static void AssertThatLdmlMatches(string language, string script, string territory, string variant, TempFile file)
		{
			AssertThatIdentityElementIsCorrectForContent("language", language, file);
			AssertThatIdentityElementIsCorrectForContent("script", script, file);
			AssertThatIdentityElementIsCorrectForContent("territory", territory, file);
			AssertThatIdentityElementIsCorrectForContent("variant", variant, file);
		}

		private static void AssertThatIdentityElementIsCorrectForContent(string element, string content, TempFile file)
		{
			if (String.IsNullOrEmpty(content) && element != "language")
			{
				AssertThatXmlIn.File(file.Path).HasNoMatchForXpath(String.Format("/ldml/identity/{0}", element));
				return;
			}
			AssertThatXmlIn.File(file.Path).HasAtLeastOneMatchForXpath(String.Format("/ldml/identity/{0}[@type='{1}']", element, content));
		}
	}
}
