#if !MONO
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
	public class RecordingDevice
	{
		public int DeviceNumber { get; set; }
		public string GenericName { get; set; }
		public string ProductName { get { return Capabilities.ProductName; } }
		public WaveInCapabilities Capabilities { get; set; }

		RecordingDevice(int deviceNumber, string name, WaveInCapabilities capabilities)
		{
			DeviceNumber = deviceNumber;
			GenericName = name;
			Capabilities = capabilities;
		}

		public static IEnumerable<RecordingDevice> Devices
		{
			get
			{
				// MMDeviceEnumerator only works in Vista and later.
				var devicEnumerator = (Environment.OSVersion.Version.Major >= 6 ?
					new MMDeviceEnumerator() : null);

				for (int i = 0; i < WaveIn.DeviceCount; i++)
				{
					string name = null;
					var capabilities = default(WaveInCapabilities);

					try
					{
						capabilities = WaveIn.GetCapabilities(i);
						name = capabilities.ProductName;

						if (devicEnumerator != null)
						{
							var x = devicEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
								.FirstOrDefault(d => d.DeviceFriendlyName == capabilities.ProductName);

							if (x != null)
								name = x.FriendlyName;
						}
					}
					catch (Exception)
					{
					}

					if (!string.IsNullOrEmpty(name))
						yield return new RecordingDevice(i, name, capabilities);

				}
			}
		}

	}
}
#endif
