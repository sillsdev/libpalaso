using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Management;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace Palaso.Backup
{
	public class UsbDriveInfo
	{
		private long _availableFreeSpace;
		private bool _isReady;
		private string _name;
		private DirectoryInfo _rootDirectory;
		private long _totalSize;
		private string _volumelabel;

		public long AvailableFreeSpace
		{
			get { return _availableFreeSpace; }
			private set { _availableFreeSpace = value; }
		}

		public bool IsReady
		{
			get { return _isReady; }
			private set { _isReady = value; }
		}

		public string Name
		{
			get { return _name; }
			private set { _name = value; }
		}

		public DirectoryInfo RootDirectory
		{
			get { return _rootDirectory; }
			private set { _rootDirectory = value; }
		}

		public long TotalSize
		{
			get { return _totalSize; }
			private set { _totalSize = value; }
		}

		public string Volumelabel
		{
			get { return _volumelabel; }
			private set { _volumelabel = value; }
		}


		public static List<UsbDriveInfo> GetDrives()
		{
			List<UsbDriveInfo> driveInfos = new List<UsbDriveInfo>();
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				Connection conn;

				conn = Bus.System;



				ObjectPath opath = new ObjectPath("/org/freedesktop/Hal/Manager");

				string name = "org.freedesktop.Hal";



				HalManager manager = conn.GetObject<HalManager>(name, opath);



				ObjectPath[] returnedStrings;



				returnedStrings = manager.GetAllDevices();
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
												usbDriveinfo._availableFreeSpace = driveInfo.AvailableFreeSpace;
												usbDriveinfo._totalSize = driveInfo.TotalSize;
												usbDriveinfo._name = driveInfo.Name;
												usbDriveinfo._volumelabel = driveInfo.VolumeLabel;
												usbDriveinfo._isReady = driveInfo.IsReady;
												usbDriveinfo._rootDirectory = driveInfo.RootDirectory;
												driveInfos.Add(usbDriveinfo);
											}
										}

									}

								}

							}

						}

					}

				}
			}
			return driveInfos;
		}

		[Interface ("org.freedesktop.Hal.Manager")]
		interface HalManager : Introspectable
		{
			ObjectPath[] GetAllDevices();
		}

		[Interface("org.freedesktop.Hal.Device")]
		interface HalDevice : Introspectable
		{
			ObjectPath[] GetAllDevices();
		}
	}
}
