// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using SIL.IO;
using SIL.Reporting;

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
		public void GetDefaultProgramForFileType_SendInvalidType_ReturnsNull()
		{
			Assert.IsNull(FileLocator.GetDefaultProgramForFileType(".blah"));
		}

		[Test]
		public void GetDefaultProgramForFileType_SendValidType_ReturnsProgramPath()
		{
			Assert.IsNotNull(FileLocator.GetDefaultProgramForFileType(".txt"));
		}

		[Test]
		public void GetDefaultProgramForFileType_SendExtensionWithoutPeriod_ReturnsProgramPath()
		{
			Assert.IsNotNull(FileLocator.GetDefaultProgramForFileType("txt"));
		}
	}
}
