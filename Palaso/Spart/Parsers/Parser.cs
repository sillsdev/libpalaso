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
// Author: Jonathan de Halleuxnamespace Spart.Parsers

using Spart.Actions;
using Spart.Parsers.Composite;
using Spart.Scanners;

namespace Spart.Parsers
{
	/// <summary>
	/// Abstract parser class
	/// </summary>
	public abstract class Parser
	{
		/// <summary>
		/// Inner parse method
		/// </summary>
		/// <param name="scanner">scanner</param>
		/// <returns>the match</returns>
		protected abstract ParserMatch ParseMain(IScanner scanner);

		/// <summary>
		/// Outer parse method (consumes input)
		/// </summary>
		/// <param name="scanner"></param>
		/// <returns></returns>
		public ParserMatch Parse(IScanner scanner)
		{
			ParserMatch m = ParseMain(scanner);
			if (m.Success)
			{
				OnAction(m);
			}
			return m;
		}

		/// <summary>
		/// Lookahead to determine if parser can be used to parse (does not consume input)
		/// </summary>
		/// <param name="scanner"></param>
		/// <returns></returns>
		internal ParserMatch TryAccept(IScanner scanner)
		{
			long offset = scanner.Offset;
			ParserMatch m = ParseMain(scanner);
			scanner.Offset = offset;
			return m;
		}

		/// <summary>
		/// Action event
		/// </summary>
		public event ActionHandler Act;

		/// <summary>
		/// Action caller method
		/// </summary>
		/// <param name="m"></param>
		public virtual void OnAction(ParserMatch m)
		{
			if (Act != null)
			{
				Act(this, new ActionEventArgs(m));
			}
		}

		#region Operators

		/// <summary>
		/// Unary repeatable operator
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static RepetitionParser operator +(Parser p)
		{
			return Ops.OneOrMore(p);
		}

		/// <summary>
		/// Unary Optional operator
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static RepetitionParser operator !(Parser p)
		{
			return Ops.Optional(p);
		}

		/// <summary>
		/// Binary Alternative operator
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static AlternativeParser operator |(Parser left, Parser right)
		{
			return Ops.Choice(left, right);
		}

		/// <summary>
		/// Binary Intersection operator
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static IntersectionParser operator &(Parser left, Parser right)
		{
			return Ops.Intersection(left, right);
		}

		/// <summary>
		/// Binary Difference operator
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static DifferenceParser operator -(Parser left, Parser right)
		{
			return Ops.Difference(left, right);
		}

		/// <summary>
		/// Binary List operator
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static ListParser operator %(Parser left, Parser right)
		{
			return Ops.List(left, right);
		}

		/// <summary>
		/// Cast operator, creates a Parser that recognizes the given char
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static implicit operator Parser(char c)
		{
			return Prims.Ch(c);
		}

		/// <summary>
		/// Cast operator, creates a Parser that recognizes the given string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static implicit operator Parser(string s)
		{
			return Prims.Str(s);
		}

		/// <summary>
		/// Applies the given action handler to this parser
		/// </summary>
		/// <param name="act">An ActionHandler</param>
		/// <returns>this</returns>
		public virtual Parser this[ActionHandler act]
		{
			get
			{
				Act += act;
				return this;
			}
		}

		#endregion
	}
}