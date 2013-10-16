using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using L10NSharp;
using Palaso.IO;
using Timer = System.Threading.Timer;

namespace SIL.Archiving
{
	/// ------------------------------------------------------------------------------------
	/// <summary>
	/// Functions dealing with RAMP specifically
	/// </summary>
	/// ------------------------------------------------------------------------------------
	public class RAMPUtils
	{
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

// ReSharper disable InconsistentNaming
		private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);    // brings window to top and makes it "always on top"
		private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);  // brings window to top but not "always on top"
		private const UInt32 SWP_NOSIZE = 0x0001;
		private const UInt32 SWP_NOMOVE = 0x0002;
		private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
// ReSharper restore InconsistentNaming

		private static Dictionary<string, string> _languageList;
		private readonly ArchivingDlgViewModel _archivingDlgViewModel;
		private Timer _timer;

		private const string kRampProcessName = "RAMP";

		/// ------------------------------------------------------------------------------------
		public string RampPackagePath { get; set; }

		/// ------------------------------------------------------------------------------------
		public RAMPUtils(ArchivingDlgViewModel model)
		{
			_archivingDlgViewModel = model;
		}

		/// ------------------------------------------------------------------------------------
		public bool Launch()
		{
			if (!File.Exists(RampPackagePath))
			{
				_archivingDlgViewModel.ReportError(null, string.Format("RAMP package prematurely removed: {0}", RampPackagePath));
				return false;
			}

			try
			{
				var prs = new Process();
				prs.StartInfo.FileName = GetExeFileLocation();
				prs.StartInfo.Arguments = "\"" + RampPackagePath + "\"";
				if (!prs.Start())
					return false;

				prs.WaitForInputIdle(8000);
				EnsureRampHasFocusAndWaitForPackageToUnlock();
				return true;
			}
			catch (InvalidOperationException)
			{
				EnsureRampHasFocusAndWaitForPackageToUnlock();
				return true;
			}
			catch (Exception e)
			{
				_archivingDlgViewModel.ReportError(e, LocalizationManager.GetString("DialogBoxes.ArchivingDlg.StartingRampErrorMsg",
					"There was an error attempting to open the archive package in RAMP."));
				return false;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Get the path and file name of the RAMP executable file
		/// </summary>
		/// <returns>The full name of the RAMP executable file</returns>
		/// ------------------------------------------------------------------------------------
		public static string GetExeFileLocation()
		{
			string exeFile;
			const string rampFileExtension = ".ramp";

			if (ArchivingDlgViewModel.IsMono)
				exeFile = FileLocator.LocateInProgramFiles("RAMP", true);
			else
				exeFile = FileLocator.GetFromRegistryProgramThatOpensFileType(rampFileExtension) ??
					FileLocator.LocateInProgramFiles("ramp.exe", true, "ramp");

			// make sure the file exists
			if (!File.Exists(exeFile))
				throw new FileNotFoundException("The RAMP executable file was not found.");

			return new FileInfo(exeFile).FullName;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Get the path and file name of the RAMP Languages file
		/// </summary>
		/// <returns>The full name of the RAMP languages file</returns>
		/// ------------------------------------------------------------------------------------
		public static string GetLanguageFileLocation()
		{
			var exeFile = GetExeFileLocation();
			var dir = Path.GetDirectoryName(exeFile);

			if (dir == null)
				throw new DirectoryNotFoundException("The RAMP directory was not found.");

			// on Linux the exe and data directory are not in the same directory
			if (!Directory.Exists(Path.Combine(dir, "data")))
			{
				dir = Directory.GetParent(dir).FullName;
				if (Directory.Exists(Path.Combine(dir, "share")))
					dir = Path.Combine(dir, "share");
			}

			// get the data directory
			dir = Path.Combine(dir, "data");
			if (!Directory.Exists(dir))
				throw new DirectoryNotFoundException(string.Format("The path {0} is not valid.", dir));

			// get the options directory
			dir = Path.Combine(dir, "options");
			if (!Directory.Exists(dir))
				throw new DirectoryNotFoundException(string.Format("The path {0} is not valid.", dir));

			// get the languages.yaml file
			var langFile = Path.Combine(dir, "languages.yaml");
			if (!File.Exists(langFile))
				throw new FileNotFoundException(string.Format("The file {0} was not found.", langFile));

			return langFile;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a Dictionary of the languages supported by RAMP.  The key is the ISO3 code,
		/// and the entry value is the language name.
		/// </summary>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public static Dictionary<string, string> GetLanguageList()
		{
			if (_languageList == null)
			{
				_languageList = new Dictionary<string, string>();
				var langFile = GetLanguageFileLocation();

				foreach (var fileLine in File.ReadLines(langFile).Where(l => l.StartsWith("  code: \"")))
				{
					const int start = 9;
					var end = fileLine.IndexOf('"', start);
					if (end > start)
					{
						var parts = fileLine.Substring(start, end - start).Split(':');
						if (parts.Length == 2)
							_languageList[parts[0]] = parts[1];
					}
				}
			}

			return _languageList;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the official RAMP name of the language associated with the is03Code.
		/// </summary>
		/// <param name="iso3Code"></param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public static string GetLanguageName(string iso3Code)
		{
			var langs = GetLanguageList();

			if (langs == null)
				throw new Exception("The language list for RAMP was not retrieved.");

			return langs.ContainsKey(iso3Code) ? langs[iso3Code] : null;
		}

		/// ------------------------------------------------------------------------------------
		private void EnsureRampHasFocusAndWaitForPackageToUnlock()
		{
#if !__MonoCS__
			var processes = Process.GetProcessesByName(kRampProcessName);
			if (processes.Length >= 1)
			{
				// First, make the window topmost: this puts it in front of all other windows
				// and sets it as "always on top."
				SetWindowPos(processes[0].MainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

				// Second, make the window notopmost: this removes the "always on top" behavior
				// and positions the window on top of all other "not always on top" windows.
				SetWindowPos(processes[0].MainWindowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
			}
#else
			// On mono this requires xdotool or wmctrl
			string args = null;
			if (!string.IsNullOrEmpty(FileLocator.LocateInProgramFiles("xdotool", true)))      /* try to find xdotool first */
				args = "-c \"for pid in `xdotool search --name RAMP`; do xdotool windowactivate $pid; done\"";
			else if (!string.IsNullOrEmpty(FileLocator.LocateInProgramFiles("wmctrl", true)))  /* if xdotool is not installed, look for wmctrl */
				args = "-c \"wmctrl -a RAMP\"";

			if (!string.IsNullOrEmpty(args))
			{
				var prs = new Process
				{
					StartInfo = {
						FileName = "bash",
						Arguments = args,
						UseShellExecute = false,
						RedirectStandardError = true
					}
				};

				prs.Start();
			}
#endif
			// Every 4 seconds we'll check to see if the RAMP package is locked. When
			// it gets unlocked by RAMP, then we'll delete it.
			_timer = new Timer(CheckIfPackageFileIsLocked, RampPackagePath, 2000, 4000);
		}

		/// ------------------------------------------------------------------------------------
		private void CheckIfPackageFileIsLocked(Object packageFile)
		{
			if (!FileUtils.IsFileLocked(packageFile as string))
				CleanUpTempRampPackage();
		}

		/// ------------------------------------------------------------------------------------
		public void CleanUpTempRampPackage()
		{
			// Comment out as a test !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			//try { File.Delete(RampPackagePath); }
			//catch { }

			if (_timer != null)
			{
				_timer.Dispose();
				_timer = null;
			}
		}
	}
}
