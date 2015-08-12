// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using NUnit.Framework;
#if !__MonoCS__
using Palaso.Email;
#endif
using Palaso.Reporting;

namespace Palaso.Tests.reporting
{
	[TestFixture]
	public class ErrorReportTests
	{
		[Test]
		[Ignore("by hand only")]
		public void ReportNonFatalException()
		{
			ErrorReport.EmailAddress = "pretend@8ksdfj83jls8.com";
			ErrorReport.ReportNonFatalException(new ApplicationException("testing"));
		}

#if !__MonoCS__
		[Test]
		[Ignore("by hand only")]
		public void TestSendEmail()
		{
			MAPI x = new MAPI();
			x.AddRecipientTo("pretend@8ksdfj83jls8.com");
			x.SendMailDirect("test", "testbody");
		}
#endif

		[Test]
		[Platform(Include = "Windows", Reason = "Windows specific test")]
		public void Properties_WindowsDoesNotContainDesktopEnvironment()
		{
			// SUT
			ErrorReport.AddStandardProperties();

			// Verify
			// (ErrorReport.Properties is a string dictionary which converts all keys to lowercase)
			Assert.That(ErrorReport.Properties.Keys, Has.No.Member("desktopenvironment"));
		}

		[Test]
		[Platform(Include = "Windows", Reason = "Windows specific test")]
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
			Assert.That(ErrorReport.Properties.Keys, Has.Member("desktopenvironment"));
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

		[Test]
		[Platform(Include = "Linux", Reason = "Linux specific test")]
		[TestCase("Unity", null, "ubuntu", null, Result = "Unity (ubuntu)", TestName = "Unity")]
		[TestCase("Unity", "/usr/share/ubuntu:/usr/share/gnome:/usr/local/share/:/usr/share/",
			"ubuntu", null, Result = "Unity (ubuntu)", TestName = "Unity with dataDir")]
		[TestCase("Unity", null, "ubuntu", "session-1",
			Result = "Unity (ubuntu [display server: Mir])", TestName = "Unity with Mir")]
		[TestCase("GNOME", null, "gnome-shell", null, Result = "GNOME (gnome-shell)"
			, TestName = "Gnome shell")]
		[TestCase(null, "/usr/share/ubuntu:/usr/share/kde:/usr/local/share/:/usr/share/",
			"kde-plasma", null, Result = "KDE (kde-plasma)", TestName = "KDE on Ubuntu 12_04")]
		public string GetStandardProperties_SimulateDesktopEnvironments(string currDesktop,
			string dataDirs, string gdmSession, string mirServerName)
		{
			// See http://askubuntu.com/a/227669 for actual values on different systems

			// Setup
			Environment.SetEnvironmentVariable("XDG_CURRENT_DESKTOP", currDesktop);
			Environment.SetEnvironmentVariable("XDG_DATA_DIRS", dataDirs);
			Environment.SetEnvironmentVariable("GDMSESSION", gdmSession);
			Environment.SetEnvironmentVariable("MIR_SERVER_NAME", mirServerName);

			// SUT
			var props = ErrorReport.GetStandardProperties();

			// Verify
			return props["DesktopEnvironment"];
		}
	}
}
