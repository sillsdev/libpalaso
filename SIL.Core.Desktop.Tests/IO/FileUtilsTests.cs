using NUnit.Framework;
using SIL.IO;
using SIL.Reporting;
using SIL.TestUtilities;
using System;
using System.IO;
using System.Reflection;

namespace SIL.Core.Desktop.Tests.IO
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
		[Test, Explicit("Must be run manually with another drive")]
		public void MANUAL_ReplaceFileWithUserInteractionIfNeeded_DifferentDrives_OK()
		{
			ErrorReport.IsOkToInteractWithUser = false;
			using (var source = new TempFile("one"))
			using (var backup = new TempFile("two"))
			{
				// Since UsbDriveInfo was moved to SIL.Core.Desktop, we need to use reflection to access 
				// the drives since we can't add a reference to SIL.Core.Desktop.
				Assembly coreDesktopAssembly = Assembly.Load("SIL.Core.Desktop");
				Type usbDriveInfoType = coreDesktopAssembly.GetType("SIL.UsbDrive.UsbDriveInfo");
				MethodInfo getDrivesMethod = usbDriveInfoType.GetMethod("GetDrives");
				dynamic drives = getDrivesMethod.Invoke(usbDriveInfoType, null);
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
		public void GrepFile_ContainsPattern_ReplacesCorrectly()
		{
			using (var e = new TestEnvironment())
			{
				FileUtils.GrepFile(e.tempFile.Path, "lang", "1234567");
				Assert.That(FileHelper.Grep(e.tempFile.Path, "1234567"), Is.True);
				var bakPath = e.tempFile.Path + ".bak";
				File.Delete(bakPath);
			}
		}
	}
}
