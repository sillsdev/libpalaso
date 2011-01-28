#if !MONO
using System;
using WIA;
using System.Runtime.InteropServices;

namespace Palaso.UI.WindowsForms.ImageToolbox.Scanner
{
	public class ImageDeviceException : ApplicationException
	{
		public ImageDeviceException()
			: base()
		{ }

		public ImageDeviceException(string message)
			: base(message)
		{ }

		public ImageDeviceException(string message, Exception innerException)
			: base(message, innerException)
		{ }

	}

	public class ImageDeviceNotFoundException : ImageDeviceException
	{
		public ImageDeviceNotFoundException()
			: base("Could not find a device. Is your device plugged in and turned on?")
		{
		}
	}

	public class ImageAcquisitionService
	{
		private readonly DeviceKind _deviceKind;

		public enum DeviceKind { Camera, Scanner };

		public ImageAcquisitionService(DeviceKind deviceKind)
		{
			_deviceKind = deviceKind;
		}

		public ImageFile Acquire()
		{
			ImageFile image;

			try
			{
				CommonDialog dialog = new CommonDialog();

				var type = WiaDeviceType.ScannerDeviceType;
				if(_deviceKind==DeviceKind.Camera)
						type = WiaDeviceType.CameraDeviceType;

				image = dialog.ShowAcquireImage(
						type,
						WiaImageIntent.GrayscaleIntent,
						WiaImageBias.MinimizeSize,
						WIA.FormatID.wiaFormatPNG,
						false,
						true,
						false);

				return image;
			}
			catch (COMException ex)
			{
				if (ex.ErrorCode == -2145320939)
				{
					throw new ImageDeviceNotFoundException();
				}
				else
				{
					throw new ImageDeviceException("COM Exception", ex);
				}
			}
		}

	}
}
#endif