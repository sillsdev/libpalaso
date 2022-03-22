using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace SIL.Media.Naudio.UI
{
	/// <summary>
	/// This control displays an icon and tooltip to indicate which recording device is currently in use.
	/// It also monitors the set of connected RecordingDevices and (a) switches to a new one if it appears,
	/// as for example when a new microphone is plugged in; (b) switches to the default one if the current
	/// one is unplugged. You can customize the icons using the __Image properties.
	/// </summary>
	public partial class RecordingDeviceIndicator : UserControl
	{
		private IAudioRecorder _recorder;

		Timer _checkNewMicTimer = new Timer();

		private HashSet<string> _knownRecordingDevices;

		/// <summary>
		/// Used to look at what names the OS has for a device and choose an icon. These are stored as functions so that we don't
		/// have to tell clients *when* they are allowed to override these.
		/// </summary>
		private readonly List<KeyValuePair<string, Func<Image>>> _nameSubstringToIconProperty =
			new List<KeyValuePair<string, Func<Image>>>();


		// These allow clients to set their own images to match their color schemes
		public Image NoAudioDeviceImage { get; set; } = AudioDeviceIcons.NoAudioDevice;
		public Image WebcamImage { get; set; } = AudioDeviceIcons.Webcam;
		public Image ComputerInternalImage { get; set; } = AudioDeviceIcons.Computer;
		public Image KnownHeadsetImage { get; set; } = AudioDeviceIcons.Headset;
		public Image UsbAudioDeviceImage { get; set; } = AudioDeviceIcons.Headset;
		public Image MicrophoneImage { get; set; } = AudioDeviceIcons.Microphone;
		public Image LineImage { get; set; } = AudioDeviceIcons.Line;
		public Image RecorderImage { get; set; } = AudioDeviceIcons.Recorder;

		public RecordingDeviceIndicator() : this(1000, true)
		{
		}

		public RecordingDeviceIndicator(int checkNewMicTimerInterval,
			bool checkNewMicTimerInitiallyEnabled)
		{
			InitializeComponent();

			// All the audio device icons shipped with libpalaso 16px high, but of varying width.
			// Because we allow clients to give different icons, honor whatever height/width they came in at,
			// no scaling. Done here so we can have this comment.
			_recordingDeviceImage.SizeMode = PictureBoxSizeMode.AutoSize;

			_checkNewMicTimer.Tick += OnCheckNewMicTimer_Tick;
			_checkNewMicTimer.Interval = checkNewMicTimerInterval;
			MicCheckingEnabled = checkNewMicTimerInitiallyEnabled;
		}

		/// <summary>
		/// This allows the client to suspend the periodic checking during operations (other than recording) where it is
		/// undesirable to change devices (or take the time to check for them).
		/// </summary>
		public bool MicCheckingEnabled
		{
			set { _checkNewMicTimer.Enabled = value; }
		}

		/// <summary>
		/// This control will find out about selected devices from the recorder, but also will tell the recorder to change devices as needed.
		/// </summary>
		public IAudioRecorder Recorder
		{
			get { return _recorder; }
			set
			{
				if(_recorder != null)
					_recorder.SelectedDeviceChanged -= RecorderOnSelectedDeviceChanged;
				_recorder = value;
				if(_recorder != null)
				{
					_recorder.SelectedDeviceChanged += RecorderOnSelectedDeviceChanged;
					_checkNewMicTimer.Start();
					SetKnownRecordingDevices(RecordingDevice.Devices);
				}
				else
				{
					_checkNewMicTimer.Stop();
				}
				if(IsHandleCreated)
					UpdateDisplay();
			}
		}

		private void SetKnownRecordingDevices(IEnumerable<IRecordingDevice> devices)
		{
			_knownRecordingDevices = new HashSet<string>(devices.Select(d => d.ProductName));
		}

		/// <summary>
		/// This is invoked once per second.
		/// It looks for new recording devices, such as when a mic is plugged in. If it finds one,
		/// it switches the Recorder to use it.
		/// It also checks whether the current recording device is still available. If not,
		/// it switches to whatever is the current default recording device (unless a new one was found,
		/// which takes precedence).
		/// The list of RecordingDevice.Devices, at least on Win7 with USB mics, seems to update as things
		/// are connected and disconnected.
		/// I'm not sure this approach will detect the plugging and unplugging of non-USB mics.
		/// </summary>
		/// <remarks>We compare product names rather than actual devices, because it appears that
		/// RecordingDevices.Devices creates a new list of objects each time; the ones from one call
		/// are never equal to the ones from a previous call.</remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnCheckNewMicTimer_Tick(object sender, EventArgs e)
		{
			if(_recorder == null)
				return;
			// Don't try to change horses in the middle of the stream if recording is in progress.
			if(_recorder.RecordingState == RecordingState.Recording ||
				_recorder.RecordingState == RecordingState.RequestedStop ||
				_recorder.RecordingState == RecordingState.Stopping)
				return;
			bool foundCurrentDevice = false;
			var devices = RecordingDevice.Devices.ToList();
			foreach(var device in devices)
			{
				if(!_knownRecordingDevices.Contains(device.ProductName))
				{
					_recorder.SelectedDevice = device;
					if(_recorder.RecordingState == RecordingState.Monitoring)
					{
						_knownRecordingDevices.Add(device.ProductName);
						UpdateDisplay();
						return;
					}
				}
				if(_recorder.SelectedDevice != null && device.ProductName ==
					_recorder.SelectedDevice.ProductName)
					foundCurrentDevice = true;
			}
			if(foundCurrentDevice)
			{
				if(_recorder.RecordingState != RecordingState.Monitoring)
				{
					_recorder.BeginMonitoring();
					if(_recorder.RecordingState == RecordingState.Monitoring)
						UpdateDisplay();
				}
			}
			else
			{
				// presumably unplugged...try to switch to another.
				var defaultDevice = devices.FirstOrDefault();
				if(defaultDevice != _recorder.SelectedDevice)
				{
					_recorder.SelectedDevice = defaultDevice;
					UpdateDisplay();
				}
			}
			// Update the list so one that was never active can be made active by unplugging and replugging
			SetKnownRecordingDevices(devices);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if(_checkNewMicTimer != null)
			{
				_checkNewMicTimer.Stop();
				_checkNewMicTimer.Dispose();
				_checkNewMicTimer = null;
			}
			if(_recorder != null)
				_recorder.SelectedDeviceChanged -= RecorderOnSelectedDeviceChanged;
			base.OnHandleDestroyed(e);
		}

		private void RecorderOnSelectedDeviceChanged(object sender, EventArgs eventArgs)
		{
			UpdateDisplay();
		}

		protected override void OnLoad(EventArgs e)
		{
			PrepareNameToIconLookup();
			UpdateDisplay();
			base.OnLoad(e);
		}

		public void UpdateDisplay()
		{
			if(_recorder?.SelectedDevice == null)
			{
				toolTip1.SetToolTip(_recordingDeviceImage, "no input device");
			}
			if(_recorder == null)
				return;

			if(Recorder.SelectedDevice == null)
				_recordingDeviceImage.Image = NoAudioDeviceImage;
			else
			{
				toolTip1.SetToolTip(_recordingDeviceImage, _recorder.SelectedDevice.GenericName);
				_recordingDeviceImage.Image = GetIconForRecordingDevice();
			}
		}

		private Image GetIconForRecordingDevice()
		{
			foreach(var pair in _nameSubstringToIconProperty)
			{
				var substring = pair.Key.ToLowerInvariant();
				if(_recorder.SelectedDevice.GenericName.ToLowerInvariant().Contains(substring) ||
					_recorder.SelectedDevice.ProductName.ToLowerInvariant().Contains(substring))
				{
					return pair.Value();
				}
			}
			return MicrophoneImage;
		}

		private void RecordingDeviceIndicator_Click(object sender, EventArgs e)
		{
			try
			{
				// launch the control panel that shows the user all their recording device options
				System.Diagnostics.Process.Start("control", "mmsys.cpl,,1");
			}
			catch(Exception)
			{
				// ah well, we tried, nothing useful to tell the user.
			}
		}

		private void PrepareNameToIconLookup()
		{
			AddDeviceMatch("None", () => NoAudioDeviceImage);
			//NB order is important here, as these are used in a substring match, so put the more specific ones (e.g. Webcam) before more general ones (e.g. Microphone)
			AddDeviceMatch("Webcam", () => WebcamImage);
			AddDeviceMatch("Internal", () => ComputerInternalImage);
			AddDeviceMatch("Headset", () => KnownHeadsetImage); // Technically not necessarily a "known" headset...
			AddDeviceMatch("USB Audio Device", () => UsbAudioDeviceImage);
			AddDeviceMatch("Microphone", () => MicrophoneImage);
			AddDeviceMatch("Line", () => LineImage);
			AddDeviceMatch("ZOOM", () => RecorderImage);

			// not sure if this ever gets used; it would if we had a recording device but failed to figure out anything else about it
			AddDeviceMatch("Generic", () => MicrophoneImage);

			// Headset device names
			// It's not clear to me why these device names are needed, but the original code was written in a way that
			// suggests sometimes the SelectedDevice.GenericName is null, requiring us to match on specific device names.

			foreach(var name in new[] {"Plantronics", "Andrea", "Microphone (VXi X200"})
			{
				AddDeviceMatch(name, () => KnownHeadsetImage);
			}
		}

		private void AddDeviceMatch(string substring, Func<Image> function)
		{
			_nameSubstringToIconProperty.Add(new KeyValuePair<string, Func<Image>>(substring, function));
		}
	}
}
