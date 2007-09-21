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
using System;
using Spart.Scanners;

namespace Spart.Parsers.Composite
{
	/// <summary>
	/// matches a given range of count of the given parser
	/// </summary>
	public class RepetitionParser : UnaryTerminalParser
	{
		private uint m_LowerBound;
		private uint m_UpperBound;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="parser">the parser to match</param>
		/// <param name="lowerBound">the least number of valid occurrences</param>
		/// <param name="upperBound">the most number of valid occurrences</param>
		public RepetitionParser(Parser parser, uint lowerBound, uint upperBound)
				: base(parser)
		{
			SetBounds(lowerBound, upperBound);
		}
		/// <summary>
		/// the least amount of occurrences of the parser allowed
		/// </summary>
		public uint LowerBound
		{
			get { return m_LowerBound; }
		}
		/// <summary>
		/// the most amount of occurrences of the parser allowed
		/// </summary>
		public uint UpperBound
		{
			get { return m_UpperBound; }
		}

		/// <summary>
		/// set the range of the amount of occurrences of the parser allowed
		/// </summary>
		/// <param name="lb">the least amount of occurrences allowed</param>
		/// <param name="ub">the most amount of occurrences allowed</param>
		public void SetBounds(uint lb, uint ub)
		{
			if (ub < lb)
			{
				throw new ArgumentOutOfRangeException("lower bound must be smaller than upper bound");
			}
			if (lb < 0)
			{
				throw new ArgumentOutOfRangeException("bounds must be positive");
			}
			m_LowerBound = lb;
			m_UpperBound = ub;
		}

		/// <summary>
		/// Inner parse method
		/// </summary>
		/// <param name="scanner">scanner</param>
		/// <returns>the match</returns>
		protected override ParserMatch ParseMain(IScanner scanner)
		{
			// save scanner state
			long startOffset = scanner.Offset;
			ParserMatch m = ParserMatch.CreateSuccessfulEmptyMatch(scanner);
			ParserMatch m_temp;

			// execution bound
			int count = 0;

			// lower bound, minimum number of executions
			while (count < LowerBound && !scanner.AtEnd)
			{
				m_temp = Parser.Parse(scanner);
				// stop if not successful
				if (!m_temp.Success)
				{
					break;
				}
				// increment count and update full match
				++count;
				m.Concat(m_temp);
			}

			if (count == LowerBound)
			{
				while (count < UpperBound && !scanner.AtEnd)
				{
					m_temp = Parser.Parse(scanner);

					// stop if not successful
					if (!m_temp.Success)
					{
						break;
					}

					// increment count
					++count;
					m.Concat(m_temp);
				}
			}
			else
			{
				m = ParserMatch.CreateFailureMatch(scanner, startOffset);
			}

			// restoring parser failed, rewind scanner
			if (!m.Success)
			{
				scanner.Seek(startOffset);
			}

			return m;
		}
	}
}