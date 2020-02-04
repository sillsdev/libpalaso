using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Reflection;

namespace SIL.Reporting
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

#region Windows8PlusVersionReportingSupport
		[DllImport("netapi32.dll", CharSet = CharSet.Auto)]
		static extern int NetWkstaGetInfo(string server,
			 int level,
			 out IntPtr info);

		[DllImport("netapi32.dll")]
		static extern int NetApiBufferFree(IntPtr pBuf);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct MachineInfo
		{
			public int platform_id;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string _computerName;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string _languageGroup;
			public int _majorVersion;
			public int _minorVersion;
		}

		/// <summary>
		/// An application can avoid the need of this method by adding/modifying the application manifest to declare support for a
		/// particular windows version. This code is still necessary to report usefully about versions of windows released after
		/// the application has shipped.
		/// </summary>
		public static string GetWindowsVersionInfoFromNetworkAPI()
		{
			IntPtr pBuffer;
			// Get the version information from the network api, passing null to get network info from this machine
			var retval = NetWkstaGetInfo(null, 100, out pBuffer);
			if(retval != 0)
				return "Windows Unknown(unidentifiable)";

			var info = (MachineInfo)Marshal.PtrToStructure(pBuffer, typeof(MachineInfo));
			string windowsVersion = null;
			if(info._majorVersion == 6)
			{
				if(info._minorVersion == 2)
					windowsVersion = "Windows 8";
				else if(info._minorVersion == 3)
					windowsVersion = "Windows 8.1";
			}
			else if(info._majorVersion == 10 && info._minorVersion == 0)
			{
				windowsVersion = "Windows 10";
			}
			else
			{
				windowsVersion = string.Format("Windows Unknown({0}.{1})", info._majorVersion, info._minorVersion);
			}
			NetApiBufferFree(pBuffer);
			return windowsVersion;
		}
#endregion

		private static IErrorReporter _errorReporter;

		//We removed all references to Winforms from SIL.Core.dll but our error reporting relied heavily on it.
		//Not wanting to break existing applications we have now added this class initializer which will
		//look for a reference to SIL.Windows.Forms in the consuming app and if it exists instantiate the
		//WinformsErrorReporter from there through Reflection. otherwise we will simply use a console
		//error reporter
		static ErrorReport()
		{
			_errorReporter = ExceptionHandler.GetObjectFromSilWindowsForms<IErrorReporter>() ?? new ConsoleErrorReporter();
		}

		/// <summary>
		/// Use this method if you want to override the default IErrorReporter.
		/// This method should normally be called only once at application startup.
		/// </summary>
		public static void SetErrorReporter(IErrorReporter reporter)
		{
			_errorReporter = reporter ?? new ConsoleErrorReporter();
		}

		protected static string s_emailAddress = null;
		protected static string s_emailSubject = "Exception Report";

		/// <summary>
		/// a list of name, string value pairs that will be included in the details of the error report.
		/// </summary>
		private static Dictionary<string, string> s_properties =
			new Dictionary<string, string>();

		private static bool s_isOkToInteractWithUser = true;
		private static Action<Exception, string> s_onShowDetails;
		private static bool s_justRecordNonFatalMessagesForTesting=false;
		private static string s_previousNonFatalMessage;
		private static Exception s_previousNonFatalException;

		public static void Init(string emailAddress)
		{
			s_emailAddress = emailAddress;
			ErrorReport.AddStandardProperties();
		}

		private static void UpdateEmailSubject(Exception error)
		{
			var subject = new StringBuilder();
			subject.AppendFormat("Exception: {0}", error.Message);
			try
			{
				subject.AppendFormat(" in {0}", error.Source);
			}
			catch {}
			s_emailSubject = subject.ToString();
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
			UpdateEmailSubject(error);
			return ExceptionHelper.GetExceptionText(error);
		}

		public static string GetVersionForErrorReporting()
		{
			return ReflectionHelper.LongVersionNumberString;
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

		public static string VersionNumberString => ReflectionHelper.VersionNumberString;

		public static string UserFriendlyVersionString
		{
			get
			{
				var asm = Assembly.GetEntryAssembly();
				var ver = asm.GetName().Version;
				var file = PathHelper.StripFilePrefix(asm.CodeBase);
				var fi = new FileInfo(file);

				return string.Format(
					"Version {0}.{1}.{2} Built on {3}",
					ver.Major,
					ver.Minor,
					ver.Build,
					fi.CreationTime.ToString("dd-MMM-yyyy")
				);
			}
		}

		/// <summary>
		/// use this in unit tests to cleanly check that a message would have been shown.
		/// E.g.  using (new SIL.Reporting.ErrorReport.NonFatalErrorReportExpected()) {...}
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
		/// use this in unit tests to cleanly check that a message would have been shown.
		/// E.g.  using (new SIL.Reporting.ErrorReport.NonFatalErrorReportExpected()) {...}
		/// </summary>
		public class NoNonFatalErrorReportExpected : IDisposable
		{
			private readonly bool previousJustRecordNonFatalMessagesForTesting;
			public NoNonFatalErrorReportExpected()
			{
				previousJustRecordNonFatalMessagesForTesting = s_justRecordNonFatalMessagesForTesting;
				s_justRecordNonFatalMessagesForTesting = true;
				s_previousNonFatalMessage = null;//this is a static, so a previous unit test could have filled it with something (yuck)
				s_previousNonFatalException = null;
			}
			public void Dispose()
			{
				s_justRecordNonFatalMessagesForTesting = previousJustRecordNonFatalMessagesForTesting;
				if (s_previousNonFatalException != null || s_previousNonFatalMessage != null)
					throw new Exception("Non Fatal Error Report was not expected but was generated: "+Message);
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
		public static Dictionary<string, string> Properties
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

		/// <summary>
		/// Software using ErrorReport's NotifyUserOfProblem methods can set this to their own method
		/// for handling when the user clicks the "Details" button.
		/// </summary>
		public static Action<Exception, string> OnShowDetails
		{
			get
			{
				return s_onShowDetails ??
				       (s_onShowDetails = ErrorReport.ReportNonFatalExceptionWithMessage);
			}
			set
			{
				s_onShowDetails = value;
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
			AddProperty("OSVersion", GetOperatingSystemLabel());
			if (Platform.IsUnix)
				AddProperty("DesktopEnvironment", Platform.DesktopEnvironmentInfoString);
			AddProperty("DotNetVersion", Environment.Version.ToString());
			AddProperty("WorkingSet", Environment.WorkingSet.ToString());
			AddProperty("UserDomainName", Environment.UserDomainName);
			AddProperty("UserName", Environment.UserName);
			AddProperty("Culture", CultureInfo.CurrentCulture.ToString());
		}

		/// <summary>
		/// Get the standard properties in a form suitable for other uses
		/// (such as analytics).
		/// </summary>
		public static Dictionary<string, string> GetStandardProperties()
		{
			var props = new Dictionary<string,string>();
			props.Add("Version", ErrorReport.GetVersionForErrorReporting());
			props.Add("CommandLine", Environment.CommandLine);
			props.Add("CurrentDirectory", Environment.CurrentDirectory);
			props.Add("MachineName", Environment.MachineName);
			props.Add("OSVersion", GetOperatingSystemLabel());
			if (Platform.IsUnix)
				props.Add("DesktopEnvironment", Platform.DesktopEnvironmentInfoString);
			props.Add("DotNetVersion", Environment.Version.ToString());
			props.Add("WorkingSet", Environment.WorkingSet.ToString());
			props.Add("UserDomainName", Environment.UserDomainName);
			props.Add("UserName", Environment.UserName);
			props.Add("Culture", CultureInfo.CurrentCulture.ToString());
			return props;
		}

		class Version
		{
			private readonly PlatformID _platform;
			private readonly int _major;
			private readonly int _minor;
			public string Label { get; private set; }

			public Version(PlatformID platform, int major, int minor, string label)
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
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				var startInfo = new ProcessStartInfo("lsb_release", "-si -sr -sc");
				startInfo.RedirectStandardOutput = true;
				startInfo.UseShellExecute = false;
				var proc = new Process { StartInfo = startInfo };
				try
				{
					proc.Start();
					proc.WaitForExit(500);
					if(proc.ExitCode == 0)
					{
						var si = proc.StandardOutput.ReadLine();
						var sr = proc.StandardOutput.ReadLine();
						var sc = proc.StandardOutput.ReadLine();
						return String.Format("{0} {1} {2}", si, sr, sc);
					}
				}
				catch (Exception)
				{
					// lsb_release should work on all supported versions but fall back to the OSVersion.VersionString
				}
			}
			else
			{
				// https://msdn.microsoft.com/en-us/library/windows/desktop/ms724832%28v=vs.85%29.aspx
				var list = new List<Version>();
				list.Add(new Version(PlatformID.Win32NT, 5, 0, "Windows 2000"));
				list.Add(new Version(PlatformID.Win32NT, 5, 1, "Windows XP"));
				list.Add(new Version(PlatformID.Win32NT, 6, 0, "Vista"));
				list.Add(new Version(PlatformID.Win32NT, 6, 1, "Windows 7"));
				// After Windows 8 the Environment.OSVersion started misreporting information unless
				// your app has a manifest which says it supports the OS it is running on.  This is not
				// helpful if someone starts using an app built before the OS is released. Anything that
				// reports its self as Windows 8 is suspect, and must get the version info another way.
				list.Add(new Version(PlatformID.Win32NT, 6, 3, "Windows 8.1"));
				list.Add(new Version(PlatformID.Win32NT, 10, 0, "Windows 10"));

				foreach (var version in list)
				{
					if (version.Match(Environment.OSVersion))
						return version.Label + " " + Environment.OSVersion.ServicePack;
				}

				// Handle any as yet unrecognized (possibly unmanifested) versions, or anything that reported its self as Windows 8.
				if(Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					return GetWindowsVersionInfoFromNetworkAPI() + " " + Environment.OSVersion.ServicePack;
				}
			}
			return Environment.OSVersion.VersionString;
		}

		public static string GetHiearchicalExceptionInfo(Exception error, ref Exception innerMostException)
		{
			UpdateEmailSubject(error);
			return ExceptionHelper.GetHiearchicalExceptionInfo(error, ref innerMostException);
		}

		public static void ReportFatalException(Exception error)
		{
			UsageReporter.ReportException(true, null, error, null);
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
			if (result == ErrorResult.Yes)
			{
				OnShowDetails(error, string.Format(messageFmt, args));
			}

			UsageReporter.ReportException(false, null, error, String.Format(messageFmt, args));
		}

		public static ErrorResult NotifyUserOfProblem(IRepeatNoticePolicy policy,
									string alternateButton1Label,
									ErrorResult resultIfAlternateButtonPressed,
									string messageFmt,
									params object[] args)
		{
			var message = string.Format(messageFmt, args);
			if (s_justRecordNonFatalMessagesForTesting)
			{
				s_previousNonFatalMessage = message;
				return ErrorResult.OK;
			}
			return _errorReporter.NotifyUserOfProblem(policy, alternateButton1Label, resultIfAlternateButtonPressed, message);
		}

		/// <summary>
		/// Bring up a "yellow box" that let's them send in a report, then return to the program.
		/// This version assumes the message has already been formatted with any arguments.
		/// </summary>
		public static void ReportNonFatalExceptionWithMessage(Exception error, string message)
		{
			s_previousNonFatalMessage = message;
			s_previousNonFatalException = error;
			_errorReporter.ReportNonFatalExceptionWithMessage(error, message);
		}

		/// <summary>
		/// Bring up a "yellow box" that let's them send in a report, then return to the program.
		/// </summary>
		public static void ReportNonFatalExceptionWithMessage(Exception error, string message, params object[] args)
		{
			s_previousNonFatalMessage = message;
			s_previousNonFatalException = error;
			_errorReporter.ReportNonFatalExceptionWithMessage(error, message, args);
		}

		/// <summary>
		/// Bring up a "yellow box" that let's them send in a report, then return to the program.
		/// Use this one only when you don't have an exception (else you're not reporting the exception's message)
		/// </summary>
		public static void ReportNonFatalMessageWithStackTrace(string message, params object[] args)
		{
			s_previousNonFatalMessage = message;
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
				ErrorReport.s_previousNonFatalException = exception;
				return;
			}
			_errorReporter.ReportNonFatalException(exception, policy);
			UsageReporter.ReportException(false, null, exception, null);
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

		public static void Reset()
		{
			_alreadyReportedMessages.Clear();
		}
	}
}
