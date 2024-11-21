// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace SIL.IO
{
	/// <summary>
	/// This is useful a temporary file is needed. When it is disposed, it will delete the file.
	/// 
	/// Sometimes it is useful to make a temp file and NOT have the TempFile class delete it. 
	/// In such cases, simply do not Dispose() the TempFile. To make this possible and reliable, 
	/// this class deliberately does NOT implement a destructor or do anything to ensure 
	/// the file is deleted if the TempFile is not disposed. Please don't change this.
	/// </summary>
	/// <example>using(f = new TempFile())</example>
	public class TempFile : IDisposable
	{
		private string _folderToDelete; // if not null, delete this as well on dispose
		private bool _detached;

		/// <summary>
		/// Create a file with a random name in the system temp folder and return a
		/// TempFile which will delete it when disposed.
		/// </summary>
		/// <remarks>If you don't set a <see cref="NamePrefix"/>, this uses the GetTempFileName() method
		/// to obtain a file. This can get very slow, or even fail, if too many of the 64K
		/// possible file names are already in use.</remarks>
		public TempFile()
		{
			Path = NamePrefix == null ? System.IO.Path.GetTempFileName() : MakeFileAtRandomPath();
		}

		/// <summary>
		/// Make an empty file at a random path. It is guaranteed to be a path that was not previously in use
		/// in the system temp folder. The file will be closed and empty, but will exist, so no subsequent
		/// call to this method could possibly yield the same path until it is deleted. The caller is
		/// responsible to make sure it gets deleted, typically by assigning it as the Path of a new
		/// TempFile and eventually calling Dispose() on it.
		/// </summary>
		/// <param name="extension">If set, the new file is guaranteed to have this extension.</param>
		/// <returns>The path to the new file</returns>
		private static string MakeFileAtRandomPath(string extension = null)
		{
			var needMoreTries = true;
			string result = "";
			while (needMoreTries)
			{
				result = System.IO.Path.Combine(System.IO.Path.GetTempPath(), (NamePrefix??"") + System.IO.Path.GetRandomFileName());
				if (extension != null)
				{
					result = System.IO.Path.ChangeExtension(result, extension);
				}
				try
				{
					// Try to make an empty file. Besides being consistent with GetTempFileName,
					// this ensures no other thread, or even process, can receive the same name
					// as long as it exists. Using FileStream like this rather than Exists
					// because it is atomic.
					new FileStream(result, FileMode.CreateNew).Close();
					needMoreTries = false;
				}
				catch (IOException)
				{
					// IOException occurs if the file exists. Just try again.
				}
			};
			return result;
		}

		public TempFile(bool dontMakeMeAFileAndDontSetPath)
		{
			if(!dontMakeMeAFileAndDontSetPath)
			{
				Path = System.IO.Path.GetTempFileName();
			}
		}

		/// <summary>
		/// Make a TempFile with the specified content. See also comment on <see cref="NamePrefix"/>.
		/// </summary>
		public TempFile(string contents)
			: this()
		{
			File.WriteAllText(Path, contents);
		}

		/// <summary>
		/// Make a TempFile with the specified content and Encoding. See also comment on <see cref="NamePrefix"/>.
		/// </summary>
		public TempFile(string contents, Encoding encoding)
			: this()
		{
			File.WriteAllText(Path, contents, encoding);
		}

		/// <summary>
		/// Make a TempFile with the specified content. See also comment on <see cref="NamePrefix"/>.
		/// </summary>
		public TempFile(string[] contentLines)
			: this()
		{
			File.WriteAllLines(Path, contentLines);
		}

		/// <summary>
		/// If this is non-null, TempFiles will be created with the specified prefix
		/// and using GetRandomFileName to generate names. This works for all the constructors
		/// that go through the default one and also WithExtension.
		/// Setting a <see cref="NamePrefix"/> has several benefits:
		/// - You can identify temp files your app failed to clean up.
		/// - You can clean them up automatically with CleanupTempFolder, e.g.,
		/// on app shutdown (or startup, to catch ones from a previous run).
		/// - Using GetRandomFilename means it won't slow down if the temp folder is
		/// very full of files created with the default GetTempFileName().
		/// Try to use a clearly unique prefix so you don't conflict with other apps.
		/// </summary>
		public static string NamePrefix { get; set; }

		/// <summary>
		/// If <see cref="NamePrefix"/> is set, remove all temp files (and directories, if any) created with that NamePrefix active.
		/// </summary>
		public static void CleanupTempFolder()
		{
			if (string.IsNullOrEmpty(NamePrefix))
				return; // or throw?
			foreach (var path in System.IO.Directory.EnumerateFiles(System.IO.Path.GetTempPath(),
				         NamePrefix + "*"))
			{
				try
				{
					// Deliberately don't use RobustFile. If deleting it is a problem, we don't want to
					// waste the time. In typical usage, we will try again next time our app runs.
					File.Delete(path);
				}
				catch (Exception)
				{
					// Don't worry if we can't delete it.
				}
			}

			foreach (var dir in System.IO.Directory.EnumerateDirectories(
				         System.IO.Path.GetTempPath(), NamePrefix + "*"))
			{
				try
				{
					Directory.Delete(dir, true);
				}
				catch (Exception)
				{
				}
			}
		}

		public string Path { get; protected set; }

		/// <summary>
		/// Don't try to delete this file when Dispose()'d.
		/// </summary>
		public void Detach()
		{
			_detached = true;
		}

		// See comment on class above regarding Dispose
		public void Dispose()
		{
			if (_detached)
			{
				return;
			}
			try
			{
				RobustFile.Delete(Path);
			}
			catch (IOException e)
			{
				// We tried, but we don't want to crash just because virus scanner or similar won't release the file.
				Debug.Fail("Could not delete temp file during Dispose(): " + e.Message, e.ToString());
			}
			if (_folderToDelete != null)
				RobustIO.DeleteDirectoryAndContents(_folderToDelete);
		}

		/// <summary>
		/// Make a TempFile that is a copy of the specified one. See also comment on <see cref="NamePrefix"/>.
		/// </summary>
		[PublicAPI]
		public static TempFile CopyOf(string pathToExistingFile)
		{
			TempFile t = new TempFile();
			File.Copy(pathToExistingFile, t.Path, true);
			return t;
		}

		public TempFile(string existingPath, bool dummy)
		{
			Path = existingPath;
		}

		/// <summary>
		/// Create a TempFile based on a pre-existing file, which will be deleted when this is disposed.
		/// </summary>
		public static TempFile TrackExisting(string path)
		{
			return new TempFile(path, false);
		}

		public static TempFile CreateAndGetPathButDontMakeTheFile()
		{
			// it's safer to use GetRandomFileName here because otherwise we might end up with
			// identical files with the same name if the app creates temp files quickly enough
			// while deleting the created file.
			var t = new TempFile(System.IO.Path.Combine(System.IO.Path.GetTempPath(),
				System.IO.Path.GetRandomFileName()), false);
			return t;
		}

		/// <summary>
		/// Use this one when it's important to have a certain file extension. See comment on <see cref="NamePrefix"/>.
		/// </summary>
		/// <param name="extension">with or with out '.', will work the same</param>
		public static TempFile WithExtension(string extension)
		{
			return TrackExisting(MakeFileAtRandomPath(extension));
		}

		/// <summary>
		/// Use this one when it's important to have a certain file name (with, or without extension).
		/// </summary>
		/// <param name="filename">with or with out an extension, will work the same</param>
		public static TempFile WithFilename(string filename)
		{
			if (filename == null) throw new ArgumentNullException(nameof(filename));
			if (filename == string.Empty)
				throw new ArgumentException("Filename has no content", nameof(filename));
			filename = filename.Trim();
			if (filename == string.Empty)
				throw new ArgumentException("Filename has only whitespace", nameof(filename));

			var pathname = System.IO.Path.Combine(System.IO.Path.GetTempPath(), filename);
			File.Create(pathname).Close();
			return TrackExisting(pathname);
		}

		/// <summary>
		/// Creates a file with the specified name in a new, randomly named folder.
		/// Dispose will dispose of the folder (and any subsequently added content) as well as the temp file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static TempFile WithFilenameInTempFolder(string fileName)
		{
			var tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			var path = System.IO.Path.Combine(tempFolder, fileName);
			var result = TrackExisting(path);
			result._folderToDelete = tempFolder;
			return result;
		}

		/// <summary>
		/// Used to make a real file out of a resource for the purpose of testing
		/// </summary>
		/// <param name="resource">e.g., an audio resource</param>
		/// <param name="extension">with or without '.', will work the same</param>
		public static TempFile FromResource(Stream resource, string extension)
		{
			var f = WithExtension(extension);
			byte[] buffer = new byte[resource.Length + 1];
			resource.Read(buffer, 0, (int)resource.Length);
			File.WriteAllBytes(f.Path, buffer);
			return f;
		}

		/// <summary>
		/// Used to make a real file out of a resource for the purpose of testing
		/// </summary>
		/// <param name="resource">e.g., a video resource</param>
		/// <param name="extension">with or with out '.', will work the same</param>
		public static TempFile FromResource(byte[] resource, string extension)
		{
			var f = WithExtension(extension);
			File.WriteAllBytes(f.Path, resource);
			return f;
		}

		/// <summary>
		/// Used to move a file to a new path
		/// </summary>
		public void MoveTo(string path)
		{
			File.Move(Path, path);
			Path = path;
		}


		/// <summary>
		/// Used to create a temporary filename in the same folder as another file.
		/// </summary>
		/// <param name="inputPath">path to an (existing) file</param>
		public static TempFile InFolderOf(string inputPath)
		{
			try
			{
				var folder = System.IO.Path.GetDirectoryName(inputPath);
				if (String.IsNullOrEmpty(folder))
					folder = ".";
				var path = System.IO.Path.Combine(folder, System.IO.Path.GetRandomFileName());
				return TrackExisting(path);
			}
			catch (Exception e)
			{
				throw new Exception($"TempFile.InFolderOf(\"{inputPath}\") failed", e);
			}
		}
	}
}
