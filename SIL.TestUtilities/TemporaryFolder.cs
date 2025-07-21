using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using NUnit.Framework;
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
			: base(true) // True means "I'll set the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
			if (parentFolder != null)
			{
				Path = parentFolder.GetPathForNewTempFile(false) + ".lift";
			}
			else
			{
				Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName() + ".lift");
			}

			string liftContents =
				$"<?xml version='1.0' encoding='utf-8'?><lift version='{claimedLiftVersion}'>{xmlOfEntries}</lift>";
			RobustFile.WriteAllText(Path, liftContents);
		}

		public TempLiftFile(string fileName, TemporaryFolder parentFolder, string xmlOfEntries, string claimedLiftVersion)
			: base(true) // True means "I'll set the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
			Path = parentFolder.Combine(fileName);

			string liftContents =
				$"<?xml version='1.0' encoding='utf-8'?><lift version='{claimedLiftVersion}'>{xmlOfEntries}</lift>";
			RobustFile.WriteAllText(Path, liftContents);
		}

		private TempLiftFile()
			: base(true) // True means "I'll set the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
		}

		/// <summary>
		/// Create a TempLiftFile based on a pre-existing file, which will be deleted when this is disposed.
		/// </summary>
		[PublicAPI]
		public new static TempLiftFile TrackExisting(string path)
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
		/// Create a temp file within the given parent folder
		/// </summary>
		public TempFileFromFolder(TemporaryFolder parentFolder)
			: base(true) // True means "I'll set the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
			Path = parentFolder != null ? parentFolder.GetPathForNewTempFile(true) : System.IO.Path.GetTempFileName();
		}

		public TempFileFromFolder(TemporaryFolder parentFolder, string name, string contents)
			: base(true) // True means "I'll set the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
			Path = parentFolder.Combine(name);
			RobustFile.WriteAllText(Path, contents);
		}

		[PublicAPI]
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
		public static TemporaryFolder TrackExisting(string path)
		{
			Debug.Assert(Directory.Exists(path), @"TrackExisting given non-existent folder to track.");
			var f = new TemporaryFolder(false);
			f._path = path;
			return f;
		}

		private static string SystemTempPath => System.IO.Path.GetTempPath();
		
		/// <summary>
		/// Private constructor that doesn't create a file. Used when tracking a pre-existing
		/// directory.
		/// </summary>
		private TemporaryFolder(bool ignored)
		{
		}

		public TemporaryFolder(string name)
		{
			_path = System.IO.Path.Combine(SystemTempPath, name);
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


		/// <summary>
		/// Create a TemporaryFolder with a unique name based on the given test context.
		/// </summary>
		public static TemporaryFolder Create(TestContext testContext)
		{
			string methodName = testContext.Test.MethodName;
			string pid = Process.GetCurrentProcess().Id.ToString();
			string guid = Guid.NewGuid().ToString();

			// For readability, we prefer the test name, but if it is too long, we use the test ID.
			string folderName = $"test-{methodName}-{pid}-{guid}";
			if (System.IO.Path.Combine(SystemTempPath, folderName).Length > 200)
				folderName = $"test-{testContext.Test.ID}-{pid}-{guid}";

			return new TemporaryFolder(folderName);
		}

		/// <summary>
		/// Full path of the temp folder.
		/// </summary>
		public string Path => _path;

		public void Dispose()
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

		/// <summary>
		/// Similar to <see cref="System.IO.Path.Combine(string[])"/>, but you don't have to
		/// specify the location of the temporary folder itself.
		/// </summary>
		/// <example> string path = t.Combine("stuff", "toys", "ball.txt")</example>
		public string Combine(params string[] partsOfThePath)
		{
			return partsOfThePath.Aggregate(_path, System.IO.Path.Combine);
		}
	}
}