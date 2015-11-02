using System;
using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.Tests.IO
{
	[TestFixture]
	public class FileLocatorTests
	{
		[Test]
		public void GetFileDistributedWithApplication_MultipleParts_FindsCorrectly()
		{
			var path = FileLocator.GetFileDistributedWithApplication("DirectoryForTests", "SampleFileForTests.txt");
			Assert.That(File.Exists(path));
		}
		[Test]
		public void GetDirectoryDistributedWithApplication_MultipleParts_FindsCorrectly()
		{
			var path = FileLocator.GetDirectoryDistributedWithApplication("DirectoryForTests");
			Assert.That(Directory.Exists(path));
		}

		[Test]
		public void DirectoryOfApplicationOrSolution_OnDevMachine_FindsOutputDirectory()
		{
			var path = FileLocator.DirectoryOfTheApplicationExecutable;
			Assert.That(Directory.Exists(path));
			Assert.That(path.Contains("output"));
		}

		[Test]
		public void LocateFile_ErrorMessageProvidedAndFileNoteFound_WouldShowErrorReport()
		{
			var locator = new FileLocator(new[] {"bogus"});
			ErrorReport.IsOkToInteractWithUser = false;
			Assert.Throws<ErrorReport.ProblemNotificationSentToUserException>(() =>
																				{
																  locator.LocateFile("foo.txt", "booo hooo");
																				});
		}

		[Test]
		public void LocateFile_FileNoteFound_ReturnsEmptyString()
		{
			var locator = new FileLocator(new[] { "bogus" });
			ErrorReport.IsOkToInteractWithUser = false;
			Assert.IsEmpty(locator.LocateFile("foo.txt"));
		}

		[Test]
		[Platform(Exclude="Unix")]
		[Category("KnownMonoIssue")]
		public void GetFromRegistryProgramThatOpensFileType_SendInvalidType_ReturnsNull()
		{
			Assert.IsNull(FileLocator.GetFromRegistryProgramThatOpensFileType(".blah"));
		}

		[Test]
		[Platform(Exclude="Unix")]
		[Category("KnownMonoIssue")]
		public void GetFromRegistryProgramThatOpensFileType_SendValidType_ReturnsProgramPath()
		{
			Assert.IsNotNull(FileLocator.GetFromRegistryProgramThatOpensFileType(".txt"));
		}

		[Test]
		[Platform(Exclude="Unix")]
		[Category("KnownMonoIssue")]
		public void GetFromRegistryProgramThatOpensFileType_SendExtensionWithoutPeriod_ReturnsProgramPath()
		{
			Assert.IsNotNull(FileLocator.GetFromRegistryProgramThatOpensFileType("txt"));
		}

		[Test]
		public void LocateInProgramFiles_SendInvalidProgramNoDeepSearch_ReturnsNull()
		{
			Assert.IsNull(FileLocator.LocateInProgramFiles("blah.exe", false));
		}

		// 12 SEP 2013, Phil Hopper: This test not valid on Mono.
		[Test]
		[Platform(Exclude="Unix")]
		[Category("SkipOnTeamCity;KnownMonoIssue")]
		public void LocateInProgramFiles_SendValidProgramNoDeepSearch_ReturnsNull()
		{
			Assert.IsNull(FileLocator.LocateInProgramFiles("msinfo32.exe", false));
		}

		[Test]
		public void LocateInProgramFiles_SendValidProgramDeepSearch_ReturnsProgramPath()
		{
			var findFile = (Platform.IsMono ? "bash" : "msinfo32.exe");
			Assert.IsNotNull(FileLocator.LocateInProgramFiles(findFile, true));
		}

		[Test]
		public void LocateInProgramFiles_SendValidProgramDeepSearch_SubFolderSpecified_ReturnsProgramPath()
		{
			var findFile = (Platform.IsMono ? "bash" : "msinfo32.exe");

			// this will work on Mono because it ignores the subFoldersToSearch parameter
			Assert.IsNotNull(FileLocator.LocateInProgramFiles(findFile, true, "Common Files"));
		}

		[Test]
		public void LocateInProgramFiles_SendInValidSubFolder_DoesNotThrow()
		{
			var findFile = (Platform.IsMono ? "bash" : "msinfo32.exe");
			Assert.DoesNotThrow(() => FileLocator.LocateInProgramFiles(findFile, true, "!~@blah"));
		}

		//TODO: this could use lots more tests

		[Test]
		public void LocateExecutable_DistFiles()
		{
			Assert.That(FileLocator.LocateExecutable("DirectoryForTests", "SampleExecutable.exe"),
				Is.StringEnding(string.Format("DistFiles{0}DirectoryForTests{0}SampleExecutable.exe",
					Path.DirectorySeparatorChar)));
		}

		[Test]
		[Platform(Exclude="Linux")]
		public void LocateExecutable_PlatformSpecificInDistFiles_Windows()
		{
			Assert.That(FileLocator.LocateExecutable("DirectoryForTests", "dummy.exe"),
				Is.StringEnding(string.Format("DistFiles{0}Windows{0}DirectoryForTests{0}dummy.exe",
				Path.DirectorySeparatorChar)));
		}

		[Test]
		[Platform(Include="Linux")]
		public void LocateExecutable_PlatformSpecificInDistFiles_LinuxWithoutExtension()
		{
			Assert.That(FileLocator.LocateExecutable("DirectoryForTests", "dummy.exe"),
				Is.StringEnding(string.Format("DistFiles{0}Linux{0}DirectoryForTests{0}dummy",
				Path.DirectorySeparatorChar)));
		}

		[Test]
		[Platform(Include="Linux")]
		public void LocateExecutable_PlatformSpecificInDistFiles_Linux()
		{
			Assert.That(FileLocator.LocateExecutable("DirectoryForTests", "dummy2.exe"),
				Is.StringEnding(string.Format("DistFiles{0}Linux{0}DirectoryForTests{0}dummy2.exe",
				Path.DirectorySeparatorChar)));
		}

		[Test]
		public void LocateExecutable_NonexistingFile()
		{
			Assert.That(FileLocator.LocateExecutable(false, "dummy", "__nonexisting.exe"), Is.Null);
		}

		[Test]
		public void LocateExecutable_NonexistingFileThrows()
		{
			Assert.That(() => FileLocator.LocateExecutable("dummy", "__nonexisting.exe"),
				Throws.Exception.TypeOf<ApplicationException>());
		}

	}
}
