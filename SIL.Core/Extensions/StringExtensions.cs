using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using static System.Char;

namespace SIL.Extensions
{
	public static class StringExtensions
	{
		public const char kObjReplacementChar = '\uFFFC';

		public static List<string> SplitTrimmed(this string s, char separator)
		{
			if (s.Trim() == string.Empty)
				return new List<string>();

			var x = s.Split(separator);

			var r = new List<string>();

			foreach (var part in x)
			{
				var trim = part.Trim();
				if (trim != string.Empty)
				{
					r.Add(trim);
				}
			}
			return r;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets an int array from a comma-delimited string of numbers.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static int[] ToIntArray(this string str)
		{
			List<int> array = new List<int>();

			if (str != null)
			{
				string[] pieces = str.Split(',');
				foreach (string piece in pieces)
				{
					int i;
					if (int.TryParse(piece, out i))
						array.Add(i);
				}
			}

			return array.ToArray();
		}

		/// <summary>
		/// normal string.format will throw if it can't do the format; this is dangerous if you're, for example
		/// just logging stuff that might contain messed up strings (myWorkSafe paths)
		/// </summary>
		public static string FormatWithErrorStringInsteadOfException(this string format, params object[] args)
		{
			try
			{
				if (args.Length == 0)
					return format;
				else
					return string.Format(format, args);
			}
			catch (Exception e)
			{
				string argList = "";
				foreach (var arg in args)
				{
					argList = argList + arg + ",";
				}
				argList = argList.Trim(new char[] {','});
				return "FormatWithErrorStringInsteadOfException(" + format + "," + argList + ") Exception: " + e.Message;
			}
		}

		public static string EscapeAnyUnicodeCharactersIllegalInXml(this string text)
		{
			//we actually want to preserve html markup, just escape the disallowed unicode characters
			text = text.Replace("<", "_lt;");
			text = text.Replace(">", "_gt;");
			text = text.Replace("&", "_amp;");
			text = text.Replace("\"", "_quot;");
			text = text.Replace("'", "_apos;");

			text = EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml(text);
			//put it back, now
			text = text.Replace("_lt;", "<");
			text = text.Replace("_gt;", ">");
			text = text.Replace("_amp;", "&");
			text = text.Replace("_quot;", "\"");
			text = text.Replace("_apos;", "'");

			// Also ensure NFC form for XML output.
			return text.Normalize(NormalizationForm.FormC);
		}

		private static object _lockUsedForEscaping = new object(); 
		private static StringBuilder _bldrUsedForEscaping;
		private static XmlWriterSettings _settingsUsedForEscaping;
		private static XmlWriter _writerUsedForEscaping;

		public static string EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml(this string text)
		{
			lock (_lockUsedForEscaping)
			{
				if (_bldrUsedForEscaping == null)
					_bldrUsedForEscaping = new StringBuilder();
				else
					_bldrUsedForEscaping.Clear();
				if (_settingsUsedForEscaping == null)
				{
					_settingsUsedForEscaping = new XmlWriterSettings();
					_settingsUsedForEscaping.NewLineHandling = NewLineHandling.None;		// don't fiddle with newlines
					_settingsUsedForEscaping.ConformanceLevel = ConformanceLevel.Fragment;	// allow just text by itself
					_settingsUsedForEscaping.CheckCharacters = false;						// allow invalid characters in
				}
				if (_writerUsedForEscaping == null)
					_writerUsedForEscaping = XmlWriter.Create(_bldrUsedForEscaping, _settingsUsedForEscaping);

				_writerUsedForEscaping.WriteString(text);
				_writerUsedForEscaping.Flush();
				return _bldrUsedForEscaping.ToString();
			}
		}

		/// <summary>
		/// Similar to Path.Combine, but it combines as may parts as you have into a single, platform-appropriate path.
		/// </summary>
		/// <example> string path = "my".Combine("stuff", "toys", "ball.txt")</example>
		public static string CombineForPath(this string rootPath, params string[] partsOfThePath)
		{
			string result = rootPath.ToString();
			foreach (var s in partsOfThePath)
			{
				result = Path.Combine(result, s);
			}
			return result;
		}

		/// <summary>
		/// Finds and replaces invalid characters in a filename
		/// </summary>
		/// <param name="input">the string to clean</param>
		/// <param name="errorChar">the character which replaces bad characters</param>
		/// <param name="replaceNbsp">Flag indicating whether to replace non-breaking spaces with
		/// normal spaces</param>
		public static string SanitizeFilename(this string input, char errorChar, bool replaceNbsp)
		{
			var invalidFilenameChars = System.IO.Path.GetInvalidFileNameChars();
			Array.Sort(invalidFilenameChars);
			return Sanitize(input, invalidFilenameChars, errorChar, replaceNbsp, false);
		}

		/// <summary>
		/// Finds and replaces invalid characters in a filename
		/// </summary>
		/// <param name="input">the string to clean</param>
		/// <param name="errorChar">the character which replaces bad characters</param>
		public static string SanitizeFilename(this string input, char errorChar) =>
			SanitizeFilename(input, errorChar, false);

		/// <summary>
		/// Finds and replaces invalid characters in a path
		/// </summary>
		/// <param name="input">the string to clean</param>
		/// <param name="errorChar">the character which replaces bad characters</param>
		/// normal spaces</param>

		public static string SanitizePath(this string input, char errorChar)
		{
			var invalidPathChars = System.IO.Path.GetInvalidPathChars();
			Array.Sort(invalidPathChars);
			return Sanitize(input, invalidPathChars, errorChar, false, true);
		}

		private static string Sanitize(string input, char[] invalidChars, char errorChar, bool replaceNbsp,
			bool allowTrailingUpHierarchyDots)
		{
			// Caller ensures invalidChars is sorted, so we can use a binary search, which should be lightning fast.

			// null always sanitizes to null
			if (input == null)
			{
				return null;
			}

			if (Array.BinarySearch(invalidChars, errorChar) >= 0)
				throw new ArgumentException("", nameof(errorChar));

			var result = new StringBuilder();

			foreach (var characterToTest in input)
			{
				if (Array.BinarySearch(invalidChars, characterToTest) >= 0 || characterToTest < ' ') // eliminate all control characters too
				{
					if (result.Length > 0 || errorChar != ' ')
						result.Append(errorChar);
				}
				else if (result.Length > 0 || !characterToTest.IsInvalidFilenameLeadingOrTrailingSpaceChar())
				{
					result.Append((replaceNbsp && characterToTest == '\u00A0') ? ' ' : characterToTest);
				}
			}

			var lastCharPos = result.Length - 1;
			while (lastCharPos >= 0)
			{
				var lastChar = result[lastCharPos];
				if ((lastChar == '.' &&
					(!allowTrailingUpHierarchyDots || lastCharPos < 2 || result[lastCharPos - 1] != '.' ||
					(result[lastCharPos - 2] != Path.DirectorySeparatorChar &&
					result[lastCharPos - 2] != Path.AltDirectorySeparatorChar)))
					|| lastChar.IsInvalidFilenameLeadingOrTrailingSpaceChar())
				{
					result.Remove(lastCharPos, 1);
					lastCharPos--;
				}
				else
					break;
			}

			return result.Length == 0 ? errorChar.ToString() : result.ToString();
		}

		/// <summary>
		/// In addition to all the "normal" space characters covered by Char.IsWhitespace, this
		/// also returns true for certain formatting characters which have no visible effect on
		/// a string when used as a leading or trailing character and would likely be confusing
		/// if allowed at the start or end of a filename. 
		/// </summary>
		/// <remarks>Originally, I implemented the second check as
		/// GetUnicodeCategory(c) == UnicodeCategory.Format, but this seemed to be too aggressive
		/// since significant formatting marks such as those used to control bidi text could be
		/// removed.
		/// </remarks>
		private static bool IsInvalidFilenameLeadingOrTrailingSpaceChar(this char c) =>
			IsWhiteSpace(c) || c == '\uFEFF' || c == '\u200B' || c == '\u200C' || c == '\u200D';

		/// <summary>
		/// Make the first letter of the string upper-case, and the rest lower case. Does not consider words.
		/// </summary>
		public static string ToUpperFirstLetter(this string source)
		{
			if (string.IsNullOrEmpty(source))
				return string.Empty;
			var letters = source.ToLowerInvariant().ToCharArray();
			letters[0] = ToUpperInvariant(letters[0]);
			return new string(letters);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Removes the ampersand accelerator prefix from the specified text.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string RemoveAcceleratorPrefix(this string text)
		{
			text = text.Replace("&&", kObjReplacementChar.ToString(CultureInfo.InvariantCulture));
			text = text.Replace("&", string.Empty);
			return text.Replace(kObjReplacementChar.ToString(CultureInfo.InvariantCulture), "&");
		}

		/// <summary>
		/// Trims the string, and returns null if the result is zero length
		/// </summary>
		/// <param name="value"></param>
		/// <param name="trimChars"></param>
		/// <returns></returns>
		public static string NullTrim(this string value, char[] trimChars)
		{
			if (string.IsNullOrEmpty(value))
				return null;

			value = value.Trim(trimChars);
			return string.IsNullOrEmpty(value) ? null : value;
		}

		/// <summary>
		/// Trims the string, and returns null if the result is zero length
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string NullTrim(this string value)
		{
			return value.NullTrim(null);
		}

		/// <summary>
		/// Determines whether the string contains the specified string using the specified comparison.
		/// </summary>
		public static bool Contains(this String stringToSearch, String stringToFind, StringComparison comparison)
		{
			int ind = stringToSearch.IndexOf(stringToFind, comparison); //This comparer should be extended to be "-"/"_" insensitive as well.
			return ind != -1;
		}

		/// <summary>
		/// Determines whether the list of string contains the specified string using the specified comparison.
		/// </summary>
		public static bool Contains(this IEnumerable<string> listToSearch, string itemToFind, StringComparison comparison)
		{
			foreach (string s in listToSearch)
			{
				if (s.Equals(itemToFind, comparison))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Removes diacritics from the specified string
		/// </summary>
		public static string RemoveDiacritics(this string stIn)
		{
			string stFormD = stIn.Normalize(NormalizationForm.FormD);
			StringBuilder sb = new StringBuilder();

			foreach (char t in stFormD)
			{
				UnicodeCategory uc = GetUnicodeCategory(t);
				if (uc != UnicodeCategory.NonSpacingMark)
					sb.Append(t);
			}

			return sb.ToString().Normalize(NormalizationForm.FormC);
		}
	}
}
