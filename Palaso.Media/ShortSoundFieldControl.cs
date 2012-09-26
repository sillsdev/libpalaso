using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows.Forms;
using Palaso.Reporting;

namespace Palaso.Media
{
	public partial class ShortSoundFieldControl : UserControl
	{
		private ISimpleAudioSession _recorder;
		// Use a standard SoundPlayer for playback. Although IrrKlang can do this with its own sounds,
		// it can't handle as wide a range of sounds recorded using other means.
		private SoundPlayer _player;
		BackgroundWorker m_worker;
		private bool IsPlaying { get; set; }
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
			_recordButton.MouseEnter += delegate(object o, EventArgs args) { ButtonEnter(args); };
			_recordButton.MouseLeave += delegate(object o, EventArgs args) { ButtonLeave(args); };
			_playButton.MouseEnter += delegate(object o, EventArgs args) { ButtonEnter(args); };
			_playButton.MouseLeave += delegate(object o, EventArgs args) { ButtonLeave(args); };
			_deleteButton.MouseLeave += delegate(object o, EventArgs args) { ButtonLeave(args); };
		}

		private void UpdateButtonAppearances(bool enable)
		{
			UpdateScreen();
			SetButtonAppearanceEnabled(enable); //we are really in
		}

		protected void ButtonEnter(EventArgs e)
		{
			UpdateButtonAppearances(true);
		}

		protected void ButtonLeave(EventArgs e)
		{
			UpdateButtonAppearances(false);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			UpdateScreen();
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			UpdateButtonAppearances(true);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			UpdateButtonAppearances(false);
		}

		public bool PlayOnly
		{
			get; set;
		}

		protected override void DestroyHandle()
		{
			base.DestroyHandle();
			Shutdown();
		}

		private void Shutdown()
		{
			;
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			if (Parent == null)
				Shutdown();
			else
				UpdateScreen();
		}

		public string Path
		{
			get { return _path; }
			set
			{
				_path = value;
				_recorder = AudioFactory.AudioSession(Path);
				// This is OK even if it does not exist, but it must exist before we try to play it.
				_player = new SoundPlayer(Path);
				toolTip1.SetToolTip(_deleteButton, _deleteButtonInstructions +"\r\n"+_path);
				UpdateScreen();
			}
		}

		private void UpdateScreen()
		{
			bool exists = File.Exists(Path);
			if (Parent == null)
			{
				// Screen update was attempted while the screen is not currently displayed;
				// set up a blank state to avoid confusion on inits, and then return as any other display
				// activity would cause a crash.
				_playButton.Visible = false;
				_recordButton.Visible = false;
				_poorMansWaveform.Visible = false;
				_deleteButton.Visible = false;
				return;
			}
			bool mouseIsWithin = Parent.RectangleToScreen(Bounds).Contains(MousePosition);
			SetButtonAppearanceEnabled(mouseIsWithin);
			bool mouseOverRecordButton = RectangleToScreen(_recordButton.Bounds).Contains(MousePosition);
			_recordButton.Enabled = _deleteButton.Enabled = !PlayOnly && mouseIsWithin;
			_recordButton.FlatAppearance.BorderSize = mouseOverRecordButton ? 1 : 0;

			_playButton.Enabled = mouseIsWithin && !IsPlaying;

			_playButton.Visible = exists && !_recorder.IsRecording;
			_deleteButton.Visible = mouseIsWithin && exists && !PlayOnly;

			 bool mouseOverDeleteButton = RectangleToScreen(_deleteButton.Bounds).Contains(MousePosition);
			 _deleteButton.FlatAppearance.BorderSize = mouseOverDeleteButton ? 1 : 0;

			 bool mouseOverPlayButton = RectangleToScreen(_playButton.Bounds).Contains(MousePosition);
			 _playButton.FlatAppearance.BorderSize = mouseOverPlayButton ? 1 : 0;

			_poorMansWaveform.Visible = false;//this was a good idea, but made the screen too busy //exists;
			_recordButton.Visible = !exists && !PlayOnly;
		}

		private void SetButtonAppearanceEnabled(bool mouseIsWithin)
		{
			if (mouseIsWithin)
			{
				_playButton.Image = global::Palaso.Media.Properties.Resources.play14x16;
				_recordButton.Image = global::Palaso.Media.Properties.Resources.record16x16;
			}
			else
			{
				_playButton.Image = global::Palaso.Media.Properties.Resources.playDisabled14x16;
				_recordButton.Image = global::Palaso.Media.Properties.Resources.recordDisabled16x16;
				_recordButton.FlatAppearance.BorderSize = _playButton.FlatAppearance.BorderSize = _deleteButton.FlatAppearance.BorderSize = 0;
			}
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
					UsageReporter.SendNavigationNotice("AudioDeleted");
				}
			}
		}

		private void OnRecordDown(object sender, MouseEventArgs e)
		{
			if (Control.ModifierKeys == Keys.Shift)
			{
				if(LetUserSelectPrerecordedFile() && SoundRecorded != null)
				{
					SoundRecorded.Invoke(this, null);
				}
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
				UsageReporter.SendNavigationNotice("AudioRecorded");
			}
		}


		private bool LetUserSelectPrerecordedFile()
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
					return false;
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
			return true;
		}

		private void OnClickPlay(object sender, MouseEventArgs e)
		{
			if (_player != null && File.Exists(Path) && !IsPlaying) //avoid crashes in situations where play should not have been available
			{
				//_recorder.Play();
				// I (JohnT) don't think any synchronization is needed here. The main thread accesses m_worker only here,
				// between creating it and starting the thread. It cannot access it again, since IsPlaying is true,
				// until the worker thread disposes the worker, sets it to null, and clears IsPlaying.
				// Similarly, the main thread sets IsPlaying only when it is false, which means no other thread is active.
				// Once it is set true, it can only be modified by the worker thread.
				// There may be some uncertainty about whether reading it gets the old or new value around the time
				// when the worker thread terminates, but that only affects the exact millisecond at which we re-enable the play button.
				IsPlaying = true;
				m_worker = new BackgroundWorker();
				m_worker.DoWork += PlaySoundInBackground;
				m_worker.RunWorkerAsync();
			}
			else
			{
				Debug.Assert(_recorder != null && File.Exists(Path), "The play button should not be enabled, there is nothing to play.");
			}
			UpdateScreen();
		}

		/// <summary>
		/// Delegate to play the sound in the background.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlaySoundInBackground(object sender, DoWorkEventArgs e)
		{
			_player.PlaySync(); // We use PlaySync in a thread so we can find out when it is done.
			m_worker.Dispose();
			m_worker = null;
			IsPlaying = false; // doing this last should prevent any contention for m_worker.
		}
	}
}
