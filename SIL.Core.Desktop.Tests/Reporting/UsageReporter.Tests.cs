using System;
using NUnit.Framework;
using SIL.Reporting;

namespace SIL.Tests.Reporting
{
	[TestFixture]
	public class UsageReporterTests
	{
		private class EnvironmentForTest : IDisposable
		{

			public EnvironmentForTest()
			{
				UsageReporter.AppNameToUseInDialogs = "PalasoUnitTest";
				UsageReporter.AppNameToUseInReporting = "PalasoUnitTest";
			}

			public void Dispose()
			{
			}
		}

//        [Test, Ignore("Run by hand")]
//        public void UsageReporterSmokeTest()
//        {
//            UsageReporter.AppNameToUseInDialogs = "PalasoUnitTest";
//            UsageReporter.AppNameToUseInReporting = "PalasoUnitTest";
//			UsageReporter.RecordLaunch();
//        }

//        [Test, Ignore("Run by hand")]
//        public void HttpPost_WithValidArgs_Ok()
//        {
//            using (var e = new EnvironmentForTest())
//            {
//                var parameters = new Dictionary<string, string>();
//                parameters.Add("app", UsageReporter.AppNameToUseInReporting);
//                parameters.Add("version", "test-0.0.0.0");
//                parameters.Add("launches", "1");
//                parameters.Add("user", "testuser");
//
//                string result = UsageReporter.HttpPost("http://www.wesay.org/usage/post.php", parameters);
//                Assert.AreEqual("OK", result);
//            }
//        }
	}
}
