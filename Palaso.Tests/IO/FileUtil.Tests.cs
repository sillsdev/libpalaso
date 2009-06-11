using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Palaso.IO;

namespace Palaso.Tests.IO
{
	[TestFixture]
	public class FileUtilTests
	{
		/// <summary>
		/// regression for WS-1394. Must be run manually because it requires a second drive
		/// </summary>
		[Test, Ignore("Must be run manually with another drive")]
		public void MANUAL_ReplaceFileWithUserInteractionIfNeeded_DifferentDrives_OK()
		{
			Reporting.ErrorReport.IsOkToInteractWithUser = false;
			using(var source = new Palaso.TestUtilities.TempFile("one"))
			{
				var backup = new Palaso.TestUtilities.TempFile("two");

				var drives = UsbDrive.UsbDriveInfo.GetDrives();
				Assert.Greater(drives.Count, 0, "This test requires at least one writeable USB drive");

				var testFolder = Path.Combine(drives[0].RootDirectory.FullName, "PalasoFileUtilsUnitTests");
				Directory.CreateDirectory(testFolder);
				using (var folder = TestUtilities.TemporaryFolder.TrackExisting(testFolder))
				{
					var destination = new Palaso.TestUtilities.TempFile(folder);
					FileUtils.ReplaceFileWithUserInteractionIfNeeded(source.Path, destination.Path, backup.Path);
				}
			}
		}

		[Test]
		public void ReplaceFileWithUserInteractionIfNeeded_SameDrive_OK()
		{
			using (var source = new Palaso.TestUtilities.TempFile("new"))
			{
				var backup = new Palaso.TestUtilities.TempFile("previousBackup");
				var destination = new Palaso.TestUtilities.TempFile("old");
				FileUtils.ReplaceFileWithUserInteractionIfNeeded(source.Path, destination.Path, backup.Path);
				Assert.AreEqual("new", File.ReadAllText(destination.Path));
				Assert.AreEqual("old", File.ReadAllText(backup.Path));
			}
		}

		[Test]
		public void ReplaceFileWithUserInteractionIfNeeded_BackupNull_OK()
		{
			using (var source = new Palaso.TestUtilities.TempFile("one"))
			{
				var destination = new Palaso.TestUtilities.TempFile("three");
				FileUtils.ReplaceFileWithUserInteractionIfNeeded(source.Path, destination.Path, null);
				Assert.AreEqual("one", File.ReadAllText(destination.Path));
			}
		}
	}
}
