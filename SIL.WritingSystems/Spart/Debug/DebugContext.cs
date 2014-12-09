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
// Author: Jonathan de Halleux

using System;
using System.IO;
using Spart.Parsers;
using Spart.Parsers.NonTerminal;
using Spart.Scanners;

namespace Spart.Debug
{
	/// <summary>
	/// A Debug Context
	/// </summary>
	public class DebugContext : IParserContext
	{
		private TextWriter m_Output;
		private int m_TabNumber;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="output"></param>
		public DebugContext(TextWriter output)
		{
			Output = output;
		}

		private void Indent()
		{
			++m_TabNumber;
		}

		private void UnIndent()
		{
			if (m_TabNumber == 0)
			{
				throw new InvalidOperationException("must call indent before unindent");
			}
			--m_TabNumber;
		}

		/// <summary>
		/// The output Text Writer
		/// </summary>
		public TextWriter Output
		{
			get
			{
				return m_Output;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				m_Output = value;
			}
		}

		/// <summary>
		/// A string with the current indentation suitable for prepending to output that should be nested
		/// </summary>
		protected string Tab
		{
			get
			{
				return new string(' ', m_TabNumber * 4);
			}
		}

		private static char MatchChar(ParserMatch parserMatch)
		{
			if (parserMatch.Success)
			{
				return '/';
			}
			return '#';
		}

		/// <summary>
		/// handle the preparse event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public virtual void PreParse(object sender, PreParseEventArgs args)
		{
			Output.WriteLine("{0}{1}: {2}", Tab, args.Parser.ID, ScannerValue(args.Scanner));
			Indent();
		}

		/// <summary>
		/// handle the postparse event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public virtual void PostParse(object sender, PostParseEventArgs args)
		{
			UnIndent();
			Output.WriteLine("{0}{1}{2}: {3}", Tab, MatchChar(args.Match), args.Parser.ID, ScannerValue(args.Scanner));
		}

		static private string ScannerValue(IScanner scanner)
		{
			return scanner.Substring(scanner.Offset, 20);
		}
	}
}