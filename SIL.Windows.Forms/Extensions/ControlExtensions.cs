using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SIL.Code;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Extensions
{
	public static class ControlExtensions
	{
		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
		private static extern void SendMessageWindows(IntPtr hWnd, int msg, int wParam, int lParam);

		private static void SendMessageLinux(IntPtr hWnd, int msg, int wParam, int lParam)
		{
			if(msg != PaintingHelper.WM_NCPAINT) { // repaint
				Console.WriteLine("Warning--using unimplemented method SendMessage"); // FIXME Linux
			}
		}

		private static void SendMessage(IntPtr hWnd, int msg, int wParam, int lParam)
		{
			if (Platform.IsWindows)
				SendMessageWindows(hWnd, msg, wParam, lParam);
			else
				SendMessageLinux(hWnd, msg, wParam, lParam);
		}

		[DllImport("user32.dll", EntryPoint = "UpdateWindow")]
		private static extern int UpdateWindowWindows(IntPtr hwnd);

		private static int UpdateWindowLinux(IntPtr hwnd)
		{
			Console.WriteLine("Warning--using unimplemented method UpdateWindow"); // FIXME Linux
			return 0;
		}

		private static int UpdateWindow(IntPtr hwnd)
		{
			return Platform.IsWindows ? UpdateWindowWindows(hwnd) : UpdateWindowLinux(hwnd);
		}

		private const int WM_SETREDRAW = 0xB;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// the built-in "DesignMode" doesn't work when the control is in someone else's design
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public static bool DesignModeAtAll(this Control control)
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return true;

			while (control != null)
			{
				if (control.Site != null && control.Site.DesignMode)
					return true;
				control = control.Parent;
			}
			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// If the method might be in a different thread, this will do an invoke,
		/// otherwise it just invokes the action. <seealso cref="SafeInvoke"/>
		/// </summary>
		/// <example>InvokeIfRequired(()=>BackgroundColor=Color.Blue);</example>
		/// <example>((Control)this).InvokeIfRequired(()=>SetChoices(languages));</example>
		/// ------------------------------------------------------------------------------------
		public static void InvokeIfRequired(this Control control, Action action)
		{
			if (control.InvokeRequired)
			{
				control.Invoke(action);
			}
			else
			{
				action();
			}
		}

		/// <summary>
		/// Calls BeginInvoke with the action. This overload can be used with a lambda expression.
		/// </summary>
		/// <example>control.BeginInvoke(() => DoSomething(x));</example>
		public static void BeginInvoke(this Control control, Action action)
		{
			control.BeginInvoke(action);
		}

		public enum ErrorHandlingAction
		{
			Throw,
			IgnoreIfDisposed,
			IgnoreAll,
		}

		/// <summary>
		/// Invoke an action on the UI thread even if called from the background thread. This method is more reliable
		/// than merely calling InvokeRequired() which, for example, gives a misleading answer if the control hasn't
		/// got a handle yet.
		/// </summary>
		/// <remarks>
		/// The exact behavior of some of the InvokeRequired method (and some of the related methods) has changed
		/// between different versions of .net. Here's the relevant page in msdn that explains the current situation:
		/// https://msdn.microsoft.com/en-us/library/system.windows.forms.control.invokerequired(v=vs.110).aspx
		/// This implementation began with http://stackoverflow.com/a/809186/723299, but allows the caller to specify the
		/// desired handling of various types of errors.
		/// This method does <i>not</i> catch and suppress errors thrown by the target action being invoked. If the caller
		/// wishes to have that behavior, the action should include the appropriate try-catch wrapper to achieve that.
		/// On Linux, it appears that when the action is to be run asynchronously (i.e., InvokeRequired returns true and
		/// forceSynchronous == false):
		/// * an ObjectDisposedException will just be ignored.
		/// * any other exception thrown by the target action will only be accessible by calling EndInvoke on the
		///   control by passing the IAsyncResult returned from this method.
		/// On Windows, both ObjectDisposedExceptions and any exceptions thrown by the action will cause the
		/// Application.ThreadException event to fire (as well as being thrown when EndInvoke is called).
		/// </remarks>
		public static IAsyncResult SafeInvoke(this Control control, Action action, string nameForErrorReporting = "context not supplied",
			ErrorHandlingAction errorHandling = ErrorHandlingAction.Throw, bool forceSynchronous = false)
		{
			Guard.AgainstNull(control, "control"); // throw this one regardless of the errorHandling directive
			Guard.AgainstNull(action, "action"); // throw this one regardless of the errorHandling directive

			if (!control.InvokeRequired)
			{
				if (control.IsDisposed || control.Disposing)
				{
					if (errorHandling == ErrorHandlingAction.Throw)
						throw new ObjectDisposedException("SafeInvoke called after the control was disposed. (" + nameForErrorReporting + ")");
					return null; // Caller asked to ignore this.
				}

				// InvokeRequired will return false if the control isn't set up yet
				if (!control.IsHandleCreated)
				{
					if (errorHandling == ErrorHandlingAction.IgnoreAll)
						return null;
					throw new InvalidOperationException("SafeInvoke called before the control's handle was created. (" + nameForErrorReporting + ")");

					// Resist the temptation to work around this by just making the handle be created with something like
					// var unused = control.Handle
					// This can create the handle on the wrong thread and make the application unstable. (I believe it would crash instantly on Linux.)
				}
				// Technically, if forceSynchronous is false, we could call control.BeginInvoke, but that just complicates our code
				// (another chance for the control to get disposed before action() happens), and by definition the order in which
				// things happen doesn't matter if we aren't forcing things to be synchronous.
				action();
			}
			else
			{
				// Unfortunately, if the control's handle is disposed (on the UI thread) between the time we check above and the time
				// we attempt to invoke the action on it below, the Invoke or BeginInvoke call will throw an InvalidOperationException,
				// not an ObjectDisposedException (which seems to make more sense). But in the case where we are invoking synchronously
				// (and the control is not disposed), it's possible that the action itself could throw an InvalidOperationException, and
				// we definitely wouldn't want to mistake that case for the special case of the Invoke command throwing the exception.
				// So we wrap the action in a simple delegate and once inside (on the UI thread), we clear this flag because any subsequent
				// exception is guaranteed to be caused by the action itself and not because the control had gotten disposed.
				bool treatInvalidOperationExceptionAsObjectDisposedException = true;
				try
				{
					// When this gets executed on the UI thread, we need to re-check IsDisposed because it might have gotten disposed
					// between the time we invoke and the time the action is executed. All we really need from SafeInvoke is that little
					// bit of code at the start that checks for IsDisposed, but re-using SafeInvoke for the delegate is probably better
					// than repeating that bit of code. Rechecking InvokeRequired should be lightning fast.
					var actionToInvoke = (Action)delegate
					{
						treatInvalidOperationExceptionAsObjectDisposedException = false;
						control.SafeInvoke(action, nameForErrorReporting, errorHandling, forceSynchronous);
					};
					if (forceSynchronous)
						control.Invoke(actionToInvoke);
					else
						return control.BeginInvoke(actionToInvoke);
				}
				catch (InvalidOperationException e)
				{
					if (treatInvalidOperationExceptionAsObjectDisposedException)
					{
						// This is to catch the case where the control gets disposed after the check at the start of this method but before
						// the Invoke or BeginInvoke executes. This does NOT happen if the control is disposed after the BeginInvoke is issued
						// but before the action is processed. In that case, the action will be dequeued (at least on Windows???), and its
						// entry will have an exception set.
						if (errorHandling == ErrorHandlingAction.Throw)
							throw new ObjectDisposedException("Control was disposed before " + (forceSynchronous ? "Invoke" : "BeginInvoke") +
								" could be called. To suppress this kind of race-condition error, call SafeInvoke with IgnoreIfDisposed. (" + nameForErrorReporting + ")", e);
					}
					else
						throw;
				}
			}
			return null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Turns window redrawing on or off. After turning on, the window will be invalidated.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void SetWindowRedraw(this Control ctrl, bool turnOn)
		{
			SetWindowRedraw(ctrl, turnOn, true);
		}

		/// ------------------------------------------------------------------------------------
		public static void SetWindowRedraw(this Control ctrl, bool turnOn,
			bool invalidateAfterTurningOn)
		{
			if (ctrl != null && !ctrl.IsDisposed && ctrl.IsHandleCreated)
			{
				if (Platform.IsWindows)
					SendMessage(ctrl.Handle, WM_SETREDRAW, (turnOn ? 1 : 0), 0);
				else
				{
					if (turnOn)
						ctrl.ResumeLayout(invalidateAfterTurningOn);
					else
						ctrl.SuspendLayout();
				}
				if (turnOn && invalidateAfterTurningOn)
					ctrl.Invalidate(true);
			}
		}

		/// ------------------------------------------------------------------------------------
		public static void SendMessage(this Control ctrl, int msg, int wParam, int lParam)
		{
			SendMessage(ctrl.Handle, msg, wParam, lParam);
		}

		/// ------------------------------------------------------------------------------------
		public static void UpdateWindow(this Control ctrl)
		{
			UpdateWindow(ctrl.Handle);
		}
	}
}
