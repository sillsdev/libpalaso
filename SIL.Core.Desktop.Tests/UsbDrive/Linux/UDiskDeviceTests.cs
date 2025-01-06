#if !NET
using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SIL.UsbDrive.Linux;

namespace SIL.Tests.UsbDrive.Linux
{
	[Category("RequiresUSB")]
	[Category("SkipOnTeamCity")]
	[TestFixture]
	[Platform(Include = "Linux", Reason = "Linux specific test")]
	public class UDiskDeviceTests
	{
		private class EnvironmentForTest : IDisposable
		{
			public UDiskDevice GetUSBDevice()
			{
				// Just return the first USB device we find.  Throw if there isn't one.
				var uDisks = new UDisks();
				foreach (var device in uDisks.EnumerateDeviceOnInterface("usb"))
				{
					return new UDiskDevice(device);
				}
				Assert.Ignore("No USB drive available. Insert a USB drive for this test");
				throw new DriveNotFoundException("No USB drive available. Insert a USB drive for this test");
			}

			public void Dispose()
			{
			}
		}

		[Test]
		public void IsMounted_USBDrive_True()
		{
			using (var e = new EnvironmentForTest())
			{
				var uDiskDevice = e.GetUSBDevice();
				Assert.True(uDiskDevice.IsMounted);
			}
		}

		[Test]
		public void MountPaths_USBDrive_HasNonEmptyString()
		{
			using (var e = new EnvironmentForTest())
			{
				var uDiskDevice = e.GetUSBDevice();
				string[] mountPaths = uDiskDevice.MountPaths;
				Assert.That(mountPaths.Count, Is.GreaterThan(0));
				Assert.That(mountPaths[0], Does.Contain("/media/"));
			}
		}

		[Test]
		public void TotalSpace_USBDrive_GreaterThanZero()
		{
			using (var e = new EnvironmentForTest())
			{
				var uDiskDevice = e.GetUSBDevice();
				Assert.That(uDiskDevice.TotalSize, Is.GreaterThan(0));
			}
		}

		[Test]
		public void DriveConnectionInterface_USBDrive_USB()
		{
			using (var e = new EnvironmentForTest())
			{
				var uDiskDevice = e.GetUSBDevice();
				Assert.That(uDiskDevice.DriveConnectionInterface, Is.EqualTo(UDiskDevice.Interfaces.USB));
			}
		}

	}
}
#endif