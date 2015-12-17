
using System;
using System.IO;
using SIL.PlatformUtilities;

namespace SIL.Archiving.Generic
{
	/// <summary />
	public static class ArchivingFileSystem
	{
		internal const string kAccessProtocolFolderName = "Archiving";
  
		/// <summary />
		public static string SilCommonDataFolder
		{
			get
			{
				// On Linux we have to use /var/lib (instead of CommonApplicationData which
				// translates to /usr/share and isn't writable by default)
				string folder = Platform.IsLinux ? "/var/lib" :
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				return CheckFolder(Path.Combine(folder, "SIL"));
			}
		}

		/// <summary />
		public static string SilCommonArchivingDataFolder
		{
			get { return CheckFolder(Path.Combine(SilCommonDataFolder, kAccessProtocolFolderName)); }
		}

		/// <summary />
		public static string SilCommonIMDIDataFolder
		{
			get { return CheckFolder(Path.Combine(SilCommonArchivingDataFolder, "IMDI")); }
		}

		/// <summary />
		public static string CheckFolder(string folderName)
		{
			if (!Directory.Exists(folderName))
			{
				try
				{
					Directory.CreateDirectory(folderName);
				}
				catch (UnauthorizedAccessException)
				{
					if (Platform.IsLinux)
					{
						if (folderName.StartsWith("/var/lib/SIL"))
						{
							// by default /var/lib isn't writable on Linux, so we can't create a new
							// directory. Create a folder in the user's home directory instead.
							var endFolder = folderName.Substring("/var/lib/SIL".Length).TrimStart('/');
							folderName = Path.Combine(
								Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
								"SIL", endFolder);
							return CheckFolder(folderName);
						}
					}
					throw;
				}
			}
			return folderName;
		}
	}
}
