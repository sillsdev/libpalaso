#if !NET
using System;
using System.Linq;
using NUnit.Framework;
using SIL.UsbDrive.Linux;

namespace SIL.Tests.UsbDrive.Linux
{
	[TestFixture]
	[Category("SkipOnTeamCity")]
	[Platform(Include = "Linux", Reason = "Linux specific test")]
	public class UDisksTests
	{
		[Test, Ignore("not all systems have adapters")]
		public void EnumerateAdapters_HasSome()
		{
			var disks = new UDisks();
			var adapters = disks.Interface.EnumerateAdapters();
			Assert.Greater(adapters.Count(), 0);
		}

		[Test]
		public void EnumerateDevices_HasSome()
		{
			var disks = new UDisks();
			var devices = disks.Interface.EnumerateDevices();
			Assert.Greater(devices.Count(), 0);
		}

		[Test]
		public void EnumerateExpanders_HasNone()
		{
			var disks = new UDisks();
			var expanders = disks.Interface.EnumerateExpanders();
			Assert.AreEqual(0, expanders.Count());
		}

		[Test, Ignore("not all systems have ports")]
		public void EnumeratePorts_HasSome()
		{
			var disks = new UDisks();
			var ports = disks.Interface.EnumeratePorts();
			Assert.Greater(ports.Count(), 0);
		}

		[Test]
		public void EnumerateDeviceFiles_HasSome()
		{
			var disks = new UDisks();
			var deviceFiles = disks.Interface.EnumerateDeviceFiles();
			Assert.Greater(deviceFiles.Count(), 0);
		}

		[Test]
		[Category("RequiresUSB")]
		public void EnumerateUSB_HasOnlyUSBDevices()
		{
			var disks = new UDisks();
			var devices = disks.EnumerateDeviceOnInterface("usb");
			Assert.Greater(devices.Count(), 0);
			// Check that the devices don't exist on any interface other than usb
			foreach (var device in devices)
			{
				var uDiskDevice = new UDiskDevice(device);
				string iface = uDiskDevice.GetProperty("DriveConnectionInterface");
				Assert.AreEqual("usb", iface);
			}
		}

		[Test, Ignore("not all usb drives have partitions")]
		public void EnumerateUSB_HasOnlyPartitions()
		{
			var disks = new UDisks();
			var devices = disks.EnumerateDeviceOnInterface("usb");
			Assert.Greater(devices.Count(), 0);
			foreach (var device in devices)
			{
				var uDiskDevice = new UDiskDevice(device);
				Assert.AreEqual("True", uDiskDevice.GetProperty("DeviceIsPartition"),
					String.Format("Device {0} does not have a partition", device)
				);
			}
		}

	}
}
#endif