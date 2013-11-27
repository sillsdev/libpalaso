using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Palaso.Extensions
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
	}
}
