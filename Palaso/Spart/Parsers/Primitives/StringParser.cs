// Spart License (zlib/png)
//
//
// Copyright (c) 2003 Jonathan de Halleux
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from
// the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software in a
// product, an acknowledgment in the product documentation would be
// appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source distribution.
//
// Author: Jonathan de Halleuxnamespace Spart.Parsers.Primitives

using System;
using Spart.Scanners;

namespace Spart.Parsers.Primitives
{
	/// <summary>
	/// Matches a given string
	/// </summary>
	public class StringParser : TerminalParser
	{
		private String m_MatchedString;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="str">The string to match</param>
		public StringParser(String str)
		{
			MatchedString = str;
		}

		/// <summary>
		/// the string to match
		/// </summary>
		public String MatchedString
		{
			get
			{
				return m_MatchedString;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("value", "matched string cannot be null");
				m_MatchedString = value;
			}
		}

		/// <summary>
		/// Inner parse method
		/// </summary>
		/// <param name="scanner">scanner</param>
		/// <returns>the match</returns>
		protected override ParserMatch ParseMain(IScanner scanner)
		{
			long offset = scanner.Offset;
			foreach(Char c in MatchedString)
			{
				// if input consummed return null
				if (scanner.AtEnd || c != scanner.Peek()  )
				{
					ParserMatch noMatch = ParserMatch.CreateFailureMatch(scanner, offset);
					scanner.Seek(offset);
					return noMatch;
				}

				// read next characted
				scanner.Read();
			}

			// if we arrive at this point, we have a match
			ParserMatch m = ParserMatch.CreateSuccessfulMatch(scanner, offset, MatchedString.Length);

			// return match
			return m;
		}
	}
}
