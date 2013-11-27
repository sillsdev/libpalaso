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
	}
}
