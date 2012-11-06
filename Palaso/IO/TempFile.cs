using System;
using System.IO;
using System.Text;
using Palaso.Progress.LogBox;

namespace Palaso.IO
{

	/// <summary>
	/// This is useful a temporary file is needed. When it is disposed, it will delete the file.
	/// </summary>
	/// <example>using(f = new TempFile())</example>
	public class TempFile : IDisposable
	{
		protected string _path;

		public TempFile()
		{
			 _path = System.IO.Path.GetTempFileName();
		}

		public TempFile(bool dontMakeMeAFileAndDontSetPath)
		{
			if(!dontMakeMeAFileAndDontSetPath)
			{
				_path = System.IO.Path.GetTempFileName();
			}
		}

		public TempFile(string contents)
			: this()
		{
			File.WriteAllText(_path, contents);
		}

		public TempFile(string contents, Encoding encoding)
			: this()
		{
			File.WriteAllText(_path, contents, encoding);
		}

		public TempFile(string[] contentLines)
			: this()
		{
			File.WriteAllLines(_path, contentLines);
		}

		public string Path
		{
			get { return _path; }
		}
		public void Dispose()
		{
			File.Delete(_path);
		}

		public static TempFile CopyOf(string pathToExistingFile)
		{
			TempFile t = new TempFile();
			File.Copy(pathToExistingFile, t.Path, true);
			return t;
		}

		public TempFile(string existingPath, bool dummy)
		{
			_path = existingPath;
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
			TempFile t = new TempFile();
			File.Delete(t.Path);
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
				throw new ArgumentException(FileRelatedStrings.kFilenameIsEmptyString, "filename");
			filename = filename.Trim();
			if (filename == string.Empty)
				throw new ArgumentException(FileRelatedStrings.kFilenameIsOnlyWhitespace, "filename");

			var pathname = System.IO.Path.Combine(System.IO.Path.GetTempPath(), filename);
			File.Create(pathname).Close();
			return TrackExisting(pathname);
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
			_path = path;
		}
	}
}
