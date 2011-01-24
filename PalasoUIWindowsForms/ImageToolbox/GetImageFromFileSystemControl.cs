using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public partial class GetImageFromFileSystemControl : UserControl, IImageToolboxControl
	{
	   private PalasoImage _previousImage;
	   static string _sLastPictureDirectory = string.Empty;
	   public event EventHandler ImageChanged;

		public GetImageFromFileSystemControl()
		{
			InitializeComponent();
			button1.Image = ImageToolboxButtons.browse;
			button1.Text = string.Empty;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			using(var dlg = new OpenFileDialog())
			{
				if (string.IsNullOrEmpty(_sLastPictureDirectory))
				{
					dlg.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				}
				else
				{
					dlg.InitialDirectory = _sLastPictureDirectory;
				}

				dlg.Filter = "picture files|*.png;*.tif;*.tiff;*.jpg;*.jpeg;*.bmp;*.gif";
				dlg.Multiselect = false;
				dlg.AutoUpgradeEnabled = true;
				if(DialogResult.OK == dlg.ShowDialog())
				{
					_pictureBox.Image = Image.FromFile(dlg.FileName);
					_sLastPictureDirectory = Path.GetDirectoryName(dlg.FileName);
					if (ImageChanged != null)
						ImageChanged.Invoke(this, null);
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
