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
/// Author: Jonathan de Halleuxnamespace Spart.Parsers


namespace Spart.Parsers
{
	using System;
	using Spart.Scanners;
	using Spart.Actions;
	using Spart.Parsers.Composite;
	using Spart.Parsers.NonTerminal;

	/// <summary>
	/// Abstract parser class
	/// </summary>
	public abstract class Parser
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public Parser()
		{}

		/// <summary>
		/// Inner parse method
		/// </summary>
		/// <param name="scan">scanner</param>
		/// <returns>the match</returns>
		public abstract ParserMatch ParseMain(IScanner scan);

		/// <summary>
		/// Outer parse method
		/// </summary>
		/// <param name="scan"></param>
		/// <returns></returns>
		public virtual ParserMatch Parse(IScanner scan)
		{
			ParserMatch m = ParseMain(scan);
			if (m.Success)
				OnAction(m);
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
				Act(this,new ActionEventArgs(m));
		}

		#region Operators

		/// <summary>
		/// Positive operator
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static RepetitionParser operator +(Parser p)
		{
			return Ops.Positive(p);
		}

		/// <summary>
		/// Optional operator
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static RepetitionParser operator ! (Parser p)
		{
			return Ops.Optional(p);
		}

		/// <summary>
		/// Alternative operator
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static AlternativeParser operator | (Parser left, Parser right)
		{
			return Ops.Alternative(left, right);
		}

		/// <summary>
		/// Intersection operator
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static IntersectionParser operator & (Parser left, Parser right)
		{
			return Ops.Intersection(left, right);
		}

		/// <summary>
		/// Difference operator
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static DifferenceParser operator - (Parser left, Parser right)
		{
			return Ops.Difference(left, right);
		}

		/// <summary>
		/// List operator
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static ListParser operator % (Parser left, Parser right)
		{
			return Ops.List(left,right);
		}

		#endregion
	}
}
