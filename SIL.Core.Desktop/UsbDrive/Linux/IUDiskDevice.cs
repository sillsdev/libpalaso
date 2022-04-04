#if !NETSTANDARD
using System;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace SIL.UsbDrive.Linux
{
	public delegate void JobChangedHandler(bool b, string s, UInt32 u, bool b2, double d);
	public delegate void ChangedHandler();

	[Interface("org.freedesktop.UDisks.Device")]
	public interface IUDiskDevice : Introspectable
	{
		#region methods

		[return: Argument("access_time_results")]
		object[] DriveBenchmark(bool do_write_benchmark, string[] options, out object[] read_transfer_rate_results, out object[] write_transfer_rate_results);

		void DriveAtaSmartInitiateSelftest(string test, string[] options);

		void DriveAtaSmartRefreshData(string cookie);

		void DriveUnsetSpindownTimeout(string cookie);

		[return: Argument("cookie")]
		string DriveSetSpindownTimeout(Int32 timeout_seconds, string[] options);

		void DriveDetach(string[] options);

		void DriveEject(string[] options);

		void DrivePollMedia();

		void DriveUninhibitPolling(string cookie);

		[return: Argument("cookie")]
		string DriveInhibitPolling(string[] options);

		[return: Argument("number_of_errors")]
		UInt64 LinuxMdCheck(string[] options);

		void LinuxLvm2LVStop(string[] options);

		void LinuxMdStop(string[] options);

		void LinuxMdRemoveComponent(ObjectPath component, string[] options);

		void LinuxMdExpand(ObjectPath[] components, string[] options);

		void LinuxMdAddSpare(ObjectPath component, string[] options);

		void LuksChangePassphrase(string current_passphrase, string new_passphrase);

		void LuksLock(string[] options);

		[return: Argument("cleartext_device")]
		ObjectPath LuksUnlock(string passphrase, string[] options);

		[return: Argument("processes")]
		object[] FilesystemListOpenFiles();

		[return: Argument("is_clean")]
		bool FilesystemCheck(string[] options);

		void FilesystemUnmount(string[] options);

		[return: Argument("is_clean")]
		string FilesystemMount(string filesystem_type, string[] options);

		void FilesystemSetLabel(string new_label);

		void FilesystemCreate(string fstype, string[] options);

		void PartitionModify(string type, string label, string[] flags);

		[return: Argument("created_device")]
		ObjectPath PartitionCreate(UInt64 offset, UInt64 size, string type, string label, string[] flags, string[] options, string fstype, string[] fsoptions);

		void PartitionDelete(string[] options);

		void PartitionTableCreate(string scheme, string[] options);

		void JobCancel();

		#endregion

		#region signals

		event JobChangedHandler JobChanged;
		event ChangedHandler Changed;

		#endregion

#if false // Not supported by dbus library yet
		#region properties

		string LinuxLoopFilename { get; }
		string LinuxDmmpParameters { get; }
		ObjectPath[] LinuxDmmpSlaves { get; }
		string LinuxDmmpName { get; }
		ObjectPath LinuxDmmpComponentHolder { get; }
		string LinuxLvm2LVGroupUuid { get; }
		string LinuxLvm2LVGroupName { get; }
		string LinuxLvm2LVUuid { get; }
		string LinuxLvm2LVName { get; }
		string[] LinuxLvm2PVGroupLogicalVolumes { get; }
		string[] LinuxLvm2PVGroupPhysicalVolumes { get; }
		UInt64 LinuxLvm2PVGroupExtentSize { get; }
		UInt64 LinuxLvm2PVGroupSequenceNumber { get; }
		UInt64 LinuxLvm2PVGroupUnallocatedSize { get; }
		UInt64 LinuxLvm2PVGroupSize { get; }
		string LinuxLvm2PVGroupUuid { get; }
		string  LinuxLvm2PVGroupName { get; }
		UInt32 LinuxLvm2PVNumMetadataAreas { get; }
		string LinuxLvm2PVUuid { get; }
		UInt64 LinuxMdSyncSpeed { get; }
		double LinuxMdSyncPercentage { get; }
		string LinuxMdSyncAction { get; }
		bool LinuxMdIsDegraded { get; }
		ObjectPath[] LinuxMdSlaves { get; }
		string LinuxMdVersion { get; }
		Int32 LinuxMdNumRaidDevices { get; }
		string LinuxMdName { get; }
		string LinuxMdHomeHost { get; }
		string LinuxMdUuid { get; }
		string LinuxMdLevel { get; }
		string LinuxMdState { get; }
		string[] LinuxMdComponentState { get; }
		ObjectPath LinuxMdComponentHolder { get; }
		string LinuxMdComponentVersion { get; }
		string LinuxMdComponentHomeHost { get; }
		string LinuxMdComponentName { get; }
		string LinuxMdComponentUuid { get; }
		Int32 LinuxMdComponentNumRaidDevices { get; }
		Int32 LinuxMdComponentPosition { get; }
		string LinuxMdComponentLevel { get; }
		byte[] DriveAtaSmartBlob { get; }
		string DriveAtaSmartStatus { get; }
		UInt64 DriveAtaSmartTimeCollected { get; }
		bool DriveAtaSmartIsAvailable { get; }
		UInt32 OpticalDiscNumSessions { get; }
		UInt32 OpticalDiscNumAudioTracks { get; }
		UInt32 OpticalDiscNumTracks { get; }
		bool OpticalDiscIsClosed { get; }
		bool OpticalDiscIsAppendable { get; }
		bool OpticalDiscIsBlank { get; }
		ObjectPath[] DriveSimilarDevices { get; }
		ObjectPath[] DrivePorts { get; }
		ObjectPath DriveAdapter { get; }
		bool DriveIsRotational { get; }
		bool DriveCanSpindown { get; }
		bool DriveCanDetach { get; }
		bool DriveIsMediaEjectable { get; }
		string DriveMedia { get; }
		string[] DriveMediaCompatibility { get; }
		UInt64 DriveConnectionSpeed { get; }
		string DriveConnectionInterface { get; }
		string DriveWriteCache { get; }
		UInt32 DriveRotationRate { get; }
		string DriveWwn { get; }
		string DriveSerial { get; }
		string DriveRevision { get; }
		string DriveModel { get; }
		string DriveVendor { get; }
		Int32 PartitionTableCount { get; }
		string PartitionTableScheme { get; }
		UInt64 PartitionAlignmentOffset { get; }
		UInt64 PartitionSize { get; }
		UInt64 PartitionOffset { get; }
		Int32 PartitionNumber { get; }
		string[] PartitionFlags { get; }
		string PartitionUuid { get; }
		string PartitionLabel { get; }
		string PartitionType { get; }
		string PartitionScheme { get; }
		ObjectPath PartitionSlave { get; }
		UInt32 LuksCleartextUnlockedByUid { get; }
		ObjectPath LuksCleartextSlave { get; }
		ObjectPath LuksHolder { get; }
		string IdLabel { get; }
		string IdUuid { get; }
		string IdVersion { get; }
		string IdType { get; }
		string IdUsage { get; }
		double JobPercentage { get; }
		bool JobIsCancellable { get; }
		UInt32 JobInitiatedByUid { get; }
		string JobId { get; }
		bool JobInProgress { get; }
		string DevicePresentationIconName { get; }
		string DevicePresentationName { get; }
		bool DevicePresentationNopolicy { get; }
		bool DevicePresentationHide { get; }
		UInt64 DeviceBlockSize { get; }
		UInt64 DeviceSize { get; }
		bool DeviceIsLinuxLoop { get; }
		bool DeviceIsLinuxDmmp { get; }
		bool DeviceIsLinuxDmmpComponent { get; }
		bool DeviceIsLinuxLvm2PV { get; }
		bool DeviceIsLinuxLvm2LV { get; }
		bool DeviceIsLinuxMd { get; }
		bool DeviceIsLinuxMdComponent { get; }
		bool DeviceIsLuksCleartext { get; }
		bool DeviceIsLuks { get; }
		UInt32 DeviceMountedByUid { get; }
		string[] DeviceMountPaths { get; }
		bool DeviceIsMounted { get; }
		bool DeviceIsOpticalDisc { get; }
		bool DeviceIsDrive { get; }
		bool DeviceIsReadOnly { get; }
		bool DeviceIsMediaChangeDetectionInhibited { get; }
		bool DeviceIsMediaChangeDetectionInhibitable { get; }
		bool DeviceIsMediaChangeDetectionPolling { get; }
		bool DeviceIsMediaChangeDetected { get; }
		bool DeviceIsMediaAvailable { get; }
		bool DeviceIsRemovable { get; }
		bool DeviceIsPartitionTable { get; }
		bool DeviceIsPartition { get; }
		bool DeviceIsSystemInternal { get; }
		string[] DeviceFileByPath { get; }
		string[] DeviceFileById { get; }
		string DeviceFilePresentation { get; }
		string DeviceFile { get; }
		Int64 DeviceMinor { get; }
		Int64 DeviceMajor { get; }
		UInt64 DeviceMediaDetectionTime { get; }
		UInt64 DeviceDetectionTime { get; }
		string NativePath { get; }

		#endregion
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
  <interface name="org.freedesktop.UDisks.Device">
	<method name="DriveBenchmark">
	  <arg name="do_write_benchmark" type="b" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	  <arg name="read_transfer_rate_results" type="a(td)" direction="out"/>
	  <arg name="write_transfer_rate_results" type="a(td)" direction="out"/>
	  <arg name="access_time_results" type="a(td)" direction="out"/>
	</method>
	<method name="DriveAtaSmartInitiateSelftest">
	  <arg name="test" type="s" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="DriveAtaSmartRefreshData">
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="DriveUnsetSpindownTimeout">
	  <arg name="cookie" type="s" direction="in"/>
	</method>
	<method name="DriveSetSpindownTimeout">
	  <arg name="timeout_seconds" type="i" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	  <arg name="cookie" type="s" direction="out"/>
	</method>
	<method name="DriveDetach">
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="DriveEject">
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="DrivePollMedia">
	</method>
	<method name="DriveUninhibitPolling">
	  <arg name="cookie" type="s" direction="in"/>
	</method>
	<method name="DriveInhibitPolling">
	  <arg name="options" type="as" direction="in"/>
	  <arg name="cookie" type="s" direction="out"/>
	</method>
	<method name="LinuxMdCheck">
	  <arg name="options" type="as" direction="in"/>
	  <arg name="number_of_errors" type="t" direction="out"/>
	</method>
	<method name="LinuxLvm2LVStop">
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="LinuxMdStop">
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="LinuxMdRemoveComponent">
	  <arg name="component" type="o" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="LinuxMdExpand">
	  <arg name="components" type="ao" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="LinuxMdAddSpare">
	  <arg name="component" type="o" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="LuksChangePassphrase">
	  <arg name="current_passphrase" type="s" direction="in"/>
	  <arg name="new_passphrase" type="s" direction="in"/>
	</method>
	<method name="LuksLock">
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="LuksUnlock">
	  <arg name="passphrase" type="s" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	  <arg name="cleartext_device" type="o" direction="out"/>
	</method>
	<method name="FilesystemListOpenFiles">
	  <arg name="processes" type="a(uus)" direction="out"/>
	</method>
	<method name="FilesystemCheck">
	  <arg name="options" type="as" direction="in"/>
	  <arg name="is_clean" type="b" direction="out"/>
	</method>
	<method name="FilesystemUnmount">
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="FilesystemMount">
	  <arg name="filesystem_type" type="s" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	  <arg name="mount_path" type="s" direction="out"/>
	</method>
	<method name="FilesystemSetLabel">
	  <arg name="new_label" type="s" direction="in"/>
	</method>
	<method name="FilesystemCreate">
	  <arg name="fstype" type="s" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="PartitionModify">
	  <arg name="type" type="s" direction="in"/>
	  <arg name="label" type="s" direction="in"/>
	  <arg name="flags" type="as" direction="in"/>
	</method>
	<method name="PartitionCreate">
	  <arg name="offset" type="t" direction="in"/>
	  <arg name="size" type="t" direction="in"/>
	  <arg name="type" type="s" direction="in"/>
	  <arg name="label" type="s" direction="in"/>
	  <arg name="flags" type="as" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	  <arg name="fstype" type="s" direction="in"/>
	  <arg name="fsoptions" type="as" direction="in"/>
	  <arg name="created_device" type="o" direction="out"/>
	</method>
	<method name="PartitionDelete">
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="PartitionTableCreate">
	  <arg name="scheme" type="s" direction="in"/>
	  <arg name="options" type="as" direction="in"/>
	</method>
	<method name="JobCancel">
	</method>
	<signal name="JobChanged">
	  <arg type="b"/>
	  <arg type="s"/>
	  <arg type="u"/>
	  <arg type="b"/>
	  <arg type="d"/>
	</signal>
	<signal name="Changed">
	</signal>
	<property name="LinuxLoopFilename" type="s" access="read"/>
	<property name="LinuxDmmpParameters" type="s" access="read"/>
	<property name="LinuxDmmpSlaves" type="ao" access="read"/>
	<property name="LinuxDmmpName" type="s" access="read"/>
	<property name="LinuxDmmpComponentHolder" type="o" access="read"/>
	<property name="LinuxLvm2LVGroupUuid" type="s" access="read"/>
	<property name="LinuxLvm2LVGroupName" type="s" access="read"/>
	<property name="LinuxLvm2LVUuid" type="s" access="read"/>
	<property name="LinuxLvm2LVName" type="s" access="read"/>
	<property name="LinuxLvm2PVGroupLogicalVolumes" type="as" access="read"/>
	<property name="LinuxLvm2PVGroupPhysicalVolumes" type="as" access="read"/>
	<property name="LinuxLvm2PVGroupExtentSize" type="t" access="read"/>
	<property name="LinuxLvm2PVGroupSequenceNumber" type="t" access="read"/>
	<property name="LinuxLvm2PVGroupUnallocatedSize" type="t" access="read"/>
	<property name="LinuxLvm2PVGroupSize" type="t" access="read"/>
	<property name="LinuxLvm2PVGroupUuid" type="s" access="read"/>
	<property name="LinuxLvm2PVGroupName" type="s" access="read"/>
	<property name="LinuxLvm2PVNumMetadataAreas" type="u" access="read"/>
	<property name="LinuxLvm2PVUuid" type="s" access="read"/>
	<property name="LinuxMdSyncSpeed" type="t" access="read"/>
	<property name="LinuxMdSyncPercentage" type="d" access="read"/>
	<property name="LinuxMdSyncAction" type="s" access="read"/>
	<property name="LinuxMdIsDegraded" type="b" access="read"/>
	<property name="LinuxMdSlaves" type="ao" access="read"/>
	<property name="LinuxMdVersion" type="s" access="read"/>
	<property name="LinuxMdNumRaidDevices" type="i" access="read"/>
	<property name="LinuxMdName" type="s" access="read"/>
	<property name="LinuxMdHomeHost" type="s" access="read"/>
	<property name="LinuxMdUuid" type="s" access="read"/>
	<property name="LinuxMdLevel" type="s" access="read"/>
	<property name="LinuxMdState" type="s" access="read"/>
	<property name="LinuxMdComponentState" type="as" access="read"/>
	<property name="LinuxMdComponentHolder" type="o" access="read"/>
	<property name="LinuxMdComponentVersion" type="s" access="read"/>
	<property name="LinuxMdComponentHomeHost" type="s" access="read"/>
	<property name="LinuxMdComponentName" type="s" access="read"/>
	<property name="LinuxMdComponentUuid" type="s" access="read"/>
	<property name="LinuxMdComponentNumRaidDevices" type="i" access="read"/>
	<property name="LinuxMdComponentPosition" type="i" access="read"/>
	<property name="LinuxMdComponentLevel" type="s" access="read"/>
	<property name="DriveAtaSmartBlob" type="ay" access="read"/>
	<property name="DriveAtaSmartStatus" type="s" access="read"/>
	<property name="DriveAtaSmartTimeCollected" type="t" access="read"/>
	<property name="DriveAtaSmartIsAvailable" type="b" access="read"/>
	<property name="OpticalDiscNumSessions" type="u" access="read"/>
	<property name="OpticalDiscNumAudioTracks" type="u" access="read"/>
	<property name="OpticalDiscNumTracks" type="u" access="read"/>
	<property name="OpticalDiscIsClosed" type="b" access="read"/>
	<property name="OpticalDiscIsAppendable" type="b" access="read"/>
	<property name="OpticalDiscIsBlank" type="b" access="read"/>
	<property name="DriveSimilarDevices" type="ao" access="read"/>
	<property name="DrivePorts" type="ao" access="read"/>
	<property name="DriveAdapter" type="o" access="read"/>
	<property name="DriveIsRotational" type="b" access="read"/>
	<property name="DriveCanSpindown" type="b" access="read"/>
	<property name="DriveCanDetach" type="b" access="read"/>
	<property name="DriveIsMediaEjectable" type="b" access="read"/>
	<property name="DriveMedia" type="s" access="read"/>
	<property name="DriveMediaCompatibility" type="as" access="read"/>
	<property name="DriveConnectionSpeed" type="t" access="read"/>
	<property name="DriveConnectionInterface" type="s" access="read"/>
	<property name="DriveWriteCache" type="s" access="read"/>
	<property name="DriveRotationRate" type="u" access="read"/>
	<property name="DriveWwn" type="s" access="read"/>
	<property name="DriveSerial" type="s" access="read"/>
	<property name="DriveRevision" type="s" access="read"/>
	<property name="DriveModel" type="s" access="read"/>
	<property name="DriveVendor" type="s" access="read"/>
	<property name="PartitionTableCount" type="i" access="read"/>
	<property name="PartitionTableScheme" type="s" access="read"/>
	<property name="PartitionAlignmentOffset" type="t" access="read"/>
	<property name="PartitionSize" type="t" access="read"/>
	<property name="PartitionOffset" type="t" access="read"/>
	<property name="PartitionNumber" type="i" access="read"/>
	<property name="PartitionFlags" type="as" access="read"/>
	<property name="PartitionUuid" type="s" access="read"/>
	<property name="PartitionLabel" type="s" access="read"/>
	<property name="PartitionType" type="s" access="read"/>
	<property name="PartitionScheme" type="s" access="read"/>
	<property name="PartitionSlave" type="o" access="read"/>
	<property name="LuksCleartextUnlockedByUid" type="u" access="read"/>
	<property name="LuksCleartextSlave" type="o" access="read"/>
	<property name="LuksHolder" type="o" access="read"/>
	<property name="IdLabel" type="s" access="read"/>
	<property name="IdUuid" type="s" access="read"/>
	<property name="IdVersion" type="s" access="read"/>
	<property name="IdType" type="s" access="read"/>
	<property name="IdUsage" type="s" access="read"/>
	<property name="JobPercentage" type="d" access="read"/>
	<property name="JobIsCancellable" type="b" access="read"/>
	<property name="JobInitiatedByUid" type="u" access="read"/>
	<property name="JobId" type="s" access="read"/>
	<property name="JobInProgress" type="b" access="read"/>
	<property name="DevicePresentationIconName" type="s" access="read"/>
	<property name="DevicePresentationName" type="s" access="read"/>
	<property name="DevicePresentationNopolicy" type="b" access="read"/>
	<property name="DevicePresentationHide" type="b" access="read"/>
	<property name="DeviceBlockSize" type="t" access="read"/>
	<property name="DeviceSize" type="t" access="read"/>
	<property name="DeviceIsLinuxLoop" type="b" access="read"/>
	<property name="DeviceIsLinuxDmmp" type="b" access="read"/>
	<property name="DeviceIsLinuxDmmpComponent" type="b" access="read"/>
	<property name="DeviceIsLinuxLvm2PV" type="b" access="read"/>
	<property name="DeviceIsLinuxLvm2LV" type="b" access="read"/>
	<property name="DeviceIsLinuxMd" type="b" access="read"/>
	<property name="DeviceIsLinuxMdComponent" type="b" access="read"/>
	<property name="DeviceIsLuksCleartext" type="b" access="read"/>
	<property name="DeviceIsLuks" type="b" access="read"/>
	<property name="DeviceMountedByUid" type="u" access="read"/>
	<property name="DeviceMountPaths" type="as" access="read"/>
	<property name="DeviceIsMounted" type="b" access="read"/>
	<property name="DeviceIsOpticalDisc" type="b" access="read"/>
	<property name="DeviceIsDrive" type="b" access="read"/>
	<property name="DeviceIsReadOnly" type="b" access="read"/>
	<property name="DeviceIsMediaChangeDetectionInhibited" type="b" access="read"/>
	<property name="DeviceIsMediaChangeDetectionInhibitable" type="b" access="read"/>
	<property name="DeviceIsMediaChangeDetectionPolling" type="b" access="read"/>
	<property name="DeviceIsMediaChangeDetected" type="b" access="read"/>
	<property name="DeviceIsMediaAvailable" type="b" access="read"/>
	<property name="DeviceIsRemovable" type="b" access="read"/>
	<property name="DeviceIsPartitionTable" type="b" access="read"/>
	<property name="DeviceIsPartition" type="b" access="read"/>
	<property name="DeviceIsSystemInternal" type="b" access="read"/>
	<property name="DeviceFileByPath" type="as" access="read"/>
	<property name="DeviceFileById" type="as" access="read"/>
	<property name="DeviceFilePresentation" type="s" access="read"/>
	<property name="DeviceFile" type="s" access="read"/>
	<property name="DeviceMinor" type="x" access="read"/>
	<property name="DeviceMajor" type="x" access="read"/>
	<property name="DeviceMediaDetectionTime" type="t" access="read"/>
	<property name="DeviceDetectionTime" type="t" access="read"/>
	<property name="NativePath" type="s" access="read"/>
  </interface>
</node>
*/
#endregion
#endif