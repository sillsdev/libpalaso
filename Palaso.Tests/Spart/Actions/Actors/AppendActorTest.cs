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

using System.Collections;
using NUnit.Framework;
using Spart.Actions;
using Spart.Parsers;
using Spart.Parsers.Primitives;
using Spart.Scanners;

namespace Spart.Tests.Actions.Actors
{
	[TestFixture]
	public class AppendActorTest
	{
		public ArrayList NewList
		{
			get { return new ArrayList(); }
		}

		[Test]
		public void AppendString()
		{
			IScanner scanner = Provider.NewScanner;
			StringParser parser = new StringParser(Provider.Text);
			IList list = NewList;
			parser.Act += ActionHandlers.Append(list);
			parser.Parse(scanner);

			Assert.AreEqual(list.Count, 1);
			Assert.AreEqual(list[0], Provider.Text);
		}

		[Test]
		public void AppendStringShorthand()
		{
			IScanner scanner = Provider.NewScanner;
			ArrayList list = NewList;
			Parser parser = new StringParser(Provider.Text)[ActionHandlers.Append(list)];
			parser.Parse(scanner);

			Assert.AreEqual(list.Count, 1);
			Assert.AreEqual(list[0], Provider.Text);

		}
	}
}