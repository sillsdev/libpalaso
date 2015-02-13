// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using SIL.Extensions;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.IO
{
	public static class PathUtilities
	{

		// On Unix there are more characters valid in file names, but we
		// want the result to be identical on both platforms, so we want
		// to use the larger invalid Windows list for both platforms
		public static char[] GetInvalidOSIndependentFileNameChars()
		{
			return new char[]
			{
				'\0',
				'\u0001',
				'\u0002',
				'\u0003',
				'\u0004',
				'\u0005',
				'\u0006',
				'\a',
				'\b',
				'\t',
				'\n',
				'\v',
				'\f',
				'\r',
				'\u000e',
				'\u000f',
				'\u0010',
				'\u0011',
				'\u0012',
				'\u0013',
				'\u0014',
				'\u0015',
				'\u0016',
				'\u0017',
				'\u0018',
				'\u0019',
				'\u001a',
				'\u001b',
				'\u001c',
				'\u001d',
				'\u001e',
				'\u001f',
				'"',
				'<',
				'>',
				'|',
				':',
				'*',
				'?',
				'\\',
				'/'
			};
		}

		// map directory name to its disk device number (not used on Windows)
		private static Dictionary<string,int> _deviceNumber = new Dictionary<string, int>();

		public static int GetDeviceNumber(string filePath)
		{
			if (Platform.IsWindows)
			{
				var driveInfo = new DriveInfo(Path.GetPathRoot(filePath));
				return driveInfo.Name.ToUpper()[0] - 'A' + 1;
			}

			// filePath can mean a file or a directory.  Get the directory
			// so that our device number cache can work better.  (fewer
			// unique directory names than filenames)
			var pathToCheck = filePath;
			if (File.Exists(pathToCheck))
				pathToCheck = Path.GetDirectoryName(pathToCheck);
			else if (!Directory.Exists(pathToCheck))
			{
				// Work up the path until a directory exists.
				do
				{
					pathToCheck = Path.GetDirectoryName(pathToCheck);
				}
				while (!String.IsNullOrEmpty(pathToCheck) && !Directory.Exists(pathToCheck));
				// If the whole path is invalid, give up.
				if (String.IsNullOrEmpty(pathToCheck))
					return -1;
			}

			int retval;
			// Use cached value if we can to avoid process invocation.
			if (_deviceNumber.TryGetValue(pathToCheck, out retval))
				return retval;

			using (var process = new Process())
			{
				process.StartInfo = new ProcessStartInfo
				{
					FileName = "stat",
					Arguments = string.Format("-c %d \"{0}\"", pathToCheck),
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				};
				process.Start();
				var output = process.StandardOutput.ReadToEnd();
				// This process is frequently not exiting even after filling the output string
				// with the desired information.  So we'll wait a couple of seconds instead of
				// waiting forever and just go on.  If there's data to process, we'll use it.
				// (2 seconds should be more than enough for that simple command to execute.
				// "time statc -c %d "/tmp" reports 2ms of real time and 2ms of user time.)
				// See https://jira.sil.org/browse/BL-771 for a bug report involving this code
				// with a simple process.WaitForExit().
				// This feels like a Mono bug of some sort, so feel free to regard this as a
				// workaround hack.
				process.WaitForExit(2000);
				if (!String.IsNullOrWhiteSpace(output))
				{
					if (Int32.TryParse(output.Trim(), out retval))
					{
						_deviceNumber.Add(pathToCheck, retval);
						return retval;
					}
				}
				return -1;
			}
		}

		public static bool PathsAreOnSameVolume(string firstPath, string secondPath)
		{
			if (string.IsNullOrEmpty(firstPath) || string.IsNullOrEmpty(secondPath))
				return false;

			return PathUtilities.GetDeviceNumber(firstPath) == PathUtilities.GetDeviceNumber(secondPath);
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
		private struct SHFILEOPSTRUCT
		{
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.U4)]
			public int wFunc;
			public string pFrom;
			public string pTo;
			public short fFlags;
			[MarshalAs(UnmanagedType.Bool)]
			public bool fAnyOperationsAborted;
			public IntPtr hNameMappings;
			public string lpszProgressTitle;
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

		private const int FO_DELETE = 3;
		private const int FOF_ALLOWUNDO = 0x40;
		private const int FOF_NOCONFIRMATION = 0x10; // Don't prompt the user
		private const int FOF_SIMPLEPROGRESS = 0x0100;

		private static void WriteTrashInfoFile(string trashPath, string filePath, string trashedFile)
		{
			var trashInfo = Path.Combine(trashPath, "info", trashedFile + ".trashinfo");
			var lines = new List<string>();
			lines.Add("[Trash Info]");
			lines.Add(string.Format("Path={0}", filePath));
			lines.Add(string.Format("DeletionDate={0}",
				DateTime.Now.ToString("yyyyMMddTHH:mm:ss", CultureInfo.InvariantCulture)));
			File.WriteAllLines(trashInfo, lines);
		}

		/// <summary>
		/// Delete a file or directory by moving it to the trash bin
		/// </summary>
		/// <param name="filePath">Full path of the file.</param>
		/// <returns><c>true</c> if successfully deleted.</returns>
		public static bool DeleteToRecycleBin(string filePath)
		{
			if (PlatformUtilities.Platform.IsWindows)
			{
				if (!File.Exists(filePath) && !Directory.Exists(filePath))
					return false;

				// alternative using visual basic dll:
				// FileSystem.DeleteDirectory(item.FolderPath,UIOption.OnlyErrorDialogs), RecycleOption.SendToRecycleBin);

				//moves it to the recyle bin
				var shf = new SHFILEOPSTRUCT();
				shf.wFunc = FO_DELETE;
				shf.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION;
				string pathWith2Nulls = filePath + "\0\0";
				shf.pFrom = pathWith2Nulls;

				SHFileOperation(ref shf);
				return !shf.fAnyOperationsAborted;
			}

			// On Linux we'll have to move the file to $XDG_DATA_HOME/Trash/files and create
			// a filename.trashinfo file in $XDG_DATA_HOME/Trash/info that contains the original
			// filepath and the deletion date. See http://stackoverflow.com/a/20255190
			// and http://freedesktop.org/wiki/Specifications/trash-spec/.
			// Environment.SpecialFolder.LocalApplicationData corresponds to $XDG_DATA_HOME.

			// move file or directory
			if (Directory.Exists(filePath) || File.Exists(filePath))
			{
				var trashPath = Path.Combine(Environment.GetFolderPath(
					Environment.SpecialFolder.LocalApplicationData), "Trash");
				var trashedFileName = Path.GetRandomFileName();
				if (!Directory.Exists(trashPath))
				{
					// in case the trash bin doesn't exist we create it. This can happen e.g.
					// on the build machine
					Directory.CreateDirectory(Path.Combine(trashPath, "files"));
					Directory.CreateDirectory(Path.Combine(trashPath, "info"));
				}

				var recyclePath = Path.Combine(Path.Combine(trashPath, "files"), trashedFileName);

				WriteTrashInfoFile(trashPath, filePath, trashedFileName);
				// Directory.Move works for directories and files
				DirectoryUtilities.MoveDirectorySafely(filePath, recyclePath);
				return true;
			}
			return false;
		}

		/// <summary>
		/// On Windows this selects the file in Windows Explorer; on Linux it selects the file
		/// in the default file manager if that supports selecting a file and we know it,
		/// otherwise we fall back to xdg-open and open the directory that contains that file.
		/// </summary>
		/// <param name="filePath">File path.</param>
		public static void SelectFileInExplorer(string filePath)
		{
			var fileManager = DefaultFileManager;
			string arguments;
			switch (fileManager)
			{
				case "explorer.exe":
					arguments = string.Format("/select, \"{0}\"", filePath);
					break;
				case "nautilus":
				case "nemo":
					arguments = string.Format("\"{0}\"", filePath);
					break;
				default:
					fileManager = "xdg-open";
					arguments = string.Format("\"{0}\"", Path.GetDirectoryName(filePath));
					break;
			}
			Process.Start(fileManager, arguments);
		}

		/// <summary>
		/// Opens the specified directory in the default file manager
		/// </summary>
		/// <param name="directory">Full path of the directory</param>
		public static void OpenDirectoryInExplorer(string directory)
		{
			var fileManager = DefaultFileManager;
			var arguments = "\"{0}\"";

			// the value returned by GetDefaultFileManager() may include arguments
			var firstSpace = fileManager.IndexOf(' ');
			if (firstSpace > -1)
			{
				arguments = fileManager.Substring(firstSpace + 1) + " " + arguments;
				fileManager = fileManager.Substring(0, firstSpace);
			}
			arguments = string.Format(arguments, directory);

			Process.Start(new ProcessStartInfo()
				{
					FileName = fileManager,
					Arguments = arguments,
					UseShellExecute = false
				});
		}

		/// <summary>
		/// Opens the file in the application associated with the file type.
		/// </summary>
		/// <param name="filePath">Full path to the file</param>
		public static void OpenFileInApplication(string filePath)
		{
			Process.Start(filePath);
		}

		private static string GetDefaultFileManager()
		{
			if (PlatformUtilities.Platform.IsWindows)
				return "explorer.exe";

			const string fallbackFileManager = "xdg-open";

			using (var xdgmime = new Process())
			{
				bool processError = false;
				xdgmime.RunProcess("xdg-mime", "query default inode/directory", exception =>  {
					processError = true;
				});
				if (processError)
				{
					Logger.WriteMinorEvent("Error executing 'xdg-mime query default inode/directory'");
					return fallbackFileManager;
				}
				string desktopFile = xdgmime.StandardOutput.ReadToEnd().TrimEnd(' ', '\n', '\r');
				xdgmime.WaitForExit();
				if (string.IsNullOrEmpty(desktopFile))
				{
					Logger.WriteMinorEvent("Didn't find default value for mime type inode/directory");
					return fallbackFileManager;
				}
				// Look in /usr/share/applications for .desktop file
				var desktopFilename = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					"applications", desktopFile);
				if (!File.Exists(desktopFilename))
				{
					// We didn't find the .desktop file yet, so check in ~/.local/share/applications
					desktopFilename = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						"applications", desktopFile);
				}
				if (!File.Exists(desktopFilename))
				{
					Logger.WriteMinorEvent("Can't find desktop file for {0}", desktopFile);
					return fallbackFileManager;
				}
				using (var reader = File.OpenText(desktopFilename))
				{
					string line;
					for (line = reader.ReadLine();
						!line.StartsWith("Exec=", StringComparison.InvariantCultureIgnoreCase) && !reader.EndOfStream;
						line = reader.ReadLine())
					{
					}

					if (!line.StartsWith("Exec=", StringComparison.InvariantCultureIgnoreCase))
					{
						Logger.WriteMinorEvent("Can't find Exec line in {0}", desktopFile);
						_defaultFileManager = string.Empty;
						return _defaultFileManager;
					}

					var start = "Exec=".Length;
					var argStart = line.IndexOf('%');
					var cmdLine = argStart > 0 ? line.Substring(start, argStart - start) : line.Substring(start);
					cmdLine = cmdLine.TrimEnd();
					Logger.WriteMinorEvent("Detected default file manager as {0}", cmdLine);
					return cmdLine;
				}
			}
		}

		private static string _defaultFileManager;

		private static string DefaultFileManager
		{
			get
			{
				if (_defaultFileManager == null)
					_defaultFileManager = GetDefaultFileManager();

				return _defaultFileManager;
			}
		}

	}
}

