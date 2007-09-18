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

namespace Spart.Tests.Parsers
{
	using NUnit.Framework;
	using Spart.Scanners;
	using Spart.Parsers;

	[TestFixture]
	public class ParserMatchTest
	{
		public ParserMatch NoMatch
		{
			get
			{
				return new ParserMatch(Provider.Scanner,0,-1);
			}
		}

		public ParserMatch EmptyMatch
		{
			get
			{
				return new ParserMatch(Provider.Scanner,0,0);
			}
		}

		[Test]
		public void NoMatchTest()
		{
			Assertion.Equals(NoMatch.Success,false);
		}

		[Test]
		[ExpectedException(typeof(Exception))]
		public void NoMatchEmpty()
		{
			bool b=NoMatch.Empty;
		}

		[Test]
		[ExpectedException(typeof(Exception))]
		public void NoMatchValue()
		{
			String o = NoMatch.Value;
		}

	}
}
