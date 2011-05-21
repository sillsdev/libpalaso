using System;
using System.IO;
using System.Windows.Forms;
using Palaso.Reporting;

namespace Palaso.IO
{
	public class FolderUtils
	{

		/// <summary>
		/// Makes a full copies of the specified folder in the system's temporary folder.
		/// If the copy fails at any point in the process, the user is notified of the
		/// problem and an attempt is made to remove the destination folder if the failure
		/// happened part way through the process.
		/// </summary>
		/// <param name="srcFolder">Folder to copy</param>
		/// <returns>Null if the copy was unsuccessful, otherwise the path to the copied folder</returns>

		public static string CopyFolderToTempFolder(string srcFolder)
		{
			string dstFolder;
			return (CopyFolder(srcFolder, Path.GetTempPath(), out dstFolder) ? dstFolder : null);
		}


		/// <summary>
		/// Makes a copy of the specifed source folder and its contents in the specified
		/// destination folder. The copy has the same folder name as the source, but ends up
		/// as a sub folder of the specified destination folder. The destination folder must
		/// already exist. If the copy fails at any point in the process, the user is notified
		/// of the problem and an attempt is made to remove the destination folder if the
		/// failure happened part way through the process.
		/// </summary>
		/// <param name="srcFolder">Folder being copied</param>
		/// <param name="dstFolderParent">Destination folder where source folder and its contents are copied</param>
		/// <returns>true if successful, otherwise, false.</returns>

		public static bool CopyFolder(string srcFolder, string dstFolderParent)
		{
			string dstFolder;
			return CopyFolder(srcFolder, dstFolderParent, out dstFolder);
		}


		private static bool CopyFolder(string srcFolder, string dstFolderParent, out string dstFolder)
		{
			dstFolder = Path.GetFileName(srcFolder);
			dstFolder = Path.Combine(dstFolderParent, dstFolder);

			if (!Directory.Exists(dstFolderParent))
			{
				ReportFailedCopyAndCleanUp(
					new DirectoryNotFoundException(dstFolderParent + " not found."),
					srcFolder, dstFolderParent);

				return false;
			}

			return CopyFolderContents(srcFolder, dstFolder);
		}


		/// <summary>
		/// Copies the specified source folder's contents to the specified destination folder.
		/// If the destination folder does not exist, it will be created first. If the source
		/// folder contains sub folders, those and their content will also be copied. If the
		/// copy fails at any point in the process, the user is notified of the problem and
		/// an attempt is made to remove the destination folder if the failure happened part
		/// way through the process.
		/// </summary>
		/// <param name="sourcePath">Folder whose contents will be copied</param>
		/// <param name="destinationPath">Destination folder receiving the content of the source folder</param>
		/// <returns>true if successful, otherwise, false.</returns>
		///
		public static bool CopyFolderContents(string sourcePath, string destinationPath)
		{
			try
			{
				CopyFolderWithException(sourcePath,destinationPath);
			}
			catch (Exception e)
			{
				//Review: generally, it's better if Palaso doesn't undertake to make these kind of UI decisions.
				//I've extracted CopyFolderWithException, so as not to mess up whatever client is using this version
				ReportFailedCopyAndCleanUp(e, sourcePath, destinationPath);
				return false;
			}

			return true;
		}

		public static void CopyFolderWithException(string sourcePath, string destinationPath)
		{
			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);

			// Copy all the files.
			foreach (var filepath in Directory.GetFiles(sourcePath))
			{
				var filename = Path.GetFileName(filepath);
				File.Copy(filepath, Path.Combine(destinationPath, filename));
			}

			// Copy all the sub folders.
			foreach (var folderpath in Directory.GetDirectories(sourcePath))
			{
				var foldername = Path.GetFileName(folderpath);
				CopyFolderContents(folderpath, Path.Combine(destinationPath, foldername));
			}
		}


		/// <summary>
		/// Directory.Move fails if the src and dest are on different partitions (e.g., temp and documents are on differen drives).
		/// This will do a move if it can, else do a copy followed by a delete.
		/// </summary>
		public static void MoveDirectorySafely(string sourcePath, string destinationPath)
		{
			if(Path.GetPathRoot(destinationPath).ToLower() == Path.GetPathRoot(sourcePath).ToLower())
			{
				Directory.Move(sourcePath, destinationPath);
				return;
			}
			CopyFolderWithException(sourcePath,destinationPath);
			Directory.Delete(sourcePath,true);
		}


		private static void ReportFailedCopyAndCleanUp(Exception error, string srcFolder, string dstFolder)
		{
			ErrorReport.NotifyUserOfProblem(error, "{0} was unable to copy the folder\n\n{1}\n\nto\n\n{2}",
				Application.ProductName, srcFolder, dstFolder);

			try
			{
				if (!Directory.Exists(dstFolder))
					return;

				// Clean up by removing the partially copied folder.
				Directory.Delete(dstFolder, true);
			}
			catch { }
		}
	}
}
