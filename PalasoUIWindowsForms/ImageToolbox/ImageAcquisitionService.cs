#if !MONO
using System;
using System.Diagnostics;
using Palaso.Reporting;
using WIA;
using System.Runtime.InteropServices;

namespace Palaso.UI.WindowsForms.ImageToolbox
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

				var device = dialog.ShowSelectDevice(type, false, false);

				foreach (var item in device.Items)
				{

				}
				foreach (Property propertyItem in device.Properties)
				{
					//if (!propertyItem.IsReadOnly)
					{
						Debug.WriteLine(String.Format("{0}\t{1}\t{2}", propertyItem.Name, propertyItem.PropertyID, propertyItem.get_Value()));
					}
				}

				//this gives the UI we want (can select profiles), but there's no way for an application to get the
				//results of the scan!!!! It just asks the user where to save the image. GRRRRRRR.
				//object x = dialog.ShowAcquisitionWizard(device);


				//With the low-end canoscan I'm using, it just ignores these settings, so we just get a bitmap of whatever
				//b&w / color the user requested

				  image = dialog.ShowAcquireImage(
						type,
						WiaImageIntent.GrayscaleIntent,
						WiaImageBias.MaximizeQuality,
						WIA.FormatID.wiaFormatBMP, //We'll automatically choose the format later depending on what we see
					   false,
					   true,
						false);
				UsageReporter.SendNavigationNotice("AcquiredImage/" + (_deviceKind == DeviceKind.Camera?"Camera": "Scanner"));

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