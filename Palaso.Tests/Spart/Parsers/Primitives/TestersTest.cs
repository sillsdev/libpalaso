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

namespace Palaso.Tests.Spart.Parsers.Primitives
{
	public class Helper
	{
		public static void Test(CharParser test, Char success, Char failed)
		{
			Assert.IsTrue(test.Accepts(success));
			Assert.IsFalse(test.Accepts(failed));
		}
	}

	[TestFixture]
	public class TestersTest
	{
		[Test]
		public void AnyCharTest()
		{
			CharParser test = Prims.AnyChar;
			Assert.IsTrue(test.Accepts('a'));
		}

		[Test]
		public void DigitCharTest()
		{
			Helper.Test(Prims.Digit, '1', ' ');
		}

		[Test]
		public void LetterCharTest()
		{
			Helper.Test(Prims.Letter, 'a', '1');
		}

		[Test]
		public void LetterOrDigitCharTest()
		{
			Helper.Test(Prims.LetterOrDigit, 'a', ' ');
			Helper.Test(Prims.LetterOrDigit, '1', ',');
		}

		[Test]
		public void LitteralCharTest()
		{
			Helper.Test(Prims.Ch('a'), 'a', ' ');
		}

		[Test]
		public void LowerCharTest()
		{
			Helper.Test(Prims.Lower, 'a', 'A');
		}

		[Test]
		public void PunctuationCharTest()
		{
			Helper.Test(Prims.Punctuation, '.', 'A');
		}

		[Test]
		public void RangeCharTest()
		{
			Helper.Test(Prims.Range('a', 'z'), 'c', '1');
		}

		[Test]
		public void SeparatorCharTest()
		{
			Helper.Test(Prims.Separator, ' ', '1');
		}

		[Test]
		public void SymbolCharTest()
		{
			Helper.Test(Prims.Symbol, '+', '1');
		}

		[Test]
		public void UpperCharTest()
		{
			Helper.Test(Prims.Upper, 'A', 'a');
		}

		[Test]
		public void WhiteSpaceCharTest()
		{
			Helper.Test(Prims.WhiteSpace, ' ', 'a');
		}
	}
}