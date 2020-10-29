using System;
using NUnit.Framework;
using SIL.Extensions;

namespace SIL.Tests.Extensions
{
	[TestFixture]
	public class StringExtensionTests
	{
		[Test]
		public void SplitTrimmed_StringHasSpacesOnly_GivesEmptyList()
		{
			Assert.AreEqual(0, "  ".SplitTrimmed(',').Count);
		}

		[Test]
		public void SplitTrimmed_StringIsEmpty_GivesEmptyList()
		{
			Assert.AreEqual(0, string.Empty.SplitTrimmed(',').Count);
		}

		[Test]
		public void SplitTrimmed_SingleItem_GivesSingleTrimmedItem()
		{
			var list = " hello ".SplitTrimmed(',');
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("hello", list[0]);
		}

		[Test]
		public void SplitTrimmed_StartsWithSeparator_JustSkipsFirstItem()
		{
			var list = " , first".SplitTrimmed(',');
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("first", list[0]);
		}

		[Test]
		public void SplitTrimmed_HasEmptyItems_JustSkipsThem()
		{
			var list = "one,, ,,two".SplitTrimmed(',');
			Assert.AreEqual(2, list.Count);
			Assert.AreEqual("one", list[0]);
			Assert.AreEqual("two", list[1]);
		}

		[Test]
		public void SplitTrimmed_Various_GivesEmptyList()
		{
			var list = " hello , bye ".SplitTrimmed(',');
			Assert.AreEqual(2, list.Count);
			Assert.AreEqual("hello", list[0]);
			Assert.AreEqual("bye", list[1]);
		}

		[Test]
		public void EscapeAnyUnicodeCharactersIllegalInXml_HasMixOfXmlAndIllegalChars_GivesXmlWithEscapedChars()
		{
			var s  ="This <span href=\"reference\">is well \u001F formed</span> XML!";
			Assert.AreEqual("This <span href=\"reference\">is well &#x1F; formed</span> XML!",
				s.EscapeAnyUnicodeCharactersIllegalInXml());
		}

		[Test]
		public void EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml_HasMixOfXmlAndIllegalChars_GivesPureTextEscapedChars()
		{
			var s = "This <span href=\"reference\">is not really \u001F xml.";
			Assert.AreEqual("This &lt;span href=\"reference\"&gt;is not really &#x1F; xml.",
				s.EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml());
//
//			XmlDocument x = new XmlDocument();
//			x.LoadXml("<hello>&#x1f;</hello>");
//			var writer = XmlWriter.Create(Path.GetTempFileName());
//			writer.WriteElementString("x", x.InnerText);
//
//			writer.WriteNode(z);
		 }

		[Test]
		public void Format_NormalSafeString_GivesSameAsStringFormat()
		{
			Assert.That("1={0}".FormatWithErrorStringInsteadOfException("1").Equals("1=1"));
		}


		[Test]
		public void Format_StringWithNoArgs_GivesErrorString()
		{
			Assert.That("{node}".FormatWithErrorStringInsteadOfException().Equals("{node}"));
		}
		[Test]
		public void Format_UnSafeString_GivesErrorString()
		{
			Assert.That("{foo}".FormatWithErrorStringInsteadOfException("blah").Contains("Error"));
		}

		[Test]
		public void ToUpperFirstLetter_Empty_EmptyString()
		{
			Assert.AreEqual("","".ToUpperFirstLetter());
		}
		[Test]
		public void ToUpperFirstLetter_OneCharacter_UpperCase()
		{
			Assert.AreEqual("X", "x".ToUpperFirstLetter());
		}
		[Test]
		public void ToUpperFirstLetter_Digit_ReturnsSame()
		{
			Assert.AreEqual("1abc", "1abc".ToUpperFirstLetter());
		}
		[Test]
		public void ToUpperFirstLetter_AlreadyUpper_ReturnsSame()
		{
			Assert.AreEqual("Abc", "Abc".ToUpperFirstLetter());
		}
		[Test]
		public void ToUpperFirstLetter_typical_MakesUppercase()
		{
			Assert.AreEqual("Abc", "abc".ToUpperFirstLetter());
		}

		[Test]
		public void ToUpperFirstLetter_ALLUPPERCASE_MakesOnlyFirstUppercase()
		{
			Assert.AreEqual("Abc def", "ABC DEF".ToUpperFirstLetter());
		}

		[Test]
		public void ToIntArray_EmptyString_ReturnsEmptyArray()
		{
			Assert.AreEqual(0, string.Empty.ToIntArray().Length);
		}

		[Test]
		public void ToIntArray_StringWithCommaSeparatedIntegers_ReturnsArrayOfIntegers()
		{
			var result = "1, 2, 3, 8".ToIntArray();
			Assert.AreEqual(4, result.Length);
			Assert.AreEqual(1, result[0]);
			Assert.AreEqual(2, result[1]);
			Assert.AreEqual(3, result[2]);
			Assert.AreEqual(8, result[3]);
		}

		[Test]
		public void ToIntArray_StringWithEmptySpot_ReturnsArrayOfIntegers()
		{
			var result = "1, 2, 3,".ToIntArray();
			Assert.AreEqual(3, result.Length);
			Assert.AreEqual(1, result[0]);
			Assert.AreEqual(2, result[1]);
			Assert.AreEqual(3, result[2]);
		}

		[Test]
		public void ToIntArray_StringWithFloatingPointNumbers_ReturnsEmptyArray()
		{
			var result = "1.3, 2.5, 3.6,".ToIntArray();
			Assert.AreEqual(0, result.Length);
		}

		[TestCase("hello\u0300 world", "hello world")]
		[TestCase("h\u00e9ll\u00f3 world", "hello world")]
		[TestCase("hello world", "hello world")]
		public void RemoveDiacritics_ExpectedResults(string str, string expectedResult)
		{
			Assert.AreEqual(expectedResult, str.RemoveDiacritics());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that we get a valid filename when the filename contains invalid characters.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase('_', ExpectedResult = "My__File__Dude_____.'[];funny()___")]
		[TestCase('w', ExpectedResult = "MywwFilewwDudewwwww.'[];funny()www")]
		public string SanitizeFilename_ManyInvalidCharacters_InvalidCharactersReplacedWithSpecifiedCharacter(char errorChar)
		{
			return (@"My?|File<>Dude\?*:/.'[];funny()" + "\u000a\t" + '"').SanitizeFilename(errorChar);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that we get a valid filename when the filename contains invalid characters.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(' ', ExpectedResult = "My  File Dude .'[];funny()")]
		[TestCase('.', ExpectedResult = ".My..File.Dude..'[];funny()")]
		public string SanitizeFilename_ReplacingInvalidCharactersWithSpaceOrDot_InvalidCharactersAtStartOrEndOfStringTrimmedIfInvalid(char errorChar)
		{
			return (@"<My?|File>Dude\.'[];funny()?*:/").SanitizeFilename(errorChar);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that we get a valid filename when the filename contains invalid characters
		/// and has leading/trailing spaces/dots.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(" ", ". ")]
		[TestCase("  ", "..")]
		[TestCase(" \u200C ", " .")]
		[TestCase("\u2002", ". .")]
		[TestCase("\u2000", "\u200A")]
		[TestCase("\u200C", "\u200D")]
		public void SanitizeFilename_LeadingAndTrailingSpacesAndDotsPlusInvalidCharacters_InvalidCharactersReplacedAndLeadingAndTrailingJunkTrimmed(
			string leading, string trailing)
		{
			Assert.AreEqual("My__File__Dude_____.'[];funny()____",
				(leading + @"My?|File<>Dude\?*:/.'[];funny()" + "\u000a\t\uFFFC" + '"' + trailing).SanitizeFilename('_'));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that we throw an exception if caller passes a bogus errorChar.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase('<')]
		[TestCase('?')]
		public void SanitizeFilename_ErrorCharInvalid_Throws(char errorChar)
		{
			Assert.Throws<ArgumentException>(() =>
				(@"My?|File<>Dude\?*:/.'[];funny()" + "\u000a\t" + '"').SanitizeFilename(errorChar));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that we get a valid filename when the filename contains invalid characters.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void SanitizeFilename_NoBreakSpace_NbspReplacedWithNormalSpace()
		{
			Assert.AreEqual("My File Dude.txt", "My File\u00A0Dude.txt".SanitizeFilename('_', true));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that final periods and spaces are stripped.
		/// (See https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file?redirectedfrom=MSDN#naming_conventions)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(".")]
		[TestCase(". ")]
		[TestCase(" .")]
		[TestCase("..")]
		[TestCase(". .")]
		[TestCase("\u2002")]
		[TestCase("\u2000")]
		[TestCase("\u200A")]
		[TestCase("\u200C")]
		[TestCase("\u200D")]
		public void SanitizeFilename_EndsWithAndContainsPeriodsAndOrSpaces_TrailingPeriodsAndSpacesRemoved(string trailing)
		{
			Assert.AreEqual($"My best{trailing}File", ($"My best{trailing}File" + trailing).SanitizeFilename('_'));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that final periods and spaces are stripped.
		/// (See https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file?redirectedfrom=MSDN#naming_conventions)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(".\u00A0.")]
		[TestCase("\u00A0")]
		public void SanitizeFilename_EndsWithPeriodsAndOrSpaces_TrailingPeriodsAndSpacesRemoved(string trailing)
		{
			Assert.AreEqual("My best.File", ("My best.File" + trailing).SanitizeFilename('_'));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that leading spaces and formatting characters are stripped.
		/// (See https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file?redirectedfrom=MSDN#naming_conventions)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(" ")]
		[TestCase("  ")]
		[TestCase("\u00A0")]
		[TestCase("\uFEFF")]
		[TestCase("\u200B")]
		[TestCase("\u200A")]
		public void SanitizeFilename_StartsWithSpaces_LeadingPeriodsAndSpacesRemoved(string leading)
		{
			Assert.AreEqual("My best.File", (leading + "My best.File").SanitizeFilename('_'));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that an empty filename or one consisting only of spaces and dots results in
		/// a single underscore (which is a legal - albeit not very nice) filename. Note:
		/// Originally I was going to have it throw an exception since it's a really weird edge
		/// case, but knowing that it is used to come up with a default filename, if ever it
		/// were to be passed junk like this (which it probably never will), neither crashing
		/// nor having to deal with it in a try-catch would be helpful.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(".")]
		[TestCase(". ")]
		[TestCase(" .")]
		[TestCase("..")]
		[TestCase(". .")]
		[TestCase(" ")]
		[TestCase("")]
		[TestCase("\u00A0")]
		[TestCase("\u00A0.")]
		public void SanitizeFilename_StringContainsNothingButPeriodsAndOrSpaces_ReturnsSingleUnderscore(string orig)
		{
			Assert.AreEqual("_", orig.SanitizeFilename('_'));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that an empty filename or one consisting only of spaces and dots results in
		/// a single underscore (which is a legal - albeit not very nice) filename. Note:
		/// Originally I was going to have it throw an exception since it's a really weird edge
		/// case, but knowing that it is used to come up with a default filename, if ever it
		/// were to be passed junk like this (which it probably never will), neither crashing
		/// nor having to deal with it in a try-catch would be helpful.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(@"..\relative\..\path")]
		[TestCase(@"c:\root")]
		public void SanitizePath_StringContainsCharactersThatAreNotValidFileCharactersButAreValidPathCharacters_NoChange(string orig)
		{
			Assert.AreEqual(orig, orig.SanitizePath('_'));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that an empty filename or one consisting only of spaces and dots results in
		/// a single underscore (which is a legal - albeit not very nice) filename. Note:
		/// Originally I was going to have it throw an exception since it's a really weird edge
		/// case, but knowing that it is used to come up with a default filename, if ever it
		/// were to be passed junk like this (which it probably never will), neither crashing
		/// nor having to deal with it in a try-catch would be helpful.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(@"blah\blah\.", ExpectedResult = @"blah\blah\")] // This would also be acceptable: blah\blah\.
		[TestCase(@"c:\root\..", ExpectedResult = @"c:\root\..")]
		[TestCase(@"My directory is awesome.", ExpectedResult = @"My directory is awesome")]
		[TestCase(@"..", ExpectedResult = @"_")]
		[TestCase(@"\.", ExpectedResult = @"\")]// This would also be acceptable: \.
		[TestCase(@".\..", ExpectedResult = @".\..")]
		[TestCase(@".\...", ExpectedResult = @".\..")]
		public string SanitizePath_StringHasTrailingDots_TrailingDotsTrimmed(string orig)
		{
			return orig.SanitizePath('_');
		}
	}
}
