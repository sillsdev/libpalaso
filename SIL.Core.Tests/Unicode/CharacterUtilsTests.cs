using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SIL.Unicode;

namespace SIL.Tests.Unicode
{
	[TestFixture]
	class CharacterUtilsTests
	{
		[TestCase(UnicodeCategory.LowercaseLetter, 'a')]
		[TestCase(UnicodeCategory.UppercaseLetter, 'A')]
		[TestCase(UnicodeCategory.OpenPunctuation, '(')]
		[TestCase(UnicodeCategory.ClosePunctuation, '}')]
		public void GetAllCharactersInUnicodeCategory_CategoryShouldContainCharacter_CategoryContainsCharacter(UnicodeCategory category, char character)
		{
			Assert.True(CharacterUtils.GetAllCharactersInUnicodeCategory(category).Contains(character));
		}

		[TestCase(UnicodeCategory.LowercaseLetter, 'A')]
		[TestCase(UnicodeCategory.UppercaseLetter, 'a')]
		[TestCase(UnicodeCategory.ClosePunctuation, '(')]
		[TestCase(UnicodeCategory.OpenPunctuation, '}')]
		[TestCase(UnicodeCategory.OpenPunctuation, ')')]
		[TestCase(UnicodeCategory.ClosePunctuation, '{')]
		public void GetAllCharactersInUnicodeCategory_CategoryShouldNotContainCharacter_CategoryDoesNotContainCharacter(UnicodeCategory category, char character)
		{
			Assert.False(CharacterUtils.GetAllCharactersInUnicodeCategory(category).Contains(character));
		}


		// For completeness, this list should be kept in synch with the list and switch statement in CharacterUtils itself.

		// Note... while one might think that char.GetUnicodeCategory could tell you if a character was a sentence separator, this is not the case. 
		// This is because, for example, '.' can be used for various things (abbreviation, decimal point, as well as sentence terminator).
		// This should be a complete list of code points with the \p{Sentence_Break=STerm} or \p{Sentence_Break=ATerm} properties that also
		// have the \p{Terminal_Punctuation} property. This list is up-to-date as of Unicode v6.1.
		// ENHANCE: Ideally this should be dynamic.

		[TestCase('.')] // ROMAN FULL STOP
		[TestCase('?')] // ROMAN QUESTION MARK
		[TestCase('!')] // ROMAN EXCLAMATION MARK
		[TestCase('\u00A7')] // SECTION SIGN
		[TestCase('\u055C')] // ARMENIAN EXCLAMATION MARK
		[TestCase('\u055E')] // ARMENIAN QUESTION MARK
		[TestCase('\u0589')] // ARMENIAN FULL STOP
		[TestCase('\u061F')] // ARABIC QUESTION MARK
		[TestCase('\u06D4')] // ARABIC FULL STOP
		[TestCase('\u0700')] // SYRIAC END OF PARAGRAPH
		[TestCase('\u0701')] // SYRIAC SUPRALINEAR FULL STOP
		[TestCase('\u0702')] // SYRIAC SUBLINEAR FULL STOP
		[TestCase('\u07F9')] // NKO EXCLAMATION MARK
		[TestCase('\u0964')] // DEVANAGARI DANDA
		[TestCase('\u0965')] // DEVANAGARI DOUBLE DANDA
		[TestCase('\u104A')] // MYANMAR SIGN LITTLE SECTION
		[TestCase('\u104B')] // MYANMAR SIGN SECTION
		[TestCase('\u1362')] // ETHIOPIC FULL STOP
		[TestCase('\u1367')] // ETHIOPIC QUESTION MARK
		[TestCase('\u1368')] // ETHIOPIC PARAGRAPH SEPARATOR
		[TestCase('\u166E')] // CANADIAN SYLLABICS FULL STOP
		[TestCase('\u17D4')] // KHMER SIGN KHAN
		[TestCase('\u1803')] // MONGOLIAN FULL STOP
		[TestCase('\u1809')] // MONGOLIAN MANCHU FULL STOP
		[TestCase('\u1944')] // LIMBU EXCLAMATION MARK
		[TestCase('\u1945')] // LIMBU QUESTION MARK
		[TestCase('\u1AA8')] // TAI THAM SIGN KAAN
		[TestCase('\u1AA9')] // TAI THAM SIGN KAANKUU
		[TestCase('\u1AAA')] // TAI THAM SIGN SATKAAN
		[TestCase('\u1AAB')] // TAI THAM SIGN SATKAANKUU
		[TestCase('\u1B5A')] // BALINESE PANTI
		[TestCase('\u1B5B')] // BALINESE PAMADA
		[TestCase('\u1B5E')] // BALINESE CARIK SIKI
		[TestCase('\u1B5F')] // BALINESE CARIK PAREREN
		[TestCase('\u1C3B')] // LEPCHA PUNCTUATION TA-ROL
		[TestCase('\u1C3C')] // LEPCHA PUNCTUATION NYET THYOOM TA-ROL
		[TestCase('\u1C7E')] // OL CHIKI PUNCTUATION MUCAAD
		[TestCase('\u1C7F')] // OL CHIKI PUNCTUATION DOUBLE MUCAAD
		[TestCase('\u203C')] // DOUBLE EXCLAMATION MARK
		[TestCase('\u203D')] // INTERROBANG
		[TestCase('\u2047')] // DOUBLE QUESTION MARK
		[TestCase('\u2048')] // QUESTION EXCLAMATION MARK
		[TestCase('\u2049')] // EXCLAMATION QUESTION MARK
		[TestCase('\u2CF9')] // COPTIC OLD NUBIAN FULL STOP
		[TestCase('\u2CFA')] // COPTIC OLD NUBIAN DIRECT QUESTION MARK
		[TestCase('\u2CFB')] // COPTIC OLD NUBIAN INDIRECT QUESTION MARK
		[TestCase('\u2CFE')] // COPTIC FULL STOP
		[TestCase('\u2E2E')] // REVERSED QUESTION MARK
		[TestCase('\u2E3C')] // STENOGRAPHIC FULL STOP
		[TestCase('\u3002')] // IDEOGRAPHIC FULL STOP
		[TestCase('\uA4FF')] // LISU PUNCTUATION FULL STOP
		[TestCase('\uA60E')] // VAI FULL STOP
		[TestCase('\uA60F')] // VAI QUESTION MARK
		[TestCase('\uA6F3')] // BAMUM FULL STOP
		[TestCase('\uA6F7')] // BAMUM QUESTION MARK
		[TestCase('\uA876')] // PHAGS-PA MARK SHAD
		[TestCase('\uA877')] // PHAGS-PA MARK DOUBLE SHAD
		[TestCase('\uA8CE')] // SAURASHTRA DANDA
		[TestCase('\uA8CF')] // SAURASHTRA DOUBLE DANDA
		[TestCase('\uA92F')] // KAYAH LI SIGN SHYA
		[TestCase('\uA9C8')] // JAVANESE PADA LINGSA
		[TestCase('\uA9C9')] // JAVANESE PADA LUNGSI
		[TestCase('\uAA5D')] // CHAM PUNCTUATION DANDA
		[TestCase('\uAA5E')] // CHAM PUNCTUATION DOUBLE DANDA
		[TestCase('\uAA5F')] // CHAM PUNCTUATION TRIPLE DANDA
		[TestCase('\uAAF0')] // MEETEI MAYEK CHEIKHAN
		[TestCase('\uAAF1')] // MEETEI MAYEK AHANG KHUDAM
		[TestCase('\uABEB')] // MEETEI MAYEK CHEIKHEI
		[TestCase('\uFE52')] // SMALL FULL STOP
		[TestCase('\uFE56')] // SMALL QUESTION MARK
		[TestCase('\uFE57')] // SMALL EXCLAMATION MARK
		[TestCase('\uFF01')] // FULLWIDTH EXCLAMATION MARK
		[TestCase('\uFF0E')] // FULLWIDTH FULL STOP
		[TestCase('\uFF1F')] // FULLWIDTH QUESTION MARK
		[TestCase('\uFF61')] // HALFWIDTH IDEOGRAPHIC FULL STOP
		public void SentenceFinalPunctuation_ContainsExpectedCharacters(char c)
		{
			Assert.IsTrue(CharacterUtils.SentenceFinalPunctuation.Contains(c));
		}

		[Test]
		public void IsSentenceFinalPunctuation_AllSentenceFinalPunctuation_ReturnsTrue()
		{
			foreach (var c in CharacterUtils.SentenceFinalPunctuation)
				Assert.IsTrue(CharacterUtils.IsSentenceFinalPunctuation(c));

			Assert.AreEqual(75, CharacterUtils.SentenceFinalPunctuation.Length,
				"If sentence-ending characters are added (or removed) in production, ensure that the test cases in the " +
				"above test match the production code. (Then this number should be changed to match.)");
		}

		[TestCase(',')]
		[TestCase('1')]
		[TestCase('#')]
		[TestCase('$')]
		[TestCase('(')]
		[TestCase('"')]
		[TestCase(')')]
		[TestCase('/')]
		[TestCase('|')]
		[TestCase(':')]
		[TestCase(';')]
		[TestCase('\uFEFF')]
		[TestCase('\uFFFF')]
		public void IsSentenceFinalPunctuation_CharacterIsNotSentenceFinalPunctuation_ReturnsFalse(char c)
		{
			Assert.IsFalse(CharacterUtils.IsSentenceFinalPunctuation(c));
		}

		[TestCase(',')]
		[TestCase('1')]
		[TestCase('#')]
		[TestCase('$')]
		[TestCase('(')]
		[TestCase('"')]
		[TestCase(')')]
		[TestCase('/')]
		[TestCase('|')]
		[TestCase(':')]
		[TestCase(';')]
		[TestCase('\uFEFF')]
		[TestCase('\uFFFF')]
		public void SentenceFinalPunctuation_DoesNotContainUnexpectedCharacters(char c)
		{
			Assert.IsFalse(CharacterUtils.SentenceFinalPunctuation.Contains(c));
		}

		[Test]
		public void SentenceFinalPunctuation_LocalArrayModified_SentenceFinalPunctuationNotChanged()
		{
			var array = CharacterUtils.SentenceFinalPunctuation;
			var originalFirstElement = array[0];
			array[0] = '$';
			Assert.AreEqual(originalFirstElement, CharacterUtils.SentenceFinalPunctuation[0]);
		}

		[TestCase("This is a short sentence.")]
		[TestCase("This is a short sentence. ")]
		[TestCase("This is a short sentence.\r\n")]
		[TestCase("This is a short sentence.\n")]
		[TestCase("This is a short sentence.' \" ")]
		[TestCase("This is a short sentence.' \"\r\n")]
		[TestCase("This is a short sentence.' \"\n")]
		[TestCase("This is a short sentence.\" ")]
		[TestCase("This is a short sentence.\"\r\n")]
		[TestCase("This is a short sentence.\"\n")]
		[TestCase("This is a short sentence.»\r\n")]
		[TestCase("This is a short sentence.»\n")]
		[Test]
		public void EndsWithSentenceFinalPunctuation_TextEndsWithSFP_ReturnsTrue(string text)
		{
			var actual = CharacterUtils.EndsWithSentenceFinalPunctuation(text);
			Assert.IsTrue(actual);
		}

		[TestCase("not a sentence")]
		[TestCase("This is a,")]
		[TestCase("This is a, ")]
		[TestCase("This is a,\r\n")]
		[TestCase("This is a short sentence.\nThis is a")]
		[TestCase("This is a \"\n")]
		[TestCase("This is a,»\r\n")]
		[TestCase("This is a»\n")]
		[Test]
		public void EndsWithSentenceFinalPunctuation_TextDoesNotEndWithSFP_ReturnsFalse(string text)
		{
			var actual = CharacterUtils.EndsWithSentenceFinalPunctuation(text);
			Assert.IsFalse(actual);
		}

		//[Test]
		//public void SpeedTest()
		//{
		//	var sw = new Stopwatch();
		//	sw.Start();
		//	for (int i = 0; i < 10000; i++)
		//	{
		//		CharacterUtils.IsSentenceFinalPunctuation('.'); // ROMAN FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('?'); // ROMAN QUESTION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('!'); // ROMAN EXCLAMATION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\u0589'); // ARMENIAN FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\u061F'); // ARABIC QUESTION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\u06D4'); // ARABIC FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\u0700'); // SYRIAC END OF PARAGRAPH
		//		CharacterUtils.IsSentenceFinalPunctuation('\u0701'); // SYRIAC SUPRALINEAR FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\u0702'); // SYRIAC SUBLINEAR FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\u07F9'); // NKO EXCLAMATION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\u0964'); // DEVANAGARI DANDA
		//		CharacterUtils.IsSentenceFinalPunctuation('\u0965'); // DEVANAGARI DOUBLE DANDA
		//		CharacterUtils.IsSentenceFinalPunctuation('\u104A'); // MYANMAR SIGN LITTLE SECTION
		//		CharacterUtils.IsSentenceFinalPunctuation('\u104B'); // MYANMAR SIGN SECTION
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1362'); // ETHIOPIC FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1367'); // ETHIOPIC QUESTION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1368'); // ETHIOPIC PARAGRAPH SEPARATOR
		//		CharacterUtils.IsSentenceFinalPunctuation('\u166E'); // CANADIAN SYLLABICS FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1803'); // MONGOLIAN FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1809'); // MONGOLIAN MANCHU FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1944'); // LIMBU EXCLAMATION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1945'); // LIMBU QUESTION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1AA8'); // TAI THAM SIGN KAAN
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1AA9'); // TAI THAM SIGN KAANKUU
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1AAA'); // TAI THAM SIGN SATKAAN
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1AAB'); // TAI THAM SIGN SATKAANKUU
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1B5A'); // BALINESE PANTI
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1B5B'); // BALINESE PAMADA
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1B5E'); // BALINESE CARIK SIKI
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1B5F'); // BALINESE CARIK PAREREN
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1C3B'); // LEPCHA PUNCTUATION TA-ROL
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1C3C'); // LEPCHA PUNCTUATION NYET THYOOM TA-ROL
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1C7E'); // OL CHIKI PUNCTUATION MUCAAD
		//		CharacterUtils.IsSentenceFinalPunctuation('\u1C7F'); // OL CHIKI PUNCTUATION DOUBLE MUCAAD
		//		CharacterUtils.IsSentenceFinalPunctuation('\u203C'); // DOUBLE EXCLAMATION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\u203D'); // INTERROBANG
		//		CharacterUtils.IsSentenceFinalPunctuation('\u2047'); // DOUBLE QUESTION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\u2048'); // QUESTION EXCLAMATION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\u2049'); // EXCLAMATION QUESTION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\u2E2E'); // REVERSED QUESTION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\u3002'); // IDEOGRAPHIC FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA4FF'); // LISU PUNCTUATION FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA60E'); // VAI FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA60F'); // VAI QUESTION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA6F3'); // BAMUM FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA6F7'); // BAMUM QUESTION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA876'); // PHAGS-PA MARK SHAD
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA877'); // PHAGS-PA MARK DOUBLE SHAD
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA8CE'); // SAURASHTRA DANDA
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA8CF'); // SAURASHTRA DOUBLE DANDA
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA92F'); // KAYAH LI SIGN SHYA
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA9C8'); // JAVANESE PADA LINGSA
		//		CharacterUtils.IsSentenceFinalPunctuation('\uA9C9'); // JAVANESE PADA LUNGSI
		//		CharacterUtils.IsSentenceFinalPunctuation('\uAA5D'); // CHAM PUNCTUATION DANDA
		//		CharacterUtils.IsSentenceFinalPunctuation('\uAA5E'); // CHAM PUNCTUATION DOUBLE DANDA
		//		CharacterUtils.IsSentenceFinalPunctuation('\uAA5F'); // CHAM PUNCTUATION TRIPLE DANDA
		//		CharacterUtils.IsSentenceFinalPunctuation('\uAAF0'); // MEETEI MAYEK CHEIKHAN
		//		CharacterUtils.IsSentenceFinalPunctuation('\uAAF1'); // MEETEI MAYEK AHANG KHUDAM
		//		CharacterUtils.IsSentenceFinalPunctuation('\uABEB'); // MEETEI MAYEK CHEIKHEI
		//		CharacterUtils.IsSentenceFinalPunctuation('\uFE52'); // SMALL FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\uFE56'); // SMALL QUESTION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\uFE57'); // SMALL EXCLAMATION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\uFF01'); // FULLWIDTH EXCLAMATION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\uFF0E'); // FULLWIDTH FULL STOP
		//		CharacterUtils.IsSentenceFinalPunctuation('\uFF1F'); // FULLWIDTH QUESTION MARK
		//		CharacterUtils.IsSentenceFinalPunctuation('\uFF61'); // HALFWIDTH IDEOGRAPHIC FULL STOP
		//	}
		//	sw.Stop();
		//	Console.WriteLine("Elapsed milliseconds: " + (sw.ElapsedTicks * 1000.0f / Stopwatch.Frequency));
		//}
	}
}
