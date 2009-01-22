using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Palaso.Backup
{
	[TestFixture]
	public class UsbDriveInfoTests
	{
		[Test]
		public void GetDrives_UsbDrivePluggedIn_ReturnsDrive()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual(1, usbDrives.Count);
		}

		[Test]
		public void GetDrives_UsbDrivewithAvailableSpacePluggedIn_ReturnsDrivesAvailableSpace()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.IsTrue(usbDrives[0].AvailableFreeSpace != 0);
		}

		[Test]
		public void GetDrives_UsbDrivePluggedIn_ReturnsWhetherDriveIsReady()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.IsTrue(usbDrives[0].IsReady);
		}

		[Test]
		public void GetDrives_UsbDriveNamedXXXPluggedIn_ReturnsDriveName()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual("E:\\", usbDrives[0].Name);
		}

		[Test]
		public void GetDrives_UsbDriveWithRootAtXPluggedIn_ReturnsRootDirectory()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual("E:\\", usbDrives[0].RootDirectory.ToString());
		}

		[Test]
		public void GetDrives_UsbDrivePluggedIn_ReturnsDrivesTotalSize()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.IsTrue(usbDrives[0].TotalSize != 0);
		}

		[Test]
		public void GetDrives_UsbDriveWithVolumeNameXXXPluggedIn_ReturnsDrivesVolumeLabel()
		{
			List<UsbDriveInfo> usbDrives = UsbDriveInfo.GetDrives();
			Assert.AreEqual("E:\\", usbDrives[0].Name);
		}
	}
}
