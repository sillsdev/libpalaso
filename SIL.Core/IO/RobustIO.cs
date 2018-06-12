// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
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
				RetryUtility.Retry(() => Directory.Delete(path, false));
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
			RetryUtility.Retry(() => Directory.Move(sourceDirName, destDirName));
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
			RetryUtility.Retry(() => xElement.Save(fileName));
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
			});
		}
	}
}
