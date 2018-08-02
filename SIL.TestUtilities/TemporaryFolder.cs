using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using SIL.IO;
using SIL.Xml;

namespace SIL.TestUtilities
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
			: base(true) // True means "I'll set the the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
			if (parentFolder != null)
			{
				Path = parentFolder.GetPathForNewTempFile(false) + ".lift";
			}
			else
			{
				Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName() + ".lift");
			}

			string liftContents = string.Format("<?xml version='1.0' encoding='utf-8'?><lift version='{0}'>{1}</lift>", claimedLiftVersion, xmlOfEntries);
			RobustFile.WriteAllText(Path, liftContents);
		}

		public TempLiftFile(string fileName, TemporaryFolder parentFolder, string xmlOfEntries, string claimedLiftVersion)
			: base(true) // True means "I'll set the the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
			Path = parentFolder.Combine(fileName);

			string liftContents = string.Format("<?xml version='1.0' encoding='utf-8'?><lift version='{0}'>{1}</lift>", claimedLiftVersion, xmlOfEntries);
			RobustFile.WriteAllText(Path, liftContents);
		}

		private TempLiftFile()
			: base(true) // True means "I'll set the the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
		}

		/// <summary>
		/// Create a TempLiftFile based on a pre-existing file, which will be deleted when this is disposed.
		/// </summary>
		public static TempLiftFile TrackExisting(string path)
		{
			Debug.Assert(File.Exists(path));
			TempLiftFile t = new TempLiftFile();
			t.Path = path;
			return t;
		}

	}

	/// <summary>
	/// This is useful for unit tests.  When it is disposed, it will delete the file.
	/// </summary>
	/// <example>using(f = new TemporaryFile(){}</example>
	public class TempFileFromFolder : TempFile
	{
		/// <summary>
		/// Create a tempfile within the given parent folder
		/// </summary>
		public TempFileFromFolder(TemporaryFolder parentFolder)
			: base(true) // True means "I'll set the the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
			Path = parentFolder != null ? parentFolder.GetPathForNewTempFile(true) : System.IO.Path.GetTempFileName();
		}

		public TempFileFromFolder(TemporaryFolder parentFolder, string name, string contents)
			: base(true) // True means "I'll set the the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
			Path = parentFolder.Combine(name);
			RobustFile.WriteAllText(Path, contents);
		}

		public static TempFile CreateXmlFileWithContents(string fileName, TemporaryFolder folder, string xmlBody)
		{
			string path = folder.Combine(fileName);
			using (var reader = XmlReader.Create(new StringReader(xmlBody)))
			{
				using (var writer = XmlWriter.Create(path, CanonicalXmlSettings.CreateXmlWriterSettings()))
				{
					writer.WriteStartDocument();
					writer.WriteNode(reader, false);
				}
			}
			return new TempFile(path, true);
		}

		public static TempFile CreateAt(string path, string contents)
		{
			File.WriteAllText(path, contents);
			return TrackExisting(path);
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
			Debug.Assert(Directory.Exists(path), @"TrackExisting given non existant folder to track.");
			var f = new TemporaryFolder(false);
			f._path = path;
			return f;
		}

		[Obsolete("Go ahead and give it a name related to the test.  Makes it easier to track down problems.")]
		public TemporaryFolder()
			: this(System.IO.Path.GetRandomFileName())
		{
		}

		/// <summary>
		/// Private constructor that doesn't create a file. Used when tracking a pre-existing
		/// directory.
		/// </summary>
		private TemporaryFolder(bool ignored)
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