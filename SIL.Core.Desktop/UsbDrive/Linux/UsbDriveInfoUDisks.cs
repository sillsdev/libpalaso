#if !NETSTANDARD
using System;
using System.Collections.Generic;
using System.IO;

namespace SIL.UsbDrive.Linux
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
					string mountPath = String.Empty;
					//When a device is present but not mounted, this method will throw an ArgumentException.
					//In particular this can be the case just after inserting a UsbDevice.
					//The loop here tries to mitigate such an occurrence.
					string[] paths = _device.MountPaths;
					while (_device.IsMounted && paths == null || paths.Length == 0)
						paths = _device.MountPaths;
					if (paths != null && paths.Length > 0)
						mountPath = paths[0];
					return new DirectoryInfo(mountPath);
				}
				return new DirectoryInfo(String.Empty);
			}
		}

		public override string VolumeLabel
		{
			get { return _device.VolumeLabel; }
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

		public new static List<IUsbDriveInfo> GetDrives()
		{
			var drives = new List<IUsbDriveInfo>();

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
#endif
