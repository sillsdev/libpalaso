using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Palaso.UsbDrive;

namespace Palaso.Tests.UsbDrive
{
	[TestFixture]
	public class UsbDeviceInfoTests
	{
		private struct DriveParamsForTests
		{
			public ulong DriveSize;
			public DirectoryInfo Path;
		}

		private DriveParamsForTests _drive0;
		private DriveParamsForTests _drive1;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
#if MONO
			_drive0.DriveSize = 256850432;
			_drive0.Path = new DirectoryInfo("/media/Kingston");
			_drive1.DriveSize = 1032724480;
			_drive1.Path = new DirectoryInfo("/media/PAXERIT");
#else
			_drive0.DriveSize = 256770048;
			_drive0.Path = new DirectoryInfo("E:\\");
			_drive1.DriveSize = 1032454144;
			_drive1.Path = new DirectoryInfo("J:\\");
#endif
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void GetDrives_1DrivesArePluggedIn_DrivesAreReturned()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(1, usbDrives.Count);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void GetDrives_2DrivesArePluggedIn_DrivesAreReturned()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(2, usbDrives.Count);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void TotalSize_2DrivesArePluggedIn_TheDrivesSizesAreCorrect()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(_drive0.DriveSize, usbDrives[0].TotalSize);
			Assert.AreEqual(_drive1.DriveSize, usbDrives[1].TotalSize);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void RootDirectory_2DrivesArePluggedInAndReady_TheDrivesPathsCorrect()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(_drive0.Path.FullName, usbDrives[0].RootDirectory.FullName);
			Assert.AreEqual(_drive1.Path.FullName, usbDrives[1].RootDirectory.FullName);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void IsReady_2DrivesAreMounted_ReturnsTrue()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.IsTrue(usbDrives[0].IsReady);
			Assert.IsTrue(usbDrives[1].IsReady);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void IsReady_2DrivesAreNotMounted_ReturnsFalse()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.IsFalse(usbDrives[0].IsReady);
			Assert.IsFalse(usbDrives[1].IsReady);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void RootDirectory_2DrivesAreNotMounted_Throws()
		{
			var usbDrives = UsbDriveInfo.GetDrives();
			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
					{
						string s = usbDrives[0].RootDirectory.FullName;
					}
			);
		}
	}
}