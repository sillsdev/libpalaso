using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.IO;
using Palaso.TestUtilities;
using Palaso.Xml;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class LdmlDataMapperTests
	{
		private LdmlDataMapper _adaptor;
		private WritingSystemDefinition _ws;

		[SetUp]
		public void SetUp()
		{
			_adaptor = new LdmlDataMapper();
			_ws = new WritingSystemDefinition("en", "Latn", "US", string.Empty, "eng", false);
		}

		[Test]
		public void ReadFromFile_NullFileName_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read((string)null, _ws)
			);
		}

		[Test]
		public void ReadFromFile_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read("foo.ldml", null)
			);
		}

		[Test]
		public void ReadFromXmlReader_NullXmlReader_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read((XmlReader)null, _ws)
			);
		}

		[Test]
		public void ReadFromXmlReader_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Read(XmlReader.Create(new StringReader("<ldml/>")), null)
			);
		}

		[Test]
		public void WriteToFile_NullFileName_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write((string)null, _ws, null)
			);
		}

		[Test]
		public void WriteToFile_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write("foo.ldml", null, null)
			);
		}

		[Test]
		public void WriteToXmlWriter_NullXmlReader_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write((XmlWriter)null, _ws, null)
			);
		}

		[Test]
		public void WriteToXmlWriter_NullWritingSystem_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => _adaptor.Write(XmlWriter.Create(new MemoryStream()), null, null)
			);
		}

		[Test]
		public void ExistingUnusedLdml_Write_PreservesData()
		{
			var sw = new StringWriter();
			var ws = new WritingSystemDefinition("en");
			var writer = XmlWriter.Create(sw, CanonicalXmlSettings.CreateXmlWriterSettings());
			_adaptor.Write(writer, ws, XmlReader.Create(new StringReader("<ldml><!--Comment--><dates/><special>hey</special></ldml>")));
			writer.Close();
			AssertThatXmlIn.String(sw.ToString()).HasAtLeastOneMatchForXpath("/ldml/special[text()=\"hey\"]");
		}

#if WS_FIX
		[Test]
		public void RoundtripSimpleCustomSortRules_WS33715()
		{
			var ldmlAdaptor = new LdmlDataMapper();

			const string sortRules = "(A̍ a̍)";
			var wsWithSimpleCustomSortRules = new WritingSystemDefinition {CollationRulesType = CollationRulesTypes.CustomSimple, CollationRules = sortRules};

			var wsFromLdml = new WritingSystemDefinition();
			using (var tempFile = new TempFile())
			{
				ldmlAdaptor.Write(tempFile.Path, wsWithSimpleCustomSortRules, null);
				ldmlAdaptor.Read(tempFile.Path, wsFromLdml);
			}

			Assert.AreEqual(sortRules, wsFromLdml.CollationRules);
		}

		[Test]
		public void RoundtripKnownKeyboards()
		{
			var ldmlAdaptor = new LdmlDataMapper();

			Keyboard.Controller = new MyKeyboardController();

			var wsWithKnownKeyboards = new WritingSystemDefinition();
			var keyboard1 = new DefaultKeyboardDefinition(KeyboardType.System, "MyFavoriteKeyboard", "en-US");
			wsWithKnownKeyboards.KnownKeyboards.Add(keyboard1);

			var keyboard2 = new DefaultKeyboardDefinition(KeyboardType.System, "SusannasFavoriteKeyboard", "en-GB");
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
			Assert.That(keyboard1FromLdml, Is.InstanceOf<MyKeyboardDefn>(), "Reader should have used controller to create keyboard defn");

			var keyboard2FromLdml = knownKeyboards[1];
			Assert.That(keyboard2FromLdml.Layout, Is.EqualTo("SusannasFavoriteKeyboard"));
			Assert.That(keyboard2FromLdml.Locale, Is.EqualTo("en-GB"));
		}

		class MyKeyboardDefn : DefaultKeyboardDefinition
		{
			public MyKeyboardDefn(KeyboardType type, string layout, string locale) : base(type, layout, locale)
			{
			}
		}

		class MyKeyboardController : DefaultKeyboardController
		{
			public override IKeyboardDefinition CreateKeyboardDefinition(string id)
			{
				return new MyKeyboardDefn(KeyboardType.System, id, id);
			}
		}
#endif

		[Test]
		public void Read_LdmlIdentity()
		{
			var ldmlAdaptor = new LdmlDataMapper();
			var wsFromLdml = new WritingSystemDefinition();
			using (var tempFile = new TempFile())
			{
				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(
@"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='$Revision$'>Identity version description</version>
		<generation date='$Date$'/>
		<!-- name.en(en)='English' -->
		<language type='en'/>
		<script type='Latn'/>
		<variant type='x-test'/>
		<special>
			<sil:identity windowsLCID='1036' defaultRegion='US' variantName='1996'>
			</sil:identity>
		</special>
	</identity>
</ldml>".Replace("'", "\""));
				}
				ldmlAdaptor.Read(tempFile.Path, wsFromLdml);
			}
			Assert.That(wsFromLdml.VersionNumber, Is.EqualTo("$Revision$"));
			Assert.That(wsFromLdml.VersionDescription, Is.EqualTo("Identity version description"));
			Assert.That(wsFromLdml.Id, Is.EqualTo("en-Latn-x-test"));
			Assert.That(wsFromLdml.WindowsLcid, Is.EqualTo("1036"));
			Assert.That(wsFromLdml.DefaultRegion, Is.EqualTo("US"));
			Assert.That(wsFromLdml.Variants[0].Name, Is.EqualTo("1996"));
		}

		// TODO: Fix this on Linux  DWONG 01-21-2015
		[Test]
		[Platform(Exclude = "Linux")]
		public void Read_LdmlCharacters()
		{
			var ldmlAdaptor = new LdmlDataMapper();
			var wsFromLdml = new WritingSystemDefinition();
			using (var tempFile = new TempFile())
			{
				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(
// ldml string is split to handle special escaped punctuation in footnotes type
#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='$Revision$'/>
		<generation date='$Date$'/>
		<!-- name.en(en)='English' -->
		<language type='en'/>
		<script type='Latn'/>
	</identity>
	<characters>
		<exemplarCharacters type='index'>[A-G H-N O P Q R S T U V W X Y Z {AZ}]</exemplarCharacters>
		<exemplarCharacters>[a b c d e f g h i j k l m n o p q r s t u v w x y z]</exemplarCharacters>
		<special>".Replace("'", "\"") +
			@"<sil:exemplarCharacters type=\quot;footnotes\quot;>[\- ‐ – — , ; \: ! ? . … ' ‘ ’ \quot; “ ” ( ) \[ \] § @ * / \&amp; # † ‡ ′ ″]</sil:exemplarCharacters>".Replace("\\quot;", "\"")+
		@"</special>
	</characters>
	<numbers>
		<defaultNumberingSystem>thai</defaultNumberingSystem>
		<numberingSystem id='thai' type='numeric' digits='๐๑๒๓๔๕๖๗๘๙' />
	</numbers>
</ldml>".Replace("'", "\""));
#endregion
				}
				ldmlAdaptor.Read(tempFile.Path, wsFromLdml);
			}
			var index = new CharacterSetDefinition("index");
			for (int i = 'A'; i <= (int) 'Z'; i++)
				index.Characters.Add(((char) i).ToString(CultureInfo.InvariantCulture));
			index.Characters.Add("AZ");

			var footnotes = new CharacterSetDefinition("footnotes");
			const string footnotesString = "- ‐ – — , ; : ! ? . … ' ‘ ’ \" “ ” ( ) [ ] § @ * / & # † ‡ ′ ″";
			foreach (string str in footnotesString.Split(' '))
			{
				footnotes.Characters.Add(str);
			}

			var numeric = new CharacterSetDefinition("numeric");
			const string numericString = "๐ ๑ ๒ ๓ ๔ ๕ ๖ ๗ ๘ ๙";
			foreach (string str in numericString.Split(' '))
			{
				numeric.Characters.Add(str);
			}
			Assert.That(wsFromLdml.CharacterSets["index"].ValueEquals(index));
			Assert.That(wsFromLdml.CharacterSets["footnotes"].ValueEquals(footnotes));
			Assert.That(wsFromLdml.CharacterSets["numeric"].ValueEquals(numeric));
		}

		[Test]
		public void Read_LdmlDelimiters()
		{
			var ldmlAdaptor = new LdmlDataMapper();
			var wsFromLdml = new WritingSystemDefinition();
			using (var tempFile = new TempFile())
			{
				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(
#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='$Revision$'/>
		<generation date='$Date$'/>
		<!-- name.en(en)='English' -->
		<language type='en'/>
		<script type='Latn'/>
	</identity>
	<delimiters>
		<quotationStart>'</quotationStart>
		<quotationEnd>'</quotationEnd>
		<alternateQuotationStart>{</alternateQuotationStart>
		<alternateQuotationEnd>}</alternateQuotationEnd>
		<special>
			<sil:matched-pairs>
				<sil:matched-pair open='mpOpen1' close='mpClose2' paraClose='false'></sil:matched-pair>
			</sil:matched-pairs>
			<sil:punctuation-patterns>
				<sil:punctuation-pattern pattern='pattern1' context='medial'></sil:punctuation-pattern>
			</sil:punctuation-patterns>
			<sil:quotation-marks paraContinueType='outer'>
				<!-- Currently parser doesn't do anything with quotationContinue or alternateQuotationContinue -->
				<sil:quotationContinue>quoteContinue1</sil:quotationContinue>
				<sil:alternateQuotationContinue>altQuoteContinue2</sil:alternateQuotationContinue>
				<sil:quotation open='open1' close='close2' continue='cont3' level='3'/>
				<sil:quotation type='narrative' level='1' open='' />
			</sil:quotation-marks>
		</special>
	</delimiters>
</ldml>".Replace("'", "\""));
#endregion
				}
				ldmlAdaptor.Read(tempFile.Path, wsFromLdml);
			}
			var mp = new MatchedPair("mpOpen1", "mpClose2", false);
			Assert.That(wsFromLdml.MatchedPairs.FirstOrDefault(), Is.EqualTo(mp));
			var pp = new PunctuationPattern("pattern1", PunctuationPatternContext.Medial);
			Assert.That(wsFromLdml.PunctuationPatterns.FirstOrDefault(), Is.EqualTo(pp));
			Assert.That(wsFromLdml.QuotationParagraphContinueType, Is.EqualTo(QuotationParagraphContinueType.Outermost));
			// Verify Level 1 normal quotation marks (quotationStart and quotationEnd)
			var qm1 = new QuotationMark("\"", "\"", null, 1, QuotationMarkingSystemType.Normal);
			Assert.That(wsFromLdml.QuotationMarks[0], Is.EqualTo(qm1));
			// Verify Level 2 normal quotation marks (alternateQuotationStart and alternateQuotationEnd)
			var qm2 = new QuotationMark("{", "}", null, 2, QuotationMarkingSystemType.Normal);
			Assert.That(wsFromLdml.QuotationMarks[1], Is.EqualTo(qm2));
			// Verify Level 3 normal quotation marks (special: sil:quotation-marks)
			var qm3 = new QuotationMark("open1", "close2", "cont3", 3, QuotationMarkingSystemType.Normal);
			Assert.That(wsFromLdml.QuotationMarks[2], Is.EqualTo(qm3));
			// Verify Level 1 narrative quotation marks (special: sil:quotation-marks)
			var qm4 = new QuotationMark("", null, null, 1, QuotationMarkingSystemType.Narrative);
			Assert.That(wsFromLdml.QuotationMarks[3], Is.EqualTo(qm4));

		}

#if WS_FIX
		[Test]
		//WS-33992
		public void Read_LdmlContainsEmptyCollationElement_SortUsingIsSetToSameAsIfNoCollationElementExisted()
		{
			const string ldmlWithEmptyCollationElement = "<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"qaa\" /></identity><dates /><collations><collation></collation></collations><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" ><palaso:version value=\"2\" /></special></ldml>";
			const string ldmlwithNoCollationElement = "<ldml><!--Comment--><identity><version number=\"\" /><generation date=\"0001-01-01T00:00:00\" /><language type=\"qaa\" /></identity><dates /><collations/><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" ><palaso:version value=\"2\" /></special></ldml>";

			string pathToLdmlWithEmptyCollationElement = Path.GetTempFileName();
			try
			{
				File.WriteAllText(pathToLdmlWithEmptyCollationElement, ldmlWithEmptyCollationElement);
				string pathToLdmlWithNoCollationElement = Path.GetTempFileName();
				try
				{
					File.WriteAllText(pathToLdmlWithNoCollationElement, ldmlwithNoCollationElement);


					var adaptor = new LdmlDataMapper();
					var wsFromEmptyCollationElement = new WritingSystemDefinition();
					adaptor.Read(pathToLdmlWithEmptyCollationElement, wsFromEmptyCollationElement);
					var wsFromNoCollationElement = new WritingSystemDefinition();
					adaptor.Read(pathToLdmlWithNoCollationElement, wsFromNoCollationElement);

					Assert.AreEqual(wsFromNoCollationElement.CollationRulesType, wsFromEmptyCollationElement.CollationRulesType);
				}
				finally
				{
					File.Delete(pathToLdmlWithNoCollationElement);
				}
			}
			finally
			{
				File.Delete(pathToLdmlWithEmptyCollationElement);
			}
		}
#endif

		[Test]
		public void Read_LdmlStandardCollation()
		{
			using (var tempFile = new TempFile())
			{
				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(
#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='$Revision$'/>
		<generation date='$Date$'/>
		<!-- name.en(aa)='Afar' -->
		<language type='aa'/>
		<script type='Latn'/>
	</identity>
	<collations>
		<defaultCollation>standard</defaultCollation>
		<collation type='standard'>
			<cr><![CDATA[
				&B<t<<<T<s<<<S<e<<<E
				&C<k<<<K<x<<<X<i<<<I
				&D<q<<<Q<r<<<R
				&G<o<<<O
				&W<h<<<H
			]]></cr>
		</collation>
	</collations>
</ldml>".Replace("'", "\""));
#endregion
				}
				var wsFromLdml = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper();

				dataMapper.Read(tempFile.Path, wsFromLdml);

				CollationDefinition cd = new CollationDefinition("standard");
				cd.IcuRules =
					"&B<t<<<T<s<<<S<e<<<E\n\t\t\t\t&C<k<<<K<x<<<X<i<<<I\n\t\t\t\t&D<q<<<Q<r<<<R\n\t\t\t\t&G<o<<<O\n\t\t\t\t&W<h<<<H";
				Assert.That(wsFromLdml.Collations.First().ValueEquals(cd));
			}
		}

		[Test]
		public void Read_LdmlSimpleCollation()
		{
			using (var tempFile = new TempFile())
			{
				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(
#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='$Revision$'/>
		<generation date='$Date$'/>
		<!-- name.en(aa)='Afar' -->
		<language type='aa'/>
		<script type='Latn'/>
	</identity>
	<collations>
		<defaultCollation>standard</defaultCollation>
		<collation type='standard'>
			<cr><![CDATA[
				&B<t<<<T<s<<<S<e<<<E
				&C<k<<<K<x<<<X<i<<<I
				&D<q<<<Q<r<<<R
				&G<o<<<O
				&W<h<<<H
			]]></cr>
			<special>
				<sil:simple><![CDATA[
					a/A
					b/B
					t/T
					s/S
					c/C
					k/K
					x/X
					i/I
					d/D
					q/Q
					r/R
					e/E
					f/F
					g/G
					o/O
					j/J
					l/L
					m/M
					n/N
					p/P
					u/U
					v/V
					w/W
					h/H
					y/Y
					z/Z
				]]></sil:simple>
			</special>
		</collation>
	</collations>
</ldml>".Replace("'", "\""));
					#endregion
				}
				var wsFromLdml = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper();

				dataMapper.Read(tempFile.Path, wsFromLdml);

				SimpleCollationDefinition cd = new SimpleCollationDefinition("standard");
				cd.SimpleRules =
					"\n\t\t\t\t\ta/A\n\t\t\t\t\tb/B\n\t\t\t\t\tt/T\n\t\t\t\t\ts/S\n\t\t\t\t\tc/C\n\t\t\t\t\tk/K\n\t\t\t\t\tx/X\n\t\t\t\t\ti/I\n\t\t\t\t\td/D\n\t\t\t\t\tq/Q\n\t\t\t\t\tr/R\n\t\t\t\t\te/E\n\t\t\t\t\tf/F\n\t\t\t\t\tg/G\n\t\t\t\t\to/O\n\t\t\t\t\tj/J\n\t\t\t\t\tl/L\n\t\t\t\t\tm/M\n\t\t\t\t\tn/N\n\t\t\t\t\tp/P\n\t\t\t\t\tu/U\n\t\t\t\t\tv/V\n\t\t\t\t\tw/W\n\t\t\t\t\th/H\n\t\t\t\t\ty/Y\n\t\t\t\t\tz/Z\n\t\t\t\t";
				cd.IcuRules =
					"&[before 1] [first regular]  < a\\/A < b\\/B < t\\/T < s\\/S < c\\/C < k\\/K < x\\/X < i\\/I < d\\/D < q\\/Q < r\\/R < e\\/E < f\\/F < g\\/G < o\\/O < j\\/J < l\\/L < m\\/M < n\\/N < p\\/P < u\\/U < v\\/V < w\\/W < h\\/H < y\\/Y < z\\/Z";
				Assert.That(wsFromLdml.Collations.First().ValueEquals(cd));
				Assert.That(wsFromLdml.DefaultCollation.ValueEquals(cd));
			}
		}

		[Test]
		public void Read_LdmlSimpleCollationNeedsCompiling()
		{
			using (var tempFile = new TempFile())
			{
				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(
					#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='$Revision$'/>
		<generation date='$Date$'/>
		<!-- name.en(aa)='Afar' -->
		<language type='aa'/>
		<script type='Latn'/>
	</identity>
	<collations>
		<defaultCollation>standard</defaultCollation>
		<collation type='standard' sil:needscompiling='true'>
			<special>
				<sil:simple><![CDATA[
					a/A
					b/B
					t/T
					s/S
					c/C
					k/K
					x/X
					i/I
					d/D
					q/Q
					r/R
					e/E
					f/F
					g/G
					o/O
					j/J
					l/L
					m/M
					n/N
					p/P
					u/U
					v/V
					w/W
					h/H
					y/Y
					z/Z
				]]></sil:simple>
			</special>
		</collation>
	</collations>
</ldml>".Replace("'", "\""));
					#endregion
				}
				var wsFromLdml = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper();

				dataMapper.Read(tempFile.Path, wsFromLdml);

				SimpleCollationDefinition cd = new SimpleCollationDefinition("standard");
				cd.SimpleRules =
					"\n\t\t\t\t\ta/A\n\t\t\t\t\tb/B\n\t\t\t\t\tt/T\n\t\t\t\t\ts/S\n\t\t\t\t\tc/C\n\t\t\t\t\tk/K\n\t\t\t\t\tx/X\n\t\t\t\t\ti/I\n\t\t\t\t\td/D\n\t\t\t\t\tq/Q\n\t\t\t\t\tr/R\n\t\t\t\t\te/E\n\t\t\t\t\tf/F\n\t\t\t\t\tg/G\n\t\t\t\t\to/O\n\t\t\t\t\tj/J\n\t\t\t\t\tl/L\n\t\t\t\t\tm/M\n\t\t\t\t\tn/N\n\t\t\t\t\tp/P\n\t\t\t\t\tu/U\n\t\t\t\t\tv/V\n\t\t\t\t\tw/W\n\t\t\t\t\th/H\n\t\t\t\t\ty/Y\n\t\t\t\t\tz/Z\n\t\t\t\t";
				cd.IcuRules =
					"&[before 1] [first regular]  < a\\/A < b\\/B < t\\/T < s\\/S < c\\/C < k\\/K < x\\/X < i\\/I < d\\/D < q\\/Q < r\\/R < e\\/E < f\\/F < g\\/G < o\\/O < j\\/J < l\\/L < m\\/M < n\\/N < p\\/P < u\\/U < v\\/V < w\\/W < h\\/H < y\\/Y < z\\/Z";
				Assert.That(wsFromLdml.Collations.First().ValueEquals(cd));
				Assert.That(wsFromLdml.DefaultCollation.ValueEquals(cd));
			}
		}

		[Test]
		public void Read_LdmlInheritedCollation()
		{
			using (var tempFile = new TempFile())
			{
				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(
					#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='$Revision$'/>
		<generation date='$Date$'/>
		<!-- name.en(aa)='Afar' -->
		<language type='aa'/>
		<script type='Latn'/>
	</identity>
	<collations>
		<defaultCollation>standard</defaultCollation>
		<collation type='standard'>
			<cr><![CDATA[
				&B<t<<<T<s<<<S<e<<<E
				&C<k<<<K<x<<<X<i<<<I
				&D<q<<<Q<r<<<R
				&G<o<<<O
				&W<h<<<H
			]]></cr>
			<special>
				<sil:inherited base='my' type='standard'/>
			</special>
		</collation>
	</collations>
</ldml>".Replace("'", "\""));
					#endregion
				}
				var wsFromLdml = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper();

				dataMapper.Read(tempFile.Path, wsFromLdml);

				InheritedCollationDefinition cd = new InheritedCollationDefinition("standard");
				cd.BaseLanguageTag = "my";
				cd.BaseType = "standard";
				cd.IcuRules =
					"&B<t<<<T<s<<<S<e<<<E\n\t\t\t\t&C<k<<<K<x<<<X<i<<<I\n\t\t\t\t&D<q<<<Q<r<<<R\n\t\t\t\t&G<o<<<O\n\t\t\t\t&W<h<<<H";
				Assert.That(wsFromLdml.Collations.First().ValueEquals(cd));
				Assert.That(wsFromLdml.DefaultCollation.ValueEquals(cd));
			}
		}

		[Test]
		public void Read_LdmlFont()
		{
			using (var tempFile = new TempFile())
			{
				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(
#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='$Revision$'/>
		<generation date='$Date$'/>
		<!-- name.en(en)='English' -->
		<language type='en'/>
		<script type='Latn'/>
	</identity>
	<special>
		<sil:external-resources>
			<sil:font types='default emphasis' name='Padauk' size='2.1' minversion='3.1.4' features='order=3 children=2 color=red createDate=1996' lang='en' engines='gr ot' otlang='abcd' subset='unknown' >
				<sil:url>http://wirl.scripts.sil.org/padauk</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=padauk</sil:url>
			</sil:font>
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\""));
					#endregion
				}
				var ws = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper();

				dataMapper.Read(tempFile.Path, ws);

				var other = new FontDefinition("Padauk");
				other.DefaultRelativeSize = 2.1f;
				other.MinVersion = "3.1.4";
				other.Features = "order=3 children=2 color=red createDate=1996";
				other.Language = "en";
				other.Engines = FontEngines.Graphite | FontEngines.OpenType;
				other.OpenTypeLanguage = "abcd";
				other.Roles = FontRoles.Default | FontRoles.Emphasis;
				other.Subset = "unknown";
				other.Urls.Add("http://wirl.scripts.sil.org/padauk");
				other.Urls.Add("http://scripts.sil.org/cms/scripts/page.php?item_id=padauk");

				Assert.That(ws.Fonts.First().ValueEquals(other));
			}
		}

		[Test]
		public void Read_LdmlSpellChecker()
		{
			using (var tempFile = new TempFile())
			{
				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(
					#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='$Revision$'/>
		<generation date='$Date$'/>
		<!-- name.en(en)='English' -->
		<language type='en'/>
		<script type='Latn'/>
	</identity>
	<special>
		<sil:external-resources>
			<sil:spellcheck type='hunspell'>
				<sil:url>http://wirl.scripts.sil.org/hunspell</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=hunspell</sil:url>
			</sil:spellcheck>
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\""));
					#endregion
				}
				var ws = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper();

				dataMapper.Read(tempFile.Path, ws);

				SpellCheckDictionaryDefinition other = new SpellCheckDictionaryDefinition("en-Latn", SpellCheckDictionaryFormat.Hunspell);
				other.Urls.Add("http://wirl.scripts.sil.org/hunspell");
				other.Urls.Add("http://scripts.sil.org/cms/scripts/page.php?item_id=hunspell");

				Assert.That(ws.SpellCheckDictionaries.First().ValueEquals(other));
			}
		}

		[Test]
		public void Read_LdmlKeyboard()
		{
			using (var tempFile = new TempFile())
			{
				using (var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(
					#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='$Revision$'/>
		<generation date='$Date$'/>
		<!-- name.en(en)='English' -->
		<language type='en'/>
		<script type='Latn'/>
	</identity>
	<special>
		<sil:external-resources>
			<sil:kbd id='Compiled Keyman9' type='kmx'>
				<sil:url>http://wirl.scripts.sil.org/keyman</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=keyman9</sil:url>
			</sil:kbd>
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\""));
					#endregion
				}
				var ws = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper();

				dataMapper.Read(tempFile.Path, ws);

				List<string>urls = new List<string>();
				urls.Add("http://wirl.scripts.sil.org/keyman");
				urls.Add("http://scripts.sil.org/cms/scripts/page.php?item_id=keyman9");
				IKeyboardDefinition other = Keyboard.Controller.CreateKeyboardDefinition("Compiled Keyman9", KeyboardFormat.CompiledKeyman, urls);

				Assert.That(ws.KnownKeyboards.First(), Is.EqualTo(other));
			}
		}

		[Test]
		public void Read_LdmlContainsOnlyPrivateUse_IsoAndprivateUseSetCorrectly()
		{
			const string ldmlWithOnlyPrivateUse = "<ldml><identity><version number=\"\" /><language type=\"\" /><variant type=\"x-private-use\" /></identity><special xmlns:palaso=\"urn://palaso.org/ldmlExtensions/v1\" ><palaso:version value=\"2\" /></special></ldml>";


			string pathToLdmlWithEmptyCollationElement = Path.GetTempFileName();
			try
			{
				File.WriteAllText(pathToLdmlWithEmptyCollationElement, ldmlWithOnlyPrivateUse);

				var adaptor = new LdmlDataMapper();
				var wsFromLdml = new WritingSystemDefinition();
				adaptor.Read(pathToLdmlWithEmptyCollationElement, wsFromLdml);
				var ws = new WritingSystemDefinition();
				adaptor.Read(pathToLdmlWithEmptyCollationElement, ws);
				Assert.That(wsFromLdml.Language, Is.EqualTo((LanguageSubtag) "private"));
				Assert.That(wsFromLdml.Variants, Is.EqualTo(new VariantSubtag[] {"use"}));
			}
			finally
			{
				File.Delete(pathToLdmlWithEmptyCollationElement);
			}
		}

		[Test]
		public void Write_LdmlIsNicelyFormatted()
		{
#if MONO
				// mono inserts \r\n\t before xmlns where windows doesn't
			string expectedFileContent =
#region filecontent
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='0001-01-01T00:00:00' />
		<language
			type='en' />
		<script
			type='Zxxx' />
		<territory
			type='US' />
		<variant
			type='x-audio' />
	</identity>
	<collations />
	<special
		xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation
			value='en' />
		<palaso:languageName
			value='English' />
		<palaso:version
			value='2' />
	</special>
</ldml>".Replace("'", "\"").Replace("\n", "\r\n");
#endregion

#else
			string expectedFileContent =
#region filecontent
 @"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='' />
		<generation date='0001-01-01T00:00:00' />
		<language type='en' />
		<script type='Zxxx' />
		<territory type='US' />
		<variant type='x-audio' />
		<special>
			<sil:identity variantName='Audio' />
		</special>
	</identity>
	<layout>
		<orientation>
			<characterOrder>left-to-right</characterOrder>
		</orientation>
	</layout>
	<collations>
		<defaultCollation>standard</defaultCollation>
		<collation type='standard' />
	</collations>
</ldml>".Replace("'", "\"").Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");

#endregion

#endif
			using (var file = new TempFile())
			{
				//Create an ldml file to read
				var adaptor = new LdmlDataMapper();
				var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
				adaptor.Write(file.Path, ws, null);

				//change the read writing system and write it out again
				var ws2 = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws2);
				ws2.Region = "US";
				adaptor.Write(file.Path, ws2, new MemoryStream(File.ReadAllBytes(file.Path)));
				Assert.That(File.ReadAllText(file.Path), Is.EqualTo(expectedFileContent));
			}
		}

		[Test]
		public void Write_WritingSystemWasloadedFromLdmlThatContainedLayoutInfo_LayoutInfoIsOnlyWrittenOnce()
		{
			using (var file = new TempFile())
			{
				//create an ldml file to read that contains layout info
				var adaptor = new LdmlDataMapper();
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

#if WS_FIX
		[Test]
		public void Read_ValidLanguageTagStartingWithXButVersion0_Throws()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("xh", "", "", "", file);
				var adaptor = new LdmlDataMapper();
				Assert.That(() => adaptor.Read(file.Path, new WritingSystemDefinition()), Throws.Exception.TypeOf<ApplicationException>());
			}
		}

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
				WriteVersion2Ldml("xh", "", "", "", file);
				var adaptor = new LdmlDataMapper();
				var ws = new WritingSystemDefinition();
				adaptor.Read(file.Path, ws);
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path), true));
				AssertThatLdmlMatches("xh", "", "", "", file);
#if WS_FIX
				AssertThatVersionIs(2, file);
#endif
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatOnlyLanguageIsPopulated_WritingSystemHasDataInPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, new LanguageSubtag("en", true), null, null, Enumerable.Empty<VariantSubtag>());
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatLanguageAndScriptArePopulated_PrivateUseLanguageMovedToPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, new LanguageSubtag("en", true), "Zxxx", null, Enumerable.Empty<VariantSubtag>());
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatLanguageAndTerritoryArePopulated_PrivateUseLanguageMovedToPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "US", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, new LanguageSubtag("en", true), null, "US", Enumerable.Empty<VariantSubtag>());
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatLanguageAndVariantArePopulated_PrivateUseLanguageMovedToPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "fonipa", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, new LanguageSubtag("en", true), null, null, new VariantSubtag[] {"fonipa"});
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatAllFieldsArePopulated_PrivateUseLanguageMovedToPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, new LanguageSubtag("en", true), "Zxxx", "US", new VariantSubtag[] {"1901", "audio"});
			}
		}

		[Test]
		public void Read_LdmlIsFlexPrivateUseFormatLanguageAndPrivateUseIsPopulated_LanguageTagIsMovedAndIsFirstInPrivateUse()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "x-private", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, new LanguageSubtag("en", true), null, null, new VariantSubtag[] {"private"});
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
		public void Write_OriginalWasFlexPrivateUseWritingSystemButNowChangedLanguage_IdentityElementChangedToPalasoWay()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, new LanguageSubtag("en", true), null, null, Enumerable.Empty<VariantSubtag>());
				ws.Language = "de";
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)));
				AssertThatLdmlMatches("de", "", "", "", file);
			}
		}

		[Test]
		public void Write_OriginalWasFlexPrivateUseWritingSystemButNowChangedScript_IdentityElementChangedToPalasoWay()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "", "", file);
				var ws = new WritingSystemDefinition();
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, new LanguageSubtag("en", true), "Zxxx", null, Enumerable.Empty<VariantSubtag>());
				ws.Script = "Latn";
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)));
				AssertThatLdmlMatches("qaa", "Latn", "", "x-en", file);
			}
		}

		[Test]
		public void Write_OriginalWasFlexPrivateUseWritingSystemButNowChangedRegion_IdentityElementChangedToPalasoWay()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "US", "", file);
				var ws = new WritingSystemDefinition();
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, new LanguageSubtag("en", true), null, "US", Enumerable.Empty<VariantSubtag>());
				ws.Region = "GB";
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)));
				AssertThatLdmlMatches("qaa", "", "GB", "x-en", file);
			}
		}

		[Test]
		public void Write_OriginalWasFlexPrivateUseWritingSystemButNowChangedVariant_IdentityElementChangedToPalasoWay()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "fonipa", file);
				var ws = new WritingSystemDefinition();
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, new LanguageSubtag("en", true), null, null, new VariantSubtag[] {"fonipa"});
				ws.Variants.Clear();
				ws.Variants.Add("1901");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)));
				AssertThatLdmlMatches("qaa", "", "", "1901-x-en", file);
			}
		}

		[Test]
		public void Write_OriginalWasFlexPrivateUseWritingSystemButNowChangedPrivateUse_IdentityElementChangedToPalasoWay()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "x-private", file);
				var ws = new WritingSystemDefinition();
				var adaptor = new LdmlDataMapper();
				adaptor.Read(file.Path, ws);
				AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(ws, new LanguageSubtag("en", true), null, null, new VariantSubtag[] {"private"});
				ws.Variants.Clear();
				ws.Variants.Add("changed");
				adaptor.Write(file.Path, ws, new MemoryStream(File.ReadAllBytes(file.Path)));
				AssertThatLdmlMatches("qaa", "", "", "x-en-changed", file);
			}
		}

#if WS_FIX
		[Test]
		public void Read_ReadPrivateUseWsFromFieldWorksLdmlThenNormalLdmlMissingVersion1Element_Throws()
		{
			using (var badFlexLdml = new TempFile())
			{
				using (var version1Ldml = new TempFile())
				{
					WriteVersion0Ldml("x-en", "", "", "x-private", badFlexLdml);
					WriteVersion0Ldml("en", "", "", "", version1Ldml);
					var wsV1 = new WritingSystemDefinition();
					var wsV0 = new WritingSystemDefinition();
					var adaptor = new LdmlDataMapper();
					adaptor.Read(badFlexLdml.Path, wsV0);
					Assert.Throws<ApplicationException>(()=>adaptor.Read(version1Ldml.Path, wsV1));
				}
			}
		}

		[Test]
		public void Write_WritePrivateUseWsFromFieldWorksLdmlThenNormalLdml_ContainsVersion1()
		{
			using (var badFlexLdml = new TempFile())
			{
				using (var version1Ldml = new TempFile())
				{
					var namespaceManager = new XmlNamespaceManager(new NameTable());
					namespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
					WriteVersion0Ldml("x-en", "", "", "x-private", badFlexLdml);
					var wsV0 = new WritingSystemDefinition();
					var adaptor = new LdmlDataMapper();
					adaptor.Read(badFlexLdml.Path, wsV0);
					adaptor.Write(badFlexLdml.Path, wsV0, new MemoryStream(File.ReadAllBytes(badFlexLdml.Path)));
					var wsV1 = new WritingSystemDefinition();
					adaptor.Write(version1Ldml.Path, wsV1, null);
					AssertThatVersionIs(2, version1Ldml);
				}
			}
		}
#endif

		[Test]
		public void Read_NonDescriptLdml_WritingSystemIdIsSameAsRfc5646Tag()
		{
			using (var file = new TempFile())
			{
				WriteVersion2Ldml("en", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				Assert.That(ws.Id, Is.EqualTo("en-Zxxx-US-1901-x-audio"));
			}
		}

		[Test]
		public void Read_FlexEntirelyPrivateUseLdmlContainingLanguageScriptRegionVariant_WritingSystemIdIsConcatOfLanguageScriptRegionVariant()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "US", "1901-x-audio", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				Assert.That(ws.Id, Is.EqualTo("x-en-Zxxx-US-1901-x-audio"));
			}
		}

		[Test]
		public void Read_FlexEntirelyPrivateUseLdmlContainingLanguage_WritingSystemIdIsLanguage()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				Assert.That(ws.Id, Is.EqualTo("x-en"));
			}
		}

		[Test]
		public void Read_FlexEntirelyPrivateUseLdmlContainingLanguageScript_WritingSystemIdIsConcatOfLanguageScript()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "Zxxx", "", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				Assert.That(ws.Id, Is.EqualTo("x-en-Zxxx"));
			}
		}

		[Test]
		public void Read_FlexEntirelyPrivateUseLdmlContainingLanguageRegion_WritingSystemIdIsConcatOfLanguageRegion()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("x-en", "", "US", "", file);
				var ws = new WritingSystemDefinition();
				new LdmlDataMapper().Read(file.Path, ws);
				Assert.That(ws.Id, Is.EqualTo("x-en-US"));
			}
		}

#if WS_FIX
		[Test]
		public void Read_V0Ldml_ThrowFriendlyException()
		{
			using (var file = new TempFile())
			{
				WriteVersion0Ldml("en", "", "", "", file);
				var ws = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper();
				Assert.That(() => dataMapper.Read(file.Path, ws),
								Throws.Exception.TypeOf<ApplicationException>()
										.With.Property("Message")
										.EqualTo(String.Format("The LDML tag 'en' is version 0.  Version {0} was expected.", 
										WritingSystemDefinition.LatestWritingSystemDefinitionVersion)));
			}
		}
#endif

		[Test]
		public void RoundTrippingLdmlDoesNotDuplicateSections()
		{
			using(var roundTripOut2 = new TempFile())
			using(var roundTripOut = new TempFile())
			using(var tempFile = new TempFile())
			{

				using(var writer = new StreamWriter(tempFile.Path, false, Encoding.UTF8))
				{
					writer.Write(
						@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<language
			type='qaa' />
		<variant
			type='x-lel' />
	</identity>
	<collations />

	<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1'>
		<fw:graphiteEnabled
			value='False' />
		<fw:windowsLCID
			value='1036' />
	</special>
</ldml>".Replace("'", "\""));
				}
				var ws = new WritingSystemDefinition();
				var dataMapper = new LdmlDataMapper();

				dataMapper.Read(tempFile.Path, ws);
				var keyboard1 = new DefaultKeyboardDefinition("MyFavoriteKeyboard", string.Empty);
				ws.KnownKeyboards.Add(keyboard1);
				using(var fileStream = new FileStream(tempFile.Path, FileMode.Open))
				{
					dataMapper.Write(roundTripOut.Path, ws, fileStream);
				}
				AssertThatXmlIn.File(roundTripOut.Path).HasSpecifiedNumberOfMatchesForXpath("/ldml/special/*[local-name()='windowsLCID']", 1);
				var secondTripMapper = new LdmlDataMapper();
				var secondTripWs = new WritingSystemDefinition();
				secondTripMapper.Read(roundTripOut.Path, secondTripWs);
				secondTripWs.KnownKeyboards.Add(new DefaultKeyboardDefinition("x-tel", string.Empty));
				secondTripWs.WindowsLcid = "1037";
				using(var fileStream = new FileStream(roundTripOut.Path, FileMode.Open))
				{
					secondTripMapper.Write(roundTripOut2.Path, secondTripWs, fileStream);
				}
				AssertThatXmlIn.File(roundTripOut2.Path).HasSpecifiedNumberOfMatchesForXpath("/ldml/special/*[local-name()='windowsLCID']", 1); //Element duplicated on round trip
			}
		}

		private static void AssertThatRfcTagComponentsOnWritingSystemAreEqualTo(WritingSystemDefinition ws, LanguageSubtag language, ScriptSubtag script, RegionSubtag territory, IEnumerable<VariantSubtag> variants)
		{
			Assert.That(ws.Language, Is.EqualTo(language));
			Assert.That(ws.Script, Is.EqualTo(script));
			Assert.That(ws.Region, Is.EqualTo(territory));
			Assert.That(ws.Variants, Is.EqualTo(variants));
		}

		private static void WriteVersion0Ldml(string language, string script, string territory, string variant, TempFile file)
		{
			//using a writing system V0 here because the real writing system can't cope with the way
			//flex encodes private-use language and shouldn't. But using a writing system + ldml adaptor
			//is the quickest way to generate ldml so I'm using it here.
			var ws = new WritingSystemDefinitionV0
						 {ISO639 = language, Script = script, Region = territory, Variant = variant};
			new LdmlAdaptorV0().Write(file.Path, ws, null);
		}

		private static void WriteVersion2Ldml(string language, string script, string territory, string variant, TempFile file)
		{
			var ws = new WritingSystemDefinition();
			ws.SetAllComponents(language, script, territory, variant);
			new LdmlDataMapper().Write(file.Path, ws, null);
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

		private static void AssertThatVersionIs(int expectedVersion, TempFile file)
		{
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");

			if(expectedVersion == 0)
			{
				AssertThatXmlIn.File(file.Path).HasNoMatchForXpath("/ldml/special/palaso:version[@value]", namespaceManager);
			}
			else
			{
				AssertThatXmlIn.File(file.Path).HasAtLeastOneMatchForXpath(
					String.Format("/ldml/special/palaso:version[@value='{0}']", expectedVersion), namespaceManager);
			}
		}
	}
}
