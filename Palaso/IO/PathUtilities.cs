// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Palaso.IO
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

		public static int GetDeviceNumber(string filePath)
		{
			if (Palaso.PlatformUtilities.Platform.IsWindows)
			{
				var driveInfo = new DriveInfo(Path.GetPathRoot(filePath));
				return driveInfo.Name.ToUpper()[0] - 'A' + 1;
			}

			// filePath can mean a file or a directory.
			var pathToCheck = filePath;
			if (!File.Exists(pathToCheck) && !Directory.Exists(pathToCheck))
			{
				pathToCheck = Path.GetDirectoryName(pathToCheck);

				if (!Directory.Exists(pathToCheck))
					return GetDeviceNumber(pathToCheck);
			}

			using (var process = new Process())
			{
				process.StartInfo = new ProcessStartInfo {
					FileName = "stat",
					Arguments = string.Format("-c %d \"{0}\"", pathToCheck),
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				};
				process.Start();
				process.WaitForExit();
				var output = process.StandardOutput.ReadToEnd();
				return Convert.ToInt32(output);
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
		/// On Windows this selects the file in Windows Explorer; on Linux it opens the containing
		/// directory in Nautilus (or whatever application xdg-open starts for displaying
		/// directories.
		/// </summary>
		/// <param name="filePath">File path.</param>
		public static void SelectFileInExplorer(string filePath)
		{

			if (PlatformUtilities.Platform.IsWindows)
				Process.Start("explorer.exe", "/select, \"" + filePath + "\"");
			else
			{
				// On Linux we can't open Nautilus and select a file in a directory.
				// So we just open the directory and let the user select the file.
				Process.Start("xdg-open", "\"" + Path.GetDirectoryName(filePath) + "\"");
			}
		}

	}
}

