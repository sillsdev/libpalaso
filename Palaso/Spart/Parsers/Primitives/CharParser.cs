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
using Spart.Parsers.Primitives.Testers;
using Spart.Scanners;

namespace Spart.Parsers.Primitives
{
	/// <summary>
	/// Matches any character that the given character recognizer recognizes
	/// </summary>
	public class CharParser : NegatableParser
	{
		private CharRecognizer m_Tester;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="tester">The character recognizer to use to match</param>
		public CharParser(CharRecognizer tester)
		{
			Accepts = tester;
		}

		/// <summary>
		/// Character tester to use to determine if we have a match
		/// </summary>
		public CharRecognizer Accepts
		{
			get { return m_Tester; }
			set
			{
				if (m_Tester == value)
				{
					return;
				}
				if (value == null)
				{
					throw new ArgumentNullException("character tester");
				}
				m_Tester = value;
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

			bool isAccepted = Accepts(scanner.Peek());
			if (isAccepted && Negate || !isAccepted && !Negate)
			{
				return scanner.NoMatch;
			}

			// match character
			scanner.Peek();
			// if we arrive at this point, we have a match
			ParserMatch m = scanner.CreateMatch(offset, 1);

			// updating offset
			scanner.Read();

			// return match
			return m;
		}
	}
}