using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SIL.Reporting;
using WIA;

namespace SIL.Windows.Forms.ImageToolbox
{
	public class ImageDeviceException : ApplicationException
	{
		public ImageDeviceException()
			: base()
		{
		}

		public ImageDeviceException(string message)
			: base(message)
		{
		}

		public ImageDeviceException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	public class WIA_Version2_MissingException : ApplicationException
	{
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

		[CLSCompliant(false)]
		public ImageFile Acquire()
		{
			try
			{
				CommonDialog dialog = new CommonDialog();

				var type = WiaDeviceType.ScannerDeviceType;
				if (_deviceKind == DeviceKind.Camera)
					type = WiaDeviceType.CameraDeviceType;

				var device = dialog.ShowSelectDevice(type, false, false);

				foreach (var _ in device.Items)
				{
					// Iterate to force enumeration of Items?
				}

				foreach (Property propertyItem in device.Properties)
				{
					//if (!propertyItem.IsReadOnly)
					{
						Debug.WriteLine(String.Format("{0}\t{1}\t{2}", propertyItem.Name, propertyItem.PropertyID, propertyItem.get_Value()));
					}
				}

				// This gives the UI we want (can select profiles), but there's no way for an application to get the
				// results of the scan!!!! It just asks the user where to save the image. GRRRRRRR.
				// object x = dialog.ShowAcquisitionWizard(device);

				//With the low-end canoscan I'm using, it just ignores these settings, so we just get a bitmap of whatever
				//b&w / color the user requested

				var image = dialog.ShowAcquireImage(
					type,
					WiaImageIntent.GrayscaleIntent,
					WiaImageBias.MaximizeQuality,
					WIA.FormatID.wiaFormatBMP, //We'll automatically choose the format later depending on what we see
					false,
					true,
					false);
				UsageReporter.SendNavigationNotice("AcquiredImage/" + (_deviceKind == DeviceKind.Camera ? "Camera" : "Scanner"));

				return image;
			}
			catch (COMException ex)
			{
				if (ex.ErrorCode == -2145320939)
				{
					throw new ImageDeviceNotFoundException();
				}

				//NB: I spend some hours working on adding this wiaaut.dll via the installer, using wix heat, but it was one problem after another.
				//Decided eventually that it wasn't worth it at this point; it's easy enough to install by hand.
				if (ErrorReport.GetOperatingSystemLabel().Contains("XP"))
				{
					var comErrorCode = new Win32Exception(ex.ErrorCode).ErrorCode;

					if (comErrorCode == 80040154)
						throw new WIA_Version2_MissingException();
				}

				throw new ImageDeviceException("COM Exception", ex);
			}
		}
	}
}
