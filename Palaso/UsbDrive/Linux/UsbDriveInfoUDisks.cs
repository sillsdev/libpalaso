using System;
using System.Collections.Generic;
using System.IO;

namespace Palaso.UsbDrive.Linux
{
	internal class UsbDriveInfoUDisks : UsbDriveInfo
	{
		private readonly UDiskDevice _device;

		public UsbDriveInfoUDisks(string device)
		{
			_device = new UDiskDevice(device);
		}

		public override bool IsReady
		{
			get
			{
				return _device.IsMounted;
			}
		}

		public override DirectoryInfo RootDirectory
		{
			get
			{
				if (_device.IsMounted)
				{
					string mountPath = _device.MountPaths[0];
					//When a device is present but not mounted. This method will throw an ArgumentException.
					//In particular this can be the case just after inserting a UsbDevice
					return new DirectoryInfo(mountPath);
				}
				return new DirectoryInfo(String.Empty);
			}
		}

		public override ulong TotalSize
		{
			get { return _device.TotalSize; }
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
			// ReSharper disable LoopCanBeConvertedToQuery
			foreach (string device in uDisks.EnumerateDeviceOnInterface("usb"))
			// ReSharper restore LoopCanBeConvertedToQuery
			{
				var deviceInfo = new UsbDriveInfoUDisks(device);
				if (deviceInfo.IsReady)
				{
					drives.Add(deviceInfo);
				}
			}
			return drives;
		}

	}
}
