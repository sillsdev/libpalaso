using System;
using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.Reporting;
using SIL.TestUtilities;

namespace SIL.Tests.IO
{
	[TestFixture]
	public class DirectoryUtilitiesTests
	{
		private string _srcFolder;
		private string _dstFolder;

		// 02 SEP 2013, Phil Hopper: set correct directory separator for OS
		private readonly string _directorySeparator = new String(Path.DirectorySeparatorChar, 1);

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void TestSetup()
		{
			ErrorReport.IsOkToInteractWithUser = false;
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
			using (new ErrorReport.NonFatalErrorReportExpected())
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
		[Platform(Exclude = "Linux", Reason = "This test won't fail as expected on Linux")]
		public void CopyFolder_SourceContainsLockedFile_ReturnsFalse()
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
		public void CopyFolder_CopyFails_DestinationFolderNotLeftBehind()
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
			string srcFolder2 = Path.Combine(_srcFolder, "copyFrom");
			Directory.CreateDirectory(srcFolder2);

			try
			{
				var returnPath = DirectoryUtilities.CopyDirectoryToTempDirectory(srcFolder2);
				Assert.IsNotNull(returnPath);
				Assert.IsTrue(Directory.Exists(returnPath));
				var foldername = Path.GetFileName(srcFolder2);
				Assert.AreEqual(Path.Combine(Path.GetTempPath(), foldername), returnPath);
			}
			finally
			{
				if (Directory.Exists(srcFolder2))
					Directory.Delete(srcFolder2, true);
			}
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CopyFolderToTempFolder_SourceFolderExists_MakesCopyInTempFolder()
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
		public void CopyFolder_DestinationFolderDoesNotExist_ReturnsFalse()
		{
			using (new ErrorReport.NonFatalErrorReportExpected())
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

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreDirectoriesEquivalent_Identical_ReturnsTrue()
		{
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(_srcFolder, _srcFolder));
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(_dstFolder, _dstFolder));
			const string nonExsistentFolderPath = @"c:\blah\BLAH\weird\..\funky\WhatEVer";
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(nonExsistentFolderPath, nonExsistentFolderPath));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreDirectoriesEquivalent_TotallyDifferent_ReturnsFalse()
		{
			Assert.IsFalse(DirectoryUtilities.AreDirectoriesEquivalent(_srcFolder, _dstFolder));
			const string nonExsistentFolderPath = @"c:\blah\BLAH\weird\..\funky\WhatEVer";
			Assert.IsFalse(DirectoryUtilities.AreDirectoriesEquivalent(_srcFolder, nonExsistentFolderPath));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreDirectoriesEquivalent_DifferByTrailingBackslash_ReturnsTrue()
		{
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp", @"C:\temp\"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Exclude = "Linux", Reason = "This test is not valid on a Linux file system")]
		public void AreDirectoriesEquivalent_DifferByDirectionOfSlash_ReturnsTrue()
		{
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp", @"C:/temp"));

		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreDirectoriesEquivalent_DifferByTrailingCurrentDirectoryDot_ReturnsTrue()
		{
			// 02 SEP 2013, Phil Hopper: set correct directory separator for OS
			//Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp", @"C:\temp\."));
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(
				"C:" + _directorySeparator + "temp",
				"C:" + _directorySeparator + "temp" + _directorySeparator + "."));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Exclude = "Linux", Reason = "This test tests Windows behaviour")]
		public void AreDirectoriesEquivalent_DifferByTrailingDot_Windows()
		{
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp.", @"C:\temp"));
		}

		[Test]
		[Platform(Include = "Linux", Reason = "This test tests Linux behaviour")]
		public void AreDirectoriesEquivalent_DifferByTrailingDot_Linux()
		{
			Assert.IsFalse(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp.", @"C:\temp"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Exclude = "Linux", Reason = "This test tests Windows behaviour")]
		public void AreDirectoriesEquivalent_DifferByTrailingDotsAndBackslash_Windows()
		{
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp...\", @"C:\temp"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Include = "Linux", Reason = "This test tests Linux behaviour")]
		public void AreDirectoriesEquivalent_DifferByTrailingDotsAndBackslash_Linux()
		{
			Assert.IsFalse(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp...\", @"C:\temp"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreDirectoriesEquivalent_OnePathContainsBacktrackingToParentFolders_ReturnsTrue()
		{
			// 02 SEP 2013, Phil Hopper: set correct directory separator for OS
			//Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp", @"C:\temp\x\..\..\temp\."));
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(
				"C:" + _directorySeparator + "temp",
				"C:" + _directorySeparator + "temp" + _directorySeparator + "x" + _directorySeparator + ".." + _directorySeparator + ".." + _directorySeparator + "temp" + _directorySeparator + "."));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Exclude = "Linux", Reason = "This test tests Windows behaviour")]
		public void AreDirectoriesEquivalent_DifferentCase_Windows()
		{
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp", @"c:\TEMP\x\..\..\tEmp\."));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Include = "Linux", Reason = "This test tests Linux behaviour")]
		public void AreDirectoriesEquivalent_DifferentCase_ResultDependsOnOperatingSystem()
		{
			Assert.IsFalse(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp", @"c:\TEMP\x\..\..\tEmp\."));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreDirectoriesEquivalent_AbsolutePathAndRelativePathToDifferentFolder_ReturnsFalse()
		{
			Directory.SetCurrentDirectory(_srcFolder);
			Assert.IsFalse(DirectoryUtilities.AreDirectoriesEquivalent(_srcFolder, "~!source"));

			// Change the current directory because this test is failing on mono sometimes
			// when TestTearDown() deletes _srcFolder, which is the current directory.
			Directory.SetCurrentDirectory(Path.GetTempPath());
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreDirectoriesEquivalent_AbsolutePathAndRelativePathToSameFolder_ReturnsTrue()
		{
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(".", Directory.GetCurrentDirectory()));

			Directory.SetCurrentDirectory(Path.GetTempPath());
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(_srcFolder, "~!source"));
			string[] logicalDrives;
			try
			{
				logicalDrives = Directory.GetLogicalDrives();
			}
			catch
			{
				// Ignore -- can't test this on this system
				return;
			}
			foreach (string logicalDrive in logicalDrives)
			{
				string[] directories;
				try
				{
					directories = Directory.GetDirectories(logicalDrive);
				}
				catch
				{
					continue; // try another drive.
				}

				foreach (string folder in directories)
				{
					try
					{
						Directory.SetCurrentDirectory(Path.Combine(logicalDrive, folder));
					}
					catch
					{
						continue; // try another folder
					}
				}
				// 02 SEP 2013, Phil Hopper: set correct directory separator for OS
				Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(_directorySeparator, logicalDrive));
				Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(_directorySeparator + "temp", logicalDrive + "temp"));
				return; // Found an accessible drive -- no need to try them all.
			}
			Assert.Ignore("Unable to find a drive and folder that could be set as current working directory.");
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Category("KnownMonoIssue")]
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

		[Test]
		public void UniqueFolderPathNotCreated()
		{
			using (var tempDir = new TemporaryFolder("TempRootDir"))
			{
				var targetDir = Path.Combine(tempDir.Path, "ZPI");
				var uniqueFolderPath = DirectoryUtilities.GetUniqueFolderPath(targetDir);
				Assert.IsFalse(Directory.Exists(uniqueFolderPath));
				Assert.AreEqual(targetDir, uniqueFolderPath);
			}
		}

		[Test]
		public void UniqueFolderPathEndingWithDirectorySeparatorNotCreated()
		{
			using (var tempDir = new TemporaryFolder("TempRootDir"))
			{
				var targetDir = Path.Combine(tempDir.Path, "ZPI");
				var uniqueFolderPath = DirectoryUtilities.GetUniqueFolderPath(targetDir + Path.DirectorySeparatorChar);
				Assert.IsFalse(Directory.Exists(uniqueFolderPath));
				Assert.AreEqual(targetDir, uniqueFolderPath);
			}
		}

		[Test]
		public void UniqueFolderPathCreated()
		{
			using (var tempDir = new TemporaryFolder("TempRootDir"))
			{
				var targetDir = Path.Combine(tempDir.Path, "ZPI");
				Directory.CreateDirectory(targetDir);
				var uniqueFolderPath = DirectoryUtilities.GetUniqueFolderPath(targetDir);
				Assert.IsFalse(Directory.Exists(uniqueFolderPath));
				Assert.AreNotEqual(targetDir, uniqueFolderPath);
			}
		}

		[Test]
		public void UniqueFolderPathEndingWithDirectorySeparatorCreated()
		{
			using (var tempDir = new TemporaryFolder("TempRootDir"))
			{
				var targetDir = Path.Combine(tempDir.Path, "ZPI");
				Directory.CreateDirectory(targetDir);
				var uniqueFolderPath = DirectoryUtilities.GetUniqueFolderPath(targetDir + Path.DirectorySeparatorChar);
				Assert.IsFalse(Directory.Exists(uniqueFolderPath));
				Assert.AreNotEqual(targetDir, uniqueFolderPath);
			}
		}

		[Test]
		public void MoveDirectorySafely_SameFileThrows()
		{
			using (var tempFile = new TempFile())
			{
				Assert.That(() => DirectoryUtilities.MoveDirectorySafely(tempFile.Path, tempFile.Path),
					Throws.InstanceOf<IOException>());
			}
		}

		[Test]
		public void MoveDirectorySafely_MoveToExistingFileThrows()
		{
			using (var tempFile = new TempFile())
			using (var existingFile = new TempFile())
			{
				Assert.That(() => DirectoryUtilities.MoveDirectorySafely(tempFile.Path, existingFile.Path),
					Throws.InstanceOf<IOException>());
			}
		}

		[Test]
		public void MoveDirectorySafely_MoveDirToExistingDirThrows()
		{
			using (var tempDir = new TemporaryFolder("TempRootDir"))
			using (var existingDir = new TemporaryFolder("NewTempRootDir"))
			{
				Assert.That(() => DirectoryUtilities.MoveDirectorySafely(tempDir.Path, existingDir.Path),
					Throws.InstanceOf<IOException>());
			}
		}

		[Test]
		public void MoveDirectorySafely_MoveDirToExistingFileThrows()
		{
			using (var tempDir = new TemporaryFolder("TempRootDir"))
			using (var existingFile = new TempFile())
			{
				Assert.That(() => DirectoryUtilities.MoveDirectorySafely(tempDir.Path, existingFile.Path),
					Throws.InstanceOf<IOException>());
			}
		}

		[Test]
		public void MoveDirectorySafely_MoveFileToExistingDirThrows()
		{
			// while this could theoretically work the docs for Directory.Move say that if you
			// specify a file as source than destination also has to be a file.
			using (var tempFile = new TempFile())
			using (var existingDir = new TemporaryFolder("TempRootDir"))
			{
				Assert.That(() => DirectoryUtilities.MoveDirectorySafely(tempFile.Path, existingDir.Path),
					Throws.InstanceOf<IOException>());
			}
		}

		[Test]
		[Platform(Exclude = "Win", Reason="Don't know how to test this on Windows")]
		public void MoveDirectorySafely_MoveDirToDifferentVolume()
		{
			// On Linux, /tmp is typicall a ram disk and therefore a different partition from
			// /var/tmp which is supposed to persist across reboots.
			// On Mac, /tmp isn't usually a ram disk. However, it's possible to create and mount
			// loop filesystems (disk images) without root privileges. So it would be possible
			// to extend this when porting to Mac.
			if (PathUtilities.GetDeviceNumber("/tmp") == PathUtilities.GetDeviceNumber("/var/tmp"))
				Assert.Ignore("For this test /tmp and /var/tmp have to be on different partitions");

			var tempDir = Path.Combine("/tmp", Path.GetRandomFileName());
			Directory.CreateDirectory(tempDir);
			var dirOnDifferentVolume = Path.Combine("/var/tmp", Path.GetRandomFileName());
			Directory.CreateDirectory(dirOnDifferentVolume);
			using (TemporaryFolder.TrackExisting(tempDir))
			using (TemporaryFolder.TrackExisting(dirOnDifferentVolume))
			{
				Assert.That(() => DirectoryUtilities.MoveDirectorySafely(tempDir,
					Path.Combine(dirOnDifferentVolume, "TempDir")),
					Throws.Nothing);
			}
		}

		[Test]
		[Platform(Exclude = "Win", Reason="Don't know how to test this on Windows")]
		public void MoveDirectorySafely_MoveFileToDifferentVolume()
		{
			// On Linux, /tmp is typicall a ram disk and therefore a different partition from
			// /var/tmp which is supposed to persist across reboots.
			// On Mac, /tmp isn't usually a ram disk. However, it's possible to create and mount
			// loop filesystems (disk images) without root privileges. So it would be possible
			// to extend this when porting to Mac.
			if (PathUtilities.GetDeviceNumber("/tmp") == PathUtilities.GetDeviceNumber("/var/tmp"))
				Assert.Ignore("For this test /tmp and /var/tmp have to be on different partitions");

			var tempFile = Path.Combine("/tmp", Path.GetRandomFileName());
			var dirOnDifferentVolume = Path.Combine("/var/tmp", Path.GetRandomFileName());
			Directory.CreateDirectory(dirOnDifferentVolume);
			using (File.Create(tempFile))
			using (TempFile.TrackExisting(tempFile))
			using (TemporaryFolder.TrackExisting(dirOnDifferentVolume))
			{
				var destinationFile = Path.Combine(dirOnDifferentVolume, "TempFile");
				Assert.That(() => DirectoryUtilities.MoveDirectorySafely(tempFile, destinationFile),
					Throws.Nothing);
				Assert.That(File.Exists(destinationFile), Is.True);
				Assert.That(File.Exists(tempFile), Is.False);
			}
		}

		[Test]
		public void MoveDirectorySafely_SourceDirDoesNotExist()
		{
			Assert.That(() => DirectoryUtilities.MoveDirectorySafely(
				Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
				Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())),
				Throws.InstanceOf<DirectoryNotFoundException>());
		}

		[Test]
		public void DirectoryIsEmpty_DirectoryIsEmpty()
		{
			using (var tempDir = new TemporaryFolder("IsEmpty_DirectoryIsEmpty"))
			{
				Assert.IsTrue(DirectoryUtilities.DirectoryIsEmpty(tempDir.Path));
				Assert.IsTrue(DirectoryUtilities.DirectoryIsEmpty(tempDir.Path, true));
			}
		}

		[Test]
		public void DirectoryIsEmpty_DirectoryContainsSubDirectory()
		{
			using (var tempDir = new TemporaryFolder("IsEmpty_DirectoryContainsSubDirectory"))
			{
				Directory.CreateDirectory(Path.Combine(tempDir.Path, "subDirectory"));
				Assert.IsFalse(DirectoryUtilities.DirectoryIsEmpty(tempDir.Path));
				Assert.IsTrue(DirectoryUtilities.DirectoryIsEmpty(tempDir.Path, true));
			}
		}

		[Test]
		public void DirectoryIsEmpty_DirectoryContainsFile()
		{
			using (var tempDir = new TemporaryFolder("IsEmpty_DirectoryContainsFile"))
			{
				var file = tempDir.GetNewTempFile(false);
				File.WriteAllText(file.Path, @"Some test text");
				Assert.IsFalse(DirectoryUtilities.DirectoryIsEmpty(tempDir.Path));
				Assert.IsFalse(DirectoryUtilities.DirectoryIsEmpty(tempDir.Path, true));
			}
		}

		[Test]
		public void DirectoryIsEmpty_SubdirectoryContainsFile()
		{
			using (var tempDir = new TemporaryFolder("IsEmpty_DirectoryContainsFile"))
			{
				var dir = Directory.CreateDirectory(Path.Combine(tempDir.Path, "subDirectory2"));
				var fileName = Path.Combine(dir.FullName, "tempFile.txt");
				File.WriteAllText(fileName, @"Some test text");
				Assert.IsTrue(File.Exists(fileName));
				Assert.IsFalse(DirectoryUtilities.DirectoryIsEmpty(tempDir.Path));
				Assert.IsTrue(DirectoryUtilities.DirectoryIsEmpty(tempDir.Path, true));
				File.Delete(fileName);
			}
		}

		[Test]
		public void DeleteDirectoryRobust_ContainsReadOnlyFile_StillRemoves()
		{
			using (var tempDir = new TemporaryFolder("DeleteDirectoryRobust_ContainsReadOnlyFile_StillRemoves"))
			{
				var fileName = tempDir.Combine("tempFile.txt");
				File.WriteAllText(fileName, @"Some test text");
				new System.IO.FileInfo(fileName).IsReadOnly = true;
				DirectoryUtilities.DeleteDirectoryRobust(tempDir.Path);
				Assert.IsFalse(Directory.Exists(tempDir.Path), "Did not delete directory");
			}
		}

		[Test]
		public void DeleteDirectoryRobust_NoOverrideContainsReadOnlyFile_ReturnsFalse()
		{
			using (var tempDir = new TemporaryFolder("DeleteDirectoryRobust_NoOverrideContainsReadOnlyFile_ReturnsFalse"))
			{
				var fileName = tempDir.Combine("tempFile.txt");
				File.WriteAllText(fileName, @"Some test text");
				new System.IO.FileInfo(fileName).IsReadOnly = true;
				Assert.IsFalse(DirectoryUtilities.DeleteDirectoryRobust(tempDir.Path, overrideReadOnly:false));
				Assert.IsTrue(Directory.Exists(tempDir.Path), "Did not expect it to delete directory because of the readonly file");
			}
		}
	}
}
