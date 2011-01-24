#if !MONO
using System;
using System.IO;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ImageToolbox.Scanner
{
	public partial class DeviceAcquire : UserControl, IImageToolboxControl
	{
		private readonly ImageAcquisitionService.DeviceKind _deviceKind;
		private PalasoImage _previousImage;


		public DeviceAcquire(ImageAcquisitionService.DeviceKind deviceKind)
		{
			_deviceKind = deviceKind;
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
			//_pictureBox.Image = SampleImages.sampleScan;
			try
			{
				var acquisitionService = new ImageAcquisitionService(_deviceKind);

				var file = acquisitionService.Acquire();
				var temp = Path.GetTempFileName();
				File.Delete(temp);
				file.SaveFile(temp);
				_pictureBox.Load(temp);
				File.Delete(temp);
				if (ImageChanged != null)
					ImageChanged.Invoke(this, null);
			}
			catch(ImageDeviceNotFoundException error)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(error.Message);
			}
			catch(Exception error)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(error, "Problem Getting Image");
			}
		}


	}
}
#endif