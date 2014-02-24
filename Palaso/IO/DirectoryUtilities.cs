using System;
using System.IO;
using System.Linq;
using System.Threading;

using Palaso.Reporting;

namespace Palaso.IO
{
	public class DirectoryUtilities
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
			if (AreDirectoriesEquivalent(srcDirectory, dstDirectory))
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
		/// directory contains sub directorys, those and their content will also be copied. If the
		/// copy fails at any point in the process, the user is notified of the problem and
		/// an attempt is made to remove the destination directory if the failure happened part
		/// way through the process.
		/// </summary>
		/// <param name="sourcePath">Directory whose contents will be copied</param>
		/// <param name="destinationPath">Destination directory receiving the content of the source directory</param>
		/// <returns>true if successful, otherwise, false.</returns>
		///
		public static bool CopyDirectoryContents(string sourcePath, string destinationPath)
		{
			try
			{
				CopyDirectoryWithException(sourcePath,destinationPath);
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

		public static void CopyDirectoryWithException(string sourcePath, string destinationPath)
		{
			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);

			// Copy all the files.
			foreach (var filepath in Directory.GetFiles(sourcePath))
			{
				var filename = Path.GetFileName(filepath);
				File.Copy(filepath, Path.Combine(destinationPath, filename));
			}

			// Copy all the sub directorys.
			foreach (var directorypath in Directory.GetDirectories(sourcePath))
			{
				var directoryname = Path.GetFileName(directorypath);
				CopyDirectoryContents(directorypath, Path.Combine(destinationPath, directoryname));
			}
		}

		public static bool AreDirectoriesEquivalent(string dir1, string dir2)
		{
			return AreDirectoriesEquivalent(new DirectoryInfo(dir1), new DirectoryInfo(dir2));
		}

		// Gleaned from http://stackoverflow.com/questions/2281531/how-can-i-compare-directory-paths-in-c
		public static bool AreDirectoriesEquivalent(DirectoryInfo dirInfo1, DirectoryInfo dirInfo2)
		{
			StringComparison comparison;
#if !MONO
			comparison = StringComparison.InvariantCultureIgnoreCase;
#else
			comparison = StringComparison.InvariantCulture;
#endif
			var backslash = new char[] { '\\' }; // added this step because mono does not implicitly convert from char to char[]
			return String.Compare(dirInfo1.FullName.TrimEnd(backslash), dirInfo2.FullName.TrimEnd(backslash), comparison) == 0;
		}

		/// <summary>
		/// Directory.Move fails if the src and dest are on different partitions (e.g., temp and
		/// documents are on different drives). This will do a move if it can, else do a copy
		/// followed by a delete.
		/// </summary>
		public static void MoveDirectorySafely(string sourcePath, string destinationPath)
		{
			if(Path.GetPathRoot(destinationPath).ToLower() == Path.GetPathRoot(sourcePath).ToLower())
			{
				try
				{
					Directory.Move(sourcePath, destinationPath);
					return;
				}
				catch (IOException)
				{
					// We get an IOException if
					// - An attempt was made to move a directory to a different volume (which
					//   can happen on Linux despite the test above).
					// - or destDirName already exists.
					// - or The sourceDirName and destDirName parameters refer to the same file
					//   or directory.
					// In the first case we want to try the copy approach.
					if (Path.GetFullPath(sourcePath).ToLower() == Path.GetFullPath(destinationPath).ToLower() ||
						Directory.Exists(destinationPath) || File.Exists(destinationPath))
					{
						throw;
					}
				}
			}
			CopyDirectoryWithException(sourcePath, destinationPath);
			Directory.Delete(sourcePath, true);
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
		/// Return subdirectories of <paramref name="path"/> that are not system or hidden.
		/// There are some cases where our call to Directory.GetDirectories() throws.
		/// For example, when the access permissions on a folder are set so that it can't be read.
		/// Another possible example may be Windows Backup files, which apparently look like directories.
		/// </summary>
		/// <param name="path">Directory path to look in.</param>
		/// <returns>Zero or more directory names that are not system or hidden.</returns>
		/// <exception cref="System.UnauthorizedAccessException">E.g. when the user does not have
		/// read permission.</exception>
		public static string[] GetSafeDirectories(string path)
		{
				return (from directoryName in Directory.GetDirectories(path)
						let dirInfo = new DirectoryInfo(directoryName)
						where (dirInfo.Attributes & FileAttributes.System) != FileAttributes.System
						where (dirInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden
						select directoryName).ToArray();
		}

		/// <summary>
		/// There are various things which can prevent a simple directory deletion, mostly timing related things which are hard to debug.
		/// This method uses all the tricks to do its best.
		/// </summary>
		/// <returns>returns true if the directory is fully deleted</returns>
		public static bool DeleteDirectoryRobust(string path)
		{
			// ReSharper disable EmptyGeneralCatchClause

			for (int i = 0; i < 40; i++) // each time, we sleep a little. This will try for up to 2 seconds (40*50ms)
			{
				if (!Directory.Exists(path))
					break;

				try
				{
					Directory.Delete(path, true);
				}
				catch (Exception)
				{
				}

				if (!Directory.Exists(path))
					break;

				try
				{
					//try to clear it out a bit
					string[] dirs = Directory.GetDirectories(path);
					string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
					foreach (string filePath in files)
					{
						try
						{
							/* we could do this too, but it's dangerous
							 *  File.SetAttributes(filePath, FileAttributes.Normal);
							 */
							File.Delete(filePath);
						}
						catch (Exception)
						{
						}
					}
					foreach (var dir in dirs)
					{
						DeleteDirectoryRobust(dir);
					}

				}
				catch (Exception)//yes, even these simple queries can throw exceptions, as stuff suddenly is deleted base on our prior request
				{
				}
				//sleep and let some OS things catch up
				Thread.Sleep(50);
			}

			return !Directory.Exists(path);
			// ReSharper restore EmptyGeneralCatchClause
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
	}
}
