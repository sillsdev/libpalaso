using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.DBus;

namespace Palaso.UsbDrive.Linux
{
	public class UDisks
	{
		private readonly IUDisks _udisks;

		public UDisks()
		{
			_udisks = Bus.System.GetObject<IUDisks>("org.freedesktop.UDisks", new ObjectPath("/org/freedesktop/UDisks"));
		}

		public IUDisks Interface
		{
			get { return _udisks; }
		}

		public IEnumerable<string> EnumerateDeviceOnInterface(string onInterface)
		{
			var devices = Interface.EnumerateDevices();
			foreach (var device in devices)
			{
				var uDiskDevice = new UDiskDevice(device);
				string iface = uDiskDevice.GetProperty("DriveConnectionInterface");
				string partition = uDiskDevice.GetProperty("DeviceIsPartition");
				if (iface == onInterface && partition == "True")
				{
					yield return device;
				}
			}
		}
	}
}
