using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Reporting;

namespace Palaso.WinForms
{
	class WinFormsErrorReporter
	{

		public void ReportFatalException(Exception e)
		{
			ExceptionReportingDialog.ReportException(e, null);
		}

		public static ErrorResult NotifyUserOfProblem(IRepeatNoticePolicy policy,
									string alternateButton1Label,
									ErrorResult resultIfAlternateButtonPressed,
									string message)
		{
			if (!policy.ShouldShowMessage(message))
			{
				return ErrorResult.OK; ;
			}

			if (ErrorReport.IsOkToInteractWithUser)
			{
				var dlg = new ProblemNotificationDialog(message, UsageReporter.AppNameToUseInDialogs + " Problem")
				{
					ReoccurenceMessage = policy.ReoccurenceMessage

				};
				if (!string.IsNullOrEmpty(alternateButton1Label))
				{
					dlg.EnableAlternateButton1(alternateButton1Label, resultIfAlternateButtonPressed);
				}
				return dlg.ShowDialog();
			}
			else
			{
				throw new ErrorReport.ProblemNotificationSentToUserException(message);
			}
		}

		/// <summary>
		/// Allow user to report an exception even though the program doesn't need to exit
		/// </summary>
		public static void ReportNonFatalException(Exception exception, IRepeatNoticePolicy policy)
		{
			if (policy.ShouldShowErrorReportDialog(exception))
			{
				if (ErrorReport.IsOkToInteractWithUser)
				{
					ExceptionReportingDialog.ReportException(exception, null, false);
				}
				else
				{
					throw new ErrorReport.NonFatalExceptionWouldHaveBeenMessageShownToUserException(exception);
				}
			}
		}

		/// <summary>
		/// Bring up a "yellow box" that let's them send in a report, then return to the program.
		/// </summary>
		public static void ReportNonFatalExceptionWithMessage(Exception error, string message, params object[] args)
		{
			var s = string.Format(message, args);
			ExceptionReportingDialog.ReportMessage(s, error, false);
		}

		/// <summary>
		/// Bring up a "yellow box" that let's them send in a report, then return to the program.
		/// Use this one only when you don't have an exception (else you're not reporting the exception's message)
		/// </summary>
		public static void ReportNonFatalMessageWithStackTrace(string message, params object[] args)
		{
			var s = string.Format(message, args);
			var stack = new System.Diagnostics.StackTrace(true);
			ExceptionReportingDialog.ReportMessage(s, stack, false);
		}
	}
}
