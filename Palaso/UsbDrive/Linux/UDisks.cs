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
				if (iface == onInterface && uDiskDevice.IsMounted)
				{
					yield return device;
				}
			}
			// If Bus.System is not closed, the program hangs when it ends, waiting for
			// the associated thread to quit.  It appears to properly reopen Bus.System
			// if we try to use it again after closing it.
			// And calling Close() here appears to work okay in conjunction with the
			// yield return above.
			Bus.System.Close();
		}
	}
}
