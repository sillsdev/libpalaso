using System;
using System.IO;
using System.Text;
using SIL.Retry;

namespace SIL.IO
{
	/// <summary>
	/// Provides a more robust version of System.IO.File methods.
	/// The original intent of this class is to attempt to mitigate issues
	/// where we attempt IO but the file is locked by another application.
	/// Our theory is that some anti-virus software locks files while it scans them.
	///
	/// The reason some methods are included here but not implemented differently from
	/// System.IO.File is that this class is intended as a full replacement for System.IO.File.
	/// The decision of which to provide a special implementation for is based on the
	/// initial attempt to resolve locked file problems.
	/// </summary>
	public static class RobustFile
	{
		public static void Copy(string sourceFileName, string destFileName)
		{
			RetryUtility.Retry(() => File.Copy(sourceFileName, destFileName));
		}

		public static void Copy(string sourceFileName, string destFileName, bool overwrite)
		{
			RetryUtility.Retry(() => File.Copy(sourceFileName, destFileName, overwrite));
		}

		public static FileStream Create(string path)
		{
			// Nothing different from File for now
			return File.Create(path);
		}

		public static StreamWriter CreateText(string path)
		{
			// Nothing different from File for now
			return File.CreateText(path);
		}

		public static void Delete(string path)
		{
			RetryUtility.Retry(() => File.Delete(path));
		}

		public static bool Exists(string path)
		{
			// Nothing different from File for now
			return File.Exists(path);
		}

		public static FileAttributes GetAttributes(string path)
		{
			return RetryUtility.Retry(() => File.GetAttributes(path));
		}

		public static DateTime GetLastWriteTime(string path)
		{
			// Nothing different from File for now
			return File.GetLastAccessTime(path);
		}

		public static DateTime GetLastWriteTimeUtc(string path)
		{
			// Nothing different from File for now
			return File.GetLastAccessTimeUtc(path);
		}

		public static void Move(string sourceFileName, string destFileName)
		{
			RetryUtility.Retry(() => File.Move(sourceFileName, destFileName));
		}

		public static FileStream OpenRead(string path)
		{
			return RetryUtility.Retry(() => File.OpenRead(path));
		}

		public static StreamReader OpenText(string path)
		{
			return RetryUtility.Retry(() => File.OpenText(path));
		}

		public static byte[] ReadAllBytes(string path)
		{
			return RetryUtility.Retry(() => File.ReadAllBytes(path));
		}

		public static string[] ReadAllLines(string path)
		{
			return RetryUtility.Retry(() => File.ReadAllLines(path));
		}

		public static string[] ReadAllLines(string path, Encoding encoding)
		{
			return RetryUtility.Retry(() => File.ReadAllLines(path, encoding));
		}

		public static string ReadAllText(string path)
		{
			return RetryUtility.Retry(() => File.ReadAllText(path));
		}

		public static string ReadAllText(string path, Encoding encoding)
		{
			return RetryUtility.Retry(() => File.ReadAllText(path, encoding));
		}

		public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
		{
			RetryUtility.Retry(() => File.Replace(sourceFileName, destinationFileName, destinationBackupFileName));
		}

		public static void SetAttributes(string path, FileAttributes fileAttributes)
		{
			RetryUtility.Retry(() => File.SetAttributes(path, fileAttributes));
		}

		public static void WriteAllBytes(string path, byte[] bytes)
		{
			RetryUtility.Retry(() => File.WriteAllBytes(path, bytes));
		}

		public static void WriteAllText(string path, string contents)
		{
			RetryUtility.Retry(() => File.WriteAllText(path, contents));
		}

		public static void WriteAllText(string path, string contents, Encoding encoding)
		{
			RetryUtility.Retry(() => File.WriteAllText(path, contents, encoding));
		}
	}
}