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
/// Author: Jonathan de Halleuxnamespace Spart.Parsers.Composite

namespace Spart.Parsers.Composite
{
	using System;
	using Spart.Scanners;
	using Spart.Actions;
	using Spart.Parsers.NonTerminal;

	public class AlternativeParser : BinaryTerminalParser
	{
		public AlternativeParser(Parser first, Parser second)
			:base(first,second)
		{}

		public override ParserMatch ParseMain(IScanner scan)
		{
			// save scanner state
			long offset = scan.Offset;

			// apply the first parser
			ParserMatch m = FirstParser.Parse( scan );
			// if m1 successful, do m2
			if (m.Success)
				return m;

			// not found
			scan.Seek(offset);

			// apply the second parser
			m = SecondParser.Parse( scan );
			if (m.Success)
				return m;

			scan.Seek(offset);
			return scan.NoMatch;
		}
	}
}
