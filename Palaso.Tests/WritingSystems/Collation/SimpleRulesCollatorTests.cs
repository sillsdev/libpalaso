using System;
using NUnit.Framework;
using Palaso.WritingSystems.Collation;
using Spart;

namespace Palaso.Tests.WritingSystems.Collation
{
	[TestFixture]
	public class SimpleRulesCollatorTests
	{
		private const string IcuStart = "&[before 1] [first regular] < ";
		static private void VerifyIcuRules(string icuRules)
		{
			new IcuRulesCollator(icuRules);
		}

		static private void VerifyParserError(string errorId, string rules, long line, long column)
		{
			try
			{
				SimpleRulesCollator.ConvertToIcuRules(rules);
			}
			catch (ParserErrorException e)
			{
				Assert.AreEqual(errorId, e.ParserError.ErrorId);
				Assert.AreEqual(line, e.ParserError.Line);
				Assert.AreEqual(column, e.ParserError.Column);

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
			VerifyExpectedIcuFromActualSimple(IcuStart + "n < ng", "n\nng");
		}

		[Test]
		public void ConvertToIcuRules_SecondarySegments()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "b << B < a << A", "b B\na A");
		}

		[Test]
		public void ConvertToIcuRules_SegmentsWithinParenthesis_ConsideredTertiary()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "b << B < \u0101 << a <<< A", "b B\n\\u0101 (a A)");
			VerifyExpectedIcuFromActualSimple(IcuStart + "b << B < \u0101 <<< a << A", "b B\n(\\u0101 a) A");
		}


		[Test]
		public void ConvertToIcuRules_PaddedWithWhiteSpace()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "b <<< B < \u0101 <<< a <<< A", "(b  \t B \t ) \n (\t\\u0101   \ta  A\t)\t");
		}

		[Test]
		public void ConvertToIcuRules_Parse()
		{
			VerifyExpectedIcuFromActualSimple(
				IcuStart + "a <<< A << \u3022 <<< \u3064 < b << B < c <<< C < e <<< E << e\u3000 <<< E\u3000 < ng << Ng << NG < \u1234\u1234\u1234",
				"   \n   ( a   A ) (\\u3022 \\u3064)\n\nb B\n(c C)\n(e E)(e\\u3000 E\\u3000)\n   \n ng Ng NG\n\\u1234\\u1234\\u1234");

		}


		[Test]
		public void ConvertToIcuRules_SingleElement()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "a", "a");
		}


		[Test]
		public void ConvertToIcuRules_NoHyphenInRules_HasIgnoreHyphen()
		{
			//&[last primary ignorable] <<< '-' <<<
			VerifyExpectedIcuFromActualSimple(IcuStart + "a", "a");
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
			VerifyExpectedIcuFromActualSimple(IcuStart + "a", "\\u0061");
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
			Assert.AreEqual(IcuStart + "a << A", result);
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
			VerifyExpectedIcuFromActualSimple(IcuStart + "a <<< A", "(a A)");
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
			VerifyExpectedIcuFromActualSimple(IcuStart + "a < A", "a\nA");
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
			VerifyExpectedIcuFromActualSimple(IcuStart + "c < b < a", "c\nb\na");
		}

		[Test]
		public void ConvertToIcuRules_TwoSingleElementsOnSeparateLinesWithBlankLines()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "a < b", "\n a \n \n b  \n");
		}


		[Test]
		public void ConvertToIcuRules_ThreeSingleCharactersOnSameLine()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "a << A << \u0301", "a A \\u0301");
		}

		[Test]
		public void ConvertToIcuRules_ThreeSingleCharactersInParenthesis()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "a <<< A <<< \u0301", "(a A \\u0301)");
		}

		[Test]
		public void ConvertToIcuRules_GroupFollowedBySingleCharacter()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "a <<< A << \u0301", "(a A)\\u0301");
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
			VerifyExpectedIcuFromActualSimple(IcuStart + "a << A <<< \u0301", "a(A \\u0301)");
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
			VerifyExpectedIcuFromActualSimple(IcuStart + "abcd << ab\u1234cd", "abcd ab\\u1234cd");
		}

		[Test]
		public void ConvertToIcuRules_ParenthesisWithNoData_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "()", 1, 2)
			);
		}

		[Test]
		public void ConvertToIcuRules_ParenthesisWithBlank_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "( )", 1, 3)
			);
		}

		[Test]
		public void ConvertToIcuRules_ParenthesisWithSingleItem_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "(a)", 1, 3)
			);
		}

		[Test]
		public void ConvertToIcuRules_ParenthesisWithSingleDigraph_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "(ab)", 1, 4)
			);
		}

		[Test]
		public void ConvertToIcuRules_ParenthesisWithSingleUnicodeCharacterReference_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "(\\u0061)", 1, 8)
			);
		}

		[Test]
		public void ConvertToIcuRules_ParenthesisWithCharacterAndUnicodeCharacterReference_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "(b\\u0061)", 1, 9)
			);
		}

		[Test]
		public void ConvertToIcuRules_ParenthesisWithDigraphUnicodeCharacterReference_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "(\\uA123\\uABCD)", 1, 14)
			);
		}

		[Test]
		public void ConvertToIcuRules_ParenthesisWithUnicodeCharacterReferenceAndCharacter_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "(\\uA123p)", 1, 9)
			);
		}


		[Test]
		public void ConvertToIcuRules_NestedParenthesis_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "((a A) b)", 1,2)
			);
		}

		[Test]
		public void ConvertToIcuRules_SingleCollatingElementInParenthesis_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "b (B)\n(\\u0101) a A", 1, 5)
			);
		}

		[Test]
		public void ConvertToIcuRules_OpenParenthesisWithoutCloseOnSameLineOneGroup_Throws()
		{
			Assert.Throws<ParserErrorException>(
				//Expected: group close ')'
				() => VerifyParserError("scr0005", "(a A \\u0301\n)", 1, 12)
			);
		}

		[Test]
		public void ConvertToIcuRules_OpenParenthesisWithoutCloseOnSameLine_Throws()
		{
			Assert.Throws<ParserErrorException>(
				//Expected: group close ')'
				() => VerifyParserError("scr0005", "a(A \\u0301\n)", 1, 11)
			);
		}

		[Test]
		public void ConvertToIcuRules_OpenParenthesisWithoutClose_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "(", 1, 2)
			);
		}

		[Test]
		public void ConvertToIcuRules_OpenParenthesisWithoutCloseWithBlanks_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// expected 2 or more collation elements in collation group
				() => VerifyParserError("scr0003", "( \n  ", 1, 3)
			);
		}

		[Test]
		public void ConvertToIcuRules_UnmatchedCloseParenthesis_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// Invalid Character
				() => VerifyParserError("scr0006", ")", 1, 1)
			);
		}

		[Test]
		public void ConvertToIcuRules_UnmatchedCloseParenthesisWithBlanks_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// Invalid Character
				() => VerifyParserError("scr0006", " ) \n  ", 1, 2)
			);
		}

		[Test]
		public void ConvertToIcuRules_BackslashWithNoCharacterFollowing_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// Invalid unicode character escape sequence
				() => VerifyParserError("scr0001", "\\", 1, 2)
			);
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithSpaceAfterBackSlash_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// Invalid unicode character escape sequence
				() => VerifyParserError("scr0001", "\\ u0301", 1, 2)
			);
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithUpperCaseU_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// Invalid unicode character escape sequence
				() => VerifyParserError("scr0001", "\\U1234", 1, 2)
			);
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithSpaceAfterU_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// Invalid unicode character escape sequence: missing hexadecimal digit after '\u'
				() => VerifyParserError("scr0002", "\\u 0301", 1, 3)
			);
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithOnlyOneHexDigit_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// Invalid unicode character escape sequence: missing hexadecimal digit after '\u'
				() => VerifyParserError("scr0002", "\\u1", 1,4)
			);
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithOnlyTwoHexDigits_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// Invalid unicode character escape sequence: missing hexadecimal digit after '\u'
				() => VerifyParserError("scr0002", "\\u12",1,5)
			);
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithOnlyThreeHexDigits_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// Invalid unicode character escape sequence: missing hexadecimal digit after '\u'
				() => VerifyParserError("scr0002", "\\u123",1,6)
			);
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReferenceWithFiveHexDigits_LastDigitTreatedAsCharacter()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "\u12345", "\\u12345");
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReference_VerifyUnsigned()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "\uA123", "\\uA123");
		}

		[Test]
		[Category("ICU54OrHigher")]
		public void ConvertToIcuRules_UnicodeCharacterReference_SurrogateLowBound_Throws()
		{
			Assert.Throws<ArgumentException>(
				// Invalid unicode character escape sequence: 
				() => VerifyExpectedIcuFromActualSimple(IcuStart + "\\ud800", "\\ud800")
			);
		}

		[Test]
		[Category("ICU54OrHigher")]
		public void ConvertToIcuRules_SurrogateCharacterLowBound_Throws()
		{
			Assert.Throws<ArgumentException>(
				// Invalid unicode character escape sequence: 
				() => VerifyExpectedIcuFromActualSimple(IcuStart + "\ud800", "\ud800")
			);
		}

		[Test]
		[Category("ICU54OrHigher")]
		public void ConvertToIcuRules_UnicodeCharacterReference_SurrogateHighBounds_Throws()
		{
			Assert.Throws<ArgumentException>(
				// Invalid unicode character escape sequence: 
				() => VerifyExpectedIcuFromActualSimple(IcuStart + "\\udfff", "\\udfff")
			);
		}

		[Test]
		[Category("ICU54OrHigher")]
		public void ConvertToIcuRules_SurrogateCharacterHighBound_Throws()
		{
			Assert.Throws<ArgumentException>(
				// Invalid unicode character escape sequence: 
				() => VerifyExpectedIcuFromActualSimple(IcuStart + "\udfff", "\udfff")
			);
		}

		[Test]
		public void ConvertToIcuRules_UnicodeCharacterReference_Surrogates()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "a << \\ud800\\udc00", "a \\ud800\\udc00");
		}

		[Test]
		public void ConvertToIcuRules_SurrogateCharacters()
		{
			VerifyExpectedIcuFromActualSimple(IcuStart + "a << \ud800\udc00", "a \ud800\udc00");
		}

		[Test]
		[Category("ICU54OrHigher")]
		public void ConvertToIcuRules_UnicodeCharacterReference_SurrogatesOutOfOrder_Throws()
		{
			Assert.Throws<ArgumentException>(
				// Invalid unicode character escape sequence: 
				() => VerifyExpectedIcuFromActualSimple(IcuStart + "a << \\udc00\\ud800", "a \\udc00\\ud800")
			);
		}

		[Test]
		[Category("ICU54OrHigher")]
		public void ConvertToIcuRules_SurrogateCharactersOutOfOrder_Throws()
		{
			Assert.Throws<ArgumentException>(
				// Invalid unicode character escape sequence: 
				() => VerifyExpectedIcuFromActualSimple(IcuStart + "a << \udc00\ud800", "a \udc00\ud800")
			);
		}


		[Test]
		public void ConvertToIcuRules_CollationElementUsedTwice_Throws()
		{
			Assert.Throws<ParserErrorException>(
				// duplicate collation element
				() => VerifyParserError("scr0100", "a \\u0061", 1, 9)
			);
		}

		[Test]
		public void ConvertToIcuRules_AsciiCharacterNotLetterOrDigit_RequiresIcuEscaping()
		{
			VerifyExpectedIcuFromActualSimple(
				IcuStart + "\\\\ << \\< << \\<\\< << \\<\\<\\< << \\= << \\&",
				"\\u005c < << <<< = &");
		}

		[Test]
		public void ParseError_CorrectLineAndOffset()
		{
			Assert.Throws<ParserErrorException>(
				() => VerifyParserError("scr0006", "ph\na A)\nb B\nc C",2,4)
			);
		}


		[Test]
		public void Compare_CapsAsSecondaryDistinction()
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

		[Test]
		public void Compare_CapsAsPrimaryDistinction()
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

			string[] expected = new string[] {
											   "ana",
											   "ano",
											   "anga",
											   "ango",
											   "Na",
											   "No",
											   "na",
											   "no",
											   "NGo",
											   "Nga",
											   "Ngo",
											   "nga",
											   "ngo"};
			Assert.AreEqual(list.Length, expected.Length);

			SimpleRulesCollator collator = new SimpleRulesCollator("A\na\nN\nn\nNG\nNg\nng\nO\no");
			Array.Sort(list, collator);


			for (int i = 0; i < list.Length; i++)
			{
				Assert.AreEqual(expected[i], list[i], "at index {0}", i);
			}
		}

		private void VerifyExpectedSort(string simpleRules, string[] original, string[] expected)
		{
			Assert.AreEqual(original.Length, expected.Length);

			SimpleRulesCollator collator = new SimpleRulesCollator(simpleRules);
			Array.Sort(original, collator);


			for (int i = 0; i < original.Length; i++)
			{
				Assert.AreEqual(expected[i], original[i], "at index {0}", i);
			}
		}

		[Test]
		public void Compare_Hyphens()
		{
			var original = new string[] {"anna",
										"-ana",
										  "ana"};

			var expected = new string[] { "ana",
										   "anna",
										   "-ana"};
			VerifyExpectedSort("a\nb\n-", original, expected);
		}

		[Test]
		public void CollatingSequenceBeginsWithDigraph()
		{
			//naive sort
			string[] list = new string[] {"hello",
										  "me",
										  "phone",
										  "test",
										  "world"};

			string[] expected = new string[] { "phone",
												"hello",
												"me",
												"test",
												"world"};
			Assert.AreEqual(list.Length, expected.Length);

			SimpleRulesCollator collator = new SimpleRulesCollator("ph (Ph  e)\na A\nb B\nc C\nd D");
			Array.Sort(list, collator);


			for (int i = 0; i < list.Length; i++)
			{
				Assert.AreEqual(expected[i], list[i], "at index {0}", i);
			}


		}


		[Test]
		public void UndefinedCollatingSequencesSortToBottom()
		{
			//naive sort
			string[] list = new string[] {"hello",
										  "me",
										  "phone",
										  "\u0268lo",
										  "igloo",
										  "test",
										  "alpha",
										  "echo",
										  "world"};

			string[] expected = new string[] { "igloo",
												"\u0268lo",
												"alpha",
												"echo",
												"hello",
												"me",
												"phone",
												"test",
												"world"};
			Assert.AreEqual(list.Length, expected.Length);

			SimpleRulesCollator collator = new SimpleRulesCollator("i \\u0268");
			Array.Sort(list, collator);


			for (int i = 0; i < list.Length; i++)
			{
				Assert.AreEqual(expected[i], list[i], "at index {0}", i);
			}


		}

	}
}