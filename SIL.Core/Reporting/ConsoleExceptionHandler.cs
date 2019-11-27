using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

		protected override bool ShowUI => false;

		protected override bool DisplayError(Exception exception)
		{
			try
			{
				if (exception.InnerException != null)
				{
					Debug.WriteLine(
						$"Exception: {exception.Message} {exception.InnerException.Message}");
					Logger.WriteError(exception.Message, exception.InnerException);
				}
				else
				{
					Debug.WriteLine($"Exception: {exception.Message}");
					Logger.WriteError(exception.Message, exception);
				}

				if (exception is ExternalException externalException
					&& (uint) (externalException.ErrorCode) == 0x8007000E) // E_OUTOFMEMORY
				{
					Debug.WriteLine("Out of memory");
					Logger.WriteEvent("Out of memory");
				}
				else
				{
					Debug.Assert(
						exception.Message != string.Empty || exception is COMException,
						"Oops - we got an empty exception description. Change the code to handle that!");
				}
			}
			catch (Exception)
			{
				Logger.WriteEvent($"This error could not be reported normally: ${exception.Message}");
				Debug.Fail("This error could not be reported normally: ", exception.Message);
			}

			return true;
		}
	}
}
