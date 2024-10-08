using System;
using System.Globalization;
using System.Text;

namespace SIL.Archiving
{
	/// ------------------------------------------------------------------------------------
	public static class Extensions
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Combines the functionality of StringBuilder.AppendFormat and
		/// StringBuilder.AppendLine.  Also allows for the delimiter to be specified.  If the
		/// delimiter is null, Environment.NewLine will be used.
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		/// <param name="delimiter"></param>
		/// ------------------------------------------------------------------------------------
		public static void AppendLineFormat(this StringBuilder sb, string format, object[] args, string delimiter)
		{
			delimiter ??= Environment.NewLine;
			if (sb.Length != 0)
				sb.Append(delimiter);
			sb.AppendFormat(format, args);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Appends with a delimiter
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="value"></param>
		/// <param name="delimiter"></param>
		/// ------------------------------------------------------------------------------------
		public static void AppendDelimiter(this StringBuilder sb, string value, string delimiter)
		{
			delimiter ??= Environment.NewLine;
			if (sb.Length != 0)
				sb.Append(delimiter);
			sb.Append(value);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Removes non-Latin characters from a string. If the string contains multiple periods,
		/// only the last one is retained.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="charReplacement">Replace non-Latin characters with this</param>
		/// <param name="spaceReplacement">Replace white-space characters with this</param>
		/// <param name="charsToIgnore">Characters in this string will not be replaced</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public static string ToLatinOnly(this string s, string charReplacement, string spaceReplacement, string charsToIgnore)
		{
			var returnVal = "";
			int lastDot = s.LastIndexOf(".", StringComparison.InvariantCulture);

			int i = 0;
			foreach (var c in s)
			{
				var test = c.ToString(CultureInfo.InvariantCulture);

				if ((test == charReplacement) || (test == spaceReplacement))  // do not replace the replacement characters
					returnVal += test;
				else if (c == '.' && i != lastDot) // only allow one dot in the file name
					returnVal += charReplacement;
				else if (charsToIgnore.Contains(test))  // do not replace ignored characters
					returnVal += test;
				else if (char.IsWhiteSpace(c))
					returnVal += spaceReplacement;
				else if (c < 33)  // consider characters before the space (32) as whitespace also
					returnVal += spaceReplacement;
				else if (c < 48)  // symbols, quotation marks, etc
					returnVal += charReplacement;
				else if ((c > 57) && (c < 65))  // symbols and punctuation
					returnVal += charReplacement;
				else if ((c > 90) && (c < 97))  // punctuation, misc
					returnVal += charReplacement;
				else if (c > 122)  // non-Latin stuff
					returnVal += charReplacement;
				else  // legal characters
					returnVal += test;
				i += 1;
			}

			// remove consecutive replacement characters
			while (returnVal.Contains(spaceReplacement + charReplacement))
				returnVal = returnVal.Replace(spaceReplacement + charReplacement, spaceReplacement);

			while (returnVal.Contains(charReplacement + spaceReplacement))
				returnVal = returnVal.Replace(charReplacement + spaceReplacement, spaceReplacement);

			while (returnVal.Contains(charReplacement + charReplacement))
				returnVal = returnVal.Replace(charReplacement + charReplacement, charReplacement);

			while (returnVal.Contains(spaceReplacement + spaceReplacement))
				returnVal = returnVal.Replace(spaceReplacement + spaceReplacement, spaceReplacement);


			return returnVal;
		}
	}
}
