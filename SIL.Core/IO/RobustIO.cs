// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using SIL.Code;
using SIL.PlatformUtilities;

namespace SIL.IO
{
	/// <summary>
	/// Provides a more robust version of various IO methods.
	/// The original intent of this class is to attempt to mitigate issues
	/// where we attempt IO but the file is locked by another application.
	/// Our theory is that some anti-virus software locks files while it scans them.
	/// </summary>
	public static class RobustIO
	{
		public static void DeleteDirectory(string path, bool recursive = false)
		{
			if(recursive)
			{
				var succeeded = DeleteDirectoryAndContents(path);
				if(!succeeded)
					throw new IOException("Could not delete directory "+path);
			}
			else
				RetryUtility.Retry(() => Directory.Delete(path, false), memo:$"DeleteDirectory {path}, {recursive}");
		}

		/// <summary>
		/// There are various things which can prevent a simple directory deletion, mostly timing related things which are hard to debug.
		/// This method uses all the tricks to do its best.
		/// </summary>
		/// <returns>returns true if the directory is fully deleted</returns>
		public static bool DeleteDirectoryAndContents(string path, bool overrideReadOnly = true)
		{
			// ReSharper disable EmptyGeneralCatchClause

			var failedToDeleteAChildDirectory = false;

			if (!Platform.IsWindows)
			{
				// The Mono runtime deletes readonly files and directories that contain readonly files.
				// This violates the MSDN specification of Directory.Delete and File.Delete.
				if (!overrideReadOnly && DirectoryContainsReadOnly(path))
					return false;
			}

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
							if (overrideReadOnly)
							{
								File.SetAttributes(filePath, FileAttributes.Normal);
							}
							File.Delete(filePath);
						}
						catch (Exception)
						{
						}
					}
					foreach (var dir in dirs)
					{
						if (!DeleteDirectoryAndContents(dir, overrideReadOnly))
						{
							failedToDeleteAChildDirectory = true;
						}
					}
				}
				catch (Exception)//yes, even these simple queries can throw exceptions, as stuff suddenly is deleted based on our prior request
				{
				}

				// if a child directory could not be deleted, well that already took us 2 seconds. If we keep trying, then
				// we will take up 40 * 2 seconds. Now if that child is 2 levels down, we're up to 40 * 40 * 2, etc. So give up.
				if (failedToDeleteAChildDirectory)
					break;

				//sleep and let some OS things catch up
				Thread.Sleep(50);
			}

			return !Directory.Exists(path);
			// ReSharper restore EmptyGeneralCatchClause
		}

		/// <summary>
		/// Check whether the given directory is readonly, or contains files or subdirectories that are readonly.
		/// </summary>
		/// <remarks>
		/// Using this check could be considered a workaround for a bug in the Mono runtime, but that bug goes so
		/// deep that it's safer and easier to work around it here.
		/// </remarks>
		private static bool DirectoryContainsReadOnly(string path)
		{
			var dirInfo = new DirectoryInfo(path);
			if ((dirInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				return true;
			foreach (var file in Directory.GetFiles(path))
			{
				var fileInfo = new FileInfo(file);
				if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					return true;
			}
			foreach (var dir in Directory.GetDirectories(path))
			{
				if (DirectoryContainsReadOnly(dir))
					return true;
			}
			return false;
		}

		public static void MoveDirectory(string sourceDirName, string destDirName)
		{
			RetryUtility.Retry(() => Directory.Move(sourceDirName, destDirName), memo:$"MoveDirectory to {destDirName}");
		}

		/// <summary>
		/// Robustly try to enumerate all of the files in a directory.  Unfortunately, this makes the
		/// method wait until all the files are gathered before any are returned.
		/// </summary>
		public static IEnumerable<string> EnumerateFilesInDirectory(string folderPath, string searchPattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
		{
			// Directory.EnumerateFiles returns files incrementally, not waiting until it has
			// accessed the whole directory. Thus retries of this method could return multiple
			// instances of some file paths, which is undesirable.  We accumulate the files in
			// a HashSet to avoid duplicates in case the operation has to be retried.  This
			// unavoidably slows things down since we have to wait until all the files are
			// gathered before any are returned.
			var fileSet = new HashSet<string>();
			RetryUtility.Retry(() =>
				EnumerateFilesInDirectoryInternal(folderPath, searchPattern, option, fileSet),
				RetryUtility.kDefaultMaxRetryAttempts,
				RetryUtility.kDefaultRetryDelay,
				new HashSet<Type>
				{
					Type.GetType("System.IO.IOException"),
					Type.GetType("System.Runtime.InteropServices.ExternalException")
				},
				memo:$"EnumerateFilesInDirectory {folderPath}, {searchPattern}, {option}");
			return fileSet;
		}

		private static void EnumerateFilesInDirectoryInternal(string folderPath, string searchPattern, SearchOption option, HashSet<string> fileSet)
		{
			foreach (var file in Directory.EnumerateFiles(folderPath, searchPattern, option))
				fileSet.Add(file);
		}

		/// <summary>
		/// Robustly try to enumerate all of the subdirectories in a directory.  Unfortunately, this
		/// makes the method wait until all the subdirectories are gathered before any are returned.
		/// </summary>
		public static IEnumerable<string> EnumerateDirectoriesInDirectory(string folderPath, string searchPattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
		{
			// Directory.EnumerateDirectories returns subdirectories incrementally, not waiting
			// until it has accessed the whole directory. Thus retries of this method could return
			// multiple instances of some subdirectory paths, which is undesirable.  We accumulate
			// the subdirectories in a HashSet to avoid duplicates in case the operation has to be
			// retried.  This unavoidably slows things down since we have to wait until all the
			// subdirectories are gathered before any are returned.
			var subdirSet = new HashSet<string>();
			RetryUtility.Retry(() =>
				EnumerateDirectoriesInDirectoryInternal(folderPath, searchPattern, option, subdirSet),
				RetryUtility.kDefaultMaxRetryAttempts,
				RetryUtility.kDefaultRetryDelay,
				new HashSet<Type>
				{
					Type.GetType("System.IO.IOException"),
					Type.GetType("System.Runtime.InteropServices.ExternalException")
				},
				memo: $"EnumerateDirectoriesInDirectory {folderPath}, {searchPattern}, {option}");
			return subdirSet;
		}

		private static void EnumerateDirectoriesInDirectoryInternal(string folderPath, string searchPattern, SearchOption option, HashSet<string> subdirSet)
		{
			foreach (var subdir in Directory.EnumerateDirectories(folderPath, searchPattern, option))
				subdirSet.Add(subdir);
		}

		/// <summary>
		/// Robustly try to enumerate all of the entries in a directory.  Unfortunately, this makes
		/// the method wait until all the entries are gathered before any are returned.
		/// </summary>
		public static IEnumerable<string> EnumerateEntriesInDirectory(string folderPath, string searchPattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
		{
			// Directory.EnumerateFileSystemEntries returns entries incrementally, not waiting
			// until it has accessed the whole directory. Thus retries of this method could return
			// multiple instances of some entry paths, which is undesirable.  We accumulate the
			// entries in a HashSet to avoid duplicates in case the operation has to be retried.
			// This unavoidably slows things down since we have to wait until all the entries are
			// gathered before any are returned.
			var entrySet = new HashSet<string>();
			RetryUtility.Retry(() =>
				EnumerateEntriesInDirectoryInternal(folderPath, searchPattern, option, entrySet),
				RetryUtility.kDefaultMaxRetryAttempts,
				RetryUtility.kDefaultRetryDelay,
				new HashSet<Type>
				{
					Type.GetType("System.IO.IOException"),
					Type.GetType("System.Runtime.InteropServices.ExternalException")
				},
				memo: $"EnumerateEntriesInDirectory {folderPath}, {searchPattern}, {option}");
			return entrySet;
		}

		private static void EnumerateEntriesInDirectoryInternal(string folderPath, string searchPattern, SearchOption option, HashSet<string> entrySet)
		{
			foreach (var entry in Directory.EnumerateFileSystemEntries(folderPath, searchPattern, option))
				entrySet.Add(entry);
		}

		public static void RequireThatDirectoryExists(string path)
		{
			bool exists = false;
			RetryUtility.Retry(() => { exists = Directory.Exists(path); }, memo: $"RequireThatDirectoryExists {path}");
			if (!exists)
			{
				throw new ArgumentException($"The path '{path}' does not exist.");
			}
		}

		public static FileStream GetFileStream(string path, FileMode mode)
		{
			// Note that new FileStream(path, mode) uses different default values for FileAccess and FileShare than
			// does File.Open(path, mode).
			FileStream stream = null;
			RetryUtility.Retry(() => { stream = new FileStream(path, mode); }, memo: $"GetFileStream {path}, {mode}");
			return stream;
		}
		public static FileStream GetFileStream(string path, FileMode mode, FileAccess access)
		{
			// Note that new FileStream(path, mode, access) uses a different default value for FileShare than
			// does File.Open(path, mode, access).
			FileStream stream = null;
			RetryUtility.Retry(() => { stream = new FileStream(path, mode, access); }, memo:$"GetFileStream {path}, {mode}, {access}");
			return stream;
		}

		/// <summary>
		/// Reads all text (like RobustFile.ReadAllText) from a file. Works even if that file may
		/// be written to one or more times.
		/// e.g. reading the progress output file of ffmpeg while ffmpeg is running.
		/// </summary>
		/// <param name="path">path of the file to read</param>
		/// <returns>the contents of the file as a string</returns>
		public static string ReadAllTextFromFileWhichMightGetWrittenTo(string path)
		{
			return RetryUtility.Retry(() => ReadAllTextFromFileWhichMightGetWrittenToInternal(path),
				memo: $"ReadAllTextFromFileWhichMightGetWrittenTo {path}");
		}

		private static string ReadAllTextFromFileWhichMightGetWrittenToInternal(string path)
		{
			using (FileStream logFileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (StreamReader logFileReader = new StreamReader(logFileStream))
			{
				StringBuilder sb = new StringBuilder();

				char[] buffer = new char[4096];
				while (!logFileReader.EndOfStream)
				{
					logFileReader.ReadBlock(buffer, 0, buffer.Length);
					sb.Append(buffer);
				}

				return sb.ToString();
			}
		}

		public static bool IsFileLocked(string filePath)
		{
			try
			{
				// If something recently changed it we might get some spurious failures
				// to open it for modification.
				// BL-10139 indicated that the default 10 retries over two seconds
				// is sometimes not enough, so I've increased it here.
				// No guarantee that even 5s is enough if Dropbox is busy syncing a large
				// file across a poor internet, but I think after that it's better to give
				// the user a failed message.
				RetryUtility.Retry(() =>
				{
					using (File.Open(filePath, FileMode.Open))
					{
					}
				}, maxRetryAttempts: 25, memo: $"IsFileLocked: ${filePath}");
			}
			catch (IOException)
			{
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				return true;
			}
			return false;
		}

		public static XElement LoadXElement(string uri)
		{
			// Previously used RetryUtility on XElement.Load(uri). However, we had problems with this
			// in Bloom using some non-roman collection names...specifically, one involving the Northern Pashti
			// localization of 'books' (کتابونه)...see BL-5416. It seems that somewhere in the
			// implementation of Linq.XElement.Load() the path is converted to a URL and then back
			// to a path and something changes in that process so that a valid path passed to Load()
			// raises an invalid path exception. Reading the file directly and then parsing the string
			// works around this problem.
			var content = RobustFile.ReadAllText(uri, Encoding.UTF8);
			return XElement.Parse(content);
		}

		public static void SaveXElement(XElement xElement, string fileName)
		{
			RetryUtility.Retry(() => xElement.Save(fileName), memo:$"SaveXElement {fileName}");
		}

		/// <summary>
		/// Save an Xml document. This should be equivalent to doc.Save(path) except for extra robustness (and slowness).
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="path"></param>
		public static void SaveXml(XmlDocument doc, string path)
		{
			RetryUtility.Retry(() =>
			{
				using (var stream = RobustFile.Create(path))
				{
					doc.Save(stream);
					stream.Close();
				}
			}, memo:$"SaveXml {path}");
		}
	}
}
