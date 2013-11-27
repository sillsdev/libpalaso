using System;
using System.Linq;


namespace Palaso.Progress
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

			foreach (var frm in Application.OpenForms.Cast<Form>())
			{
				if (frm.InvokeRequired)
					frm.Invoke(new Action(() => frm.Cursor = (turnOn ? Cursors.WaitCursor : Cursors.Default)));
				else
					frm.Cursor = (turnOn ? Cursors.WaitCursor : Cursors.Default);
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
