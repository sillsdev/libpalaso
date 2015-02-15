using System.Drawing;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Extensions
{
	public static class FormExtensions
	{
		/// ------------------------------------------------------------------------------------
		public static void CenterFormInScreen(this Form frm)
		{
			Rectangle rc = Screen.GetWorkingArea(frm);
			if (rc == Rectangle.Empty)
				rc = Screen.PrimaryScreen.WorkingArea;

			if (frm.Width > rc.Width)
				frm.Width = rc.Width;

			if (frm.Height > rc.Height)
				frm.Height = rc.Height;

			frm.Location = new Point((rc.Width - frm.Width) / 2, (rc.Height - frm.Height) / 2);
		}
	}
}
