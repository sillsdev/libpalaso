using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using SIL.Reporting;

namespace SIL.Windows.Forms.Reporting
{
	public class WinFormsExceptionHandler: ExceptionHandler
	{
		// see comment on ExceptionReportingDialog.s_reportDataStack
		internal static Control ControlOnUIThread { get; private set; }

		internal static bool InvokeRequired
		{
			get
			{
				return !ControlOnUIThread.IsDisposed && ControlOnUIThread.InvokeRequired;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Set exception handler. Needs to be done before we create splash screen (don't
		/// understand why, but otherwise some exceptions don't get caught).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public WinFormsExceptionHandler()
		{
			// We need to create a control on the UI thread so that we have a control that we
			// can use to invoke the error reporting dialog on the correct thread.
			ControlOnUIThread = new Control();
			ControlOnUIThread.CreateControl();

			// Using Application.ThreadException rather than
			// AppDomain.CurrentDomain.UnhandledException has the advantage that the
			// program doesn't necessarily end - we can ignore the exception and continue.
			Application.ThreadException += HandleTopLevelError;

			// We also want to catch the UnhandledExceptions for all the cases that
			// ThreadException doesn't catch, e.g. in the startup.
			AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Catches and displays a otherwise unhandled exception.
		/// </summary>
		/// <param name="sender">sender</param>
		/// <param name="e">Exception</param>
		/// <remarks>previously <c>AfApp::HandleTopLevelError</c></remarks>
		/// ------------------------------------------------------------------------------------
		protected void HandleTopLevelError(object sender, ThreadExceptionEventArgs e)
		{
			if (!GetShouldHandleException(sender, e.Exception))
				return;

			if (DisplayError(e.Exception))
			{
				//Are we inside a Application.Run() statement?
				if (Application.MessageLoop)
					Application.Exit();
				else
					Environment.Exit(1); //the 1 here is just non-zero
			}
		}

		protected override bool ShowUI
		{
			get
			{
				{
					string strShowUI = ConfigurationManager.AppSettings["ShowUI"];
					bool fShowUI = true;
					try
					{
						if (strShowUI != null)
							fShowUI = Convert.ToBoolean(strShowUI);
					}
					catch
					{
					}
					return fShowUI;
				}
			}
		}

		protected override bool DisplayError(Exception exception)
		{
			UsageReporter.ReportException(false, null, exception, null);
			try
			{
				// To disable displaying a message box, put
				// <add key="ShowUI" value="False"/>
				// in the <appSettings> section of the .config file (see MSDN for details).
				bool showUI = ShowUI;

				if (exception.InnerException != null)
				{
					Debug.WriteLine(string.Format("Exception: {0} {1}", exception.Message, exception.InnerException.Message));
					Logger.WriteEvent("Exception: {0} {1}", exception.Message, exception.InnerException.Message);
				}
				else
				{
					Debug.WriteLine(String.Format("Exception: {0}", exception.Message));
					Logger.WriteEvent("Exception: {0}", exception.Message);
				}

				if (exception is ExternalException
					&& (uint)(((ExternalException)exception).ErrorCode) == 0x8007000E) // E_OUTOFMEMORY
				{
					if (showUI)
						ExceptionReportingDialog.ReportException(exception);//, parent);
					else
					{
						Trace.Fail("Out of memory");
						//                        Trace.Assert(false, FwApp.GetResourceString("kstidMiscError"),
						//                            FwApp.GetResourceString("kstidOutOfMemory"));
					}
				}
				else
				{
					Debug.Assert(exception.Message != string.Empty || exception is COMException,
						"Oops - we got an empty exception description. Change the code to handle that!");

					if (showUI)
					{
						// bool fIsLethal = !(exception is Reporting.ConfigurationException);
						//ErrorReporter.ReportException(exception, parent, fIsLethal);
						ExceptionReportingDialog.ReportException(exception);
						return false;
					}
					else
					{
						//todo: the reporting component should do all this, too
						/*                       Exception innerE = ExceptionHelper.GetInnerMostException(exception);
												string strMessage = "Error: "; // FwApp.GetResourceString("kstidProgError") + FwApp.GetResourceString("kstidFatalError");
												string strVersion;
												Assembly assembly = Assembly.GetEntryAssembly();
												object[] attributes = null;
												if (assembly != null)
													attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
												if (attributes != null && attributes.Length > 0)
													strVersion = ((AssemblyFileVersionAttribute)attributes[0]).Version;
												else
													strVersion = Application.ProductVersion;
												string strReport = string.Format(
													"Got Exception", //"FwApp.GetResourceString("kstidGotException"),
													"errors@wesay.org", //"FwApp.GetResourceString("kstidSupportEmail"),

													exception.Source, strVersion,
													ExceptionHelper.GetAllExceptionMessages(exception), innerE.Source,
													innerE.TargetSite.Name, ExceptionHelper.GetAllStackTraces(exception));
												Trace.Assert(false, strMessage, strReport);
						*/

						Debug.Fail(exception.Message);
					}
				}

			}
			catch (Exception)
			{
				Debug.Fail("This error could not be reported normally: ", exception.Message);
			}
			return true;
		}

		public static void DoNotCallThisMethod()
		{
			// SP-835: The Debug.Fail code is running in what is supposed to be Release code
			Debug.Fail("This is a DEBUG build.");
		}
	}
}
