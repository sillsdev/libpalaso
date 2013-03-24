using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Palaso.Reporting;
using RestSharp;


namespace Palaso.Reporting
{
	public interface IErrorReporter
	{
		void ReportFatalException(Exception e);
		ErrorResult NotifyUserOfProblem(IRepeatNoticePolicy policy,
										string alternateButton1Label,
										ErrorResult resultIfAlternateButtonPressed,
										string message);
		void ReportNonFatalException(Exception exception, IRepeatNoticePolicy policy);
		void ReportNonFatalExceptionWithMessage(Exception error, string message, params object[] args);
		void ReportNonFatalMessageWithStackTrace(string message, params object[] args);
		void ReportFatalMessageWithStackTrace(string message, object[] args);
	}

	public enum ErrorResult
	{
		None,
		OK,
		Cancel,
		Abort,
		Retry,
		Ignore,
		Yes,
		No
	}

	public class ErrorReport
	{
		private static IErrorReporter _errorReporter = new ConsoleErrorReporter();
		private static string _parseDotComApplicationId;
		private static string _parseDotComRestApiKey;

		//We removed all references to Winforms from Palaso.dll but our error reporting relied heavily on it.
		//Not wanting to break existing applications we have now added this class initializer which will
		//look for a reference to PalasoUIWindowsForms in the consuming app and if it exists instantiate the
		//WinformsErrorReporter from there through Reflection. otherwise we will simply use a console
		//error reporter
		static ErrorReport()
		{
			var topMostAssembly = Assembly.GetEntryAssembly();
			if (topMostAssembly != null)
			{
				var referencedAssemblies = topMostAssembly.GetReferencedAssemblies();
				var palasoUiWindowsFormsInializeAssemblyName =
					referencedAssemblies.FirstOrDefault(a => a.Name.Contains("PalasoUIWindowsForms"));//This will fail when there are multiple matches: SingleOrDefault
				if (palasoUiWindowsFormsInializeAssemblyName != null)
				{
					var palasoUIWinFormsAssembly = Assembly.Load(palasoUiWindowsFormsInializeAssemblyName);
					//Make this go find the actual winFormsErrorReporter as opposed to looking for the interface
					var interfaceToFind = typeof (IErrorReporter);
					var typeImplementingInterface =
						palasoUIWinFormsAssembly.GetTypes().Where(p => interfaceToFind.IsAssignableFrom(p));
					foreach (var type in typeImplementingInterface)
					{
						_errorReporter = type.GetConstructor(Type.EmptyTypes).Invoke(null) as IErrorReporter;
					}
				}
			}
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="appUrl">This isn't actualy used, but it's required so that people reading the client's code can tell where the data is going (they'd still need permission to access it)</param>
		/// <param name="parseDotComApplicationId"></param>
		/// <param name="parseDotComRestApiKey"></param>
		public static void SetUpForParseDotCom(string appUrl, string parseDotComApplicationId, string parseDotComRestApiKey)
		{
			_parseDotComApplicationId = parseDotComApplicationId;
			_parseDotComRestApiKey = parseDotComRestApiKey;
		}

		/// <summary>
		/// Use this method if you want to override the default IErrorReporter.
		/// This method should be called only once at application startup.
		/// </summary>
		/// <param name="handler"></param>
		public static void SetErrorReporter(IErrorReporter reporter)
		{
			_errorReporter = reporter;
		}

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

		public static void Init(string emailAddress)
		{
			s_emailAddress = emailAddress;
			AddStandardProperties();
		}

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
/*                object attr = GetAssemblyAttribute(typeof (AssemblyFileVersionAttribute));
				if (attr != null)
				{
					return ((AssemblyFileVersionAttribute) attr).Version;
				}
				return Application.ProductVersion;
 */
				var ver = Assembly.GetEntryAssembly().GetName().Version;
				return String.Format("Version {0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
			}
		}

		public static string UserFriendlyVersionString
		{
			get
			{
				var asm = Assembly.GetEntryAssembly();
				var ver = asm.GetName().Version;
				var file = asm.CodeBase.Replace("file:", String.Empty);
				file = file.TrimStart('/');
				var fi = new FileInfo(file);

				return String.Format(
					"Version {0}.{1}.{2} Built on {3}",
					ver.Major,
					ver.Minor,
					ver.Build,
					fi.CreationTime.ToString("dd-MMM-yyyy")
				);
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
			AddProperty("Version", GetVersionForErrorReporting());
			AddProperty("CommandLine", Environment.CommandLine);
			AddProperty("CurrentDirectory", Environment.CurrentDirectory);
			AddProperty("MachineName", Environment.MachineName);
			AddProperty("OSVersion", GetOperatingSystemLabel());
			AddProperty("DotNetVersion", Environment.Version.ToString());
			AddProperty("WorkingSet", Environment.WorkingSet.ToString());
			AddProperty("UserDomainName", Environment.UserDomainName);
			AddProperty("UserName", Environment.UserName);
			AddProperty("Culture", CultureInfo.CurrentCulture.ToString());
		}

		class Version
		{
			private readonly PlatformID _platform;
			private readonly int _major;
			private readonly int _minor;
			public string Label { get; private set; }

			public Version(PlatformID platform, int minor, int major,  string label)
			{
				_platform = platform;
				_major = major;
				_minor = minor;
				Label = label;
			}
			public bool Match(OperatingSystem os)
			{
				return os.Version.Minor == _minor &&
					   os.Version.Major == _major &&
					   os.Platform == _platform;
			}
		}

		public static string GetOperatingSystemLabel()
		{
			var list = new List<Version>();
			list.Add(new Version(PlatformID.Win32NT,0,5, "Windows 2000"));
			list.Add(new Version(PlatformID.Win32NT, 1, 5, "Windows XP"));
			list.Add(new Version(PlatformID.Win32NT, 0, 6, "Vista"));
			list.Add(new Version(PlatformID.Win32NT, 1, 6, "Windows 7"));
			foreach (var version in list)
			{
				if(version.Match(Environment.OSVersion))
					return version.Label + " " + Environment.OSVersion.ServicePack;
			}
			return Environment.OSVersion.VersionString;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public static string GetHiearchicalExceptionInfo(Exception error, ref Exception innerMostException)
		{
			string x = GetExceptionText(error);

			if (error.InnerException != null)
			{
				innerMostException = error.InnerException;

				x += "**Inner Exception:\r\n";
				x += GetHiearchicalExceptionInfo(error.InnerException, ref innerMostException);
			}
			return x;
		}

		public static void ReportFatalException(Exception error)
		{
			UsageReporter.ReportException(true, null, error, null);
			SendExceptionToParseDotCom(true,"",error,"");
			_errorReporter.ReportFatalException(error);
		}

		/// <summary>
		/// Put up a message box, unless OkToInteractWithUser is false, in which case throw an Appliciation Exception.
		/// This will not report the problem to the developer.  Use one of the "report" methods for that.
		/// </summary>
		public static void NotifyUserOfProblem(string message, params object[] args)
		{
			NotifyUserOfProblem(new ShowAlwaysPolicy(), message, args);
		}

		public static ErrorResult NotifyUserOfProblem(IRepeatNoticePolicy policy, string messageFmt, params object[] args)
		{
			return NotifyUserOfProblem(policy, null, default(ErrorResult), messageFmt, args);
		}

		public static void NotifyUserOfProblem(Exception error, string messageFmt, params object[] args)
		{
			NotifyUserOfProblem(new ShowAlwaysPolicy(), error, messageFmt, args);
		}

		public static void NotifyUserOfProblem(IRepeatNoticePolicy policy, Exception error, string messageFmt, params object[] args)
		{
			var result = NotifyUserOfProblem(policy, "Details", ErrorResult.Yes, messageFmt, args);
			var userMessage = String.Format(messageFmt, args);
			if (result == ErrorResult.Yes)
			{
				ReportNonFatalExceptionWithMessage(error, userMessage);
			}

			UsageReporter.ReportException(false, null, error, userMessage);
			SendExceptionToParseDotCom(false, "", error, userMessage);
		}

		public static ErrorResult NotifyUserOfProblem(IRepeatNoticePolicy policy,
									string alternateButton1Label,
									ErrorResult resultIfAlternateButtonPressed,
									string messageFmt,
									params object[] args)
		{
			var message = String.Format(messageFmt, args);
			if (s_justRecordNonFatalMessagesForTesting)
			{
				s_previousNonFatalMessage = message;
				return ErrorResult.OK;
			}
			return _errorReporter.NotifyUserOfProblem(policy, alternateButton1Label, resultIfAlternateButtonPressed, message);
		}

		/// <summary>
		/// Bring up a "yellow box" that let's them send in a report, then return to the program.
		/// </summary>
		public static void ReportNonFatalExceptionWithMessage(Exception error, string message, params object[] args)
		{
			_errorReporter.ReportNonFatalExceptionWithMessage(error, message, args);
		}

		/// <summary>
		/// Bring up a "yellow box" that let's them send in a report, then return to the program.
		/// Use this one only when you don't have an exception (else you're not reporting the exception's message)
		/// </summary>
		public static void ReportNonFatalMessageWithStackTrace(string message, params object[] args)
		{
			_errorReporter.ReportNonFatalMessageWithStackTrace(message, args);
		}
		/// <summary>
		/// Bring up a "green box" that let's them send in a report, then exit.
		/// </summary>
		public static void ReportFatalMessageWithStackTrace(string message, params object[] args)
		{
			_errorReporter.ReportFatalMessageWithStackTrace(message, args);
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
			if (s_justRecordNonFatalMessagesForTesting)
			{
				s_previousNonFatalException = exception;
				return;
			}
			_errorReporter.ReportNonFatalException(exception, policy);
			 UsageReporter.ReportException(false, null, exception, null);
			 SendExceptionToParseDotCom(false, "", exception, "");
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

		public static string GetEnvironmentDetails()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("--Error Reporting Properties--");
			foreach (string label in Properties.Keys)
			{
				builder.AppendLine(label + ": " + Properties[label]);
			}
			return builder.ToString();
		}
		public static string GetLogOfEventsBeforeError()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(Environment.NewLine + "--Log--");
			try
			{
				builder.AppendLine(Logger.LogText);
			}
			catch (Exception err)
			{
				//We have more than one report of dieing while logging an exception.
				builder.AppendLine("****Could not read from log: " + err.Message);
			}
			return builder.ToString();
		}

		public static void SendExceptionToParseDotCom(bool wasFatal, string theCommandOrOtherContext, Exception error, string messageUserSaw)
		{
			try
			{
				if (String.IsNullOrEmpty(_parseDotComApplicationId) || String.IsNullOrEmpty(_parseDotComRestApiKey))
					return;

				var request = new RestRequest("/1/classes/Exception", Method.POST);
				request.AddHeader("X-Parse-Application-Id", _parseDotComApplicationId);

				request.AddHeader("X-Parse-REST-API-Key", _parseDotComRestApiKey);
				request.RequestFormat = DataFormat.Json;

				request.AddBody(new
									{
										app = UsageReporter.AppNameToUseInReporting,
										fatal = wasFatal,
										commandOrOtherContext = theCommandOrOtherContext,
										version = VersionNumberString,
										area = UsageReporter.MostRecentArea,
										userMessage = messageUserSaw,
										exceptionMessage = error==null? "no exception" : error.Message,
										stack = error == null ? "no exception" : error.StackTrace,
										environment = GetEnvironmentDetails(),
										log = GetLogOfEventsBeforeError()
									});
				var client = new RestClient("https://api.parse.com:443");
				client.ExecuteAsync(request, (resp) => Debug.WriteLine("***Parse.com: "+resp.Content));

			}
			catch (Exception)
			{
#if DEBUG
				throw;
#endif
			}
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

		public static void Reset()
		{
			_alreadyReportedMessages.Clear();
		}
	}
}
