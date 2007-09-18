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
using Spart.Scanners;

namespace Spart.Parsers.NonTerminal
{
	/// <summary>
	/// Summary description for PostParseEventArgs.
	/// </summary>
	public class PreParseEventArgs : EventArgs
	{
		private NonTerminalParser m_Parser;
		private IScanner m_Scanner;
		/// <summary>
		/// construct a preparse event args from parser and scanner
		/// </summary>
		/// <param name="parser"></param>
		/// <param name="scanner"></param>
		public PreParseEventArgs(NonTerminalParser parser, IScanner scanner)
		{
			if (parser == null)
				throw new ArgumentNullException("parser");
			if (scanner == null)
				throw new ArgumentNullException("scanner");

			m_Parser = parser;
			m_Scanner = scanner;
		}
/// <summary>
/// Current parser
/// </summary>
		public NonTerminalParser Parser
		{
			get
			{
				return m_Parser;
			}
		}

		/// <summary>
		/// current scanner
		/// </summary>
		public IScanner Scanner
		{
			get
			{
				return m_Scanner;
			}
		}
	}
}
