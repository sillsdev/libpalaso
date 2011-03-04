#if !MONO
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
			button1.Text = string.Empty;
			switch (deviceKind)
			{
				case ImageAcquisitionService.DeviceKind.Camera:
					button1.Image = ImageToolboxButtons.camera64x64;
					break;
				case ImageAcquisitionService.DeviceKind.Scanner:
					button1.Image = ImageToolboxButtons.scanner64x64;
					break;
				default:
					throw new ArgumentOutOfRangeException("deviceKind");
			}
		}

		public void SetImage(PalasoImage image)
		{
			_previousImage = image;
		}

		public PalasoImage GetImage()
		{
			if(_pictureBox.Image != null)
			{
				return PalasoImage.FromImage(_pictureBox.Image);
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
				temp = ConvertToPngIfNotAlready(temp);
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

		/// <summary>
		/// Will delete the incoming file if it needs to do a conversion
		/// </summary>
		private string ConvertToPngIfNotAlready(string incoming)
		{
			string outgoing = incoming;
			//important to dispose of these things, they lock down the file.
			using (var image = Image.FromFile(incoming))
			{
				 if(!ImageFormat.Png.Equals(image.PixelFormat))
				 {
					 outgoing = Path.GetTempFileName();
					 image.Save(outgoing, ImageFormat.Png);
				 }
			}
			if(outgoing != incoming)
			{
				try
				{
					File.Delete(incoming);
				}
				catch (Exception e)
				{
					Debug.Fail(e.Message);
					//in release, just keep going
				}
			}
			return outgoing;
		}
	}
}
#endif