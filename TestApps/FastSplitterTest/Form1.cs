using System;
using System.Windows.Forms;
using SIL.Xml;

namespace FastSplitterTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void BrowseClicked(object sender, EventArgs e)
		{
			string repeatingElementMarker = _tbRepeatingElementMarker.Text;
			if (string.IsNullOrWhiteSpace(repeatingElementMarker))
			{
				MessageBox.Show(this, "You must enter the name of the repeating xml element.", "Repeating element name is missing",
								MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			using (var fileDlg = new OpenFileDialog())
			{
				if (fileDlg.ShowDialog(this) != DialogResult.OK)
					return;
				_tbXmlFile.Text = fileDlg.FileName;
				if (string.IsNullOrWhiteSpace(_tbXmlFile.Text))
				{
					MessageBox.Show(this, "You must select the name of an xml file.", "Xml file is missing",
									MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}

			try
			{
				using (var fastSplitter = new FastXmlElementSplitter(_tbXmlFile.Text))
				{
					var optionalMarker = string.IsNullOrWhiteSpace(_tbOptionFirstElementMarker.Text)
											 ? null
											 : _tbOptionFirstElementMarker.Text;
					_ = fastSplitter.GetSecondLevelElementStrings(optionalMarker, repeatingElementMarker, out _);
					{ /* Do nothing with it. */ }
				}
			}
			catch (Exception err)
			{
				var msg = err.Message;
				MessageBox.Show(this, msg);
				throw;
			}
			MessageBox.Show(this, "all fine.");
		}
	}
}
