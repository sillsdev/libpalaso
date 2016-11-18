using System;
using System.Drawing;
using System.Windows.Forms;
using L10NSharp;
using SIL.IO;
using SIL.Reporting;

namespace SIL.Windows.Forms.FileSystem
{
	public partial class ConfirmRecycleDialog : Form
	{
		public string LabelForThingBeingDeleted { get; set; }

		public ConfirmRecycleDialog()
		{
			InitializeComponent();
			_messageLabel.Font = SystemFonts.MessageBoxFont;
		}

		public ConfirmRecycleDialog(string labelForThingBeingDeleted, bool multipleItems = false, string localizationManagerId = null)
			: this()
		{
			if (!string.IsNullOrEmpty(localizationManagerId))
				_L10NSharpExtender.LocalizationManagerId = localizationManagerId;

			LabelForThingBeingDeleted = labelForThingBeingDeleted.Trim();
			string msgFmt = multipleItems ? LocalizationManager.GetString("DialogBoxes.ConfirmRecycleDialog.MessageForMultipleItems",
				"{0} will be moved to the Recycle Bin.", 
				"Param {0} is a description of the things being deleted (e.g., \"The selected files\")") : _messageLabel.Text;
			_messageLabel.Text = string.Format(msgFmt, LabelForThingBeingDeleted);
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

		public static bool JustConfirm(string labelForThingBeingDeleted, bool multipleItems = false, string localizationManagerId = null)
		{
			using (var dlg = new ConfirmRecycleDialog(labelForThingBeingDeleted, multipleItems, localizationManagerId))
			{
				return DialogResult.OK == dlg.ShowDialog();
			}
		}

		public static bool ConfirmThenRecycle(string labelForThingBeingDeleted, string pathToRecycle, bool multipleItems = false, string localizationManagerId = null)
		{
			using (var dlg = new ConfirmRecycleDialog(labelForThingBeingDeleted, multipleItems, localizationManagerId))
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
				return PathUtilities.DeleteToRecycleBin(path);
			}
			catch (Exception exception)
			{
				ErrorReport.NotifyUserOfProblem(exception,
					"Could not delete file or directory {0}.", path);
				return false;
			}
		}
	}
}
