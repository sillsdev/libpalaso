using System;
using System.Collections.Generic;
using System.IO;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace Palaso.UsbDrive.Linux
{
	internal class UsbDriveInfoUDisks : UsbDriveInfo
	{
		private readonly UDiskDevice _device;

		public UsbDriveInfoUDisks(UDiskDevice device)
		{
			_device = device;
		}

		public override bool IsReady
		{
			get
			{
				// TODO: confirm is mounted
				return TryGetDevicePropertyBoolean(_device, "volume.is_mounted");
			}
		}

		public override DirectoryInfo RootDirectory
		{
			get
			{
				// TODO: return path where the device is mounted
				string devicePath = TryGetDevicePropertyString(_device, "volume.mount_point");
				//When a device is present but not mounted. This method will throw an ArgumentException.
				//In particular this can be the case just after inserting a UsbDevice
				return new DirectoryInfo(devicePath);
			}
		}

		public override ulong TotalSize
		{
			// TODO
			get { return TryGetDevicePropertyInteger(_device, "volume.size"); }
		}

		public override ulong AvailableFreeSpace
		{
			// TODO
			get { throw new NotImplementedException("TotalFreeSpace not implemented in Mono yet."); }
		}

		public new static List<UsbDriveInfo> GetDrives()
		{
			var drives = new List<UsbDriveInfo>();

			var uDisks = new UDisks();
			foreach (string device in uDisks.Interface.EnumerateDevices())
			{
				var uDiskDevice = new UDiskDevice(device);
				// Check if the device is on USB
				if (uDiskDevice.IsConnectedViaUSB)
				{
					var deviceInfo = new UsbDriveInfoUDisks(uDiskDevice);
					if (deviceInfo.IsReady)
					{
						drives.Add(deviceInfo);
					}
				}
			}
			return drives;
		}

	}
}
