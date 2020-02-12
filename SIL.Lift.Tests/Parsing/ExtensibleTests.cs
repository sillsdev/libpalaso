using System;
using NUnit.Framework;
using SIL.Lift.Parsing;

namespace SIL.Lift.Tests.Parsing
{
	[TestFixture]
	public class ExtensibleTests
	{
		[Test]
		public void Construct_CreationTimeRecent()
		{
			Extensible e = new Extensible();
			// created less than one second ago
			Assert.Less(DateTime.UtcNow - e.CreationTime, new TimeSpan(0, 0, 1));
		}

		[Test]
		public void Construct_CreationTimeSameAsModifiedTime()
		{
			Extensible e = new Extensible();
			Assert.IsTrue(e.CreationTime == e.ModificationTime);
		}
		[Test]
		public void Construct_CreationTimeIsUtc()
		{
			Extensible e = new Extensible();
			Assert.AreEqual(DateTimeKind.Utc, e.CreationTime.Kind);
		}
		[Test]
		public void Construct_ModificationTimeIsUtc()
		{
			Extensible e = new Extensible();
			Assert.AreEqual(DateTimeKind.Utc, e.ModificationTime.Kind);
		}
		[Test]
		public void Construct_GuidIsNotEmpty()
		{
			Extensible e = new Extensible();
			Assert.IsTrue(e.Guid != Guid.Empty);
		}


		[Test]
		public void ParseDateTimeCorrectly_DateOnly()
		{
			DateTime parsedDateTime = Extensible.ParseDateTimeCorrectly("2007-02-03");
			Assert.IsTrue(parsedDateTime.Kind == DateTimeKind.Utc);
			Assert.AreEqual(new DateTime(2007, 2, 3),parsedDateTime);
		}

		[Test]
		public void ParseDateTimeCorrectly_WithTimeZone()
		{
			DateTime parsedDateTime = Extensible.ParseDateTimeCorrectly("2007-02-03T03:01:39+07:00");
			Assert.IsTrue(parsedDateTime.Kind == DateTimeKind.Utc);
			Assert.AreEqual(new DateTime(2007, 2, 2, 20, 1, 39, DateTimeKind.Utc), parsedDateTime);
		}

		[Test]
		public void ParseDateTimeCorrectly_WithTimeZoneAtBeginningOfYear()
		{
			DateTime parsedDateTime = Extensible.ParseDateTimeCorrectly("2005-01-01T01:11:11+8:00");
			Assert.IsTrue(parsedDateTime.Kind == DateTimeKind.Utc);
			Assert.AreEqual(new DateTime(2004, 12, 31, 17, 11, 11, DateTimeKind.Utc), parsedDateTime);
		}

		[Test]
		public void ParseDateTimeCorrectly_NoTimeZone()
		{
			DateTime parsedDateTime = Extensible.ParseDateTimeCorrectly("2007-02-03T03:01:39Z");
			Assert.IsTrue(parsedDateTime.Kind == DateTimeKind.Utc);
			Assert.AreEqual(new DateTime(2007, 2, 3,3,1,39,DateTimeKind.Utc), parsedDateTime);
		}

		[Test]
		public void ParseDateTimeCorrectly_Bad_ThrowsLiftFormatException()
		{
			Assert.Throws<LiftFormatException>(() => Extensible.ParseDateTimeCorrectly("2007-02-03T03:01:39"));
		}
	}
}