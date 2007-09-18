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

namespace Spart.Tests.Parsers.Composite
{
	using NUnit.Framework;
	using Spart.Parsers;
	using Spart.Parsers.Composite;
	using Spart.Parsers.Primitives;
	using Spart.Scanners;
	using Spart.Parsers.NonTerminal;

	[TestFixture]
	public class AlternativeTest
	{
		[Test]
		public void FirstMatch()
		{
			Rule d = new Rule();
			Rule l = new Rule();
			d.Parser = Prims.Digit;
			l.Parser = Prims.Letter;
			AlternativeParser rp = l|d;
			IScanner scan = Provider.Scanner;
			ParserMatch m = rp.Parse(scan);

			Assertion.Assert(m.Success);
			Assertion.Equals(m.Length,1);
			Assertion.Equals(scan.Offset,1);
		}

		[Test]
		public void SecondMatch()
		{
			Rule d = Rule.AssignParser(null,Prims.Digit);
			Rule l = Rule.AssignParser(null,Prims.Letter);
			AlternativeParser rp = d|l;
			IScanner scan = Provider.Scanner;
			ParserMatch m = rp.Parse(scan);

			Assertion.Assert(m.Success);
			Assertion.Equals(m.Length,1);
			Assertion.Equals(scan.Offset,1);
		}

		[Test]
		public void ThirdMatch()
		{
			Rule d = Rule.AssignParser(null,Prims.Digit);
			Rule l = Rule.AssignParser(null,Prims.Letter);
			AlternativeParser rp = d|d|l;
			IScanner scan = Provider.Scanner;
			ParserMatch m = rp.Parse(scan);

			Assertion.Assert(m.Success);
			Assertion.Equals(m.Length,1);
			Assertion.Equals(scan.Offset,1);
		}

		[Test]
		public void NoMatchMatch()
		{
			Rule d = Rule.AssignParser(null,Prims.Digit);
			AlternativeParser rp = d|d|d;
			IScanner scan = Provider.Scanner;
			ParserMatch m = rp.Parse(scan);

			Assertion.Assert(!m.Success);
			Assertion.Equals(scan.Offset,0);
		}
	}
}
