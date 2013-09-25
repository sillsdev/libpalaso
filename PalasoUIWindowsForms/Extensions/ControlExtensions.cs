using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.Extensions
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
