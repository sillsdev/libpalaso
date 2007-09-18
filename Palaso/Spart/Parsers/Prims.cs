/// Spart License (zlib/png)
///
///
/// Copyright (c) 2003 Jonathan de Halleux
///
/// This software is provided 'as-is', without any express or implied warranty.
/// In no event will the authors be held liable for any damages arising from
/// the use of this software.
///
/// Permission is granted to anyone to use this software for any purpose,
/// including commercial applications, and to alter it and redistribute it
/// freely, subject to the following restrictions:
///
/// 1. The origin of this software must not be misrepresented; you must not
/// claim that you wrote the original software. If you use this software in a
/// product, an acknowledgment in the product documentation would be
/// appreciated but is not required.
///
/// 2. Altered source versions must be plainly marked as such, and must not be
/// misrepresented as being the original software.
///
/// 3. This notice may not be removed or altered from any source distribution.
///
/// Author: Jonathan de Halleuxusing System;

namespace Spart.Parsers
{
	using System;
	using Spart.Parsers.Primitives;
	using Spart.Parsers.Primitives.Testers;

	/// <summary>
	/// Static helper class to create primitive parsers
	/// </summary>
	public class Prims
	{
		/// <summary>
		/// Creates a parser that matches a single character
		/// </summary>
		/// <param name="c">character to match</param>
		/// <returns></returns>
		public static CharParser Ch(Char c)
		{
			return new CharParser(new LitteralCharTester(c));
		}

		/// <summary>
		/// Creates a parser that matches a string
		/// </summary>
		/// <param name="s">string to match</param>
		/// <returns></returns>
		public static StringParser Str(String s)
		{
			return new StringParser(s);
		}

		/// <summary>
		/// Creates a parser that matches a range of character
		/// </summary>
		/// <param name="first"></param>
		/// <param name="last"></param>
		/// <returns></returns>
		public static CharParser Range(Char first, Char last)
		{
			return new CharParser(new RangeCharTester(first, last));
		}

		/// <summary>
		/// Creates a parser that matches any character
		/// </summary>
		public static CharParser AnyChar
		{
			get
			{
				return new CharParser(new AnyCharTester());
			}
		}

		/// <summary>
		/// Creates a parser that matches control characters
		/// </summary>
		public static CharParser Control
		{
			get
			{
				return new CharParser(new ControlCharTester());
			}
		}

		/// <summary>
		/// Creates a parser that matches digit characters
		/// </summary>
		public static CharParser Digit
		{
			get
			{
				return new CharParser(new DigitCharTester());
			}
		}

		/// <summary>
		/// Creates a parser that matches letter characters
		/// </summary>
		public static CharParser Letter
		{
			get
			{
				return new CharParser(new LetterCharTester());
			}
		}

		/// <summary>
		/// Creates a parser that matches letter or digit characters
		/// </summary>
		public static CharParser LetterOrDigit
		{
			get
			{
				return new CharParser(new LetterOrDigitCharTester());
			}
		}

		/// <summary>
		/// Creates a parser that matches lower case characters
		/// </summary>
		public static CharParser Lower
		{
			get
			{
				return new CharParser(new LowerCharTester());
			}
		}

		/// <summary>
		/// Creates a parser that matches punctuation characters
		/// </summary>
		public static CharParser Punctuation
		{
			get
			{
				return new CharParser(new PunctuationCharTester());
			}
		}

		/// <summary>
		/// Creates a parser that matches separator characters
		/// </summary>
		public static CharParser Separator
		{
			get
			{
				return new CharParser(new SeparatorCharTester());
			}
		}

		/// <summary>
		/// Creates a parser that matches symbol characters
		/// </summary>
		public static CharParser Symbol
		{
			get
			{
				return new CharParser(new SymbolCharTester());
			}
		}

		/// <summary>
		/// Creates a parser that matches upper case characters
		/// </summary>
		public static CharParser Upper
		{
			get
			{
				return new CharParser(new UpperCharTester());
			}
		}

		/// <summary>
		/// Creates a parser that matches whitespace characters
		/// </summary>
		public static CharParser WhiteSpace
		{
			get
			{
				return new CharParser(new WhiteSpaceCharTester());
			}
		}

		/// <summary>
		/// Creates a parser that matches and end of line
		/// </summary>
		public static EolParser Eol
		{
			get
			{
				return new EolParser();
			}
		}

		/// <summary>
		/// Creates a parser that matches the end of the input
		/// </summary>
		public static EndParser End
		{
			get
			{
				return new EndParser();
			}
		}
	}
}
