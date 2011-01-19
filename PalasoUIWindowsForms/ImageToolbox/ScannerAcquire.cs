using System;
using System.Drawing;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public partial class ScannerAcquire : UserControl, IImageToolboxControl
	{
		private PalasoImage _previousImage;

		public ScannerAcquire()
		{
			InitializeComponent();
		}

		public void SetImage(PalasoImage image)
		{
			_previousImage = image;
		}

		public PalasoImage GetImage()
		{
			if(_pictureBox.Image != null)
			{
				return new PalasoImage(){Image = _pictureBox.Image};
			}
			return _previousImage;
		}

		public event EventHandler ImageChanged;

		private void button1_Click(object sender, EventArgs e)
		{
			_pictureBox.Image = SampleImages.sampleScan;
			if(ImageChanged!=null)
				ImageChanged.Invoke(this,null);
		}


	}
}
