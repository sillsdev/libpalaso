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

using System;
using System.Reflection;
using Spart.Parsers.Composite;
using Spart.Parsers.NonTerminal;

namespace Spart.Parsers
{

	/// <summary>
	/// Static helper class to create new parser operators
	/// </summary>
	public class Ops
	{
		public static AssertiveParser Expect(string id, string message, Parser p)
		{
			return new AssertiveParser(p, id, message);
		}

		public static AssertiveParser Expect(AssertiveParser.AssertDelegate assert,
										string id, string message, Parser p)
		{
			return new AssertiveParser(assert, p, id, message);
		}

		/// <summary>
		/// &gt;&gt; operator
		/// </summary>
		/// <param name="args">A list of parsers</param>
		/// <returns>A sequence parser</returns>
		public static SequenceParser Sequence(params Parser[] args)
		{
			return DistillParser<SequenceParser>(args);
		}

		/// <summary>
		/// | operator
		/// </summary>
		/// <param name="args">a list of the parsers to alternate between</param>
		/// <returns></returns>
		public static AlternativeParser Choice(params Parser[] args)
		{
			return DistillParser<AlternativeParser>(args);
		}
		/// <summary>
		/// &amp; operator
		/// </summary>
		/// <param name="args">a list of the parsers to intersect</param>
		/// <returns></returns>
		public static IntersectionParser Intersection(params Parser[] args)
		{
			return DistillParser<IntersectionParser>(args);
		}

		private static T DistillParser<T>(Parser[] args) where T : BinaryTerminalParser
		{
			if(args == null)
			{
				throw new ArgumentNullException("args");
			}
			if (args.Length < 2)
			{
				throw new ArgumentException("a sequence must have at least two items");
			}

			Parser head = args[0];
			if (head == null)
			{
				throw new ArgumentNullException(string.Format("argument at index {0} is null", args.Length - 1));
			}
			ConstructorInfo newT = typeof(T).GetConstructor(new Type[] { typeof(Parser), typeof(Parser) });
			for (int i = 1; i < args.Length; i++)
			{
				Parser tail = args[i];
				if(tail == null)
				{
					throw new ArgumentNullException(string.Format("argument at index {0} is null", i));
				}

				// would really like to do:
				//     head = new T(head, tail);
				// but not allowed, thus:
				head = (Parser) newT.Invoke(new object[] { head, tail });
			}
			return (T)head;
		}


		/// <summary>
		/// - operator
		/// </summary>
		/// <param name="first">A parser to recognize</param>
		/// <param name="second">A parser to not recognize</param>
		/// <returns></returns>
		public static DifferenceParser Difference(Parser first, Parser second)
		{
			if (first == null)
				throw new ArgumentNullException("first parser is null");
			if (second == null)
				throw new ArgumentNullException("second parser is null");

			return new DifferenceParser(first, second);
		}


		/// <summary>
		/// % operator
		/// </summary>
		/// <param name="first">A parser that recognizes the item(s)</param>
		/// <param name="second">A parser that recognizes the delimiter</param>
		/// <returns></returns>
		public static ListParser List(Parser first, Parser second)
		{
			if (first == null)
				throw new ArgumentNullException("first parser is null");
			if (second == null)
				throw new ArgumentNullException("second parser is null");

			return new ListParser(first, second);
		}

		/// <summary>
		/// * operators
		/// </summary>
		/// <param name="parser"></param>
		/// <returns></returns>
		public static RepetitionParser ZeroOrMore(Parser parser)
		{
			if (parser == null)
				throw new ArgumentNullException("parser is null");

			return new RepetitionParser(parser, 0, uint.MaxValue);
		}

		/// <summary>
		/// + operator
		/// </summary>
		/// <param name="parser"></param>
		/// <returns></returns>
		public static RepetitionParser OneOrMore(Parser parser)
		{
			if (parser == null)
				throw new ArgumentNullException("parser is null");
			return new RepetitionParser(parser, 1, uint.MaxValue);
		}

		/// <summary>
		/// ! operator
		/// </summary>
		/// <param name="parser"></param>
		/// <returns></returns>
		public static RepetitionParser Optional(Parser parser)
		{
			if (parser == null)
				throw new ArgumentNullException("parser is null");

			return new RepetitionParser(parser, 0, 1);
		}

	}
}
