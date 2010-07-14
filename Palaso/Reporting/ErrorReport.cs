using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Palaso.Reporting
{
	public class ErrorReport
	{
		protected static string s_emailAddress = null;
		protected static string s_emailSubject = "Exception Report";

		/// <summary>
		/// a list of name, string value pairs that will be included in the details of the error report.
		/// </summary>
		private static StringDictionary s_properties =
			new StringDictionary();

		private static bool s_isOkToInteractWithUser = true;
		private static bool s_justRecordNonFatalMessagesForTesting=false;
		private static string s_previousNonFatalMessage;
		private static Exception s_previousNonFatalException;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public static string GetExceptionText(Exception error)
		{
			StringBuilder subject = new StringBuilder();
			subject.AppendFormat("Exception: {0}", error.Message);

			StringBuilder txt = new StringBuilder();

			txt.Append("Msg: ");
			txt.Append(error.Message);

			try
			{
				if (error is COMException)
				{
					txt.Append("\r\nCOM message: ");
					txt.Append(new Win32Exception(((COMException) error).ErrorCode).Message);
				}
			}
			catch {}

			try
			{
				txt.Append("\r\nSource: ");
				txt.Append(error.Source);
				subject.AppendFormat(" in {0}", error.Source);
			}
			catch {}

			try
			{
				if (error.TargetSite != null)
				{
					txt.Append("\r\nAssembly: ");
					txt.Append(error.TargetSite.DeclaringType.Assembly.FullName);
				}
			}
			catch {}

			try
			{
				txt.Append("\r\nStack: ");
				txt.Append(error.StackTrace);
			}
			catch {}

			s_emailSubject = subject.ToString();

			txt.Append("\r\n");
			return txt.ToString();
		}

		public static string GetVersionForErrorReporting()
		{
			Assembly assembly = Assembly.GetEntryAssembly();
			if (assembly != null)
			{
				string version = VersionNumberString;

				version += " (apparent build date: ";
				try
				{
					string path = assembly.CodeBase.Replace(@"file:///", "");
					version += File.GetLastWriteTimeUtc(path).Date.ToShortDateString() + ")";
				}
				catch
				{
					version += "???";
				}

#if DEBUG
				version += "  (Debug version)";
#endif
				return version;
			}
			return "unknown";
		}

		public static object GetAssemblyAttribute(Type attributeType)
		{
			Assembly assembly = Assembly.GetEntryAssembly();
			if (assembly != null)
			{
				object[] attributes =
					assembly.GetCustomAttributes(attributeType, false);
				if (attributes != null && attributes.Length > 0)
				{
					return attributes[0];
				}
			}
			return null;
		}

		public static string VersionNumberString
		{
			get
			{
				object attr = GetAssemblyAttribute(typeof (AssemblyFileVersionAttribute));
				if (attr != null)
				{
					return ((AssemblyFileVersionAttribute) attr).Version;
				}
				return Application.ProductVersion;
			}
		}

		public static string UserFriendlyVersionString
		{
			get
			{
				string v = VersionNumberString;
				string build = v.Substring(v.LastIndexOf('.') + 1);
				string label = "";
				object attr = GetAssemblyAttribute(typeof (AssemblyProductAttribute));
				if (attr != null)
				{
					label = ((AssemblyProductAttribute) attr).Product + ", ";
				}

				//return "Version 1 Preview, build " + build;
				return label + "build " + build;
			}
		}

//        /// ------------------------------------------------------------------------------------
//        /// <summary>
//        ///make this false during automated testing
//        /// </summary>
//        /// ------------------------------------------------------------------------------------
//        public static bool OkToInteractWithUser
//        {
//            set { s_isOkToInteractWithUser = value; }
//            get { return s_isOkToInteractWithUser; }
//        }

		/// <summary>
		/// this overrides OkToInteractWithUser
		/// The test can then retrieve from PreviousNonFatalMessage
		/// </summary>
//        public static bool JustRecordNonFatalMessagesForTesting
//        {
//            set { s_justRecordNonFatalMessagesForTesting = value; }
//            get { return s_justRecordNonFatalMessagesForTesting; }
//        }

		/// <summary>
		/// for unit test
		/// </summary>
//        public static string PreviousNonFatalMessage
//        {
//            get { return s_previousNonFatalMessage; }
//        }

		/// <summary>
		/// use this in unit tests to cleanly check that a message would have been shown.
		/// E.g.  using (new Palaso.Reporting.ErrorReport.NonFatalErrorReportExpected()) {...}
		/// </summary>
		public class NonFatalErrorReportExpected :IDisposable
		{
			private readonly bool previousJustRecordNonFatalMessagesForTesting;
			public NonFatalErrorReportExpected()
			{
				previousJustRecordNonFatalMessagesForTesting = s_justRecordNonFatalMessagesForTesting;
				s_justRecordNonFatalMessagesForTesting = true;
				s_previousNonFatalMessage = null;//this is a static, so a previous unit test could have filled it with something (yuck)
			}
			public void Dispose()
			{
				s_justRecordNonFatalMessagesForTesting= previousJustRecordNonFatalMessagesForTesting;
				if (s_previousNonFatalException == null &&  s_previousNonFatalMessage == null)
					throw new Exception("Non Fatal Error Report was expected but wasn't generated.");
				s_previousNonFatalMessage = null;
			}
			/// <summary>
			/// use this to check the actual contents of the message that was triggered
			/// </summary>
			public string Message
			{
				get { return s_previousNonFatalMessage; }
			}
		}

		/// <summary>
		/// set this property if you want the dialog to offer to create an e-mail message.
		/// </summary>
		public static string EmailAddress
		{
			set { s_emailAddress = value; }
			get { return s_emailAddress; }
		}

		/// <summary>
		/// set this property if you want something other than the default e-mail subject
		/// </summary>
		public static string EmailSubject
		{
			set { s_emailSubject = value; }
			get { return s_emailSubject; }
		}

		/// <summary>
		/// a list of name, string value pairs that will be included in the details of the error report.
		/// </summary>
		public static StringDictionary Properties
		{
			get
			{
				return s_properties;
			}
			set
			{
				s_properties = value;
			}
		}

		public static bool IsOkToInteractWithUser
		{
			get
			{
				return s_isOkToInteractWithUser;
			}
			set
			{
				s_isOkToInteractWithUser = value;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///	add a property that he would like included in any bug reports created by this application.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void AddProperty(string label, string contents)
		{
			//avoid an error if the user changes the value of something,
			//which happens in FieldWorks, for example, when you change the language project.
			if (s_properties.ContainsKey(label))
			{
				s_properties.Remove(label);
			}

			s_properties.Add(label, contents);
		}

		public static void AddStandardProperties()
		{
			AddProperty("Version", ErrorReport.GetVersionForErrorReporting());
			AddProperty("CommandLine", Environment.CommandLine);
			AddProperty("CurrentDirectory", Environment.CurrentDirectory);
			AddProperty("MachineName", Environment.MachineName);
			AddProperty("OSVersion", Environment.OSVersion.ToString());
			AddProperty("DotNetVersion", Environment.Version.ToString());
			AddProperty("WorkingSet", Environment.WorkingSet.ToString());
			AddProperty("UserDomainName", Environment.UserDomainName);
			AddProperty("UserName", Environment.UserName);
			AddProperty("Culture", CultureInfo.CurrentCulture.ToString());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public static string GetHiearchicalExceptionInfo(Exception error, ref Exception innerMostException)
		{
			string x = ErrorReport.GetExceptionText(error);

			if (error.InnerException != null)
			{
				innerMostException = error.InnerException;

				x += "**Inner Exception:\r\n";
				x += GetHiearchicalExceptionInfo(error.InnerException, ref innerMostException);
			}
			return x;
		}

		public static void ReportFatalException(Exception e)
		{
			ExceptionReportingDialog.ReportException(e, null);
		}

		public static void ReportFatalException(Exception e, Form parentForm)
		{
			ExceptionReportingDialog.ReportException(e, parentForm);
		}

		/// <summary>
		/// Put up a message box, unless OkToInteractWithUser is false, in which case throw an Appliciation Exception.
		/// This will not report the problem to the developer.  Use one of the "report" methods for that.
		/// </summary>
		public static void NotifyUserOfProblem(string message, params object[] args)
		{
			NotifyUserOfProblem(new ShowAlwaysPolicy(), message, args);
		}

		public static DialogResult NotifyUserOfProblem(IRepeatNoticePolicy policy, string messageFmt, params object[] args)
		{
			return NotifyUserOfProblem(policy, null, default(DialogResult), messageFmt, args);
		}

		public static void NotifyUserOfProblem(Exception error, string messageFmt, params object[] args)
		{
			NotifyUserOfProblem(new ShowAlwaysPolicy(), "Details", error, messageFmt, args);
		}

		public static void NotifyUserOfProblem(IRepeatNoticePolicy policy, Exception error, string messageFmt, params object[] args)
		{
			var result = NotifyUserOfProblem(policy, "Details", DialogResult.Yes, messageFmt, args);
			if (result == DialogResult.Yes)
			{
				ErrorReport.ReportNonFatalExceptionWithMessage(error, string.Format(messageFmt, args));
			}
		}

		public static DialogResult NotifyUserOfProblem(IRepeatNoticePolicy policy,
									string alternateButton1Label,
									DialogResult resultIfAlternateButtonPressed,
									string messageFmt,
									params object[] args)
		{
			var message = string.Format(messageFmt, args);
			if (!policy.ShouldShowMessage(message))
			{
				return DialogResult.OK;
			}

			if (s_justRecordNonFatalMessagesForTesting)
			{
				ErrorReport.s_previousNonFatalMessage = message;
				return DialogResult.OK;
			}
			else if (ErrorReport.IsOkToInteractWithUser)
			{
				var dlg = new ProblemNotificationDialog(message, UsageReporter.AppNameToUseInDialogs + " Problem")
				{
					ReoccurenceMessage = policy.ReoccurenceMessage

				};
				if(!string.IsNullOrEmpty(alternateButton1Label))
				{
					dlg.EnableAlternateButton1(alternateButton1Label, resultIfAlternateButtonPressed);
				}
				return dlg.ShowDialog();
			}
			else
			{
				throw new ProblemNotificationSentToUserException(message);
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
			var stack =  new System.Diagnostics.StackTrace(true);
			ExceptionReportingDialog.ReportMessage(s,stack, false);
		}
		/// <summary>
		/// Bring up a "green box" that let's them send in a report, then exit.
		/// </summary>
		public static void ReportFatalMessageWithStackTrace(string message, params object[] args)
		{
			var s = string.Format(message, args);
			var stack = new System.Diagnostics.StackTrace(true);
			ExceptionReportingDialog.ReportMessage(s, stack, false);
		}

		/// <summary>
		/// Bring up a "yellow box" that lets them send in a report, then return to the program.
		/// </summary>
		public static void ReportNonFatalException(Exception exception)
		{
			ReportNonFatalException(exception, new ShowAlwaysPolicy());
		}

		/// <summary>
		/// Allow user to report an exception even though the program doesn't need to exit
		/// </summary>
		public static void ReportNonFatalException(Exception exception, IRepeatNoticePolicy policy)
		{
			if(s_justRecordNonFatalMessagesForTesting)
			{
				ErrorReport.s_previousNonFatalException = exception;
				return;
			}
			 if(policy.ShouldShowErrorReportDialog(exception))
			{
				if (ErrorReport.IsOkToInteractWithUser)
				{
					   ExceptionReportingDialog.ReportException(exception, null, false);
				}
				else
				{
					throw new NonFatalExceptionWouldHaveBeenMessageShownToUserException(exception);
				}
			}
		}

		/// <summary>
		/// this is for interacting with test code which doesn't want to allow an actual UI
		/// </summary>
		public class ProblemNotificationSentToUserException : ApplicationException
		{
			public ProblemNotificationSentToUserException(string message) : base(message) {}
		}

		/// <summary>
		/// this is for interacting with test code which doesn't want to allow an actual UI
		/// </summary>
		public class NonFatalExceptionWouldHaveBeenMessageShownToUserException : ApplicationException
		{
			public NonFatalExceptionWouldHaveBeenMessageShownToUserException(Exception e)  : base(e.Message, e) { }
		}


	}

	public interface IRepeatNoticePolicy
	{
		bool ShouldShowErrorReportDialog(Exception exception);
		bool ShouldShowMessage(string message);
		string ReoccurenceMessage
		{ get;
		}
	}

	public class ShowAlwaysPolicy :IRepeatNoticePolicy
	{
		public bool ShouldShowErrorReportDialog(Exception exception)
		{
			return true;
		}

		public bool ShouldShowMessage(string message)
		{
			return true;
		}

		public string ReoccurenceMessage
		{
			get { return string.Empty; }
		}
	}

	public class ShowOncePerSessionBasedOnExactMessagePolicy :IRepeatNoticePolicy
	{
		private static List<string> _alreadyReportedMessages = new List<string>();

		public bool ShouldShowErrorReportDialog(Exception exception)
		{
			return ShouldShowMessage(exception.Message);
		}

		public bool ShouldShowMessage(string message)
		{
			 if(_alreadyReportedMessages.Contains(message))
				return false;
			_alreadyReportedMessages.Add(message);
			return true;
		}

		public string ReoccurenceMessage
		{
			get { return "This message will not be shown again this session."; }
		}
	}
}
