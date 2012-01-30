using System;
using NUnit.Framework;
using Palaso.Reporting;
using System.Windows.Forms;

namespace PalasoUIWindowsForms.Tests.ErrorReporting
{

	[TestFixture]
	[Category("SkipOnTeamCity")]
	public class ErrorReportingTests
	{

		[Test, Ignore("By hand only")]
		public void NotifyUserOfProblem_Message()
		{
			string message = "Oh no! This is quite a long message to see if it will wrap so I will have to keep typing to see if this will work now";
			ErrorReport.NotifyUserOfProblem(message);
		}

		[Test, Ignore("By hand only")]
		public void NotifyUserOfProblem_OncePerSession()
		{
			string message = "Oh no! This is quite a long message to see if it will wrap so I will have to keep typing to see if this will work now";
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);

			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);
		}
	}
}
