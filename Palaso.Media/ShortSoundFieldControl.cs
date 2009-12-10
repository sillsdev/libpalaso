using System;
using System.IO;
using System.Windows.Forms;

namespace Palaso.Media
{
	public partial class ShortSoundFieldControl : UserControl
	{
		private ISimpleAudioSession _recorder;
		private string _path;
		private string _deleteButtonInstructions = "Delete this recording.";

		public event EventHandler BeforeStartingToRecord;
		public event EventHandler SoundRecorded;
		public event EventHandler SoundDeleted;

		public ShortSoundFieldControl()
		{
			InitializeComponent();
			_recordButton.Bounds = _playButton.Bounds;//put it on top
			_hint.Left = _recordButton.Right + 5;
			_poorMansWaveform.Left = _hint.Left;
			_hint.Text = "";
		}

		public bool PlayOnly
		{
			get; set;
		}

		public string Path
		{
			get { return _path; }
			set
			{
				_path = value;
				_recorder = AudioFactory.AudioSession(Path);
				toolTip1.SetToolTip(_deleteButton, _deleteButtonInstructions +"\r\n"+_path);
				_timer.Enabled = true;
			}
		}

		private void UpdateScreen()
		{
			bool exists = File.Exists(Path);
			bool mouseIsWithin = Parent.RectangleToScreen(Bounds).Contains(MousePosition);

			if(mouseIsWithin)
			{
				_playButton.Image = global::Palaso.Media.Properties.Resources.play14x16;
				_recordButton.Image = global::Palaso.Media.Properties.Resources.record16x16;
			}
			else
			{
				_playButton.Image = global::Palaso.Media.Properties.Resources.playDisabled14x16;
				_recordButton.Image = global::Palaso.Media.Properties.Resources.recordDisabled16x16;
			}

			_recordButton.Enabled = _deleteButton.Enabled = !PlayOnly && mouseIsWithin;
			_playButton.Enabled = mouseIsWithin && !_recorder.IsPlaying;
			_recordButton.FlatAppearance.BorderSize = mouseIsWithin ? 1 : 0;

			_playButton.Visible =  exists && (_recorder.IsPlaying || _recorder.CanPlay);
			 _deleteButton.Visible = mouseIsWithin && exists && !PlayOnly ;

			 bool mouseOverDeleteButton = RectangleToScreen(_deleteButton.Bounds).Contains(MousePosition);
			 _deleteButton.FlatAppearance.BorderSize = mouseOverDeleteButton ? 1 : 0;

			 bool mouseOverPlayButton = RectangleToScreen(_playButton.Bounds).Contains(MousePosition);
			 _playButton.FlatAppearance.BorderSize = mouseOverPlayButton ? 1 : 0;

			_poorMansWaveform.Visible = false;//this was a good idea, but made the screen too busy //exists;
			_recordButton.Visible = !exists && !PlayOnly;


		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			UpdateScreen();
		}

		private void OnDeleteClick(object sender, EventArgs e)
		{
			if(File.Exists(_path))
			{
				File.Delete(_path);
				UpdateScreen();
				if (SoundDeleted != null)
				{
					SoundDeleted.Invoke(this, null);
				}
			}
		}

		private void OnRecordDown(object sender, MouseEventArgs e)
		{
			if (Control.ModifierKeys == Keys.Shift)
			{
				LetUserSelectPrerecordedFile();
				return;
			}
			//allow owner one last chance to set a path (which may be sensitive to other ui controls)
			if (BeforeStartingToRecord!=null)
				BeforeStartingToRecord.Invoke(this, null);

			if (File.Exists(Path))
				File.Delete(Path);

			_recorder.StartRecording();
			UpdateScreen();

		}

		private void OnRecordUp(object sender, MouseEventArgs e)
		{
			if(!_recorder.IsRecording)
				return;
			try
			{
				_recorder.StopRecordingAndSaveAsWav();
			}
			catch(Exception)
			{
				//swallow it review: initial reason is that they didn't hold it down long enough, could detect and give message
			}

			if(_recorder.LastRecordingMilliseconds < 500)
			{
				if (File.Exists(_path))
				{
					File.Delete(_path);
				}
				_hint.Text = "Hold down the record button while talking.";
			}
			else
			{
				_hint.Text = "";
			}
			UpdateScreen();
			if(SoundRecorded!=null)
			{
				SoundRecorded.Invoke(this, null);
			}
		}


		private void LetUserSelectPrerecordedFile()
		{
			try
			{
				var dlg = new OpenFileDialog();
				dlg.DefaultExt = ".wav";
				dlg.Multiselect = false;
				dlg.RestoreDirectory = true;
				dlg.AutoUpgradeEnabled = true;
				dlg.Filter = "sound files (*.wav)|*.wav";
				if (DialogResult.OK != dlg.ShowDialog())
				{
					return;
				}
				if (File.Exists(Path))
					File.Delete(Path);

				File.Copy(dlg.FileName, Path);
			}
			catch (Exception error)
			{
				MessageBox.Show(error.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			UpdateScreen();

		}

		private void OnClickPlay(object sender, MouseEventArgs e)
		{
			_recorder.Play();
			UpdateScreen();
		}
	}
}
