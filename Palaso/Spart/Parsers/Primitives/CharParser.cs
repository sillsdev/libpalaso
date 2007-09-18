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
/// Author: Jonathan de Halleuxnamespace Spart.Parsers.Primitives


namespace Spart.Parsers.Primitives
{
	using System;
	using Spart.Scanners;
	using Spart.Actions;
	using Spart.Parsers.NonTerminal;
	using Spart.Parsers.Primitives.Testers;

	public class CharParser : NegatableParser
	{
		private ICharTester m_Tester;

		public CharParser(ICharTester tester)
		:base()
		{
			Tester = tester;
		}

		public ICharTester Tester
		{
			get
			{
				return m_Tester;
			}
			set
			{
				if (m_Tester==value)
					return;
				if (value == null)
					throw new ArgumentNullException("character tester");
				m_Tester = value;
			}
		}

		public override ParserMatch ParseMain(IScanner scanner)
		{
			long offset = scanner.Offset;

			bool test = Tester.Test(scanner.Peek());
			if (test && Negate || !test && !Negate)
				return scanner.NoMatch;

			// match character
			char c = (char)scanner.Peek();
			// if we arrive at this point, we have a match
			ParserMatch m = scanner.CreateMatch(offset, 1);

			// updating offset
			scanner.Read();

			// return match
			return m;
		}
	}
}
