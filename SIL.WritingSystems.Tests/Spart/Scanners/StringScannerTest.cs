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
using Spart.Scanners;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests.Spart.Scanners
{
	[TestFixture]
	public class StringScannerTest
	{
		public String Text
		{
			get
			{
				return Provider.Text;
			}
		}

		public long Offset
		{
			get
			{
				return 5;
			}
		}

		[Test]
		public void Constructor()
		{
			StringScanner scanner = new StringScanner(Text);
			Assert.AreEqual(Text, scanner.InputString);
		}


		[Test]
		public void Constructor2()
		{
			StringScanner scanner = new StringScanner(Text,Offset);
			Assert.AreEqual(Text, scanner.InputString);
			Assert.AreEqual(Offset, scanner.Offset);
		}

		[Test]
		public void Substring()
		{
			StringScanner scanner = new StringScanner(Text,Offset);
			Assert.AreEqual(Text.Substring(3, 6), scanner.Substring(3, 6));
		}

		[Test]
		public void ReadAndPeek()
		{
			StringScanner scanner = new StringScanner(Text);
			int i=0;

			while (!scanner.AtEnd)
			{
				Assert.Less(i, Text.Length);
				Assert.AreEqual(Text[i], scanner.Peek());
				scanner.Read();
				++i;
			}

			Assert.AreEqual(Text.Length, i);
		}

		[Test]
		public void ReadAndPeekOffset()
		{
			StringScanner scanner = new StringScanner(Text,Offset);
			int i=(int)Offset;

			while (!scanner.AtEnd)
			{
				Assert.Less(i, Text.Length);
				Assert.AreEqual(Text[i], scanner.Peek());
				scanner.Read();
				++i;
			}

			Assert.AreEqual(Text.Length, i);
		}

		[Test]
		public void Seek()
		{
			StringScanner scanner = new StringScanner(Text);
			int i=(int)Offset;
			scanner.Seek(Offset);

			while (!scanner.AtEnd)
			{
				Assert.Less(i, Text.Length);
				Assert.AreEqual(Text[i], scanner.Peek());
				scanner.Read();
				++i;
			}

			Assert.AreEqual(Text.Length,i);
		}

		[Test]
		public void Peek_AtEnd_NullChar()
		{
			StringScanner scanner = new StringScanner(Text);
			scanner.Seek(Text.Length);
			Assert.IsTrue(scanner.AtEnd);
			Assert.AreEqual('\0', scanner.Peek());
		}

		[Test]
		public void Read_AtEnd_Throws()
		{
			StringScanner scanner = new StringScanner(Text);
			scanner.Seek(Text.Length);
			Assert.IsTrue(scanner.AtEnd);
			Assert.Throws<InvalidOperationException>(
				() => scanner.Read());
		}

		[Test]
		public void Offset_InitiallyZero()
		{
			StringScanner scanner = new StringScanner(Text);
			Assert.AreEqual(0, scanner.Offset);
		}

		[Test]
		public void Offset_Initialized()
		{
			StringScanner scanner = new StringScanner(Text, 4);
			Assert.AreEqual(4, scanner.Offset);
		}

		[Test]
		public void Offset_IncrementedAfterRead()
		{
			StringScanner scanner = new StringScanner(Text);
			scanner.Read();
			Assert.AreEqual(1,scanner.Offset);
		}

		[Test]
		public void Offset_NotIncrementedAfterPeek()
		{
			StringScanner scanner = new StringScanner(Text);
			scanner.Peek();
			Assert.AreEqual(0, scanner.Offset);

		}


	}
}
