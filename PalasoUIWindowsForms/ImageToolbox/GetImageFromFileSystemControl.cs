using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public partial class GetImageFromFileSystemControl : UserControl, IImageToolboxControl
	{
	   private PalasoImage _previousImage;
		private PalasoImage _currentImage;

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
				if (string.IsNullOrEmpty(ImageToolboxSettings.Default.LastImageFolder))
				{
					dlg.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				}
				else
				{
					dlg.InitialDirectory = ImageToolboxSettings.Default.LastImageFolder;
				}

				dlg.Filter = "picture files|*.png;*.tif;*.tiff;*.jpg;*.jpeg;*.bmp;*.gif";
				dlg.Multiselect = false;
				dlg.AutoUpgradeEnabled = true;
				if(DialogResult.OK == dlg.ShowDialog())
				{
					_currentImage = PalasoImage.FromFile(dlg.FileName);
					_pictureBox.Image = _currentImage.Image;
					ImageToolboxSettings.Default.LastImageFolder = Path.GetDirectoryName(dlg.FileName);
					ImageToolboxSettings.Default.Save();
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
			if (_currentImage!=null)
//            if (_pictureBox.Image != null)
			{
				//return new PalasoImage() { Image = _pictureBox.Image, FileName = _fileName };
				return _currentImage;
			}
			return _previousImage;
		}

		private void GetImageFromFileSystemControl_Load(object sender, EventArgs e)
		{
		}

		private void _startupTimer_Tick(object sender, EventArgs e)
		{
			//nb: had problems doing this on Load event (listeners weren't notified of the new picture)
			_startupTimer.Enabled = false;
			button1_Click(this,null);
		}
	}
}
