using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace Palaso.UsbDrive.Linux
{
	public class UDiskDevice
	{
		private readonly IUDiskDevice _device;
		private readonly Properties _properties;

		public UDiskDevice(string deviceName)
		{
			DeviceName = deviceName;
			_device = Bus.System.GetObject<IUDiskDevice>("org.freedesktop.UDisks", new ObjectPath(deviceName));
			_properties = Bus.System.GetObject<Properties>("org.freedesktop.UDisks", new ObjectPath(deviceName));
		}

		public IUDiskDevice Interface
		{
			get { return _device; }
		}

		public string DeviceName { get; set; }

		public string GetProperty(string name)
		{
			return _properties.Get(String.Empty, name).ToString();
		}

		public bool IsConnectedViaUSB
		{
			get { throw new NotImplementedException(); }
		}
	}
}
