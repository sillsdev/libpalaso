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

		[Test]
		public void NotifyUserOfProblem_Message()
		{
			string message = "Oh no!";
			ErrorReport.NotifyUserOfProblem(message);
		}

		[Test]
		public void NotifyUserOfProblem_OncePerSession()
		{
			string message = "Oh no!";
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);

			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);
		}
	}
}
