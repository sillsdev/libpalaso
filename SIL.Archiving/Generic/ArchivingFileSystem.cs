
using System;
using System.IO;

namespace SIL.Archiving.Generic
{
	/// <summary />
	public static class ArchivingFileSystem
	{
		internal const string kAccessProtocolFolderName = "Archiving";
  
		/// <summary />
		public static string SilCommonDataFolder
		{
			get { return CheckFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SIL")); }
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
			if (!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);
			return folderName;
		}
	}
}
