// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

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

		public TempFile()
		{
			Path = System.IO.Path.GetTempFileName();
		}

		public TempFile(bool dontMakeMeAFileAndDontSetPath)
		{
			if(!dontMakeMeAFileAndDontSetPath)
			{
				Path = System.IO.Path.GetTempFileName();
			}
		}

		public TempFile(string contents)
			: this()
		{
			File.WriteAllText(Path, contents);
		}

		public TempFile(string contents, Encoding encoding)
			: this()
		{
			File.WriteAllText(Path, contents, encoding);
		}

		public TempFile(string[] contentLines)
			: this()
		{
			File.WriteAllLines(Path, contentLines);
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
		/// Use this one when it's important to have a certain file extension
		/// </summary>
		/// <param name="extension">with or with out '.', will work the same</param>
		public static TempFile WithExtension(string extension)
		{
			extension = extension.TrimStart('.');
			var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName() + "." + extension);
			File.Create(path).Close();
			return TrackExisting(path);
		}

		/// <summary>
		/// Use this one when it's important to have a certain file name (with, or without extension).
		/// </summary>
		/// <param name="filename">with or with out an extension, will work the same</param>
		public static TempFile WithFilename(string filename)
		{
			if (filename == null) throw new ArgumentNullException("filename");
			if (filename == string.Empty)
				throw new ArgumentException("Filename has no content", "filename");
			filename = filename.Trim();
			if (filename == string.Empty)
				throw new ArgumentException("Filename has only whitespace", "filename");

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
		/// <param name="extension">with or with out '.', will work the same</param>
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
