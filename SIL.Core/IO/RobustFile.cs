// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using SIL.Code;
using System.Security.AccessControl;

namespace SIL.IO
{
	/// <summary>
	/// Provides a more robust version of System.IO.File methods.
	/// The original intent of this class is to attempt to mitigate issues
	/// where we attempt IO but the file is locked by another application.
	/// Our theory is that some anti-virus software locks files while it scans them.
	///
	/// Later (BL-4340) we encountered problems of corrupted files suspected to be caused
	/// by power failures or similar disasters before the disk write cache was flushed.
	/// In an attempt to fix this we made as many methods as possible use FileOptions.WriteThrough,
	/// which may slow things down but is supposed to guarantee the data is really written
	/// all the way to something persistent.
	///
	/// The reason some methods are included here but not implemented differently from
	/// System.IO.File is that this class is intended as a full replacement for System.IO.File.
	/// The decision of which to provide a special implementation for is based on the
	/// initial attempt to resolve locked file problems, and the later problems
	/// with cached writes.
	///
	/// These functions may not throw exactly the same exceptions in the same circumstances
	/// as the File methods with the same names.
	/// </summary>
	public static class RobustFile
	{
		private const int FileStreamBufferSize = 4096; // FileStream.DefaultBufferSize, unfortunately not public.

		// This is just a guess at a buffer size that should make copying reasonably efficient without
		// being too hard to allocate.
		internal static int BufferSize = FileStreamBufferSize; // internal so we can tweak in unit tests

		public static void Copy(string sourceFileName, string destFileName, bool overwrite = false)
		{
			RetryUtility.Retry(() =>
			{
				// This is my own implementation; the .NET library uses a native windows function.
				var buffer = new byte[BufferSize];
				using (var input = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read))
				{
					using (
						// We don't need WriteThrough here, it would just slow things down to force
						// each write to commit. The final Flush(true) guarantees everything is
						// written before the method returns.
						var output = new FileStream(destFileName,
							(overwrite ? FileMode.Create : FileMode.CreateNew),
							FileAccess.Write,
							FileShare.None, BufferSize,
							FileOptions.SequentialScan))
					{
						int count;
						while ((count = input.Read(buffer, 0, BufferSize)) > 0)
						{
							output.Write(buffer, 0, count);
						}
						// This is the whole point of replacing File.Copy(); forces a real write all the
						// way to disk.
						output.Flush(true);
					}
				}
				// original
				//File.Copy(sourceFileName, destFileName, overwrite);
			}, memo:$"Copy to {destFileName}");
		}

		/// <summary>
		/// Creates a stream that will write to disk on every write.
		/// For most purposes (except perhaps logging when crashes are likely),
		/// calling Flush(true) on the stream when finished would probably suffice.
		/// </summary>
		public static FileStream Create(string path)
		{
			// Make it based on a WriteThrough file.
			// This is based on the .Net implementation of Create (flattened somewhat)
			// File.Create calls File.Create(path, FileStream.DefaultBufferSize)
			// which returns new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileStream.DefaultBufferSize)
			// which calls another constructor adding FileOptions.None, Path.GetFileName(path), false
			// That constructor is unfortunately internal. But the simplest public constructor that takes a FileOptions
			// calls the same thing, passing the same extra two options. So the following gives the exact same
			// effect, except that unfortunately we can't get at the internal constant for FileStream.DefaultBufferSize
			// and just have to insert its value.
			return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileStreamBufferSize, FileOptions.WriteThrough);
			// original
			//return File.Create(path);
		}

		/// <summary>
		/// Creates a StreamWriter that will write to disk on every write.
		/// For most purposes (except perhaps logging when crashes are likely),
		/// calling Flush(true) on the stream when finished would probably suffice.
		/// </summary>
		public static StreamWriter CreateText(string path)
		{
			// Make it based on a WriteThrough file.
			// This is based on the .Net implementation of CreateText (flattened somewhat)
			// Various methods in the call chain check these things:
			if (path == null)
				throw new ArgumentNullException(nameof(path)); // TeamCity builds do not handle nameof()
			if (path.Length == 0)
				throw new ArgumentException("Argument_EmptyPath");

			// File.CreateText uses the FileStream constructor, passing path, false[not append].
			// that uses another FileStream constructor, passing path, false[not append], UTF8NoBOM, DefaultBufferSize[=1024]
			// that uses another constructor, with the same arguments plus true[checkHost]

			// apart from error checking, the 5-argument FileStream constructor does:
			// Stream stream = CreateFile(path, append, checkHost);
			// Init(stream, encoding, bufferSize, false);

			// The call to CreateFile expands to a call to an internal constructor, equivalent to:
			// Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read,
			//	4096, FileOptions.SequentialScan, Path.GetFileName(path), false, false, true)
			// The fileStream constructor call below is equivalent with two exceptions:
			// - we're adding in FileOptions.WriteThrough (as desired)
			// - we end up passing false instead of true for an obscure internal option called checkHost,
			//    which does nothing in the version of .NET that VS thinks we're using.
			Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read,
				FileStreamBufferSize, FileOptions.SequentialScan | FileOptions.WriteThrough);

			// The call to Init works out to Init(path, UTF8NoBOM, DefaultBufferSize, false), but
			// we can't call that private method from here.
			// However it is also called by the StreamWriter constructor which takes a stream.
			// In fact, that constructor provides the same values for the other default arguments.
			// So we can just call the simple constructor.
			return new StreamWriter(stream);

			// This is the original (not write-through) implementation.
			//return File.CreateText(path);
		}

		public static void Delete(string path)
		{
			RetryUtility.Retry(() => File.Delete(path), memo:$"Delete {path}");
		}

		public static bool Exists(string path)
		{
			// Nothing different from File for now
			return File.Exists(path);
		}

		public static FileAttributes GetAttributes(string path)
		{
			return RetryUtility.Retry(() => File.GetAttributes(path), memo:$"GetAttributes {path}");
		}

		[PublicAPI]
		public static DateTime GetLastWriteTime(string path)
		{
			// Nothing different from File for now
			return File.GetLastWriteTime(path);
		}

		[PublicAPI]
		public static DateTime GetLastWriteTimeUtc(string path)
		{
			// Nothing different from File for now
			return File.GetLastWriteTimeUtc(path);
		}

		public static void Move(string sourceFileName, string destFileName)
		{
			RetryUtility.Retry(() => File.Move(sourceFileName, destFileName), memo:$"Move to {destFileName}");
		}

		// TODO: Look at the myriad places where the above method is used that could be
		// simplified by removing preceding code to check for and delete the destination file.
		public static void Move(string sourceFileName, string destFileName, bool overWrite)
		{
			if (overWrite && Exists(destFileName))
				Delete(destFileName);
			Move(sourceFileName, destFileName);
		}

		public static FileStream OpenRead(string path)
		{
			return RetryUtility.Retry(() => File.OpenRead(path), memo:$"OpenRead {path}");
		}

		public static StreamReader OpenText(string path)
		{
			return RetryUtility.Retry(() => File.OpenText(path), memo:$"OpenText {path}");
		}

		[PublicAPI]
		public static byte[] ReadAllBytes(string path)
		{
			return RetryUtility.Retry(() => File.ReadAllBytes(path), memo:$"ReadAllBytes {path}");
		}

		[PublicAPI]
		public static string[] ReadAllLines(string path)
		{
			return RetryUtility.Retry(() => File.ReadAllLines(path), memo:$"ReadAllLines {path}");
		}

		[PublicAPI]
		public static string[] ReadAllLines(string path, Encoding encoding)
		{
			return RetryUtility.Retry(() => File.ReadAllLines(path, encoding), memo:$"ReaderAllLines {path},{encoding}");
		}

		public static string ReadAllText(string path)
		{
			return RetryUtility.Retry(() => File.ReadAllText(path), memo:$"ReadAllText {path}");
		}

		public static string ReadAllText(string path, Encoding encoding)
		{
			return RetryUtility.Retry(() => File.ReadAllText(path, encoding), memo:$"ReadAllText {path},{encoding}");
		}

		public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
		{
			// I haven't tried to make this WriteThrough; not sure how to do that while keeping all the
			// clever properties Replace has.
			RetryUtility.Retry(() =>
			{
				try
				{
					File.Replace(sourceFileName, destinationFileName, destinationBackupFileName);
				}
				catch (UnauthorizedAccessException uae)
				{
					// We were getting this while trying to Replace on a JAARS network drive.
					// The network drive is U:\ which maps to \\waxhaw\users\{username}.
					// Both files were in the same directory and there were no permissions issues,
					// but the Replace command was failing with "Access to the path is denied." anyway.
					// I never could figure out why. See http://issues.bloomlibrary.org/youtrack/issue/BL-4436.
					// There is very similar code in FileUtils.ReplaceFileWithUserInteractionIfNeeded.
					try
					{
						ReplaceByCopyDelete(sourceFileName, destinationFileName, destinationBackupFileName);
					}
					catch
					{
						// Though it probably doesn't matter, report the original exception since we prefer Replace to CopyDelete.
						throw uae;
					}
				}
			}, memo:$"Replace {destinationFileName}");
		}

		public static void ReplaceByCopyDelete(string sourcePath, string destinationPath, string backupPath)
		{
			if (!string.IsNullOrEmpty(backupPath) && Exists(destinationPath))
			{
				Copy(destinationPath, backupPath, true);
			}
			Copy(sourcePath, destinationPath, true);
			Delete(sourcePath);
		}

		public static void SetAttributes(string path, FileAttributes fileAttributes)
		{
			RetryUtility.Retry(() => File.SetAttributes(path, fileAttributes), memo:$"SetAttributes {path}");
		}

		public static void WriteAllBytes(string path, byte[] bytes)
		{
			RetryUtility.Retry(() =>
			{
				// This forces it to block until the data is really safely on disk.
				using (var f = File.Create(path, FileStreamBufferSize, FileOptions.WriteThrough))
				{
					f.Write(bytes, 0, bytes.Length);
					f.Close();
				}
			}, memo:$"WriteAllBytes {path}");
		}

		public static void WriteAllText(string path, string contents)
		{
			// Note that we can't just call WriteAllText(path, contents, Encoding.Utf8).
			// That would insert an unwanted BOM.
			var content = Encoding.UTF8.GetBytes(contents);
			WriteAllBytes(path, content);
		}

		/// <summary>
		/// As in Windows, this version inserts a BOM at the start of the file. Thus,
		/// in particular, the file produced by WriteString(x,y, Encoding.UTF8) is not
		/// the same as that produced by WriteString(x, y), though both are encoded
		/// using UTF8.
		/// On Windows, the BOM is inserted even if contents is an empty string.
		/// As of Mono 3.4, Linux instead writes an empty file. We think this is a bug.
		/// Accordingly, this version is consistent and writes a BOM on both platforms,
		/// even for empty strings.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="contents"></param>
		/// <param name="encoding"></param>
		public static void WriteAllText(string path, string contents, Encoding encoding)
		{
			// It's helpful to check for these first so we don't actually create the file.
			if (contents == null)
				throw new ArgumentNullException(nameof(contents), @"contents must not be null");
			if (encoding == null)
				throw new ArgumentNullException(nameof(encoding), @"encoding must not be null");
			RetryUtility.Retry(() =>
			{
				// This forces it to block until the data is really safely on disk.
				using (var f = File.Create(path, FileStreamBufferSize, FileOptions.WriteThrough))
				{
					var preamble = encoding.GetPreamble();
					f.Write(preamble, 0, preamble.Length);
					var content = encoding.GetBytes(contents);
					f.Write(content, 0, content.Length);
					f.Close();
				}
			}, memo:$"WriteAllText {path}");
		}

		public static FileStream Open(string filePath, FileMode mode)
		{
			FileStream stream = null;
			RetryUtility.Retry(() => { stream = File.Open(filePath, mode); }, memo:$"Open {filePath}, {mode}");
			return stream;
		}
		public static FileStream Open(string filePath, FileMode mode, FileAccess access, FileShare share)
		{
			FileStream stream = null;
			RetryUtility.Retry(() => { stream = File.Open(filePath, mode, access, share); }, memo:$"Open {filePath}");
			return stream;
		}

		public static void AppendAllText(string path, string contents)
		{
			RetryUtility.Retry(() => File.AppendAllText(path, contents), memo:$"AppendAllText {path}");
		}

		[PublicAPI]
		public static void WriteAllLines(string path, IEnumerable<string> contents)
		{
			// Note: this doesn't block until the data is really safely on disk.
			RetryUtility.Retry(() => File.WriteAllLines(path, contents), memo:$"WriteAllLines {path}");
		}

		public static FileSecurity GetAccessControl(string filePath)
		{
#if NET462
			return RetryUtility.Retry(() => File.GetAccessControl(filePath), memo: $"GetAccessControl {filePath}");
#else
			return RetryUtility.Retry(() => FileSystemAclExtensions.GetAccessControl(new FileInfo(filePath)), memo: $"GetAccessControl {filePath}");
#endif
		}
	}
}
