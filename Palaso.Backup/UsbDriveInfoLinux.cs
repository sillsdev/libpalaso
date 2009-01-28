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
		private HalDevice _volumeDevice;

		public override bool IsReady
		{
			get
			{
				return TryGetDevicePropertyBoolean(_volumeDevice, "volume.is_mounted");
			}
		}

		public override DirectoryInfo RootDirectory
		{
			get
			{
				string devicePath = TryGetDevicePropertyString(_volumeDevice, "volume.mount_point");
				//When a device is present but not mounted. This method will throw an ArgumentException.
				//In particular this can be the case just after inserting a UsbDevice
				return new DirectoryInfo(devicePath);
			}
		}

		public override ulong TotalSize
		{
			get { return TryGetDevicePropertyInteger(_volumeDevice, "volume.size"); }
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

		private static bool TryGetDevicePropertyBoolean(HalDevice device, string propertyName)
		{
			//if the property does not exist, we don't care
			try
			{
				return device.GetPropertyBoolean(propertyName);
			}
			catch { }
			return false;
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
					deviceInfo._volumeDevice = volumeDevice;
					//This emulates Windows behavior
					if (deviceInfo.IsReady)
					{
						drives.Add(deviceInfo);
					}
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
			bool GetPropertyBoolean(string propertyName);
		}
	}
}
//#endif