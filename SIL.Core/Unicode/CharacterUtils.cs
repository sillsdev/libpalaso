using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SIL.Unicode
{
	public static class CharacterUtils
	{
		private static readonly Dictionary<UnicodeCategory, IGrouping<UnicodeCategory, char>> CharInfo;
		private static Regex _sentenceEndingRegex;

		// This list MUST be kept in synch with the cases in the switch statement below.
		// To optimize performance (switch is twice as fast as a HashSet lookup), this list
		// is not used to implement IsSentenceFinalPunctuation.
		private static readonly char[] SentenceFinalCharactersArray = { '.', '?', '!',
				'\u00A7', // SECTION SIGN
				'\u055C', // ARMENIAN EXCLAMATION MARK
				'\u055E', // ARMENIAN QUESTION MARK
				'\u0589', // ARMENIAN FULL STOP
				'\u061F', // ARABIC QUESTION MARK
				'\u06D4', // ARABIC FULL STOP
				'\u0700', // SYRIAC END OF PARAGRAPH
				'\u0701', // SYRIAC SUPRALINEAR FULL STOP
				'\u0702', // SYRIAC SUBLINEAR FULL STOP
				'\u07F9', // NKO EXCLAMATION MARK
				'\u0964', // DEVANAGARI DANDA
				'\u0965', // DEVANAGARI DOUBLE DANDA
				'\u104A', // MYANMAR SIGN LITTLE SECTION
				'\u104B', // MYANMAR SIGN SECTION
				'\u1362', // ETHIOPIC FULL STOP
				'\u1367', // ETHIOPIC QUESTION MARK
				'\u1368', // ETHIOPIC PARAGRAPH SEPARATOR
				'\u166E', // CANADIAN SYLLABICS FULL STOP
				'\u17D4', // KHMER SIGN KHAN (Comments: functions as a full stop, period)
				'\u1803', // MONGOLIAN FULL STOP
				'\u1809', // MONGOLIAN MANCHU FULL STOP
				'\u1944', // LIMBU EXCLAMATION MARK
				'\u1945', // LIMBU QUESTION MARK
				'\u1AA8', // TAI THAM SIGN KAAN
				'\u1AA9', // TAI THAM SIGN KAANKUU
				'\u1AAA', // TAI THAM SIGN SATKAAN
				'\u1AAB', // TAI THAM SIGN SATKAANKUU
				'\u1B5A', // BALINESE PANTI
				'\u1B5B', // BALINESE PAMADA
				'\u1B5E', // BALINESE CARIK SIKI
				'\u1B5F', // BALINESE CARIK PAREREN
				'\u1C3B', // LEPCHA PUNCTUATION TA-ROL
				'\u1C3C', // LEPCHA PUNCTUATION NYET THYOOM TA-ROL
				'\u1C7E', // OL CHIKI PUNCTUATION MUCAAD
				'\u1C7F', // OL CHIKI PUNCTUATION DOUBLE MUCAAD
				'\u203C', // DOUBLE EXCLAMATION MARK
				'\u203D', // INTERROBANG
				'\u2047', // DOUBLE QUESTION MARK
				'\u2048', // QUESTION EXCLAMATION MARK
				'\u2049', // EXCLAMATION QUESTION MARK
				'\u2CF9', // COPTIC OLD NUBIAN FULL STOP
				'\u2CFA', // COPTIC OLD NUBIAN DIRECT QUESTION MARK
				'\u2CFB', // COPTIC OLD NUBIAN INDIRECT QUESTION MARK
				'\u2CFE', // COPTIC FULL STOP
				'\u2E2E', // REVERSED QUESTION MARK
				'\u2E3C', // STENOGRAPHIC FULL STOP
				'\u3002', // IDEOGRAPHIC FULL STOP
				'\uA4FF', // LISU PUNCTUATION FULL STOP
				'\uA60E', // VAI FULL STOP
				'\uA60F', // VAI QUESTION MARK
				'\uA6F3', // BAMUM FULL STOP
				'\uA6F7', // BAMUM QUESTION MARK
				'\uA876', // PHAGS-PA MARK SHAD
				'\uA877', // PHAGS-PA MARK DOUBLE SHAD
				'\uA8CE', // SAURASHTRA DANDA
				'\uA8CF', // SAURASHTRA DOUBLE DANDA
				'\uA92F', // KAYAH LI SIGN SHYA
				'\uA9C8', // JAVANESE PADA LINGSA
				'\uA9C9', // JAVANESE PADA LUNGSI
				'\uAA5D', // CHAM PUNCTUATION DANDA
				'\uAA5E', // CHAM PUNCTUATION DOUBLE DANDA
				'\uAA5F', // CHAM PUNCTUATION TRIPLE DANDA
				'\uAAF0', // MEETEI MAYEK CHEIKHAN
				'\uAAF1', // MEETEI MAYEK AHANG KHUDAM
				'\uABEB', // MEETEI MAYEK CHEIKHEI
				'\uFE52', // SMALL FULL STOP
				'\uFE56', // SMALL QUESTION MARK
				'\uFE57', // SMALL EXCLAMATION MARK
				'\uFF01', // FULLWIDTH EXCLAMATION MARK
				'\uFF0E', // FULLWIDTH FULL STOP
				'\uFF1F', // FULLWIDTH QUESTION MARK
				'\uFF61', // HALFWIDTH IDEOGRAPHIC FULL STOP
				// These would require surrogate pairs
				//'\u11047', // BRAHMI DANDA
				//'\u11048', // BRAHMI DOUBLE DANDA
				//'\u110BE', // KAITHI SECTION MARK
				//'\u110BF', // KAITHI DOUBLE SECTION MARK
				//'\u110C0', // KAITHI DANDA
				//'\u110C1', // KAITHI DOUBLE DANDA
				//'\u11141', // CHAKMA DANDA
				//'\u11142', // CHAKMA DOUBLE DANDA
				//'\u11143', // CHAKMA QUESTION MARK
				//'\u111C5', // SHARADA DANDA
				//'\u111C6', // SHARADA DOUBLE DANDA
			};

		// Unambiguous Paragraph Ending Punctuation
		// Source: http://www.unicode.org/reports/tr29
		private static readonly char[] ParagraphEndingCharactersArray =
		{
			'\r',     // Carriage Return
			'\n',     // Line Feed
			'\u0085'  // Next Line
		};

		static CharacterUtils()
		{
			CharInfo = Enumerable.Range(0, 0x110000)
				.Where(x => x < 0x00d800 || x > 0x00dfff)
				.Select(x => char.ConvertFromUtf32(x)[0])
				.GroupBy(char.GetUnicodeCategory)
				.ToDictionary(g => g.Key);
		}

		public static IEnumerable<char> GetAllCharactersInUnicodeCategory(UnicodeCategory category)
		{
			return CharInfo[category];
		}

		public static char[] SentenceFinalPunctuation
		{
			get { return SentenceFinalCharactersArray.ToArray(); }
		}

		public static bool IsSentenceFinalPunctuation(char c)
		{
			// The cases in this switch statement MUST be kept in synch with the list above.
			switch (c)
			{
				case '.':
				case '?':
				case '!':
				case '\u00A7': // SECTION SIGN
				case '\u055C': // ARMENIAN EXCLAMATION MARK
				case '\u055E': // ARMENIAN QUESTION MARK
				case '\u0589': // ARMENIAN FULL STOP
				case '\u061F': // ARABIC QUESTION MARK
				case '\u06D4': // ARABIC FULL STOP
				case '\u0700': // SYRIAC END OF PARAGRAPH
				case '\u0701': // SYRIAC SUPRALINEAR FULL STOP
				case '\u0702': // SYRIAC SUBLINEAR FULL STOP
				case '\u07F9': // NKO EXCLAMATION MARK
				case '\u0964': // DEVANAGARI DANDA
				case '\u0965': // DEVANAGARI DOUBLE DANDA
				case '\u104A': // MYANMAR SIGN LITTLE SECTION
				case '\u104B': // MYANMAR SIGN SECTION
				case '\u1362': // ETHIOPIC FULL STOP
				case '\u1367': // ETHIOPIC QUESTION MARK
				case '\u1368': // ETHIOPIC PARAGRAPH SEPARATOR
				case '\u166E': // CANADIAN SYLLABICS FULL STOP
				case '\u17D4': // KHMER SIGN KHAN
				case '\u1803': // MONGOLIAN FULL STOP
				case '\u1809': // MONGOLIAN MANCHU FULL STOP
				case '\u1944': // LIMBU EXCLAMATION MARK
				case '\u1945': // LIMBU QUESTION MARK
				case '\u1AA8': // TAI THAM SIGN KAAN
				case '\u1AA9': // TAI THAM SIGN KAANKUU
				case '\u1AAA': // TAI THAM SIGN SATKAAN
				case '\u1AAB': // TAI THAM SIGN SATKAANKUU
				case '\u1B5A': // BALINESE PANTI
				case '\u1B5B': // BALINESE PAMADA
				case '\u1B5E': // BALINESE CARIK SIKI
				case '\u1B5F': // BALINESE CARIK PAREREN
				case '\u1C3B': // LEPCHA PUNCTUATION TA-ROL
				case '\u1C3C': // LEPCHA PUNCTUATION NYET THYOOM TA-ROL
				case '\u1C7E': // OL CHIKI PUNCTUATION MUCAAD
				case '\u1C7F': // OL CHIKI PUNCTUATION DOUBLE MUCAAD
				case '\u203C': // DOUBLE EXCLAMATION MARK
				case '\u203D': // INTERROBANG
				case '\u2047': // DOUBLE QUESTION MARK
				case '\u2048': // QUESTION EXCLAMATION MARK
				case '\u2049': // EXCLAMATION QUESTION MARK
				case '\u2CF9': // COPTIC OLD NUBIAN FULL STOP
				case '\u2CFA': // COPTIC OLD NUBIAN DIRECT QUESTION MARK
				case '\u2CFB': // COPTIC OLD NUBIAN INDIRECT QUESTION MARK
				case '\u2CFE': // COPTIC FULL STOP
				case '\u2E2E': // REVERSED QUESTION MARK
				case '\u2E3C': // STENOGRAPHIC FULL STOP
				case '\u3002': // IDEOGRAPHIC FULL STOP
				case '\uA4FF': // LISU PUNCTUATION FULL STOP
				case '\uA60E': // VAI FULL STOP
				case '\uA60F': // VAI QUESTION MARK
				case '\uA6F3': // BAMUM FULL STOP
				case '\uA6F7': // BAMUM QUESTION MARK
				case '\uA876': // PHAGS-PA MARK SHAD
				case '\uA877': // PHAGS-PA MARK DOUBLE SHAD
				case '\uA8CE': // SAURASHTRA DANDA
				case '\uA8CF': // SAURASHTRA DOUBLE DANDA
				case '\uA92F': // KAYAH LI SIGN SHYA
				case '\uA9C8': // JAVANESE PADA LINGSA
				case '\uA9C9': // JAVANESE PADA LUNGSI
				case '\uAA5D': // CHAM PUNCTUATION DANDA
				case '\uAA5E': // CHAM PUNCTUATION DOUBLE DANDA
				case '\uAA5F': // CHAM PUNCTUATION TRIPLE DANDA
				case '\uAAF0': // MEETEI MAYEK CHEIKHAN
				case '\uAAF1': // MEETEI MAYEK AHANG KHUDAM
				case '\uABEB': // MEETEI MAYEK CHEIKHEI
				case '\uFE52': // SMALL FULL STOP
				case '\uFE56': // SMALL QUESTION MARK
				case '\uFE57': // SMALL EXCLAMATION MARK
				case '\uFF01': // FULLWIDTH EXCLAMATION MARK
				case '\uFF0E': // FULLWIDTH FULL STOP
				case '\uFF1F': // FULLWIDTH QUESTION MARK
				case '\uFF61': // HALFWIDTH IDEOGRAPHIC FULL STOP
				// These would require surrogate pairs
				//'\u11047', // BRAHMI DANDA
				//'\u11048', // BRAHMI DOUBLE DANDA
				//'\u110BE', // KAITHI SECTION MARK
				//'\u110BF', // KAITHI DOUBLE SECTION MARK
				//'\u110C0', // KAITHI DANDA
				//'\u110C1', // KAITHI DOUBLE DANDA
				//'\u11141', // CHAKMA DANDA
				//'\u11142', // CHAKMA DOUBLE DANDA
				//'\u11143', // CHAKMA QUESTION MARK
				//'\u111C5', // SHARADA DANDA
				//'\u111C6', // SHARADA DOUBLE DANDA
					return true;
				default:
					return false;
			}
		}

		public static bool EndsWithSentenceFinalPunctuation(string text)
		{
			if (_sentenceEndingRegex == null)
			{
				// One or more of the sentence ending punctuation characters is at or near the end of the string
				var sentenceEndingChars = "[" + new string(SentenceFinalPunctuation) + "]+";

				// Zero or more non-word-forming characters that can follow the sentence ending punctuation
				// Pf => Punctuation, Final quote
				// Pe => Punctuation, Close
				var sentenceTrailingChars = "[\\s\'\"\\p{Pf}\\p{Pe}]*";

				// Zero or more unambiguous paragraph ending characters
				// Zl => Separator, Line
				// Zp => Separator, Paragraph
				var paragraphEndingChars = "[" + new string(ParagraphEndingCharactersArray.ToArray()) + "\\p{Zl}\\p{Zp}]*";

				var regexPattern = sentenceEndingChars + sentenceTrailingChars + paragraphEndingChars + "$";

				_sentenceEndingRegex = new Regex(regexPattern);
			}

			var match = _sentenceEndingRegex.Match(text);

			return match.Success;
		}

		/// <summary>
		/// Return true for ASCII, Latin-1, Latin Ext. A, Latin Ext. B, IPA Extensions, and Spacing Modifier Letters.
		/// </summary>
		public static bool IsLatinChar(char test) => test <= 0x02FF;
	}
}
