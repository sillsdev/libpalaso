using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ClearShare.WinFormsUI
{
	public partial class MetadataEditorDialog : Form
	{
		public MetadataEditorDialog(Metadata metaData)
		{
			InitializeComponent();
			metdataEditorControl1.Metadata = metaData;  //TODO: this approach means that cancel wouldn't really cancel.
		}


		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
