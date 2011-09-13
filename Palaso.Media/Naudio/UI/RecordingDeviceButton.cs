using System;
using System.Windows.Forms;

namespace Palaso.Media.Naudio.UI
{
	public partial class RecordingDeviceButton : UserControl
	{
		private IAudioRecorder _recorder;

		public RecordingDeviceButton()
		{
			InitializeComponent();
		}

		public IAudioRecorder Recorder
		{
			get { return _recorder; }
			set
			{
				_recorder = value;
				if(_recorder !=null)
					toolTip1.SetToolTip(_recordingDeviceButton, value.SelectedDevice.Capabilities.ProductName);

			}
		}

		protected override void OnLoad(EventArgs e)
		{
			UpdateDisplay();
			base.OnLoad(e);
		}

		public void UpdateDisplay()
		{
			if (_recorder == null)
				return;

			if(_recorder.SelectedDevice.GenericName.Contains("Internal"))
				_recordingDeviceButton.ImageNormal = AudioDeviceIcons.Computer;
			else if (_recorder.SelectedDevice.GenericName.Contains("USB Audio Device"))
				_recordingDeviceButton.ImageNormal = AudioDeviceIcons.HeadSet;

			var deviceName = _recorder.SelectedDevice.ProductName;

			if(deviceName.Contains("ZOOM"))
				_recordingDeviceButton.ImageNormal = AudioDeviceIcons.Recorder;
			if (deviceName.Contains("Plantronics"))
				_recordingDeviceButton.ImageNormal = AudioDeviceIcons.HeadSet;
			if (deviceName.Contains("Andrea"))
				_recordingDeviceButton.ImageNormal = AudioDeviceIcons.HeadSet;
		}
	}
}
