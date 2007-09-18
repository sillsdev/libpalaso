using System;
using System.Collections.Generic;
using NUnit.Framework;
using Spart;

namespace Palaso.WritingSystems.Collation.Tests
{
	[TestFixture]
	public class SimpleRulesCollatorTests
	{

		static private void VerifyIcuRules(string icuRules)
		{
			new IcuRulesCollator(icuRules);
		}

		static private void VerifyParserError(string errorId, string rules)
		{
			try
			{
				SimpleRulesCollator.ConvertToIcuRules(rules);
			}
			catch (ParserErrorException e)
			{
				Assert.AreEqual(errorId, e.ParserError.ErrorId);
				throw;
			}
		}

		static private void VerifyParseIsSyntacticEquivalent(string simplestRule, string syntacticEquivalent)
		{
			string expected = SimpleRulesCollator.ConvertToIcuRules(simplestRule);
			string actual = SimpleRulesCollator.ConvertToIcuRules(syntacticEquivalent);
			Assert.AreEqual(expected, actual);
		}

		static private void VerifyExpectedIcuFromActualSimple(string icuExpected, string shoeboxActual)
		{
			string icuRulesActual = SimpleRulesCollator.ConvertToIcuRules(shoeboxActual);
			VerifyIcuRules(icuExpected);
			Assert.AreEqual(icuExpected, icuRulesActual);
		}

		[Test]
		public void ConvertToIcuRules_Empty()
		{
			VerifyExpectedIcuFromActualSimple(string.Empty, string.Empty);
		}

		[Test]
		public void ConvertToIcuRules_Blank()
		{
			VerifyParseIsSyntacticEquivalent(string.Empty, "\n");
			VerifyParseIsSyntacticEquivalent(string.Empty, " ");

			VerifyParseIsSyntacticEquivalent(string.Empty, "\n ");
			VerifyParseIsSyntacticEquivalent(string.Empty, " \n");
			VerifyParseIsSyntacticEquivalent(string.Empty, " \n ");

			VerifyParseIsSyntacticEquivalent(string.Empty, "\n\n");
			VerifyParseIsSyntacticEquivalent(string.Empty, "\n\n ");
			VerifyParseIsSyntacticEquivalent(string.Empty, " \n\n");
			VerifyParseIsSyntacticEquivalent(string.Empty, " \n\n ");
			VerifyParseIsSyntacticEquivalent(string.Empty, "\n \n");
			VerifyParseIsSyntacticEquivalent(string.Empty, " \n \n");
			VerifyParseIsSyntacticEquivalent(string.Empty, " \n \n ");

			VerifyParseIsSyntacticEquivalent(string.Empty, "\n\n\n");
			VerifyParseIsSyntacticEquivalent(string.Empty, "\n \n  \n  ");
		}

		[Test]
		public void ConvertToIcuRules_Digraph()
		{
			VerifyExpectedIcuFromActualSimple("&n < ng", "n\nng");
		}

		[Test]
		public void ConvertToIcuRules_SecondarySegments()
		{
			VerifyExpectedIcuFromActualSimple("&b << B < a << A", "b B\na A");
		}

		[Test]
		public void ConvertToIcuRules_SegmentsWithinParenthesis_ConsideredTertiary()
		{
			VerifyExpectedIcuFromActualSimple("&b << B < \u0101 << a <<< A", "b B\n\\u0101 (a A)");
			VerifyExpectedIcuFromActualSimple("&b << B < \u0101 <<< a << A", "b B\n(\\u0101 a) A");
		}


		[Test]
		public void ConvertToIcuRules_PaddedWithWhiteSpace()
		{
			VerifyExpectedIcuFromActualSimple("&b <<< B < \u0101 <<< a <<< A", "(b  \t B \t ) \n (\t\\u0101   \ta  A\t)\t");
		}

		[Test]
		public void ConvertToIcuRules_Parse()
		{
			VerifyExpectedIcuFromActualSimple(
				"&a <<< A << \u3022 <<< \u3064 < b << B < c <<< C < e <<< E << e\u3000 <<< E\u3000 < ng << Ng << NG < \u1234\u1234\u1234",
				"   \n   ( a   A ) (\\u3022 \\u3064)\n\nb B\n(c C)\n(e E)(e\\u3000 E\\u3000)\n   \n ng Ng NG\n\\u1234\\u1234\\u1234");

		}


		[Test]
		public void ConvertToIcuRules_SingleElement()
		{
			VerifyExpectedIcuFromActualSimple("&a","a");
		}

		[Test]
		public void ConvertToIcuRules_SingleElementWithBlanks()
		{
			VerifyParseIsSyntacticEquivalent("a", "a ");
			VerifyParseIsSyntacticEquivalent("a", " a");
			VerifyParseIsSyntacticEquivalent("a", " a ");

			VerifyParseIsSyntacticEquivalent("a", "a\n");
			VerifyParseIsSyntacticEquivalent("a", "\na");
			VerifyParseIsSyntacticEquivalent("a", "\na\n");

			VerifyParseIsSyntacticEquivalent("a", "\na ");
			VerifyParseIsSyntacticEquivalent("a", "\n a");
			VerifyParseIsSyntacticEquivalent("a", "\n a ");
			VerifyParseIsSyntacticEquivalent("a", "\n\na");
			VerifyParseIsSyntacticEquivalent("a", "\n\na\n");

			VerifyParseIsSyntacticEquivalent("a", "a \n");
			VerifyParseIsSyntacticEquivalent("a", " a\n");
			VerifyParseIsSyntacticEquivalent("a", " a \n");
			VerifyParseIsSyntacticEquivalent("a", "a\n\n");
			VerifyParseIsSyntacticEquivalent("a", "\na\n");
			VerifyParseIsSyntacticEquivalent("a", "\na\n\n");

			VerifyParseIsSyntacticEquivalent("a", "  a");
			VerifyParseIsSyntacticEquivalent("a", "  a ");
			VerifyParseIsSyntacticEquivalent("a", " a\n");
			VerifyParseIsSyntacticEquivalent("a", " \na");
			VerifyParseIsSyntacticEquivalent("a", " \na\n");

			VerifyParseIsSyntacticEquivalent("a", "a  ");
			VerifyParseIsSyntacticEquivalent("a", " a  ");
			VerifyParseIsSyntacticEquivalent("a", "a\n ");
			VerifyParseIsSyntacticEquivalent("a", "\na ");
			VerifyParseIsSyntacticEquivalent("a", "\na\n ");

			VerifyParseIsSyntacticEquivalent("a", " \na \n");
			VerifyParseIsSyntacticEquivalent("a", "\n a\n ");
			VerifyParseIsSyntacticEquivalent("a", " \n a \n ");
		}

		[Test]
		public void ConvertToIcuRules_SingleUnicodeEscChar()
		{
			VerifyExpectedIcuFromActualSimple("&a", "\\u0061");
		}

		[Test]
		public void ConvertToIcuRules_SingleUnicodeEscCharWithBlanks()
		{
			VerifyParseIsSyntacticEquivalent("a", "\\u0061 ");
			VerifyParseIsSyntacticEquivalent("a", " \\u0061");
			VerifyParseIsSyntacticEquivalent("a", " \\u0061 ");

			VerifyParseIsSyntacticEquivalent("a", "\\u0061\n");
			VerifyParseIsSyntacticEquivalent("a", "\n\\u0061");
			VerifyParseIsSyntacticEquivalent("a", "\n\\u0061\n");

			VerifyParseIsSyntacticEquivalent("a", "\n\\u0061 ");
			VerifyParseIsSyntacticEquivalent("a", "\n \\u0061");
			VerifyParseIsSyntacticEquivalent("a", "\n \\u0061 ");
			VerifyParseIsSyntacticEquivalent("a", "\n\n\\u0061");
			VerifyParseIsSyntacticEquivalent("a", "\n\n\\u0061\n");

			VerifyParseIsSyntacticEquivalent("a", "\\u0061 \n");
			VerifyParseIsSyntacticEquivalent("a", " \\u0061\n");
			VerifyParseIsSyntacticEquivalent("a", " \\u0061 \n");
			VerifyParseIsSyntacticEquivalent("a", "\\u0061\n\n");
			VerifyParseIsSyntacticEquivalent("a", "\n\\u0061\n");
			VerifyParseIsSyntacticEquivalent("a", "\n\\u0061\n\n");

			VerifyParseIsSyntacticEquivalent("a", "  \\u0061");
			VerifyParseIsSyntacticEquivalent("a", "  \\u0061 ");
			VerifyParseIsSyntacticEquivalent("a", " \\u0061\n");
			VerifyParseIsSyntacticEquivalent("a", " \n\\u0061");
			VerifyParseIsSyntacticEquivalent("a", " \n\\u0061\n");

			VerifyParseIsSyntacticEquivalent("a", "\\u0061  ");
			VerifyParseIsSyntacticEquivalent("a", " \\u0061  ");
			VerifyParseIsSyntacticEquivalent("a", "\\u0061\n ");
			VerifyParseIsSyntacticEquivalent("a", "\n\\u0061 ");
			VerifyParseIsSyntacticEquivalent("a", "\n\\u0061\n ");
		}

		[Test]
		public void ConvertToIcuRules_TwoSingleElements()
		{
			string result = SimpleRulesCollator.ConvertToIcuRules("a A");
			Assert.AreEqual("&a << A", result);
		}

		[Test]
		public void ConvertToIcuRules_TwoSingleElementsWithBlanks()
		{
			VerifyParseIsSyntacticEquivalent("a A", "a A ");
			VerifyParseIsSyntacticEquivalent("a A", " a A");
			VerifyParseIsSyntacticEquivalent("a A", " a A ");
			VerifyParseIsSyntacticEquivalent("a A", "a  A");
			VerifyParseIsSyntacticEquivalent("a A", " a  A ");

			VerifyParseIsSyntacticEquivalent("a A", "\na A");
			VerifyParseIsSyntacticEquivalent("a A", "\na A ");
			VerifyParseIsSyntacticEquivalent("a A", "\n a A");
			VerifyParseIsSyntacticEquivalent("a A", "\n a A ");
			VerifyParseIsSyntacticEquivalent("a A", "\na  A");
			VerifyParseIsSyntacticEquivalent("a A", "\n a  A ");

			VerifyParseIsSyntacticEquivalent("a A", "a A\n");
			VerifyParseIsSyntacticEquivalent("a A", "a A \n");
			VerifyParseIsSyntacticEquivalent("a A", " a A\n");
			VerifyParseIsSyntacticEquivalent("a A", " a A \n");
			VerifyParseIsSyntacticEquivalent("a A", "a  A\n");
			VerifyParseIsSyntacticEquivalent("a A", " a  A \n");

			VerifyParseIsSyntacticEquivalent("a A", "\na A\n");
			VerifyParseIsSyntacticEquivalent("a A", " \na A \n");
			VerifyParseIsSyntacticEquivalent("a A", "\n a A\n ");
			VerifyParseIsSyntacticEquivalent("a A", " \n a A \n ");
		}

		[Test]
		public void ConvertToIcuRules_TwoSingleElementsInGroup()
		{
			VerifyExpectedIcuFromActualSimple("&a <<< A", "(a A)");
		}

		[Test]
		public void ConvertToIcuRules_TwoSingleElementsInGroupWithBlanks()
		{
			VerifyParseIsSyntacticEquivalent("(a A)", " (a A)");
			VerifyParseIsSyntacticEquivalent("(a A)", " (a A) ");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n(a A)");
			VerifyParseIsSyntacticEquivalent("(a A)", "(a A)\n");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n(a A)\n");
			VerifyParseIsSyntacticEquivalent("(a A)", " \n(a A) \n");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n (a A)\n ");
			VerifyParseIsSyntacticEquivalent("(a A)", " \n (a A) \n ");
			VerifyParseIsSyntacticEquivalent("(a A)", "( a A)");
			VerifyParseIsSyntacticEquivalent("(a A)", " ( a A)");
			VerifyParseIsSyntacticEquivalent("(a A)", " ( a A) ");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n( a A)");
			VerifyParseIsSyntacticEquivalent("(a A)", "( a A)\n");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n( a A)\n");
			VerifyParseIsSyntacticEquivalent("(a A)", " \n( a A) \n");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n ( a A)\n ");
			VerifyParseIsSyntacticEquivalent("(a A)", " \n ( a A) \n ");
			VerifyParseIsSyntacticEquivalent("(a A)", "(a A )");
			VerifyParseIsSyntacticEquivalent("(a A)", " (a A )");
			VerifyParseIsSyntacticEquivalent("(a A)", " (a A ) ");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n(a A )");
			VerifyParseIsSyntacticEquivalent("(a A)", "(a A )\n");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n(a A )\n");
			VerifyParseIsSyntacticEquivalent("(a A)", " \n(a A ) \n");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n (a A )\n ");
			VerifyParseIsSyntacticEquivalent("(a A)", " \n (a A ) \n ");
			VerifyParseIsSyntacticEquivalent("(a A)", "( a A )");
			VerifyParseIsSyntacticEquivalent("(a A)", " ( a A )");
			VerifyParseIsSyntacticEquivalent("(a A)", " ( a A ) ");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n( a A )");
			VerifyParseIsSyntacticEquivalent("(a A)", "( a A )\n");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n( a A )\n");
			VerifyParseIsSyntacticEquivalent("(a A)", " \n( a A ) \n");
			VerifyParseIsSyntacticEquivalent("(a A)", "\n ( a A )\n ");
			VerifyParseIsSyntacticEquivalent("(a A)", " \n ( a A ) \n ");
		}

		[Test]
		public void ConvertToIcuRules_TwoSingleElementsOnSeparateLines()
		{
			VerifyExpectedIcuFromActualSimple("&a < A", "a\nA");
		}

		[Test]
		public void ConvertToIcuRules_TwoSingleElementsOnSeparateLinesWithBlanks()
		{
			VerifyParseIsSyntacticEquivalent("a\nA", " a\nA");
			VerifyParseIsSyntacticEquivalent("a\nA", " a\nA ");
			VerifyParseIsSyntacticEquivalent("a\nA", "\na\nA");
			VerifyParseIsSyntacticEquivalent("a\nA", "a\nA\n");
			VerifyParseIsSyntacticEquivalent("a\nA", "\na\nA\n");
			VerifyParseIsSyntacticEquivalent("a\nA", " \na\nA \n");
			VerifyParseIsSyntacticEquivalent("a\nA", "\n a\nA\n ");
			VerifyParseIsSyntacticEquivalent("a\nA", " \n a\nA \n ");
			VerifyParseIsSyntacticEquivalent("a\nA", "a \nA");
			VerifyParseIsSyntacticEquivalent("a\nA", " a \nA");
			VerifyParseIsSyntacticEquivalent("a\nA", " a \nA ");
			VerifyParseIsSyntacticEquivalent("a\nA", "\na \nA");
			VerifyParseIsSyntacticEquivalent("a\nA", "a \nA\n");
			VerifyParseIsSyntacticEquivalent("a\nA", "\na \nA\n");
			VerifyParseIsSyntacticEquivalent("a\nA", " \na \nA \n");
			VerifyParseIsSyntacticEquivalent("a\nA", "\n a \nA\n ");
			VerifyParseIsSyntacticEquivalent("a\nA", " \n a \nA \n ");
			VerifyParseIsSyntacticEquivalent("a\nA", "a\n A");
			VerifyParseIsSyntacticEquivalent("a\nA", " a\n A");
			VerifyParseIsSyntacticEquivalent("a\nA", " a\n A ");
			VerifyParseIsSyntacticEquivalent("a\nA", "\na\n A");
			VerifyParseIsSyntacticEquivalent("a\nA", "a\n A\n");
			VerifyParseIsSyntacticEquivalent("a\nA", "\na\n A\n");
			VerifyParseIsSyntacticEquivalent("a\nA", " \na\n A \n");
			VerifyParseIsSyntacticEquivalent("a\nA", "\n a\n A\n ");
			VerifyParseIsSyntacticEquivalent("a\nA", " \n a\n A \n ");
			VerifyParseIsSyntacticEquivalent("a\nA", "a \n A");
			VerifyParseIsSyntacticEquivalent("a\nA", " a \n A");
			VerifyParseIsSyntacticEquivalent("a\nA", " a \n A ");
			VerifyParseIsSyntacticEquivalent("a\nA", "\na \n A");
			VerifyParseIsSyntacticEquivalent("a\nA", "a \n A\n");
			VerifyParseIsSyntacticEquivalent("a\nA", "\na \n A\n");
			VerifyParseIsSyntacticEquivalent("a\nA", " \na \n A \n");
			VerifyParseIsSyntacticEquivalent("a\nA", "\n a \n A\n ");
			VerifyParseIsSyntacticEquivalent("a\nA", " \n a \n A \n ");
		}

		[Test]
		public void ConvertToIcuRules_ThreeSingleElementsOnSeparateLines()
		{
			VerifyExpectedIcuFromActualSimple("&c < b < a", "c\nb\na");
		}

		[Test]
		public void ConvertToIcuRules_TwoSingleElementsOnSeparateLinesWithBlankLines()
		{
			VerifyExpectedIcuFromActualSimple("&a < b", "\n a \n \n b  \n");
		}


		[Test]
		public void ConvertToIcuRules_ThreeSingleCharactersOnSameLine()
		{
			VerifyExpectedIcuFromActualSimple("&a << A << \u0301", "a A \\u0301");
		}

		[Test]
		public void ConvertToIcuRules_ThreeSingleCharactersInParenthesis()
		{
			VerifyExpectedIcuFromActualSimple("&a <<< A <<< \u0301", "(a A \\u0301)");
		}

		[Test]
		public void ConvertToIcuRules_GroupFollowedBySingleCharacter()
		{
			VerifyExpectedIcuFromActualSimple("&a <<< A << \u0301", "(a A)\\u0301");
		}

		[Test]
		public void ConvertToIcuRules_GroupFollowedBySingleCharacterWithBlanks()
		{
			VerifyParseIsSyntacticEquivalent("(a A)\\u0301", "(a A) \\u0301");
			VerifyParseIsSyntacticEquivalent("(a A)\\u0301", "(a A )\\u0301");
			VerifyParseIsSyntacticEquivalent("(a A)\\u0301", " ( a A ) \\u0301 ");
		}

		[Test]
		public void ConvertToIcuRules_SingleCharacterFollowedByGroup()
		{
			VerifyExpectedIcuFromActualSimple("&a << A <<< \u0301", "a(A \\u0301)");
		}

		[Test]
		public void ConvertToIcuRules_SingleCharacterFollowedByGroupWithBlanks()
		{
			VerifyParseIsSyntacticEquivalent("a(A \\u0301)", "a (A \\u0301)");
			VerifyParseIsSyntacticEquivalent("a(A \\u0301)", " a (A \\u0301) ");
			VerifyParseIsSyntacticEquivalent("a(A \\u0301)", " a ( A \\u0301 ) ");
		}

		[Test]
		public void ConvertToIcuRules_MultipleCharactersFormingCollationElement()
		{
			VerifyExpectedIcuFromActualSimple("&abcd << ab\u1234cd", "abcd ab\\u1234cd");
		}

		[Test]
		[ExpectedException(typeof(ParserErrorException))]
		public void ConvertToIcuRules_ParenthesisWithNoData_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "()");
		}

		[Test]
		[ExpectedException(typeof(ParserErrorException))]
		public void ConvertToIcuRules_ParenthesisWithBlank_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "( )");
		}

		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_ParenthesisWithSingleItem_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "(a)");
		}

		[Test]
		[ExpectedException(typeof(ParserErrorException))]
		public void ConvertToIcuRules_ParenthesisWithSingleDigraph_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "(ab)");
		}

		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_ParenthesisWithSingleUnicodeCharacterReference_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "(\\u0061)");
		}

		[Test]
		[ExpectedException(typeof(ParserErrorException))]
		public void ConvertToIcuRules_ParenthesisWithCharacterAndUnicodeCharacterReference_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "(b\\u0061)");
		}

		[Test]
		[ExpectedException(typeof(ParserErrorException))]
		public void ConvertToIcuRules_ParenthesisWithDigraphUnicodeCharacterReference_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "(\\uA123\\uABCD)");
		}

		[Test]
		[ExpectedException(typeof(ParserErrorException))]
		public void ConvertToIcuRules_ParenthesisWithUnicodeCharacterReferenceAndCharacter_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "(\\uA123p)");
		}


		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_NestedParenthesis_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "((a A) b)");
		}

		[Test]
		[ExpectedException(typeof(ParserErrorException))]
		public void ConvertToIcuRules_SingleCollatingElementInParenthesis_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "b (B)\n(\\u0101) a A");
		}

		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_OpenParenthesisWithoutCloseOnSameLineOneGroup_Throws()
		{
			//Expected: group close ')'
			VerifyParserError("scr0005", "(a A \\u0301\n)");
		}

		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_OpenParenthesisWithoutCloseOnSameLine_Throws()
		{
			//Expected: group close ')'
			VerifyParserError("scr0005", "a(A \\u0301\n)");
		}

		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_OpenParenthesisWithoutClose_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "(");
		}

		[Test]
		[ExpectedException(typeof(ParserErrorException))]
		public void ConvertToIcuRules_OpenParenthesisWithoutCloseWithBlanks_Throws()
		{
			// expected 2 or more collation elements in collation group
			VerifyParserError("scr0003", "( \n  ");
		}

		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_UnmatchedCloseParenthesis_Throws()
		{
			// Invalid Character
			VerifyParserError("scr0006", ")");
		}

		[Test]
		[ExpectedException(typeof(ParserErrorException))]
		public void ConvertToIcuRules_UnmatchedCloseParenthesisWithBlanks_Throws()
		{
			// Invalid Character
			VerifyParserError("scr0006", " ) \n  ");
		}

		[Test]
		[ExpectedException(typeof(ParserErrorException))]
		public void ConvertToIcuRules_BackslashWithNoCharacterFollowing_Throws()
		{
			// Invalid unicode character escape sequence
			VerifyParserError("scr0001", "\\");
		}

		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithSpaceAfterBackSlash_Throws()
		{
			// Invalid unicode character escape sequence
			VerifyParserError("scr0001", "\\ u0301");
		}

		[Test]
		[ExpectedException(typeof(ParserErrorException))]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithUpperCaseU_Throws()
		{
			// Invalid unicode character escape sequence
			VerifyParserError("scr0001", "\\U1234");
		}

		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithSpaceAfterU_Throws()
		{
			// Invalid unicode character escape sequence: missing hexadecimal digit after '\u'
			VerifyParserError("scr0002", "\\u 0301");
		}

		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithOnlyOneHexDigit_Throws()
		{
			// Invalid unicode character escape sequence: missing hexadecimal digit after '\u'
			VerifyParserError("scr0002", "\\u1");
		}

		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithOnlyTwoHexDigits_Throws()
		{
			// Invalid unicode character escape sequence: missing hexadecimal digit after '\u'
			VerifyParserError("scr0002", "\\u12");
		}

		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithOnlyThreeHexDigits_Throws()
		{
			// Invalid unicode character escape sequence: missing hexadecimal digit after '\u'
			VerifyParserError("scr0002", "\\u123");
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithFiveHexDigits_LastDigitTreatedAsCharacter()
		{
			VerifyExpectedIcuFromActualSimple("&\u12345", "\\u12345");
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReference_VerifyUnsigned()
		{
			VerifyExpectedIcuFromActualSimple("&\uA123", "\\uA123");
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReference_SurrogateLowBound()
		{
			VerifyExpectedIcuFromActualSimple("&\\ud800", "\\ud800");
		}

		[Test]
		public void ConvertToIcuRules_SurrogateCharacterLowBound()
		{
			VerifyExpectedIcuFromActualSimple("&\ud800", "\ud800");
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReference_SurrogateHighBounds()
		{
			VerifyExpectedIcuFromActualSimple("&\\udfff", "\\udfff");
		}

		[Test]
		public void ConvertToIcuRules_SurrogateCharacterHighBound()
		{
			VerifyExpectedIcuFromActualSimple("&\udfff", "\udfff");
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReference_Surrogates()
		{
			VerifyExpectedIcuFromActualSimple("&a << \\ud800\\udc00", "a \\ud800\\udc00");
		}

		[Test]
		public void ConvertToIcuRules_SurrogateCharacters()
		{
			VerifyExpectedIcuFromActualSimple("&a << \ud800\udc00", "a \ud800\udc00");
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReference_SurrogatesOutOfOrder()
		{
			//I would have thought that this would not be legal from ICU but there
			//is no error message now
			VerifyExpectedIcuFromActualSimple("&a << \\udc00\\ud800", "a \\udc00\\ud800");
		}

		[Test]
		public void ConvertToIcuRules_SurrogateCharactersOutOfOrder()
		{
			//I would have thought that this would not be legal from ICU but there
			//is no error message now
			VerifyExpectedIcuFromActualSimple("&a << \udc00\ud800", "a \udc00\ud800");
		}


		[Test]
		[ExpectedException(typeof (ParserErrorException))]
		public void ConvertToIcuRules_CollationElementUsedTwice_Throws()
		{
			// duplicate collation element
			VerifyParserError("scr0100", "a \\u0061");
		}

		[Test]
		public void ConvertToIcuRules_AsciiCharacterNotLetterOrDigit_RequiresIcuEscaping()
		{
			VerifyExpectedIcuFromActualSimple(
				"&\\u005C << \\u003C << \\u003C\\u003C << \\u003C\\u003C\\u003C << \\u003D << \\u0026",
				"\\u005c < << <<< = &");
		}

		[Test]
		public void Compare()
		{
			//naive sort
			string[] list = new string[] {"ana",
										  "anga",
										  "ango",
										  "ano",
										  "na",
										  "Na",
										  "nga",
										  "Nga",
										  "ngo",
										  "Ngo",
										  "NGo",
										  "no",
										  "No"};

			string[] expected = new string[] { "ana",
											   "ano",
											   "anga",
											   "ango",
											   "Na",
											   "na",
											   "No",
											   "no",
											   "Nga",
											   "nga",
											   "NGo",
											   "Ngo",
											   "ngo"};
			Assert.AreEqual(list.Length, expected.Length );

			SimpleRulesCollator collator = new SimpleRulesCollator("A a\nN n\nNG Ng ng\nO o");
			Array.Sort(list, collator);


			for (int i = 0; i < list.Length; i++)
			{
				Assert.AreEqual(expected[i], list[i], "at index {0}", i);
			}
		}
	}
}