using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.WritingSystems.Collation;

namespace Palaso.Tests.WritingSystems.Collation
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
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			settings.Indent = false;
			//settings.NewLineChars = String.Empty;
			settings.OmitXmlDeclaration = true;
			_writer = XmlWriter.Create(_xmlText, settings);
		}

		[Test]
		public void EmptyString_ProducesNoRules()
		{
			_icuParser.WriteIcuRules(_writer, string.Empty);
			_writer.Close();
			Assert.AreEqual("", _xmlText.ToString());
		}

		[Test]
		public void WhiteSpace_ProducesNoRules()
		{
			_icuParser.WriteIcuRules(_writer, "   \n\t");
			_writer.Close();
			Assert.AreEqual("", _xmlText.ToString());
		}

		[Test]
		public void Reset_ProducesResetNode()
		{
			_icuParser.WriteIcuRules(_writer, "&a");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void PrimaryDifference_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a < b");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><p>b</p></rules>", _xmlText.ToString());
		}

		[Test]
		public void SecondaryDifference_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a << b");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><s>b</s></rules>", _xmlText.ToString());
		}

		[Test]
		public void TertiaryDifference_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a <<< b");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><t>b</t></rules>", _xmlText.ToString());
		}

		[Test]
		public void NoDifference_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a = b");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><i>b</i></rules>", _xmlText.ToString());
		}

		[Test]
		public void Prefix_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a < b|c");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><x><context>b</context><p>c</p></x></rules>", _xmlText.ToString());
		}

		[Test]
		public void Expansion_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a < b/ c");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><x><p>b</p><extend>c</extend></x></rules>", _xmlText.ToString());
		}

		[Test]
		public void PrefixAndExpansion_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a < b|c/d");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><x><context>b</context><p>c</p><extend>d</extend></x></rules>", _xmlText.ToString());
		}

		[Test]
		public void Contraction_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&ab < cd");
			_writer.Close();
			Assert.AreEqual("<rules><reset>ab</reset><p>cd</p></rules>", _xmlText.ToString());
		}

		[Test]
		public void MultiplePrimaryDifferences_ProducesOptimizedXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a < b<c");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><pc>bc</pc></rules>", _xmlText.ToString());
		}

		[Test]
		public void MultipleSecondaryDifferences_ProducesOptimizedXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a << b<<c");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><sc>bc</sc></rules>", _xmlText.ToString());
		}

		[Test]
		public void MultipleTertiaryDifferences_ProducesOptimizedXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a <<< b<<<c");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><tc>bc</tc></rules>", _xmlText.ToString());
		}

		[Test]
		public void MultipleNoDifferences_ProducesOptimizedXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a = b=c");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><ic>bc</ic></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstTertiaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first tertiary ignorable]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><first_tertiary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastTertiaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last tertiary ignorable]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><last_tertiary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstSecondarygnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first secondary ignorable]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><first_secondary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastSecondaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last secondary ignorable]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><last_secondary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstPrimaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first primary ignorable]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><first_primary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastPrimaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last primary ignorable]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><last_primary_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstVariable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first variable]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><first_variable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastVariable_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last variable]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><last_variable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstRegular_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first regular]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><first_non_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastRegular_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last regular]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><last_non_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstTrailing_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[first trailing]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><first_trailing /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastTrailing_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[last trailing]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><last_trailing /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void FirstImplicit_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&a < [first implicit]");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><p><first_implicit /></p></rules>", _xmlText.ToString());
		}

		[Test]
		public void LastImplicit_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&a < [last implicit]");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><p><last_implicit /></p></rules>", _xmlText.ToString());
		}

		[Test]
		public void Top_ProducesCorrectIndirectNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[top]");
			_writer.Close();
			Assert.AreEqual("<rules><reset><last_non_ignorable /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Before1_ProducesCorrectResetNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[before 1]a");
			_writer.Close();
			Assert.AreEqual("<rules><reset before=\"primary\">a</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Before2_ProducesCorrectResetNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[before 2]a");
			_writer.Close();
			Assert.AreEqual("<rules><reset before=\"secondary\">a</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Before3_ProducesCorrectResetNode()
		{
			_icuParser.WriteIcuRules(_writer, "&[before 3]a");
			_writer.Close();
			Assert.AreEqual("<rules><reset before=\"tertiary\">a</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void TwoRules_ProducesCorrectXml()
		{
			_icuParser.WriteIcuRules(_writer, "&a<b \n&c<<d");
			_writer.Close();
			Assert.AreEqual("<rules><reset>a</reset><p>b</p><reset>c</reset><s>d</s></rules>", _xmlText.ToString());
		}

		[Test]
		public void AlternateShiftedOption_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[alternate shifted]");
			_writer.Close();
			Assert.AreEqual("<settings alternate=\"shifted\" />", _xmlText.ToString());
		}

		[Test]
		public void AlternateNonIgnorableOption_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[alternate non-ignorable]");
			_writer.Close();
			Assert.AreEqual("<settings alternate=\"non-ignorable\" />", _xmlText.ToString());
		}

		[Test]
		public void Strength1Option_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength 1]");
			_writer.Close();
			Assert.AreEqual("<settings strength=\"primary\" />", _xmlText.ToString());
		}

		[Test]
		public void Strength2Option_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength 2]");
			_writer.Close();
			Assert.AreEqual("<settings strength=\"secondary\" />", _xmlText.ToString());
		}

		[Test]
		public void Strength3Option_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength 3]");
			_writer.Close();
			Assert.AreEqual("<settings strength=\"tertiary\" />", _xmlText.ToString());
		}

		[Test]
		public void Strength4Option_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength 4]");
			_writer.Close();
			Assert.AreEqual("<settings strength=\"quaternary\" />", _xmlText.ToString());
		}

		[Test]
		public void Strength5Option_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength 5]");
			_writer.Close();
			Assert.AreEqual("<settings strength=\"identical\" />", _xmlText.ToString());
		}

		[Test]
		public void StrengthIOption_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[strength I]");
			_writer.Close();
			Assert.AreEqual("<settings strength=\"identical\" />", _xmlText.ToString());
		}

		[Test]
		public void Backwards1_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[backwards 1]");
			_writer.Close();
			Assert.AreEqual("<settings backwards=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void Backwards2_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[backwards 2]");
			_writer.Close();
			Assert.AreEqual("<settings backwards=\"on\" />", _xmlText.ToString());
		}

		[Test]
		public void NormalizationOn_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[normalization on]");
			_writer.Close();
			Assert.AreEqual("<settings normalization=\"on\" />", _xmlText.ToString());
		}

		[Test]
		public void NormalizationOff_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[normalization off]");
			_writer.Close();
			Assert.AreEqual("<settings normalization=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void CaseLevelOn_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[caseLevel on]");
			_writer.Close();
			Assert.AreEqual("<settings caseLevel=\"on\" />", _xmlText.ToString());
		}

		[Test]
		public void CaseLevelOff_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[caseLevel off]");
			_writer.Close();
			Assert.AreEqual("<settings caseLevel=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void CaseFirstOff_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[caseFirst off]");
			_writer.Close();
			Assert.AreEqual("<settings caseFirst=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void CaseFirstLower_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[caseFirst lower]");
			_writer.Close();
			Assert.AreEqual("<settings caseFirst=\"lower\" />", _xmlText.ToString());
		}

		[Test]
		public void CaseFirstUpper_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[caseFirst upper]");
			_writer.Close();
			Assert.AreEqual("<settings caseFirst=\"upper\" />", _xmlText.ToString());
		}

		[Test]
		public void HiraganaQOff_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[hiraganaQ off]");
			_writer.Close();
			Assert.AreEqual("<settings hiraganaQuaternary=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void HiraganaQOn_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[hiraganaQ on]");
			_writer.Close();
			Assert.AreEqual("<settings hiraganaQuaternary=\"on\" />", _xmlText.ToString());
		}

		[Test]
		public void NumericOff_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[numeric off]");
			_writer.Close();
			Assert.AreEqual("<settings numeric=\"off\" />", _xmlText.ToString());
		}

		[Test]
		public void NumericOn_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "[numeric on]");
			_writer.Close();
			Assert.AreEqual("<settings numeric=\"on\" />", _xmlText.ToString());
		}

		[Test]
		public void VariableTop_ProducesCorrectSettingsNode()
		{
			_icuParser.WriteIcuRules(_writer, "& A < [variable top]");
			_writer.Close();
			Assert.AreEqual("<settings variableTop=\"u41\" /><rules><reset>A</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_u_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\u0041");
			_writer.Close();
			Assert.AreEqual("<rules><reset>A</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_U_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\U00000041");
			_writer.Close();
			Assert.AreEqual("<rules><reset>A</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_x_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\x41");
			_writer.Close();
			Assert.AreEqual("<rules><reset>A</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_x_brace_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\x{041}");
			_writer.Close();
			Assert.AreEqual("<rules><reset>A</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_octal_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\102");
			_writer.Close();
			Assert.AreEqual("<rules><reset>B</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_c_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\c\u0083");
			_writer.Close();
			Assert.AreEqual("<rules><reset><cp hex=\"3\" /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_a_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\a");
			_writer.Close();
			Assert.AreEqual("<rules><reset><cp hex=\"7\" /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_b_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\b");
			_writer.Close();
			Assert.AreEqual("<rules><reset><cp hex=\"8\" /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_t_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\t");
			_writer.Close();
			Assert.AreEqual("<rules><reset>\t</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_n_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\n");
			_writer.Close();
			Assert.AreEqual(String.Format("<rules><reset>{0}</reset></rules>", _writer.Settings.NewLineChars), _xmlText.ToString());
		}

		[Test]
		public void Escape_v_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\v");
			_writer.Close();
			Assert.AreEqual("<rules><reset><cp hex=\"B\" /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_f_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\f");
			_writer.Close();
			Assert.AreEqual("<rules><reset><cp hex=\"C\" /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_r_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\r");
			_writer.Close();
			Assert.AreEqual("<rules><reset><cp hex=\"D\" /></reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void Escape_OtherChar_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&\\\\");
			_writer.Close();
			Assert.AreEqual("<rules><reset>\\</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void TwoSingleQuotes_ProducesCorrectCharacter()
		{
			_icuParser.WriteIcuRules(_writer, "&''");
			_writer.Close();
			Assert.AreEqual("<rules><reset>'</reset></rules>", _xmlText.ToString());
		}

		[Test]
		public void QuotedString_ProducesCorrectString()
		{
			_icuParser.WriteIcuRules(_writer, "&'''\\<&'");
			_writer.Close();
			Assert.AreEqual("<rules><reset>'\\&lt;&amp;</reset></rules>", _xmlText.ToString());
		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void InvalidIcu_Throws()
		{
			_icuParser.WriteIcuRules(_writer, "&a <<<< b");
			_writer.Close();
		}

		[Test]
		public void InvalidIcu_NoXmlProduced()
		{
			try
			{
				_icuParser.WriteIcuRules(_writer, "&a <<<< b");
			}
			catch {}
			_writer.Close();
			Assert.IsTrue(String.IsNullOrEmpty(_xmlText.ToString()));
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
			string icu = "[strength 3] [alternate shifted]\n[backwards 2]&[before 1][ first  regular]<b<\\u0041"
				+ "<'cde'&gh<<p<K|Q/\\<<[last variable]<<4<[variable\ttop]\t<9";
			_icuParser.WriteIcuRules(_writer, icu);
			_writer.Close();
			string xml = "<settings alternate=\"shifted\" backwards=\"on\" variableTop=\"u34\" strength=\"tertiary\" />"
				+ "<rules><reset before=\"primary\"><first_non_ignorable /></reset>"
				+ "<pc>bA</pc><p>cde</p><reset>gh</reset><s>p</s>"
				+ "<x><context>K</context><p>Q</p><extend>&lt;</extend></x>"
				+ "<p><last_variable /></p><s>4</s><p>9</p></rules>";
			Assert.AreEqual(xml, _xmlText.ToString());
		}
	}
}
