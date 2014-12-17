using System;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.WritingSystems.Collation;

namespace SIL.WritingSystems.Tests.Collation
{
	[TestFixture]
	public class LdmlCollationParserTests
	{
		[Test]
		public void NoRules_ProducesEmptyString()
		{
			XElement collationXml = XElement.Parse("<collation></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual(string.Empty, icu);
		}

		[Test]
		public void Reset_ProducesResetIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a", icu);
		}

		[Test]
		public void PrimaryDifference_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><p>b</p></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a < b", icu);
		}

		[Test]
		public void SecondaryDifference_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><s>b</s></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a << b", icu);
		}

		[Test]
		public void TertiaryDifference_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><t>b</t></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a <<< b", icu);
		}

		[Test]
		public void NoDifference_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><i>b</i></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a = b", icu);
		}

		[Test]
		public void Prefix_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><x><context>b</context><p>c</p></x></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a < b | c", icu);
		}

		[Test]
		public void Expansion_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><x><p>b</p><extend>c</extend></x></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a < b / c", icu);
		}

		[Test]
		public void PrefixAndExpansion_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><x><context>b</context><p>c</p><extend>d</extend></x></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a < b | c / d", icu);
		}

		[Test]
		public void Contraction_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>ab</reset><p>cd</p></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& ab < cd", icu);
		}

		[Test]
		public void MultiplePrimaryDifferences_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><pc>bc</pc></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a < b < c", icu);
		}

		[Test]
		public void MultipleSecondaryDifferences_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><sc>bc</sc></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a << b << c", icu);
		}

		[Test]
		public void MultipleTertiaryDifferences_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><tc>bc</tc></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a <<< b <<< c", icu);
		}

		[Test]
		public void MultipleNoDifferences_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><ic>bc</ic></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a = b = c", icu);
		}

		[Test]
		public void FirstTertiaryIgnorable_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><first_tertiary_ignorable /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [first tertiary ignorable]", icu);
		}

		[Test]
		public void LastTertiaryIgnorable_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><last_tertiary_ignorable /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [last tertiary ignorable]", icu);
		}

		[Test]
		public void TertiaryIgnorableSpace_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><last_tertiary_ignorable/></reset><ic> ?-</ic></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [last tertiary ignorable] = ' ' = '?' = '-'", icu);
		}

		[Test]
		public void TertiaryIgnorableSpaceAsLast_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><last_tertiary_ignorable/></reset><ic>?- </ic></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [last tertiary ignorable] = '?' = '-' = ' '", icu);
		}

		[Test]
		public void FirstSecondarygnorable_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><first_secondary_ignorable /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [first secondary ignorable]", icu);
		}

		[Test]
		public void LastSecondaryIgnorable_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><last_secondary_ignorable /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [last secondary ignorable]", icu);
		}

		[Test]
		public void FirstPrimaryIgnorable_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><first_primary_ignorable /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [first primary ignorable]", icu);
		}

		[Test]
		public void LastPrimaryIgnorable_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><last_primary_ignorable /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [last primary ignorable]", icu);
		}

		[Test]
		public void FirstVariable_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><first_variable /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [first variable]", icu);
		}

		[Test]
		public void LastVariable_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><last_variable /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [last variable]", icu);
		}

		[Test]
		public void FirstRegular_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><first_non_ignorable /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [first regular]", icu);
		}

		[Test]
		public void LastRegular_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><last_non_ignorable /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [last regular]", icu);
		}

		[Test]
		public void FirstTrailing_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><first_trailing /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [first trailing]", icu);
		}

		[Test]
		public void LastTrailing_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><last_trailing /></reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [last trailing]", icu);
		}

		[Test]
		public void FirstImplicit_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><p><first_implicit /></p></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a < [first implicit]", icu);
		}

		[Test]
		public void LastImplicit_ProducesCorrectIndirectNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><p><last_implicit /></p></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a < [last implicit]", icu);
		}

		[Test]
		public void Before1_ProducesCorrectResetNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"primary\">a</reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [before 1] a", icu);
		}

		[Test]
		public void Before2_ProducesCorrectResetNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"secondary\">a</reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [before 2] a", icu);
		}

		[Test]
		public void Before3_ProducesCorrectResetNode()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"tertiary\">a</reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& [before 3] a", icu);
		}

		[Test]
		public void TwoRules_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>a</reset><p>b</p><reset>c</reset><s>d</s></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a < b\r\n& c << d", icu);
		}

		[Test]
		public void AlternateShiftedOption_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings alternate=\"shifted\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[alternate shifted]", icu);
		}

		[Test]
		public void AlternateNonIgnorableOption_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings alternate=\"non-ignorable\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[alternate non-ignorable]", icu);
		}

		[Test]
		public void Strength1Option_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings strength=\"primary\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[strength 1]", icu);
		}

		[Test]
		public void Strength2Option_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings strength=\"secondary\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[strength 2]", icu);
		}

		[Test]
		public void Strength3Option_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings strength=\"tertiary\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[strength 3]", icu);
		}

		[Test]
		public void Strength4Option_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings strength=\"quaternary\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[strength 4]", icu);
		}

		[Test]
		public void StrengthIOption_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings strength=\"identical\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[strength I]", icu);
		}

		[Test]
		public void Backwards1_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings backwards=\"off\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[backwards 1]", icu);
		}

		[Test]
		public void Backwards2_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings backwards=\"on\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[backwards 2]", icu);
		}

		[Test]
		public void NormalizationOn_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings normalization=\"on\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[normalization on]", icu);
		}

		[Test]
		public void NormalizationOff_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings normalization=\"off\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[normalization off]", icu);
		}

		[Test]
		public void CaseLevelOn_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings caseLevel=\"on\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[caseLevel on]", icu);
		}

		[Test]
		public void CaseLevelOff_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings caseLevel=\"off\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[caseLevel off]", icu);
		}

		[Test]
		public void CaseFirstOff_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings caseFirst=\"off\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[caseFirst off]", icu);
		}

		[Test]
		public void CaseFirstLower_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings caseFirst=\"lower\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[caseFirst lower]", icu);
		}

		[Test]
		public void CaseFirstUpper_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings caseFirst=\"upper\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[caseFirst upper]", icu);
		}

		[Test]
		public void HiraganaQOff_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings hiraganaQuaternary=\"off\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[hiraganaQ off]", icu);
		}

		[Test]
		public void HiraganaQOn_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings hiraganaQuaternary=\"on\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[hiraganaQ on]", icu);
		}

		[Test]
		public void NumericOff_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings numeric=\"off\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[numeric off]", icu);
		}

		[Test]
		public void NumericOn_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings numeric=\"on\" /><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[numeric on]", icu);
		}

		[Test]
		public void VariableTop_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings variableTop=\"u41\" /><rules><reset>A</reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& A < [variable top]", icu);
		}

		[Test]
		public void SuppressContractions_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><suppress_contractions>[abc]</suppress_contractions><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[suppress contractions [abc]]", icu);
		}

		[Test]
		public void Optimize_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><optimize>[abc]</optimize><rules /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[optimize [abc]]", icu);
		}

		[Test]
		public void IcuEscapableCharacter_ProducesCorrectEscapeSequence()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>(</reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& '('", icu);
		}

		[Test]
		public void IcuEscapedCharacter_ProducesCorrectEscapeSequence()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>\\(</reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& \\(", icu);
		}

		[Test]
		public void IcuUnicodeEscapes_ProducesCorrectSequence()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>\\u0062</reset><p>\\U00000061</p></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& \\u0062 < \\U00000061", icu);
		}

		[Test]
		public void IcuEscapableSequence_ProducesCorrectSequence()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>k .w</reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& k' .'w", icu);
		}

		[Test]
		public void IcuSingleQuote_ProducesCorrectSequence()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset>k'w'</reset></rules></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& k''w''", icu);
		}

		[Test]
		public void IcuComplexIcRuleWithMultipleEscapableChars_ProducesCorrectSequence()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset><last_tertiary_ignorable/></reset><ic>-()ʼ</ic></rules></collation>");
			Assert.AreEqual("& [last tertiary ignorable] = '-' = '(' = ')' = ʼ", LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml));
		}

		[Test]
		public void InvalidLdml_Throws()
		{
			XElement collationXml =XElement.Parse("<collation><rules><m>a</m></rules></collation>");
			Assert.Throws<ApplicationException>(
				() => LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml));
		}

		[Test]
		public void BigCombinedRule_ParsesCorrectlyIntoIcu()
		{
			// certainly some of this actually doesn't form semantically vaild ICU, but it should be syntactically correct
			string icuExpected = "[strength 3]\r\n[alternate shifted]\r\n[backwards 2]\r\n& [before 1] [first regular] < b < A < cde\r\n"
				+ "& gh << p < K | Q / '<' < [last variable] << 4 < [variable top] < 9";
			string xml = "<collation><settings strength=\"tertiary\" alternate=\"shifted\" backwards=\"on\" variableTop=\"u34\" />"
				+ "<rules><reset before=\"primary\"><first_non_ignorable /></reset>"
				+ "<pc>bA</pc><p>cde</p><reset>gh</reset><s>p</s>"
				+ "<x><context>K</context><p>Q</p><extend>&lt;</extend></x>"
				+ "<p><last_variable /></p><s>4</s><p>9</p></rules></collation>";
			XElement collationXml =XElement.Parse(xml);
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual(icuExpected, icu);
		}

		[Test]
		public void Reset_ProducesEmptySimpleRules()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"primary\"><first_non_ignorable /></reset></rules></collation>");
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out simple));
			Assert.AreEqual(string.Empty, simple);
		}

		[Test]
		public void WhiteSpace_IsIgnored()
		{
			XElement collationXml =XElement.Parse("<collation><rules>\r\n<reset before=\"primary\">\r\n<first_non_ignorable />\r\n</reset>\r\n<p>a</p>\r\n<sc>bcd</sc>\r\n</rules></collation>");
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out simple));
			Assert.AreEqual("a b c d", simple);
		}

		[Test]
		public void PrimaryDifference_ProducesCorrectSimpleRules()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><p>b</p></rules></collation>");
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out simple));
			Assert.AreEqual("a\r\nb", simple);
		}

		[Test]
		public void SecondaryDifference_ProducesCorrectSimpleRules()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><s>b</s></rules></collation>");
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out simple));
			Assert.AreEqual("a b", simple);
		}

		[Test]
		public void TertiaryDifference_ProducesCorrectSimpleRules()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><t>b</t></rules></collation>");
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out simple));
			Assert.AreEqual("(a b)", simple);
		}

		[Test]
		public void ConcatenatedPrimaryDifferences_ProduceCorrectSimpleRules()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"primary\"><first_non_ignorable /></reset><pc>abc</pc></rules></collation>");
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out simple));
			Assert.AreEqual("a\r\nb\r\nc", simple);
		}

		[Test]
		public void ConcatenatedSecondaryDifferences_ProduceCorrectSimpleRules()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><sc>bcd</sc></rules></collation>");
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out simple));
			Assert.AreEqual("a b c d", simple);
		}

		[Test]
		public void ConcatenatedTertiaryDifferences_ProduceCorrectSimpleRules()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><tc>bcd</tc></rules></collation>");
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out simple));
			Assert.AreEqual("(a b c d)", simple);
		}

		[Test]
		public void LotsOLdmlCollation_ProducesCorrectSimpleRules()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"primary\"><first_non_ignorable /></reset><p>a</p><sc>bc</sc>"
				+ "<s>d</s><pc>efg</pc><t>h</t><tc>ijk</tc><p>l</p></rules></collation>");
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out simple));
			Assert.AreEqual("a b c d\r\ne\r\nf\r\n(g h i j k)\r\nl", simple);
		}

		[Test]
		public void LdmlTooComplexForSimpleRules_ReturnsFalse()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"primary\"><first_non_ignorable /></reset><p>A</p>"
									  + "<x><t>B</t><extend>C</extend></x></rules></collation>");
			string simple;
			Assert.IsFalse(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out simple));
		}

		[Test]
		public void CharacterEscapedForIcu_NotEscapedInSimpleRules()
		{
			XElement collationXml =XElement.Parse("<collation><rules><reset before=\"primary\"><first_non_ignorable /></reset><p>=</p><p>b</p></rules></collation>");
			string simple;
			Assert.IsTrue(LdmlCollationParser.TryGetSimpleRulesFromCollationNode(collationXml, out simple));
			Assert.AreEqual("=\r\nb", simple);
		}
	}
}
