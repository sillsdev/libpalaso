using NUnit.Framework;
using System.IO;
using Palaso.TestUtilities;
using SIL.IO;


namespace Palaso.TestUtilities {
	[TestFixture]
	public class TemporaryFolderTests
	{
		[Test]
		public void Constructor_CreatesTemporarySubDirectory()
		{
			var temporaryFolder = new TemporaryFolder();
			Assert.IsTrue(Directory.Exists(temporaryFolder.FolderPath));
			temporaryFolder.Dispose();
			Assert.IsFalse(Directory.Exists(temporaryFolder.Path));
		}

		[Test]
		public void Constructor_Path_CreatesTemporarySubDirectoryAtPath()
		{
			using (TemporaryFolder temporaryFolder = new TemporaryFolder("foo"))
			{
				Assert.IsTrue(Directory.Exists(temporaryFolder.FolderPath));
			}
		}

		[Test]
		public void Constructor_PathDirectoryName_CreatesTemporarySubDirectoryAtPathWithGivenName()
		{
			TemporaryFolder temporaryFolder = new TemporaryFolder("Constructor_PathDirectoryName_CreatesTemporarySubDirectoryAtPathWithGivenName");
			Assert.IsTrue(Directory.Exists(temporaryFolder.FolderPath));
			temporaryFolder.Dispose();
			Assert.IsFalse(Directory.Exists(temporaryFolder.Path));
		}

		[Test]
		public void Constructor_TemporarySubDirectoryAlreadyExistsAndHasFilesInIt_EmptyTheTemporarySubDirectory()
		{
			TemporaryFolder temporaryFolder = new TemporaryFolder("NonStandard");
			string pathToFile = Path.Combine(temporaryFolder.Path, Path.GetRandomFileName());
			FileStream file = File.Create(pathToFile);
			file.Close();
			TemporaryFolder temporaryFolderUsingSameDirectory = new TemporaryFolder("NonStandard");
			Assert.AreEqual(0, Directory.GetFiles(temporaryFolder.Path).Length);
			temporaryFolder.Dispose();
			Assert.IsFalse(Directory.Exists(temporaryFolder.Path));
			Assert.IsFalse(Directory.Exists(temporaryFolderUsingSameDirectory.Path));
		}

		[Test]
		public void GetTemporaryFile_FileExistsInTemporarySubdirectory()
		{
			TemporaryFolder temporaryFolder = new TemporaryFolder();
			string pathToFile = temporaryFolder.GetTemporaryFile();
			Assert.IsTrue(File.Exists(pathToFile));
			temporaryFolder.Dispose();
			Assert.IsFalse(Directory.Exists(temporaryFolder.Path));
		}

		[Test]
		public void GetTemporaryFile_Name_FileWithNameExistsInTemporarySubdirectory()
		{
			TemporaryFolder temporaryFolder = new TemporaryFolder();
			string pathToFile = temporaryFolder.GetTemporaryFile("blah");
			Assert.IsTrue(File.Exists(pathToFile));
			Assert.AreEqual(pathToFile, Path.Combine(temporaryFolder.Path, "blah"));
			temporaryFolder.Dispose();
			Assert.IsFalse(Directory.Exists(temporaryFolder.Path));
		}

		[Test]
		public void GetTemporaryFile_CalledTwice_BothFilesFoundInSameTemporarySubdirectory()
		{
			TemporaryFolder temporaryFolder = new TemporaryFolder();
			temporaryFolder.GetTemporaryFile();
			temporaryFolder.GetTemporaryFile();
			Assert.AreEqual(2, Directory.GetFiles(temporaryFolder.Path).Length);
			temporaryFolder.Dispose();
			Assert.IsFalse(Directory.Exists(temporaryFolder.Path));
		}

		[Test]
		public void Delete_RemovesTemporarySubDirectory()
		{
			TemporaryFolder temporaryFolder = new TemporaryFolder("NonStandard");
			temporaryFolder.Delete();
			Assert.IsFalse(Directory.Exists(temporaryFolder.Path));
		}

		[Test]
		public void Delete_FileInDirectory_RemovesTemporaryDirectory()
		{
			TemporaryFolder temporaryFolder = new TemporaryFolder("NonStandard");
			string pathToFile = Path.Combine(temporaryFolder.FolderPath, Path.GetRandomFileName());
			FileStream file = File.Create(pathToFile);
			file.Close();
			temporaryFolder.Dispose();
			Assert.IsFalse(Directory.Exists(temporaryFolder.Path));
		}

		[Test]
		public void Delete_SubDirectoriesInDirectory_RemovesTemporaryDirectory()
		{
			TemporaryFolder temporaryFolder = new TemporaryFolder("NonStandard");
			string pathToSubdirectory = Path.Combine(temporaryFolder.Path, Path.GetRandomFileName());
			Directory.CreateDirectory(pathToSubdirectory);
			temporaryFolder.Dispose();
			Assert.IsFalse(Directory.Exists(temporaryFolder.Path));
		}

		[Test]
		public void MoveTo_Path_PathPropertyIsSet()
		{
			using (TempFile temp = new TempFile())
			{
				string newPath = Path.Combine(Path.GetTempPath(), "TempFile.tmp");
				string oldPath = temp.Path;

				Assert.IsTrue(File.Exists(oldPath));
				Assert.IsFalse(File.Exists(newPath));
				temp.MoveTo(newPath);
				Assert.IsFalse(File.Exists(oldPath));
				Assert.IsTrue(File.Exists(newPath));
				Assert.AreEqual(newPath, temp.Path);
			}
		}

		[Test]
		public void TrackExisting()
		{
			using (var tempFolder = new TemporaryFolder())
			{
				using (var sut = TemporaryFolder.TrackExisting(tempFolder.Path))
				{
				}
				Assert.IsFalse(Directory.Exists(tempFolder.Path));
			}
		}
	}
}