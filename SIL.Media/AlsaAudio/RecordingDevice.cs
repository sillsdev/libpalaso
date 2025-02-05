// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace SIL.Media.AlsaAudio
{
	/// <summary>
	/// This class implements the methods and properties used by Bloom.  It reimplements a class in
	/// SIL.Media/NAudio, but for Linux.
	/// </summary>
	public class RecordingDevice: IRecordingDevice
	{
		private static RecordingDevice _default;
		public static RecordingDevice DefaultDevice
		{
			get
			{
				if (_default == null)
				{
					// This sets the value as a side-effect.
					var list = Devices;
					if (list.Count == 0)
					{
						Debug.WriteLine("No input audio devices available!");
					}
				}
				return _default;
			}
			private set => _default = value;
		}

		public string Id { get; }
		public int DeviceNumber { get; set; }
		public string GenericName { get; set; }
		public string ProductName { get; set; }
		public WaveInCapabilities Capabilities { get; set; }

		public override bool Equals(object obj)
		{
			var that = obj as RecordingDevice;
			if (that == null)
				return false;
			return DeviceNumber == that.DeviceNumber &&
				GenericName == that.GenericName &&
				ProductName == that.ProductName;
		}

		public override int GetHashCode()
		{
			return GenericName.GetHashCode() ^ ProductName.GetHashCode() + DeviceNumber;
		}

		public override string ToString () =>
			$"[RecordingDevice: DeviceNumber={DeviceNumber}, GenericName={GenericName}, ProductName={ProductName}]";

		public static List<RecordingDevice> Devices
		{
			get
			{
				var list = new List<RecordingDevice>();
				DefaultDevice = null;
/*
/proc/asound/pcm contains lines like the following:

00-00: ALC269VB Analog : ALC269VB Analog : playback 1 : capture 1
01-03: HDMI 0 : HDMI 0 : playback 1
02-00: USB Audio : USB Audio : playback 1 : capture 1

/proc/asound/cards contains pairs of lines like the following:

 0 [PCH            ]: HDA-Intel - HDA Intel PCH
                      HDA Intel PCH at 0xf7f30000 irq 31
 1 [HDMI           ]: HDA-Intel - HDA ATI HDMI
                      HDA ATI HDMI at 0xf7e40000 irq 32
 2 [Headset        ]: USB-Audio - Logitech USB Headset
                      Logitech Logitech USB Headset at usb-0000:00:1a.0-1.4, full speed
*/
				var pcmLines = System.IO.File.ReadAllLines("/proc/asound/pcm");
				var cardsLines = System.IO.File.ReadAllLines("/proc/asound/cards");
				if (pcmLines == null || pcmLines.Length == 0 || cardsLines == null || cardsLines.Length == 0)
				{
					return list;
				}

				foreach (var pcm in pcmLines)
				{
					if (pcm.Contains(" capture "))
					{
						var pcmPieces = pcm.Split(new char[] { ':' });
						var num = pcmPieces[0];
						if (num.StartsWith("0"))
							num = num.Substring(1);
						var idx = num.IndexOf('-');
						if (idx > 0)
							num = num.Substring(0, idx);
						var idNum = String.Format(" {0} ", num);
						for (int i = 0; i < cardsLines.Length; ++i)
						{
							if (cardsLines[i].StartsWith(idNum))
							{
								var desc = cardsLines[i];
								idx = desc.IndexOf("]: ");
								if (idx > 0)
									desc = desc.Substring(idx + 3);
								idx = desc.IndexOf(" - ");
								if (idx > 0)
									desc = desc.Substring(idx + 3);
								var dev = new RecordingDevice()
								{
									DeviceNumber = Int32.Parse(num),
									GenericName = pcmPieces[1].Trim(),
									ProductName = desc
								};
								if (IsCardAssignedAsDefault(num))
									DefaultDevice = dev;
								if (!dev.GenericName.ToLowerInvariant().Contains("usb") &&
									!dev.ProductName.ToLowerInvariant().Contains("usb"))
								{
									var mics = FindPluggedInMicrophones(dev.DeviceNumber);
									if (mics.Count == 0)
										continue;
									dev.GenericName = mics[0];
								}
								list.Add(dev);
							}
						}
					}
				}
				if (list.Count == 0)
					DefaultDevice = null;
				else if (_default == null)
					DefaultDevice = list[0];
				return list;
			}
		}

		static bool IsCardAssignedAsDefault(string num)
		{
/*
When the USB headset on card 2 is assigned as the preferred/default input device, then
/proc/asound/card0/pcm0c/info looks something like this

card: 0
device: 0
subdevice: 0
stream: CAPTURE
id: ALC269VB Analog
name: ALC269VB Analog
subname: subdevice #0
class: 0
subclass: 0
subdevices_count: 1
subdevices_avail: 1

and /proc/asound/card2/pcm0c/info looks something like this

card: 2
device: 0
subdevice: 0
stream: CAPTURE
id: USB Audio
name: USB Audio
subname: subdevice #0
class: 0
subclass: 0
subdevices_count: 1
subdevices_avail: 0

This is the only clue I've found in 2 days of searching to tell which card is
assigned as the default (preferred) input device.
 */
			try
			{
				// get the information about the capture device of this card
				var filename = $"/proc/asound/card{num}/pcm0c/info";
				var infoLines = System.IO.File.ReadAllLines(filename);
				if (infoLines == null | infoLines.Length == 0)
					return false;
				int subdeviceCount = 0;
				int subdeviceAvail = 0;
				foreach (var line in infoLines)
				{
					if (line.StartsWith("subdevices_count: "))
						subdeviceCount = Int32.Parse(line.Substring(18));
					else if (line.StartsWith("subdevices_avail: "))
						subdeviceAvail = Int32.Parse(line.Substring(18));
				}
				return subdeviceAvail < subdeviceCount;
			}
			catch
			{
				return false;
			}
		}

		// The following low-level hackery is needed to detect whether a microphone is plugged in to a
		// sound card through an external microphone jack.  The code is adapted from the C sources to
		// the amixer program.
		[DllImport ("libasound.so.2")]
		static extern int snd_ctl_elem_id_malloc(ref IntPtr id);
		[DllImport ("libasound.so.2")]
		static extern void snd_ctl_elem_id_free(IntPtr id);
		[DllImport ("libasound.so.2")]
		static extern int snd_ctl_elem_info_malloc(ref IntPtr info);
		[DllImport ("libasound.so.2")]
		static extern void snd_ctl_elem_info_free(IntPtr info);
		[DllImport ("libasound.so.2")]
		static extern int snd_hctl_open(ref IntPtr hctl, string name, int mode);
		[DllImport ("libasound.so.2")]
		static extern int snd_hctl_close(IntPtr hctl);
		[DllImport ("libasound.so.2")]
		static extern int snd_hctl_load(IntPtr hctl);
		[DllImport ("libasound.so.2")]
		static extern IntPtr snd_hctl_first_elem(IntPtr hctl);
		[DllImport ("libasound.so.2")]
		static extern IntPtr snd_hctl_elem_next(IntPtr elem);
		[DllImport ("libasound.so.2")]
		static extern int snd_hctl_elem_info(IntPtr elem, IntPtr info);
		[DllImport ("libasound.so.2")]
		static extern void snd_hctl_elem_get_id(IntPtr elem, IntPtr id);
		[DllImport ("libasound.so.2")]
		static extern uint snd_ctl_elem_info_get_count(IntPtr info);
		[DllImport ("libasound.so.2")]
		static extern int snd_ctl_elem_info_get_type(IntPtr info);
		[DllImport ("libasound.so.2")]
		static extern int snd_ctl_elem_value_malloc(ref IntPtr obj);
		[DllImport ("libasound.so.2")]
		static extern void snd_ctl_elem_value_free(IntPtr obj);
		[DllImport ("libasound.so.2")]
		static extern int snd_hctl_elem_read(IntPtr elem, IntPtr control);
		[DllImport ("libasound.so.2")]
		static extern int snd_ctl_elem_value_get_boolean(IntPtr control, uint idx);
		[DllImport ("libasound.so.2")]
		static extern int snd_ctl_elem_info_is_readable(IntPtr info);
		[DllImport ("libasound.so.2")]
		static extern string snd_ctl_ascii_elem_id_get(IntPtr id);

		const int SND_CTL_ELEM_TYPE_BOOLEAN = 1;

		static List<string> FindPluggedInMicrophones(int cardNumber)
		{
			IntPtr handle = IntPtr.Zero;
			IntPtr elem = IntPtr.Zero;
			IntPtr id = IntPtr.Zero;
			IntPtr info = IntPtr.Zero;

			var retval = new List<string>();
			var cardId = $"hw:{cardNumber}";
			if (snd_hctl_open(ref handle, cardId, 0) < 0)
				return retval;
			if (snd_hctl_load(handle) < 0)
			{
				snd_hctl_close(handle);
				return retval;
			}
			try
			{
				snd_ctl_elem_id_malloc(ref id);
				snd_ctl_elem_info_malloc(ref info);
				for (elem = snd_hctl_first_elem(handle); elem != IntPtr.Zero; elem = snd_hctl_elem_next(elem))
				{
					if (snd_hctl_elem_info(elem, info) < 0)
						break;
					snd_hctl_elem_get_id(elem, id);
					var str = snd_ctl_ascii_elem_id_get(id);
					if (String.IsNullOrEmpty(str))
						continue;
					if (str.Contains("Mic ") && str.Contains(" Jack"))
					{
						if (CheckMicrophoneValuesForOn(elem))
						{
							var idx = str.IndexOf("name=");
							if (idx > 0)
							{
								str = str.Substring(idx + 5);
								str = str.Trim(new char[] {'\'', '"'});
							}
							retval.Add(str);
						}
					}
				}
			}
			finally
			{
				snd_hctl_close(handle);
				snd_ctl_elem_info_free(info);
				snd_ctl_elem_id_free(id);
			}
			return retval;
		}

		static bool CheckMicrophoneValuesForOn(IntPtr elem)
		{
			IntPtr info = IntPtr.Zero;		// snd_ctl_elem_info_t *
			IntPtr control = IntPtr.Zero;	// snd_ctl_elem_value_t *

			snd_ctl_elem_info_malloc(ref info);
			if (snd_hctl_elem_info(elem, info) < 0)
			{
				snd_ctl_elem_info_free(info);
				return false;
			}
			if (snd_ctl_elem_info_is_readable(info) == 0)
			{
				snd_ctl_elem_info_free(info);
				return false;
			}
			snd_ctl_elem_value_malloc(ref control);
			bool retval = false;
			if (snd_hctl_elem_read(elem, control) >= 0)
			{
				// Microphone jacks have a single boolean value that indicates whether a microphone
				// is actually plugged in.
				uint count = snd_ctl_elem_info_get_count(info);
				int type = snd_ctl_elem_info_get_type(info);
				if (type == SND_CTL_ELEM_TYPE_BOOLEAN && count == 1)
					retval = snd_ctl_elem_value_get_boolean(control, 0) != 0;
			}
			snd_ctl_elem_value_free(control);
			snd_ctl_elem_info_free(info);
			return retval;
		}
	}
}
