using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SIL.Reporting
{
	/// ----------------------------------------------------------------------------------------
	public abstract class ExceptionHandler
	{
		/// ------------------------------------------------------------------------------------
		public delegate void CancelExceptionHandlingEventHandler(object sender, CancelExceptionHandlingEventArgs e);

		private readonly HashSet<CancelExceptionHandlingEventHandler> _errorHandlerDelegates =
			new HashSet<CancelExceptionHandlingEventHandler>();

		private static ExceptionHandler _singleton;

		// ------------------------------------------------------------------------------------
		//We removed all references to Winforms from SIL.Core.dll but our error reporting relied heavily on it.
		//Not wanting to break existing applications we have now added this class initializer which will
		//look for a reference to SIL.Windows.Forms in the consuming app and if it exists instantiate the
		//ExceptionHandler from there through Reflection. Otherwise we will simply use a console
		//exception handler
		/// <summary>
		/// Initialize the ExceptionHandler. By default, the exception handler will be initialized with a ConsoleExceptionHandler
		/// unless the entry assembly uses a dependency on SIL.Windows.Forms.dll. In that case we default to the WinFormsExceptionHandler
		/// </summary>
		[Obsolete("Use Init(ExceptionHandler) instead, e.g. Init(new ConsoleExceptionHandler()) or Init(new WinFormsExceptionHandler())")]
		public static void Init()
		{
			if (_singleton != null)
				throw new InvalidOperationException($"An ExceptionHandler (of type ${_singleton.GetType()}) has already been set.");

			//If we can't find the WinFormsExceptionHandler we'll use the Console
			_singleton = GetObjectFromSilWindowsForms<ExceptionHandler>() ?? new ConsoleExceptionHandler();
		}

		/// <summary>
		/// Initialize the ExceptionHandler. This method should be called only once.
		/// </summary>
		/// <param name="handler">The ExceptionHandler object. Predefined exception handlers are
		/// ConsoleExceptionHandler and WinFormsExceptionHandler.</param>
		public static void Init(ExceptionHandler handler)
		{
			if (_singleton != null)
				throw new InvalidOperationException($"An ExceptionHandler (of type ${_singleton.GetType()}) has already been set.");

			_singleton = handler;
		}

		// ReSharper disable once MemberCanBePrivate.Global
		/// <summary>
		/// Get all the types we can load from the assembly.
		/// In case we can't load a particular type (e.g., TextBoxSpellChecker, because the
		/// application doesn't use it and is not installing Enchant.dll), just skip it.
		/// </summary>
		/// <returns>The types loaded.</returns>
		/// <param name="assembly">Assembly.</param>
		internal static Type[] GetLoadableTypes(Assembly assembly)
		{
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				types = e.Types.Where(t => t != null).ToArray();
			}

			return types;
		}

		internal static T GetObjectFromSilWindowsForms<T>() where T : class
		{
			const string silWindowsFormsAssemblyName = "SIL.Windows.Forms";

			var topMostAssembly = Assembly.GetEntryAssembly();
			if (topMostAssembly != null)
			{
				var referencedAssemblies = topMostAssembly.GetReferencedAssemblies();
				var silWindowsFormsInitializeAssemblyName =
					referencedAssemblies.FirstOrDefault(a => a.Name.Equals(silWindowsFormsAssemblyName));
				if (silWindowsFormsInitializeAssemblyName != null)
				{
					var toInitializeAssembly = Assembly.Load(silWindowsFormsInitializeAssemblyName);
					//TomB: this comment (presumably an idea for future enhancement) was in both versions of the code from which
					// this method was created, even though it really only seems to apply to the one that was formerly in
					// ErrorReport: "Make this go find the actual winFormsErrorReporter as opposed to looking for the interface"
					// Not sure exactly what the author of that comment had in mind (perhaps to look for an explicit type name),
					// but now that this method is called from two different places, it might be harder to do this.
					var interfaceToFind = typeof(T);
					var typeImplementingInterface = GetLoadableTypes(toInitializeAssembly).FirstOrDefault(
						t =>
						{
						try
						{
							// Even though we supposedly filtered all the types we can't load,
							// we STILL get an exception here when Enchant.dll is missing.
							// (Nor is just this check enough...GetTypes() indeed throws also.
							return interfaceToFind.IsAssignableFrom(t);
						}
						catch (TypeLoadException)
						{
							return false;
						}
						});
					if (typeImplementingInterface != null)
					{
						var winFormsExceptionHandlerConstructor = typeImplementingInterface.GetConstructor(Type.EmptyTypes);
						if (winFormsExceptionHandlerConstructor != null)
						{
							return winFormsExceptionHandlerConstructor.Invoke(null) as T;
						}
					}
				}
			}
			return null;
		}

		// ReSharper disable once MemberCanBePrivate.Global
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

		// ReSharper disable once MemberCanBePrivate.Global
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
		/// Catches and displays otherwise unhandled exception, especially those that happen
		/// during startup of the application before we show our main window.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exception = e.ExceptionObject as Exception;
			if (!GetShouldHandleException(sender, exception))
				return;

			DisplayError(exception ?? new ApplicationException("Got unknown exception"));
		}

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

		protected static void HandleUnhandledExceptionOnSingleton(UnhandledExceptionEventArgs e)
		{
			_singleton.HandleUnhandledException(null, e);
		}

		protected static void ResetSingleton()
		{
			_singleton = null;
		}
	}

	/// ----------------------------------------------------------------------------------------
	public class CancelExceptionHandlingEventArgs : CancelEventArgs
	{
		// ReSharper disable once MemberCanBePrivate.Global
		/// ------------------------------------------------------------------------------------
		public Exception Exception { get; private set; }

		/// ------------------------------------------------------------------------------------
		public CancelExceptionHandlingEventArgs(Exception error)
		{
			Exception = error;
		}
	}
}
