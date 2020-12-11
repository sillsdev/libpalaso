// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.TestUtilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SIL.Tests.IO
{
	[TestFixture]
	public class PathHelperTests
	{
		private bool TmpAndRootOnDifferentPartitions;

		[OneTimeSetUp]
		public void FixtureSetUp()
		{
			if (Platform.IsUnix)
				TmpAndRootOnDifferentPartitions = StatFile("/tmp") != StatFile("/");
		}

		[Test]
		public void AreOnSameVolume_SameFile()
		{
			using (var tempFile = new TempFile())
			{
				Assert.That(PathHelper.AreOnSameVolume(tempFile.Path, tempFile.Path),
					Is.True);
			}
		}

		[Test]
		public void AreOnSameVolume_SameVolume()
		{
			using (var tempFile1 = new TempFile())
			using (var tempFile2 = new TempFile())
			{
				Assert.That(PathHelper.AreOnSameVolume(tempFile1.Path, tempFile2.Path),
					Is.True);
			}
		}

		[Test]
		public void AreOnSameVolume_OneEmpty()
		{
			using (var tempFile = new TempFile())
			{
				Assert.That(PathHelper.AreOnSameVolume(tempFile.Path, string.Empty),
					Is.False);
				Assert.That(PathHelper.AreOnSameVolume(string.Empty, tempFile.Path),
					Is.False);
			}
		}

		[Test]
		public void AreOnSameVolume_OneNull()
		{
			using (var tempFile = new TempFile())
			{
				Assert.That(PathHelper.AreOnSameVolume(tempFile.Path, null),
					Is.False);
				Assert.That(PathHelper.AreOnSameVolume(null, tempFile.Path),
					Is.False);
			}
		}

		[Test]
		[Platform(Include = "Linux", Reason="Linux specific test")]
		public void AreOnSameVolume_TwoVolumesLinux()
		{
			// On Linux, /tmp is typically a ram disk and therefore a different partition from
			// /var/tmp which is supposed to persist across reboots.
			// On Mac, /tmp isn't usually a ram disk. However, it's possible to create and mount
			// loop filesystems (disk images) without root privileges. So it would be possible
			// to extend this when porting to Mac.
			if (PathHelper.GetDeviceNumber("/tmp") == PathHelper.GetDeviceNumber("/var/tmp"))
				Assert.Ignore("For this test /tmp and /var/tmp have to be on different partitions");

			var tempFile1 = Path.Combine("/tmp", Path.GetRandomFileName());
			var tempFile2 = Path.Combine("/var/tmp", Path.GetRandomFileName());
			using (File.Create(tempFile1))
			using (File.Create(tempFile2))
			using (TempFile.TrackExisting(tempFile1))
			using (TempFile.TrackExisting(tempFile2))
			{
				Assert.That(PathHelper.AreOnSameVolume(tempFile1, tempFile2),
					Is.False);
			}
		}

		[Test]
		[Platform(Include = "Win", Reason="Windows specific test")]
		public void AreOnSameVolume_TwoVolumesWindows()
		{
			if (Environment.GetLogicalDrives().Length < 2)
				Assert.Ignore("For this test we need at least two drives");

			var tempFile1 = Path.Combine(Environment.GetLogicalDrives()[0], "file1");
			var tempFile2 = Path.Combine(Environment.GetLogicalDrives()[1], "file2");
			Assert.That(PathHelper.AreOnSameVolume(tempFile1, tempFile2),
				Is.False);
		}

		private string StatFile(string path)
		{
			using (var process = new Process())
			{
				var statFlags = Platform.IsMac ? "-f" : "-c";
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

			var deviceNumber = PathHelper.GetDeviceNumber(Path.Combine("/tmp", Path.GetRandomFileName()));
			Assert.That(deviceNumber, Is.EqualTo(PathHelper.GetDeviceNumber("/tmp")));
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
				var deviceNumber = PathHelper.GetDeviceNumber(fileName);
				Assert.That(deviceNumber, Is.EqualTo(PathHelper.GetDeviceNumber("/tmp")));
			}
		}

		[Test]
		[Platform(Include = "Linux", Reason="Linux specific test")]
		public void GetDeviceNumber_FileInNonExistingSubdirectoryReturnsNumberOfParentDirectory()
		{
			if (!TmpAndRootOnDifferentPartitions)
				Assert.Ignore("For this test / and /tmp have to be on different partitions");

			var deviceNumber = PathHelper.GetDeviceNumber(
				Path.Combine("/tmp", Path.GetRandomFileName(), Path.GetRandomFileName()));
			Assert.That(deviceNumber, Is.EqualTo(PathHelper.GetDeviceNumber("/tmp")));
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
				var deviceNumber = PathHelper.GetDeviceNumber(
					Path.Combine(dirName, Path.GetRandomFileName()));
				Assert.That(deviceNumber, Is.EqualTo(PathHelper.GetDeviceNumber("/tmp")));
			}
		}

		[TestCase(null, ExpectedResult = false)]
		[TestCase("", ExpectedResult = false)]
		[TestCase("rect", ExpectedResult = false)]
		[TestCase("none", ExpectedResult = false)]
		[TestCase("directory", ExpectedResult = true)]
		[TestCase("subdir", ExpectedResult = true)]
		public bool ContainsDirectory(string directory)
		{
			var path = Path.Combine(Path.GetTempPath(), "some", "directory", "and", "other", "subdir");
			return PathHelper.ContainsDirectory(path, directory);
		}

		[Test]
		public void UniqueFolderPathNotCreated()
		{
			using (var tempDir = new TemporaryFolder("TempRootDir"))
			{
				var targetDir = Path.Combine(tempDir.Path, "ZPI");
				var uniqueFolderPath = PathHelper.GetUniqueFolderPath(targetDir);
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
				var uniqueFolderPath = PathHelper.GetUniqueFolderPath(targetDir + Path.DirectorySeparatorChar);
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
				var uniqueFolderPath = PathHelper.GetUniqueFolderPath(targetDir);
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
				var uniqueFolderPath = PathHelper.GetUniqueFolderPath(targetDir + Path.DirectorySeparatorChar);
				Assert.IsFalse(Directory.Exists(uniqueFolderPath));
				Assert.AreNotEqual(targetDir, uniqueFolderPath);
			}
		}

		[Test]
		public void CheckValidPathname_RejectsNullOrEmpty_Pathname()
		{
			Assert.IsFalse(PathHelper.CheckValidPathname(null, "xyz"));
			Assert.IsFalse(PathHelper.CheckValidPathname("", "xyz"));
		}

		[Test]
		public void CheckValidPathname_RejectsNonExtant_Pathname()
		{
			Assert.IsFalse(PathHelper.CheckValidPathname("Bogus.txt", "xyz"));
		}

		[Test]
		public void CheckValidPathname_RejectsMismatchedExtension()
		{
			using (var e = new FileTestEnvironment())
			{
				Assert.IsFalse(PathHelper.CheckValidPathname(e.TempFile.Path, "xyz"));
				Assert.IsFalse(PathHelper.CheckValidPathname(e.TempFile.Path, null));
				Assert.IsFalse(PathHelper.CheckValidPathname(e.TempFile.Path, ""));
			}
		}

		[Test]
		public void CheckValidPathname_AcceptsMatchedExtensions()
		{
			using (var e = new FileTestEnvironment())
			{
				Assert.True(PathHelper.CheckValidPathname(e.TempFile.Path, Path.GetExtension(e.TempFile.Path))); // With starting '.'
				Assert.True(PathHelper.CheckValidPathname(e.TempFile.Path, Path.GetExtension(e.TempFile.Path).Substring(1))); // Sans starting '.'
			}
		}

		[Test]
		public void CheckValidPathname_AcceptsMissingExtensions()
		{
			var tempPathname = Path.Combine(Path.GetTempPath(), "extensionlessfile");
			try
			{
				File.WriteAllText(tempPathname, "stuff");
				Assert.True(PathHelper.CheckValidPathname(tempPathname, null));
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
			Assert.That(PathHelper.NormalizePath("/a/b/c"), Is.EqualTo("/a/b/c"));
		}

		[Test]
		public void NormalizePath_BackslashConvertsToSlash()
		{
			Assert.That(PathHelper.NormalizePath("\\a\\b\\c"), Is.EqualTo("/a/b/c"));
		}

		[Test]
		public void NormalizePath_MixedConvertsToSlashes()
		{
			Assert.That(PathHelper.NormalizePath("/a\\b/c"), Is.EqualTo("/a/b/c"));
		}
		[Test]
		public void NormalizePath_WindowsStylePathConvertsToSlashes()
		{
			Assert.That(PathHelper.NormalizePath("c:\\a\\b\\c"), Is.EqualTo("c:/a/b/c"));
		}

		[Test]
		[Platform(Include = "Win")]
		public void StripFilePrefix_EnsureFilePrefixIsRemoved_Windows()
		{
			var prefix = Uri.UriSchemeFile + ":";
			var fullPathname = Assembly.GetExecutingAssembly().CodeBase;
			Assert.IsTrue(fullPathname.StartsWith(prefix));

			var reducedPathname = PathHelper.StripFilePrefix(fullPathname);
			Assert.IsFalse(reducedPathname.StartsWith(prefix));
			Assert.IsFalse(reducedPathname.StartsWith("/"));
		}

		[Test]
		[Platform(Include = "Linux")]
		public void StripFilePrefix_EnsureFilePrefixIsRemoved_Linux()
		{
			var prefix = Uri.UriSchemeFile + ":";
			var fullPathname = Assembly.GetExecutingAssembly().CodeBase;
			Assert.IsTrue(fullPathname.StartsWith(prefix));

			var reducedPathname = PathHelper.StripFilePrefix(fullPathname);
			Assert.IsFalse(reducedPathname.StartsWith(prefix));
			Assert.IsTrue(reducedPathname.StartsWith("/"));
		}
	}
}

