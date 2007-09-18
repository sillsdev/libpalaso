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

	public class RepetitionParser : UnaryTerminalParser
	{
		private uint m_LowerBound;
		private uint m_UpperBound;

		public RepetitionParser(Parser parser, uint lowerBound, uint upperBound)
			:base(parser)
		{
			SetBounds(lowerBound,upperBound);
		}

		public uint LowerBound
		{
			get
			{
				return m_LowerBound;
			}
		}

		public uint UpperBound
		{
			get
			{
				return m_UpperBound;
			}
		}

		public void SetBounds(uint lb, uint ub)
		{
			if (ub < lb)
				throw new ArgumentException("lower bound must be smaller than upper bound");
			m_LowerBound = lb;
			m_UpperBound = ub;
		}

		public override ParserMatch ParseMain(IScanner scanner)
		{
			// save scanner state
			long offset = scanner.Offset;

			ParserMatch m=scanner.EmptyMatch;
			ParserMatch m_temp=null;

			// execution bound
			int count=0;

			// lower bound, minimum number of executions
			while(count < LowerBound && !scanner.AtEnd)
			{
				m_temp = Parser.Parse(scanner);
				// stop if not successful
				if (!m_temp.Success)
					break;
				// increment count and update full match
				++count;
			}

			if (count == LowerBound)
			{
				while(count < UpperBound && !scanner.AtEnd)
				{
					m_temp = Parser.Parse(scanner);

					// stop if not successful
					if (!m_temp.Success)
						break;

					// increment count
					++count;
				}
			}
			else
				m=scanner.NoMatch;

			if (m.Success)
				m = scanner.CreateMatch(offset,count);

			// restoring parser failed, rewind scanner
			if (!m.Success)
				scanner.Seek(offset);

			return m;
		}
	}
}
