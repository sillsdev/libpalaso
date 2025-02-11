using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Reflection;

namespace SIL.Reporting
{

	public interface IErrorReporter
	{
		void ReportFatalException(Exception e);

		/// <summary>
		/// Notify the user of the <paramref name="exception"/> and <paramref name="message"/>, if <paramref name="policy"/> permits, using the IErrorReporter's default options
		/// </summary>
		/// <remarks>In contrast to the <see cref="SIL.Reporting.IErrorReporter.NotifyUserOfProblem(IRepeatNoticePolicy, string, ErrorResult, string)" /> version,
		/// this method overload does not require the implementation to block (if the caller is not on the UI thread)
		/// I would suggest that it should not block (if the caller is not on the UI thread).
		/// By not blocking, you reduce headache and worry about the application deadlocking.
		/// This will allow the caller to do an easy "fire and forget" of the problem notification mechanism</remarks>
		void NotifyUserOfProblem(IRepeatNoticePolicy policy, Exception exception, string message);

		/// <summary>
		/// Notify the user of the <paramref name="message"/>, if <paramref name="policy"/> permits. Customize the alternate button label. Wait for user input. Return button clicked to the user.
		/// </summary>
		/// <remarks>The method overload should block and wait for the user to press the required button</remarks>
		/// <returns>The method should return <paramref name="resultIfAlternateButtonPressed"/> if the alternate button is clicked, ErrorResult.OK otherwise</returns>
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
		/// Gets the current IErrorReporter
		/// </summary>
		public static IErrorReporter GetErrorReporter() => _errorReporter;

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

				return $"Version {ver.Major}.{ver.Minor}.{ver.Build} Built on {fi.CreationTime:dd-MMM-yyyy}";
			}
		}

		/// <remarks>
		/// https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed?view=netframework-4.8
		/// Beginning with .NET 4.5, Environment.Version is all but deprecated. The recommended way to determine which .NET Framework versions are
		/// installed is to query the registry for a key whose path contains "v4", ensuring that upgrading to .NET 5 will be a breaking change.
		/// </remarks>
		public static string DotNet4VersionFromWindowsRegistry()
		{
#if NETSTANDARD
			return ".NET Standard";
#else
			if (!Platform.IsWindows)
				return string.Empty;

			using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"))
			{
				return key == null ? "(unable to determine)" : $"{key.GetValue("Version")} ({key.GetValue("Release")})";
			}
#endif
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
		public static Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

		public static bool IsOkToInteractWithUser { get; set; } = true;

		/// <summary>
		/// Software using ErrorReport's NotifyUserOfProblem methods can set this to their own method
		/// for handling when the user clicks the "Details" button.
		/// </summary>
		public static Action<Exception, string> OnShowDetails
		{
			get
			{
				return s_onShowDetails ?? (s_onShowDetails = ReportNonFatalExceptionWithMessage);
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
			if (Properties.ContainsKey(label))
			{
				Properties.Remove(label);
			}

			Properties.Add(label, contents);
		}

		public static void AddStandardProperties()
		{
			AddProperty("Version", GetVersionForErrorReporting());
			AddProperty("CommandLine", Environment.CommandLine);
			AddProperty("CurrentDirectory", Environment.CurrentDirectory);
			AddProperty("MachineName", Environment.MachineName);
			AddProperty("OSVersion", GetOperatingSystemLabel());
			if (Platform.IsUnix)
				AddProperty("DesktopEnvironment", Platform.DesktopEnvironmentInfoString);
			if (Platform.IsMono)
				AddProperty("MonoVersion", Platform.MonoVersion);
			else
				AddProperty("DotNetVersion", DotNet4VersionFromWindowsRegistry());
			// https://docs.microsoft.com/en-us/dotnet/api/system.environment.version?view=netframework-4.8 warns that using the Version property is
			// no longer recommended for .NET Framework 4.5 or later. I'm leaving it in in hope that it may once again become useful. Note that, as
			// of .NET Framework 4.8, it is *not* marked as deprecated.
			// https://www.mono-project.com/docs/about-mono/versioning/#framework-versioning (accessed 2020-04-02) states that
			// "Mono's System.Environment.Version property [. . .] should be the same version number that .NET would return."
			AddProperty("CLR Version (deprecated)", Environment.Version.ToString());
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
			props.Add("Version", GetVersionForErrorReporting());
			props.Add("CommandLine", Environment.CommandLine);
			props.Add("CurrentDirectory", Environment.CurrentDirectory);
			props.Add("MachineName", Environment.MachineName);
			props.Add("OSVersion", GetOperatingSystemLabel());
			if (Platform.IsUnix)
				props.Add("DesktopEnvironment", Platform.DesktopEnvironmentInfoString);
			if (Platform.IsMono)
				props.Add("MonoVersion", Platform.MonoVersion);
			else
				props.Add("DotNetVersion", DotNet4VersionFromWindowsRegistry());
			props.Add("CLR Version (deprecated)", Environment.Version.ToString());
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
					{
						// From: https://stackoverflow.com/questions/69038560/detect-windows-11-with-net-framework-or-windows-api
						if (version.Label == "Windows 10" && Environment.OSVersion.Version.Build >= 22000)
						{
							return "Windows 11+ " + Environment.OSVersion.ServicePack + " (" + Environment.OSVersion.Version + ")";
						}
						return version.Label + " " + Environment.OSVersion.ServicePack + " (" + Environment.OSVersion.Version + ")";
					}
				}

				// Handle any as yet unrecognized (possibly unmanifested) versions, or anything that reported its self as Windows 8.
				if(Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					return GetWindowsVersionInfoFromNetworkAPI() + " " + Environment.OSVersion.ServicePack;
				}
			}
			return Environment.OSVersion.VersionString;
		}

		public static string GetHierarchicalExceptionInfo(Exception error, ref Exception innerMostException)
		{
			UpdateEmailSubject(error);
			return ExceptionHelper.GetHierarchicalExceptionInfo(error, ref innerMostException);
		}

		public static void ReportFatalException(Exception error)
		{
			UsageReporter.ReportException(true, null, error, null);
			_errorReporter.ReportFatalException(error);
		}

		/// <summary>
		/// Put up a message box, unless OkToInteractWithUser is false, in which case throw an Application Exception.
		/// This will not report the problem to the developer.  Use one of the "report" methods for that.
		/// </summary>
		public static void NotifyUserOfProblem(string message, params object[] args)
		{
			NotifyUserOfProblem(new ShowAlwaysPolicy(), message, args);
		}

		public static ErrorResult NotifyUserOfProblem(IRepeatNoticePolicy policy, string messageFmt, params object[] args)
		{
			NotifyUserOfProblem(policy, null, messageFmt, args);
			return ErrorResult.OK;
		}

		public static void NotifyUserOfProblem(Exception exception, string messageFmt, params object[] args)
		{
			NotifyUserOfProblem(new ShowAlwaysPolicy(), exception, messageFmt, args);
		}

		public static void NotifyUserOfProblem(IRepeatNoticePolicy policy, Exception exception, string messageFmt, params object[] args)
		{
			var message = string.Format(messageFmt, args);
			NotifyUserOfProblemWrapper(message, exception, () =>
			{
				_errorReporter.NotifyUserOfProblem(policy, exception, message);
				return ErrorResult.OK;
			});
		}

		public static ErrorResult NotifyUserOfProblem(IRepeatNoticePolicy policy,
									string alternateButton1Label,
									ErrorResult resultIfAlternateButtonPressed,
									string messageFmt,
									params object[] args)
		{
			var message = string.Format(messageFmt, args);
			return NotifyUserOfProblemWrapper(message, null, () =>
			{
				return _errorReporter.NotifyUserOfProblem(policy, alternateButton1Label, resultIfAlternateButtonPressed, message);
			});
		}

		/// <summary>
		/// Subclasses should call this method if they offer another overload of NotifyUserOfProblem.
		/// This will call their NotifyUserOfProblem functionality, plus the before-and-after logic for them, notably:
		/// * Checks if s_justRecordNonFatalMessagesForTesting is true and if so, skips calling the notification/reporting logic.
		/// * Calls UsageReporter.ReportException if exception is non-null
		/// </summary>
		/// <param name="message">The message that would be showed to the user</param>
		/// <param name="exception">The associated exception. May be null.</param>
		/// <param name="notifyUserOfProblem">A delegate that takes no arguments.
		/// It should call (via a closure) the actual core logic of having the _errorReporter notify the user of the problem.
		/// It should return the ErrorResult that is desired to be passed back to the caller</param>
		/// <returns>Returns the same return result as the result of NotifyDelegate</returns>
		protected static ErrorResult NotifyUserOfProblemWrapper(string message, Exception exception, Func<ErrorResult> notifyUserOfProblem)
		{
			if (s_justRecordNonFatalMessagesForTesting)
			{
				s_previousNonFatalMessage = message;
				return ErrorResult.OK;
			}

			var returnResult = notifyUserOfProblem();

			if (exception != null)
			{
				UsageReporter.ReportException(false, null, exception, message);
			}
			return returnResult;
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
		string ReoccurrenceMessage
		{
			get;
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

		public string ReoccurrenceMessage
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

		public string ReoccurrenceMessage
		{
			get { return "This message will not be shown again this session."; }
		}

		public static void Reset()
		{
			_alreadyReportedMessages.Clear();
		}
	}
}
