using System;
using System.Drawing;
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

	}
}
