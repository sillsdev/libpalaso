// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Linq;
using SIL.PlatformUtilities;

namespace SIL.IO
{
	public static class DirectoryHelper
	{
		public static void Copy(string sourcePath, string destinationPath, bool overwrite = false)
		{
			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);

			// Copy all the files.
			foreach (var filepath in Directory.GetFiles(sourcePath))
			{
				var filename = Path.GetFileName(filepath);
				File.Copy(filepath, Path.Combine(destinationPath, filename), overwrite);
			}

			// Copy all the sub directories.
			foreach (var directorypath in Directory.GetDirectories(sourcePath))
			{
				var directoryname = Path.GetFileName(directorypath);
				Copy(directorypath, Path.Combine(destinationPath, directoryname), overwrite);
			}
		}

		/// <summary>
		/// Move <paramref name="sourcePath"/> to <paramref name="destinationPath"/>. If src
		/// and dest are on different partitions (e.g., temp and documents are on different
		/// drives) then this method will do a copy followed by a delete. This is in contrast
		/// to Directory.Move which fails if src and dest are on different partitions.
		/// </summary>
		/// <param name="sourcePath">The source directory or file, similar to Directory.Move</param>
		/// <param name="destinationPath">The destination directory or file. If <paramref name="sourcePath"/>
		/// is a file then <paramref name="destinationPath"/> also needs to be a file.</param>
		public static void Move(string sourcePath, string destinationPath)
		{
			if (PathHelper.AreOnSameVolume(destinationPath, sourcePath))
			{
				Directory.Move(sourcePath, destinationPath);
				return;
			}
			if (Directory.Exists(sourcePath))
			{
				Copy(sourcePath, destinationPath);
				Directory.Delete(sourcePath, true);
			}
			else if (File.Exists(sourcePath))
			{
				if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
					throw new IOException("Cannot create a file when that file already exists.");

				File.Copy(sourcePath, destinationPath);
				File.Delete(sourcePath);
			}
			else
			{
				throw new DirectoryNotFoundException(
					string.Format("Could not find a part of the path '{0}'", sourcePath));
			}
		}

		public static bool AreEquivalent(string dir1, string dir2)
		{
			return AreEquivalent(new DirectoryInfo(dir1), new DirectoryInfo(dir2));
		}

		// Gleaned from http://stackoverflow.com/questions/2281531/how-can-i-compare-directory-paths-in-c
		public static bool AreEquivalent(DirectoryInfo dirInfo1, DirectoryInfo dirInfo2)
		{
			var comparison = Platform.IsWindows ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
			var backslash = new char[] { '\\', '/' }; // added this step because mono does not implicitly convert from char to char[]
			return string.Compare(dirInfo1.FullName.TrimEnd(backslash), dirInfo2.FullName.TrimEnd(backslash), comparison) == 0;
		}

		/// <summary>
		/// Checks if there are any entries in a directory
		/// </summary>
		/// <param name="path">Path to the directory to check</param>
		/// <param name="onlyCheckForFiles">if this is TRUE, a directory that contains subdirectories but no files will be considered empty.
		/// Subdirectories are not checked, so if onlyCheckForFiles is TRUE and there is a subdirectory that contains a file, the directory
		/// will still be considered empty.</param>
		/// <returns></returns>
		public static bool IsEmpty(string path, bool onlyCheckForFiles = false)
		{
			if (onlyCheckForFiles)
				return !Directory.EnumerateFiles(path).Any();

			return !Directory.EnumerateFileSystemEntries(path).Any();
		}

		/// <summary>
		/// Return subdirectories of <paramref name="path"/> that are not system or hidden.
		/// There are some cases where our call to Directory.GetDirectories() throws.
		/// For example, when the access permissions on a folder are set so that it can't be read.
		/// Another possible example may be Windows Backup files, which apparently look like directories.
		/// </summary>
		/// <param name="path">Directory path to look in.</param>
		/// <returns>Zero or more directory names that are not system or hidden.</returns>
		/// <exception cref="UnauthorizedAccessException">E.g. when the user does not have
		/// read permission.</exception>
		public static string[] GetSafeDirectories(string path)
		{
			return (from directoryName in Directory.GetDirectories(path)
					let dirInfo = new DirectoryInfo(directoryName)
					where (dirInfo.Attributes & FileAttributes.System) != FileAttributes.System
					where (dirInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden
					select directoryName).ToArray();
		}
	}
}
