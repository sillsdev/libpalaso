// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml.Linq;
using NUnit.Framework;
using SIL.TestUtilities.NUnitExtensions;
using Is = SIL.TestUtilities.NUnitExtensions.Is;

namespace SIL.TestUtilities.Tests.NUnitExtensions
{
	[TestFixture]
	public class IsTests
	{
		[Test]
		public void ValueEqualTo_Equal()
		{
			Assert.That(new SomeClass("foo"), Is.ValueEqualTo(new SomeClass("foo")));
		}

		[Test]
		public void ValueEqualTo_NotEqual()
		{
			Assert.That(new SomeClass("foo"), Is.Not.ValueEqualTo(new SomeClass("bar")));
		}

		[Test]
		public void XmlEqualTo_Equal()
		{
			var xmlString = @"<root><foo id='abc'/></root>";
			Assert.That(XElement.Parse(xmlString), Is.XmlEqualTo(xmlString));
		}

		[Test]
		public void XmlEqualTo_NotEqual()
		{
			Assert.That(XElement.Parse(@"<root><foo id='abc'/></root>"),
				Is.Not.XmlEqualTo(@"<root><somethingother/></root>"));
		}
	}

}