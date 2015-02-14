using System;
using System.Windows.Forms;
using SIL.Reporting;

namespace SIL.WindowsForms.ImageToolbox
{
	public partial class ImageToolboxDialog : Form
	{
		/// <summary>
		///
		/// </summary>
		/// <param name="imageInfo">optional (can be null)</param>
		/// <param name="initialSearchString">optional</param>
		public ImageToolboxDialog(PalasoImage imageInfo, string initialSearchString)
		{
			 InitializeComponent();
			imageToolboxControl1.ImageInfo = imageInfo;
			imageToolboxControl1.InitialSearchString = initialSearchString;
		}
		public PalasoImage ImageInfo { get { return imageToolboxControl1.ImageInfo; } }

		private void _okButton_Click(object sender, EventArgs e)
		{
			//enhance: doesn't tell us all that much.
			UsageReporter.SendNavigationNotice("ImageToolboxDialog/Ok");
			DialogResult = (ImageInfo==null || ImageInfo.Image==null)? DialogResult.Cancel : DialogResult.OK;
			imageToolboxControl1.Closing();
			Close();
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			imageToolboxControl1.Closing();
			Close();
		}

		private void ImageToolboxDialog_Load(object sender, EventArgs e)
		{
			UsageReporter.SendNavigationNotice("ImageToolbox");
		}
	}
}
