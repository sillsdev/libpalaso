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
			var usbDrives = UsbDriveInfo.GetDrives();
			if (usbDrives == null || usbDrives.Count < 1)
			{
				Assert.Ignore("Tests require USB drive");
				return;
			}
#if __MonoCS__
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
		public void GetDrives_1Drive_DrivesAreReturned()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(1, usbDrives.Count);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void IsReady_1Drive_True()
		{
			var drives = UsbDriveInfo.GetDrives();
			Assert.That(drives.Count, Is.GreaterThan(0));
			Assert.That(drives[0].IsReady, Is.True);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void RootDirectory_1Drive_MatchesMountPath()
		{
			var drives = UsbDriveInfo.GetDrives();
			// TODO The below is a platform specific expectation.  Fix for windows
			Assert.That(drives.Count, Is.GreaterThan(0));
			Assert.That(drives[0].RootDirectory.FullName, Is.StringContaining("/media/"));
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void TotalSize_1Drive_GreaterThan1000()
		{
			var drives = UsbDriveInfo.GetDrives();
			Assert.That(drives.Count, Is.GreaterThan(0));
			Assert.That(drives[0].TotalSize, Is.GreaterThan(1000));
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void GetDrives_2DrivesArePluggedIn_DrivesAreReturned()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(2, usbDrives.Count);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void GetDrives_3DrivesArePluggedIn_DrivesAreReturned()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(3, usbDrives.Count);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void TotalSize_2DrivesArePluggedIn_TheDrivesSizesAreCorrect()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.That(usbDrives.Count, Is.GreaterThan(1));
			Assert.AreEqual(_drive0.DriveSize, usbDrives[0].TotalSize);
			Assert.AreEqual(_drive1.DriveSize, usbDrives[1].TotalSize);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void RootDirectory_2DrivesArePluggedInAndReady_TheDrivesPathsCorrect()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.That(usbDrives.Count, Is.GreaterThan(1));
			Assert.AreEqual(_drive0.Path.FullName, usbDrives[0].RootDirectory.FullName);
			Assert.AreEqual(_drive1.Path.FullName, usbDrives[1].RootDirectory.FullName);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void IsReady_2DrivesAreMounted_ReturnsTrue()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.That(usbDrives.Count, Is.GreaterThan(1));
			Assert.IsTrue(usbDrives[0].IsReady);
			Assert.IsTrue(usbDrives[1].IsReady);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void IsReady_3DrivesAreMounted_ReturnsTrue()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.That(usbDrives.Count, Is.GreaterThan(2));
			Assert.IsTrue(usbDrives[0].IsReady);
			Assert.IsTrue(usbDrives[1].IsReady);
			Assert.IsTrue(usbDrives[2].IsReady);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void IsReady_2DrivesAreNotMounted_ReturnsFalse()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.That(usbDrives.Count, Is.GreaterThan(1));
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

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void VolumeLabel_1Drive_GivesInfo()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.That(usbDrives.Count, Is.GreaterThan(0));
			Assert.False(string.IsNullOrEmpty(usbDrives[0].VolumeLabel));
		}
	}
}
