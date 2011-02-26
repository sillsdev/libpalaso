using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Palaso.UsbDrive.Linux;

namespace Palaso.Tests.UsbDrive.Linux
{
	[TestFixture]
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
				Assert.That(mountPaths.Count(), Is.GreaterThan(0));
				Assert.That(mountPaths[0], Is.StringContaining("/media/"));
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
