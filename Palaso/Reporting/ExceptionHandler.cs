using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;


namespace Palaso.Reporting
{
	/// ----------------------------------------------------------------------------------------
	public abstract class ExceptionHandler
	{
		/// ------------------------------------------------------------------------------------
		public delegate void CancelExceptionHandlingEventHandler(object sender, CancelExceptionHandlingEventArgs e);

		private readonly HashSet<CancelExceptionHandlingEventHandler> _errorHandlerDelegates =
			new HashSet<CancelExceptionHandlingEventHandler>();

		private static ExceptionHandler _singleton;


		/// ------------------------------------------------------------------------------------
		//We removed all references to Winforms from Palaso.dll but our error reporting relied heavily on it.
		//Not wanting to break existing applications we have now added this class initializer which will
		//look for a reference to PalasoUIWindowsForms in the consuming app and if it exists instantiate the
		//ExceptionHandler from there through Reflection. Otherwise we will simply use a console
		//exception handler
		/// <summary>
		/// Initialize the ExceptionHandler. By default, the exceptionhandler will be initialized with a ConsoleExceptionHandler
		/// unless he entry assembly uses a dependency on PalasoUIWinForms.dll. In that case we default to the WinFormsExceptionHandler
		/// </summary>
		public static void Init()
		{
			if (_singleton == null)
			{
				var topMostAssembly = Assembly.GetEntryAssembly();
				if (topMostAssembly != null)
				{
					var referencedAssemblies = topMostAssembly.GetReferencedAssemblies();
					var palasoUiWindowsFormsInializeAssemblyName =
						referencedAssemblies.FirstOrDefault(a => a.Name.Contains("PalasoUIWindowsForms"));
					if (palasoUiWindowsFormsInializeAssemblyName != null)
					{
						var toInitializeAssembly = Assembly.Load(palasoUiWindowsFormsInializeAssemblyName);
						//Make this go find the actual winFormsErrorReporter as opposed to looking for the interface
						var interfaceToFind = typeof (ExceptionHandler);
						var typeImplementingInterface =
							toInitializeAssembly.GetTypes().Where(p => interfaceToFind.IsAssignableFrom(p));
						foreach (var type in typeImplementingInterface)
						{
							_singleton = type.GetConstructor(Type.EmptyTypes).Invoke(null) as ExceptionHandler;
						}
					}
					//If we can't find the WinFormsExceptionHandler we'll use the Console
					if (_singleton == null)
					{
						_singleton = new ConsoleExceptionHandler();
					}
				}
			}
			else { throw new InvalidOperationException("An ExceptionHandler has already been set."); }
		}

		/// <summary>
		/// Use this method if you want to use an exeption handler besides the default.
		/// This method should be called only once
		/// </summary>
		/// <param name="handler"></param>
		public static void Init(ExceptionHandler handler)
		{
			if (_singleton == null)
			{
				_singleton = handler;
			}
			else{throw new InvalidOperationException("An ExceptionHandler has already been set.");}
		}

		/// ------------------------------------------------------------------------------------
		public static bool Suspend { set; get; }

		/// ------------------------------------------------------------------------------------
		public static bool AddDelegate(CancelExceptionHandlingEventHandler errorHandlerDelegate)
		{
			return (errorHandlerDelegate != null && _singleton != null &&
				_singleton._errorHandlerDelegates.Add(errorHandlerDelegate));
		}

		/// ------------------------------------------------------------------------------------
		public static bool RemoveDelegate(CancelExceptionHandlingEventHandler errorHandlerDelegate)
		{
			return (_singleton != null && _singleton._errorHandlerDelegates.Remove(errorHandlerDelegate));
		}

		/// ------------------------------------------------------------------------------------
		protected bool GetShouldHandleException(object sender, Exception error)
		{
			if (Suspend)
				return false;

			foreach (var errHandler in _errorHandlerDelegates)
			{
				var args = new CancelExceptionHandlingEventArgs(error);
				errHandler(sender, args);
				if (args.Cancel)
					return false;
			}

			return true;
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
		protected abstract bool ShowUI { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Displays the error.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		protected abstract bool DisplayError(Exception exception);
	}

	/// ----------------------------------------------------------------------------------------
	public class CancelExceptionHandlingEventArgs : CancelEventArgs
	{
		/// ------------------------------------------------------------------------------------
		public Exception Exception { get; private set; }

		/// ------------------------------------------------------------------------------------
		public CancelExceptionHandlingEventArgs(Exception error)
		{
			Exception = error;
		}
	}
}
