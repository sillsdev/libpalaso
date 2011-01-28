using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems.Collation;
using System.Xml;

namespace Palaso.Tests.WritingSystems.Collation
{
	[TestFixture]
	public class LdmlCollationParserTests
	{
		private string _collationXml;

		[SetUp]
		public void SetUp()
		{
			_collationXml = string.Empty;
		}

		[Test]
		public void NoRules_ProducesEmptyString()
		{
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual(string.Empty, icu);
		}

		[Test]
		public void Reset_ProducesResetIcu()
		{
			_collationXml = "<rules><reset>a</reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a", icu);
		}

		[Test]
		public void PrimaryDifference_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><p>b</p></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a < b", icu);
		}

		[Test]
		public void SecondaryDifference_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><s>b</s></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a << b", icu);
		}

		[Test]
		public void TertiaryDifference_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><t>b</t></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a <<< b", icu);
		}

		[Test]
		public void NoDifference_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><i>b</i></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a = b", icu);
		}

		[Test]
		public void Prefix_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><x><context>b</context><p>c</p></x></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a < b | c", icu);
		}

		[Test]
		public void Expansion_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><x><p>b</p><extend>c</extend></x></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a < b / c", icu);
		}

		[Test]
		public void PrefixAndExpansion_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><x><context>b</context><p>c</p><extend>d</extend></x></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a < b | c / d", icu);
		}

		[Test]
		public void Contraction_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>ab</reset><p>cd</p></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& ab < cd", icu);
		}

		[Test]
		public void MultiplePrimaryDifferences_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><pc>bc</pc></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a < b < c", icu);
		}

		[Test]
		public void MultipleSecondaryDifferences_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><sc>bc</sc></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a << b << c", icu);
		}

		[Test]
		public void MultipleTertiaryDifferences_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><tc>bc</tc></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a <<< b <<< c", icu);
		}

		[Test]
		public void MultipleNoDifferences_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><ic>bc</ic></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a = b = c", icu);
		}

		[Test]
		public void FirstTertiaryIgnorable_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><first_tertiary_ignorable /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [first tertiary ignorable]", icu);
		}

		[Test]
		public void LastTertiaryIgnorable_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><last_tertiary_ignorable /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [last tertiary ignorable]", icu);
		}

		[Test]
		public void FirstSecondarygnorable_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><first_secondary_ignorable /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [first secondary ignorable]", icu);
		}

		[Test]
		public void LastSecondaryIgnorable_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><last_secondary_ignorable /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [last secondary ignorable]", icu);
		}

		[Test]
		public void FirstPrimaryIgnorable_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><first_primary_ignorable /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [first primary ignorable]", icu);
		}

		[Test]
		public void LastPrimaryIgnorable_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><last_primary_ignorable /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [last primary ignorable]", icu);
		}

		[Test]
		public void FirstVariable_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><first_variable /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [first variable]", icu);
		}

		[Test]
		public void LastVariable_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><last_variable /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [last variable]", icu);
		}

		[Test]
		public void FirstRegular_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><first_non_ignorable /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [first regular]", icu);
		}

		[Test]
		public void LastRegular_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><last_non_ignorable /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [last regular]", icu);
		}

		[Test]
		public void FirstTrailing_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><first_trailing /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [first trailing]", icu);
		}

		[Test]
		public void LastTrailing_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset><last_trailing /></reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [last trailing]", icu);
		}

		[Test]
		public void FirstImplicit_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset>a</reset><p><first_implicit /></p></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a < [first implicit]", icu);
		}

		[Test]
		public void LastImplicit_ProducesCorrectIndirectNode()
		{
			_collationXml = "<rules><reset>a</reset><p><last_implicit /></p></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a < [last implicit]", icu);
		}

		[Test]
		public void Before1_ProducesCorrectResetNode()
		{
			_collationXml = "<rules><reset before=\"primary\">a</reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [before 1] a", icu);
		}

		[Test]
		public void Before2_ProducesCorrectResetNode()
		{
			_collationXml = "<rules><reset before=\"secondary\">a</reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [before 2] a", icu);
		}

		[Test]
		public void Before3_ProducesCorrectResetNode()
		{
			_collationXml = "<rules><reset before=\"tertiary\">a</reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& [before 3] a", icu);
		}

		[Test]
		public void TwoRules_ProducesCorrectIcu()
		{
			_collationXml = "<rules><reset>a</reset><p>b</p><reset>c</reset><s>d</s></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& a < b\r\n& c << d", icu);
		}

		[Test]
		public void AlternateShiftedOption_ProducesCorrectIcu()
		{
			_collationXml = "<settings alternate=\"shifted\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[alternate shifted]", icu);
		}

		[Test]
		public void AlternateNonIgnorableOption_ProducesCorrectIcu()
		{
			_collationXml = "<settings alternate=\"non-ignorable\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[alternate non-ignorable]", icu);
		}

		[Test]
		public void Strength1Option_ProducesCorrectIcu()
		{
			_collationXml = "<settings strength=\"primary\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[strength 1]", icu);
		}

		[Test]
		public void Strength2Option_ProducesCorrectIcu()
		{
			_collationXml = "<settings strength=\"secondary\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[strength 2]", icu);
		}

		[Test]
		public void Strength3Option_ProducesCorrectIcu()
		{
			_collationXml = "<settings strength=\"tertiary\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[strength 3]", icu);
		}

		[Test]
		public void Strength4Option_ProducesCorrectIcu()
		{
			_collationXml = "<settings strength=\"quaternary\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[strength 4]", icu);
		}

		[Test]
		public void StrengthIOption_ProducesCorrectIcu()
		{
			_collationXml = "<settings strength=\"identical\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[strength I]", icu);
		}

		[Test]
		public void Backwards1_ProducesCorrectIcu()
		{
			_collationXml = "<settings backwards=\"off\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[backwards 1]", icu);
		}

		[Test]
		public void Backwards2_ProducesCorrectIcu()
		{
			_collationXml = "<settings backwards=\"on\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[backwards 2]", icu);
		}

		[Test]
		public void NormalizationOn_ProducesCorrectIcu()
		{
			_collationXml = "<settings normalization=\"on\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[normalization on]", icu);
		}

		[Test]
		public void NormalizationOff_ProducesCorrectIcu()
		{
			_collationXml = "<settings normalization=\"off\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[normalization off]", icu);
		}

		[Test]
		public void CaseLevelOn_ProducesCorrectIcu()
		{
			_collationXml = "<settings caseLevel=\"on\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[caseLevel on]", icu);
		}

		[Test]
		public void CaseLevelOff_ProducesCorrectIcu()
		{
			_collationXml = "<settings caseLevel=\"off\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[caseLevel off]", icu);
		}

		[Test]
		public void CaseFirstOff_ProducesCorrectIcu()
		{
			_collationXml = "<settings caseFirst=\"off\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[caseFirst off]", icu);
		}

		[Test]
		public void CaseFirstLower_ProducesCorrectIcu()
		{
			_collationXml = "<settings caseFirst=\"lower\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[caseFirst lower]", icu);
		}

		[Test]
		public void CaseFirstUpper_ProducesCorrectIcu()
		{
			_collationXml = "<settings caseFirst=\"upper\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[caseFirst upper]", icu);
		}

		[Test]
		public void HiraganaQOff_ProducesCorrectIcu()
		{
			_collationXml = "<settings hiraganaQuaternary=\"off\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[hiraganaQ off]", icu);
		}

		[Test]
		public void HiraganaQOn_ProducesCorrectIcu()
		{
			_collationXml = "<settings hiraganaQuaternary=\"on\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[hiraganaQ on]", icu);
		}

		[Test]
		public void NumericOff_ProducesCorrectIcu()
		{
			_collationXml = "<settings numeric=\"off\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[numeric off]", icu);
		}

		[Test]
		public void NumericOn_ProducesCorrectIcu()
		{
			_collationXml = "<settings numeric=\"on\" /><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[numeric on]", icu);
		}

		[Test]
		public void VariableTop_ProducesCorrectIcu()
		{
			_collationXml = "<settings variableTop=\"u41\" /><rules><reset>A</reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& A < [variable top]", icu);
		}

		[Test]
		public void SuppressContractions_ProducesCorrectIcu()
		{
			_collationXml = "<suppress_contractions>[abc]</suppress_contractions><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[suppress contractions [abc]]", icu);
		}

		[Test]
		public void Optimize_ProducesCorrectIcu()
		{
			_collationXml = "<optimize>[abc]</optimize><rules />";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("[optimize [abc]]", icu);
		}

		[Test]
		public void IcuEscapableCharacter_ProducesCorrectEscapeSequence()
		{
			_collationXml = "<rules><reset>(</reset></rules>";
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual("& \\(", icu);
		}

		[Test]
		public void InvalidLdml_Throws()
		{
			_collationXml = "<rules><m>a</m></rules>";
			Assert.Throws<ApplicationException>(
				() => LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml));
		}

		[Test]
		public void BigCombinedRule_ParsesCorrectlyIntoIcu()
		{
			// certainly some of this actually doesn't form semantically vaild ICU, but it should be syntactically correct
			string icuExpected = "[strength 3]\r\n[alternate shifted]\r\n[backwards 2]\r\n& [before 1] [first regular] < b < A < cde\r\n"
				+ "& gh << p < K | Q / \\< < [last variable] << 4 < [variable top] < 9";
			string xml = "<settings strength=\"tertiary\" alternate=\"shifted\" backwards=\"on\" variableTop=\"u34\" />"
				+ "<rules><reset before=\"primary\"><first_non_ignorable /></reset>"
				+ "<pc>bA</pc><p>cde</p><reset>gh</reset><s>p</s>"
				+ "<x><context>K</context><p>Q</p><extend>&lt;</extend></x>"
				+ "<p><last_variable /></p><s>4</s><p>9</p></rules>";
			_collationXml = xml;
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(_collationXml);
			Assert.AreEqual(icuExpected, icu);
		}

		[Test]
		public void Reset_ProducesEmptySimpleRules()
		{
			_collationXml = "<rules><reset before=\"primary\"><first_non_ignorable /></reset></rules>";
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(_collationXml, out simple));
			Assert.AreEqual(string.Empty, simple);
		}

		[Test]
		public void WhiteSpace_IsIgnored()
		{
			_collationXml = "<rules>\r\n<reset before=\"primary\">\r\n<first_non_ignorable />\r\n</reset>\r\n<p>a</p>\r\n<sc>bcd</sc>\r\n</rules>";
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(_collationXml, out simple));
			Assert.AreEqual("a b c d", simple);
		}

		[Test]
		public void PrimaryDifference_ProducesCorrectSimpleRules()
		{
			_collationXml = "<rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><p>b</p></rules>";
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(_collationXml, out simple));
			Assert.AreEqual("a\r\nb", simple);
		}

		[Test]
		public void SecondaryDifference_ProducesCorrectSimpleRules()
		{
			_collationXml = "<rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><s>b</s></rules>";
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(_collationXml, out simple));
			Assert.AreEqual("a b", simple);
		}

		[Test]
		public void TertiaryDifference_ProducesCorrectSimpleRules()
		{
			_collationXml = "<rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><t>b</t></rules>";
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(_collationXml, out simple));
			Assert.AreEqual("(a b)", simple);
		}

		[Test]
		public void ConcatenatedPrimaryDifferences_ProduceCorrectSimpleRules()
		{
			_collationXml = "<rules><reset before=\"primary\"><first_non_ignorable /></reset><pc>abc</pc></rules>";
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(_collationXml, out simple));
			Assert.AreEqual("a\r\nb\r\nc", simple);
		}

		[Test]
		public void ConcatenatedSecondaryDifferences_ProduceCorrectSimpleRules()
		{
			_collationXml = "<rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><sc>bcd</sc></rules>";
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(_collationXml, out simple));
			Assert.AreEqual("a b c d", simple);
		}

		[Test]
		public void ConcatenatedTertiaryDifferences_ProduceCorrectSimpleRules()
		{
			_collationXml = "<rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><tc>bcd</tc></rules>";
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(_collationXml, out simple));
			Assert.AreEqual("(a b c d)", simple);
		}

		[Test]
		public void LotsOLdmlCollation_ProducesCorrectSimpleRules()
		{
			_collationXml = "<rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><sc>bc</sc>"
				+ "<s>d</s><pc>efg</pc><t>h</t><tc>ijk</tc><p>l</p></rules>";
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(_collationXml, out simple));
			Assert.AreEqual("a b c d\r\ne\r\nf\r\n(g h i j k)\r\nl", simple);
		}

		[Test]
		public void LdmlTooComplexForSimpleRules_ReturnsFalse()
		{
			_collationXml = "<rules><reset before=\"primary\"><first_non_ignorable /></reset><p>A</p>"
									  + "<x><t>B</t><extend>C</extend></x></rules>";
			string simple;
			Assert.IsFalse(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(_collationXml, out simple));
		}

		[Test]
		public void CharacterEscapedForIcu_NotEscapedInSimpleRules()
		{
			_collationXml = "<rules><reset before=\"primary\"><first_non_ignorable /></reset><p>=</p><p>b</p></rules>";
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(_collationXml, out simple));
			Assert.AreEqual("=\r\nb", simple);
		}
	}
}
