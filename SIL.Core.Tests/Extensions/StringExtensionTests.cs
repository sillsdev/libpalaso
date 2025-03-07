using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SIL.Extensions;
using static System.String;
using static SIL.Extensions.StringExtensions;

namespace SIL.Tests.Extensions
{
	[TestFixture]
	public class StringExtensionTests
	{
		[SetUp]
		public void Setup()
		{
			Assert.That(StringExtensions.AltImplGetUnicodeCategory, Is.Null);
		}

		[TearDown]
		public void TearDown()
		{
			StringExtensions.AltImplGetUnicodeCategory = null;
		}

		[Test]
		public void SplitTrimmed_StringHasSpacesOnly_GivesEmptyList()
		{
			Assert.AreEqual(0, "  ".SplitTrimmed(',').Count);
		}

		[Test]
		public void SplitTrimmed_StringIsEmpty_GivesEmptyList()
		{
			Assert.AreEqual(0, Empty.SplitTrimmed(',').Count);
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
			var s = "This <span href=\"reference\">is well \u001F formed</span> XML!";
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
			Assert.AreEqual("", "".ToUpperFirstLetter());
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
			Assert.AreEqual(0, Empty.ToIntArray().Length);
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

		[Test]
		public void SplitLines_NoLineBreaks_ReturnsArrayWithSingleString()
		{
			Assert.That("just this".SplitLines(), Is.EquivalentTo(new [] {"just this"}));
		}
		
		[TestCase("line1\rline2")]
		[TestCase("line1\nline2")]
		[TestCase("line1\nline2\n")]
		[TestCase("line1\nline2\n\r")]
		[TestCase("line1\n\rline2")]
		[TestCase("line1\n\rline2\r\n")]
		[TestCase("line1\r\nline2")]
		[TestCase("line1\r\rline2")]
		[TestCase("line1\n\nline2")]
		[TestCase("line1\n\r\n\rline2")]
		[TestCase("line1\r\n\r\nline2")]
		public void SplitLines_LineBreaks_ReturnsArrayWithAStringForEachLine(string str)
		{
			var result = str.SplitLines();
			Assert.That(result, Is.EquivalentTo(new [] {"line1", "line2"}));
		}

		[TestCase("hello\u0300 world", "hello world")]
		[TestCase("h\u00e9ll\u00f3 world", "hello world")]
		[TestCase("hello world", "hello world")]
		public void RemoveDiacritics_ExpectedResults(string str, string expectedResult)
		{
			Assert.AreEqual(expectedResult, str.RemoveDiacritics());
		}

		[TestCaseSource(nameof(GetInvalidFilenameCharacters))]
		public void SanitizeFilename_InvalidCharacter_Replaced(char invalidChar)
		{
			Assert.AreEqual("File name.txt", $"File{invalidChar}name.txt".SanitizeFilename(' '));
		}

		private static IEnumerable<char> GetInvalidFilenameCharacters() =>
			System.IO.Path.GetInvalidFileNameChars();

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that we get a valid filename when the filename contains invalid characters.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase('_')]
		[TestCase('w')]
		public void SanitizeFilename_ManyInvalidCharacters_InvalidCharactersReplacedWithSpecifiedCharacter(char errorChar)
		{
			var invalidFilename = @"My?|File<>Dude\?*:/.'[];funny()" + "\u000a\t" +
				GetInvalidFilenameCharacters().First();
			var validFileName = invalidFilename.SanitizeFilename(errorChar);
			Assert.IsFalse(validFileName.Any(c => GetInvalidFilenameCharacters().Contains(c)));
			Assert.IsTrue(validFileName.Contains(errorChar));
		}
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that we get a valid filename that does not end with spaces when the filename
		/// ends with invalid characters and the error replacement character is a space.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(' ')]
		[TestCase('\u200C')]
		public void SanitizeFilename_LastCharIsInvalidAndReplacementCharIsSpace_SanitizedStringDoesNotEndWithSpace(char errorChar)
		{
			var invalidFilename = @"My?|File<>Dude\?*:/.'[];funny" +
				GetInvalidFilenameCharacters().First();
			var validFileName = invalidFilename.SanitizeFilename(errorChar);
			Assert.IsFalse(validFileName.Any(c => GetInvalidFilenameCharacters().Contains(c)));
			Assert.IsTrue(validFileName.EndsWith("funny"));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that we get a valid filename when the filename contains invalid characters.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(' ', ExpectedResult = "My")]
		[TestCase('.', ExpectedResult = ".M")]
		public string SanitizeFilename_ReplacingInvalidCharactersWithSpaceOrDot_InvalidCharactersAtStartOrEndOfStringTrimmedIfInvalid(char errorChar)
		{
			var someInvalidCharacters = Join("", GetInvalidFilenameCharacters().Take(5));
			var invalidFilename = someInvalidCharacters.First() + @"My?|File>Dude\.'[];funny()" +
				someInvalidCharacters;
			var sanitized = invalidFilename.SanitizeFilename(errorChar);
			Assert.That(sanitized.EndsWith(".'[];funny()"));
			return sanitized.Substring(0, 2);
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
			var filenameWithLeadingAndTrailingJunk = leading +
				@"My?|File<>Dude\?*:/.'[];funny()" + GetInvalidFilenameCharacters().Last() + trailing;
			var sanitizedFilename = filenameWithLeadingAndTrailingJunk.SanitizeFilename('_');
			Assert.That(sanitizedFilename.StartsWith("My"));
			Assert.That(sanitizedFilename.EndsWith("funny()_"));
			Assert.IsFalse(sanitizedFilename.Any(c => GetInvalidFilenameCharacters().Contains(c)));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that we throw an exception if caller passes a bogus errorChar.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test, TestCaseSource(nameof(GetInvalidFilenameCharacters))]
		public void SanitizeFilename_ErrorCharInvalid_Throws(char errorChar)
		{
			Assert.Throws<ArgumentException>(() => @"My?|Dude\?funny".SanitizeFilename(errorChar));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that we get a filename with a normal space when the given filename contains a
		/// non-breaking space.
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
		/// (See https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file#naming-conventions)
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
		/// (See https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file#naming-conventions)
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
		/// (See https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file#naming-conventions)
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
		/// Tests that valid path characters (colons, slashes and leading dots) that are (at least on
		/// Windows) invalid in filenames are not replaced.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(@"..\relative\..\path")]
		[TestCase(@"../relative/../path")]
		[TestCase(@"c:\root")]
		[TestCase(@"c:root")] // This is valid, though some apps might not allow with it.
		[TestCase(@".git")]
		[TestCase(@"c:")]
		[TestCase(@"Z:")]
		public void SanitizePath_StringContainsCharactersThatAreNotValidFileCharactersButAreValidPathCharacters_NoChange(string orig)
		{
			Assert.AreEqual(orig, orig.SanitizePath('_'));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that on Windows improperly placed colons are replaced.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Platform(Include = "win", Reason = "Rules about colons and slashes only apply to Windows.")]
		[TestCase(@"\:c", ExpectedResult = @"\_c")]
		[TestCase(@"c::\", ExpectedResult = @"c:_\")]
		[TestCase(@"c:\:", ExpectedResult = @"c:\_")]
		[TestCase(@"%:\yeah", ExpectedResult = @"%_\yeah")]
		[TestCase(@"abc:", ExpectedResult = "abc_")]
		[TestCase(@"c :", ExpectedResult = "c _")]
		public string SanitizePath_StringContainsBogusColonsOrSlashes_Sanitized(string orig)
		{
			return orig.SanitizePath('_');
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that trailing dots are removed from a path unless they are valid relative path
		/// specifiers.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(@"blah\blah\.", ExpectedResult = @"blah\blah\")] // This would also be acceptable: blah\blah\.
		[TestCase(@"blah\blah/.", ExpectedResult = @"blah\blah/")] // This would also be acceptable: blah\blah/.
		[TestCase(@"My directory is awesome.", ExpectedResult = @"My directory is awesome")]
		[TestCase(".", ExpectedResult = ".")]
		[TestCase(@"..", ExpectedResult = @"..")]
		[TestCase(@". .", ExpectedResult = @"_")]
		[TestCase(@"\.", ExpectedResult = @"\")] // This would also be acceptable: \.
		[TestCase(@".\...", ExpectedResult = @".\")] // REVIEW: maybe we want: .\_
		[TestCase(@"....", ExpectedResult = @"_")]
		public string SanitizePath_StringHasTrailingDots_InvalidTrailingDotsTrimmed(string orig)
		{
			return orig.SanitizePath('_');
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that two trailing dots are not removed from the end of the path if they occur
		/// following the primary directory separator char.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(@"c:\root")]
		[TestCase(@"c:/root")]
		[TestCase(".")]
		public void SanitizePath_StringHasTwoTrailingDotsFollowingDirectorySeparator_NoChange(string prefix)
		{
			var directorySeparators = new HashSet<char>();
			directorySeparators.Add(Path.DirectorySeparatorChar);
			// On Linux, AltDirectorySeparatorChar is the same, so there will only be one.
			directorySeparators.Add(Path.AltDirectorySeparatorChar);

			foreach (var orig in directorySeparators.Select(separator => $"{prefix}{separator}.."))
				Assert.AreEqual(orig, orig.SanitizePath('_'));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase("Hello", "Hello", ExpectedResult = "Hello")] // two equal strings
		[TestCase("Hello over there", "Hello over here",
			ExpectedResult = "Hello over")] // LCS at the start
		[TestCase("I want to be over there", "You want to be over here",
			ExpectedResult = "want to be over")] // LCS in the middle
		[TestCase("Will you come to visit my relatives?", "Do I ever visit my relatives?",
			ExpectedResult = "visit my relatives?")] // LCS at the end
		[TestCase("This sentence has common words", "This paragraph has common words",
			ExpectedResult = "has common words")] // two common strings, find the longest
		[TestCase("frog frog snake frog frog frog frog", "frog frog frog snake frog frog",
			ExpectedResult = "frog frog snake frog frog")] // repeated words
		public string GetLongestUsefulCommonSubstring_Basic(string s1, string s2)
		{
			var result = s1.GetLongestUsefulCommonSubstring(s2, out bool fWholeWord, .15);
			Assert.IsTrue(fWholeWord);
			return result;
		}

		/// <summary>
		/// Strings have nothing at all in common
		/// </summary>
		[TestCase("We have nothing in common", "absolutely nill items")]
		// pathological cases
		[TestCase("", "")]
		[TestCase(null, "")]
		[TestCase("", "Hello there")]
		public void NoCommonString(string s1, string s2)
		{
			Assert.AreEqual(Empty, s1.GetLongestUsefulCommonSubstring(s2, out bool _, 0.15));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures that if a useful
		/// substring contains one or more whole words that parts of adjacent words will not be
		/// included.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase("Hello over here", "Jello over here", ExpectedResult = "over here")]
		[TestCase("Come over heretic over here.", "cover here over here", ExpectedResult = "over here")]
		[TestCase("Come over heretic thumb mover here.", "cover here over here thumb", ExpectedResult = "thumb")]
		public string GetLongestUsefulCommonSubstring_AvoidPartialWordsInIsolatingLanguages(string s1, string s2)
		{
			var result = s1.GetLongestUsefulCommonSubstring(s2, out bool fWholeWord, .15);
			Assert.IsTrue(fWholeWord);
			return result;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method when a minimum percentage is not
		/// supplied. This test ensures that if the longest substring contains only part of one
		/// word, the match will not be returned.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLongestUsefulCommonSubstring_OnlyConsiderWholeWords()
		{
			// No whole-word match
			Assert.AreEqual(Empty, "Mimamamedijoquenofueraafuera".GetLongestUsefulCommonSubstring(
				"Mipamamedijoquenofueraalaiglesia", out _));

			// Use shorter whole-word match instead of longer partial-word match.
			Assert.AreEqual("over", "Come over heretic big thumbsucker mover here."
				.GetLongestUsefulCommonSubstring("cover here over here big thumbelina",
					out var fWholeWord));
			Assert.IsTrue(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures that if a useful substring
		/// contains only part of one word, the match will be returned (as long as there are no
		/// whole-word matches - see following test as well).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLongestUsefulCommonSubstring_AgglutinativeLanguage_AllowPartialWordIn()
		{
			Assert.AreEqual("amamedijoquenofueraa",
				StringExtensions.GetLongestUsefulCommonSubstring("Mimamamedijoquenofueraafuera",
					"Mipamamedijoquenofueraalaiglesia", out var fWholeWord, .15));
			Assert.IsFalse(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures that if a substring
		/// contains only part of one word but there is a shorter whole-word match, the shorter
		/// whole-word match is returned instead of longer partial-word match.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLongestUsefulCommonSubstring_IsolatingLanguage_PreferWholeWordMatchOverPartial()
		{
			Assert.AreEqual("over", "Come over heretic big thumbsucker mover here."
				.GetLongestUsefulCommonSubstring("cover here over here big thumbelina",
				out var fWholeWord, .15));
			Assert.IsTrue(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures that adjacent
		/// punctuation or whitespace is not included in whole-word matches.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase("\"best friends!\"", "best friends.", ExpectedResult = "best friends")]
		[TestCase("We are best friends.", "I am the best; friends are worthless.", ExpectedResult = "friends")]
		[TestCase("\"best friends!\"", "\"We are best friends!\"", ExpectedResult = "best friends!\"")]
		[TestCase("\"You were best friends!\"", "We are best friends!", ExpectedResult = "best friends!")]
		[TestCase("best friends forever.", "I am the best; friends are nice.", ExpectedResult = "friends")]
		public string GetLongestUsefulCommonSubstring_Punctuation(string s1, string s2)
		{
			var result = s1.GetLongestUsefulCommonSubstring(s2, out bool fWholeWord, .15);
			Assert.IsTrue(fWholeWord);
			return result;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures that ORCs don't match.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLongestUsefulCommonSubstring_PreventOrcMatching_WholeWordMatch()
		{
			Assert.AreEqual("friends", ("best " + kObjReplacementChar + " friends")
				.GetLongestUsefulCommonSubstring("best " + kObjReplacementChar + " friends",
				out var fWholeWord, .15));
			Assert.IsTrue(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures that we don't get
		/// an index-out-of-range exception.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLongestUsefulCommonSubstring_StringEndsWithORC_NoIndexOutOfRangeError()
		{
			var s1 = "Cuándo pelearon el " + kObjReplacementChar + " y " + kObjReplacementChar;
			var s2 = "Qué hicieron el " + kObjReplacementChar + " y " + kObjReplacementChar;
			Assert.AreEqual("el", s1.GetLongestUsefulCommonSubstring(s2, out var fWholeWord));
			Assert.IsTrue(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test tests that leading punctuation is
		/// included in the match.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLongestUsefulCommonSubstring_LeadingPunctuationWithOrc()
		{
			Assert.AreEqual("\u00BFEntonces que\u0301",
				StringExtensions.GetLongestUsefulCommonSubstring(
				"\u00BFEntonces que\u0301 " + kObjReplacementChar + "?",
				"\u00BFEntonces que\u0301 " + kObjReplacementChar + "?", out var fWholeWord, .15));
			Assert.IsTrue(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures that ORCs don't match
		/// when matching partial words.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLongestUsefulCommonSubstring_PreventOrcMatching_PartialWordMatch()
		{
			Assert.AreEqual(" f", ("floppy " + kObjReplacementChar + " friends")
				.GetLongestUsefulCommonSubstring("best " + kObjReplacementChar + " forks",
				out var fWholeWord, .15));
			Assert.IsFalse(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures that ORCs don't match
		/// when matching partial words.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLongestUsefulCommonSubstring_PreventOrcMatching_OnlyOrcsAndWhitespaceMatch()
		{
			Assert.AreEqual(Empty, StringExtensions.GetLongestUsefulCommonSubstring(
				kObjReplacementChar + " " + kObjReplacementChar + "ab?",
				kObjReplacementChar + " " + kObjReplacementChar + "?", out var fWholeWord, .15));
			Assert.IsFalse(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures that ORCs don't match
		/// when matching partial words.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLongestUsefulCommonSubstring_PreventOrcMatching_OneStringIsAllOrcsAndSpaces()
		{
			Assert.AreEqual(Empty, StringExtensions.GetLongestUsefulCommonSubstring(
				kObjReplacementChar + " " + kObjReplacementChar + " " + kObjReplacementChar +
				" " + kObjReplacementChar + " ",
				kObjReplacementChar + " " + kObjReplacementChar + " " + kObjReplacementChar +
				"a " + kObjReplacementChar + " ", out var fWholeWord, .15));
			Assert.IsFalse(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures that if words have
		/// letters with diacritics or other combining marks, those are not treated as word-
		/// breaking characters, but rather whole words are kept together.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(NormalizationForm.FormD)]
		[TestCase(NormalizationForm.FormC)]
		public void GetLongestUsefulCommonSubstring_LettersWithCombiningMarks_MarksTreatedAsWordFormingCharacters(
			NormalizationForm normalization)
		{
			Assert.AreEqual("me aborrece, porque yo testifico de",
				StringExtensions.GetLongestUsefulCommonSubstring(
					"mas a mi me aborrece, porque yo testifico de él, que sus obras son malas.".Normalize(normalization),
					"mas a mí me aborrece, porque yo testifico de el, que sus obras son malas.".Normalize(normalization),
				out var fWholeWord, .15));
			Assert.IsTrue(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures correct handling of
		/// surrogate pairs.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLongestUsefulCommonSubstring_SurrogatePairs_CorrectlyDeterminesWordFormingCharacters()
		{
			Assert.AreEqual(GetUtf16StringFromUtf32CodePoints(0x12368, 0x12361, 0x12362,
					0x12364, 0x12361, 0x12365, 0x12367, 0x12366, 0x12363, 0x12378),
				StringExtensions.GetLongestUsefulCommonSubstring(
					GetUtf16StringFromUtf32CodePoints(0x12368, 0x12361, 0x12362, 0x12364,
						0x12361, 0x12365, 0x12367, 0x12366, 0x12363, 0x12378,
						0x12341, 0x12362, 0x12344, 0x12371, 0x12355, 0x12367, 0x12366, 0x12333),
					GetUtf16StringFromUtf32CodePoints(0x12368, 0x12361, 0x12362, 0x12364,
						0x12361, 0x12365, 0x12367, 0x12366, 0x12363, 0x12378,
						0x12342, 0x12342, 0x12334, 0x12381, 0x12355, 0x12367, 0x12366, 0x12333),
					out var fWholeWord, .15));
			Assert.IsFalse(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures correct handling of
		/// non-word-forming surrogate pairs.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetLongestUsefulCommonSubstring_SurrogatePairsWithSymbolsAsWordBreaks_CorrectlyDeterminesWordFormingCharacters()
		{
			Assert.AreEqual(GetUtf16StringFromUtf32CodePoints(0x12368, 0x12361, 0x20, 0x12362, 0x12364,
					0x12361, 0x1F64F, 0x12365, 0x12367, 0x12366, 0x12363, 0x12378),
				StringExtensions.GetLongestUsefulCommonSubstring(
					GetUtf16StringFromUtf32CodePoints(0x12368, 0x12361, 0x20, 0x12362, 0x12364,
						0x12361, 0x1F64F, 0x12365, 0x12367, 0x12366, 0x12363, 0x12378, 0x1F030,
						0x12341, 0x12362, 0x12344, 0x12371, 0x12355, 0x12367, 0x12366, 0x12333),
					GetUtf16StringFromUtf32CodePoints(0x12368, 0x12361, 0x20, 0x12362, 0x12364,
						0x12361, 0x1F64F, 0x12365, 0x12367, 0x12366, 0x12363, 0x12378, 0x1F035,
						0x12342, 0x12342, 0x12334, 0x12381, 0x12355, 0x12367, 0x12366, 0x12333),
					out var fWholeWord, .15));
			Assert.IsTrue(fWholeWord);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the GetLongestUsefulCommonSubstring method. This test ensures that strings with
		/// surrogate pairs will not match on half of the pair.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase("\uD800\uDC00\ud803\ude6d\udbff\udfff", "\udbff\udfef", 0.15)]
		[TestCase("\uD800\uDC00\ud803\ude6d\udbff\udfff", "\udbff\udfef", 0.01)]
		[TestCase("\uD800\uDC00\ud803\ude6d\udbff\udfff", "\ud801\ude6d\udbff\udfef", 0.15)]
		[TestCase("\uD800\uDC00\ud803\ude6d\udbff\udfff", "\ud801\ude6d\udbff\udfef", 1D)]
		public void GetLongestUsefulCommonSubstring_SurrogatePairsWithNothingInCommon_ReturnsEmptyString(
			string a, string b, double minPctForPartialWordMatch)
		{
			Assert.AreEqual("", StringExtensions.GetLongestUsefulCommonSubstring(a, b,
				out var _, minPctForPartialWordMatch));
		}

		[TestCase("\u200D")] // Format: ZWJ
		[TestCase("s")] // Lowercase letter
		[TestCase("\u02C6")] // Modifier letter
		[TestCase("\u02C6")] // NonSpacingMark
		[TestCase("\u02C6")] // OtherLetter
		[TestCase("\u100000")] // PrivateUse
		[TestCase("\u094C")] // SpacingCombiningMa
		[TestCase("\u01C8")] // TitlecaseLetter:
		[TestCase("Q")] // Uppercase letter
		public void IsLikelyWordForming_WordFormingCharactersKnownToDotNet_ReturnsTrue(string s)
		{
			Assert.IsTrue(s.IsLikelyWordForming(0, false));
		}

		[TestCase(")")] // ClosePunctuation
		[TestCase("\u203F")] // ConnectorPunctuation
		[TestCase("\u0000")] // Control
		[TestCase("$")] // CurrencySymbol
		[TestCase("-")] // DashPunctuation
		[TestCase("5")] // DecimalDigitNumber
		[TestCase("\u20DD")] // EnclosingMark
		[TestCase("»")] // FinalQuotePunctuation
		[TestCase("\u201C")] // InitialQuotePunctuation
		[TestCase("\u2129")] // LetterNumber
		[TestCase("\u2028")] // LineSeparator
		[TestCase("+")] // MathSymbol
		[TestCase("\u0060")] // ModifierSymbol
		[TestCase("[")] // OpenPunctuation
		[TestCase("\u00B2")] // OtherNumber
		[TestCase("!")] // OtherPunctuation
		[TestCase("\u00B0")] // OtherSymbol
		[TestCase("\u2029")] // ParagraphSeparator
		[TestCase(" ")] // SpaceSeparator
		public void IsLikelyWordForming_OtherCharactersKnownToDotNet_ReturnsFalse(string s)
		{
			Assert.IsFalse(s.IsLikelyWordForming(0, false));
		}

		private const string kVietnameseAlternateReadingMark = "\u00016FF0";

		[Test]
		[Category("ByHand")]
		[Explicit("This test was written to demonstrate the need for an alternate implementation " +
			"for characters that are not known to .Net (see next test).")]
		public void IsLikelyWordForming_VietnameseAlternateReadingMarkCa_ReturnsTrue()
		{
			// We expect this to fail unless we upgrade to some future .Net version that knows
			// about this code point.
			Assert.IsTrue(kVietnameseAlternateReadingMark.IsLikelyWordForming(0, false));
		}

		[Test]
		public void IsLikelyWordForming_CharactersNotKnownToDotNetButHandledByAlternateImplementation_ReturnsTrue()
		{
			StringExtensions.AltImplGetUnicodeCategory = (str, index) =>
				str.Substring(index, kVietnameseAlternateReadingMark.Length) ==
				kVietnameseAlternateReadingMark ? UnicodeCategory.SpacingCombiningMark :
				CharUnicodeInfo.GetUnicodeCategory(str, index);

			Assert.IsTrue(kVietnameseAlternateReadingMark.IsLikelyWordForming(0, false));
		}

		private string GetUtf16StringFromUtf32CodePoints(params int[] codepoints) =>
			string.Join("", codepoints.Select(cp => Char.ConvertFromUtf32(cp)));
	}
}
