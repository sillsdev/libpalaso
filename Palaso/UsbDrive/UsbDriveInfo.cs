using System;
using System.Collections.Generic;
using System.IO;
using Palaso.UsbDrive.Linux;

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

		public abstract ulong AvailableFreeSpace { get; }

		public static List<UsbDriveInfo> GetDrives()
		{
#if MONO
			return UsbDriveInfoUDisks.GetDrives(); // Lucid now uses UDisks, HAL use is deprecated.
#else
			return UsbDriveInfoWindows.GetDrives();
#endif

		}
	}

}