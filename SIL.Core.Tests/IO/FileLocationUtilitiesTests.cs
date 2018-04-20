using System;
using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.PlatformUtilities;

namespace SIL.Tests.IO
{
	[TestFixture]
	class FileLocationUtilitiesTests
	{
		[Test]
		public void GetFileDistributedWithApplication_MultipleParts_FindsCorrectly()
		{
			var path = FileLocationUtilities.GetFileDistributedWithApplication("DirectoryForTests", "SampleFileForTests.txt");
			Assert.That(File.Exists(path));
		}
		[Test]
		public void GetDirectoryDistributedWithApplication_MultipleParts_FindsCorrectly()
		{
			var path = FileLocationUtilities.GetDirectoryDistributedWithApplication("DirectoryForTests");
			Assert.That(Directory.Exists(path));
		}

		[Test]
		public void GetDirectoryDistributedWithApplication_WhenFails_ReportsAllTried()
		{
			try
			{
				FileLocationUtilities.GetDirectoryDistributedWithApplication("LookHere", "ThisWillNotExist");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message, Is.StringContaining(Path.Combine("LookHere", "ThisWillNotExist")));
				Assert.That(ex.Message, Is.StringContaining(FileLocationUtilities.DirectoryOfApplicationOrSolution));
				Assert.That(ex.Message, Is.StringContaining("DistFiles"));
				Assert.That(ex.Message, Is.StringContaining("src"));
			}
		}

		[Test]
		public void DirectoryOfApplicationOrSolution_OnDevMachine_FindsOutputDirectory()
		{
			var path = FileLocationUtilities.DirectoryOfTheApplicationExecutable;
			Assert.That(Directory.Exists(path));
			Assert.That(path.Contains("output"));
		}

		[Test]
		public void LocateInProgramFiles_SendInvalidProgramNoDeepSearch_ReturnsNull()
		{
			Assert.IsNull(FileLocationUtilities.LocateInProgramFiles("blah.exe", false));
		}

		// 12 SEP 2013, Phil Hopper: This test not valid on Mono.
		[Test]
		[Platform(Exclude = "Unix")]
		[Category("SkipOnTeamCity;KnownMonoIssue")]
		public void LocateInProgramFiles_SendValidProgramNoDeepSearch_ReturnsNull()
		{
			Assert.IsNull(FileLocationUtilities.LocateInProgramFiles("msinfo32.exe", false));
		}

		[Test]
		public void LocateInProgramFiles_SendValidProgramDeepSearch_ReturnsProgramPath()
		{
			var findFile = (Platform.IsMono ? "bash" : "msinfo32.exe");
			Assert.IsNotNull(FileLocationUtilities.LocateInProgramFiles(findFile, true));
		}

		[Test]
		public void LocateInProgramFiles_SendValidProgramDeepSearch_SubFolderSpecified_ReturnsProgramPath()
		{
			var findFile = (Platform.IsMono ? "bash" : "msinfo32.exe");

			// this will work on Mono because it ignores the subFoldersToSearch parameter
			Assert.IsNotNull(FileLocationUtilities.LocateInProgramFiles(findFile, true, "Common Files"));
		}

		[Test]
		public void LocateInProgramFiles_SendInValidSubFolder_DoesNotThrow()
		{
			var findFile = (Platform.IsMono ? "bash" : "msinfo32.exe");
			Assert.DoesNotThrow(() => FileLocationUtilities.LocateInProgramFiles(findFile, true, "!~@blah"));
		}

		//TODO: this could use lots more tests

		[Test]
		public void LocateExecutable_DistFiles()
		{
			Assert.That(FileLocationUtilities.LocateExecutable("DirectoryForTests", "SampleExecutable.exe"),
				Is.StringEnding(string.Format("DistFiles{0}DirectoryForTests{0}SampleExecutable.exe",
					Path.DirectorySeparatorChar)));
		}

		[Test]
		[Platform(Exclude = "Linux")]
		public void LocateExecutable_PlatformSpecificInDistFiles_Windows()
		{
			Assert.That(FileLocationUtilities.LocateExecutable("DirectoryForTests", "dummy.exe"),
				Is.StringEnding(string.Format("DistFiles{0}Windows{0}DirectoryForTests{0}dummy.exe",
				Path.DirectorySeparatorChar)));
		}

		[Test]
		[Platform(Include = "Linux")]
		public void LocateExecutable_PlatformSpecificInDistFiles_LinuxWithoutExtension()
		{
			Assert.That(FileLocationUtilities.LocateExecutable("DirectoryForTests", "dummy.exe"),
				Is.StringEnding(string.Format("DistFiles{0}Linux{0}DirectoryForTests{0}dummy",
				Path.DirectorySeparatorChar)));
		}

		[Test]
		[Platform(Include = "Linux")]
		public void LocateExecutable_PlatformSpecificInDistFiles_Linux()
		{
			Assert.That(FileLocationUtilities.LocateExecutable("DirectoryForTests", "dummy2.exe"),
				Is.StringEnding(string.Format("DistFiles{0}Linux{0}DirectoryForTests{0}dummy2.exe",
				Path.DirectorySeparatorChar)));
		}

		[Test]
		public void LocateExecutable_NonexistingFile()
		{
			Assert.That(FileLocationUtilities.LocateExecutable(false, "dummy", "__nonexisting.exe"), Is.Null);
		}

		[Test]
		public void LocateExecutable_NonexistingFileThrows()
		{
			Assert.That(() => FileLocationUtilities.LocateExecutable("dummy", "__nonexisting.exe"),
				Throws.Exception.TypeOf<ApplicationException>());
		}
	}
}
