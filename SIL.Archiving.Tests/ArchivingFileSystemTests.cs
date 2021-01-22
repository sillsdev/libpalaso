// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using NUnit.Framework;
using SIL.Archiving.Generic;
using SIL.IO;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	public class ArchivingFileSystemTests
	{
		[Test]
		[Platform(Include = "Linux", Reason = "Linux specific test")]
		public void CheckFolderChangesToAppData()
		{
			var resultFolder = ArchivingFileSystem.CheckFolder("/var/lib/SIL/test");
			if (Directory.Exists("/var/lib/SIL/test"))
			{
				Assert.Ignore("This system has write access to the /var/lib folder and doesn't execute the SUT");
			}
			else
			{
				Assert.AreEqual(
					Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
						"SIL", "test"),
					resultFolder);
				RobustIO.DeleteDirectory(resultFolder);
			}
		}
	}
}
