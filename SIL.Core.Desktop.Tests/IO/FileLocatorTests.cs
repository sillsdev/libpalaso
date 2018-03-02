using NUnit.Framework;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Reporting;
using System;
using System.IO;

namespace SIL.Tests.IO
{
	[TestFixture]
	public class FileLocatorTests
	{
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
	}
}
