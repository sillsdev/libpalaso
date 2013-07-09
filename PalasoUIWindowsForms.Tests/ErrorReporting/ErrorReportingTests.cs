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
			string message = "Oh no! This is quite a long message to see if it will wrap so I will have to keep typing to see if this will work now. And then some more.";
			ErrorReport.NotifyUserOfProblem(message);
		}

		[Test, Ignore("By hand only")]
		public void NotifyUserOfProblem_OncePerSession()
		{
			ShowOncePerSessionBasedOnExactMessagePolicy.Reset();
			string message = "Oh no! This is quite a long message to see if it will wrap so I will have to keep typing to see if this will work now. And then some more.";
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);

			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);
		}

		[Test, Ignore("By hand only")]
		public void NotifyUserOfProblem_WithAlternateButton()
		{
			ShowOncePerSessionBasedOnExactMessagePolicy.Reset();
			string message = "Oh no! This is quite a long message to see if it will wrap so I will have to keep typing to see if this will work now. And then some more.";
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
											"&Caller Defined",
											DialogResult.Cancel,
											message);
		}

		[Test, Ignore("By hand only")]
		public void NotifyUserOfProblem_SmallMessage()
		{
			string message = "Oh no!";
			ErrorReport.NotifyUserOfProblem(message);
		}

		[Test, Ignore("By hand only")]
		public void NotifyUserOfProblem_SmallWithAlternateButton()
		{
			ShowOncePerSessionBasedOnExactMessagePolicy.Reset();
			string message = "Oh no!";
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
											"&Caller Defined",
											DialogResult.Cancel,
											message);
		}

		[Test, Ignore("By hand only")]
		public void NotifyUserOfProblem_ReallyLong()
		{
			string message = "Oh no! This is quite a long message to see if it will wrap so I will have to keep typing to see if this will work now. And then some more." +
				"And then keep going because I want to see what happens for a really long one," +
				"especially what happens with the resizing and if it works or not" + Environment.NewLine +
				Environment.NewLine + "and a newline as well.";
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);

			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);
		}
	}
}
