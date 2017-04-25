using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SIL.Code;

namespace SIL.Windows.Forms.Extensions
{
	public static class ControlExtensions
	{
#if !__MonoCS__
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern void SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
#else
		private static void SendMessage(IntPtr hWnd, int msg, int wParam, int lParam)
		{
			if(msg != PaintingHelper.WM_NCPAINT) { // repaint
				Console.WriteLine("Warning--using unimplemented method SendMessage"); // FIXME Linux
			}
			return;
		}
#endif

#if !__MonoCS__
		[DllImport("user32")]
		private static extern int UpdateWindow(IntPtr hwnd);
#else
		private static int UpdateWindow(IntPtr hwnd)
		{
			Console.WriteLine("Warning--using unimplemented method UpdateWindow"); // FIXME Linux
			return(0);
		}
#endif

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
		/// otherwise it just invokes the action
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
		/// </remarks>
		public static void SafeInvoke(this Control control, Action action, string nameForErrorReporting = "context not supplied",
			ErrorHandlingAction errorHandling = ErrorHandlingAction.Throw, bool forceSynchronous = false)
		{
			Guard.AgainstNull(control, nameof(control)); // throw this one regardless of the errorHandling directive
			Guard.AgainstNull(action, nameof(action)); // throw this one regardless of the errorHandling directive

			if (control.IsDisposed)
			{
				if (errorHandling == ErrorHandlingAction.Throw)
					throw new ObjectDisposedException("SafeInvoke called after the control was disposed. (" + nameForErrorReporting + ")");
				return; // Caller asked to ignore this.
			}

			if (!control.InvokeRequired)
			{
				// InvokeRequired will return false if the control isn't set up yet
				if (!control.IsHandleCreated)
				{
					if (errorHandling == ErrorHandlingAction.IgnoreAll)
						return;
					throw new InvalidOperationException("SafeInvoke called before the control's handle was created. (" + nameForErrorReporting + ")");

					// Resist the temptation to work around this by just making the handle be created with something like
					// var unused = control.Handle
					// This can create the handle on the wrong thread and make the application unstable. (I believe it would crash instantly on Linux.)
				}
				// Technically, if forceSynchronous is false, we could call control.BeginInvoke, but then we'd need to wrap it in a
				// delegate (as above) to re-check to see if the control has been disposed. Probably not worth it. If we're already
				// on the UI thread, even if synchronous execution wasn't required, it should be okay.
				action();
			}
			else
			{
				bool treatInvalidOperationExceptionAsObjectDisposedException = true;
				try
				{
					if (forceSynchronous)
					{
						var synchronousAction = (Action)delegate
						{
							treatInvalidOperationExceptionAsObjectDisposedException = false;
							action();
						};
						control.Invoke(synchronousAction);
					}
					else
						control.BeginInvoke(action);
				}
				catch (InvalidOperationException e)
				{
					if (treatInvalidOperationExceptionAsObjectDisposedException)
					{
						if (errorHandling == ErrorHandlingAction.Throw)
							throw new ObjectDisposedException("SafeInvoke called after the control was disposed. (" + nameForErrorReporting + ")", e);
					}
					else
						throw;
				}
			}
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
#if !__MonoCS__
				SendMessage(ctrl.Handle, WM_SETREDRAW, (turnOn ? 1 : 0), 0);
#else
				if (turnOn)
					ctrl.ResumeLayout(invalidateAfterTurningOn);
				else
					ctrl.SuspendLayout();
#endif
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
