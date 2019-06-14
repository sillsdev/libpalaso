using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.IO;
using SIL.Keyboarding;
using SIL.TestUtilities;
using SIL.WritingSystems.Migration;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using SIL.Xml;
using Is = SIL.TestUtilities.NUnitExtensions.Is;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class LdmlDataMapperTests
	{
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

		private static WritingSystemDefinition CreateWritingSystem()
		{
			return new WritingSystemDefinition("en", "Latn", "US", string.Empty, "eng", false);
		}

		[Test]
		public void ReadFromFile_NullFileName_Throws()
		{
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			Assert.Throws<ArgumentNullException>(
				() => adaptor.Read((string)null, CreateWritingSystem())
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
				() => adaptor.Read((XmlReader)null, CreateWritingSystem())
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
				() => adaptor.Write((string)null, CreateWritingSystem(), null)
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
				() => adaptor.Write((XmlWriter)null, CreateWritingSystem(), null)
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
		public void Write_InvalidLanguageTag_Throws()
		{
			var ws = new WritingSystemDefinition {Language = "InvalidLanguage"};
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			var sw = new StringWriter();
			var writer = XmlWriter.Create(sw, CanonicalXmlSettings.CreateXmlWriterSettings());
			Assert.Throws<ArgumentException>(() => adaptor.Write(writer, ws, null));
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

		[Test]
		public void ExistingLdml_UnknownCollation_Write_PreservesData()
		{
			var ldmlwithcollation =
				@"<ldml><collations><collation><unknown>" +
				@"<![CDATA[[caseLevel on]& c < k]]>" +
				@"</unknown></collation></collations></ldml>";
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			var sw = new StringWriter();
			var ws = new WritingSystemDefinition("en");
			var writer = XmlWriter.Create(sw, CanonicalXmlSettings.CreateXmlWriterSettings());
			adaptor.Write(writer, ws, XmlReader.Create(new StringReader(ldmlwithcollation)));
			writer.Close();
			AssertThatXmlIn.String(sw.ToString()).HasSpecifiedNumberOfMatchesForXpath("/ldml/collations/collation", 1);
		}

		[Test]
		public void ExistingLdml_SilReorderCollation_Write_PreservesData()
		{
			var ldmlwithcollation =
				@"<ldml><!--Comment--><dates/><special>hey</special><collations><collation type=""nonsense"">" +
				@"<special xmlns:sil=""urn://www.sil.org/ldml/0.1""><sil:reordered>junk</sil:reordered></special>" +
			@"</collation></collations></ldml>";
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			var sw = new StringWriter();
			var ws = new WritingSystemDefinition("en");
			var writer = XmlWriter.Create(sw, CanonicalXmlSettings.CreateXmlWriterSettings());
			adaptor.Write(writer, ws, XmlReader.Create(new StringReader(ldmlwithcollation)));
			writer.Close();
			AssertThatXmlIn.String(sw.ToString()).HasSpecifiedNumberOfMatchesForXpath("/ldml/collations/collation", 1);
		}

		[Test]
		public void ExistingLdml_NonSILSpecialCollation_Write_PreservesData()
		{
			var ldmlwithcollation =
				@"<ldml><!--Comment--><dates/><special>hey</special><collations><collation type=""nonsense"">" +
				@"<special xmlns:notsil=""urn://www.notsil.org/ldml/42""><notsil:notsospecial>Hedburg</notsil:notsospecial></special>" +
				@"</collation></collations></ldml>";
			var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
			var sw = new StringWriter();
			var ws = new WritingSystemDefinition("en");
			var writer = XmlWriter.Create(sw, CanonicalXmlSettings.CreateXmlWriterSettings());
			adaptor.Write(writer, ws, XmlReader.Create(new StringReader(ldmlwithcollation)));
			writer.Close();
			AssertThatXmlIn.String(sw.ToString()).HasSpecifiedNumberOfMatchesForXpath("/ldml/collations/collation", 1);
		}

		#region Roundtrip
		[Test]
		public void RoundtripSimpleCustomSortRules_WS33715()
		{
			var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());

			const string sortRules = "(A̍ a̍)";
			var cd = new SimpleRulesCollationDefinition("standard") {SimpleRules = sortRules};
			var wsWithSimpleCustomSortRules = new WritingSystemDefinition();
			wsWithSimpleCustomSortRules.Collations.Add(cd);

			var wsFromLdml = new WritingSystemDefinition();
			using (var tempFile = new TempFile())
			{
				ldmlAdaptor.Write(tempFile.Path, wsWithSimpleCustomSortRules, null);
				ldmlAdaptor.Read(tempFile.Path, wsFromLdml);
			}

			var cdFromLdml = (SimpleRulesCollationDefinition) wsFromLdml.Collations.First();
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
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='GB']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-test']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/version[@number='$Revision$' and text()='Identity version description']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@windowsLCID='1036' and @defaultRegion='US' and not(@variantName)]", environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.VersionNumber, Is.EqualTo("$Revision$"));
				Assert.That(wsFromLdml.VersionDescription, Is.EqualTo("Identity version description"));
				Assert.That(wsFromLdml.LanguageTag, Is.EqualTo("en-GB-x-test"));
				Assert.That(wsFromLdml.WindowsLcid, Is.EqualTo("1036"));
				Assert.That(wsFromLdml.DefaultRegion, Is.EqualTo("US"));
				int index = IetfLanguageTag.GetIndexOfFirstNonCommonPrivateUseVariant(wsFromLdml.Variants);
				Assert.That(index, Is.EqualTo(0));
				Assert.That(wsFromLdml.Variants[index].Name, Is.EqualTo(string.Empty));
			}
		}

		[Test]
		public void Roundtrip_VariantName()
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
				wsToLdml.Variants[0] = new VariantSubtag(wsToLdml.Variants[0], "test0");
				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());

				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasNoMatchForXpath("/ldml/identity/script");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='GB']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-test']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/version[@number='$Revision$' and text()='Identity version description']");
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@windowsLCID='1036' and @defaultRegion='US' and @variantName='test0']", environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.VersionNumber, Is.EqualTo("$Revision$"));
				Assert.That(wsFromLdml.VersionDescription, Is.EqualTo("Identity version description"));
				Assert.That(wsFromLdml.LanguageTag, Is.EqualTo("en-GB-x-test"));
				Assert.That(wsFromLdml.WindowsLcid, Is.EqualTo("1036"));
				Assert.That(wsFromLdml.DefaultRegion, Is.EqualTo("US"));
				int index = IetfLanguageTag.GetIndexOfFirstNonCommonPrivateUseVariant(wsFromLdml.Variants);
				Assert.That(index, Is.EqualTo(0));
				Assert.That(wsFromLdml.Variants[index].Name, Is.EqualTo("test0"));
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

		// This tests characters elements
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

				var footnotes = new CharacterSetDefinition("footnotes") {Characters = {"¹", "²", "³", "⁴", "⁵", "⁶", "⁷", "⁸", "⁹", "¹⁰"}};

				var numeric = new CharacterSetDefinition("numeric") {Characters = {"๐", "๑", "๒", "๓", "๔", "๕", "๖", "๗", "๘", "๙"}};

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
					.HasAtLeastOneMatchForXpath("/ldml/characters/special/sil:exemplarCharacters[@type='footnotes' and text()='[¹ ² ³ ⁴ ⁵ ⁶ ⁷ ⁸ ⁹ {¹⁰}]']", environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.CharacterSets["index"], Is.ValueEqualTo(index));
				Assert.That(wsFromLdml.CharacterSets["main"], Is.ValueEqualTo(main));
				Assert.That(wsFromLdml.CharacterSets["footnotes"], Is.ValueEqualTo(footnotes));
			}
		}

		[Test]
		public void Roundtrip_LdmlCLDRNumbers()
		{
			using (var environment = new TestEnvironment())
			{
				var numeric = new NumberingSystemDefinition("arab");

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.NumberingSystem = numeric;

				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/numbers/defaultNumberingSystem[text()='arab']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasNoMatchForXpath("/ldml/numbers/numberingSystem", environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.NumberingSystem, Is.ValueEqualTo(numeric));
			}
		}

		[Test]
		public void Roundtrip_LdmlCustomNumbers()
		{
			using (var environment = new TestEnvironment())
			{
				var numeric = NumberingSystemDefinition.CreateCustomSystem("๐๑๒๓๔๕๖๗๘๙");

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.NumberingSystem = numeric;

				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/numbers/defaultNumberingSystem[text()='other(๐๑๒๓๔๕๖๗๘๙)']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasNoMatchForXpath("/ldml/numbers/numberingSystem", environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.NumberingSystem, Is.ValueEqualTo(numeric));
			}
		}

		[Test]
		public void BadCustomDigitsReturnDefault()
		{
			Assert.AreSame(NumberingSystemDefinition.Default, NumberingSystemDefinition.CreateCustomSystem(string.Empty));
			Assert.AreSame(NumberingSystemDefinition.Default, NumberingSystemDefinition.CreateCustomSystem("123"));
		}

		[Test]
		public void Roundtrip_LdmlCustomNumbersWithSurrogatePairs()
		{
			using (var environment = new TestEnvironment())
			{
				var numeric = NumberingSystemDefinition.CreateCustomSystem("01234𠈓6789");

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.NumberingSystem = numeric;

				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/numbers/defaultNumberingSystem[text()='other(01234𠈓6789)']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasNoMatchForXpath("/ldml/numbers/numberingSystem", environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.NumberingSystem, Is.ValueEqualTo(numeric));
			}
		}

		[Test]
		public void Roundtrip_LdmlAlternateCharacters_Ignored()
		{
			// Test when reading LDML that elements with alt attribute are ignored.
			// Because we don't write elements with alt attribute, this test
			// starts by writing a set content
			using (var environment = new TestEnvironment())
			{
				string content =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='' />
		<language type='en' />
		<script type='Latn' />
	</identity>
	<characters>
		<exemplarCharacters alt='capital'>[A-Z{AZ}]</exemplarCharacters>
		<exemplarCharacters>[a-z{az}]</exemplarCharacters>
	</characters>
</ldml>".Replace("\'", "\"");

				var main = new CharacterSetDefinition("main");
				for (int i = 'a'; i <= (int)'z'; i++)
					main.Characters.Add(((char)i).ToString(CultureInfo.InvariantCulture));
				main.Characters.Add("az");

				File.WriteAllText(environment.FilePath("test.ldml"), content);

				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.CharacterSets.Count, Is.EqualTo(1));
				Assert.That(wsFromLdml.CharacterSets["main"], Is.ValueEqualTo(main));

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.CharacterSets.Add(main);

				ldmlAdaptor.Write(environment.FilePath("test2.ldml"), wsToLdml, new MemoryStream(File.ReadAllBytes(environment.FilePath("test.ldml"))));
				AssertThatXmlIn.File(environment.FilePath("test2.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/characters/exemplarCharacters[text()='[a-z{az}]']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test2.ldml"))
					.HasAtLeastOneMatchForXpath("/ldml/characters/exemplarCharacters[@alt='capital' and text()='[A-Z{AZ}]']", environment.NamespaceManager);
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
				var qm1 = new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal);
				var qm2 = new QuotationMark("{", "}", "{", 2, QuotationMarkingSystemType.Normal);
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
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:matched-pairs/sil:matched-pair[@open='mpOpen1' and @close='mpClose2' and @paraClose='false']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:punctuation-patterns/sil:punctuation-pattern[@pattern='pattern1' and @context='medial']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/quotationStart[text()='\"']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/quotationEnd[text()='\"']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/alternateQuotationStart[text()='{']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/alternateQuotationEnd[text()='}']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:quotation-marks/sil:quotationContinue[text()='\"']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:quotation-marks/sil:alternateQuotationContinue[text()='{']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:quotation-marks/sil:quotation[@open='open1' and @close='close2' and @continue='cont3' and @level='3']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:quotation-marks/sil:quotation[@open and string-length(@open)=0 and @level='1' and @type='narrative']", 1, environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.MatchedPairs.FirstOrDefault(), Is.EqualTo(mp));
				Assert.That(wsFromLdml.PunctuationPatterns.FirstOrDefault(), Is.EqualTo(pp));
				Assert.That(wsFromLdml.QuotationParagraphContinueType, Is.EqualTo(QuotationParagraphContinueType.Outermost));
				Assert.That(wsFromLdml.QuotationMarks[0], Is.EqualTo(qm1));
				Assert.That(wsFromLdml.QuotationMarks[1], Is.EqualTo(qm2));
				Assert.That(wsFromLdml.QuotationMarks[2], Is.EqualTo(qm3));
				Assert.That(wsFromLdml.QuotationMarks[3], Is.EqualTo(qm4));

				// Test rewriting the loaded file while using the original version as a base to make sure
				// no duplicate elements are created
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsFromLdml, new MemoryStream(File.ReadAllBytes(environment.FilePath("test.ldml"))));
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:matched-pairs/sil:matched-pair[@open='mpOpen1' and @close='mpClose2' and @paraClose='false']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:punctuation-patterns/sil:punctuation-pattern[@pattern='pattern1' and @context='medial']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/quotationStart[text()='\"']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/quotationEnd[text()='\"']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/alternateQuotationStart[text()='{']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/alternateQuotationEnd[text()='}']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:quotation-marks/sil:quotationContinue[text()='\"']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:quotation-marks/sil:alternateQuotationContinue[text()='{']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:quotation-marks/sil:quotation[@open='open1' and @close='close2' and @continue='cont3' and @level='3']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/delimiters/special/sil:quotation-marks/sil:quotation[@open and string-length(@open)=0 and @level='1' and @type='narrative']", 1, environment.NamespaceManager);
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
				var cd = new IcuRulesCollationDefinition("standard")
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
				Assert.That(wsFromLdml.Collations.First(), Is.ValueEqualTo(cd));
			}
		}

		[Test]
		public void Roundtrip_LdmlInvalidStandardCollation()
		{
			using (var environment = new TestEnvironment())
			{
				const string icuRules =
					"&&&B<t<<<T<s<<<S<e<<<E\r\n\t\t\t\t&C<k<<<K<x<<<X<i<<<I\r\n\t\t\t\t&D<q<<<Q<r<<<R\r\n\t\t\t\t&G<o<<<O\r\n\t\t\t\t&W<h<<<H";
				var cd = new IcuRulesCollationDefinition("standard")
				{
					IcuRules = icuRules,
					CollationRules = icuRules,
					IsValid = false
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
				Assert.That((string)collationElem.Attribute("type"), Is.EqualTo("standard"));
				Assert.That((string)collationElem, Is.EqualTo(icuRules.Replace("\r\n", "\n")));
				// Verify comment written about being unable to parse ICU rule
				const string expectedComment = "'Unable to parse the ICU rules with ICU version'";
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath(string.Format("/ldml/collations/collation/comment()[contains(.,{0})]", expectedComment));

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.Collations.First(), Is.ValueEqualTo(cd));
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
				var cd = new SimpleRulesCollationDefinition("standard")
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

				Assert.That(wsFromLdml.Collations.First(), Is.ValueEqualTo(cd));
				Assert.That(wsFromLdml.DefaultCollation, Is.ValueEqualTo(cd));
			}
		}

		[Test]
		public void Roundtrip_LdmlSimpleCollationNeedsCompiling()
		{
			using (var environment = new TestEnvironment())
			{
				const string simpleRules =
					"\r\n\t\t\t\t\t\\!/A\r\n\t\t\t\t\tb/B\r\n\t\t\t\t\tt/T\r\n\t\t\t\t\ts/S\r\n\t\t\t\t\tc/C\r\n\t\t\t\t\tk/K\r\n\t\t\t\t\tx/X\r\n\t\t\t\t\ti/I\r\n\t\t\t\t\td/D\r\n\t\t\t\t\tq/Q\r\n\t\t\t\t\tr/R\r\n\t\t\t\t\te/E\r\n\t\t\t\t\tf/F\r\n\t\t\t\t\tg/G\r\n\t\t\t\t\to/O\r\n\t\t\t\t\tj/J\r\n\t\t\t\t\tl/L\r\n\t\t\t\t\tm/M\r\n\t\t\t\t\tn/N\r\n\t\t\t\t\tp/P\r\n\t\t\t\t\tu/U\r\n\t\t\t\t\tv/V\r\n\t\t\t\t\tw/W\r\n\t\t\t\t\th/H\r\n\t\t\t\t\ty/Y\r\n\t\t\t\t\tz/Z\r\n\t\t\t\t";
				var cd = new SimpleRulesCollationDefinition("standard")
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

				var validatedCd = new SimpleRulesCollationDefinition("standard")
				{
					SimpleRules = simpleRules
				};
				// When the LDML reader parses the invalid rules, it will validate and regenerate icu rules
				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);

				Assert.That(wsFromLdml.Collations.First(), Is.ValueEqualTo(validatedCd));
				Assert.That(wsFromLdml.DefaultCollation, Is.ValueEqualTo(validatedCd));
			}
		}

		[Test]
		public void Roundtrip_LdmlIcuCollationWithImports()
		{
			using (var environment = new TestEnvironment())
			{
				const string icuRules =
					"&B<t<<<T<s<<<S<e<<<E\n\t\t\t\t&C<k<<<K<x<<<X<i<<<I\n\t\t\t\t&D<q<<<Q<r<<<R\n\t\t\t\t&G<o<<<O\n\t\t\t\t&W<h<<<H";
				var cd = new IcuRulesCollationDefinition("standard")
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
						Collations = {new IcuRulesCollationDefinition("standard") {IcuRules = icuRules, CollationRules = icuRules, IsValid = true}}
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

				Assert.That(wsFromLdml.Collations.Count, Is.EqualTo(1));
				Assert.That(wsFromLdml.Collations.First(), Is.ValueEqualTo(cd));
				Assert.That(wsFromLdml.DefaultCollation, Is.ValueEqualTo(cd));
			}
		}

		[Test]
		public void Roundtrip_LdmlMultipleIcuCollations()
		{
			using (var environment = new TestEnvironment())
			{
				var cd = new IcuRulesCollationDefinition("standard")
				{
					CollationRules = "&B<t",
					IsValid = true
				};
				var phonebookCd = new IcuRulesCollationDefinition("phonebook")
				{
					CollationRules = "&c<f",
					IsValid = true
				};

				var wsToLdml = new WritingSystemDefinition("aa", "Latn", "", "");
				wsToLdml.Collations.Add(cd);
				wsToLdml.Collations.Add(phonebookCd);

				var wsFactory = new TestWritingSystemFactory();
				var ldmlAdaptor = new LdmlDataMapper(wsFactory);
				var testLdmlFile = environment.FilePath("test.ldml");
				ldmlAdaptor.Write(testLdmlFile, wsToLdml, null);
				// validate that the data was written to the file correctly
				AssertThatXmlIn.File(testLdmlFile).HasSpecifiedNumberOfMatchesForXpath("/ldml/collations/collation[@type='standard']", 1);
				AssertThatXmlIn.File(testLdmlFile).HasSpecifiedNumberOfMatchesForXpath("/ldml/collations/collation[@type='phonebook']", 1);
				AssertThatXmlIn.File(testLdmlFile).HasSpecifiedNumberOfMatchesForXpath("/ldml/collations/defaultCollation[text()='standard']", 1);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(testLdmlFile, wsFromLdml);

				// verify that both collations came out and that the default is set correctly
				Assert.That(wsFromLdml.Collations.Count, Is.EqualTo(2));
				Assert.That(wsFromLdml.DefaultCollation, Is.ValueEqualTo(cd));
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

				Assert.That(wsFromLdml.Fonts.First(), Is.ValueEqualTo(fd));
			}
		}

		[Test]
		public void Roundtrip_LdmlFontRoles()
		{
			using (var environment = new TestEnvironment())
			{
				var fd1 = new FontDefinition("font1")
				{
					RelativeSize = 2.1f,
					MinVersion = "3.1.4",
					Features = "order=3 children=2 color=red createDate=1996",
					Language = "en",
					Engines = FontEngines.Graphite | FontEngines.OpenType,
					OpenTypeLanguage = "abcd",
					Roles = FontRoles.Default,
					Subset = "unknown"
				};
				fd1.Urls.Add("http://wirl.scripts.sil.org/font1");

				var fd2 = new FontDefinition("font2")
				{
					RelativeSize = 2.1f,
					MinVersion = "3.1.4",
					Features = "order=3 children=2 color=red createDate=1996",
					Language = "en",
					Engines = FontEngines.Graphite | FontEngines.OpenType,
					OpenTypeLanguage = "abcd",
					Roles = FontRoles.None,
					Subset = "unknown"
				};
				fd2.Urls.Add("http://wirl.scripts.sil.org/font2");

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.Fonts.Add(fd1);
				wsToLdml.Fonts.Add(fd2);
				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/special/sil:external-resources/sil:font[@name='font1' and @types='default' and @size='2.1' and @minversion='3.1.4' and @features='order=3 children=2 color=red createDate=1996' and @lang='en' and @otlang='abcd' and @subset='unknown']/sil:url[text()='http://wirl.scripts.sil.org/font1']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasSpecifiedNumberOfMatchesForXpath("/ldml/special/sil:external-resources/sil:font[@name='font2' and @size='2.1' and @minversion='3.1.4' and @features='order=3 children=2 color=red createDate=1996' and @lang='en' and @otlang='abcd' and @subset='unknown']/sil:url[text()='http://wirl.scripts.sil.org/font2']", 1, environment.NamespaceManager);
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

				Assert.That(wsFromLdml.SpellCheckDictionaries.First(), Is.ValueEqualTo(scd));
			}
		}

		[Test]
		public void Roundtrip_LdmlKeyboard()
		{
			using (var environment = new TestEnvironment())
			{
				var urls = new List<string>
				{
					"http://wirl.scripts.sil.org/keyman",
					"http://scripts.sil.org/cms/scripts/page.php?item_id=keyman9"
				};
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
					writer.Write(LdmlContentForTests.Version3Identity("", "", "", "x-private-use", "abcdefg", "123456", "private", "", ""));
				adaptor.Read(tempFile.Path, wsFromLdml);
				Assert.That(wsFromLdml.Variants, Is.EqualTo(new[]
				{
					new VariantSubtag("private", "private", true, false, new List<string>()),
					new VariantSubtag("use", "", true, false, new List<string>())
				}));
			}
		}

		#region Write_Ldml
		[Test]
		public void Write_UnknownKeyboard_NotWrittenToLdml()
		{
			using (var environment = new TestEnvironment())
			{
				var urls = new List<string>
				{
					"http://wirl.scripts.sil.org/keyman",
					"http://scripts.sil.org/cms/scripts/page.php?item_id=keyman9"
				};
				IKeyboardDefinition kbd1 = Keyboard.Controller.CreateKeyboard("Compiled Keyman9", KeyboardFormat.CompiledKeyman, urls);
				IKeyboardDefinition kbd2 = Keyboard.Controller.CreateKeyboard("Unknown System Keyboard", KeyboardFormat.Unknown, urls);

				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.KnownKeyboards.Add(kbd1);
				wsToLdml.KnownKeyboards.Add(kbd2);
				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath(
						"/ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman9' and @type='kmx']/sil:url[text()='http://wirl.scripts.sil.org/keyman']",
						environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath(
						"/ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman9' and @type='kmx']/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=keyman9']",
						environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasNoMatchForXpath(
						"/ldml/special/sil:external-resources/sil:kbd[@id='Unknown System Keyboard']",
						environment.NamespaceManager);

				var wsFromLdml = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), wsFromLdml);
				Assert.That(wsFromLdml.KnownKeyboards.First(), Is.EqualTo(kbd1));
				Assert.That(wsFromLdml.KnownKeyboards.Count, Is.EqualTo(1));
			}
		}

		[Test]
		public void Write_KeyboardInfoIsOnlyWrittenOnce()
		{
			using (var environment = new TestEnvironment())
			{
				var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
				var keyboard1 = new DefaultKeyboardDefinition("en-US_MyFavoriteKeyboard", "MyFavoriteKeyboard", "MyFavoriteKeyboard", "en-US", true);
				keyboard1.Format = KeyboardFormat.Msklc;
				ws.KnownKeyboards.Add(keyboard1);
				var keyboard2 = new DefaultKeyboardDefinition("en-GB_SusannasFavoriteKeyboard", "SusannasFavoriteKeyboard", "SusannasFavoriteKeyboard", "en-GB", true);
				keyboard2.Format = KeyboardFormat.Msklc;
				ws.KnownKeyboards.Add(keyboard2);

				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());


				ldmlAdaptor.Write(environment.FilePath("test.ldml"), ws, null);

				//read the file and write it out unchanged
				var ws2 = new WritingSystemDefinition();
				ldmlAdaptor.Read(environment.FilePath("test.ldml"), ws2);
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), ws2, new MemoryStream(File.ReadAllBytes(environment.FilePath("test.ldml"))));

				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasSpecifiedNumberOfMatchesForXpath("/ldml/special/sil:external-resources/sil:kbd[@id='en-US_MyFavoriteKeyboard' and @type='msklc']", 1, environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml")).HasSpecifiedNumberOfMatchesForXpath("/ldml/special/sil:external-resources/sil:kbd[@id='en-GB_SusannasFavoriteKeyboard' and @type='msklc']", 1, environment.NamespaceManager);
			}
		}

		[Test]
		public void Write_SystemCollationDefinition_NotWrittenToLdml()
		{
			using (var environment = new TestEnvironment())
			{
				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.DefaultCollation = new SystemCollationDefinition { LanguageTag = "en-US" };
				wsToLdml.Collations.Add(new IcuRulesCollationDefinition("standard"));
				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasAtLeastOneMatchForXpath(
						"/ldml/collations/collation[@type='standard']",
						environment.NamespaceManager);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasNoMatchForXpath(
						"/ldml/collations/collation[@type='system']",
						environment.NamespaceManager);
			}
		}

		[Test]
		public void Write_NoLdmlCollations_CollationsNotWrittenToLdml()
		{
			using (var environment = new TestEnvironment())
			{
				var wsToLdml = new WritingSystemDefinition("en", "Latn", "", "");
				wsToLdml.DefaultCollation = new SystemCollationDefinition {LanguageTag = "en-US"};
				var ldmlAdaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				ldmlAdaptor.Write(environment.FilePath("test.ldml"), wsToLdml, null);
				AssertThatXmlIn.File(environment.FilePath("test.ldml"))
					.HasNoMatchForXpath(
						"/ldml/collations",
						environment.NamespaceManager);
			}
		}

		[TestCase(null)]
		[TestCase("\u0000\u0000")]
		public void Write_LdmlIsNicelyFormatted(string curentContent)
		{
			using (var file = new TempFile())
			{
				//Create an ldml file to read
				var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
				ws.DateModified = new DateTime(01, 01, 01, 0, 0, 0, DateTimeKind.Utc);
				Stream existingDataStream = curentContent == null ? null : new MemoryStream(Encoding.UTF8.GetBytes(curentContent));
				adaptor.Write(file.Path, ws, existingDataStream);

				//change the read writing system and write it out again
				var ws2 = new WritingSystemDefinition();
				ws2.DateModified = new DateTime(01, 01, 01, 0, 0, 0, DateTimeKind.Utc);
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
				var ws = new WritingSystemDefinition("en-Zxxx-x-audio") {RightToLeftScript = true};
				adaptor.Write(file.Path, ws, null);

				//read the file and write it out unchanged
				var ws2 = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws2);
				adaptor.Write(file.Path, ws2, new MemoryStream(File.ReadAllBytes(file.Path)));

				AssertThatXmlIn.File(file.Path).HasNoMatchForXpath("/ldml/layout[2]");
			}
		}

		#endregion

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
				AssertThatLdmlMatches("xh", "", "", "", file);
				var versionReader = new WritingSystemLdmlVersionGetter();
				Assert.That(LdmlDataMapper.CurrentLdmlLibraryVersion, Is.EqualTo(versionReader.GetFileVersion(file.Path)));
			}
		}

		[Test]
		public void Read_NonDescriptLdml_WritingSystemIdIsSameAsIetfLanguageTag()
		{
			using (var file = new TempFile())
			{
				WriteCurrentVersionLdml("en", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper(new TestWritingSystemFactory()).Read(file.Path, ws);
				Assert.That(ws.LanguageTag, Is.EqualTo("en-Zxxx-US-1901-x-audio"));
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
										LdmlDataMapper.CurrentLdmlLibraryVersion)));
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
										LdmlDataMapper.CurrentLdmlLibraryVersion)));
			}
		}

		[Test]
		public void Read_LdmlWithDuplicateCollations_DropsExtraAndDoesNotCrash()
		{
			using (var file = new TempFile())
			{
				WriteCurrentVersionLdml("en", "", "", "", file);
				var doc = XDocument.Load(file.Path);
				var collationsNode = doc.Root.Descendants("collations").FirstOrDefault();
				if (collationsNode == null)
				{
					collationsNode = XElement.Parse("<collations/>");
					doc.Root.Add(collationsNode);
				}
				collationsNode.Add(XElement.Parse("<collation type='standard'/>"));
				collationsNode.Add(XElement.Parse("<collation type='standard'/>"));
				doc.Save(file.Path);
				var ws = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper(new TestWritingSystemFactory());
				Assert.DoesNotThrow(() => dataMapper.Read(file.Path, ws));
				Assert.That(ws.Collations.Count(cd => cd.Type == "standard"), Is.EqualTo(1));
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

		[Test]
		public void DuplicatedFontInLdml_ReportsBadData()
		{
			using (var roundTripOut = new TempFile())
			using (var tempFile = new TempFile())
			{

				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(LdmlContentForTests.Version3("qaa", "", "", "", LdmlContentForTests.FontElem + LdmlContentForTests.FontElem));
				}
				var ws = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper(new TestWritingSystemFactory());

				var message = Assert.Throws<ArgumentException>(() => dataMapper.Read(tempFile.Path, ws)).Message;
				StringAssert.IsMatch("The font .* is defined twice in .*\\.ldml", message);
			}
		}

		private static void WriteVersion0Ldml(string language, string script, string territory, string variant, TempFile file)
		{
			//using a writing system V0 here because the real writing system can't cope with the way
			//flex encodes private-use language and shouldn't. But using a writing system + ldml adaptor
			//is the quickest way to generate ldml so I'm using it here.
			var ws = new WritingSystemDefinitionV0 {ISO639 = language, Script = script, Region = territory, Variant = variant};
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
			ws.LanguageTag = IetfLanguageTag.Create(language, script, territory, variant);
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
