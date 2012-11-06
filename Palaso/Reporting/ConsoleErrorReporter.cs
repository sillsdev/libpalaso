using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.Reporting
{
	public class ConsoleErrorReporter:IErrorReporter
	{
		public void ReportFatalException(Exception e)
		{
			throw new NotImplementedException();
		}

		public ErrorResult NotifyUserOfProblem(IRepeatNoticePolicy policy, string alternateButton1Label, ErrorResult resultIfAlternateButtonPressed, string message)
		{
			if (!policy.ShouldShowMessage(message))
			{
				return ErrorResult.OK; ;
			}

			if (ErrorReport.IsOkToInteractWithUser)
			{
				Console.WriteLine(String.Format(UsageReporter.AppNameToUseInDialogs + " Problem: " + message));
				return ErrorResult.OK;
			}
			else
			{
				throw new ErrorReport.ProblemNotificationSentToUserException(message);
			}
		}

		public void ReportNonFatalException(Exception exception, IRepeatNoticePolicy policy)
		{
			throw new NotImplementedException();
		}

		public void ReportNonFatalExceptionWithMessage(Exception error, string message, params object[] args)
		{
			throw new NotImplementedException();
		}

		public void ReportNonFatalMessageWithStackTrace(string message, params object[] args)
		{
			throw new NotImplementedException();
		}
	}
}
