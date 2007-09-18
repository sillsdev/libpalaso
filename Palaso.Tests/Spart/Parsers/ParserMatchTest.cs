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
using Spart.Scanners;

namespace Spart.Tests.Parsers
{

	[TestFixture]
	public class ParserMatchTest
	{
		public ParserMatch NoMatch
		{
			get
			{
				return Provider.NewScanner.NoMatch;
			}
		}

		public ParserMatch EmptyMatch
		{
			get
			{
				return Provider.NewScanner.EmptyMatch;
			}
		}

		[Test]
		public void NoMatchTest()
		{
			Assert.AreEqual(NoMatch.Success,false);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void NoMatchEmpty()
		{
			bool b=NoMatch.Empty;
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void NoMatchValue()
		{
			String o = NoMatch.Value;
		}

		[Test]
		public void Concat_MatchOntoEmpty_Match()
		{
			IScanner scanner = Provider.NewScanner;
			scanner.Read();

			ParserMatch emptyMatch = scanner.EmptyMatch;

			long startOffset = scanner.Offset;
			scanner.Read();
			long endOffset = scanner.Offset;
			ParserMatch match = new ParserMatch(scanner, startOffset, (int) (endOffset - startOffset));
			emptyMatch.Concat(match);

			Assert.AreEqual(startOffset, emptyMatch.Offset);
			Assert.AreEqual(endOffset - startOffset, emptyMatch.Length);
			Assert.IsFalse(emptyMatch.Empty);
		}

		[Test]
		public void Concat_EmptyToMatch_Match()
		{
			IScanner scanner = Provider.NewScanner;
			scanner.Read();

			long startOffset = scanner.Offset;
			scanner.Read();
			long endOffset = scanner.Offset;

			ParserMatch emptyMatch = scanner.EmptyMatch;

			ParserMatch match = new ParserMatch(scanner, startOffset, (int)(endOffset - startOffset));
			match.Concat(emptyMatch);

			Assert.AreEqual(startOffset, match.Offset);
			Assert.AreEqual(endOffset - startOffset, match.Length);
			Assert.IsFalse(match.Empty);
		}

		[Test]
		public void Concat()
		{
			IScanner scanner = Provider.NewScanner;
			scanner.Read();

			long startOffset1 = scanner.Offset;
			scanner.Read();
			long endOffset1 = scanner.Offset;

			ParserMatch match1 = new ParserMatch(scanner, startOffset1, (int)(endOffset1 - startOffset1));

			long startOffset2 = scanner.Offset;
			scanner.Read();
			scanner.Read();
			long endOffset2 = scanner.Offset;

			ParserMatch match2 = new ParserMatch(scanner, startOffset2, (int)(endOffset2 - startOffset2));

			match1.Concat(match2);

			Assert.AreEqual(startOffset1, match1.Offset);
			Assert.AreEqual(endOffset2 - startOffset1, match1.Length);
			Assert.IsFalse(match1.Empty);
		}



	}
}
