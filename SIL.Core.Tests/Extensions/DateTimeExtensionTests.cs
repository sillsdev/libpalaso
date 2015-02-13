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
