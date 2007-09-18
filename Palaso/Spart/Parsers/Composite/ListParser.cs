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
// Author: Jonathan de Halleuxusing System;
using Spart.Scanners;

namespace Spart.Parsers.Composite
{
	/// <summary>
	/// recognize a list of one or more repetitions of first parser seperated by occurrences of
	/// the second parser (the delimiter). This is equivalent to:
	/// Sequence(first, ZeroOrMore(Sequence(second, first))
	/// a must not also match b!
	/// </summary>
	public class ListParser : BinaryTerminalParser
	{
		/// <summary>
		/// constructs a list parser that recognizes items separated by a delimiter
		/// </summary>
		/// <param name="first">item</param>
		/// <param name="second">delimiter</param>
		public ListParser(Parser first, Parser second)
				: base(first, second) {}

		/// <summary>
		/// Inner parse method
		/// </summary>
		/// <param name="scan">scanner</param>
		/// <returns>the match</returns>
		protected override ParserMatch ParseMain(IScanner scan)
		{
			long offset = scan.Offset;

			ParserMatch m = FirstParser.Parse(scan);
			if (!m.Success)
			{
				scan.Seek(offset);
				return scan.NoMatch;
			}

			while (!scan.AtEnd)
			{
				offset = scan.Offset;

				ParserMatch b = SecondParser.Parse(scan);
				if (!b.Success)
				{
					scan.Seek(offset);
					return m;
				}
				ParserMatch a = FirstParser.Parse(scan);
				if (!a.Success)
				{
					scan.Seek(offset);
					return m;
				}

				m.Concat(b);
				m.Concat(a);
			}

			return m;
		}
	}
}