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

		[TestCase("2025-05-12")]
		[TestCase("1482-04-14")]
		[TestCase("2033-01-02")]
		public void ParseModernPastDateTimePermissivelyWithException_GregorianISO8601WithThaiBuddhistCalendar_ReturnsDateWithCorrectYear(
			string inputGregorian)
		{
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				// Temporarily set system locale to use Buddhist date system
				var buddhistCulture = new CultureInfo("th-TH");
				buddhistCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
				Thread.CurrentThread.CurrentCulture = buddhistCulture;

				var result = inputGregorian.ParseModernPastDateTimePermissivelyWithException();
				var inputDateOnly = inputGregorian.Split(' ')[0];
				var inputDateParts = inputDateOnly.Split('-');
				Assert.That(inputDateParts.Length, Is.EqualTo(3), "Sanity check");
				// Note that even when the current culture is Thai/Buddhist, the year,
				// month and day values in the DateTime object are still Gregorian.
				Assert.That(result.Year.ToString(), Is.EqualTo(inputDateParts[0]));
				Assert.That(result.Month, Is.EqualTo(int.Parse(inputDateParts[1])));
				Assert.That(result.Day, Is.EqualTo(int.Parse(inputDateParts[2])));
				Assert.That(result.ToISO8601TimeFormatDateOnlyString(),
					Is.EqualTo(inputDateOnly));

				var expectedResultFormattedAsBuddhistDate =
					$"{result.Day}/{result.Month}/{result.Year + 543}";
				Assert.That(result.ToShortDateString(),
					Is.EqualTo(expectedResultFormattedAsBuddhistDate));
			}
			finally
			{
				// Reset system locale to the original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		/// <summary>
		/// Input dates are not formatted as valid ISO8601 dates. Method should guess which
		/// calendar to use based on year and current culture. If year is in the "modern" range
		/// for the Gregorian calendar, it should be treated as a Gregorian date. If year is
		/// older or way in the future, it should be treated as a Buddhist date. Thai culture
		/// normally formats dates as dd/MM/yyyy, so that is how these dates should be
		/// interpreted.
		/// </summary>
		/// <param name="input">The ambiguous input date, not formatted as ISO 8601</param>
		/// <param name="expectedToInterpretAsGregorianYear">Flag indicating whether algorithm is
		/// expected to guess that the input date represents a year in the Gregorian calendar
		/// </param>
		/// <param name="expectedResultFormattedAsBuddhistDate"></param>
		[TestCase("9/3/2025 0:00:00", true, "9/3/2568")]
		[TestCase("2-2-1920", true, "2/2/2463")]
		[TestCase("14/4/1543", false, "14/4/1543")]
		[TestCase("3/9/1257 0:01:00", false, "3/9/1257")]
		[TestCase("9/3/2568 0:01:00", false, "9/3/2568")]
		[NonParallelizable]
		public void ParseModernPastDateTimePermissivelyWithException_WithThaiBuddhistCalendar_ReturnsDateWithCorrectYear(
			string input, bool expectedToInterpretAsGregorianYear,
			string expectedResultFormattedAsBuddhistDate)
		{
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				// Temporarily set system locale to use Buddhist date system
				var buddhistCulture = new CultureInfo("th-TH");
				buddhistCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
				Thread.CurrentThread.CurrentCulture = buddhistCulture;

				var result = input.ParseModernPastDateTimePermissivelyWithException();
				Assert.That(result.ToShortDateString(),
					Is.EqualTo(expectedResultFormattedAsBuddhistDate));
				var inputDateOnly = input.Split(' ')[0];
				var inputDateParts = inputDateOnly.Split('/', '-');
				Assert.That(inputDateParts.Length, Is.EqualTo(3), "Sanity check");
				// Note that even when the current culture is Thai/Buddhist, the unformatted year,
				// month and day stored in the DateTime object are still Gregorian.
				var expectedGregorianYear = expectedToInterpretAsGregorianYear
					? inputDateParts[2]
					: (int.Parse(inputDateParts[2]) - 543).ToString();
				Assert.That(result.Year.ToString(), Is.EqualTo(expectedGregorianYear));
				Assert.That(result.Month.ToString(), Is.EqualTo(inputDateParts[1]));
				Assert.That(result.Day.ToString(), Is.EqualTo(inputDateParts[0]));
				Assert.That(result.ToISO8601TimeFormatDateOnlyString(), Is.EqualTo(
					$"{int.Parse(expectedGregorianYear):D4}-{int.Parse(inputDateParts[1]):D2}-{int.Parse(inputDateParts[0]):D2}"));
			}
			finally
			{
				// Reset system locale to the original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		/// <summary>
		/// For these test cases, we will supply a future date (beyond reasonableMax) and pass
		/// a reasonableMin that is more than 543 years ago. Thus, we expect the algorithm to guess
		/// that these are (ancient) Buddhist dates as opposed to future Gregorian dates.
		/// </summary>
		/// <remarks>Given the non-Thai (probably usually English) month name when using the
		/// "dd MMM yyyy" format, it's slightly surprising that the Thai/Buddhist locale can parse
		/// it, but apparently it can.</remarks>
		[TestCase("dd MMM yyyy")] // Medium pattern (e.g. 14 May 2025)
		[TestCase("dd/MM/yyyy")] // Thai/European-style numeric date, zero-padded (e.g. 14/05/2025)
		[TestCase("d/M/yyyy")] // Thai/European-style numeric date (e.g. 14/5/2025)
		[TestCase("d-M-yyyy")] // Thai/European-style numeric date with dashes (e.g. 14-5-2025)
		[NonParallelizable]
		public void ParseDateTimePermissivelyWithException_NearFutureDatesWithThaiBuddhistCalendar_ReturnsDateWithPastYear(string inputFormat)
		{
			var futureDate = DateTime.Today.AddDays(5);
			string input = futureDate.ToString(inputFormat);
			int expectedYear = futureDate.Year - 543;
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				// Temporarily set system locale to use Buddhist date system
				var buddhistCulture = new CultureInfo("th-TH");
				buddhistCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
				Thread.CurrentThread.CurrentCulture = buddhistCulture;

				var reasonableMin = new DateTime(1481, 1, 1);
				var reasonableMax = DateTime.Today + TimeSpan.FromDays(4);
				var result = input.ParseDateTimePermissivelyWithException(reasonableMin,
					reasonableMax);
				Assert.That(result.Year, Is.EqualTo(expectedYear));
			}
			finally
			{
				// Reset system locale to the original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		/// <summary>
		/// For these test cases, we will supply a future date and pass a reasonableMin that is
		/// more than 543 years ago. Thus, we expect the algorithm to guess that these are
		/// (ancient) Buddhist dates as opposed to future Gregorian dates.
		/// </summary>
		/// <remarks>Given the non-Thai (probably usually English) month name when using the
		/// "dd MMM yyyy" format, it's slightly surprising that the Thai/Buddhist locale can parse
		/// it, but apparently it can.</remarks>
		[TestCase("dd MMM yyyy")] // Medium pattern (e.g. 14 May 2025)
		[TestCase("dd/MM/yyyy")] // Thai/European-style numeric date, zero-padded (e.g. 14/05/2025)
		[TestCase("d/M/yyyy")] // Thai/European-style numeric date (e.g. 14/5/2025)
		[TestCase("d-M-yyyy")] // Thai/European-style numeric date with dashes (e.g. 14-5-2025)
		public void ParsePastDateTimePermissivelyWithException_NearFutureDatesWithThaiBuddhistCalendar_ReturnsDateWithPastYear(string inputFormat)
		{
			// Set the calendar in an isolated thread to avoid any cross test effects
			var futureDate = DateTime.Today.AddDays(2);
			var input = futureDate.ToString(inputFormat);
			var expectedYear = futureDate.Year - 543;
			Exception isolatedTestException = null;
			DateTime? result = null;
			var thread = new Thread(() =>
			{
				try
				{
					// Temporarily set system locale to use Buddhist date system
					var buddhistCulture = new CultureInfo("th-TH")
					{
						DateTimeFormat = {
							Calendar = new ThaiBuddhistCalendar()
						}
					};
					Thread.CurrentThread.CurrentCulture = buddhistCulture;

					result = input.ParsePastDateTimePermissivelyWithException();
				}
				catch (Exception e)
				{
					isolatedTestException = e;
				}
			});
			thread.Start();
			thread.Join();
			Assert.That(isolatedTestException, Is.Null, $"Test failed with exception: {isolatedTestException?.Message}");
			Assert.That(result?.Year, Is.EqualTo(expectedYear), $"Should have returned a past year");

		}

		/// <summary>
		/// For these test cases, we will supply a future date (beyond reasonableMax). However,
		/// these are date formats that cannot be parsed in Thai/Buddhist culture, so we expect
		/// them to be interpreted as Gregorian dates.
		/// </summary>
		[TestCase("d")] // Short date pattern (e.g. 14/5/2025)
		[TestCase("D")] // Long date pattern (e.g. Wednesday, 14 May 2025)
		[TestCase("dddd, dd MMMM yyyy")] // Full long format (e.g. Wednesday, 14 May 2025)
		[TestCase("M/d/yyyy")] // US-style numeric date (e.g. 5/14/2025)
		[TestCase("M-d-yyyy")] // US-style numeric date with dashes (e.g. 5-14-2025)
		[TestCase("MM/dd/yyyy")] // US-style numeric date, zero-padded (e.g. 05/14/2025)
		[TestCase("MM-dd-yyyy")] // US-style numeric date with dashes, zero-padded (e.g. 05-14-2025)
		[NonParallelizable]
		public void ParseDateTimePermissivelyWithException_NearFutureUSDatesWithThaiBuddhistCalendar_ReturnsDateWithPastYear(string inputFormat)
		{
			var futureDate = DateTime.Today.AddDays(5);
			string input = futureDate.ToString(inputFormat);
			int expectedYear = futureDate.Year;
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				// Temporarily set system locale to use Buddhist date system
				var buddhistCulture = new CultureInfo("th-TH");
				buddhistCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
				Thread.CurrentThread.CurrentCulture = buddhistCulture;

				var reasonableMax = DateTime.Today + TimeSpan.FromDays(4);
				var result = input.ParseDateTimePermissivelyWithException(new DateTime(1900, 1, 1),
					reasonableMax);
				Assert.That(result.Year, Is.EqualTo(expectedYear));
			}
			finally
			{
				// Reset system locale to the original culture
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		/// <summary>
		/// For these test cases, we will supply a future date. However, these are date formats
		/// that cannot be parsed in Thai/Buddhist culture, so we expect them to be interpreted as
		/// Gregorian dates.
		/// </summary>
		[TestCase("d")] // Short date pattern (e.g. 14/5/2025)
		[TestCase("D")] // Long date pattern (e.g. Wednesday, 14 May 2025)
		[TestCase("dddd, dd MMMM yyyy")] // Full long format (e.g. Wednesday, 14 May 2025)
		[TestCase("M/d/yyyy")] // US-style numeric date (e.g. 5/14/2025)
		[TestCase("M-d-yyyy")] // US-style numeric date with dashes (e.g. 5-14-2025)
		[TestCase("MM/dd/yyyy")] // US-style numeric date, zero-padded (e.g. 05/14/2025)
		[TestCase("MM-dd-yyyy")] // US-style numeric date with dashes, zero-padded (e.g. 05-14-2025)
		[NonParallelizable]
		public void ParsePastDateTimePermissivelyWithException_NearFutureUSDatesWithThaiBuddhistCalendar_ReturnsDateWithPastYear(string inputFormat)
		{
			var futureDate = DateTime.Today.AddDays(2);
			string input = futureDate.ToString(inputFormat);
			int expectedYear = futureDate.Year;
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				// Temporarily set system locale to use Buddhist date system
				var buddhistCulture = new CultureInfo("th-TH");
				buddhistCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
				Thread.CurrentThread.CurrentCulture = buddhistCulture;

				var result = input.ParsePastDateTimePermissivelyWithException();
				Assert.That(result.Year, Is.EqualTo(expectedYear));
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
		public string ParseModernPastDateTimePermissivelyWithException_WithGregorianCalendar_ReturnsDateWithCorrectYear(string input)
		{
			// First, make sure we're using a Gregorian calendar
			if (!(Thread.CurrentThread.CurrentCulture.DateTimeFormat.Calendar is GregorianCalendar))
				Assert.Ignore("This test requires the current culture to use a Gregorian calendar.");

			return input.ParseModernPastDateTimePermissivelyWithException()
				.ToISO8601TimeFormatDateOnlyString();
		}

		[TestCase("19102025 0:00:00")]
		[TestCase("14&4&1482")]
		[TestCase("@13:00.T")]
		public void ParseModernPastDateTimePermissivelyWithException_UnknownFormat_ThrowsApplicationException(string input)
		{
			Assert.That(() => input.ParseModernPastDateTimePermissivelyWithException(),
				Throws.TypeOf<ApplicationException>().With.InnerException.TypeOf<FormatException>());
		}
	}
}