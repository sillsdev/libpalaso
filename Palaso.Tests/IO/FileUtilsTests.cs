using System;
using System.IO;
using NUnit.Framework;
using Palaso.IO;
using Palaso.TestUtilities;
using Palaso.UsbDrive;

namespace Palaso.Tests.IO
{
	[TestFixture]
	public class FileUtilTests
	{
		private TemporaryFolder _parentFolder;

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void Setup()
		{
			_parentFolder = new TemporaryFolder("FileUtilsTests");
		}

		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TearDown()
		{
			_parentFolder.Dispose();
			_parentFolder = null;
		}

		[Test]
		public void IsFileLocked_FilePathIsNull_ReturnsFalse()
		{
			Assert.IsFalse(FileUtils.IsFileLocked(null));
		}

		[Test]
		public void IsFileLocked_FileDoesntExist_ReturnsFalse()
		{
			Assert.IsFalse(FileUtils.IsFileLocked(@"c:\blahblah.blah"));
		}

		[Test]
		public void IsFileLocked_FileExistsAndIsNotLocked_ReturnsFalse()
		{
			using (var file = new TempFileFromFolder(_parentFolder))
				Assert.IsFalse(FileUtils.IsFileLocked(file.Path));
		}

		[Test]
		public void IsFileLocked_FileExistsAndIsLocked_ReturnsTrue()
		{
			using (var file = new TempFileFromFolder(_parentFolder))
			{
				var stream = File.OpenWrite(file.Path);
				try
				{
					Assert.IsTrue(FileUtils.IsFileLocked(file.Path));
				}
				finally
				{
					stream.Close();
				}
			}
		}

		public class TestEnvironment : IDisposable
		{
			public TempFile tempFile;
			public TestEnvironment()
			{

				string fileContents = "some bogus text\n<element></element>\n<el2 lang='fr' />";
				tempFile = new TempFile(fileContents);
			}


			public void Dispose()
			{
				tempFile.Dispose();
			}
		}
		/// <summary>
		/// regression for WS-1394. Must be run manually because it requires a second drive
		/// </summary>
		[Test, Ignore("Must be run manually with another drive")]
		public void MANUAL_ReplaceFileWithUserInteractionIfNeeded_DifferentDrives_OK()
		{
			Reporting.ErrorReport.IsOkToInteractWithUser = false;
			using (var source = new TempFile("one"))
			using (var backup = new TempFile("two"))
			{
				var drives = UsbDriveInfo.GetDrives();
				Assert.Greater(drives.Count, 0, "This test requires at least one writeable USB drive");

				var testFolder = Path.Combine(drives[0].RootDirectory.FullName, "PalasoFileUtilsUnitTests");
				Directory.CreateDirectory(testFolder);
				using (var folder = TemporaryFolder.TrackExisting(testFolder))
				using (var destination = new TempFileFromFolder(folder))
				{
					FileUtils.ReplaceFileWithUserInteractionIfNeeded(source.Path, destination.Path, backup.Path);
				}
			}
		}

		/// <summary>
		/// Regression Test. It used to be that if the path, on windows, was a network path (even \\localhost\c$\),
		/// this would fail as it couldn't find a drive letter.
		/// </summary>
		[Test]
		[Platform(Exclude = "Linux", Reason = "ConvertToUNCLocalHostPath only works on Windows")]
		public void ReplaceFileWithUserInteractionIfNeeded_UsingUNCLocalHostPath()
		{
			using (var source = new TempFile("new"))
			using (var backup = new TempFile("previousBackup"))
			using (var destination = new TempFile("old"))
			{
				FileUtils.ReplaceFileWithUserInteractionIfNeeded(ConvertToUNCLocalHostPath(source.Path), ConvertToUNCLocalHostPath(destination.Path), backup.Path);
				Assert.AreEqual("new", File.ReadAllText(destination.Path));
				Assert.AreEqual("old", File.ReadAllText(backup.Path));
			}
		}

		private string ConvertToUNCLocalHostPath(string drivePath)
		{
			string driveLetter = Directory.GetDirectoryRoot(drivePath);
			return drivePath.Replace(driveLetter, "//localhost/" + driveLetter.Replace(":\\", "") + "$/");
		}

		[Test]
		public void ReplaceFileWithUserInteractionIfNeeded_SameDrive_OK()
		{
			using (var source = new TempFile("new"))
			using (var backup = new TempFile("previousBackup"))
			using (var destination = new TempFile("old"))
			{
				FileUtils.ReplaceFileWithUserInteractionIfNeeded(source.Path, destination.Path, backup.Path);
				Assert.AreEqual("new", File.ReadAllText(destination.Path));
				Assert.AreEqual("old", File.ReadAllText(backup.Path));
			}
		}

		[Test]
		public void ReplaceFileWithUserInteractionIfNeeded_BackupNull_OK()
		{
			using (var source = new TempFile("one"))
			using (var destination = new TempFile("three"))
			{
				FileUtils.ReplaceFileWithUserInteractionIfNeeded(source.Path, destination.Path, null);
				Assert.AreEqual("one", File.ReadAllText(destination.Path));
			}
		}

		[Test]
		public void ReplaceFileWithUserInteractionIfNeeded_DestinationDoesntExist_OK()
		{
			using (var destination = TempFile.CreateAndGetPathButDontMakeTheFile())
			using (var source = new TempFile("one"))
			{
				Assert.That(() => FileUtils.ReplaceFileWithUserInteractionIfNeeded(source.Path, destination.Path, null),
					Throws.Nothing);
				Assert.That(File.ReadAllText(destination.Path), Is.EqualTo("one"));
			}
		}

		[Test]
		public void GrepFile_FileContainsPattern_True()
		{
			using (var e = new TestEnvironment())
			{
				Assert.That(FileUtils.GrepFile(e.tempFile.Path, "lang='fr'"), Is.True);
			}
		}


		[Test]
		public void GrepFile_FileDoesNotContainPattern_False()
		{
			using (var e = new TestEnvironment())
			{
				Assert.That(FileUtils.GrepFile(e.tempFile.Path, "lang='ee'"), Is.False);
			}
		}

		[Test]
		public void GrepFile_ContainsPattern_ReplacesCorrectly()
		{
			using (var e = new TestEnvironment())
			{
				FileUtils.GrepFile(e.tempFile.Path, "lang", "1234567");
				Assert.That(FileUtils.GrepFile(e.tempFile.Path, "1234567"), Is.True);
				var bakPath = e.tempFile.Path + ".bak";
				File.Delete(bakPath);
			}
		}

		[Test]
		public void CheckValidPathname_RejectsNullOrEmpty_Pathname()
		{
			Assert.IsFalse(FileUtils.CheckValidPathname(null, "xyz"));
			Assert.IsFalse(FileUtils.CheckValidPathname("", "xyz"));
		}

		[Test]
		public void CheckValidPathname_RejectsNonExtant_Pathname()
		{
			Assert.IsFalse(FileUtils.CheckValidPathname("Bogus.txt", "xyz"));
		}

		[Test]
		public void CheckValidPathname_RejectsMismatchedExtension()
		{
			using (var e = new TestEnvironment())
			{
				Assert.IsFalse(FileUtils.CheckValidPathname(e.tempFile.Path, "xyz"));
				Assert.IsFalse(FileUtils.CheckValidPathname(e.tempFile.Path, null));
				Assert.IsFalse(FileUtils.CheckValidPathname(e.tempFile.Path, ""));
			}
		}

		[Test]
		public void CheckValidPathname_AcceptsMatchedExtensions()
		{
			using (var e = new TestEnvironment())
			{
				Assert.True(FileUtils.CheckValidPathname(e.tempFile.Path, Path.GetExtension(e.tempFile.Path))); // With starting '.'
				Assert.True(FileUtils.CheckValidPathname(e.tempFile.Path, Path.GetExtension(e.tempFile.Path).Substring(1))); // Sans starting '.'
			}
		}

		[Test]
		public void CheckValidPathname_AcceptsMissingExtensions()
		{
			var tempPathname = Path.Combine(Path.GetTempPath(), "extensionlessfile");
			try
			{
				File.WriteAllText(tempPathname, "stuff");
				Assert.True(FileUtils.CheckValidPathname(tempPathname, null));
			}
			finally
			{
				if (File.Exists(tempPathname))
					File.Delete(tempPathname);
			}
		}

		[Test]
		public void NormalizePath_SlashStaysSlash()
		{
			Assert.That(FileUtils.NormalizePath("/a/b/c"), Is.EqualTo("/a/b/c"));
		}

		[Test]
		public void NormalizePath_BackslashConvertsToSlash()
		{
			Assert.That(FileUtils.NormalizePath("\\a\\b\\c"), Is.EqualTo("/a/b/c"));
		}

		[Test]
		public void NormalizePath_MixedConvertsToSlashes()
		{
			Assert.That(FileUtils.NormalizePath("/a\\b/c"), Is.EqualTo("/a/b/c"));
		}
		[Test]
		public void NormalizePath_WindowsStylePathConvertsToSlashes()
		{
			Assert.That(FileUtils.NormalizePath("c:\\a\\b\\c"), Is.EqualTo("c:/a/b/c"));
		}
	}
}
