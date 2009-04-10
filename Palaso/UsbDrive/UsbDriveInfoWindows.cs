using System;
using System.Collections.Generic;
using System.IO;
using System.Management;

namespace Palaso.UsbDrive
{
	internal class UsbDriveInfoWindows : UsbDriveInfo
	{
		private DriveInfo _driveInfo;

		private UsbDriveInfoWindows()
		{
		}

		public override bool IsReady
		{
			get { return _driveInfo.IsReady; }
		}

		public override DirectoryInfo RootDirectory
		{
			get
			{
				//this check is here because Linux throws if the drive is unmounted
				if (!IsReady)
				{
					throw new ArgumentException("Is drive mounted?");
				}
				return _driveInfo.RootDirectory;
			}
		}

		public override ulong TotalSize
		{
			get
			{
				//We use a ulong because that's what linux uses
				return  (ulong)_driveInfo.TotalSize;
			}
		}

		public new static List<UsbDriveInfo> GetDrives()
		{
			List<UsbDriveInfo> drives = new List<UsbDriveInfo>();
			using (ManagementObjectSearcher driveSearcher =
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
											UsbDriveInfoWindows usbDriveinfo = new UsbDriveInfoWindows();

											usbDriveinfo._driveInfo = driveInfo;
											drives.Add(usbDriveinfo);
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
	}
}