// Copyright (c) 2009-2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SIL.PlatformUtilities;
using SIL.UsbDrive;

namespace SIL.Tests.UsbDrive
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

		[OneTimeSetUp]
		public void TestFixtureSetup()
		{
			var usbDrives = UsbDriveInfo.GetDrives();
			if (usbDrives == null || usbDrives.Count < 1)
			{
				Assert.Ignore("Tests require USB drive");
			}

			if (!Platform.IsWindows)
			{
				_drive0.DriveSize = 256850432;
				_drive0.Path = new DirectoryInfo("/media/Kingston");
				_drive1.DriveSize = 1032724480;
				_drive1.Path = new DirectoryInfo("/media/PAXERIT");
			}
			else
			{
				_drive0.DriveSize = 256770048;
				_drive0.Path = new DirectoryInfo("E:\\");
				_drive1.DriveSize = 1032454144;
				_drive1.Path = new DirectoryInfo("J:\\");
			}
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		[Explicit("Run this test if you have exactly 1 USB drive plugged in")]
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
			if (drives.Count < 1)
				Assert.Ignore("Need at least 1 USB drive plugged in");
			Assert.That(drives[0].IsReady, Is.True);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void RootDirectory_1Drive_MatchesMountPath()
		{
			var drives = UsbDriveInfo.GetDrives();
			if (drives.Count < 1)
				Assert.Ignore("Need at least 1 USB drive plugged in");
			// Be sure it matches a valid linux or windows root path
			Assert.That(drives[0].RootDirectory.FullName, Does.Contain("/media/").Or.Match(@"^[A-Z]:\\$"));
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void TotalSize_1Drive_GreaterThan1000()
		{
			var drives = UsbDriveInfo.GetDrives();
			if (drives.Count < 1)
				Assert.Ignore("Need at least 1 USB drive plugged in");
			Assert.That(drives[0].TotalSize, Is.GreaterThan(1000));
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		[Explicit("Run this test if you have exactly 2 USB drives plugged in")]
		public void GetDrives_2DrivesArePluggedIn_DrivesAreReturned()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(2, usbDrives.Count);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		[Explicit("Run this test if you have exactly 3 USB drives plugged in")]
		public void GetDrives_3DrivesArePluggedIn_DrivesAreReturned()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(3, usbDrives.Count);
		}

		[Test]
		[Ignore("This test will only pass if the DriveParamsForTests in the Fixture setup match the test system.")]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void TotalSize_2DrivesArePluggedIn_TheDrivesSizesAreCorrect()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			if (usbDrives.Count < 2)
				Assert.Ignore("Need at least 2 USB drives plugged in");
			Assert.AreEqual(_drive0.DriveSize, usbDrives[0].TotalSize);
			Assert.AreEqual(_drive1.DriveSize, usbDrives[1].TotalSize);
		}

		[Test]
		[Ignore("This test will only pass if the DriveParamsForTests in the Fixture setup match the test system.")]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void RootDirectory_2DrivesArePluggedInAndReady_TheDrivesPathsCorrect()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			if (usbDrives.Count < 2)
				Assert.Ignore("Need at least 2 USB drives plugged in");
			Assert.AreEqual(_drive0.Path.FullName, usbDrives[0].RootDirectory.FullName);
			Assert.AreEqual(_drive1.Path.FullName, usbDrives[1].RootDirectory.FullName);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void IsReady_2DrivesAreMounted_ReturnsTrue()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			if (usbDrives.Count < 2)
				Assert.Ignore("Need at least 2 USB drives plugged in");
			Assert.IsTrue(usbDrives[0].IsReady);
			Assert.IsTrue(usbDrives[1].IsReady);
		}

		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void IsReady_3DrivesAreMounted_ReturnsTrue()
		{
			List<IUsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			if (usbDrives.Count < 3)
				Assert.Ignore("Need at least 3 USB drives plugged in");
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
			if (usbDrives.Count < 2)
				Assert.Ignore("Need at least 2 USB drives plugged in");
			if (Platform.IsWindows)
				Assert.Ignore("On windows unmounted USB drives are not listed.");
			Assert.IsFalse(usbDrives[0].IsReady);
			Assert.IsFalse(usbDrives[1].IsReady);
		}

#if !NET
		[Test]
		[Category("RequiresUSB")]
		[Category("SkipOnTeamCity")]
		public void RootDirectory_FirstDriveIsNotMounted_Throws()
		{
			var usbDrives = UsbDriveInfo.GetDrives();
			if (usbDrives.Count < 1)
				Assert.Ignore("Need at least 1 USB drive plugged in");
			if (Platform.IsWindows || SIL.UsbDrive.Linux.UsbDriveInfoUDisks2.IsUDisks2Available)
				Assert.Ignore("On windows, or on linux with udisks2 GetDrives() only returns mounted drives");

			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
					{
						string s = usbDrives[0].RootDirectory.FullName;
					}
				);
		}
#endif
	}
}
