//#if MONO
using System;
using System.Collections.Generic;
using System.IO;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace Palaso.Backup
{
	internal class UsbDriveInfoLinux:UsbDriveInfo
	{
		private DirectoryInfo _rootDirectory;
		private ulong _totalSize;

		public override DirectoryInfo RootDirectory
		{
			get { return _rootDirectory; }
		}

		public override ulong TotalSize
		{
			get { return _totalSize; }
		}

		private static string TryGetDevicePropertyString(HalDevice device, string propertyName)
		{
			//if the property does not exist, we don't care
			try
			{
				return device.GetPropertyString(propertyName);
			}
			catch{}
			return String.Empty;
		}

		private static ulong TryGetDevicePropertyInteger(HalDevice device, string propertyName)
		{
			//if the property does not exist, we don't care
			try
			{
				return device.GetPropertyInteger(propertyName);
			}
			catch { }
			return 0;
		}

		public new static List<UsbDriveInfo> GetDrives()
		{
			List<UsbDriveInfo> drives = new List<UsbDriveInfo>();
			Connection conn = Bus.System;

			ObjectPath halManagerPath = new ObjectPath("/org/freedesktop/Hal/Manager");
			string halNameOnDbus = "org.freedesktop.Hal";

			HalManager manager = conn.GetObject<HalManager>(halNameOnDbus, halManagerPath);

			ObjectPath[] volumeDevicePaths = manager.FindDeviceByCapability("volume");
			foreach (ObjectPath volumeDevicePath in volumeDevicePaths)
			{
				HalDevice volumeDevice = conn.GetObject<HalDevice>(halNameOnDbus, volumeDevicePath);

				if (DeviceIsOnUsbBus(conn, halNameOnDbus, volumeDevice))
				{
					UsbDriveInfoLinux deviceInfo = new UsbDriveInfoLinux();

					string devicePath = TryGetDevicePropertyString(volumeDevice, "volume.mount_point");

					deviceInfo._totalSize = TryGetDevicePropertyInteger(volumeDevice, "volume.size");
					deviceInfo._rootDirectory = new DirectoryInfo(devicePath);

					drives.Add(deviceInfo);
				}
			}
			return drives;
		}

		private static bool DeviceIsOnUsbBus(Connection conn, string halNameOnDbus, HalDevice device)
		{
			bool deviceIsOnUsbSubsystem;
			bool thereIsAPathToParent;
			do
			{
				string subsystem = TryGetDevicePropertyString(device, "info.subsystem");
				deviceIsOnUsbSubsystem = subsystem.Contains("usb");
				string pathToParent = TryGetDevicePropertyString(device, "info.parent");
				thereIsAPathToParent = String.IsNullOrEmpty(pathToParent);
				device = conn.GetObject<HalDevice>(halNameOnDbus, new ObjectPath(pathToParent));
			} while (!deviceIsOnUsbSubsystem && !thereIsAPathToParent);
			return deviceIsOnUsbSubsystem;
		}

		[Interface ("org.freedesktop.Hal.Manager")]
		interface HalManager : Introspectable
		{
			ObjectPath[] GetAllDevices();
			ObjectPath[] FindDeviceByCapability(string capability);
		}

		[Interface("org.freedesktop.Hal.Device")]
		interface HalDevice : Introspectable
		{
			string GetPropertyString(string propertyName);
			string[] GetPropertyStringList(string propertyName);
			ulong GetPropertyInteger(string propertyName);
		}
	}
}
//#endif