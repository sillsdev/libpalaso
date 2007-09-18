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

namespace Spart.Parsers.Composite
{
	/// <summary>
	/// Match first but not second. If both match and the matched text of the second is
	/// shorter than the matched text of the first, a successful match is made
	/// </summary>
	public class DifferenceParser : BinaryTerminalParser
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="left">match left</param>
		/// <param name="right">don't match right</param>
		public DifferenceParser(Parser left, Parser right)
			:base(left,right)
		{}

		/// <summary>
		/// Inner parse method
		/// </summary>
		/// <param name="scanner">scanner</param>
		/// <returns>the match</returns>
		protected override ParserMatch ParseMain(Scanners.IScanner scanner)
		{
			ParserMatch m = FirstParser.Accepts(scanner);
			if (!m.Success)
			{
				return scanner.NoMatch;
			}

			// doing difference
			ParserMatch d = SecondParser.Accepts(scanner);
			if (d.Success)
			{
				if (d.Length >= m.Length)
				{
					return scanner.NoMatch;
				}
			}

			// ok
			FirstParser.Parse(scanner);
			return m;
		}
	}

}
