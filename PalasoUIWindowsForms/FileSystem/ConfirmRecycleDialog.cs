using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.FileSystem
{
	public partial class ConfirmRecycleDialog : Form
	{
		/// <summary>
		///
		/// </summary>
		/// <param name="labelForThingBeingDeleted">e.g. "This book"</param>
		public ConfirmRecycleDialog(string labelForThingBeingDeleted)
		{
			LabelForThingBeingDeleted = labelForThingBeingDeleted.Trim();
			Font = SystemFonts.MessageBoxFont;
			InitializeComponent();
			_messageLabel.BackColor = this.BackColor;
		}
		public string LabelForThingBeingDeleted { get; set; }

		private void deleteBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Close();

		}

		private void ConfirmDelete_BackColorChanged(object sender, EventArgs e)
		{
			_messageLabel.BackColor = this.BackColor;
		}

		private void ConfirmDelete_Load(object sender, EventArgs e)
		{
			_messageLabel.Text = string.Format(_messageLabel.Text, LabelForThingBeingDeleted);
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
									Directory.Delete(item.FolderPath);
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
