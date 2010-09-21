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
			var path = FileLocator.GetFileDistributedWithApplication("lib", "common", "nunit.framework.dll");
			Assert.That(path.Contains("nunit"));
			Assert.That(File.Exists(path));
		}
		[Test]
		public void GetDirectoryDistributedWithApplication_MultipleParts_FindsCorrectly()
		{
			var path = FileLocator.GetDirectoryDistributedWithApplication("lib", "common");
			Assert.That(path.Contains("common"));
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

		//TODO: this could use lots more tests
	}
}
