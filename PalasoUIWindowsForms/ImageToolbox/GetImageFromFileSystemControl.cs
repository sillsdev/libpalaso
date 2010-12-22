using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public partial class GetImageFromFileSystemControl : UserControl, IImageToolboxControl
	{
	   private PalasoImage _previousImage;

		public GetImageFromFileSystemControl()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			using(var dlg = new OpenFileDialog())
			{
				dlg.Filter = "picture files|*.png;*.tif;*.tiff;*.jpg;*.jpeg;*.bmp;*.gif";
				dlg.Multiselect = false;
				dlg.AutoUpgradeEnabled = true;
				if(DialogResult.OK == dlg.ShowDialog())
				{
					_pictureBox.Image = Image.FromFile(dlg.FileName);
				}
			}
		}


		public void SetImage(PalasoImage image)
		{
			_previousImage = image;
		}

		public PalasoImage GetImage()
		{
			if (_pictureBox.Image != null)
			{
				return new PalasoImage() { Image = _pictureBox.Image };
			}
			return _previousImage;
		}
	}
}
