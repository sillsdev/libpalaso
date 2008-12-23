using System;
using System.IO;
using System.Windows.Forms;

namespace Palaso.Media
{
	public partial class SoundFieldControl : UserControl
	{
		private  AudioRecorder _recorder;
		private string _path;

		public SoundFieldControl()
		{
			InitializeComponent();
		}

		public SoundFieldControl(string path)
			:this()
		{
			Path = path;
		}

		public string Path
		{
			get { return _path; }
			set
			{
				_path = value;
				_recorder = new AudioRecorder(Path);
				_timer.Enabled = true;
			}
		}


		private void UpdateScreen()
		{
			_recordButton.Enabled = _recorder.CanRecord;
			_stopButton.Enabled = _recorder.CanStop;
			_playButton.Enabled = _recorder.CanPlay && File.Exists(Path);
		}

		private void _recordButton_Click(object sender, EventArgs e)
		{
		   if(File.Exists(Path))
			   File.Delete(Path);

			_recorder.StartRecording();
			UpdateScreen();
		}

		private void _playButton_Click(object sender, EventArgs e)
		{
			_recorder.Play();
			UpdateScreen();
		}

		private void _stopButton_Click(object sender, EventArgs e)
		{
			if(_recorder.IsRecording)
			{
				_recorder.StopRecording();
			}
			else
			{
				_recorder.StopPlaying();
			}
			UpdateScreen();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			UpdateScreen();
		}
	}
}
