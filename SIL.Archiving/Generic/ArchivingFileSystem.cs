// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using SIL.PlatformUtilities;

namespace SIL.Archiving.Generic
{
	/// <summary />
	public static class ArchivingFileSystem
	{
		internal const string kAccessProtocolFolderName = "Archiving";
		private const string MacFolderRoot = "/Users/Shared";
		internal const string LinuxFolderRoot = "/var/lib";

		/// <summary />
		public static string SilCommonDataFolder
		{
			get
			{
				// On Unix we have to use /var/lib (instead of CommonApplicationData which
				// translates to /usr/share and isn't writable by default)
				// On Mac the place to store data shared between users is /Users/Shared
				var folder = Platform.IsMac ? MacFolderRoot : Platform.IsUnix ? LinuxFolderRoot :
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				return CheckFolder(Path.Combine(folder, "SIL"));
			}
		}

		/// <summary />
		public static string SilCommonArchivingDataFolder =>
			CheckFolder(Path.Combine(SilCommonDataFolder, kAccessProtocolFolderName));

		/// <summary />
		public static string SilCommonIMDIDataFolder =>
			CheckFolder(Path.Combine(SilCommonArchivingDataFolder, "IMDI"));

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
					// If the default location on a Unix system doesn't have write permissions we try again in the ApplicationData folder
					var unixRoot = Platform.IsMac ? MacFolderRoot : LinuxFolderRoot;
					if (Platform.IsUnix && folderName.StartsWith(Path.Combine(unixRoot, "SIL")))
					{
						folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
								folderName.Replace(unixRoot, "").TrimStart(Path.DirectorySeparatorChar));
						return CheckFolder(folderName);
					}
					throw;
				}
			}
			return folderName;
		}
	}
}
