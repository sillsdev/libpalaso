using System;
using System.Windows.Forms;
using SIL.Extensions;
using SIL.Reporting;

namespace SIL.Windows.Forms.Reporting
{
	public class WinFormsErrorReporter : IErrorReporter
	{

		public void ReportFatalException(Exception e)
		{
			ExceptionReportingDialog.ReportException(e, null);
		}

		/// <summary>
		// Notifies the user of {message}, if {policy} permits.
		// If {exception} is non-null, then a "Details" button will appear, which if pressed, will invoke {ErrorReport.OnShowDetails(exception, message)}
		/// </summary>
		/// <remarks>
		/// For legacy reasons, this function will wait for the user to respond to the UI dialog.
		/// (This is just because that's what it always used to do. It can be changed if desired, assuming all its references continue to work correctly)
		/// </remarks>
		public void NotifyUserOfProblem(IRepeatNoticePolicy policy, Exception exception, string message)
		{
			string alternateButton1Label;
			ErrorResult resultIfAlternateButtonPressed;
			if (exception == null)
			{
				alternateButton1Label = null;
				resultIfAlternateButtonPressed = default(ErrorResult);
			}
			else
			{
				alternateButton1Label = "Details";
				resultIfAlternateButtonPressed = ErrorResult.Yes;
			}
			var result = NotifyUserOfProblem(policy, alternateButton1Label, resultIfAlternateButtonPressed, message);

			if (result == ErrorResult.Yes)
			{
				ErrorReport.OnShowDetails(exception, message);
			}
		}

		public ErrorResult NotifyUserOfProblem(IRepeatNoticePolicy policy,
									string alternateButton1Label,
									ErrorResult resultIfAlternateButtonPressed,
									string message)
		{
			if (!policy.ShouldShowMessage(message))
			{
				return ErrorResult.OK;
			}

			if (ErrorReport.IsOkToInteractWithUser)
			{
				var dlg = new ProblemNotificationDialog(message, UsageReporter.AppNameToUseInDialogs + " Problem")
				{
					ReoccurrenceMessage = policy.ReoccurrenceMessage
				};
				if (!string.IsNullOrEmpty(alternateButton1Label))
				{
					dlg.EnableAlternateButton1(alternateButton1Label, GetDialogResultForErrorResult(resultIfAlternateButtonPressed));
				}
				return GetErrorResultForDialogResult(dlg.ShowDialog());
			}
			else
			{
				throw new ErrorReport.ProblemNotificationSentToUserException(message);
			}
		}

		/// <summary>
		/// Allow user to report an exception even though the program doesn't need to exit
		/// </summary>
		public void ReportNonFatalException(Exception exception, IRepeatNoticePolicy policy)
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
		public void ReportNonFatalExceptionWithMessage(Exception error, string message, params object[] args)
		{
			var s = message.FormatWithErrorStringInsteadOfException(args);
			ExceptionReportingDialog.ReportMessage(s, error, false);
		}

		/// <summary>
		/// Bring up a "yellow box" that let's them send in a report, then return to the program.
		/// Use this one only when you don't have an exception (else you're not reporting the exception's message)
		/// </summary>
		public void ReportNonFatalMessageWithStackTrace(string message, params object[] args)
		{
			var s = message.FormatWithErrorStringInsteadOfException(args);
			var stack = new System.Diagnostics.StackTrace(true);
			ExceptionReportingDialog.ReportMessage(s, stack, false);
		}

		public void ReportFatalMessageWithStackTrace(string message, object[] args)
		{
			var s = message.FormatWithErrorStringInsteadOfException(args);
			var stack = new System.Diagnostics.StackTrace(true);
			ExceptionReportingDialog.ReportMessage(s, stack, true);
		}

		private static ErrorResult GetErrorResultForDialogResult(DialogResult dialogResult)
		{
			ErrorResult errorResult;
			switch (dialogResult)
			{
				case DialogResult.Abort:
					errorResult = ErrorResult.Abort;
					break;
				case DialogResult.Cancel:
					errorResult = ErrorResult.Cancel;
					break;
				case DialogResult.Ignore:
					errorResult = ErrorResult.Ignore;
					break;
				case DialogResult.No:
					errorResult = ErrorResult.No;
					break;
				case DialogResult.None:
					errorResult = ErrorResult.None;
					break;
				case DialogResult.OK:
					errorResult = ErrorResult.OK;
					break;
				case DialogResult.Retry:
					errorResult = ErrorResult.Retry;
					break;
				case DialogResult.Yes:
					errorResult = ErrorResult.Yes;
					break;
				default:
					throw new ArgumentOutOfRangeException(String.Format("Can't convert DialogResult {0} to ErrorResult Type", dialogResult));
			}
			return errorResult;
		}

		private static DialogResult GetDialogResultForErrorResult(ErrorResult errorResult)
		{
			DialogResult dialogResult;
			switch (errorResult)
			{
				case ErrorResult.Abort:
					dialogResult = DialogResult.Abort;
					break;
				case ErrorResult.Cancel:
					dialogResult = DialogResult.Cancel;
					break;
				case ErrorResult.Ignore:
					dialogResult = DialogResult.Ignore;
					break;
				case ErrorResult.No:
					dialogResult = DialogResult.No;
					break;
				case ErrorResult.None:
					dialogResult = DialogResult.None;
					break;
				case ErrorResult.OK:
					dialogResult = DialogResult.OK;
					break;
				case ErrorResult.Retry:
					dialogResult = DialogResult.Retry;
					break;
				case ErrorResult.Yes:
					dialogResult = DialogResult.Yes;
					break;
				default:
					throw new ArgumentOutOfRangeException(String.Format("Can't convert ErrorResult {0} to DialogResult Type", errorResult));
			}
			return dialogResult;
		}
	}
}
