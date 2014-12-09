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

using NUnit.Framework;
using Spart.Parsers;
using Spart.Parsers.Composite;
using Spart.Parsers.Primitives;
using Spart.Scanners;

namespace SIL.WritingSystems.Tests.Spart.Parsers.Composite
{
	[TestFixture]
	public class SequenceTest
	{
		public StringParser First
		{
			get { return new StringParser(Provider.Text.Substring(0, 3)); }
		}

		public StringParser Second
		{
			get { return new StringParser(Provider.Text.Substring(3, 4)); }
		}

		public StringParser Second2
		{
			get { return new StringParser(Provider.Text.Substring(5, 2)); }
		}

		[Test]
		public void Constructor()
		{
			IScanner scanner = Provider.NewScanner;
			Parser f = First;
			Parser s = Second;
			SequenceParser parser = new SequenceParser(f, s);
			Assert.AreEqual(f, parser.FirstParser);
			Assert.AreEqual(s, parser.SecondParser);
		}

		[Test]
		public void Success()
		{
			IScanner scanner = Provider.NewScanner;
			SequenceParser parser = new SequenceParser(First, Second);

			ParserMatch m = parser.Parse(scanner);
			Assert.IsTrue(m.Success);
			Assert.AreEqual(m.Offset, 0);
			Assert.AreEqual(m.Offset + m.Length, scanner.Offset);
			Assert.AreEqual(First.MatchedString + Second.MatchedString, m.Value);
		}

		[Test]
		public void FailureFirst()
		{
			IScanner scanner = Provider.NewScanner;
			SequenceParser parser = new SequenceParser(Second, Second2);

			ParserMatch m = parser.Parse(scanner);
			Assert.IsFalse(m.Success);
			Assert.AreEqual(0, scanner.Offset);
		}

		[Test]
		public void FailureSecond()
		{
			IScanner scanner = Provider.NewScanner;
			SequenceParser parser = new SequenceParser(First, Second2);

			ParserMatch m = parser.Parse(scanner);
			Assert.IsFalse(m.Success);
			Assert.AreEqual(0, scanner.Offset);
		}
	}
}