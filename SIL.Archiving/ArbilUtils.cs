using Palaso.IO;
using System.IO;

namespace SIL.Archiving
{
	class ArbilUtils
	{

		private readonly ArchivingDlgViewModel _archivingDlgViewModel;

		public ArbilUtils(ArchivingDlgViewModel model)
		{
			_archivingDlgViewModel = model;
		}

		public bool Launch()
		{



			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Get the path and file name of the Arbil executable file
		/// </summary>
		/// <returns>The full name of the Arbil executable file</returns>
		/// ------------------------------------------------------------------------------------
		public static string GetExeFileLocation()
		{
			string exeFile;

			if (ArchivingDlgViewModel.IsMono)
				exeFile = FileLocator.LocateInProgramFiles("RAMP", true);
			else
				exeFile = FileLocator.LocateInProgramFiles("arbil-stable.exe", true, "arbil");

			// make sure the file exists
			if (!File.Exists(exeFile))
				throw new FileNotFoundException("The Arbil executable file was not found.");

			return new FileInfo(exeFile).FullName;
		}
	}
}
