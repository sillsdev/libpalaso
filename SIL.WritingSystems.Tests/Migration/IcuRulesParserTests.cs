using System;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using SIL.Xml;

namespace SIL.WritingSystems.Tests.Migration
{
	[TestFixture]
	public class IcuRulesParserTests
	{
		private IcuRulesParser _icuParser;
		private XmlWriter _writer;
		private StringBuilder _xmlText;

		[SetUp]
		public void SetUp()
		{
			_icuParser = new IcuRulesParser();
			_xmlText = new StringBuilder();
			_writer = XmlWriter.Create(_xmlText, CanonicalXmlSettings.CreateXmlWriterSettings(ConformanceLevel.Fragment));
		}

		private string Environment_OutputString()
		{
			_writer.Close();
			return _xmlText.ToString();
		}

		[Test]
		public void EmptyString_ProducesNoRules()
		{
			_icuParser.WriteIcuRules(_writer, string.Empty);
			string result = Environment_OutputString();
			Assert.AreEqual("", result);
		}

		[Test]
		public void WhiteSpace_ProducesNoRules()
		{
			_icuParser.WriteIcuRules(_writer, "   \n\t");
			string result = Environment_OutputString();
			Assert.AreEqual("", result);
		}

		[Test]
		public void Reset_ProducesResetNode()
		{
			_icuParser.WriteIcuRules(_writer, "&a");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
		}

		[Test]
		public void PrimaryDifference_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a < b");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/p[text()='b']"
			);
		}

		[Test]
		public void SecondaryDifference_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a << b");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/s[text()='b']"
			);
		}

		[Test]
		public void TertiaryDifference_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a <<< b");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/t[text()='b']"
			);
		}

		[Test]
		public void NoDifference_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a = b");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/i[text()='b']"
			);
		}

		[Test]
		public void Prefix_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a < b|c");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/x/context[text()='b']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/x/p[text()='c']"
			);
		}

		[Test]
		public void Expansion_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a < b/ c");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/x/p[text()='b']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/x/extend[text()='c']"
			);
		}

		[Test]
		public void PrefixAndExpansion_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a < b|c/d");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/x/context[text()='b']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/x/p[text()='c']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/x/extend[text()='d']"
			);
		}

		[Test]
		public void Contraction_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&ab < cd");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='ab']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/following-sibling::p[text()='cd']"
			);
		}

		[Test]
		public void EscapedUnicode_ProducesCorrectXml()
		{
			//Fieldworks issue LT-11999
			_icuParser.WriteIcuRules(_writer, "&\\U00008000 < \\u0061");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='\\U00008000']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/following-sibling::p[text()='\\u0061']"
			);
		}

		[Test]
		public void MultiplePrimaryDifferences_ProducesOptimizedXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a < b<c");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/pc[text()='bc']"
			);
			//            Assert.AreEqual("<rules><reset>a</reset><pc>bc</pc></rules>", _xmlText.ToString());
		}

		[Test]
		public void MultipleSecondaryDifferences_ProducesOptimizedXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a << b<<c");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/sc[text()='bc']"
			);
			//            Assert.AreEqual("<rules><reset>a</reset><sc>bc</sc></rules>", _xmlText.ToString());
		}

		[Test]
		public void MultipleTertiaryDifferences_ProducesOptimizedXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a <<< b<<<c");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/tc[text()='bc']"
			);
			//            Assert.AreEqual("<rules><reset>a</reset><tc>bc</tc></rules>", _xmlText.ToString());
		}

		[Test]
		public void MultipleNoDifferences_ProducesOptimizedXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a = b=c");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/ic[text()='bc']"
			);
			//            Assert.AreEqual("<rules><reset>a</reset><ic>bc</ic></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstTertiaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first tertiary ignorable]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/first_tertiary_ignorable"
			);
			//            Assert.AreEqual("<rules><reset><first_tertiary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastTertiaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last tertiary ignorable]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/last_tertiary_ignorable"
			);
//            Assert.AreEqual("<rules><reset><last_tertiary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstSecondarygnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first secondary ignorable]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/first_secondary_ignorable"
			);
//            Assert.AreEqual("<rules><reset><first_secondary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastSecondaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last secondary ignorable]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/last_secondary_ignorable"
			);
//            Assert.AreEqual("<rules><reset><last_secondary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstPrimaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first primary ignorable]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/first_primary_ignorable"
			);
//            Assert.AreEqual("<rules><reset><first_primary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastPrimaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last primary ignorable]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/last_primary_ignorable"
			);
//            Assert.AreEqual("<rules><reset><last_primary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstVariable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first variable]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/first_variable"
			);
//            Assert.AreEqual("<rules><reset><first_variable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastVariable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last variable]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/last_variable"
			);
//            Assert.AreEqual("<rules><reset><last_variable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstRegular_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first regular]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/first_non_ignorable"
			);
//            Assert.AreEqual("<rules><reset><first_non_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastRegular_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last regular]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/last_non_ignorable"
			);
//            Assert.AreEqual("<rules><reset><last_non_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstTrailing_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first trailing]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/first_trailing"
			);
//            Assert.AreEqual("<rules><reset><first_trailing /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastTrailing_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last trailing]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/last_trailing"
			);
//            Assert.AreEqual("<rules><reset><last_trailing /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstImplicit_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&a < [first implicit]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/p/first_implicit"
			);
			//            Assert.AreEqual("<rules><reset>a</reset><p><first_implicit /></p></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastImplicit_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&a < [last implicit]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/p/last_implicit"
			);
			//            Assert.AreEqual("<rules><reset>a</reset><p><last_implicit /></p></rules>", _xmlText.ToString());
		}

		[Test]
		public void Top_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[top]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset/last_non_ignorable"
			);
//            Assert.AreEqual("<rules><reset><last_non_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Before1_ProducesCorrectResetNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[before 1]a");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[@before='primary' and text()='a']"
			);
//            Assert.AreEqual("<rules><reset before=\"primary\">a</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Before2_ProducesCorrectResetNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[before 2]a");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[@before='secondary' and text()='a']"
			);
//            Assert.AreEqual("<rules><reset before=\"secondary\">a</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Before3_ProducesCorrectResetNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[before 3]a");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[@before='tertiary' and text()='a']"
			);
//            Assert.AreEqual("<rules><reset before=\"tertiary\">a</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void TwoRules_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a<b \n&c<<d");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[1 and text()='a']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[1]/following-sibling::p[text()='b']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[2 and text()='c']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[2]/following-sibling::s[text()='d']"
			);
			//            Assert.AreEqual("<rules><reset>a</reset><p>b</p><reset>c</reset><s>d</s></rules>", _xmlText.ToString());
		}

		[Test]
		public void AlternateShiftedOption_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[alternate shifted]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath("/settings[@alternate='shifted']");
		}

		[Test]
		public void AlternateNonIgnorableOption_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[alternate non-ignorable]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath("/settings[@alternate='non-ignorable']");
}

		[Test]
		public void Strength1Option_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength 1]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@strength='primary']"
			);
//            Assert.AreEqual("<settings strength=\"primary\" />", _xmlText.ToString());
		}

		[Test]
		public void Strength2Option_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength 2]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@strength='secondary']"
			);
//            Assert.AreEqual("<settings strength=\"secondary\" />", _xmlText.ToString());
		}

		[Test]
		public void Strength3Option_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength 3]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@strength='tertiary']"
			);
//            Assert.AreEqual("<settings strength=\"tertiary\" />", _xmlText.ToString());
		}

		[Test]
		public void Strength4Option_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength 4]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@strength='quaternary']"
			);
//            Assert.AreEqual("<settings strength=\"quaternary\" />", _xmlText.ToString());
		}

		[Test]
		public void Strength5Option_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength 5]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@strength='identical']"
			);
//            Assert.AreEqual("<settings strength=\"identical\" />", _xmlText.ToString());
		}

		[Test]
		public void StrengthIOption_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength I]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@strength='identical']"
			);
//            Assert.AreEqual("<settings strength=\"identical\" />", _xmlText.ToString());
		}

		[Test]
		public void Backwards1_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[backwards 1]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@backwards='off']"
			);
//            Assert.AreEqual("<settings backwards=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void Backwards2_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[backwards 2]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@backwards='on']"
			);
//            Assert.AreEqual("<settings backwards=\"on\" />", _xmlText.ToString());
		}

		[Test]
		public void NormalizationOn_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[normalization on]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@normalization='on']"
			);
//            Assert.AreEqual("<settings normalization=\"on\" />", _xmlText.ToString());
		}

		[Test]
		public void NormalizationOff_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[normalization off]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@normalization='off']"
			);
//            Assert.AreEqual("<settings normalization=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void CaseLevelOn_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[caseLevel on]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@caseLevel='on']"
			);
//            Assert.AreEqual("<settings caseLevel=\"on\" />", _xmlText.ToString());
		}

		[Test]
		public void CaseLevelOff_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[caseLevel off]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@caseLevel='off']"
			);
//            Assert.AreEqual("<settings caseLevel=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void CaseFirstOff_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[caseFirst off]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@caseFirst='off']"
			);
//            Assert.AreEqual("<settings caseFirst=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void CaseFirstLower_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[caseFirst lower]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@caseFirst='lower']"
			);
//            Assert.AreEqual("<settings caseFirst=\"lower\" />", _xmlText.ToString());
		}

		[Test]
		public void CaseFirstUpper_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[caseFirst upper]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@caseFirst='upper']"
			);
//            Assert.AreEqual("<settings caseFirst=\"upper\" />", _xmlText.ToString());
		}

		[Test]
		public void HiraganaQOff_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[hiraganaQ off]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@hiraganaQuaternary='off']"
			);
//            Assert.AreEqual("<settings hiraganaQuaternary=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void HiraganaQOn_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[hiraganaQ on]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@hiraganaQuaternary='on']"
			);
//            Assert.AreEqual("<settings hiraganaQuaternary=\"on\" />", _xmlText.ToString());
		}

		[Test]
		public void NumericOff_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[numeric off]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@numeric='off']"
			);
//            Assert.AreEqual("<settings numeric=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void NumericOn_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[numeric on]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/settings[@numeric='on']"
			);
//            Assert.AreEqual("<settings numeric=\"on\" />", _xmlText.ToString());
		}

		[Test]
		public void VariableTop_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "& A < [variable top]");
			string result = "<root>" + Environment_OutputString() + "</root>";
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/root/settings[@variableTop='u41']"
			);
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/root/rules/reset[text()='A']"
			);
			//            Assert.AreEqual("<settings variableTop=\"u41\" /><rules><reset>A</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void SuppressContractions_ProducesCorrectNode()
		{
			_icuParser.WriteIcuRules(_writer, "[suppress contractions [abc]]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/suppress_contractions[text()='[abc]']"
			);
//            Assert.AreEqual("<suppress_contractions>[abc]</suppress_contractions>", _xmlText.ToString());
		}

		[Test]
		public void Optimize_ProducesCorrectNode()
		{
			_icuParser.WriteIcuRules(_writer, "[optimize [abc]]");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/optimize[text()='[abc]']"
			);
//            Assert.AreEqual("<optimize>[abc]</optimize>", _xmlText.ToString());
		}

		// Most of these escapes aren't actually handled by ICU - it just treats the character
		// following backslash as a literal.  These tests just check for no other special escape
		// handling that is invalid.
		[Test]
		public void Escape_x_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\x41");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='x41']"
			);
//            Assert.AreEqual("<rules><reset>x41</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_octal_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\102");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='102']"
			);
//            Assert.AreEqual("<rules><reset>102</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_c_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\c\u0083");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='c\u0083']"
			);
//            Assert.AreEqual("<rules><reset>c\u0083</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_a_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\a");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='a']"
			);
//            Assert.AreEqual("<rules><reset>a</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_b_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\b");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='b']"
			);
//            Assert.AreEqual("<rules><reset>b</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_t_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\t");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='t']"
			);
//            Assert.AreEqual("<rules><reset>t</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_n_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\n");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='n']"
			);
//            Assert.AreEqual(String.Format("<rules><reset>n</reset></rules>", _writer.Settings.NewLineChars), _xmlText.ToString());
		}

		[Test]
		public void Escape_v_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\v");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='v']"
			);
//            Assert.AreEqual("<rules><reset>v</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_f_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\f");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='f']"
			);
//            Assert.AreEqual("<rules><reset>f</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_r_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\r");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='r']"
			);
//            Assert.AreEqual("<rules><reset>r</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_OtherChar_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\\\");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()='\\']"
			);
//            Assert.AreEqual("<rules><reset>\\</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void TwoSingleQuotes_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&''");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()=\"'\"]"
			);
//            Assert.AreEqual("<rules><reset>'</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void QuotedString_ProducesCorrectString()
		{
			_icuParser.WriteIcuRules(_writer, "&'''\\<&'");
			string result = Environment_OutputString();
			AssertThatXmlIn.String(result).HasAtLeastOneMatchForXpath(
				"/rules/reset[text()=\"'\\<&\"]"
			);
//            Assert.AreEqual("<rules><reset>'\\&lt;&amp;</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void InvalidIcu_Throws()
		{

			Assert.Throws<ApplicationException>(
				() => _icuParser.WriteIcuRules(_writer, "&a <<<< b"));
			_writer.Close();
		}

		[Test]
		public void InvalidIcu_NoXmlProduced()
		{
			try
			{
				_icuParser.WriteIcuRules(_writer, "&a <<<< b");
			}
			// ReSharper disable EmptyGeneralCatchClause
			catch {}
			// ReSharper restore EmptyGeneralCatchClause
			string result = Environment_OutputString();
			Assert.IsTrue(String.IsNullOrEmpty(result));
		}

		[Test]
		public void ValidateIcuRules_ValidIcu_ReturnsTrue()
		{
			string message;
			Assert.IsTrue(_icuParser.ValidateIcuRules("&a < b <<< c < e/g\n&[before 1]m<z", out message));
			Assert.AreEqual(string.Empty, message);
		}

		[Test]
		public void ValidateIcuRules_InvalidIcu_ReturnsFalse()
		{
			string message;
			Assert.IsFalse(_icuParser.ValidateIcuRules("&a < b < c(", out message));
			Assert.IsNotEmpty(message);
		}

		[Test]
		public void BigCombinedRule_ParsesCorrectly()
		{
			// certainly some of this actually doesn't form semantically vaild ICU, but it should be syntactically correct
			const string icu = "[strength 3] [alternate shifted]\n[backwards 2]&[before 1][ first  regular]<b<\\u"
							   + "<'cde'&gh<<p<K|Q/\\<<[last variable]<<4<[variable\ttop]\t<9";
			_icuParser.WriteIcuRules(_writer, icu);
			string result = Environment_OutputString();
			string expected = CanonicalXml.ToCanonicalStringFragment(
				  "<settings alternate=\"shifted\" backwards=\"on\" variableTop=\"u34\" strength=\"tertiary\" />"
				+ "<rules><reset before=\"primary\"><first_non_ignorable /></reset>"
				+ "<pc>bu</pc><p>cde</p><reset>gh</reset><s>p</s>"
				+ "<x><context>K</context><p>Q</p><extend>&lt;</extend></x>"
				+ "<p><last_variable /></p><s>4</s><p>9</p></rules>"
			);
			Assert.AreEqual(expected, result);
		}
	}
}
