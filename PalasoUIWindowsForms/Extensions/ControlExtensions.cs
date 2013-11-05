using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.Extensions
{
	public static class ControlExtensions
	{
		/// <summary>
		/// the built-in "DesignMode" doesn't work when the control is in someone else's design
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
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

		/// <summary>
		/// If the method might be in a different thread, this will do an invoke,
		/// otherwise it just invokes the action
		/// </summary>
		/// <example>InvokeIfRequired(()=>BackgroundColor=Color.Blue);</example>
		/// <example>((Control)this).InvokeIfRequired(()=>SetChoices(languages));</example>
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
	}
}
