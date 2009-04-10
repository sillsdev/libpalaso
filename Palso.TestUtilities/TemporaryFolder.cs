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
		public static TempLiftFile TrackExisting(string path)
		{
			Debug.Assert(File.Exists(path));
			TempLiftFile t = new TempLiftFile();
			t._path = path;
			return t;
		}

	}

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


		//        public static TempFile TrackExisting(string path)
		//        {
		//            return new TempFile(path, false);
		//        }
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
	}

	public class TemporaryFolder : IDisposable
	{
		private string _path;


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

		public string FolderPath
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


		public string Combine(string innerFileName)
		{
			return System.IO.Path.Combine(_path, innerFileName);
		}
	}
}