// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SIL.PlatformUtilities;

namespace SIL.IO
{
	public static class PathHelper
	{
		// map directory name to its disk device number (not used on Windows)
		private static Dictionary<string, int> _deviceNumber = new Dictionary<string, int>();

		public static int GetDeviceNumber(string filePath)
		{
			if (Platform.IsWindows)
			{
				var driveInfo = new DriveInfo(Path.GetPathRoot(filePath));
				return driveInfo.Name.ToUpper()[0] - 'A' + 1;
			}

			// path can mean a file or a directory.  Get the directory
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
				var statFlags = Platform.IsMac ? "-f" : "-c";
				process.StartInfo = new ProcessStartInfo
				{
					FileName = "stat",
					Arguments = string.Format("{0} %d \"{1}\"", statFlags, pathToCheck),
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

		public static bool AreOnSameVolume(string firstPath, string secondPath)
		{
			if (string.IsNullOrEmpty(firstPath) || string.IsNullOrEmpty(secondPath))
				return false;

			return GetDeviceNumber(firstPath) == GetDeviceNumber(secondPath);
		}

		/// <summary>
		/// Make sure the given <paramref name="pathToFile"/> file is 'valid'.
		///
		/// Valid means that
		///		1. <paramref name="pathToFile"/> must not be null or an empty string
		///		2. <paramref name="pathToFile"/> must exist, and
		///		3. The extension for <paramref name="pathToFile"/> must equal <paramref name="expectedExtension"/>
		///			(Or both must be null)
		/// </summary>
		public static bool CheckValidPathname(string pathToFile, string expectedExtension)
		{
			var extension = ((expectedExtension == null) || (expectedExtension.Trim() == String.Empty))
								? null
								: expectedExtension.StartsWith(".") ? expectedExtension.Substring(1) : expectedExtension;

			if (string.IsNullOrEmpty(pathToFile) || !File.Exists(pathToFile))
				return false;

			var actualExtension = Path.GetExtension(pathToFile);
			if (actualExtension == String.Empty)
				actualExtension = null;
			return (actualExtension == null && extension == null) || (actualExtension != null && extension != null &&
				   actualExtension.ToLowerInvariant() == "." + extension.ToLowerInvariant());
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern uint GetShortPathName(
		   [MarshalAs(UnmanagedType.LPTStr)]string lpszLongPath,
		   [MarshalAs(UnmanagedType.LPTStr)]StringBuilder lpszShortPath,
		   uint cchBuffer);

		/// <summary>
		/// When calling external exe's on Windows any non-ascii characters can get converted to '?'. This
		/// will convert them to 8.3 format which is all ascii (and do nothing on Linux).
		/// </summary>
		public static string MakePathSafeFromEncodingProblems(string path)
		{
			if (!Platform.IsWindows)
				return path;//Linux doesn't have these problems, far as I know

			const int MAXPATH = 260;
			var shortBuilder = new StringBuilder(MAXPATH);
			GetShortPathName(path, shortBuilder, (uint) shortBuilder.Capacity);
			return shortBuilder.ToString();
		}

		/// <summary>
		/// Normalize the path so that it uses forward slashes instead of backslashes. This is
		/// useful when a path gets read from a file that gets shared between Windows and Linux -
		/// if the path contains backslashes it can't be found on Linux.
		/// </summary>
		public static string NormalizePath(string path)
		{
			return path.Replace('\\', '/');
		}

		/// <summary>
		/// Strips file URI prefix from the beginning of a file URI string, and keeps
		/// a beginning slash if in Linux.
		/// eg "file:///C:/Windows" becomes "C:/Windows" in Windows, and
		/// "file:///usr/bin" becomes "/usr/bin" in Linux.
		/// Returns the input unchanged if it does not begin with "file:".
		///
		/// Does not convert the result into a valid path or a path using current platform
		/// path separators.
		///
		/// See uri.LocalPath, http://en.wikipedia.org/wiki/File_URI , and
		/// http://blogs.msdn.com/b/ie/archive/2006/12/06/file-uris-in-windows.aspx .
		/// </summary>
		public static string StripFilePrefix(string fileString)
		{
			if (String.IsNullOrEmpty(fileString))
				return fileString;

			var prefix = Uri.UriSchemeFile + ":";

			if (!fileString.StartsWith(prefix))
				return fileString;

			var path = fileString.Substring(prefix.Length);
			// Trim any number of beginning slashes
			path = path.TrimStart('/');
			// Prepend slash on Linux
			if (Platform.IsUnix)
				path = '/' + path;

			return path;
		}

		/// <summary>
		/// If necessary, append a number to make the folder path unique.
		/// </summary>
		/// <param name="folderPath">Source folder pathname.</param>
		/// <returns>A unique folder pathname at the same level as <paramref name="folderPath"/>. It may have a number apended to <paramref name="folderPath"/>, or it may be <paramref name="folderPath"/>.</returns>
		public static string GetUniqueFolderPath(string folderPath)
		{
			var i = 0;
			var suffix = "";
			// Remove ending path separator, if it exists.
			folderPath = folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			var parent = Directory.GetParent(folderPath).FullName;
			var name = Path.GetFileName(folderPath);
			while (Directory.Exists(Path.Combine(parent, name + suffix)))
			{
				++i;
				suffix = i.ToString();
			}
			return Path.Combine(parent, name + suffix);
		}

		public static bool ContainsDirectory(string path, string directory)
		{
			if (string.IsNullOrEmpty(directory))
				return false;

			if (path.Contains(directory))
			{
				while (!string.IsNullOrEmpty(path))
				{
					var subdir = Path.GetFileName(path);
					if (subdir == directory)
						return true;
					path = Path.GetDirectoryName(path);
				}
			}
			return false;
		}
	}
}
