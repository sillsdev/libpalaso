﻿using System;
using System.Diagnostics;

namespace Palaso.Reporting
{
	public class ConsoleErrorReporter:IErrorReporter
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

		public ErrorResult NotifyUserOfProblem(IRepeatNoticePolicy policy, string alternateButton1Label, ErrorResult resultIfAlternateButtonPressed, string message)
		{
			if (!policy.ShouldShowMessage(message))
			{
				return ErrorResult.OK;
			}


			if (ErrorReport.IsOkToInteractWithUser)
			{
				if (ErrorReport.IsOkToInteractWithUser)
				{
					Console.WriteLine(message);
					Console.WriteLine(policy.ReoccurenceMessage);
				}
				return ErrorResult.OK;
			}
			else
			{
				throw new ErrorReport.ProblemNotificationSentToUserException(message);
			}
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

		//This implementation is a stripped down version of what is found in
		//ExceptionReportingDialog.Report(string message, string messageBeforeStack, Exception error, Form owningForm)
		private void WriteExceptionToConsole(Exception error, string message, Severity severity)
		{
			var textToReport = GetErrorStamp(severity);

			Exception innerMostException = null;
			textToReport += ErrorReport.GetHiearchicalExceptionInfo(error, ref innerMostException);

			//if the exception had inner exceptions, show the inner-most exception first, since that is usually the one
			//we want the developer to read.
			if (innerMostException != null)
			{
				textToReport += "Inner-most exception:\r\n" + ErrorReport.GetExceptionText(innerMostException) +
								 "\r\n\r\nFull, hierarchical exception contents:\r\n" + textToReport;
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
				//We have more than one report of dieing while logging an exception.
				textToReport += "****Could not write to log (" + err.Message + ")" + Environment.NewLine;
				textToReport += "Was trying to log the exception: " + error.Message + Environment.NewLine;
				textToReport += "Recent events:" + Environment.NewLine;
				textToReport += Logger.MinorEventsLog;
			}
			Console.WriteLine(textToReport);
		}

		private static string GetErrorStamp(Severity severity)
		{
			var textToReport = String.Format("{0}:", DateTime.UtcNow.ToString("r")) + Environment.NewLine;
			textToReport += "Severity: ";

			switch (severity)
			{
				case Severity.Fatal:
					textToReport = textToReport + "Fatal";
					break;
				case Severity.NonFatal:
					textToReport = textToReport + "Warning";
					break;
				default:
					throw new ArgumentOutOfRangeException("severity");
			}
			textToReport += Environment.NewLine;
			return textToReport;
		}

		private string ErrorReportingProperties
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

		//This implementation is a stripped down version of what is found in
		//ExceptionReportingDialog.Report(string message, string messageBeforeStack, StackTrace stackTrace, Form owningForm)
		private void WriteStackToConsole(string message, StackTrace stack, Severity severity)
		{
			var textToReport = GetErrorStamp(severity);

			textToReport += "Message (not an exception): " + message + Environment.NewLine;
			textToReport += Environment.NewLine;
			textToReport += "--Stack--" + Environment.NewLine; ;
			textToReport += stack.ToString() + Environment.NewLine; ;


			textToReport += ErrorReportingProperties;

			try
			{
				Logger.WriteEvent("Got error message " + message);
			}
			catch (Exception err)
			{
				//We have more than one report of dieing while logging an exception.
				textToReport += "****Could not write to log (" + err.Message + ")" + Environment.NewLine;
			}
			Console.WriteLine(textToReport);
		}
	}
}
