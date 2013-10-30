using System;
using System.Linq;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.Miscellaneous
{
	/// ----------------------------------------------------------------------------------------
	public class WaitCursor
	{
		/// ------------------------------------------------------------------------------------
		public static void Show()
		{
			ToggleWaitCursorState(true);
		}

		/// ------------------------------------------------------------------------------------
		public static void Hide()
		{
			ToggleWaitCursorState(false);
		}

		/// ------------------------------------------------------------------------------------
		private static void ToggleWaitCursorState(bool turnOn)
		{
			Application.UseWaitCursor = turnOn;

			foreach (var frm in Application.OpenForms.Cast<Form>().ToList())
			{
				Form form = frm; // Avoid resharper message about accessing foreach variable in closure.
				try
				{
					if (form.InvokeRequired)
						form.Invoke(new Action(() => form.Cursor = (turnOn ? Cursors.WaitCursor : Cursors.Default)));
					else
						form.Cursor = (turnOn ? Cursors.WaitCursor : Cursors.Default);
				}
				catch
				{
					// Form may have closed and been disposed. Oh, well.
				}
			}

			try
			{
				// I hate doing this, but setting the cursor property in .Net
				// often doesn't otherwise take effect until it's too late.
				Application.DoEvents();
			}
			catch { }
		}
	}
}
