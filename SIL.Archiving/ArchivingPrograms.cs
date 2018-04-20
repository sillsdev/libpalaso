using System.IO;
using System.Linq;
using SIL.IO;

namespace SIL.Archiving
{
	/// <summary></summary>
	public static class ArchivingPrograms
	{
		/// <summary>Is RAMP installed on this computer</summary>
		public static bool RampIsInstalled()
		{
			return (GetRampExeFileLocation() != null);
		}

		/// <summary>Is Arbil installed on this computer</summary>
		public static bool ArbilIsInstalled()
		{
			return (GetArbilExeFileLocation() != null);
		}

		/// <summary>Get the path and file name of the RAMP executable file</summary>
		public static string GetRampExeFileLocation()
		{
			string exeFile;
			const string rampFileExtension = ".ramp";

			if (ArchivingDlgViewModel.IsMono)
				exeFile = FileLocationUtilities.LocateInProgramFiles("RAMP", true);
			else
				exeFile = FileLocator.GetFromRegistryProgramThatOpensFileType(rampFileExtension) ??
					FileLocationUtilities.LocateInProgramFiles("ramp.exe", true, "ramp");

			// make sure the file exists
			return !File.Exists(exeFile) ? null : new FileInfo(exeFile).FullName;
		}

		/// <summary>Get the path and file name of the Arbil executable file</summary>
		public static string GetArbilExeFileLocation()
		{
			var exeFile = ArchivingDlgViewModel.IsMono
				? FindArbilJarFileMono()
				: FileLocationUtilities.LocateInProgramFiles("arbil-stable.exe", true, "arbil");

			// make sure the file exists
			return !File.Exists(exeFile)
				? null
				: new FileInfo(exeFile).FullName;
		}

		/// <summary>Arbil does not have a separate executable file on mono, it is launched as java</summary>
		private static string FindArbilJarFileMono()
		{
			// look for /usr/share/arbil-stable
			var dirs = Directory.GetDirectories("/usr/share", "arbil-stable");

			// if not found, do wildcard search
			if (dirs.Length == 0)
				dirs = Directory.GetDirectories("/usr/share", "arbil*");

			// if still not found, give up
			if (dirs.Length == 0)
				return null;

			foreach (var files in dirs.Select(dir => Directory.GetFiles(dir, "arbil*.jar", SearchOption.TopDirectoryOnly)))
			{
				// there are 2 jar files, ignore the help file
				foreach (var file in files.Where(file => !file.Contains("help")))
					return file;
			}

			// failed to find it
			return null;
		}

		/// <summary>Command line parameters use to launch Arbil in Linux</summary>
		public static string ArbilCommandLineArgs
		{
			get { return "-Xms256m -Xmx1024m -jar \"{0}\""; }
		}
	}
}
