using System;
using System.IO;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Media
{
	public partial class SoundFieldControl : UserControl
	{
		private  ISimpleAudioSession _recorder;
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
				if (_recorder != null) _recorder.Dispose();
				_recorder = AudioFactory.CreateAudioSession(Path);
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

			try
			{
				_recorder.StartRecording();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Sound Problem");
			}
			UpdateScreen();
		}

		private void _playButton_Click(object sender, EventArgs e)
		{
			try
			{
				_recorder.Play();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Sound Problem");
			}
			UpdateScreen();
		}

		private void _stopButton_Click(object sender, EventArgs e)
		{
			try
			{
				if(_recorder.IsRecording)
				{
					_recorder.StopRecordingAndSaveAsWav();
				}
				else
				{
					_recorder.StopPlaying();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Sound Problem");
			}
			UpdateScreen();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			UpdateScreen();
		}
	}
}
