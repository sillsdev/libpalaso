// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using SIL.Reporting;

namespace SIL.Media
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
			get => _path;
			set
			{
				_path = value;
				if (_recorder != null) _recorder.Dispose();
				_recorder = AudioFactory.CreateAudioSession(Path);
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

			_playButton.Enabled = mouseIsWithin && !_recorder.IsPlaying;

			_playButton.Visible = exists && (_recorder.IsPlaying || _recorder.CanPlay);
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
				_playButton.Image = Properties.Resources.play14x16;
				_recordButton.Image = Properties.Resources.record16x16;
			}
			else
			{
				_playButton.Image = Properties.Resources.playDisabled14x16;
				_recordButton.Image = Properties.Resources.recordDisabled16x16;
				_recordButton.FlatAppearance.BorderSize = _playButton.FlatAppearance.BorderSize = _deleteButton.FlatAppearance.BorderSize = 0;
			}
		}

		private void OnDeleteClick(object sender, EventArgs e)
		{
			if(File.Exists(_path))
			{
				_recorder.StopPlaying();
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
			if (ModifierKeys == Keys.Shift)
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
				var dlg = new OpenFileDialog
				{
					Multiselect = false,
					RestoreDirectory = true,
					AutoUpgradeEnabled = true,
					Filter = "sound files (*.wav, *.mp3)|*.wav;*.mp3"
				};
				if (DialogResult.OK != dlg.ShowDialog())
				{
					return false;
				}
				if (File.Exists(Path))
					File.Delete(Path);

				SetPathToAvailableFilenameInDefaultDir(dlg.FileName);
				// *Always* make a copy, since the only way to change the recording is to delete the old one, and there is no delete confirmation.
				// (We don't want to make it that easy to delete a file that somebody else might be using)
				File.Copy(dlg.FileName, Path);
			}
			catch (Exception error)
			{
				MessageBox.Show(error.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			UpdateScreen();
			return true;
		}

		/// <summary>
		/// LT-15375 Don't copy the file to some weird name.
		/// Use the filename that the user selected (or filename_#.ext if there is a conflict) in the default directory.
		/// </summary>
		private void SetPathToAvailableFilenameInDefaultDir(string dlgPath)
		{
			var defaultDir = System.IO.Path.GetDirectoryName(Path);
			var dlgName = System.IO.Path.GetFileName(dlgPath);
			var newPath = System.IO.Path.Combine(defaultDir, dlgName);
			if(File.Exists(newPath))
			{
				var newPathNoExt = System.IO.Path.ChangeExtension(newPath, null); // Get the path without an extension
				var dlgExt = System.IO.Path.GetExtension(dlgPath);
				var disambiguation = 1; // add a number at the end of the filename to disambiguate between new and pre-existing files
				while(File.Exists(newPath))
				{
					newPath = string.Format("{0}_{1}{2}", newPathNoExt, disambiguation++, dlgExt);
				}
			}
			Path = newPath;
		}

		private void OnClickPlay(object sender, MouseEventArgs e)
		{
			if (_recorder != null && File.Exists(Path)) //avoid crashes in situations where play should not have been available
			{
				try
				{
					_recorder.Play();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Sound Problem");
				}
			}
			else
			{
				Debug.Assert(_recorder != null && File.Exists(Path), "The play button should not be enabled, there is nothing to play.");
			}
			UpdateScreen();
		}
	}
}
