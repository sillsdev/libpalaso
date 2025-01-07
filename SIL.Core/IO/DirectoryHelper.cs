// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using JetBrains.Annotations;
using SIL.PlatformUtilities;
using SIL.Reporting;
using static System.Environment;
using static System.Environment.SpecialFolder;
using static System.IO.Path;

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
				var filename = GetFileName(filepath);
				File.Copy(filepath, Combine(destinationPath, filename), overwrite);
			}

			// Copy all the subdirectories.
			foreach (var directoryPath in Directory.GetDirectories(sourcePath))
			{
				var directoryName = GetFileName(directoryPath);
				Copy(directoryPath, Combine(destinationPath, directoryName), overwrite);
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
					$"Could not find a part of the path '{sourcePath}'");
			}
		}

		public static bool AreEquivalent(string dir1, string dir2)
		{
			return AreEquivalent(new DirectoryInfo(dir1), new DirectoryInfo(dir2));
		}

		// Gleaned from http://stackoverflow.com/questions/2281531/how-can-i-compare-directory-paths-in-c
		public static bool AreEquivalent(DirectoryInfo dirInfo1, DirectoryInfo dirInfo2)
		{
			var comparison = Platform.IsWindows ? StringComparison.InvariantCultureIgnoreCase
				: StringComparison.InvariantCulture;
			var backslash = new[]
			{
				'\\', '/', '.'
			}; // added this step because mono does not implicitly convert from char to char[]
			return string.Compare(dirInfo1.FullName.TrimEnd(backslash),
				dirInfo2.FullName.TrimEnd(backslash), comparison) == 0;
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
		/// Subdirectories which the user does not have permission to access will also be skipped.
		/// There are some cases where our call to Directory.GetDirectories() throws.
		/// For example, when the access permissions on a folder are set so that it can't be read.
		/// Another possible example may be Windows Backup files, which apparently look like directories.
		/// </summary>
		/// <param name="path">Directory path to look in.</param>
		/// <returns>Zero or more directory names that are not system or hidden</returns>
		/// <exception cref="UnauthorizedAccessException">The caller does not have the required
		/// permission to access the subdirectories of <paramref name="path"/>.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="path" /> is <see langword="null" />.</exception>
		/// <exception cref="PathTooLongException">The specified <paramref name="path" />, file name,
		/// or both exceed the system-defined maximum length.</exception>
		/// <exception cref="IOException"><paramref name="path" /> is a file name.</exception>
		/// <exception cref="DirectoryNotFoundException">The specified <paramref name="path" /> is
		/// invalid (for example, it is on an unmapped drive).</exception>
		public static string[] GetSafeDirectories(string path)
		{
			var list = new List<string>();
			foreach (var directoryName in Directory.GetDirectories(path))
			{
				try
				{
					var dirInfo = new DirectoryInfo(directoryName);
					if ((dirInfo.Attributes & FileAttributes.System) != FileAttributes.System)
					{
						if ((dirInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
							list.Add(directoryName);
					}
				}
				catch (SecurityException e)
				{
					Logger.WriteError(e);
				}
			}

			return list.ToArray();
		}

		#region Utilities that are specific to the Windows OS.
		/// <summary>
		/// Returns 'true' unless we find we can't write to all the specified folders, in which
		/// case it performs the specified report action. Note that the action will never be
		/// performed more than once, since this method does not keep checking additional folders
		/// once it finds one that is not writable.
		/// In Oct of 2017, a Windows update to Defender on some machines set Protections on
		/// certain basic folders, like MyDocuments! This resulted in throwing an exception any
		/// time Bloom tried to write out CollectionSettings files! More recently (especially on
		/// Windows 11), some other apps have been having similar failures due to enhanced anti-
		/// ransomware checking.
		/// </summary>
		/// <param name="report">Action to perform if a folder is found which is not writable (or
		/// null to just return false)
		/// </param>
		/// <param name="foldersToCheck">The folders to test for write access.</param>
		[PublicAPI]
		public static bool CanWriteToDirectories(Action<Exception, string> report,
			params string[] foldersToCheck)
		{
			foreach (var folder in foldersToCheck)
			{
				if (!CanWriteToDirectory(report, folder))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns 'true' unless we find we can't write to the specified folder, in which
		/// case it performs the specified report action.
		/// In Oct of 2017, a Windows update to Defender on some machines set Protections on
		/// certain basic folders, like MyDocuments! This resulted in throwing an exception any
		/// time Bloom tried to write out CollectionSettings files! More recently (especially on
		/// Windows 11), some other apps have been having similar failures due to enhanced anti-
		/// ransomware checking.
		/// </summary>
		/// <param name="report">Action to perform if a folder is found which is not writable
		/// </param>
		/// <param name="folderToCheck">An optional folder to test for write access.</param>
		[PublicAPI]
		public static bool CanWriteToDirectory(Action<Exception, string> report,
			string folderToCheck = null)
		{
			if (!Platform.IsWindows)
				return true;

			if (string.IsNullOrEmpty(folderToCheck))
				folderToCheck = GetFolderPath(MyDocuments);
			else
			{
				if (!Directory.Exists(folderToCheck))
					throw new ArgumentException(
						"Folder provided should be an existing directory.",
						nameof(folderToCheck));
			}

			string testPath = GetTestFileName(folderToCheck);

			try
			{
				RobustFile.WriteAllText(testPath, "test contents");
			}
			catch (Exception exc)
			{
				// Creating a previously non-existent file under these conditions just gives a WinIOError, "could not find file".
				report?.Invoke(exc, GetDirectoryName(testPath));
				return false;
			}
			finally
			{
				Cleanup(testPath);
			}

			return true;
		}

		private static string GetTestFileName(string folderToCheck)
		{
			while (true)
			{
				var testPath = Combine(folderToCheck,
					GetFileName(TempFile.CreateAndGetPathButDontMakeTheFile().Path));
				// Really unlikely that the temp file name would exist, but let's check to be sure.
				if (!File.Exists(testPath) && !Directory.Exists(testPath))
					return testPath;
			}
		}

		private static void Cleanup(string testPath)
		{
			// try to clean up behind ourselves
			try
			{
				RobustFile.Delete(testPath);
			}
			catch (Exception)
			{
				// but don't try too hard
			}
		}
		#endregion
	}
}
