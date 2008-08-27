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
		private XmlDocument _dom;
		private XmlNode _collationNode;
		private XmlNamespaceManager _nameSpaceManager;

		[SetUp]
		public void SetUp()
		{
			_icuParser = new IcuRulesParser();
			_dom = new XmlDocument();
			_nameSpaceManager = new XmlNamespaceManager(_dom.NameTable);
			_collationNode = _dom.CreateElement("collation");
			_dom.AppendChild(_collationNode);
		}

		[Test]
		public void EmptyString_ProducesNoRules()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, string.Empty, _nameSpaceManager);
			Assert.AreEqual("<rules />", _collationNode.InnerXml);
		}

		[Test]
		public void WhiteSpace_ProducesNoRules()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "   \n\t", _nameSpaceManager);
			Assert.AreEqual("<rules />", _collationNode.InnerXml);
		}

		[Test]
		public void Reset_ProducesResetNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void PrimaryDifference_ProducesCorrectXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a < b", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><p>b</p></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void SecondaryDifference_ProducesCorrectXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a << b", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><s>b</s></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void TertiaryDifference_ProducesCorrectXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a <<< b", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><t>b</t></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void NoDifference_ProducesCorrectXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a = b", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><i>b</i></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Prefix_ProducesCorrectXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a < b|c", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><x><context>b</context><p>c</p></x></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Expansion_ProducesCorrectXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a < b/ c", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><x><p>b</p><extend>c</extend></x></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void PrefixAndExpansion_ProducesCorrectXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a < b|c/d", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><x><context>b</context><p>c</p><extend>d</extend></x></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Contraction_ProducesCorrectXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&ab < cd", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>ab</reset><p>cd</p></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void MultiplePrimaryDifferences_ProducesOptimizedXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a < b<c", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><pc>bc</pc></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void MultipleSecondaryDifferences_ProducesOptimizedXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a << b<<c", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><sc>bc</sc></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void MultipleTertiaryDifferences_ProducesOptimizedXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a <<< b<<<c", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><tc>bc</tc></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void MultipleNoDifferences_ProducesOptimizedXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a = b=c", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><ic>bc</ic></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void FirstTertiaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[first tertiary ignorable]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><first_tertiary_ignorable /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void LastTertiaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[last tertiary ignorable]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><last_tertiary_ignorable /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void FirstSecondarygnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[first secondary ignorable]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><first_secondary_ignorable /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void LastSecondaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[last secondary ignorable]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><last_secondary_ignorable /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void FirstPrimaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[first primary ignorable]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><first_primary_ignorable /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void LastPrimaryIgnorable_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[last primary ignorable]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><last_primary_ignorable /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void FirstVariable_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[first variable]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><first_variable /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void LastVariable_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[last variable]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><last_variable /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void FirstRegular_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[first regular]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><first_non_ignorable /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void LastRegular_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[last regular]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><last_non_ignorable /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void FirstTrailing_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[first trailing]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><first_trailing /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void LastTrailing_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[last trailing]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset><last_trailing /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void FirstImplicit_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a < [first implicit]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><p><first_implicit /></p></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void LastImplicit_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a < [last implicit]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><p><last_implicit /></p></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Top_ProducesCorrectIndirectNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[top]", _nameSpaceManager);
			Assert.AreEqual("<rules><reset before=\"primary\"><first_tertiary_ignorable /></reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Before1_ProducesCorrectResetNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[before 1]a", _nameSpaceManager);
			Assert.AreEqual("<rules><reset before=\"primary\">a</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Before2_ProducesCorrectResetNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[before 2]a", _nameSpaceManager);
			Assert.AreEqual("<rules><reset before=\"secondary\">a</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Before3_ProducesCorrectResetNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&[before 3]a", _nameSpaceManager);
			Assert.AreEqual("<rules><reset before=\"tertiary\">a</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void TwoRules_ProducesCorrectXml()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a<b \n&c<<d", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>a</reset><p>b</p><reset>c</reset><s>d</s></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void AlternateShiftedOption_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[alternate shifted]", _nameSpaceManager);
			Assert.AreEqual("<settings alternate=\"shifted\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void AlternateNonIgnorableOption_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[alternate non-ignorable]", _nameSpaceManager);
			Assert.AreEqual("<settings alternate=\"non-ignorable\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void Strength1Option_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[strength 1]", _nameSpaceManager);
			Assert.AreEqual("<settings strength=\"primary\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void Strength2Option_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[strength 2]", _nameSpaceManager);
			Assert.AreEqual("<settings strength=\"secondary\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void Strength3Option_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[strength 3]", _nameSpaceManager);
			Assert.AreEqual("<settings strength=\"tertiary\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void Strength4Option_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[strength 4]", _nameSpaceManager);
			Assert.AreEqual("<settings strength=\"quaternary\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void Strength5Option_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[strength 5]", _nameSpaceManager);
			Assert.AreEqual("<settings strength=\"identical\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void StrengthIOption_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[strength I]", _nameSpaceManager);
			Assert.AreEqual("<settings strength=\"identical\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void Backwards1_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[backwards 1]", _nameSpaceManager);
			Assert.AreEqual("<settings backwards=\"off\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void Backwards2_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[backwards 2]", _nameSpaceManager);
			Assert.AreEqual("<settings backwards=\"on\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void NormalizationOn_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[normalization on]", _nameSpaceManager);
			Assert.AreEqual("<settings normalization=\"on\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void NormalizationOff_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[normalization off]", _nameSpaceManager);
			Assert.AreEqual("<settings normalization=\"off\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void CaseLevelOn_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[caseLevel on]", _nameSpaceManager);
			Assert.AreEqual("<settings caseLevel=\"on\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void CaseLevelOff_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[caseLevel off]", _nameSpaceManager);
			Assert.AreEqual("<settings caseLevel=\"off\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void CaseFirstOff_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[caseFirst off]", _nameSpaceManager);
			Assert.AreEqual("<settings caseFirst=\"off\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void CaseFirstLower_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[caseFirst lower]", _nameSpaceManager);
			Assert.AreEqual("<settings caseFirst=\"lower\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void CaseFirstUpper_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[caseFirst upper]", _nameSpaceManager);
			Assert.AreEqual("<settings caseFirst=\"upper\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void HiraganaQOff_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[hiraganaQ off]", _nameSpaceManager);
			Assert.AreEqual("<settings hiraganaQuaternary=\"off\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void HiraganaQOn_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[hiraganaQ on]", _nameSpaceManager);
			Assert.AreEqual("<settings hiraganaQuaternary=\"on\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void NumericOff_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[numeric off]", _nameSpaceManager);
			Assert.AreEqual("<settings numeric=\"off\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void NumericOn_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "[numeric on]", _nameSpaceManager);
			Assert.AreEqual("<settings numeric=\"on\" /><rules />", _collationNode.InnerXml);
		}

		[Test]
		public void VariableTop_ProducesCorrectSettingsNode()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "& A < [variable top]", _nameSpaceManager);
			Assert.AreEqual("<settings variableTop=\"u41\" /><rules><reset>A</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_u_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\u0041", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>A</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_U_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\U00000041", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>A</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_x_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\x41", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>A</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_x_brace_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\x{041}", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>A</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_octal_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\102", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>B</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_c_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\c\u0083", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>&#x3;</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_a_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\a", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>&#x7;</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_b_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\b", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>&#x8;</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_t_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\t", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>\t</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_n_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\n", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>\n</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_v_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\v", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>&#xB;</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_f_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\f", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>&#xC;</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_r_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\r", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>\r</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void Escape_OtherChar_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&\\\\", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>\\</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void TwoSingleQuotes_ProducesCorrectCharacter()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&''", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>'</reset></rules>", _collationNode.InnerXml);
		}

		[Test]
		public void QuotedString_ProducesCorrectString()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&'''\\<&'", _nameSpaceManager);
			Assert.AreEqual("<rules><reset>'\\&lt;&amp;</reset></rules>", _collationNode.InnerXml);
		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void InvalidIcu_Throws()
		{
			_icuParser.AddIcuRulesToNode(_collationNode, "&a <<<< b", _nameSpaceManager);
		}

		[Test]
		public void InvalidIcu_NoXmlProduced()
		{
			try
			{
				_icuParser.AddIcuRulesToNode(_collationNode, "&a <<<< b", _nameSpaceManager);
			}
			catch {}
			Assert.IsFalse(_collationNode.HasChildNodes);
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
			_icuParser.AddIcuRulesToNode(_collationNode, icu, _nameSpaceManager);
			string xml = "<settings alternate=\"shifted\" backwards=\"on\" variableTop=\"u34\" strength=\"tertiary\" />"
				+ "<rules><reset before=\"primary\"><first_non_ignorable /></reset>"
				+ "<pc>bA</pc><p>cde</p><reset>gh</reset><s>p</s>"
				+ "<x><context>K</context><p>Q</p><extend>&lt;</extend></x>"
				+ "<p><last_variable /></p><s>4</s><p>9</p></rules>";
			Assert.AreEqual(xml, _collationNode.InnerXml);
		}
	}
}
