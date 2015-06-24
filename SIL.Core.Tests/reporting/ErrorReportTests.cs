using System;
using NUnit.Framework;
using SIL.Email;
using SIL.Reporting;

namespace SIL.Tests.reporting
{
	[TestFixture]
	public class ErrorReportTests
	{
		[Test, Ignore("by hand only")]
		public void Test()
		{
			ErrorReport.EmailAddress = "pretend@8ksdfj83jls8.com";
			ErrorReport.ReportNonFatalException(new ApplicationException("testing"));
		}
#if !MONO
		[Test, Ignore("by hand only")]
		public void TestSendEmail()
		{
			MAPI x = new MAPI();
			x.AddRecipientTo("pretend@8ksdfj83jls8.com");
			x.SendMailDirect("test", "testbody");
		}
#endif
	}
}
