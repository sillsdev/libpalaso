
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Palaso.Email;
using Palaso.Reporting;

namespace Palaso.Tests.reporting
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
