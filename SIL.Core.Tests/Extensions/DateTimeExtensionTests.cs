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
		public void ToISO8601TimeFormatWithUTCString_DifferentInputKind_ReturnsTimeInUTC(
			DateTimeKind kind)
		{
			var dateTime = new DateTime(2017, 02, 20, 17, 18, 19, kind);
			Assert.That(dateTime.ToISO8601TimeFormatWithUTCString(),
				Is.EqualTo(dateTime.ToUniversalTime()
					.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)));
		}

		[TestCase(1482)]
		[TestCase(0939)]
		[NonParallelizable]
		public void ToISO8601TimeFormatWithUTCString_BuddhistDate_ReturnsTimeInUTC(int year)
		{
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				// Temporarily set system locale to use Buddhist date system
				var buddhistCulture = new CultureInfo("th-TH");
				buddhistCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
				Thread.CurrentThread.CurrentCulture = buddhistCulture;

				// Create a DateTime in the Buddhist calendar
				var dateTime = new DateTime(year, 3, 9, 10, 28, 39, DateTimeKind.Local);

				Assert.That(dateTime.ToISO8601TimeFormatWithUTCString(),
					Is.EqualTo(dateTime.ToUniversalTime()
						.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)));
			}
			finally
			{
				// Reset system locale to the original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
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

		[TestCase("2014-01-01", ExpectedResult = true)]
		[TestCase("2014-12-31", ExpectedResult = true)]
		[TestCase("2014/12/31", ExpectedResult = false)]
		[TestCase("12/12/2014", ExpectedResult = false)]
		[TestCase("12 DEC 2014", ExpectedResult = false)]
		[TestCase("2014-13-01", ExpectedResult = false)]
		[TestCase("2014-12-32", ExpectedResult = false)]
		public bool IsISO8601Date_ReturnsCorrectValue(string date)
		{
			return DateTimeExtensions.IsISO8601Date(date);
		}

		// We used to test with a timezone offset of 20 hours instead of 12. This works with .NET
		// Framework, but fails with .NET Core and with Mono 6. The exception we get on these
		// platforms mentions that the time zone offset must be within plus or minus 14 hours -
		// which makes sense.
		[TestCase("2014-12-31T13:56:29+0000", ExpectedResult = true)]
		[TestCase("2014-12-31T13:56:29+1200", ExpectedResult = true)]
		[TestCase("2014-12-31T13:56:29-1200", ExpectedResult = true)]
		[TestCase("2014-12-31T13:56:29Z", ExpectedResult = true)]
		[TestCase("2014-12-31T13:56:29", ExpectedResult = true)]
		[TestCase("2014-12-31T13:56:29-12:00", ExpectedResult = true)]
		[TestCase("2014-12-31T13:56:29+12:00", ExpectedResult = true)]
		[TestCase("2014/12/31T13:56:29-1200", ExpectedResult = false)]
		[TestCase("2014-12-31 13:56:29-1200", ExpectedResult = false)]
		[TestCase("2014-12-31T13:56:29 0000", ExpectedResult = false)]
		[TestCase("2014-12-31T13:56:29ZZ", ExpectedResult = false)]
		[TestCase("2014-12-31T13:56:29Z0000", ExpectedResult = false)]
		[TestCase("2014-12-31T13:56:29 0000", ExpectedResult = false)]
		[TestCase("2014-12-31 04:45:29", ExpectedResult = false)]
		[TestCase("2014/12/32T04:45:29", ExpectedResult = false)]
		[TestCase("2014-13-01T04:45:29", ExpectedResult = false)]
		[TestCase("2014-12-32T04:45:29", ExpectedResult = false)]
		public bool IsISO8601DateTime_ReturnsCorrectValue(string dateTime)
		{
			return DateTimeExtensions.IsISO8601DateTime(dateTime);
		}

		[TestCase("0001-01-01T00:00:00+00:00")]
		[TestCase("0001-01-01T00:00:00+0000")]
		[TestCase("0001-01-01T00:00:00Z")]
		[TestCase("0001-01-01T00:00:00")]
		[TestCase("0001-01-01")]
		public void ParseISO8601DateTime_DefaultTime(string dateTime)
		{
			var defaultTime = new DateTime();
			Assert.That(DateTimeExtensions.ParseISO8601DateTime(dateTime),
				Is.EqualTo(defaultTime));
		}

		[TestCase("2012-02-29T12:30:45+0000")]
		[TestCase("2012-02-29T12:30:45+00:00")]
		[TestCase("2012-02-29T00:30:45-1200")]
		[TestCase("2012-02-29T00:30:45-12:00")]
		public void ParseISO8601DateTime_ExpectedTime(string dateTime)
		{
			Assert.That(DateTimeExtensions.ParseISO8601DateTime(dateTime),
				Is.EqualTo(new DateTime(2012, 02, 29, 12, 30, 45)));
		}

		[TestCase("9/3/2025 0:00:00", ExpectedResult = "1482-03-09")]
		[TestCase("14/4/1482", ExpectedResult = "0939-04-14")]
		[TestCase("9/3/1800 0:01:00", ExpectedResult = "1257-03-09")]
		[NonParallelizable]
		public string ParseDateTimePermissivelyWithException_WithThaiBuddhistCalendar_ReturnsDateWithCorrectYear(string input)
		{
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				// Temporarily set system locale to use Buddhist date system
				var buddhistCulture = new CultureInfo("th-TH");
				buddhistCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
				Thread.CurrentThread.CurrentCulture = buddhistCulture;

				return input.ParseDateTimePermissivelyWithException()
					.ToISO8601TimeFormatDateOnlyString();
			}
			finally
			{
				// Reset system locale to the original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		[TestCase("19/10/2025 0:00:00", ExpectedResult = "2025-10-19")]
		[TestCase("14/4/1482", ExpectedResult = "1482-04-14")]
		[TestCase("13/3/1800 0:01:00", ExpectedResult = "1800-03-13")]
		public string ParseDateTimePermissivelyWithException_WithGregorianCalendar_ReturnsDateWithCorrectYear(string input)
		{
			// First, make sure we're using a Gregorian calendar
			if (!(Thread.CurrentThread.CurrentCulture.DateTimeFormat.Calendar is GregorianCalendar))
				Assert.Ignore("This test requires the current culture to use a Gregorian calendar.");

			return input.ParseDateTimePermissivelyWithException()
				.ToISO8601TimeFormatDateOnlyString();
		}

		[TestCase("19102025 0:00:00")]
		[TestCase("14&4&1482")]
		[TestCase("@13:00.T")]
		public void ParseDateTimePermissivelyWithException_UnknownFormat_ThrowsApplicationException(string input)
		{
			Assert.That(input.ParseDateTimePermissivelyWithException,
				Throws.TypeOf<ApplicationException>().With.InnerException.TypeOf<FormatException>());
		}
	}
}