using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using SIL.PlatformUtilities;
using JetBrains.Annotations;
using static System.Char;

namespace SIL.Extensions
{
	public static class StringExtensions
	{
		[PublicAPI]
		public const char kObjReplacementChar = '\uFFFC';

		[PublicAPI]
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
		[PublicAPI]
		public static int[] ToIntArray(this string str)
		{
			List<int> array = new List<int>();

			if (str != null)
			{
				string[] pieces = str.Split(',');
				foreach (string piece in pieces)
				{
					if (int.TryParse(piece, out var i))
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
				argList = argList.Trim(',');
				return "FormatWithErrorStringInsteadOfException(" + format + "," + argList + ") Exception: " + e.Message;
			}
		}

		[PublicAPI]
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
		[PublicAPI]
		public static string CombineForPath(this string rootPath, params string[] partsOfThePath)
		{
			string result = rootPath;
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
			var invalidFilenameChars = Path.GetInvalidFileNameChars();
			Array.Sort(invalidFilenameChars);
			return Sanitize(input, invalidFilenameChars, errorChar, replaceNbsp, false);
		}

		// ENHANCE: Make versions of these methods that can guarantee a file/path that would be
		// valid across all known/likely operating systems to ensure better portability.

		/// <summary>
		/// Finds and replaces invalid characters in a filename
		/// </summary>
		/// <param name="input">the string to clean</param>
		/// <param name="errorChar">the character which replaces bad characters</param>
		/// <remarks>This is platform-specific.</remarks>
		[PublicAPI]
		public static string SanitizeFilename(this string input, char errorChar) =>
			SanitizeFilename(input, errorChar, false);

		/// <summary>
		/// Finds and replaces invalid characters in a path
		/// </summary>
		/// <param name="input">the string to clean</param>
		/// <param name="errorChar">the character which replaces bad characters</param>
		[PublicAPI]
		public static string SanitizePath(this string input, char errorChar)
		{
			var invalidPathChars = Path.GetInvalidPathChars();
			Array.Sort(invalidPathChars);
			var sanitized = Sanitize(input, invalidPathChars, errorChar, false, true);
			if (Platform.IsWindows)
			{
				sanitized = Regex.Replace(sanitized, "^(:)(.*)", $"{errorChar}$3");
				sanitized = Regex.Replace(sanitized, "^([^a-zA-Z])(:)(.*)", $"$1{errorChar}$3");
				sanitized = new String(sanitized.Take(2).ToArray()) + new String(sanitized.Skip(2).ToArray()).Replace(':', errorChar);
			}

			return sanitized;
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
				throw new ArgumentException("The character used to replace bad characters must not itself be an invalid character.", nameof(errorChar));

			var result = new StringBuilder();

			foreach (var characterToTest in input)
			{
				if (Array.BinarySearch(invalidChars, characterToTest) >= 0 ||
					characterToTest < ' ' || // eliminate all control characters
					// Apparently Windows allows the ORC in *some* positions in filenames, but
					// that can't be good, so we'll replace that too.
					characterToTest == '\uFFFC')
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
				if ((lastChar == '.' && (!allowTrailingUpHierarchyDots ||
					!TrailingDotIsValidHierarchySpecifier(result, lastCharPos))) ||
					lastChar.IsInvalidFilenameLeadingOrTrailingSpaceChar())
				{
					if (!IsWhiteSpace(lastChar))
					{
						// Once we've stripped off anything besides whitespace, we
						// can't legitimately treat any remaining trailing dots as
						// valid hierarchy specifiers.
						allowTrailingUpHierarchyDots = false;
					}
					result.Remove(lastCharPos, 1);
					lastCharPos--;
				}
				else
					break;
			}

			return result.Length == 0 ? errorChar.ToString() : result.ToString();
		}

		private static bool TrailingDotIsValidHierarchySpecifier(StringBuilder result, int lastCharPos)
		{
			Debug.Assert(lastCharPos == result.Length - 1 && result[lastCharPos] == '.');

			if (lastCharPos == 0)
			{
				// REVIEW: This is an iffy case. Technically, a single dot is a valid path,
				// referring to the current directory, though it's not likely to be
				// useful. If the caller wants that, they could just not specify anything.
				return true;
			}
			if (result[lastCharPos - 1] == '.')
			{
				// This is a probably a valid up-one-level specifier.
				if (lastCharPos == 1 ||
					result[lastCharPos - 2] == Path.DirectorySeparatorChar ||
					result[lastCharPos - 2] == Path.AltDirectorySeparatorChar)
					return true;
			}

			return false;
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
		[PublicAPI]
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
		[PublicAPI]
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
		[PublicAPI]
		public static string NullTrim(this string value)
		{
			return value.NullTrim(null);
		}

		/// <summary>
		/// Determines whether the string contains the specified string using the specified comparison.
		/// </summary>
		[PublicAPI]
		public static bool Contains(this String stringToSearch, String stringToFind, StringComparison comparison)
		{
			int ind = stringToSearch.IndexOf(stringToFind, comparison); //This comparer should be extended to be "-"/"_" insensitive as well.
			return ind != -1;
		}

		/// <summary>
		/// Determines whether the list of string contains the specified string using the specified comparison.
		/// </summary>
		[PublicAPI]
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
		/// Overload of IndexOf that takes a custom IComparer<String> and considers all
		/// substrings of source to find a match
		/// https://stackoverflow.com/questions/50338199/indexof-with-custom-stringcomparer
		/// </summary>
		/// <param name="source">The source string to search</param>
		/// <param name="match">The string to look for within the target string</param>
		/// <param name="sc">the comparer to use to determine equality</param>
		/// <returns>The index of the first match or -1 if no matching substring is found</returns>
		[PublicAPI]
		public static int IndexOf(this string source, string match, IComparer<String> sc) {
			return Enumerable.Range(0, source.Length) // for each position in the string
				.FirstOrDefault(i => // find the first position where either
						// match is Equal at this position for length of match (or to end of string) or
						sc.Compare(source.Substring(i, Math.Min(match.Length, source.Length-i)), match) == 0 ||
						// match is Equal to one of the substrings beginning at this position
						Enumerable.Range(1, source.Length-i).Any(ml => sc.Compare(source.Substring(i, ml), match) == 0),
					-1 // else return -1 if no position matches
				);
		}

		/// <summary>
		/// Removes diacritics from the specified string
		/// </summary>
		[PublicAPI]
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
