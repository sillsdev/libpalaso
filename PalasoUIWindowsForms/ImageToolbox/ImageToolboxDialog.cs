using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public partial class ImageToolboxDialog : Form
	{
		public ImageToolboxDialog(PalasoImage imageInfo)
		{
			 InitializeComponent();
			imageToolboxControl1.ImageInfo = imageInfo;
		}
		public PalasoImage ImageInfo { get { return imageToolboxControl1.ImageInfo; } }

		private void _okButton_Click(object sender, EventArgs e)
		{
		  DialogResult = DialogResult.OK;
			imageToolboxControl1.Closing();
			Close();
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			imageToolboxControl1.Closing();
			Close();
		}
	}
}
