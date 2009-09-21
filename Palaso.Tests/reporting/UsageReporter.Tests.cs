using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Palaso.Reporting;

namespace Palaso.Tests.reporting
{
	[TestFixture]
	public class UsageReporterTests
	{
		[Test, Ignore("Run by hand")]
		public void UsageReporterSmokeTest()
		{
			UsageReporter.AppNameToUseInDialogs = "PalasoUnitTest";
			UsageReporter.AppNameToUseInReporting = "PalasoUnitTest";
			UsageReporter.AppReportingSettings = new ReportingSettings();
			UsageReporter.RecordLaunch();
		}
	}
}
