using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace Palaso.Backup
{
	public class UsbDriveInfo
	{
		private DirectoryInfo _rootDirectory;
		private ulong _totalSize;

		public DirectoryInfo RootDirectory
		{
			get { return _rootDirectory; }
		}

		public double TotalSize
		{
			get { return _totalSize; }
		}

		private static string TryGetDevicePropertyString(HalDevice device, string propertyName)
		{
			try
			{
				return device.GetPropertyString(propertyName);
			}
			catch{}
			return String.Empty;
		}

		private static ulong TryGetDevicePropertyUInt64(HalDevice device, string propertyName)
		{
			try
			{
				return device.GetPropertyInteger(propertyName);
			}
			catch{}
			return 0;
		}

		public static List<UsbDriveInfo> GetDrives()
		{
			List<UsbDriveInfo> drives = new List<UsbDriveInfo>();
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				Connection conn;
				conn = Bus.System;

				ObjectPath halManagerPath = new ObjectPath("/org/freedesktop/Hal/Manager");
				string halNameOnDbus = "org.freedesktop.Hal";

				HalManager manager = conn.GetObject<HalManager>(halNameOnDbus, halManagerPath);

				ObjectPath[] volumeDevicePaths = manager.FindDeviceByCapability("volume");
				foreach (ObjectPath volumeDevicePath in volumeDevicePaths)
				{
					HalDevice volumeDevice = conn.GetObject<HalDevice>(halNameOnDbus, volumeDevicePath);

					if (DeviceIsOnUsbBus(conn, halNameOnDbus, volumeDevice))
					{
						UsbDriveInfo deviceInfo = new UsbDriveInfo();

						string devicePath = TryGetDevicePropertyString(volumeDevice, "volume.mount_point");

						deviceInfo._totalSize = TryGetDevicePropertyUInt64(volumeDevice, "volume.size");
						deviceInfo._rootDirectory = new DirectoryInfo(devicePath);

						drives.Add(deviceInfo);
					}
				}

				ObjectPath[] storageDevices = manager.FindDeviceByCapability("storage");
			}
			else
			{
				using ( ManagementObjectSearcher driveSearcher =
					new ManagementObjectSearcher(
						"SELECT Caption, DeviceID FROM Win32_DiskDrive WHERE InterfaceType='USB'")
					)
				{
					// walk all USB WMI physical disks
					foreach (ManagementObject drive in driveSearcher.Get())
					{
						// browse all USB WMI physical disks
						using (ManagementObjectSearcher searcher =
								new ManagementObjectSearcher(
									"ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" +
									drive["DeviceID"] +
									"'} WHERE AssocClass = Win32_DiskDriveToDiskPartition"))
						{
							// walk all USB WMI physical disks
							foreach (ManagementObject partition in searcher.Get())
							{
								using (
									ManagementObjectSearcher partitionSearcher =
										new ManagementObjectSearcher(
											"ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" +
											partition["DeviceID"] +
											"'} WHERE AssocClass = Win32_LogicalDiskToPartition")
									)
								{
									foreach (ManagementObject diskInfoFromWMI in partitionSearcher.Get())
									{
										foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
										{
											string s = driveInfo.Name.Replace("\\", "");
											if (s == diskInfoFromWMI["NAME"].ToString())
											{
												UsbDriveInfo usbDriveinfo = new UsbDriveInfo();
												//We use a ulong because that's what linux uses
												usbDriveinfo._totalSize = (ulong)driveInfo.TotalSize;

												usbDriveinfo._rootDirectory = driveInfo.RootDirectory;
												drives.Add(usbDriveinfo);
											}
										}

									}

								}

							}

						}

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
		}
	}
}
