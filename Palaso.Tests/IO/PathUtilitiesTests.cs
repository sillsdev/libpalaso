// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using NUnit.Framework;
using Palaso.IO;
using System.Diagnostics;
using Palaso.TestUtilities;

namespace Palaso.Tests.IO
{
	[TestFixture]
	public class PathUtilitiesTests
	{
		private bool TmpAndRootOnDifferentPartitions;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			if (Palaso.PlatformUtilities.Platform.IsUnix)
				TmpAndRootOnDifferentPartitions = StatFile("/tmp") != StatFile("/");
		}

		[Test]
		public void DeleteToRecycleBin_FileDeleted()
		{
			// Setup
			var file = Path.GetTempFileName();

			// Exercise
			var result = PathUtilities.DeleteToRecycleBin(file);

			// Verify
			Assert.That(result, Is.True);
			Assert.That(File.Exists(file), Is.False);
		}

		/// <summary>
		/// Finds the name of the trashed file for <paramref name="filePath"/>
		/// </summary>
		private string GetTrashedFileName(string trashBinPath, string filePath)
		{
			foreach (var possibleTrashedFile in Directory.EnumerateFiles(Path.Combine(trashBinPath, "files")))
			{
				var metaFile = Path.Combine(trashBinPath, "info",
					Path.GetFileName(possibleTrashedFile) + ".trashinfo");
				var metaFileContent = File.ReadAllText(metaFile);
				var lines = metaFileContent.Split('\n');
				if (lines.Length < 2)
					continue;

				if (lines[1].Substring("Path=".Length) == filePath)
					return possibleTrashedFile;
			}
			return null;
		}

		[Test]
		[Platform(Exclude = "Win", Reason="Don't know how to test this on Windows")]
		public void DeleteToRecycleBin_MovedToTrashBin()
		{
			// Setup
			var file = Path.GetTempFileName();
			var content = Guid.NewGuid().ToString();
			File.WriteAllText(file, content);

			// Exercise
			var result = PathUtilities.DeleteToRecycleBin(file);

			// Verify
			Assert.That(result, Is.True);
			var trashBinPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Trash");

			var trashedFile = GetTrashedFileName(trashBinPath, file);
			using (TempFile.TrackExisting(trashedFile))
			{
				Assert.That(File.Exists(trashedFile), Is.True);
				Assert.That(File.ReadAllText(trashedFile), Is.EqualTo(content));

				var metaFile = Path.Combine(trashBinPath, "info", Path.GetFileName(trashedFile) + ".trashinfo");
				using (TempFile.TrackExisting(metaFile))
				{
					Assert.That(File.Exists(metaFile), Is.True);

					var metaFileContent = File.ReadAllText(metaFile);
					var lines = metaFileContent.Split('\n');
					Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
					Assert.That(lines[0], Is.EqualTo("[Trash Info]"));
					Assert.That(lines[1], Is.StringStarting("Path="));
					Assert.That(lines[1], Is.StringEnding(file));
					Assert.That(lines[2], Is.StringMatching(@"DeletionDate=\d\d\d\d\d\d\d\dT\d\d:\d\d:\d\d"));
				}
			}
		}

		[Test]
		public void DeleteToRecycleBin_DirectoryDeleted()
		{
			// Setup
			var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(dir);

			var file = Path.Combine(dir, Path.GetRandomFileName());
			File.WriteAllText(file, "Some content");

			// Exercise
			var result = PathUtilities.DeleteToRecycleBin(dir);

			// Verify
			Assert.That(result, Is.True);
			Assert.That(File.Exists(file), Is.False);
			Assert.That(Directory.Exists(dir), Is.False);
		}

		[Test]
		public void DeleteToRecycleBin_NonexistingFileReturnsFalse()
		{
			// Setup
			var file = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			// Exercise
			var result = PathUtilities.DeleteToRecycleBin(file);

			// Verify
			Assert.That(result, Is.False);
		}

		[Test]
		public void PathsAreOnSameVolume_SameFile()
		{
			using (var tempFile = new TempFile())
			{
				Assert.That(PathUtilities.PathsAreOnSameVolume(tempFile.Path, tempFile.Path),
					Is.True);
			}
		}

		[Test]
		public void PathsAreOnSameVolume_SameVolume()
		{
			using (var tempFile1 = new TempFile())
			using (var tempFile2 = new TempFile())
			{
				Assert.That(PathUtilities.PathsAreOnSameVolume(tempFile1.Path, tempFile2.Path),
					Is.True);
			}
		}

		[Test]
		public void PathsAreOnSameVolume_OneEmpty()
		{
			using (var tempFile = new TempFile())
			{
				Assert.That(PathUtilities.PathsAreOnSameVolume(tempFile.Path, string.Empty),
					Is.False);
				Assert.That(PathUtilities.PathsAreOnSameVolume(string.Empty, tempFile.Path),
					Is.False);
			}
		}

		[Test]
		public void PathsAreOnSameVolume_OneNull()
		{
			using (var tempFile = new TempFile())
			{
				Assert.That(PathUtilities.PathsAreOnSameVolume(tempFile.Path, null),
					Is.False);
				Assert.That(PathUtilities.PathsAreOnSameVolume(null, tempFile.Path),
					Is.False);
			}
		}

		[Test]
		[Platform(Include = "Linux", Reason="Linux specific test")]
		public void PathsAreOnSameVolume_TwoVolumesLinux()
		{
			// On Linux, /tmp is typically a ram disk and therefore a different partition from
			// /var/tmp which is supposed to persist across reboots.
			// On Mac, /tmp isn't usually a ram disk. However, it's possible to create and mount
			// loop filesystems (disk images) without root privileges. So it would be possible
			// to extend this when porting to Mac.
			if (PathUtilities.GetDeviceNumber("/tmp") == PathUtilities.GetDeviceNumber("/var/tmp"))
				Assert.Ignore("For this test /tmp and /var/tmp have to be on different partitions");

			var tempFile1 = Path.Combine("/tmp", Path.GetRandomFileName());
			var tempFile2 = Path.Combine("/var/tmp", Path.GetRandomFileName());
			using (File.Create(tempFile1))
			using (File.Create(tempFile2))
			using (TempFile.TrackExisting(tempFile1))
			using (TempFile.TrackExisting(tempFile2))
			{
				Assert.That(PathUtilities.PathsAreOnSameVolume(tempFile1, tempFile2),
					Is.False);
			}
		}

		[Test]
		[Platform(Include = "Win", Reason="Windows specific test")]
		public void PathsAreOnSameVolume_TwoVolumesWindows()
		{
			if (Environment.GetLogicalDrives().Length < 2)
				Assert.Ignore("For this test we need at least two drives");

			var tempFile1 = Path.Combine(Environment.GetLogicalDrives()[0], "file1");
			var tempFile2 = Path.Combine(Environment.GetLogicalDrives()[1], "file2");
			Assert.That(PathUtilities.PathsAreOnSameVolume(tempFile1, tempFile2),
				Is.False);
		}

		private string StatFile(string path)
		{
			using (var process = new Process())
			{
				var statFlags = Palaso.PlatformUtilities.Platform.IsMac ? "-f" : "-c";
				process.StartInfo = new ProcessStartInfo {
					FileName = "stat",
					Arguments = string.Format("{0} %d {1}", statFlags, path),
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				};
				process.Start();
				process.WaitForExit();
				return process.StandardOutput.ReadToEnd();
			}
		}

		[Test]
		[Platform(Include = "Linux", Reason="Linux specific test")]
		public void GetDeviceNumber_NonExistingFileReturnsNumberOfParentDirectory()
		{
			if (!TmpAndRootOnDifferentPartitions)
				Assert.Ignore("For this test / and /tmp have to be on different partitions");

			var deviceNumber = PathUtilities.GetDeviceNumber(Path.Combine("/tmp", Path.GetRandomFileName()));
			Assert.That(deviceNumber, Is.EqualTo(PathUtilities.GetDeviceNumber("/tmp")));
		}

		[Test]
		[Platform(Include = "Linux", Reason="Linux specific test")]
		public void GetDeviceNumber_ExistingFileReturnsNumberOfParentDirectory()
		{
			if (!TmpAndRootOnDifferentPartitions)
				Assert.Ignore("For this test / and /tmp have to be on different partitions");

			var fileName = Path.Combine("/tmp", Path.GetRandomFileName());

			using (File.Create(fileName))
			using (TempFile.TrackExisting(fileName))
			{
				var deviceNumber = PathUtilities.GetDeviceNumber(fileName);
				Assert.That(deviceNumber, Is.EqualTo(PathUtilities.GetDeviceNumber("/tmp")));
			}
		}

		[Test]
		[Platform(Include = "Linux", Reason="Linux specific test")]
		public void GetDeviceNumber_FileInNonExistingSubdirectoryReturnsNumberOfParentDirectory()
		{
			if (!TmpAndRootOnDifferentPartitions)
				Assert.Ignore("For this test / and /tmp have to be on different partitions");

			var deviceNumber = PathUtilities.GetDeviceNumber(
				Path.Combine("/tmp", Path.GetRandomFileName(), Path.GetRandomFileName()));
			Assert.That(deviceNumber, Is.EqualTo(PathUtilities.GetDeviceNumber("/tmp")));
		}

		[Test]
		[Platform(Include = "Linux", Reason="Linux specific test")]
		public void GetDeviceNumber_NonExistingFileInExistingSubdirectoryReturnsNumberOfParentDirectory()
		{
			if (!TmpAndRootOnDifferentPartitions)
				Assert.Ignore("For this test / and /tmp have to be on different partitions");

			var dirName = Path.Combine("/tmp", Path.GetRandomFileName());
			Directory.CreateDirectory(dirName);

			using (TemporaryFolder.TrackExisting(dirName))
			{
				var deviceNumber = PathUtilities.GetDeviceNumber(
					Path.Combine(dirName, Path.GetRandomFileName()));
				Assert.That(deviceNumber, Is.EqualTo(PathUtilities.GetDeviceNumber("/tmp")));
			}
		}

		//See http://stackoverflow.com/a/30405340/723299
		[Test, Ignore("By Hand")]
		public void SelectFileInExplorer_PathHasCombiningCharacters_StillOpensAndSelects()
		{
			var path = Path.Combine(Path.GetTempPath(), "ปู  should select this file");
			if (!File.Exists(path))
			{
				File.WriteAllText(path, "");
			}
			PathUtilities.SelectFileInExplorer(path);
		}
		//See http://stackoverflow.com/a/30405340/723299
		[Test, Ignore("By Hand")]
		public void SelectFileInExplorer_PathIsADirectoryHasCombiningCharacters_StillOpensAndSelects()
		{
			var path = Path.Combine(Path.GetTempPath(), "ปู  should select this directory");
			if(!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			PathUtilities.SelectFileInExplorer(path);
		}

		//See http://stackoverflow.com/a/30405340/723299
		[Test, Ignore("By Hand")]
		public void OpenDirectoryInExplorer_PathIsADirectoryHasCombiningCharacters_StillOpens()
		{
			//as of May 27 2015, this is expected to fail on Windows. See enhancment note 
			//in the OpenDirectoryInExplorer() code.
			var path = Path.Combine(Path.GetTempPath(), "ปู should select this directory");
			if(!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			PathUtilities.OpenDirectoryInExplorer(path);
		}
	}
}

