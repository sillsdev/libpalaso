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

using System;

namespace Spart.Parsers
{
	/// <summary>
	/// A Parser with two terminals (which may be parsers themselves)
	/// </summary>
	public abstract class BinaryTerminalParser : TerminalParser
	{
		private Parser m_FirstParser;
		private Parser m_SecondParser;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		public BinaryTerminalParser(Parser first, Parser second)
		{
			FirstParser = first;
			SecondParser = second;
		}

		/// <summary>
		/// Access or change the first parser
		/// </summary>
		public Parser FirstParser
		{
			get
			{
				return m_FirstParser;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("first parser is null");
				m_FirstParser = value;
			}
		}

		/// <summary>
		/// Access or change the second parser
		/// </summary>
		public Parser SecondParser
		{
			get
			{
				return m_SecondParser;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("second parser is null");
				m_SecondParser = value;
			}
		}
	}
}
