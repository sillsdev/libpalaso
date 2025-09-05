// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using SIL.Extensions;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.IO
{
	/// <summary>
	/// Desktop-specific utility methods for processing file paths.
	/// </summary>
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

		/// <summary>
		/// Possible flags for the SHFileOperation method.
		/// </summary>
		[CLSCompliant(false)]
		[Flags]
		public enum FileOperationFlags : ushort
		{
			/// <summary>
			/// Do not show a dialog during the process
			/// </summary>
			FOF_SILENT = 0x0004,

			/// <summary>
			/// Do not ask the user to confirm selection
			/// </summary>
			FOF_NOCONFIRMATION = 0x0010,

			/// <summary>
			/// Delete the file to the recycle bin.  (Required flag to send a file to the bin
			/// </summary>
			FOF_ALLOWUNDO = 0x0040,

			/// <summary>
			/// Do not show the names of the files or folders that are being recycled.
			/// </summary>
			FOF_SIMPLEPROGRESS = 0x0100,

			/// <summary>
			/// Suppress errors, if any occur during the process.
			/// </summary>
			FOF_NOERRORUI = 0x0400,

			/// <summary>
			/// Warn if files are too big to fit in the recycle bin and will need
			/// to be deleted completely.
			/// </summary>
			FOF_WANTNUKEWARNING = 0x4000,
		}

		/// <summary>
		/// File Operation Function Type for SHFileOperation
		/// </summary>
		[CLSCompliant(false)]
		public enum FileOperationType : uint
		{
			/// <summary>
			/// Move the objects
			/// </summary>
			FO_MOVE = 0x0001,

			/// <summary>
			/// Copy the objects
			/// </summary>
			FO_COPY = 0x0002,

			/// <summary>
			/// Delete (or recycle) the objects
			/// </summary>
			FO_DELETE = 0x0003,

			/// <summary>
			/// Rename the object(s)
			/// </summary>
			FO_RENAME = 0x0004,
		}



		/// <summary>
		/// SHFILEOPSTRUCT for SHFileOperation from COM
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SHFILEOPSTRUCT
		{

			public readonly IntPtr hwnd;
			[MarshalAs(UnmanagedType.U4)] public FileOperationType wFunc;
			public string pFrom;
			public readonly string pTo;
			public FileOperationFlags fFlags;
			[MarshalAs(UnmanagedType.Bool)] public readonly bool fAnyOperationsAborted;
			public readonly IntPtr hNameMappings;
			public readonly string lpszProgressTitle;
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

		private static void WriteTrashInfoFile(string trashPath, string filePath, string trashedFile)
		{
			var trashInfo = Path.Combine(trashPath, "info", trashedFile + ".trashinfo");
			var lines = new List<string>
			{
				"[Trash Info]",
				$"Path={filePath}",
				$"DeletionDate={DateTime.Now.ToString("yyyyMMddTHH:mm:ss", CultureInfo.InvariantCulture)}"
			};
			File.WriteAllLines(trashInfo, lines);
		}

		/// <summary>
		/// Delete a file or directory by moving it to the trash bin
		/// Display dialog, display warning if files are too big to fit (FOF_WANTNUKEWARNING)
		/// </summary>
		/// <remarks>See: http://stackoverflow.com/questions/3282418/send-a-file-to-the-recycle-bin</remarks>
		/// <param name="filePath">Full path of the file.</param>
		/// <returns><c>true</c> if successfully deleted.</returns>
		public static bool DeleteToRecycleBin(string filePath)
		{
			return DeleteToRecycleBin(filePath, FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_WANTNUKEWARNING);
		}

		/// <summary>
		/// Delete a file or directory by moving it to the trash bin
		/// </summary>
		/// <param name="filePath">Full path of the file.</param>
		/// <param name="flags">options flags from FileOperationFlags</param>
		/// <returns><c>true</c> if successfully deleted.</returns>
		[CLSCompliant(false)]
		public static bool DeleteToRecycleBin(string filePath, FileOperationFlags flags)
		{
			if (Platform.IsWindows)
			{
				if (!File.Exists(filePath) && !Directory.Exists(filePath))
					return false;

				// alternative using visual basic dll:
				// FileSystem.DeleteDirectory(item.FolderPath,UIOption.OnlyErrorDialogs), RecycleOption.SendToRecycleBin);

				//moves it to the recycle bin
				try
				{
					var shf = new SHFILEOPSTRUCT
					{
						wFunc = FileOperationType.FO_DELETE,
						pFrom = filePath + '\0' + '\0',
						fFlags = FileOperationFlags.FOF_ALLOWUNDO | flags
					};

					SHFileOperation(ref shf);
					return !shf.fAnyOperationsAborted;
				}
				catch (Exception)
				{
					return false;
				}
			}

			// On Linux we'll have to move the file to $XDG_DATA_HOME/Trash/files and create
			// a filename.trashinfo file in $XDG_DATA_HOME/Trash/info that contains the original
			// filepath and the deletion date. See http://stackoverflow.com/a/20255190
			// and http://freedesktop.org/wiki/Specifications/trash-spec/.
			// Environment.SpecialFolder.LocalApplicationData corresponds to $XDG_DATA_HOME.

			// move file or directory
			if (Directory.Exists(filePath) || File.Exists(filePath))
			{
				var localDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				// We want to use the standard Trash location for flatpak, not the sandboxed one.
				if (Platform.IsFlatpak)
					localDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".local/share");
				var trashPath = Path.Combine(localDataPath, "Trash");
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
				if (Platform.IsFlatpak)
				{
					// flatpak mounts pieces of the file system differently for sandboxing, so Move won't work.
					DirectoryHelper.Copy(filePath, recyclePath);
					Directory.Delete(filePath, true);
					return true;
				}
				DirectoryHelper.Move(filePath, recyclePath);
				return true;
			}
			return false;
		}

		[DllImport("shell32.dll", ExactSpelling = true)]
		public static extern void ILFree(IntPtr pidlList);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		public static extern IntPtr ILCreateFromPathW(string pszPath);

		[DllImport("shell32.dll", ExactSpelling = true)]
		[CLSCompliant(false)]
		public static extern int SHOpenFolderAndSelectItems(IntPtr pidlList, uint cild, IntPtr children, uint dwFlags);

		[PublicAPI]
		public static void SelectItemInExplorerEx(string path)
		{
			var pidlList = ILCreateFromPathW(path);
			if(pidlList == IntPtr.Zero)
				throw new Exception($"ILCreateFromPathW({path}) failed");
			try
			{
				Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
			}
			finally
			{
				ILFree(pidlList);
			}
		}

		/// <summary>
		/// On Windows this selects the file or directory in Windows Explorer; on Linux it selects the file
		/// in the default file manager if that supports selecting a file and we know it,
		/// otherwise we fall back to xdg-open and open the directory that contains that file.
		/// </summary>
		/// <param name="path">File or directory path.</param>
		public static void SelectFileInExplorer(string path)
		{
			if (Platform.IsWindows)
			{
				//we need to use this because of a bug in windows that strips composed characters before trying to find the target path (http://stackoverflow.com/a/30405340/723299)
				var pidlList = ILCreateFromPathW(path);
				if(pidlList == IntPtr.Zero)
					throw new Exception($"ILCreateFromPathW({path}) failed");
				try
				{
					Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
				}
				finally
				{
					ILFree(pidlList);
				}
			}
			else
			{
				var fileManager = DefaultFileManager;
				string arguments;
				switch (fileManager)
				{
					case "nautilus":
					case "nemo":
						arguments = $"\"{path}\"";
						break;
					default:
						fileManager = "xdg-open";
						arguments = $"\"{Path.GetDirectoryName(path)}\"";
						break;
				}
				// UseShellExecute defaults to true in .net framework (including .net 4) and to false in .net core (including .net 8)
				// We are explicitly setting it to true for consistency with the old behavior
				// but have not checked if it is necessary here.
				Process.Start(new ProcessStartInfo(fileManager, arguments) { UseShellExecute = true });
			}
		}

		/// <summary>
		/// Opens the specified directory in the default file manager
		/// </summary>
		/// <param name="directory">Full path of the directory</param>
		public static void OpenDirectoryInExplorer(string directory)
		{
			//Enhance: on Windows, use ShellExecuteExW instead, as it will probably be able to
			//handle languages with combining characters (diacritics), whereas this explorer
			//approach will fail (at least as of windows 8.1)

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

			Process.Start(new ProcessStartInfo
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
			// UseShellExecute defaults to true in .net framework (including .net 4) and to false in .net core (including .net 8)
			// We are explicitly setting it to true for consistency with the old behavior
			// but have not checked if it is necessary here.
			Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
		}

		private static string GetDefaultFileManager()
		{
			if (Platform.IsWindows)
				return "explorer.exe";

			const string fallbackFileManager = "xdg-open";

			using var xdgmime = new Process();
			// UseShellExecute defaults to true in .net framework (including .net 4) and to false in .net core (including .net 8)
			// We are explicitly setting it to true for consistency with the old behavior
			// but have not checked if it is necessary here.
			xdgmime.StartInfo.UseShellExecute = true;
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
			string desktopFilename = null;
			if (Platform.IsFlatpak)
				desktopFilename = Path.Combine("/app/share/applications", desktopFile);
			if (desktopFilename == null || !File.Exists(desktopFilename))
				desktopFilename = Path.Combine(
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

		private static string _defaultFileManager;

		private static string DefaultFileManager
		{
			get { return _defaultFileManager ??= GetDefaultFileManager(); }
		}
	}
}

