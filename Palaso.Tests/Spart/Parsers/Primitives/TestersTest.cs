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

namespace Spart.Tests.Parsers.Primitives
{
	using NUnit.Framework;
	using Spart.Parsers.Primitives.Testers;
	using Spart.Parsers.Primitives;

	public class Helper
	{
		public static void Test(ICharTester test, Char success, Char failed)
		{
			Assertion.Assert(test.Test(success));
			Assertion.Assert(!test.Test(failed));
		}
	}

	[TestFixture]
	public class TestersTest
	{
		[Test]
		public void AnyCharTest()
		{
			AnyCharTester test = new AnyCharTester();
			Assertion.Assert(test.Test('a'));
		}

		[Test]
		public void DigitCharTest()
		{
			Helper.Test(new DigitCharTester(),'1',' ');
		}

		[Test]
		public void LetterCharTest()
		{
			Helper.Test(new LetterCharTester(),'a','1');
		}

		[Test]
		public void LetterOrDigitCharTest()
		{
			Helper.Test(new LetterOrDigitCharTester(),'a',' ');
			Helper.Test(new LetterOrDigitCharTester(),'1',',');
		}

		[Test]
		public void LitteralCharTest()
		{
			Helper.Test(new LitteralCharTester('a'),'a',' ');
		}


		[Test]
		public void LowerCharTest()
		{
			Helper.Test(new LowerCharTester(),'a','A');
		}

		[Test]
		public void PunctuationCharTest()
		{
			Helper.Test(new PunctuationCharTester(),'.','A');
		}

		[Test]
		public void RangeCharTest()
		{
			Helper.Test(new RangeCharTester('a','z'),'c','1');
		}

		[Test]
		public void SeparatorCharTest()
		{
			Helper.Test(new SeparatorCharTester(),' ','1');
		}

		[Test]
		public void SymbolCharTest()
		{
			Helper.Test(new SymbolCharTester(),'+','1');
		}

		[Test]
		public void UpperCharTest()
		{
			Helper.Test(new UpperCharTester(),'A','a');
		}

		[Test]
		public void WhiteSpaceCharTest()
		{
			Helper.Test(new WhiteSpaceCharTester(),' ','a');
		}

	}
}
