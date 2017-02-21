using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Palaso.Extensions;

namespace Palaso.Tests.Extensions
{
	[TestFixture]
	public class DateTimeExtensionTests
	{
		[Test]
		public void ToISO8601DateAndUTCTimeString_ReturnsTimeInUTC()
		{
			var dateTime = DateTime.Parse("2017-02-20T12:08:09+03:00");
			Assert.That(dateTime.ToISO8601DateAndUTCTimeString(),
				Is.EqualTo("2017-02-20T09:08:09Z"));
		}

		[TestCase(DateTimeKind.Local)]
		[TestCase(DateTimeKind.Utc)]
		[TestCase(DateTimeKind.Unspecified)]
		public void ToISO8601DateAndUTCTimeString_DifferentInputKind_ReturnsTimeInUTC(DateTimeKind kind)
		{
			var dateTime = new DateTime(2017, 02, 20, 17, 18, 19, kind);
			Assert.That(dateTime.ToISO8601DateAndUTCTimeString(),
				Is.EqualTo(dateTime.ToUniversalTime()
					.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)));
		}

		[Test]
		public void ToISO8601DateAndUTCTimeString_CultureUsesPeriods_OutputsWithColons()
		{
			//simulate the culture that was messing us up
			var culture = new CultureInfo("en-US");
			culture.DateTimeFormat.TimeSeparator = ".";
			Thread.CurrentThread.CurrentCulture = culture;

			const string whenSample = "2008-07-01T16:29:23Z";
			var when = DateTimeExtensions.ParseISO8601DateTime(whenSample);
			Assert.AreEqual(whenSample, when.ToISO8601DateAndUTCTimeString());
		}

		[Test]
		public void IsISO8601Date_ReturnsCorrectValue()
		{
			Assert.True("2014-01-01".IsISO8601Date());
			Assert.True("2014-12-31".IsISO8601Date());
			Assert.False("12/12/2014".IsISO8601Date());
			Assert.False("12 DEC 2014".IsISO8601Date());
			Assert.False("2014-13-01".IsISO8601Date());
			Assert.False("2014-12-32".IsISO8601Date());
		}
	}
}
