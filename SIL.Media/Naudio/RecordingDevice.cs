using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace SIL.Media.Naudio
{
	/// <summary>
	/// A RecordingDevice is used to select and get information about the
	/// hardware which the record is listening to.
	/// </summary>
	public class RecordingDevice : IRecordingDevice
	{
		public string Id { get; }
		public int DeviceNumber { get; set; }
		public string GenericName { get; set; }
		public string ProductName => Capabilities.ProductName;
		public WaveInCapabilities Capabilities { get; set; }

		private RecordingDevice(string id, int deviceNumber, string name, WaveInCapabilities capabilities)
		{
			Id = id;
			DeviceNumber = deviceNumber;
			GenericName = name;
			Capabilities = capabilities;
		}

		public static IEnumerable<IRecordingDevice> Devices
		{
			get
			{
				// MMDeviceEnumerator only works in Vista and later.
				var devicEnumerator = (Environment.OSVersion.Version.Major >= 6 ?
					new MMDeviceEnumerator() : null);

				for (int i = 0; i < WaveIn.DeviceCount; i++)
				{
					string name = null;
					string id = null;
					var capabilities = default(WaveInCapabilities);

					try
					{
						capabilities = WaveIn.GetCapabilities(i);
						name = capabilities.ProductName;

						if (devicEnumerator != null)
						{
							var x = devicEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
								.FirstOrDefault(d => d.DeviceFriendlyName == capabilities.ProductName);
							if (x == null)
							{
								// Seems quite often the capabilities ProductName is a truncation of the endPoint FriendlyName.
								// See if we can match it that way.
								x = devicEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
									.FirstOrDefault(d => d.FriendlyName.StartsWith(capabilities.ProductName));
							}

							if (x != null)
							{
								name = x.FriendlyName;
								id = x.ID;
							}
						}
					}
					catch (Exception)
					{
					}

					if (!string.IsNullOrEmpty(name))
						yield return new RecordingDevice(id, i, name, capabilities);

				}
			}
		}

		/// <summary>
		/// A sensible notion of 'Equals' is very important for this class, otherwise, since Devices returns a collection
		/// of completely new objects, any existing RecordingDevice (such as the current one) will not be found
		/// in the list of Devices, and clients will think it has been unplugged, possibly leading to spuriously
		/// selecting a different one.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return (obj is RecordingDevice) && this == (RecordingDevice) obj;
		}

		// I doubt very much this is used, but it ought to be overridden if Equals is.
		public override int GetHashCode()
		{
			return (Id??"").GetHashCode() ^ (ProductName??"").GetHashCode() ^ (Capabilities.ProductName??"").GetHashCode();
		}

		public static bool operator ==(RecordingDevice x, RecordingDevice y)
		{
			if (object.ReferenceEquals(x, null))
				return object.ReferenceEquals(y, null);
			return !object.ReferenceEquals(y, null) && x.Id == y.Id && x.ProductName == y.ProductName && x.Capabilities.ProductName == y.Capabilities.ProductName;
		}

		public static bool operator !=(RecordingDevice x, RecordingDevice y)
		{
			return !(x == y);
		}
	}
}
