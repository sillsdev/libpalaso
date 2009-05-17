using System;
using System.Collections.Generic;
using System.IO;

namespace Palaso.UsbDrive
{
	[CLSCompliant (false)]
	public abstract class UsbDriveInfo
	{
		public abstract bool IsReady
		{
			get;
		}

		public abstract DirectoryInfo RootDirectory
		{
			get;
		}

		public abstract ulong TotalSize
		{
			get;
		}

		public static List<UsbDriveInfo> GetDrives()
		{
#if MONO
			return UsbDriveInfoLinux.GetDrives();
#else
			return UsbDriveInfoWindows.GetDrives();
#endif

		}
	}

	/*
	public class Test
	{
		static void la()
		{
			List<UsbDriveInfo> drives = UsbDriveInfo.GetDrives();
			DirectoryInfo path = drives[0].RootDirectory;
		}
	}
	*/
}