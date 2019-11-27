using System;

namespace SIL.Reporting
{
	internal class ConsoleExceptionHandler: ExceptionHandler
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Set exception handler. Needs to be done before we create splash screen (don't
		/// understand why, but otherwise some exceptions don't get caught).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ConsoleExceptionHandler()
		{
			// We also want to catch the UnhandledExceptions for all the cases that
			// ThreadException don't catch, e.g. in the startup.
			AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
		}

		protected override bool ShowUI
		{
			get { return false; }
		}

		protected override bool DisplayError(Exception exception)
		{

			{
				//try
				//{
				//    if (exception.InnerException != null)
				//    {
				//        Debug.WriteLine(string.Format("Exception: {0} {1}", exception.Message, exception.InnerException.Message));
				//        Logger.WriteEvent("Exception: {0} {1}", exception.Message, exception.InnerException.Message);
				//    }
				//    else
				//    {
				//        Debug.WriteLine(String.Format("Exception: {0}", exception.Message));
				//        Logger.WriteEvent("Exception: {0}", exception.Message);
				//    }

				//    if (exception is ExternalException
				//        && (uint)(((ExternalException)exception).ErrorCode) == 0x8007000E) // E_OUTOFMEMORY
				//    {
				//        if (showUI)
				//            ExceptionReportingDialog.ReportException(exception);//, parent);
				//        else
				//        {
				//            Trace.Fail("Out of memory");
				//            //                        Trace.Assert(false, FwApp.GetResourceString("kstidMiscError"),
				//            //                            FwApp.GetResourceString("kstidOutOfMemory"));
				//        }
				//    }
				//    else
				//    {
				//        Debug.Assert(exception.Message != string.Empty || exception is COMException,
				//            "Oops - we got an empty exception description. Change the code to handle that!");

				//        if (showUI)
				//        {
				//            // bool fIsLethal = !(exception is Reporting.ConfigurationException);
				//            //ErrorReporter.ReportException(exception, parent, fIsLethal);
				//            ExceptionReportingDialog.ReportException(exception);
				//            return false;
				//        }
				//        else
				//        {
				//            //todo: the reporting component should do all this, too
				//            /*                       Exception innerE = ExceptionHelper.GetInnerMostException(exception);
				//                                   string strMessage = "Error: "; // FwApp.GetResourceString("kstidProgError") + FwApp.GetResourceString("kstidFatalError");
				//                                   string strVersion;
				//                                   Assembly assembly = Assembly.GetEntryAssembly();
				//                                   object[] attributes = null;
				//                                   if (assembly != null)
				//                                       attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
				//                                   if (attributes != null && attributes.Length > 0)
				//                                       strVersion = ((AssemblyFileVersionAttribute)attributes[0]).Version;
				//                                   else
				//                                       strVersion = Application.ProductVersion;
				//                                   string strReport = string.Format(
				//                                       "Got Exception", //"FwApp.GetResourceString("kstidGotException"),
				//                                       "errors@wesay.org", //"FwApp.GetResourceString("kstidSupportEmail"),

				//                                       exception.Source, strVersion,
				//                                       ExceptionHelper.GetAllExceptionMessages(exception), innerE.Source,
				//                                       innerE.TargetSite.Name, ExceptionHelper.GetAllStackTraces(exception));
				//                                   Trace.Assert(false, strMessage, strReport);
				//            */

				//            Debug.Fail(exception.Message);
				//        }
				//    }

				//}
				//catch (Exception)
				//{
				//    Debug.Fail("This error could not be reported normally: ", exception.Message);
				//}
				return true;
			}
		}
	}
}
