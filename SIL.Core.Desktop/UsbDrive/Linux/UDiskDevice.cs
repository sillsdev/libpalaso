#if !NETSTANDARD
using System;
using System.Collections.Generic;
using NDesk.DBus;
using DBusProperties = org.freedesktop.DBus.Properties;

namespace SIL.UsbDrive.Linux
{
	public class UDiskDevice
	{

		public enum Interfaces
		{
			ATA,
			USB
		}

		private readonly IUDiskDevice _device;
		private readonly DBusProperties _properties;

		public UDiskDevice(string deviceName)
		{
			DeviceName = deviceName;
			_device = Bus.System.GetObject<IUDiskDevice>("org.freedesktop.UDisks", new ObjectPath(deviceName));
			_properties = Bus.System.GetObject<DBusProperties>("org.freedesktop.UDisks", new ObjectPath(deviceName));
		}

		public IUDiskDevice Interface
		{
			get { return _device; }
		}

		public string DeviceName { get; private set; }

		public string GetProperty(string name)
		{
			return _properties.Get(String.Empty, name).ToString();
		}

		public IDictionary<string, object> GetAllProperties()
		{
			return _properties.GetAll(String.Empty);
		}

		public Interfaces DriveConnectionInterface
		{
			get
			{
				string iface = GetProperty("DriveConnectionInterface");
				switch (iface)
				{
					case "usb":
						return Interfaces.USB;
					case "ata":
						return Interfaces.ATA;
					default:
						throw new NotImplementedException(String.Format("Unknown drive interface {0}", iface));
				}
			}
		}

		public ulong TotalSize
		{
			get { return ulong.Parse(GetProperty("DeviceSize")); }
		}

		public string[] MountPaths
		{
			get { return _properties.Get(String.Empty, "DeviceMountPaths") as string[]; }
		}

		public bool IsMounted
		{
			get
			{
				var isMounted = GetProperty("DeviceIsMounted");
				return !String.IsNullOrEmpty(isMounted) && isMounted.ToLowerInvariant() == "true";
			}
		}

		public string VolumeLabel
		{
			get { return GetProperty("IdLabel"); }
		}

	}
}
#endif
