
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
				// On Unix we have to use /var/lib (instead of CommonApplicationData which
				// translates to /usr/share and isn't writable by default)
				// On Mac the place to store data shared between users is /Users/Shared
				var folder = Platform.IsMac ? "/Users/Shared" : Platform.IsUnix ? "/var/lib" :
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
					if (Platform.IsUnix && (folderName.StartsWith("/var/lib/SIL") || folderName.StartsWith("/Users/Shared")))
					{
						// If the default directory isn't writable on a Unix system (we couldn't create the SIL directory)
						// fall back to creating a folder in the users home directory instead.
						var endFolder = folderName.Substring("/var/lib/SIL".Length).TrimStart('/');
						folderName = Path.Combine(
							Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
							"SIL", endFolder);
						return CheckFolder(folderName);
					}
					throw;
				}
			}
			return folderName;
		}
	}
}
