// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;

namespace SIL.Tests.IO
{
	[TestFixture]
	public class DirectoryHelperTests
	{
		private string _srcFolder;
		private string _dstFolder;

		// 02 SEP 2013, Phil Hopper: set correct directory separator for OS
		private readonly string _directorySeparator = new String(Path.DirectorySeparatorChar, 1);

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void TestSetup()
		{
			string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			_srcFolder = Path.Combine(tempDir, "~!source");
			_dstFolder = Path.Combine(tempDir, "~!destination");
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
		public void AreEquivalent_Identical_ReturnsTrue()
		{
			Assert.IsTrue(DirectoryHelper.AreEquivalent(_srcFolder, _srcFolder));
			Assert.IsTrue(DirectoryHelper.AreEquivalent(_dstFolder, _dstFolder));
			const string nonExistentFolderPath = @"c:\blah\BLAH\weird\..\funky\WhatEVer";
			Assert.IsTrue(DirectoryHelper.AreEquivalent(nonExistentFolderPath, nonExistentFolderPath));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreEquivalent_TotallyDifferent_ReturnsFalse()
		{
			Assert.IsFalse(DirectoryHelper.AreEquivalent(_srcFolder, _dstFolder));
			const string nonExistentFolderPath = @"c:\blah\BLAH\weird\..\funky\WhatEVer";
			Assert.IsFalse(DirectoryHelper.AreEquivalent(_srcFolder, nonExistentFolderPath));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreEquivalent_DifferByTrailingBackslash_ReturnsTrue()
		{
			Assert.IsTrue(DirectoryHelper.AreEquivalent(@"C:\temp", @"C:\temp\"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Exclude = "Linux", Reason = "This test is not valid on a Linux file system")]
		public void AreEquivalent_DifferByDirectionOfSlash_ReturnsTrue()
		{
			Assert.IsTrue(DirectoryHelper.AreEquivalent(@"C:\temp", @"C:/temp"));

		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreEquivalent_DifferByTrailingCurrentDirectoryDot_ReturnsTrue()
		{
			// 02 SEP 2013, Phil Hopper: set correct directory separator for OS
			//Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp", @"C:\temp\."));
			Assert.IsTrue(DirectoryHelper.AreEquivalent(
				"C:" + _directorySeparator + "temp",
				"C:" + _directorySeparator + "temp" + _directorySeparator + "."));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Exclude = "Linux", Reason = "This test tests Windows behaviour")]
		public void AreEquivalent_DifferByTrailingDot_Windows()
		{
			Assert.IsTrue(DirectoryHelper.AreEquivalent(@"C:\temp.", @"C:\temp"));
		}

		[Test]
		[Platform(Include = "Linux", Reason = "This test tests Linux behaviour")]
		public void AreEquivalent_DifferByTrailingDot_Linux()
		{
			Assert.IsFalse(DirectoryHelper.AreEquivalent(@"C:\temp.", @"C:\temp"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Exclude = "Linux", Reason = "This test tests Windows behaviour")]
		public void AreEquivalent_DifferByTrailingDotsAndBackslash_Windows()
		{
			Assert.IsTrue(DirectoryHelper.AreEquivalent(@"C:\temp...\", @"C:\temp"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Include = "Linux", Reason = "This test tests Linux behaviour")]
		public void AreEquivalent_DifferByTrailingDotsAndBackslash_Linux()
		{
			Assert.IsFalse(DirectoryHelper.AreEquivalent(@"C:\temp...\", @"C:\temp"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreEquivalent_OnePathContainsBacktrackingToParentFolders_ReturnsTrue()
		{
			// 02 SEP 2013, Phil Hopper: set correct directory separator for OS
			//Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp", @"C:\temp\x\..\..\temp\."));
			Assert.IsTrue(DirectoryHelper.AreEquivalent(
				"C:" + _directorySeparator + "temp",
				"C:" + _directorySeparator + "temp" + _directorySeparator + "x" + _directorySeparator + ".." + _directorySeparator + ".." + _directorySeparator + "temp" + _directorySeparator + "."));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Exclude = "Linux", Reason = "This test tests Windows behaviour")]
		public void AreEquivalent_DifferentCase_Windows()
		{
			Assert.IsTrue(DirectoryHelper.AreEquivalent(@"C:\temp", @"c:\TEMP\x\..\..\tEmp\."));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		[Platform(Include = "Linux", Reason = "This test tests Linux behaviour")]
		public void AreEquivalent_DifferentCase_ResultDependsOnOperatingSystem()
		{
			Assert.IsFalse(DirectoryHelper.AreEquivalent(@"C:\temp", @"c:\TEMP\x\..\..\tEmp\."));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreEquivalent_ForwardSlashAtEnd_ReturnsTrue()
		{
			Assert.IsTrue(DirectoryHelper.AreEquivalent("c:/temp/", "c:/temp"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreEquivalent_AbsolutePathAndRelativePathToDifferentFolder_ReturnsFalse()
		{
			Directory.SetCurrentDirectory(_srcFolder);
			Assert.IsFalse(DirectoryHelper.AreEquivalent(_srcFolder, "~!source"));

			// Change the current directory because this test is failing on mono sometimes
			// when TestTearDown() deletes _srcFolder, which is the current directory.
			Directory.SetCurrentDirectory(Path.GetTempPath());
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreEquivalent_AbsolutePathAndRelativePathToSameFolder_ReturnsTrue()
		{
			Assert.IsTrue(DirectoryHelper.AreEquivalent(".", Directory.GetCurrentDirectory()));

			string curDir = Path.GetDirectoryName(_srcFolder);
			Debug.Assert(curDir != null);
			Directory.SetCurrentDirectory(curDir);
			Assert.IsTrue(DirectoryHelper.AreEquivalent(_srcFolder, "~!source"));
			string[] logicalDrives = null;
			try
			{
				logicalDrives = Directory.GetLogicalDrives();
			}
			catch
			{
				Assert.Ignore("Can't test this on this system");
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
						break;
					}
					catch
					{
						// try another folder
					}
				}
				// 02 SEP 2013, Phil Hopper: set correct directory separator for OS
				Assert.IsTrue(DirectoryHelper.AreEquivalent(_directorySeparator, logicalDrive));
				Assert.IsTrue(DirectoryHelper.AreEquivalent(_directorySeparator + "temp", logicalDrive + "temp"));
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

				var safeDirectories = DirectoryHelper.GetSafeDirectories(tempDir.Path);

				Assert.AreEqual(1, safeDirectories.Length);
				Assert.AreEqual(ordinaryDir.FullName, safeDirectories[0]);
			}
		}

		[Test]
		public void Move_SameFileThrows()
		{
			using (var tempFile = new TempFile())
			{
				Assert.That(() => DirectoryHelper.Move(tempFile.Path, tempFile.Path),
					Throws.InstanceOf<IOException>());
			}
		}

		[Test]
		public void Move_MoveToExistingFileThrows()
		{
			using (var tempFile = new TempFile())
			using (var existingFile = new TempFile())
			{
				Assert.That(() => DirectoryHelper.Move(tempFile.Path, existingFile.Path),
					Throws.InstanceOf<IOException>());
			}
		}

		[Test]
		public void Move_MoveDirToExistingDirThrows()
		{
			using (var tempDir = new TemporaryFolder("TempRootDir"))
			using (var existingDir = new TemporaryFolder("NewTempRootDir"))
			{
				Assert.That(() => DirectoryHelper.Move(tempDir.Path, existingDir.Path),
					Throws.InstanceOf<IOException>());
			}
		}

		[Test]
		public void Move_MoveDirToExistingFileThrows()
		{
			using (var tempDir = new TemporaryFolder("TempRootDir"))
			using (var existingFile = new TempFile())
			{
				Assert.That(() => DirectoryHelper.Move(tempDir.Path, existingFile.Path),
					Throws.InstanceOf<IOException>());
			}
		}

		[Test]
		public void Move_MoveFileToExistingDirThrows()
		{
			// while this could theoretically work the docs for Directory.Move say that if you
			// specify a file as source than destination also has to be a file.
			using (var tempFile = new TempFile())
			using (var existingDir = new TemporaryFolder("TempRootDir"))
			{
				Assert.That(() => DirectoryHelper.Move(tempFile.Path, existingDir.Path),
					Throws.InstanceOf<IOException>());
			}
		}

		[Test]
		[Platform(Exclude = "Win", Reason="Don't know how to test this on Windows")]
		public void Move_MoveDirToDifferentVolume()
		{
			// On Linux, /tmp is typically a ram disk and therefore a different partition from
			// /var/tmp which is supposed to persist across reboots.
			// On Mac, /tmp isn't usually a ram disk. However, it's possible to create and mount
			// loop filesystems (disk images) without root privileges. So it would be possible
			// to extend this when porting to Mac.
			if (PathHelper.GetDeviceNumber("/tmp") == PathHelper.GetDeviceNumber("/var/tmp"))
				Assert.Ignore("For this test /tmp and /var/tmp have to be on different partitions");

			var tempDir = Path.Combine("/tmp", Path.GetRandomFileName());
			Directory.CreateDirectory(tempDir);
			var dirOnDifferentVolume = Path.Combine("/var/tmp", Path.GetRandomFileName());
			Directory.CreateDirectory(dirOnDifferentVolume);
			using (TemporaryFolder.TrackExisting(tempDir))
			using (TemporaryFolder.TrackExisting(dirOnDifferentVolume))
			{
				Assert.That(() => DirectoryHelper.Move(tempDir,
					Path.Combine(dirOnDifferentVolume, "TempDir")),
					Throws.Nothing);
			}
		}

		[Test]
		[Platform(Exclude = "Win", Reason="Don't know how to test this on Windows")]
		public void Move_MoveFileToDifferentVolume()
		{
			// On Linux, /tmp is typically a ram disk and therefore a different partition from
			// /var/tmp which is supposed to persist across reboots.
			// On Mac, /tmp isn't usually a ram disk. However, it's possible to create and mount
			// loop filesystems (disk images) without root privileges. So it would be possible
			// to extend this when porting to Mac.
			if (PathHelper.GetDeviceNumber("/tmp") == PathHelper.GetDeviceNumber("/var/tmp"))
				Assert.Ignore("For this test /tmp and /var/tmp have to be on different partitions");

			var tempFile = Path.Combine("/tmp", Path.GetRandomFileName());
			var dirOnDifferentVolume = Path.Combine("/var/tmp", Path.GetRandomFileName());
			Directory.CreateDirectory(dirOnDifferentVolume);
			using (File.Create(tempFile))
			using (TempFile.TrackExisting(tempFile))
			using (TemporaryFolder.TrackExisting(dirOnDifferentVolume))
			{
				var destinationFile = Path.Combine(dirOnDifferentVolume, "TempFile");
				Assert.That(() => DirectoryHelper.Move(tempFile, destinationFile),
					Throws.Nothing);
				Assert.That(File.Exists(destinationFile), Is.True);
				Assert.That(File.Exists(tempFile), Is.False);
			}
		}

		[Test]
		public void Move_SourceDirDoesNotExist()
		{
			Assert.That(() => DirectoryHelper.Move(
				Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
				Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())),
				Throws.InstanceOf<DirectoryNotFoundException>());
		}

		[Test]
		public void IsEmpty_DirectoryIsEmpty()
		{
			using (var tempDir = new TemporaryFolder("IsEmpty_DirectoryIsEmpty"))
			{
				Assert.IsTrue(DirectoryHelper.IsEmpty(tempDir.Path));
				Assert.IsTrue(DirectoryHelper.IsEmpty(tempDir.Path, true));
			}
		}

		[Test]
		public void IsEmpty_DirectoryContainsSubDirectory()
		{
			using (var tempDir = new TemporaryFolder("IsEmpty_DirectoryContainsSubDirectory"))
			{
				Directory.CreateDirectory(Path.Combine(tempDir.Path, "subDirectory"));
				Assert.IsFalse(DirectoryHelper.IsEmpty(tempDir.Path));
				Assert.IsTrue(DirectoryHelper.IsEmpty(tempDir.Path, true));
			}
		}

		[Test]
		public void IsEmpty_DirectoryContainsFile()
		{
			using (var tempDir = new TemporaryFolder("IsEmpty_DirectoryContainsFile"))
			{
				var file = tempDir.GetNewTempFile(false);
				File.WriteAllText(file.Path, @"Some test text");
				Assert.IsFalse(DirectoryHelper.IsEmpty(tempDir.Path));
				Assert.IsFalse(DirectoryHelper.IsEmpty(tempDir.Path, true));
			}
		}

		[Test]
		public void IsEmpty_SubdirectoryContainsFile()
		{
			using (var tempDir = new TemporaryFolder("IsEmpty_DirectoryContainsFile"))
			{
				var dir = Directory.CreateDirectory(Path.Combine(tempDir.Path, "subDirectory2"));
				var fileName = Path.Combine(dir.FullName, "tempFile.txt");
				File.WriteAllText(fileName, @"Some test text");
				Assert.IsTrue(File.Exists(fileName));
				Assert.IsFalse(DirectoryHelper.IsEmpty(tempDir.Path));
				Assert.IsTrue(DirectoryHelper.IsEmpty(tempDir.Path, true));
				File.Delete(fileName);
			}
		}
	}
}
