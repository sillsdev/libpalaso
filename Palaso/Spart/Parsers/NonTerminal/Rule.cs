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
// Author: Jonathan de Halleuxnamespace Spart.Parsers.NonTerminal
using System;
using Spart.Actions;
using Spart.Parsers.Primitives;
using Spart.Scanners;

namespace Spart.Parsers.NonTerminal
{
	/// <summary>
	/// A rule is a parser holder.
	/// </summary>
	public class Rule : NonTerminalParser
	{
		private Parser m_Parser;

		/// <summary>
		/// Empty rule creator
		/// </summary>
		public Rule()
		{
			Parser = new NothingParser();
		}

		/// <summary>
		/// Constructs a rule with an id (used for debugging)
		/// </summary>
		/// <param name="id">rule id (used for debugging)</param>
		public Rule(string id)
				: this()
		{
			ID = id;
		}

		/// <summary>
		/// Creates a rule and assign parser
		/// </summary>
		/// <param name="p"></param>
		public Rule(Parser p)
		{
			Parser = p;
		}

		/// <summary>
		/// Creates a rule with an idand assign parser
		/// </summary>
		/// <param name="p"></param>
		/// <param name="id">rule id (used for debugging)</param>
		public Rule(string id, Parser p)
				: this(p)
		{
			ID = id;
		}

		/// <summary>
		/// Rule parser
		/// </summary>
		public Parser Parser
		{
			get {
				return m_Parser;
			}
			set
			{
				if (value == null && !(m_Parser is NothingParser))
				{
					m_Parser = new NothingParser();
				}
				else
				{
					if (m_Parser != value)
					{
						m_Parser = value;
					}
				}
			}
		}

		/// <summary>
		/// Inner parse method
		/// </summary>
		/// <param name="scanner"></param>
		/// <returns></returns>
		protected override ParserMatch ParseMain(IScanner scanner)
		{
			if (scanner == null)
			{
				throw new ArgumentNullException("scanner");
			}

			OnPreParse(scanner);
			ParserMatch match = Parser.Parse(scanner);
			OnPostParse(match, scanner);

			return match;
		}

		/// <summary>
		/// Applies the given action handler to this parser
		/// </summary>
		/// <remarks>
		/// This is syntactic sugar to allow Rules to use shorthand and not require a cast
		/// </remarks>
		/// <param name="act">An ActionHandler</param>
		/// <returns>this</returns>
		public new Rule this[ActionHandler act]
		{
			get
			{
				Parser p = base[act];
				return this;
			}
		}
	}
}