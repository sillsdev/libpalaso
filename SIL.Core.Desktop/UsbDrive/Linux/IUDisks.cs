#if !NETSTANDARD
using System;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace SIL.UsbDrive.Linux
{
	public delegate void PortChangedHandler(string port);
	public delegate void PortRemovedHandler(string port);
	public delegate void PortAddedHandler(string port);
	public delegate void ExpanderChangedHandler(string o);
	public delegate void ExpanderRemovedHandler(string o);
	public delegate void ExpanderAddedHandler(string o);
	public delegate void AdapterChangedHandler(string o);
	public delegate void AdapterRemovedHandler(string o);
	public delegate void AdapterAddedHandler(string o);
	public delegate void DeviceJobChangedHandler(string o, bool b, string s, UInt32 u, bool b2, double d);
	public delegate void DeviceAddedHandler(ObjectPath o);
	public delegate void DeviceRemovedHandler(ObjectPath o);
	public delegate void DeviceChangedHandler(ObjectPath o);

	[Interface("org.freedesktop.UDisks")]
	public interface IUDisks : Introspectable
	{
		#region methods

		void Uninhibit(string cookie);

		[return: Argument("cookie")]
		string Inhibit();

		[return: Argument("device")]
		string LinuxMdCreate(string[] components, string level, UInt64 strip_size, string name, string[] options);

		[return: Argument("device")]
		string LinuxMdStart(string[] components, string[] options);

		[return: Argument("created_device")]
		string LinuxLvm2LVCreate(string group_uuid, string name, UInt64 size, UInt32 num_strips, UInt64 num_mirrors, string[] options, string fstype, string[] fsoptions);

		void LinuxLvm2LVRemove(string group_uuid, string uuid, string[] options);

		void LinuxLvm2LVStart(string group_uuid, string uuid, string[] options);

		void LinuxLvm2LVSetName(string group_uuid, string uuid, string[] options);

		void LinuxLvm2VGRemovePV(string vg_uuid, string pv_uuid, string[] options);

		void LinuxLvm2VGAddPV(string uuid, string physical_volume, string[] options);

		void LinuxLvm2VGSetName(string uuid, string name);

		void LinuxLvm2VGStop(string uuid, string name);

		void LinuxLvm2VGStart(string uuid, string[] options);

		void DriveUnsetAllSpindownTimeouts(string cookie);

		[return: Argument("cookie")]
		string DriveSetAllSpindownTimeouts(Int32 timeout_seconds, string[] options);

		void DriveUninhibitAllPolling(string cookie);

		[return: Argument("cookie")]
		string DriveInhibitAllPolling(string[] options);

		[return: Argument("device")]
		string FindDeviceByMajorMinor(Int64 device_major, Int64 device_minor);

		[return: Argument("device")]
		string FindDeviceByDeviceFile(string device_file);

		[return: Argument("device_files")]
		string[] EnumerateDeviceFiles();

		[return: Argument("devices")]
		string[] EnumerateDevices();

		[return: Argument("devices")]
		string[] EnumeratePorts();

		[return: Argument("devices")]
		string[] EnumerateExpanders();

		[return: Argument("devices")]
		string[] EnumerateAdapters();

		#endregion

		#region signals

		event PortChangedHandler PortChanged;
		event PortRemovedHandler PortRemoved;
		event PortAddedHandler PortAdded;
		event ExpanderChangedHandler ExpanderChanged;
		event ExpanderRemovedHandler ExpanderRemoved;
		event ExpanderAddedHandler ExpanderAdded;
		event AdapterChangedHandler AdapterChanged;
		event AdapterRemovedHandler AdapterRemoved;
		event AdapterAddedHandler AdapterAdded;
		event DeviceJobChangedHandler DeviceJobChanged;
		event DeviceAddedHandler DeviceAdded;
		event DeviceRemovedHandler DeviceRemoved;
		event DeviceChangedHandler DeviceChanged;

		#endregion

#if false // Not supported by dbus library yet
		#region properties

			// a(ssbbbubbbbbbbb) dbus lib doesn't support this correctly yet.
			object[] KnownFilesystems { get; }
			bool SupportsLuksDevices { get; }
			bool DaemonIsInhibited { get; }
			string DaemonVersion { get; }

		#endregion.
#endif
	}
}

#region interface xml dump
/*
 <!DOCTYPE node PUBLIC "-//freedesktop//DTD D-BUS Object Introspection 1.0//EN"
"http://www.freedesktop.org/standards/dbus/1.0/introspect.dtd">
<node>
  <interface name="org.freedesktop.DBus.Introspectable">
	<method name="Introspect">
	  <arg name="data" direction="out" type="s"/>
	</method>
  </interface>
  <interface name="org.freedesktop.DBus.Properties">
	<method name="Get">
	  <arg name="interface" direction="in" type="s"/>
	  <arg name="propname" direction="in" type="s"/>
	  <arg name="value" direction="out" type="v"/>
	</method>
	<method name="Set">
	  <arg name="interface" direction="in" type="s"/>
	  <arg name="propname" direction="in" type="s"/>
	  <arg name="value" direction="in" type="v"/>
	</method>
	<method name="GetAll">
	  <arg name="interface" direction="in" type="s"/>
	  <arg name="props" direction="out" type="a{sv}"/>
	</method>
  </interface>
  <interface name="org.freedesktop.UDisks">
	<method name="Uninhibit">
	  <arg name="cookie" type="s" direction="in"/>
	</method>
	<method name="Inhibit">
	  <arg name="cookie" type="s" direction="out"/>
	</method>
	<method name="LinuxMdCreate">
	  <arg name="components" type="ao" direction="in"/>
	  <arg name="level" type="s" direction="in"/>
	  <arg name="stripe_size" type="t" direction="in"/>
	  <arg name="name" type="s" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	  <arg name="device" type="o" direction="out"/>
	</method>
	<method name="LinuxMdStart">
	  <arg name="components" type="ao" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	  <arg name="device" type="o" direction="out"/>
	</method>
	<method name="LinuxLvm2LVCreate">
	  <arg name="group_uuid" type="s" direction="in"/>
	  <arg name="name" type="s" direction="in"/>
	  <arg name="size" type="t" direction="in"/>
	  <arg name="num_stripes" type="u" direction="in"/>
	  <arg name="stripe_size" type="t" direction="in"/>
	  <arg name="num_mirrors" type="u" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	  <arg name="fstype" type="s" direction="in"/>
	  <arg name="fsoptions" type="as" direction="in"/>
	  <arg name="created_device" type="o" direction="out"/>
	</method>
	<method name="LinuxLvm2LVRemove">
	  <arg name="group_uuid" type="s" direction="in"/>
	  <arg name="uuid" type="s" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="LinuxLvm2LVStart">
	  <arg name="group_uuid" type="s" direction="in"/>
	  <arg name="uuid" type="s" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="LinuxLvm2LVSetName">
	  <arg name="group_uuid" type="s" direction="in"/>
	  <arg name="uuid" type="s" direction="in"/>
	  <arg name="name" type="s" direction="in"/>
	</method>
	<method name="LinuxLvm2VGRemovePV">
	  <arg name="vg_uuid" type="s" direction="in"/>
	  <arg name="pv_uuid" type="s" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="LinuxLvm2VGAddPV">
	  <arg name="uuid" type="s" direction="in"/>
	  <arg name="physical_volume" type="o" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="LinuxLvm2VGSetName">
	  <arg name="uuid" type="s" direction="in"/>
	  <arg name="name" type="s" direction="in"/>
	</method>
	<method name="LinuxLvm2VGStop">
	  <arg name="uuid" type="s" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="LinuxLvm2VGStart">
	  <arg name="uuid" type="s" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="DriveUnsetAllSpindownTimeouts">
	  <arg name="cookie" type="s" direction="in"/>
	</method>
	<method name="DriveSetAllSpindownTimeouts">
	  <arg name="timeout_seconds" type="i" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	  <arg name="cookie" type="s" direction="out"/>
	</method>
	<method name="DriveUninhibitAllPolling">
	  <arg name="cookie" type="s" direction="in"/>
	</method>
	<method name="DriveInhibitAllPolling">
	  <arg name="options" type="as" direction="in"/>
	  <arg name="cookie" type="s" direction="out"/>
	</method>
	<method name="FindDeviceByMajorMinor">
	  <arg name="device_major" type="x" direction="in"/>
	  <arg name="device_minor" type="x" direction="in"/>
	  <arg name="device" type="o" direction="out"/>
	</method>
	<method name="FindDeviceByDeviceFile">
	  <arg name="device_file" type="s" direction="in"/>
	  <arg name="device" type="o" direction="out"/>
	</method>
	<method name="EnumerateDeviceFiles">
	  <arg name="device_files" type="as" direction="out"/>
	</method>
	<method name="EnumerateDevices">
	  <arg name="devices" type="ao" direction="out"/>
	</method>
	<method name="EnumeratePorts">
	  <arg name="devices" type="ao" direction="out"/>
	</method>
	<method name="EnumerateExpanders">
	  <arg name="devices" type="ao" direction="out"/>
	</method>
	<method name="EnumerateAdapters">
	  <arg name="devices" type="ao" direction="out"/>
	</method>
	<signal name="PortChanged">
	  <arg type="o"/>
	</signal>
	<signal name="PortRemoved">
	  <arg type="o"/>
	</signal>
	<signal name="PortAdded">
	  <arg type="o"/>
	</signal>
	<signal name="ExpanderChanged">
	  <arg type="o"/>
	</signal>
	<signal name="ExpanderRemoved">
	  <arg type="o"/>
	</signal>
	<signal name="ExpanderAdded">
	  <arg type="o"/>
	</signal>
	<signal name="AdapterChanged">
	  <arg type="o"/>
	</signal>
	<signal name="AdapterRemoved">
	  <arg type="o"/>
	</signal>
	<signal name="AdapterAdded">
	  <arg type="o"/>
	</signal>
	<signal name="DeviceJobChanged">
	  <arg type="o"/>
	  <arg type="b"/>
	  <arg type="s"/>
	  <arg type="u"/>
	  <arg type="b"/>
	  <arg type="d"/>
	</signal>
	<signal name="DeviceChanged">
	  <arg type="o"/>
	</signal>
	<signal name="DeviceRemoved">
	  <arg type="o"/>
	</signal>
	<signal name="DeviceAdded">
	  <arg type="o"/>
	</signal>
	<property name="KnownFilesystems" type="a(ssbbbubbbbbbbb)" access="read"/>
	<property name="SupportsLuksDevices" type="b" access="read"/>
	<property name="DaemonIsInhibited" type="b" access="read"/>
	<property name="DaemonVersion" type="s" access="read"/>
  </interface>
  <node name="adapters"/>
  <node name="devices"/>
</node>
*/
#endregion
#endif
