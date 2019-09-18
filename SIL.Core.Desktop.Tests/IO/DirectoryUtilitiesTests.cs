// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.Reporting;

namespace SIL.Tests.IO
{
	[TestFixture]
	public class DirectoryUtilitiesTests
	{
		private string _tempFolder;
		private string _srcFolder;
		private string _dstFolder;

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void TestSetup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
			_tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			_srcFolder = Path.Combine(_tempFolder, "~!source");
			_dstFolder = Path.Combine(_tempFolder, "~!destination");
			Directory.CreateDirectory(_srcFolder);
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TestTearDown()
		{
			try
			{
				if (Directory.Exists(_tempFolder))
					Directory.Delete(_tempFolder, true);
			}
			catch
			{
				// ignored
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyDirectoryContents_SourceDoesNotExist_ReturnsFalse()
		{
			using (new ErrorReport.NonFatalErrorReportExpected())
				Assert.IsFalse(DirectoryUtilities.CopyDirectoryContents("sblah", "dblah"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyDirectoryContents_DestinationFolderDoesNotExist_CreatesItAndReturnsTrue()
		{
			Assert.IsFalse(Directory.Exists(_dstFolder));
			Assert.IsTrue(DirectoryUtilities.CopyDirectoryContents(_srcFolder, _dstFolder));
			Assert.IsTrue(Directory.Exists(_dstFolder));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyDirectoryContents_SourceContainsFilesNoSubFolders_CopiesThemAndSucceeds()
		{
			File.CreateText(Path.Combine(_srcFolder, "file1.txt")).Close();
			File.CreateText(Path.Combine(_srcFolder, "file2.txt")).Close();
			Assert.IsTrue(DirectoryUtilities.CopyDirectoryContents(_srcFolder, _dstFolder));
			Assert.IsTrue(File.Exists(Path.Combine(_dstFolder, "file1.txt")));
			Assert.IsTrue(File.Exists(Path.Combine(_dstFolder, "file2.txt")));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Exclude = "Linux", Reason = "This test won't fail as expected on Linux")]
		public void CopyDirectoryContents_SourceContainsLockedFile_ReturnsFalse()
		{
			using (new ErrorReport.NonFatalErrorReportExpected())
			using (var fs = File.Open(Path.Combine(_srcFolder, "file1.txt"), FileMode.Append))
			{
				Assert.IsFalse(DirectoryUtilities.CopyDirectoryContents(_srcFolder, _dstFolder));
				fs.Close();
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Exclude = "Linux", Reason = "This test won't fail as expected on Linux")]
		public void CopyDirectoryContents_CopyFails_DestinationFolderNotLeftBehind()
		{
			using (new ErrorReport.NonFatalErrorReportExpected())
			using (var fs = File.Open(Path.Combine(_srcFolder, "file1.txt"), FileMode.Append))
			{
				Assert.IsFalse(DirectoryUtilities.CopyDirectoryContents(_srcFolder, _dstFolder));
				Assert.IsFalse(Directory.Exists(_dstFolder));
				fs.Close();
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyDirectoryContents_SourceContainsEmptySubFolders_CopiesThem()
		{
			Directory.CreateDirectory(Path.Combine(_srcFolder, "subfolder1"));
			Directory.CreateDirectory(Path.Combine(_srcFolder, "subfolder2"));
			Assert.IsTrue(DirectoryUtilities.CopyDirectoryContents(_srcFolder, _dstFolder));
			Assert.IsTrue(Directory.Exists(Path.Combine(_dstFolder, "subfolder1")));
			Assert.IsTrue(Directory.Exists(Path.Combine(_dstFolder, "subfolder2")));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyDirectoryContents_SourceContainsSubFolderWithFiles_CopiesThem()
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
		public void CopyDirectoryToTempDirectory_SourceFolderExists_ReturnsCorrectFolderPath()
		{
			string srcFolder2 = Path.Combine(_srcFolder, "copyFrom");
			Directory.CreateDirectory(srcFolder2);

			try
			{
				var returnPath = DirectoryUtilities.CopyDirectoryToTempDirectory(srcFolder2);
				Assert.IsNotNull(returnPath);
				Assert.IsTrue(Directory.Exists(returnPath));
				var folderName = Path.GetFileName(srcFolder2);
				Assert.AreEqual(Path.Combine(Path.GetTempPath(), folderName), returnPath);
			}
			finally
			{
				if (Directory.Exists(srcFolder2))
					Directory.Delete(srcFolder2, true);
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyDirectoryToTempDirectory_SourceFolderExists_MakesCopyInTempFolder()
		{
			string srcFolder2 = Path.Combine(_srcFolder, "copyFrom");
			Directory.CreateDirectory(srcFolder2);

			try
			{
				var returnPath = DirectoryUtilities.CopyDirectoryToTempDirectory(srcFolder2);
				Assert.IsNotNull(returnPath);
				Assert.IsTrue(Directory.Exists(returnPath));
			}
			finally
			{
				if (Directory.Exists(srcFolder2))
					Directory.Delete(srcFolder2, true);
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyDirectory_DestinationFolderDoesNotExist_ReturnsFalse()
		{
			using (new ErrorReport.NonFatalErrorReportExpected())
				Assert.IsFalse(DirectoryUtilities.CopyDirectory(_srcFolder, _dstFolder));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyDirectory_SourceFolderExists_MakesCopyOfFolderWithSameName()
		{
			Directory.CreateDirectory(_dstFolder);
			Assert.IsTrue(DirectoryUtilities.CopyDirectory(_srcFolder, _dstFolder));
			var folderName = Path.GetFileName(_srcFolder);
			Assert.IsNotNull(folderName);
			Assert.IsTrue(Directory.Exists(Path.Combine(_dstFolder, folderName)));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void IsDirectoryWritable_WritablePath_True()
		{
			var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			try
			{
				Directory.CreateDirectory(tempDir);

				var writable = DirectoryUtilities.IsDirectoryWritable(tempDir);
				Assert.True(writable);
			}
			finally
			{
				if (Directory.Exists(tempDir))
					Directory.Delete(tempDir, true);
			}
		}

		[Test]
		public void IsDirectoryWritable_NonexistentPath_False()
		{
			const string dir = "/one/two/three";
			var writable = DirectoryUtilities.IsDirectoryWritable(dir);
			Assert.False(writable);
		}
	}
}
