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
	using Spart.Parsers.Composite;

	/// <summary>
	/// Static helper class to create new parser operators
	/// </summary>
	public class Ops
	{
		/// <summary>
		/// &gt;&gt; operator
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		public static SequenceParser Seq(Parser first, Parser second)
		{
			if (first == null)
				throw new ArgumentNullException("first parser is null");
			if (second== null)
				throw new ArgumentNullException("second parser is null");

			return new SequenceParser(first,second);
		}

		public static SequenceParser Seq(char first, Parser second)
		{
			return Seq(Prims.Ch(first),second);
		}


		public static SequenceParser Seq(Parser first, char second)
		{
			return Seq(first,Prims.Ch(second));
		}


		public static SequenceParser Seq(String first, Parser second)
		{
			return Seq(Prims.Str(first),second);
		}


		public static SequenceParser Seq(Parser first, String second)
		{
			return Seq(first,Prims.Str(second));
		}

		/// <summary>
		/// * operators
		/// </summary>
		/// <param name="parser"></param>
		/// <returns></returns>
		public static RepetitionParser Klenee(Parser parser)
		{
			if (parser == null)
				throw new ArgumentNullException("parser is null");

			return new RepetitionParser(parser,0,uint.MaxValue);
		}

		public static RepetitionParser Klenee(char c)
		{
			return Klenee(Prims.Ch(c));
		}


		public static RepetitionParser Klenee(String s)
		{
			return Klenee(Prims.Str(s));
		}

		/// <summary>
		/// + operator
		/// </summary>
		/// <param name="parser"></param>
		/// <returns></returns>
		public static RepetitionParser Positive(Parser parser)
		{
			if (parser == null)
				throw new ArgumentNullException("parser is null");
			return new RepetitionParser(parser,1,uint.MaxValue);
		}


		public static RepetitionParser Positive(char c)
		{
			return Positive(Prims.Ch(c));
		}


		public static RepetitionParser Positive(String s)
		{
			return Positive(Prims.Str(s));
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

			return new RepetitionParser(parser,0,1);
		}


		public static RepetitionParser Optional(char c)
		{
			return Optional(Prims.Ch(c));
		}


		public static RepetitionParser Optional(String s)
		{
			return Optional(Prims.Str(s));
		}

		/// <summary>
		/// | operator
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		public static AlternativeParser Alternative(Parser first, Parser second)
		{
			if (first == null)
				throw new ArgumentNullException("first parser is null");
			if (second== null)
				throw new ArgumentNullException("second parser is null");

			return new AlternativeParser(first, second);
		}

		public static AlternativeParser Alternative(char first, Parser second)
		{
			return Alternative(Prims.Ch(first),second);
		}


		public static AlternativeParser Alternative(Parser first, char second)
		{
			return Alternative(first,Prims.Ch(second));
		}


		public static AlternativeParser Alternative(String first, Parser second)
		{
			return Alternative(Prims.Str(first),second);
		}


		public static AlternativeParser Alternative(Parser first, String second)
		{
			return Alternative(first,Prims.Str(second));
		}

		/// <summary>
		/// & operator
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		public static IntersectionParser Intersection(Parser first, Parser second)
		{
			if (first == null)
				throw new ArgumentNullException("first parser is null");
			if (second== null)
				throw new ArgumentNullException("second parser is null");

			return new IntersectionParser(first, second);
		}


		public static IntersectionParser Intersection(char first, Parser second)
		{
			return Intersection(Prims.Ch(first),second);
		}


		public static IntersectionParser Intersection(Parser first, char second)
		{
			return Intersection(first,Prims.Ch(second));
		}


		public static IntersectionParser Intersection(String first, Parser second)
		{
			return Intersection(Prims.Str(first),second);
		}


		public static IntersectionParser Intersection(Parser first, String second)
		{
			return Intersection(first,Prims.Str(second));
		}

		/// <summary>
		/// - operator
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		public static DifferenceParser Difference(Parser first, Parser second)
		{
			if (first == null)
				throw new ArgumentNullException("first parser is null");
			if (second== null)
				throw new ArgumentNullException("second parser is null");

			return new DifferenceParser(first, second);
		}

		public static DifferenceParser Difference(char first, Parser second)
		{
			return Difference(Prims.Ch(first),second);
		}


		public static DifferenceParser Difference(Parser first, char second)
		{
			return Difference(first,Prims.Ch(second));
		}


		public static DifferenceParser Difference(String first, Parser second)
		{
			return Difference(Prims.Str(first),second);
		}


		public static DifferenceParser Difference(Parser first, String second)
		{
			return Difference(first,Prims.Str(second));
		}

		/// <summary>
		/// % operator
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		public static ListParser List(Parser first, Parser second)
		{
			if (first == null)
				throw new ArgumentNullException("first parser is null");
			if (second== null)
				throw new ArgumentNullException("second parser is null");

			return new ListParser(first, second);
		}

		public static ListParser List(char first, Parser second)
		{
			return List(Prims.Ch(first),second);
		}


		public static ListParser List(Parser first, char second)
		{
			return List(first,Prims.Ch(second));
		}


		public static ListParser List(String first, Parser second)
		{
			return List(Prims.Str(first),second);
		}


		public static ListParser List(Parser first, String second)
		{
			return List(first,Prims.Str(second));
		}
	}
}
