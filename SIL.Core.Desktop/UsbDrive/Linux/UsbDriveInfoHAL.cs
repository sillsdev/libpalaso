#if !NETSTANDARD
using System;
using System.Collections.Generic;
using System.IO;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace SIL.UsbDrive.Linux
{
	internal class UsbDriveInfoHal : UsbDriveInfo
	{
		// Suppressed because it is an interface created by NDesk.DBus
		#pragma warning disable 649
		private IHalDevice _volumeDevice;
		#pragma warning restore 649

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
				//In particular this can be the case just after inserting a USB device
				return new DirectoryInfo(devicePath);
			}
		}

		public override ulong TotalSize
		{
			get { return TryGetDevicePropertyInteger(_volumeDevice, "volume.size"); }
		}

		public override string VolumeLabel
		{
			get { throw new NotImplementedException("VolumeLabel not implemented in HAL Mono yet."); }
		}

		public override ulong AvailableFreeSpace
		{
			get { throw new NotImplementedException("TotalFreeSpace not implemented in Mono yet."); }
		}

		private static string TryGetDevicePropertyString(IHalDevice device, string propertyName)
		{
			//if the property does not exist, we don't care
			try
			{
				return device.GetPropertyString(propertyName);
			}
			catch{}
			return String.Empty;
		}

		private static bool TryGetDevicePropertyBoolean(IHalDevice device, string propertyName)
		{
			//if the property does not exist, we don't care
			try
			{
				return device.GetPropertyBoolean(propertyName);
			}
			catch { }
			return false;
		}

		private static ulong TryGetDevicePropertyInteger(IHalDevice device, string propertyName)
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
			var drives = new List<UsbDriveInfo>();
			Connection conn =  Bus.System;

			var halManagerPath = new ObjectPath("/org/freedesktop/Hal/Manager");
			const string halNameOnDbus = "org.freedesktop.Hal";

			var manager = conn.GetObject<IHalManager>(halNameOnDbus, halManagerPath);

			ObjectPath[] volumeDevicePaths = manager.FindDeviceByCapability("volume");
			foreach (var volumeDevicePath in volumeDevicePaths)
			{
				var volumeDevice = conn.GetObject<IHalDevice>(halNameOnDbus, volumeDevicePath);

				if (DeviceIsOnUsbBus(conn, halNameOnDbus, volumeDevice))
				{
					var deviceInfo = new UsbDriveInfoHal();
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

		private static bool DeviceIsOnUsbBus(Connection conn, string halNameOnDbus, IHalDevice device)
		{
			bool deviceIsOnUsbSubsystem;
			bool thereIsAPathToParent;
			do
			{
				string subsystem = TryGetDevicePropertyString(device, "info.subsystem");
				deviceIsOnUsbSubsystem = subsystem.Contains("usb");
				string pathToParent = TryGetDevicePropertyString(device, "info.parent");
				thereIsAPathToParent = String.IsNullOrEmpty(pathToParent);
				device = conn.GetObject<IHalDevice>(halNameOnDbus, new ObjectPath(pathToParent));
			} while (!deviceIsOnUsbSubsystem && !thereIsAPathToParent);
			return deviceIsOnUsbSubsystem;
		}

		[Interface ("org.freedesktop.Hal.Manager")]
		interface IHalManager : Introspectable
		{
			ObjectPath[] GetAllDevices();
			ObjectPath[] FindDeviceByCapability(string capability);
		}

		[Interface("org.freedesktop.Hal.Device")]
		interface IHalDevice : Introspectable
		{
			string GetPropertyString(string propertyName);
			string[] GetPropertyStringList(string propertyName);
			ulong GetPropertyInteger(string propertyName);
			bool GetPropertyBoolean(string propertyName);
		}
	}
}
#endif
