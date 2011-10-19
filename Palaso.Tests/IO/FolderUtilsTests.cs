using System.IO;
using System.Security.AccessControl;
using NUnit.Framework;
using Palaso.IO;
using Palaso.TestUtilities;

namespace Palaso.Tests.IO
{
	[TestFixture]
	public class FolderUtilsTests
	{
		private string _srcFolder;
		private string _dstFolder;

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void TestSetup()
		{
			Reporting.ErrorReport.IsOkToInteractWithUser = false;
			_srcFolder = Path.Combine(Path.GetTempPath(), "~!source");
			_dstFolder = Path.Combine(Path.GetTempPath(), "~!destination");
			Directory.CreateDirectory(_srcFolder);
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TestTearDown()
		{
			try
			{
				if (Directory.Exists(_srcFolder))
					Directory.Delete(_srcFolder, true);
			}
			catch { }

			try
			{
				if (Directory.Exists(_dstFolder))
					Directory.Delete(_dstFolder, true);
			}
			catch { }
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_SourceDoesNotExist_ReturnsFalse()
		{
			using (new Reporting.ErrorReport.NonFatalErrorReportExpected())
				Assert.IsFalse(DirectoryUtilities.CopyDirectoryContents("sblah", "dblah"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_DestinationFolderDoesNotExist_CreatesItAndReturnsTrue()
		{
			Assert.IsFalse(Directory.Exists(_dstFolder));
			Assert.IsTrue(DirectoryUtilities.CopyDirectoryContents(_srcFolder, _dstFolder));
			Assert.IsTrue(Directory.Exists(_dstFolder));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_SourceContainsFilesNoSubFolders_CopiesThemAndSucceeds()
		{
			File.CreateText(Path.Combine(_srcFolder, "file1.txt")).Close();
			File.CreateText(Path.Combine(_srcFolder, "file2.txt")).Close();
			Assert.IsTrue(DirectoryUtilities.CopyDirectoryContents(_srcFolder, _dstFolder));
			Assert.IsTrue(File.Exists(Path.Combine(_dstFolder, "file1.txt")));
			Assert.IsTrue(File.Exists(Path.Combine(_dstFolder, "file2.txt")));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
#if MONO
		[Ignore("This test won't fail as expected on Linux")]
#endif
		public void CopyFolder_SourceContainsLockedFile_ReturnsFalse()
		{
			using (new Reporting.ErrorReport.NonFatalErrorReportExpected())
			using (var fs = File.Open(Path.Combine(_srcFolder, "file1.txt"), FileMode.Append))
			{
				Assert.IsFalse(DirectoryUtilities.CopyDirectoryContents(_srcFolder, _dstFolder));
				fs.Close();
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
#if MONO
		[Ignore("This test won't fail as expected on Linux")]
#endif
		public void CopyFolder_CopyFails_DestinationFolderNotLeftBehind()
		{
			using (new Reporting.ErrorReport.NonFatalErrorReportExpected())
			using (var fs = File.Open(Path.Combine(_srcFolder, "file1.txt"), FileMode.Append))
			{
				Assert.IsFalse(DirectoryUtilities.CopyDirectoryContents(_srcFolder, _dstFolder));
				Assert.IsFalse(Directory.Exists(_dstFolder));
				fs.Close();
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_SourceContainsEmptySubFolders_CopiesThem()
		{
			Directory.CreateDirectory(Path.Combine(_srcFolder, "subfolder1"));
			Directory.CreateDirectory(Path.Combine(_srcFolder, "subfolder2"));
			Assert.IsTrue(DirectoryUtilities.CopyDirectoryContents(_srcFolder, _dstFolder));
			Assert.IsTrue(Directory.Exists(Path.Combine(_dstFolder, "subfolder1")));
			Assert.IsTrue(Directory.Exists(Path.Combine(_dstFolder, "subfolder2")));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_SourceContainsSubFolderWithFiles_CopiesThem()
		{
			var subfolder = Path.Combine(_srcFolder, "subfolder");
			Directory.CreateDirectory(subfolder);
			File.CreateText(Path.Combine(subfolder, "file1.txt")).Close();
			Assert.IsTrue(DirectoryUtilities.CopyDirectoryContents(_srcFolder, _dstFolder));

			subfolder = Path.Combine(_dstFolder, "subfolder");
			Assert.IsTrue(File.Exists(Path.Combine(subfolder, "file1.txt")));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolderToTempFolder_SourceFolderExists_ReturnsCorrectFolderPath()
		{
			var returnPath = DirectoryUtilities.CopyDirectoryToTempDirectory(_srcFolder);
			Assert.IsNotNull(returnPath);
			var foldername = Path.GetFileName(_srcFolder);
			Assert.AreEqual(Path.Combine(Path.GetTempPath(), foldername), returnPath);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolderToTempFolder_SourceFolderExists_MakesCopyInTempFolder()
		{
			var returnPath = DirectoryUtilities.CopyDirectoryToTempDirectory(_srcFolder);
			Assert.IsNotNull(returnPath);
			Assert.IsTrue(Directory.Exists(returnPath));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_DestinationFolderDoesNotExist_ReturnsFalse()
		{
			using (new Reporting.ErrorReport.NonFatalErrorReportExpected())
				Assert.IsFalse(DirectoryUtilities.CopyDirectory(_srcFolder, _dstFolder));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolder_SourceFolderExists_MakesCopyOfFolderWithSameName()
		{
			Directory.CreateDirectory(_dstFolder);
			Assert.IsTrue(DirectoryUtilities.CopyDirectory(_srcFolder, _dstFolder));
			var foldername = Path.GetFileName(_srcFolder);
			Assert.IsTrue(Directory.Exists(Path.Combine(_dstFolder, foldername)));
		}
		[Test]
		[Platform(Exclude="Unix")]
		public void SafeFoldersOmitSystemAndHiddenFolders()
		{
			using (var tempDir = new TemporaryFolder("TempRootDir"))
			{
				var sysDir = Directory.CreateDirectory(Path.Combine(tempDir.Path, "SysDir"));
				sysDir.Attributes |= FileAttributes.System;
				var hiddenDir = Directory.CreateDirectory(Path.Combine(tempDir.Path, "HiddenDir"));
				hiddenDir.Attributes |= FileAttributes.Hidden;
				var hiddenSysDir = Directory.CreateDirectory(Path.Combine(tempDir.Path, "HiddenSysDir"));
				hiddenSysDir.Attributes |= FileAttributes.System;
				hiddenSysDir.Attributes |= FileAttributes.Hidden;
				var ordinaryDir = Directory.CreateDirectory(Path.Combine(tempDir.Path, "OrdinaryDir"));

				Assert.AreEqual(4, Directory.GetDirectories(tempDir.Path).Length);

				var safeDirectories = DirectoryUtilities.GetSafeDirectories(tempDir.Path);

				Assert.AreEqual(1, safeDirectories.Length);
				Assert.AreEqual(ordinaryDir.FullName, safeDirectories[0]);
			}
		}
	}
}
