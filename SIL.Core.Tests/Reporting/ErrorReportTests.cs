// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;
using SIL.Email;
using SIL.Reporting;

namespace SIL.Tests.Reporting
{
	[TestFixture]
	public class ErrorReportTests
	{
		[Test]
		[Explicit("By hand only")]
		public void ReportNonFatalException()
		{
			ErrorReport.EmailAddress = "pretend@8ksdfj83jls8.com";
			ErrorReport.ReportNonFatalException(new ApplicationException("testing"));
		}

		[Test]
		[Platform(Include = "Win", Reason = "Windows specific test")]
		public void Properties_WindowsDoesNotContainDesktopEnvironment()
		{
			// SUT
			ErrorReport.AddStandardProperties();

			// Verify
			// (ErrorReport.Properties is a string dictionary which converts all keys to lowercase)
			Assert.That(ErrorReport.Properties.Keys, Has.No.Member("DesktopEnvironment"));
		}

		[Test]
		[Platform(Include = "Win", Reason = "Windows specific test")]
		public void GetStandardProperties_WindowsDoesNotContainDesktopEnvironment()
		{
			// SUT
			var props = ErrorReport.GetStandardProperties();

			// Verify
			Assert.That(props.Keys, Has.No.Member("DesktopEnvironment"));
		}

		[Test]
		[Platform(Include = "Linux", Reason = "Linux specific test")]
		public void Properties_ContainDesktopEnvironment()
		{
			// SUT
			ErrorReport.AddStandardProperties();

			// Verify
			// (ErrorReport.Properties is a string dictionary which converts all keys to lowercase)
			Assert.That(ErrorReport.Properties.Keys, Has.Member("DesktopEnvironment"));
		}

		[Test]
		[Platform(Include = "Linux", Reason = "Linux specific test")]
		public void GetStandardProperties_ContainDesktopEnvironment()
		{
			// SUT
			var props = ErrorReport.GetStandardProperties();

			// Verify
			Assert.That(props.Keys, Has.Member("DesktopEnvironment"));
		}
	}
}
