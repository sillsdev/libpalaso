﻿using System;
using System.Collections.Generic;
using SIL.Reporting;
using NDesk.DBus;

namespace SIL.UsbDrive.Linux
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

		/// <summary>
		/// Enumerate the devices on the given interface.
		/// </summary>
		/// <remarks>
		/// DBus is a bit flakey and subject to random timing problems.  We need to
		/// ignore any exceptions that it might throw.  This method is typically
		/// called on a timer loop so that any information we lose will probably
		/// be available in a few seconds.  (See https://jira.sil.org/browse/WS-226
		/// and a number of other JIRA issues that record exceptions at this
		/// point in the code.)
		/// Note that yield cannot appear in a try body of a try-catch expression.
		/// </remarks>
		public IEnumerable<string> EnumerateDeviceOnInterface(string onInterface)
		{
			IEnumerable<string> devices;
			try
			{
				devices = Interface.EnumerateDevices();
			}
			catch (Exception ex)
			{
				Logger.Init();
				Logger.WriteEvent("Ignoring exception from DBus while enumerating devices: {0}",
					ex.Message);
				devices = new List<string>();
			}
			foreach (var device in devices)
			{
				string iface = null;
				bool isMounted = false;
				try
				{
					var uDiskDevice = new UDiskDevice(device);
					iface = uDiskDevice.GetProperty("DriveConnectionInterface");
					isMounted = uDiskDevice.IsMounted;
				}
				catch (Exception ex)
				{
					Logger.Init();
					Logger.WriteEvent("Ignoring exception from DBus while scanning for {0} devices: {1}",
						onInterface, ex.Message);
					continue;
				}
				if (iface == onInterface && isMounted)
				{
					yield return device;
				}
			}
		}
	}
}
