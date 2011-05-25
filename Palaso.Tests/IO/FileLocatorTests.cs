using System.IO;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Reporting;
using Palaso.UsbDrive;

namespace Palaso.Tests.IO
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
		public void GetFromRegistryProgramThatOpensFileType_SendInvalidType_ReturnsNull()
		{
			Assert.IsNull(FileLocator.GetFromRegistryProgramThatOpensFileType(".blah"));
		}

		[Test]
		public void GetFromRegistryProgramThatOpensFileType_SendValidType_ReturnsProgramPath()
		{
			Assert.IsNotNull(FileLocator.GetFromRegistryProgramThatOpensFileType(".txt"));
		}

		[Test]
		public void GetFromRegistryProgramThatOpensFileType_SendExtensionWithoutPeriod_ReturnsProgramPath()
		{
			Assert.IsNotNull(FileLocator.GetFromRegistryProgramThatOpensFileType("txt"));
		}

		[Test]
		public void LocateInProgramFiles_SendInvalidProgramNoDeepSearch_ReturnsNull()
		{
			Assert.IsNull(FileLocator.LocateInProgramFiles("blah.exe", false));
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void LocateInProgramFiles_SendValidProgramNoDeepSearch_ReturnsNull()
		{
			Assert.IsNull(FileLocator.LocateInProgramFiles("msinfo32.exe", false));
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void LocateInProgramFiles_SendValidProgramDeepSearch_ReturnsProgramPath()
		{
			Assert.IsNotNull(FileLocator.LocateInProgramFiles("msinfo32.exe", true));
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void LocateInProgramFiles_SendValidProgramDeepSearch_SubFolderSpecified_ReturnsProgramPath()
		{
			Assert.IsNotNull(FileLocator.LocateInProgramFiles("msinfo32.exe", true, "Common Files"));
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void LocateInProgramFiles_SendInValidSubFolder_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => FileLocator.LocateInProgramFiles("msinfo32.exe", true, "!~@blah"));
		}

		//TODO: this could use lots more tests
	}
}
