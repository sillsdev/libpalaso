using System;
using System.Xml.Linq;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
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
		public void Rules_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><cr><![CDATA[& a << b << c]]></cr></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& a << b << c", icu);
		}

		[Test]
		public void AlternateShiftedOption_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings alternate=\"shifted\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[alternate shifted]", icu);
		}

		[Test]
		public void AlternateNonIgnorableOption_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings alternate=\"non-ignorable\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[alternate non-ignorable]", icu);
		}

		[Test]
		public void Strength1Option_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings strength=\"primary\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[strength 1]", icu);
		}

		[Test]
		public void Strength2Option_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings strength=\"secondary\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[strength 2]", icu);
		}

		[Test]
		public void Strength3Option_ProducesCorrectIcu()
		{
			XElement collationXml =XElement.Parse("<collation><settings strength=\"tertiary\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[strength 3]", icu);
		}

		[Test]
		public void Strength4Option_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings strength=\"quaternary\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[strength 4]", icu);
		}

		[Test]
		public void StrengthIOption_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings strength=\"identical\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[strength I]", icu);
		}

		[Test]
		public void Backwards1_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings backwards=\"off\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[backwards 1]", icu);
		}

		[Test]
		public void Backwards2_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings backwards=\"on\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[backwards 2]", icu);
		}

		[Test]
		public void NormalizationOn_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings normalization=\"on\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[normalization on]", icu);
		}

		[Test]
		public void NormalizationOff_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings normalization=\"off\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[normalization off]", icu);
		}

		[Test]
		public void CaseLevelOn_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings caseLevel=\"on\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[caseLevel on]", icu);
		}

		[Test]
		public void CaseLevelOff_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings caseLevel=\"off\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[caseLevel off]", icu);
		}

		[Test]
		public void CaseFirstOff_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings caseFirst=\"off\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[caseFirst off]", icu);
		}

		[Test]
		public void CaseFirstLower_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings caseFirst=\"lower\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[caseFirst lower]", icu);
		}

		[Test]
		public void CaseFirstUpper_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings caseFirst=\"upper\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[caseFirst upper]", icu);
		}

		[Test]
		public void HiraganaQOff_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings hiraganaQuaternary=\"off\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[hiraganaQ off]", icu);
		}

		[Test]
		public void HiraganaQOn_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings hiraganaQuaternary=\"on\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[hiraganaQ on]", icu);
		}

		[Test]
		public void NumericOff_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings numeric=\"off\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[numeric off]", icu);
		}

		[Test]
		public void NumericOn_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings numeric=\"on\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[numeric on]", icu);
		}

		[Test]
		public void VariableTop_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><settings variableTop=\"u41\" /><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("& A < [variable top]", icu);
		}

		[Test]
		public void SuppressContractions_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><suppress_contractions>[abc]</suppress_contractions><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[suppress contractions [abc]]", icu);
		}

		[Test]
		public void Optimize_ProducesCorrectIcu()
		{
			XElement collationXml = XElement.Parse("<collation><optimize>[abc]</optimize><cr /></collation>");
			string icu = LdmlCollationParser.GetIcuRulesFromCollationNode(collationXml);
			Assert.AreEqual("[optimize [abc]]", icu);
		}
	}
}
