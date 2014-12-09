using System;
using System.Collections.Generic;
using Palaso.Lift;

using NUnit.Framework;
using Palaso.TestUtilities;

namespace Palaso.Lift.Tests
{
	internal class TestClass: PalasoDataObject
	{
		public TestClass(PalasoDataObject parent): base(parent) {}

		public override bool IsEmpty
		{
			get { throw new NotImplementedException(); }
		}
	}

	[TestFixture]
	public class PalasoObjectTests
	{
		[Test]
		public void NoProperties_NoFlag()
		{
			TestClass t = new TestClass(null);
			Assert.IsFalse(t.GetHasFlag("foo"));
		}

		[Test]
		public void LackingProperty_NoFlag()
		{
			TestClass t = new TestClass(null);
			t.SetFlag("notfoo");
			Assert.IsFalse(t.GetHasFlag("foo"));
		}

		[Test]
		public void AfterSettingReportsTrue()
		{
			TestClass t = new TestClass(null);
			t.SetFlag("foo");
			Assert.IsTrue(t.GetHasFlag("foo"));
		}

		[Test]
		public void SetPropertiesToTrue()
		{
			TestClass t = new TestClass(null);
			t.SetFlag("foo");
			Assert.IsTrue(t.GetHasFlag("foo"));
		}
	}
}