using System;
using System.Drawing.Imaging;
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
					toolTip1.SetToolTip(_recordingDeviceImage, value.SelectedDevice.Capabilities.ProductName);

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
				_recordingDeviceImage.Image = AudioDeviceIcons.Computer;
			else if (_recorder.SelectedDevice.GenericName.Contains("USB Audio Device"))
				_recordingDeviceImage.Image = AudioDeviceIcons.HeadSet;

			var deviceName = _recorder.SelectedDevice.ProductName;

			if(deviceName.Contains("ZOOM"))
				_recordingDeviceImage.Image = AudioDeviceIcons.Recorder;
			if (deviceName.Contains("Plantronics"))
				_recordingDeviceImage.Image = AudioDeviceIcons.HeadSet;
			if (deviceName.Contains("Andrea"))
				_recordingDeviceImage.Image = AudioDeviceIcons.HeadSet;
		}
	}
}
