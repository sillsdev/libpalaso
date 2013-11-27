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
/// Author: Jonathan de Halleux
///

using System;
using NUnit.Framework;
using Spart.Parsers;
using Spart.Parsers.Primitives;
using Spart.Scanners;

namespace Palaso.Tests.Spart.Parsers.Primitives
{
	[TestFixture]
	public class CharParserTest
	{
		public Char MatchedChar
		{
			get
			{
				return Provider.Text[0];
			}
		}

		public Char NonMatchedChar
		{
			get
			{
				return Provider.Text[1];
			}
		}

		[Test]
		public void SuccessParse()
		{
			IScanner scanner = Provider.NewScanner;
			CharParser parser = Prims.Ch(MatchedChar);

			ParserMatch m = parser.Parse(scanner);
			Assert.IsTrue(m.Success);
			Assert.AreEqual(m.Offset,0);
			Assert.AreEqual(m.Length,1);
			Assert.AreEqual(scanner.Offset,1);
		}

		[Test]
		public void FailParse()
		{
			IScanner scanner = Provider.NewScanner;
			CharParser parser = Prims.Ch(NonMatchedChar);

			ParserMatch m = parser.Parse(scanner);
			Assert.IsFalse(m.Success);
			Assert.AreEqual(scanner.Offset,0);
		}


		[Test]
		public void NegateSuccessParse()
		{
			IScanner scanner = Provider.NewScanner;
			NegatableParser parser = ~Prims.Ch(NonMatchedChar);

			ParserMatch m = parser.Parse(scanner);
			Assert.IsTrue(m.Success);
			Assert.AreEqual(m.Offset,0);
			Assert.AreEqual(m.Length,1);
			Assert.AreEqual(scanner.Offset,1);
		}

		[Test]
		public void NegateFailParse()
		{
			IScanner scanner = Provider.NewScanner;
			NegatableParser parser = ~Prims.Ch(MatchedChar);

			ParserMatch m = parser.Parse(scanner);
			Assert.IsFalse(m.Success);
			Assert.AreEqual(scanner.Offset,0);
		}

	}
}
