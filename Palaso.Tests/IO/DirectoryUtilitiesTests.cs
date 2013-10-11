using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Palaso.IO;
using Palaso.TestUtilities;

namespace Palaso.Tests.IO
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
		public void AreDirectoriesEquivalent_DifferByDirectionOfSlash_ReturnsTrue()
		{
#if MONO
			// 02 SEP 2013, Phil Hopper: this test is not valid on a linux file system.
			return;
#else
			Assert.IsTrue(DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp", @"C:/temp"));
#endif

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
		public void AreDirectoriesEquivalent_DifferByTrailingDot_ResultDependsOnOperatingSystem()
		{
#if MONO
			Assert.IsFalse(
#else
			Assert.IsTrue(
#endif
DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp.", @"C:\temp"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void AreDirectoriesEquivalent_DifferByTrailingDotsAndBackslash_ResultDependsOnOperatingSystem()
		{
#if MONO
			Assert.IsFalse(
#else
			Assert.IsTrue(
#endif
DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp...\", @"C:\temp"));
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
		public void AreDirectoriesEquivalent_DifferentCase_ResultDependsOnOperatingSystem()
		{
#if MONO
			Assert.IsFalse(
#else
			Assert.IsTrue(
#endif
			DirectoryUtilities.AreDirectoriesEquivalent(@"C:\temp", @"c:\TEMP\x\..\..\tEmp\."));
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
	}
}
