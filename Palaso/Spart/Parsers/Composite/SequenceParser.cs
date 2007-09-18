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
// Author: Jonathan de Halleuxnamespace Spart.Parsers.Composite
using Spart.Scanners;

namespace Spart.Parsers.Composite
{
	/// <summary>
	/// matches a sequence of parsers
	/// </summary>
	public class SequenceParser : BinaryTerminalParser
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="first">the first parser to match</param>
		/// <param name="second">the next parser to match</param>
		public SequenceParser(Parser first, Parser second)
				: base(first, second) {}

				/// <summary>
				/// Inner parse method
				/// </summary>
				/// <param name="scanner">scanner</param>
				/// <returns>the match</returns>
				protected override ParserMatch ParseMain(IScanner scanner)
		{
			// save scanner state
			long offset = scanner.Offset;

			// apply the first parser
			ParserMatch m = FirstParser.Parse(scanner);
			// if m1 successful, do m2
			if (m.Success)
			{
				ParserMatch m2 = SecondParser.Parse(scanner);
				if (m2.Success)
				{
					m.Concat(m2);
				}
				else
				{
					m = scanner.NoMatch;
				}
			}

			// restoring parser failed, rewind scanner
			if (!m.Success)
			{
				scanner.Seek(offset);
			}

			return m;
		}
	}
}