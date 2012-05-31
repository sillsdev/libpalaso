using System;
using System.Collections.Generic;
using System.IO;

#if MONO
using Palaso.UsbDrive.Linux;
#else
using Palaso.UsbDrive.Windows;
#endif

namespace Palaso.UsbDrive
{

	[CLSCompliant (false)]
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
	///
	[CLSCompliant (false)]
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



	[CLSCompliant (false)]
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
#if MONO
			return UsbDriveInfoUDisks.GetDrives(); // Lucid now uses UDisks, HAL use is deprecated.
#else
			return UsbDriveInfoWindows.GetDrives();
#endif

		}
	}

}