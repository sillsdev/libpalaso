#if !MONO
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
	/// one is unplugged.
	/// Enhance JohnT: Possibly the RecordingDeviceIndicator could become a RecordingDeviceButton and could respond to a click by cycling through
	/// the available devices, or pop up a chooser...though that is probably overdoing things, users
	/// are unlikely to have more than two. Currently there is no click behavior.
	/// </summary>
	public partial class RecordingDeviceIndicator : UserControl
	{
		private IAudioRecorder _recorder;

		Timer _checkNewMicTimer = new Timer();

		private HashSet<string> _knownRecordingDevices;

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
			SetInitialIcons();
		}

		/// <summary>
		/// Clients can set to icons that fit their color scheme.
		/// </summary>
		public Dictionary<string, Image> NameToIcon { get;  set; } =
			new Dictionary<string, Image>();

		private void SetInitialIcons()
		{
			NameToIcon = new Dictionary<string, Image>();
			NameToIcon.Add("None", AudioDeviceIcons.NoAudioDevice);
			//NB order is important here, as these are used in a substring match, so put the more specific ones (e.g. Webcam) before more general ones (e.g. Microphone)
			NameToIcon.Add("Webcam", AudioDeviceIcons.Webcam);
			NameToIcon.Add("Internal", AudioDeviceIcons.Computer);
			NameToIcon.Add("USB Audio Device", AudioDeviceIcons.Headset);
			NameToIcon.Add("Microphone", AudioDeviceIcons.Microphone);
			NameToIcon.Add("Line", AudioDeviceIcons.Line);
			NameToIcon.Add("ZOOM", AudioDeviceIcons.Recorder);

			// not sure if this ever gets used; it would if we had a recording device but failed to figure out anything else about it
			NameToIcon.Add("Generic", AudioDeviceIcons.Microphone);

			// Headset device names
			// It's not clear to me why these device names are needed, but the original code was written in a way that
			// suggests sometimes the SelectedDevice.GenericName is null, requiring us to match on specific device names.

			foreach(var name in new[] {"Plantronics", "Andrea", "Microphone (VXi X200"})
			{
				NameToIcon.Add(name, AudioDeviceIcons.Headset);
			}
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
					SetKnownRecordingDevices();
				}
				else
				{
					_checkNewMicTimer.Stop();
				}
				if(IsHandleCreated)
					UpdateDisplay();
			}
		}

		private void SetKnownRecordingDevices()
		{
			_knownRecordingDevices =
				new HashSet<string>(from d in RecordingDevice.Devices select d.ProductName);
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
			if(_recorder.RecordingState != RecordingState.Monitoring &&
				_recorder.RecordingState != RecordingState.Stopped)
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
			SetKnownRecordingDevices();
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
			UpdateDisplay();
			base.OnLoad(e);
		}

		public void UpdateDisplay()
		{
			if(_recorder?.SelectedDevice != null)
			{
				toolTip1.SetToolTip(_recordingDeviceImage, _recorder.SelectedDevice.Capabilities.ProductName);
			}
			else
			{
				toolTip1.SetToolTip(_recordingDeviceImage, "no input device");
			}
			if(_recorder == null)
				return;

			if(Recorder.SelectedDevice == null)
				_recordingDeviceImage.Image = NameToIcon["None"];
			else
			{
				toolTip1.SetToolTip(_recordingDeviceImage, _recorder.SelectedDevice.GenericName);
				_recordingDeviceImage.Image = GetIconForRecodingDevice();
			}
		}

		private Image GetIconForRecodingDevice()
		{
			foreach (var key in NameToIcon.Keys)
			{
				var substring = key.ToLowerInvariant();
				if (_recorder.SelectedDevice.GenericName.ToLowerInvariant().Contains(substring) ||
					_recorder.SelectedDevice.ProductName.ToLowerInvariant().Contains(substring))
				{
					return NameToIcon[key];
				}
			}
				return NameToIcon["Generic"];
		}

		private void RecordingDeviceIndicator_Click(object sender, EventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start("control", "mmsys.cpl,,1");
			}
			catch (Exception)
			{
				// ah well, we tried, nothing useful to tell the user.
			}
		}
	}
}
#endif
