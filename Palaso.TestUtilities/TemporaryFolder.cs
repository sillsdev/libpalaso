using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Palaso.TestUtilities
{
	public class TempLiftFile : TempFile
	{
		public TempLiftFile(string xmlOfEntries)
			: this(xmlOfEntries, /*LiftIO.Validation.Validator.LiftVersion*/ "0.12")
		{
		}
		public TempLiftFile(string xmlOfEntries, string claimedLiftVersion)
			: this(null, xmlOfEntries, claimedLiftVersion)
		{
		}

		public TempLiftFile(TemporaryFolder parentFolder, string xmlOfEntries, string claimedLiftVersion)
			: base(false)
		{
			if (parentFolder != null)
			{
				_path = parentFolder.GetPathForNewTempFile(false) + ".lift";
			}
			else
			{
				_path = System.IO.Path.GetRandomFileName() + ".lift";
			}

			string liftContents = string.Format("<?xml version='1.0' encoding='utf-8'?><lift version='{0}'>{1}</lift>", claimedLiftVersion, xmlOfEntries);
			File.WriteAllText(_path, liftContents);
		}

		public TempLiftFile(string fileName, TemporaryFolder parentFolder, string xmlOfEntries, string claimedLiftVersion)
			: base(false)
		{
			_path = parentFolder.Combine(fileName);

			string liftContents = string.Format("<?xml version='1.0' encoding='utf-8'?><lift version='{0}'>{1}</lift>", claimedLiftVersion, xmlOfEntries);
			File.WriteAllText(_path, liftContents);
		}
		private TempLiftFile()
		{
		}

		/// <summary>
		/// Create a TempLiftFile based on a pre-existing file, which will be deleted when this is disposed.
		/// </summary>
		public static TempLiftFile TrackExisting(string path)
		{
			Debug.Assert(File.Exists(path));
			TempLiftFile t = new TempLiftFile();
			t._path = path;
			return t;
		}

	}

	/// <summary>
	/// This is useful for unit tests.  When it is disposed, it will delete the file.
	/// </summary>
	/// <example>using(f = new TemporaryFile(){}</example>
	public class TempFile : IDisposable
	{
		protected string _path;

		public TempFile()
		{
			_path = System.IO.Path.GetTempFileName();
		}

		internal TempFile(bool dontMakeMeAFile)
		{
		}

		/// <summary>
		/// Create a tempfile within the given parent folder
		/// </summary>
		public TempFile(TemporaryFolder parentFolder)
		{
			if (parentFolder != null)
			{
				_path = parentFolder.GetPathForNewTempFile(true);
			}
			else
			{
				_path = System.IO.Path.GetTempFileName();
			}

		}


		public TempFile(string contents)
			: this()
		{
			File.WriteAllText(_path, contents);
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

		private TempFile(string existingPath, bool dummy)
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

		public static TempFile CreateXmlFileWithContents(string fileName, TemporaryFolder folder, string xmlBody)
		{
			string path = folder.Combine(fileName);
			using (XmlWriter x = XmlWriter.Create(path))
			{
				x.WriteStartDocument();
				x.WriteRaw(xmlBody);
			}
			return new TempFile(path, true);
		}

		/// <summary>
		/// Use this one when it's important to have a certain file extension
		/// </summary>
		/// <param name="extension">with or with out '.', will work the same</param>
		public static TempFile WithExtension(string extension)
		{
			extension = extension.TrimStart('.');
			var path = System.IO.Path.GetRandomFileName() + "."+extension;
			File.Create(path).Close();
			return TempFile.TrackExisting(path);
		}

		/// <summary>
		/// Used to make a real file out of a resource for the purpose of testing
		/// </summary>
		/// <param name="resource">e.g., an audio resource</param>
		/// <param name="extension">with or with out '.', will work the same</param>
		public static TempFile FromResource(Stream resource, string extension)
		{
			var f = TempFile.WithExtension(extension);
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
			var f = TempFile.WithExtension(extension);
			File.WriteAllBytes(f.Path, resource);
			return f;
		}

		/// <summary>
		/// Used to move a file to a new path
		/// </summary>
		/// <param name="resource">e.g., a video resource</param>
		/// <param name="extension">with or with out '.', will work the same</param>
		public void MoveTo(string path)
		{
			File.Move(Path, path);
			_path = path;
		}
	}

	/// <summary>
	/// This is useful for unit tests.  When it is disposed, it works hard to empty and remove the folder.
	/// </summary>
	/// <example>using(f = new TemporaryFolder("My Export Tests"){}</example>
	public class TemporaryFolder : IDisposable
	{
		private string _path;


		/// <summary>
		/// Create a TemporaryFolder based on a pre-existing directory, which will be deleted when this is disposed.
		/// </summary>
		static public TemporaryFolder TrackExisting(string path)
		{
			Debug.Assert(Directory.Exists(path));
			TemporaryFolder f = new TemporaryFolder();
			f._path = path;
			return f;
		}

		[Obsolete("Go ahead and give it a name related to the test.  Makes it easier to track down problems.")]
		public TemporaryFolder()
			: this("unnamedTestFolder")
		{
		}


		public TemporaryFolder(string name)
		{
			_path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), name);
			if (Directory.Exists(_path))
			{
				TestUtilities.DeleteFolderThatMayBeInUse(_path);
			}
			Directory.CreateDirectory(_path);
		}

		public TemporaryFolder(TemporaryFolder parent, string name)
		{
			_path = parent.Combine(name);
			if (Directory.Exists(_path))
			{
				TestUtilities.DeleteFolderThatMayBeInUse(_path);
			}
			Directory.CreateDirectory(_path);
		}

		[Obsolete("Path is preferred")]
		public string FolderPath
		{
			get { return _path; }
		}

		/// <summary>
		/// Same as FolderPath, but I repent of that poor name
		/// </summary>
		public string Path
		{
			get { return _path; }
		}


		public void Dispose()
		{
			TestUtilities.DeleteFolderThatMayBeInUse(_path);
		}

		[Obsolete("It's better to wrap the use of this in a using() so that it is automatically cleaned up, even if a test fails.")]
		public void Delete()
		{
			TestUtilities.DeleteFolderThatMayBeInUse(_path);
		}

		public string GetPathForNewTempFile(bool doCreateTheFile)
		{
			string s = System.IO.Path.GetRandomFileName();
			s = System.IO.Path.Combine(_path, s);
			if (doCreateTheFile)
			{
				File.Create(s).Close();
			}
			return s;
		}

		public TempFile GetNewTempFile(bool doCreateTheFile)
		{
			string s = System.IO.Path.GetRandomFileName();
			s = System.IO.Path.Combine(_path, s);
			if (doCreateTheFile)
			{
				File.Create(s).Close();
			}
			return TempFile.TrackExisting(s);
		}

		[Obsolete("It's better to use the explict GetNewTempFile, which makes you say if you want the file to be created or not, and give you back a whole TempFile class, which is itself IDisposable.")]
		public string GetTemporaryFile()
		{
			return GetTemporaryFile(System.IO.Path.GetRandomFileName());
		}

		[Obsolete("It's better to use the explict GetNewTempFile, which makes you say if you want the file to be created or not, and give you back a whole TempFile class, which is itself IDisposable.")]
		public string GetTemporaryFile(string name)
		{
			string s = System.IO.Path.Combine(_path, name);
			File.Create(s).Close();
			return s;
		}


		/// <summary>
		/// Similar to Path.Combine, but you don't have to specify the location of the temporaryfolder itself, and you can add multiple parts to combine.
		/// </summary>
		/// <example> string path = t.Combine("stuff", "toys", "ball.txt")</example>
		public string Combine(params string[] partsOfThePath)
		{
			string result = _path;
			foreach (var s in partsOfThePath)
			{
				result = System.IO.Path.Combine(result, s);
			}
			return result;
		}
	}
}