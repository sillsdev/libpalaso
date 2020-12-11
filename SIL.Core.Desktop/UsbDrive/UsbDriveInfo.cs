// Copyright (c) 2009-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using SIL.PlatformUtilities;
using SIL.UsbDrive.Linux;
using SIL.UsbDrive.Windows;

namespace SIL.UsbDrive
{

	public interface IUsbDriveInfo
	{
		bool IsReady { get; }
		DirectoryInfo RootDirectory { get; }
		string VolumeLabel { get; }
		ulong TotalSize { get; }
		ulong AvailableFreeSpace { get; }
	}

	/// <summary>
	/// This class allows tests to set up pretend usb drives, in order to test situations like
	/// 1) no drives found
	/// 2) multiple drives
	/// 3) full drives
	/// 4) locked drives(not today, but maybe soon)
	/// </summary>
	public class UsbDriveInfoForTests : IUsbDriveInfo
	{
		public UsbDriveInfoForTests(string path)
		{
			IsReady = true;
			TotalSize = ulong.MaxValue;
			AvailableFreeSpace = ulong.MinValue;
			RootDirectory =  new DirectoryInfo(path);
			VolumeLabel = path;
		}

		public bool IsReady{get;set;}
		public DirectoryInfo RootDirectory { get; set; }
		public string VolumeLabel { get; set; }
		public ulong TotalSize { get; set; }
		public ulong AvailableFreeSpace{get; set;}
	}

	public interface IRetrieveUsbDriveInfo
	{
		List<IUsbDriveInfo> GetDrives();
	}

	public class RetrieveUsbDriveInfo : IRetrieveUsbDriveInfo
	{
		public List<IUsbDriveInfo> GetDrives()
		{
			return UsbDriveInfo.GetDrives();
		}
	}

	/// <summary>
	/// This class allows tests to set up pretend usb drives
	/// </summary>
	public class RetrieveUsbDriveInfoForTests : IRetrieveUsbDriveInfo
	{
		private readonly List<IUsbDriveInfo> _driveInfos;

		public RetrieveUsbDriveInfoForTests(List<IUsbDriveInfo> driveInfos)
		{
			_driveInfos = driveInfos;
		}

		public List<IUsbDriveInfo> GetDrives()
		{
			return _driveInfos;
		}
	}

	public abstract class UsbDriveInfo : IUsbDriveInfo
	{
		public abstract bool IsReady
		{
			get;
		}

		public abstract DirectoryInfo RootDirectory
		{
			get;
		}

		public abstract string VolumeLabel
		{
			get;
		}

		public abstract ulong TotalSize
		{
			get;
		}

		public abstract ulong AvailableFreeSpace { get; }

		public static List<IUsbDriveInfo> GetDrives()
		{
			if (Platform.IsWindows)
				return UsbDriveInfoWindows.GetDrives();

			// Using SIL.UsbDrive on Linux/Mono results in NDesk spinning up a thread that
			// continues until NDesk Bus is closed.  Failure to close the thread results in a
			// program hang when closing.  Closing the system bus allows the thread to close,
			// and thus the program to close.
			AppDomain.CurrentDomain.ProcessExit += (sender, args) => NDesk.DBus.Bus.System.Close();

			// Ubuntu 12.04 uses udisks. HAL use is deprecated.
			// Ubuntu 14.04 can use udisks or udisks2.
			// Ubuntu 16.04 uses udisks2.
			return UsbDriveInfoUDisks2.IsUDisks2Available
				? UsbDriveInfoUDisks2.GetDrives()
				: UsbDriveInfoUDisks.GetDrives();
		}
	}

}
