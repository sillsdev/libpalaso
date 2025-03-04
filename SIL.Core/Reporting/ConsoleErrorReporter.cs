using System;
using System.Diagnostics;

namespace SIL.Reporting
{
	public class ConsoleErrorReporter: IErrorReporter
	{
		private enum Severity
		{
			Fatal,
			NonFatal
		}

		public void ReportFatalException(Exception error)
		{
			WriteExceptionToConsole(error, null, Severity.Fatal);
		}

		/// <summary>
		/// Notifies the user of problem by writing to console
		/// </summary>
		/// <param name="policy">The policy used to check if the message should be shown</param>
		/// <param name="error">The exception to print to console, if the policy allows</param>
		/// <param name="message">The message to print to console, if the policy allows</param>
		/// <exception cref="ErrorReport.ProblemNotificationSentToUserException"></exception>
		public void NotifyUserOfProblem(IRepeatNoticePolicy policy, Exception error, string message)
		{
			if (!policy.ShouldShowMessage(message))
				return;

			if (ErrorReport.IsOkToInteractWithUser)
			{
				Console.WriteLine(message);
				if (error != null)
					Console.WriteLine(error.ToString());

				Console.WriteLine(policy.ReoccurrenceMessage);
				return;
			}

			throw new ErrorReport.ProblemNotificationSentToUserException(message);
		}

		/// <param name="alternateButton1Label">N/A. You may pass null. This parameter will be ignored.</param>
		/// <param name="resultIfAlternateButtonPressed">N/A. This parameter will be ignored.</param>
		public ErrorResult NotifyUserOfProblem(IRepeatNoticePolicy policy, string alternateButton1Label,
			ErrorResult resultIfAlternateButtonPressed, string message)
		{
			NotifyUserOfProblem(policy, null, message);
			return ErrorResult.OK;
		}

		public void ReportNonFatalException(Exception exception, IRepeatNoticePolicy policy)
		{
			if (policy.ShouldShowErrorReportDialog(exception))
			{
				WriteExceptionToConsole(exception, null, Severity.NonFatal);
			}
		}

		public void ReportNonFatalExceptionWithMessage(Exception error, string message, params object[] args)
		{
			var s = string.Format(message, args);
			WriteExceptionToConsole(error, s, Severity.NonFatal);
		}

		public void ReportNonFatalMessageWithStackTrace(string message, params object[] args)
		{
			var s = string.Format(message, args);
			var stack = new StackTrace(true);
			WriteStackToConsole(s, stack, Severity.NonFatal);
		}

		public void ReportFatalMessageWithStackTrace(string message, object[] args)
		{
			var s = string.Format(message, args);
			var stack = new StackTrace(true);
			WriteStackToConsole(s, stack, Severity.Fatal);
		}

		// This implementation is a stripped down version of what is found in
		// ExceptionReportingDialog.Report(string message, string messageBeforeStack, Exception error, Form owningForm)
		private static void WriteExceptionToConsole(Exception error, string message, Severity severity)
		{
			var textToReport = GetErrorStamp(severity);

			Exception innerMostException = null;
			textToReport += ErrorReport.GetHierarchicalExceptionInfo(error, ref innerMostException);

			// If the exception had inner exceptions, show the innermost exception first, since
			// that is usually the one we want the developer to read.
			if (innerMostException != null)
			{
				textToReport += string.Format("Inner-most exception:{0}{1}{0}{0}Full, hierarchical exception contents:{0}{2}",
					Environment.NewLine, ErrorReport.GetExceptionText(innerMostException), textToReport);
			}

			textToReport += ErrorReportingProperties;

			if (!string.IsNullOrEmpty(message))
			{
				textToReport += "Message (not an exception): " + message + Environment.NewLine;
				textToReport += Environment.NewLine;
			}

			if (innerMostException != null)
			{
				error = innerMostException;
			}

			try
			{
				Logger.WriteEvent("Got exception " + error.GetType().Name);
			}
			catch (Exception err)
			{
				// We have more than one report of dying while logging an exception.
				textToReport += "****Could not write to log (" + err.Message + ")" + Environment.NewLine;
				textToReport += "Was trying to log the exception: " + error.Message + Environment.NewLine;
				textToReport += "Recent events:" + Environment.NewLine;
				textToReport += Logger.MinorEventsLog;
			}
			Console.WriteLine(textToReport);
		}

		private static string GetErrorStamp(Severity severity)
		{
			var textToReport = $"{DateTime.UtcNow:r}:" + Environment.NewLine;
			textToReport += "Severity: ";

			switch (severity)
			{
				case Severity.Fatal:
					textToReport += "Fatal";
					break;
				case Severity.NonFatal:
					textToReport += "Warning";
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(severity));
			}
			textToReport += Environment.NewLine;
			return textToReport;
		}

		private static string ErrorReportingProperties
		{
			get
			{
				var properties = "";
				properties += Environment.NewLine + "--Error Reporting Properties--" + Environment.NewLine;
				foreach (string label in ErrorReport.Properties.Keys)
				{
					properties += label + ": " + ErrorReport.Properties[label] + Environment.NewLine;
				}
				return properties;
			}
		}

		// This implementation is a stripped down version of what is found in
		// ExceptionReportingDialog.Report(string, string, StackTrace, Form)
		private static void WriteStackToConsole(string message, StackTrace stack, Severity severity)
		{
			var textToReport = GetErrorStamp(severity);

			textToReport += "Message (not an exception): " + message + Environment.NewLine;
			textToReport += Environment.NewLine;
			textToReport += "--Stack--" + Environment.NewLine;
			textToReport += stack + Environment.NewLine;
			textToReport += ErrorReportingProperties;

			try
			{
				Logger.WriteEvent("Got error message " + message);
			}
			catch (Exception err)
			{
				// We have more than one report of dying while logging an exception.
				textToReport +=
					$"****Could not write to log ({err.Message}){Environment.NewLine}";
			}
			Console.WriteLine(textToReport);
		}
	}
}
