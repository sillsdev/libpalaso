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
			using(var source = new TempFile("one"))
			{
				var backup = new TempFile("two");

				var drives = UsbDriveInfo.GetDrives();
				Assert.Greater(drives.Count, 0, "This test requires at least one writeable USB drive");

				var testFolder = Path.Combine(drives[0].RootDirectory.FullName, "PalasoFileUtilsUnitTests");
				Directory.CreateDirectory(testFolder);
				using (var folder = TemporaryFolder.TrackExisting(testFolder))
				{
					var destination = new TempFileFromFolder(folder);
					FileUtils.ReplaceFileWithUserInteractionIfNeeded(source.Path, destination.Path, backup.Path);
				}
			}
		}

		[Test]
		public void ReplaceFileWithUserInteractionIfNeeded_SameDrive_OK()
		{
			using (var source = new TempFile("new"))
			{
				var backup = new TempFile("previousBackup");
				var destination = new TempFile("old");
				FileUtils.ReplaceFileWithUserInteractionIfNeeded(source.Path, destination.Path, backup.Path);
				Assert.AreEqual("new", File.ReadAllText(destination.Path));
				Assert.AreEqual("old", File.ReadAllText(backup.Path));
			}
		}

		[Test]
		public void ReplaceFileWithUserInteractionIfNeeded_BackupNull_OK()
		{
			using (var source = new TempFile("one"))
			{
				var destination = new TempFile("three");
				FileUtils.ReplaceFileWithUserInteractionIfNeeded(source.Path, destination.Path, null);
				Assert.AreEqual("one", File.ReadAllText(destination.Path));
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
			}
		}
	}
}
