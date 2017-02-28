using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using SIL.Extensions;

namespace SIL.Tests.Extensions
{
	[TestFixture]
	public class DateTimeExtensionTests
	{
		[Test]
		public void ToISO8601TimeFormatWithUTCString_ReturnsTimeInUTC()
		{
			var dateTime = DateTime.Parse("2017-02-20T12:08:09+03:00");
			Assert.That(dateTime.ToISO8601TimeFormatWithUTCString(),
				Is.EqualTo("2017-02-20T09:08:09Z"));
		}

		[TestCase(DateTimeKind.Local)]
		[TestCase(DateTimeKind.Utc)]
		[TestCase(DateTimeKind.Unspecified)]
		public void ToISO8601TimeFormatWithUTCString_DifferentInputKind_ReturnsTimeInUTC(DateTimeKind kind)
		{
			var dateTime = new DateTime(2017, 02, 20, 17, 18, 19, kind);
			Assert.That(dateTime.ToISO8601TimeFormatWithUTCString(),
				Is.EqualTo(dateTime.ToUniversalTime()
					.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)));
		}

		[Test]
		public void ToISO8601XTimeString_CultureUsesPeriods_OutputsWithColons()
		{
			//simulate the culture that was messing us up
			var culture = new CultureInfo("en-US");
			culture.DateTimeFormat.TimeSeparator = ".";
			Thread.CurrentThread.CurrentCulture = culture;

			string whenSample = "2008-07-01T16:29:23Z";
			var when = DateTimeExtensions.ParseISO8601DateTime(whenSample);
			Assert.AreEqual(whenSample, when.ToISO8601TimeFormatWithUTCString());

			whenSample = "2008-07-01T16:29:23";
			when = DateTimeExtensions.ParseISO8601DateTime(whenSample);
			Assert.AreEqual(whenSample, when.ToISO8601TimeFormatNoTimeZoneString());

			whenSample = "2008-07-01";
			when = DateTimeExtensions.ParseISO8601DateTime(whenSample);
			Assert.AreEqual(whenSample, when.ToISO8601TimeFormatDateOnlyString());
		}

		[Test]
		public void IsISO8601Date_ReturnsCorrectValue()
		{
			Assert.True(DateTimeExtensions.IsISO8601Date("2014-01-01"));
			Assert.True(DateTimeExtensions.IsISO8601Date("2014-12-31"));
			Assert.False(DateTimeExtensions.IsISO8601Date("2014/12/31"));
			Assert.False(DateTimeExtensions.IsISO8601Date("12/12/2014"));
			Assert.False(DateTimeExtensions.IsISO8601Date("12 DEC 2014"));
			Assert.False(DateTimeExtensions.IsISO8601Date("2014-13-01"));
			Assert.False(DateTimeExtensions.IsISO8601Date("2014-12-32"));
		}

		[Test]
		public void IsISO8601DateTime_ReturnsCorrectValue()
		{
			Assert.True(DateTimeExtensions.IsISO8601DateTime("2014-12-31T13:56:29+0000"));
			Assert.True(DateTimeExtensions.IsISO8601DateTime("2014-12-31T13:56:29+2000"));
			Assert.True(DateTimeExtensions.IsISO8601DateTime("2014-12-31T13:56:29-2000"));
			Assert.True(DateTimeExtensions.IsISO8601DateTime("2014-12-31T13:56:29Z"));
			Assert.True(DateTimeExtensions.IsISO8601DateTime("2014-12-31T13:56:29"));
			Assert.True(DateTimeExtensions.IsISO8601DateTime("2014-12-31T13:56:29-20:00"));
			Assert.True(DateTimeExtensions.IsISO8601DateTime("2014-12-31T13:56:29+20:00"));
			Assert.False(DateTimeExtensions.IsISO8601DateTime("2014/12/31T13:56:29-2000"));
			Assert.False(DateTimeExtensions.IsISO8601DateTime("2014-12-31 13:56:29-2000"));
			Assert.False(DateTimeExtensions.IsISO8601DateTime("2014-12-31T13:56:29 0000"));
			Assert.False(DateTimeExtensions.IsISO8601DateTime("2014-12-31T13:56:29ZZ"));
			Assert.False(DateTimeExtensions.IsISO8601DateTime("2014-12-31T13:56:29Z0000"));
			Assert.False(DateTimeExtensions.IsISO8601DateTime("2014-12-31T13:56:29 0000"));
			Assert.False(DateTimeExtensions.IsISO8601DateTime("2014-12-31 04:45:29"));
			Assert.False(DateTimeExtensions.IsISO8601DateTime("2014/12/32T04:45:29"));
			Assert.False(DateTimeExtensions.IsISO8601DateTime("2014-13-01T04:45:29"));
			Assert.False(DateTimeExtensions.IsISO8601DateTime("2014-12-32T04:45:29"));

		}

		[Test]
		public void ParseISO8601DateTime()
		{
			var defaultTime = new DateTime();
			Assert.That(DateTimeExtensions.ParseISO8601DateTime("0001-01-01T00:00:00+00:00"), Is.EqualTo(defaultTime));
			Assert.That(DateTimeExtensions.ParseISO8601DateTime("0001-01-01T00:00:00+0000"), Is.EqualTo(defaultTime));
			Assert.That(DateTimeExtensions.ParseISO8601DateTime("0001-01-01T00:00:00Z"), Is.EqualTo(defaultTime));
			Assert.That(DateTimeExtensions.ParseISO8601DateTime("0001-01-01T00:00:00"), Is.EqualTo(defaultTime));
			Assert.That(DateTimeExtensions.ParseISO8601DateTime("0001-01-01"), Is.EqualTo(defaultTime));

			var expectedTime = new DateTime(2012, 02, 29, 12, 30, 45);
			Assert.That(DateTimeExtensions.ParseISO8601DateTime("2012-02-29T12:30:45+0000"), Is.EqualTo(expectedTime));
			Assert.That(DateTimeExtensions.ParseISO8601DateTime("2012-02-29T12:30:45+00:00"), Is.EqualTo(expectedTime));
			Assert.That(DateTimeExtensions.ParseISO8601DateTime("2012-02-29T00:30:45-1200"), Is.EqualTo(expectedTime));
			Assert.That(DateTimeExtensions.ParseISO8601DateTime("2012-02-29T00:30:45-12:00"), Is.EqualTo(expectedTime));
		}

	}
}
