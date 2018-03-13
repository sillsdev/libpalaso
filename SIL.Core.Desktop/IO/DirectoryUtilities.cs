// Copyright (c) 2017-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.IO
{
	/// <summary>
	/// Desktop-specific utility methods for processing directories, e.g. methods that report to the user.
	/// </summary>
	public static class DirectoryUtilities
	{
		/// <summary>
		/// Makes a full copy of the specified directory in the system's temporary directory.
		/// If the copy fails at any point in the process, the user is notified of the
		/// problem and an attempt is made to remove the destination directory if the failure
		/// happened part way through the process.
		/// </summary>
		/// <param name="srcDirectory">Directory to copy</param>
		/// <returns>Null if the copy was unsuccessful, otherwise the path to the copied directory</returns>
		public static string CopyDirectoryToTempDirectory(string srcDirectory)
		{
			string dstDirectory;
			return (CopyDirectory(srcDirectory, Path.GetTempPath(), out dstDirectory) ? dstDirectory : null);
		}


		/// <summary>
		/// Makes a copy of the specifed source directory and its contents in the specified
		/// destination directory. The copy has the same directory name as the source, but ends up
		/// as a sub directory of the specified destination directory. The destination directory must
		/// already exist. If the copy fails at any point in the process, the user is notified
		/// of the problem and an attempt is made to remove the destination directory if the
		/// failure happened part way through the process.
		/// </summary>
		/// <param name="srcDirectory">Directory being copied</param>
		/// <param name="dstDirectoryParent">Destination directory where source directory and its contents are copied</param>
		/// <returns>true if successful, otherwise, false.</returns>
		public static bool CopyDirectory(string srcDirectory, string dstDirectoryParent)
		{
			string dstDirectory;
			return CopyDirectory(srcDirectory, dstDirectoryParent, out dstDirectory);
		}

		private static bool CopyDirectory(string srcDirectory, string dstDirectoryParent, out string dstDirectory)
		{
			dstDirectory = Path.Combine(dstDirectoryParent, Path.GetFileName(srcDirectory));

			if (!Directory.Exists(dstDirectoryParent))
			{
				ErrorReport.NotifyUserOfProblem(new DirectoryNotFoundException(dstDirectoryParent + " not found."),
					"{0} was unable to copy the directory {1} to {2}", EntryAssembly.ProductName, srcDirectory, dstDirectoryParent);
				return false;
			}
			if (DirectoryHelper.AreEquivalent(srcDirectory, dstDirectory))
			{
				ErrorReport.NotifyUserOfProblem(new Exception("Cannot copy directory to itself."),
					"{0} was unable to copy the directory {1} to {2}", EntryAssembly.ProductName, srcDirectory, dstDirectoryParent);
				return false;
			}

			return CopyDirectoryContents(srcDirectory, dstDirectory);
		}


		/// <summary>
		/// Copies the specified source directory's contents to the specified destination directory.
		/// If the destination directory does not exist, it will be created first. If the source
		/// directory contains sub directories, those and their content will also be copied. If the
		/// copy fails at any point in the process, the user is notified of the problem and
		/// an attempt is made to remove the destination directory if the failure happened part
		/// way through the process.
		/// </summary>
		/// <param name="sourcePath">Directory whose contents will be copied</param>
		/// <param name="destinationPath">Destination directory receiving the content of the source directory</param>
		/// <returns>true if successful, otherwise, false.</returns>
		public static bool CopyDirectoryContents(string sourcePath, string destinationPath)
		{
			try
			{
				DirectoryHelper.Copy(sourcePath,destinationPath);
			}
			catch (Exception e)
			{
				//Review: generally, it's better if Palaso doesn't undertake to make these kind of UI decisions.
				//I've extracted CopyDirectoryWithException, so as not to mess up whatever client is using this version
				ReportFailedCopyAndCleanUp(e, sourcePath, destinationPath);
				return false;
			}

			return true;
		}

		private static void ReportFailedCopyAndCleanUp(Exception error, string srcDirectory, string dstDirectory)
		{
			ErrorReport.NotifyUserOfProblem(error, "{0} was unable to copy the directory {1} to {2}",
				EntryAssembly.ProductName, srcDirectory, dstDirectory);

			try
			{
				if (!Directory.Exists(dstDirectory))
					return;

				// Clean up by removing the partially copied directory.
				Directory.Delete(dstDirectory, true);
			}
			catch { }
		}

		/// <summary>
		/// Sets the permissions for this directory so that everyone has full control
		/// </summary>
		/// <param name="fullDirectoryPath"></param>
		/// <param name="showErrorMessage"></param>
		/// <returns>True if able to set access, False otherwise</returns>
		public static bool SetFullControl(string fullDirectoryPath, bool showErrorMessage = true)
		{
			if (!Platform.IsWindows)
				return false;

			// get current settings
			var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			var security = Directory.GetAccessControl(fullDirectoryPath, AccessControlSections.Access);
			var currentRules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

			// if everyone already has full control, return now
			if (currentRules.Cast<FileSystemAccessRule>()
				.Where(rule => rule.IdentityReference.Value == everyone.Value)
				.Any(rule => rule.FileSystemRights == FileSystemRights.FullControl))
			{
				return true;
			}

			// initialize
			var returnVal = false;

			try
			{
				// set the permissions so everyone can read and write to this directory
				var fullControl = new FileSystemAccessRule(everyone,
															FileSystemRights.FullControl,
															InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
															PropagationFlags.None,
															AccessControlType.Allow);
				security.AddAccessRule(fullControl);
				Directory.SetAccessControl(fullDirectoryPath, security);

				returnVal = true;
			}
			catch (Exception ex)
			{
				if (showErrorMessage)
				{
					ErrorReport.NotifyUserOfProblem(ex, "{0} was not able to set directory security for '{1}' to 'full control' for everyone.",
						EntryAssembly.ProductName, fullDirectoryPath);
				}
			}

			return returnVal;
		}

		[Obsolete("Use DirectoryHelper.Copy()")]
		public static void CopyDirectoryWithException(string sourcePath, string destinationPath, bool overwrite = false)
		{
			DirectoryHelper.Copy(sourcePath, destinationPath, overwrite);
		}

		[Obsolete("Use DirectoryHelper.AreEquivalent()")]
		public static bool AreDirectoriesEquivalent(string dir1, string dir2)
		{
			return DirectoryHelper.AreEquivalent(dir1, dir2);
		}

		[Obsolete("Use DirectoryHelper.AreEquivalent()")]
		public static bool AreDirectoriesEquivalent(DirectoryInfo dirInfo1, DirectoryInfo dirInfo2)
		{
			return DirectoryHelper.AreEquivalent(dirInfo1, dirInfo2);
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
		[Obsolete("Use DirectoryHelper.Move()")]
		public static void MoveDirectorySafely(string sourcePath, string destinationPath)
		{
			DirectoryHelper.Move(sourcePath, destinationPath);
		}

		/// <summary>
		/// Return subdirectories of <paramref name="path"/> that are not system or hidden.
		/// There are some cases where our call to Directory.GetDirectories() throws.
		/// For example, when the access permissions on a folder are set so that it can't be read.
		/// Another possible example may be Windows Backup files, which apparently look like directories.
		/// </summary>
		/// <param name="path">Directory path to look in.</param>
		/// <returns>Zero or more directory names that are not system or hidden.</returns>
		/// <exception cref="System.UnauthorizedAccessException">E.g. when the user does not have
		/// read permission.</exception>
		[Obsolete("Use DirectoryHelper.GetSafeDirectories()")]
		public static string[] GetSafeDirectories(string path)
		{
			return DirectoryHelper.GetSafeDirectories(path);
		}

		/// <summary>
		/// There are various things which can prevent a simple directory deletion, mostly timing related things which are hard to debug.
		/// This method uses all the tricks to do its best.
		/// </summary>
		/// <returns>returns true if the directory is fully deleted</returns>
		[Obsolete("Use RobustIO.DeleteDirectoryAndContents()")]
		public static bool DeleteDirectoryRobust(string path, bool overrideReadOnly = true)
		{
			return RobustIO.DeleteDirectoryAndContents(path, overrideReadOnly);
		}

		/// <summary>
		/// If necessary, append a number to make the folder path unique.
		/// </summary>
		/// <param name="folderPath">Source folder pathname.</param>
		/// <returns>A unique folder pathname at the same level as <paramref name="folderPath"/>. It may have a number apended to <paramref name="folderPath"/>, or it may be <paramref name="folderPath"/>.</returns>
		[Obsolete("Use PathHelper.GetUniqueFolderPath()")]
		public static string GetUniqueFolderPath(string folderPath)
		{
			return PathHelper.GetUniqueFolderPath(folderPath);
		}

		/// <summary>
		/// Checks if there are any entries in a directory
		/// </summary>
		/// <param name="path">Path to the directory to check</param>
		/// <param name="onlyCheckForFiles">if this is TRUE, a directory that contains subdirectories but no files will be considered empty.
		/// Subdirectories are not checked, so if onlyCheckForFiles is TRUE and there is a subdirectory that contains a file, the directory
		/// will still be considered empty.</param>
		/// <returns></returns>
		[Obsolete("Use DirectoryHelper.IsEmpty()")]
		public static bool DirectoryIsEmpty(string path, bool onlyCheckForFiles = false)
		{
			return DirectoryHelper.IsEmpty(path, onlyCheckForFiles);
		}
	}
}
