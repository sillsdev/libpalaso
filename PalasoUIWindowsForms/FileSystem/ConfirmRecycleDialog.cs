using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.FileSystem
{
	public partial class ConfirmRecycleDialog : Form
	{
		public string LabelForThingBeingDeleted { get; set; }

		public ConfirmRecycleDialog()
		{
			InitializeComponent();
			_messageLabel.Font = SystemFonts.MessageBoxFont;
		}

		public ConfirmRecycleDialog(string labelForThingBeingDeleted) : this()
		{
			LabelForThingBeingDeleted = labelForThingBeingDeleted.Trim();
			_messageLabel.Text = string.Format(_messageLabel.Text, LabelForThingBeingDeleted);

			// Sometimes, setting the text in the previous line will force the table layout control
			// to resize itself accordingly, which will fire its SizeChanged event. However,
			// sometimes the text is not long enough to force the table layout to be resized,
			// therefore, we need to call it manually, just to be sure the form gets sized correctly.
			HandleTableLayoutSizeChanged(null, null);
		}

		private void HandleTableLayoutSizeChanged(object sender, EventArgs e)
		{
			if (!IsHandleCreated)
				CreateHandle();

			var scn = Screen.FromControl(this);
			var desiredHeight = tableLayout.Height + Padding.Top + Padding.Bottom + (Height - ClientSize.Height);
			Height = Math.Min(desiredHeight, scn.WorkingArea.Height - 20);
		}

		protected override void OnBackColorChanged(EventArgs e)
		{
			base.OnBackColorChanged(e);
			_messageLabel.BackColor = BackColor;
		}

		private void deleteBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		public static bool JustConfirm(string labelForThingBeingDeleted)
		{
			using (var dlg = new ConfirmRecycleDialog(labelForThingBeingDeleted))
			{
				return DialogResult.OK == dlg.ShowDialog();
			}
		}

		public static bool ConfirmThenRecycle(string labelForThingBeingDeleted, string pathToRecycle)
		{
			using (var dlg = new ConfirmRecycleDialog(labelForThingBeingDeleted))
			{
				if (DialogResult.OK != dlg.ShowDialog())
					return false;
			}

			return Recycle(pathToRecycle);
		}

		/// <summary>
		/// Actually do the move of a file/directory to the recycleBin
		/// </summary>
		/// <param name="path"></param>
		/// <returns>true if it succeed.</returns>
		public static bool Recycle(string path)
		{
			try
			{
#if MONO
					// TODO: Find a way in Mono to send something to the recycle bin.
					Directory.Delete(path);
					return true;
#else

				//alternative using visual basic dll:  FileSystem.DeleteDirectory(item.FolderPath,UIOption.OnlyErrorDialogs), RecycleOption.SendToRecycleBin);

				//moves it to the recyle bin
				var shf = new SHFILEOPSTRUCT();
				shf.wFunc = FO_DELETE;
				shf.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION;
				string pathWith2Nulls = path + "\0\0";
				shf.pFrom = pathWith2Nulls;

				SHFileOperation(ref shf);
				return !shf.fAnyOperationsAborted;
				#endif

			}
			catch (Exception exception)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(exception, "Could not delete that book.");
				return false;
			}
		}

#if !MONO
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
		public struct SHFILEOPSTRUCT
		{
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.U4)]
			public int wFunc;
			public string pFrom;
			public string pTo;
			public short fFlags;
			[MarshalAs(UnmanagedType.Bool)]
			public bool fAnyOperationsAborted;
			public IntPtr hNameMappings;
			public string lpszProgressTitle;
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

		public const int FO_DELETE = 3;
		public const int FOF_ALLOWUNDO = 0x40;
		public const int FOF_NOCONFIRMATION = 0x10; // Don't prompt the user
		public const int FOF_SIMPLEPROGRESS = 0x0100;
#endif
	}
}
