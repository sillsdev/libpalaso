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
		private struct driveParamsForTests
		{
			public ulong driveSize;
			public DirectoryInfo path;
		}

		private driveParamsForTests drive0;
		private driveParamsForTests drive1;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
#if MONO
			drive0.driveSize = 256850432;
			drive0.path = new DirectoryInfo("/media/Kingston");
			drive1.driveSize = 1032724480;
			drive1.path = new DirectoryInfo("/media/PAXERIT");
#else
			drive0.driveSize = 256770048;
			drive0.path = new DirectoryInfo("E:\\");
			drive1.driveSize = 1032454144;
			drive1.path = new DirectoryInfo("J:\\");
#endif
		}

		[Test]
		[NUnit.Framework.Category("RequiresUSB")]
		public void GetDrives_1DrivesArePluggedIn_DrivesAreReturned()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(1, usbDrives.Count);
		}

		[Test]
		[NUnit.Framework.Category("RequiresUSB")]
		public void GetDrives_2DrivesArePluggedIn_DrivesAreReturned()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(2, usbDrives.Count);
		}

		[Test]
		[NUnit.Framework.Category("RequiresUSB")]
		public void TotalSize_2DrivesArePluggedIn_TheDrivesSizesAreCorrect()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(drive0.driveSize, usbDrives[0].TotalSize);
			Assert.AreEqual(drive1.driveSize, usbDrives[1].TotalSize);
		}

		[Test]
		[NUnit.Framework.Category("RequiresUSB")]
		public void RootDirectory_2DrivesArePluggedInAndReady_TheDrivesPathsCorrect()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(drive0.path.FullName, usbDrives[0].RootDirectory.FullName);
			Assert.AreEqual(drive1.path.FullName, usbDrives[1].RootDirectory.FullName);
		}

		[Test]
		[NUnit.Framework.Category("RequiresUSB")]
		public void IsReady_2DrivesAreMounted_ReturnsTrue()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.IsTrue(usbDrives[0].IsReady);
			Assert.IsTrue(usbDrives[1].IsReady);
		}

		[Test]
		[NUnit.Framework.Category("RequiresUSB")]
		public void IsReady_2DrivesAreNotMounted_ReturnsFalse()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.IsFalse(usbDrives[0].IsReady);
			Assert.IsFalse(usbDrives[1].IsReady);
		}

		[Test]
		[NUnit.Framework.Category("RequiresUSB")]
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