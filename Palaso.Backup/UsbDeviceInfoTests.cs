using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Palaso.Backup
{
	//In order for these tests to be relevant you must attach 2 usb drives to your computer and adjust
	//their expected size and path in the TestFixtureSetup() method
	[TestFixture]
	[Ignore("Hardware specific")]
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
			drive0.driveSize = 256770048;
			drive0.path = new DirectoryInfo("E:\\");
			drive1.driveSize = 1032454144;
			drive1.path = new DirectoryInfo("J:\\");
		}

		[Test]
		public void GetDrives_2DrivesArePluggedIn_DrivesAreReturned()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(2, usbDrives.Count);
		}

		[Test]
		public void TotalSize_2DrivesArePluggedIn_TheDrivesSizesAreCorrect()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(drive0.driveSize, usbDrives[0].TotalSize);
			Assert.AreEqual(drive1.driveSize, usbDrives[1].TotalSize);
		}
		[Test]
		public void TotalSize_2DrivesArePluggedIn_TheDrivesSizesPathsCorrect()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(drive0.path.FullName, usbDrives[0].RootDirectory.FullName);
			Assert.AreEqual(drive1.path.FullName, usbDrives[1].RootDirectory.FullName);
		}
	}
}
