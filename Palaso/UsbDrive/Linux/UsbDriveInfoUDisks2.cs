using System;
using System.Collections.Generic;
using System.IO;
using NDesk.DBus;
using System.Linq;
using System.Text;
using Palaso.Linq;
using org.freedesktop.DBus;

namespace Palaso.UsbDrive.Linux
{
	/// <summary>
	/// Accesses information about a particular filesystem on a USB Flash Drive, using UDisks2.
	/// </summary>
	internal class UsbDriveInfoUDisks2 : UsbDriveInfo
	{
		/// <summary>
		/// dbus object path of the block device
		/// </summary>
		private readonly string _blockDevicePath;

		private UsbDriveInfoUDisks2(string dbusBlockDevicePath)
		{
			_blockDevicePath = dbusBlockDevicePath;
		}

		/// <summary>
		/// Get block device object with information, corresponding to _blockDevicePath.
		/// </summary>
		private KeyValuePair<ObjectPath, IDictionary<string, IDictionary<string, object>>> BlockDevice
		{
			get
			{
				var disks = Bus.System.GetObject<ObjectManager>("org.freedesktop.UDisks2", new ObjectPath("/org/freedesktop/UDisks2"));
				var managedObjects = disks.GetManagedObjects();
				var blockDevice = managedObjects.First(obj => obj.Key.ToString() == _blockDevicePath);
				return blockDevice;
			}
		}

		public override bool IsReady
		{
			get
			{
				return !string.IsNullOrEmpty(RootDirectory.FullName);
			}
		}

		/// <summary>
		/// Mount point of USB Flash Drive.
		/// </summary>
		public override DirectoryInfo RootDirectory
		{
			get
			{
				var mountPoints = BlockDevice.Value["org.freedesktop.UDisks2.Filesystem"]["MountPoints"] as byte[][];
				if (mountPoints.Length == 0)
					return new DirectoryInfo(string.Empty);

				// The mountPoints byte array contains NULL terminators.
				var mountPoint = Encoding.UTF8.GetString(mountPoints[0]).TrimEnd('\0');

				return new DirectoryInfo(mountPoint);
			}
		}

		public override string VolumeLabel
		{
			get
			{
				return BlockDevice.Value["org.freedesktop.UDisks2.Block"]["IdLabel"].ToString();
			}
		}

		public override ulong TotalSize
		{
			get
			{
				return (ulong)BlockDevice.Value["org.freedesktop.UDisks2.Block"]["Size"];
			}
		}

		public override ulong AvailableFreeSpace
		{
			// UDisks2 itself does not appear to provide this information.
			// Could use another library if need to support this.
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Checks if UDisks2 is available to be used on this system.
		/// </summary>
		public static bool IsUDisks2Available
		{
			get
			{
				var names = Bus.System.GetObject<IBus>("org.freedesktop.DBus", new ObjectPath("/org/freedesktop/DBus")).ListActivatableNames();
				return names.Contains("org.freedesktop.UDisks2");
			}
		}

		/// <summary>
		/// Get a set of all mounted filesystems on the system's USB Flash Drives.
		/// </summary>
		public new static List<IUsbDriveInfo> GetDrives()
		{
			var drives = new List<IUsbDriveInfo>();
			drives = GetUsbBlockDevices()
				.Select(device => new UsbDriveInfoUDisks2(device) as IUsbDriveInfo).ToList();
			return drives;
		}

		/// <summary>
		/// Get set of mounted, removable filesystems connected over USB, represented as dbus object paths.
		/// </summary>
		private static IEnumerable<string> GetUsbBlockDevices()
		{
			var disks = Bus.System.GetObject<ObjectManager>("org.freedesktop.UDisks2", new ObjectPath("/org/freedesktop/UDisks2"));
			var managedObjects = disks.GetManagedObjects();
			var blockDevices =	managedObjects
				.Where(obj => obj.Key.ToString().StartsWith("/org/freedesktop/UDisks2/block_devices"))
				.Where(obj => obj.Value.ContainsKey("org.freedesktop.UDisks2.Block"))
				.Where(obj => obj.Value.ContainsKey("org.freedesktop.UDisks2.Filesystem"))
				.Where(obj => (obj.Value["org.freedesktop.UDisks2.Filesystem"]["MountPoints"] as byte[][]).Length > 0)
				.Where(obj =>
				{
					var drive = (ObjectPath)obj.Value["org.freedesktop.UDisks2.Block"]["Drive"];
					return managedObjects.ContainsKey(drive) &&
						(bool)managedObjects[drive]["org.freedesktop.UDisks2.Drive"]["Removable"] &&
						managedObjects[drive]["org.freedesktop.UDisks2.Drive"]["ConnectionBus"] as string == "usb";
				});

			var objectPaths = blockDevices.Select(device => device.Key.ToString());
			return objectPaths;
		}
	}
}
