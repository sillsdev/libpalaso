using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Palaso.Reporting
{
	public class ExceptionHandler
	{
		private static ExceptionHandler _singleton=null;

		public static void Init()
		{
			if (_singleton == null)
			{
				_singleton = new ExceptionHandler();
			}
		}

		protected ExceptionHandler()
		{

			 // Set exception handler. Needs to be done before we create splash screen
			// (don't understand why, but otherwise some exceptions don't get caught)
			// Using Application.ThreadException rather than
			// AppDomain.CurrentDomain.UnhandledException has the advantage that the program
			// doesn't necessarily ends - we can ignore the exception and continue.
			Application.ThreadException += new ThreadExceptionEventHandler(HandleTopLevelError);

			// we also want to catch the UnhandledExceptions for all the cases that
			// ThreadException don't catch, e.g. in the startup.
			AppDomain.CurrentDomain.UnhandledException +=
				new UnhandledExceptionEventHandler(HandleUnhandledException);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Catches and displays otherwise unhandled exception, especially those that happen
		/// during startup of the application before we show our main window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception)
				DisplayError(e.ExceptionObject as Exception);
			else
				DisplayError(new ApplicationException("Got unknown exception"));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Catches and displays a otherwise unhandled exception.
		/// </summary>
		/// <param name="sender">sender</param>
		/// <param name="eventArgs">Exception</param>
		/// <remarks>previously <c>AfApp::HandleTopLevelError</c></remarks>
		/// ------------------------------------------------------------------------------------
		protected void HandleTopLevelError(object sender, ThreadExceptionEventArgs eventArgs)
		{
			if (DisplayError(eventArgs.Exception))
			{
				//Are we inside a Application.Run() statement?
				if (System.Windows.Forms.Application.MessageLoop)
				{
					System.Windows.Forms.Application.Exit();
				}
				else
				{
					System.Environment.Exit(1); //the 1 here is just non-zero
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Shows the error message of the exception to the user.
		/// </summary>
		/// <returns><c>true</c> to exit application, <c>false</c> to continue</returns>
		/// ------------------------------------------------------------------------------------
		//        protected bool DisplayError(Exception exception)
		//        {
		//  //review
		//            //          Form form = (m_rgMainWindows.Count > 0 ? m_rgMainWindows[0] as Form : null);
		//    //        return DisplayError(exception, form);
		//            return DisplayError(exception, Application.m);
		//        }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the setting for displaying error message boxes. The value is retrieved from
		/// the .config file.
		/// </summary>
		/// <remarks>
		/// To disable displaying an error message box, put
		/// <code>&lt;add key="ShowUI" value="False"/></code>
		/// in the &lt;appSettings> section of the .config file (see MSDN for details).
		/// </remarks>
		/// ------------------------------------------------------------------------------------
		protected static bool ShowUI
		{
			get
			{
				string strShowUI =
					ConfigurationManager.AppSettings["ShowUI"];
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

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Displays the error.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		protected static bool DisplayError(Exception exception)//, Form parent)
		{
			try
			{
				// To disable displaying a message box, put
				// <add key="ShowUI" value="False"/>
				// in the <appSettings> section of the .config file (see MSDN for details).
				bool showUI = ShowUI;

				if (exception is ExternalException
					&& (uint)(((ExternalException)exception).ErrorCode) == 0x8007000E) // E_OUTOFMEMORY
				{
					if (showUI)
						ErrorNotificationDialog.ReportException(exception);//, parent);
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
						ErrorNotificationDialog.ReportException(exception);
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
			catch(Exception e)
			{
				Debug.Fail("Yikes. There was an error while trying to report an exception.",e.Message);
			}
			return true;
		}
	}
}
