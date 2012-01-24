using System;
using System.IO;
using System.Linq;
using NAudio.Wave;
using Palaso.Media.Naudio;

namespace Palaso.Media
{
	/// <summary>
	/// This class wraps our NAudio player and recorder (developed for SayMore)
	/// into the interace used by WeSay/FLEx. We built this when we found that
	///  IrrKlang wasn't playing some wavs that came from another source.
	/// </summary>
	public class NAudioSession : ISimpleAudioSession, IDisposable
	{
		private NAudioAudioPlayer _player;
		private NAudioAudioRecorder _recorder;

		public NAudioSession(string filePath)
		{
			FilePath = filePath;
			_player = new NAudioAudioPlayer();
			_recorder = new NAudioAudioRecorder();
			_recorder.SelectedDevice = RecordingDevice.Devices.First();

			_recorder.BeginMonitoring();
		}

		public string FilePath { get; private set; }

		public bool IsPlaying
		{
			get { return _player.PlaybackState == PlaybackState.Playing; }
		}

		public bool CanStop
		{
			get { return _player.PlaybackState == PlaybackState.Playing || _player.PlaybackState == PlaybackState.Paused; }
		}

		public bool CanPlay
		{
			get { return _player.PlaybackState == PlaybackState.Stopped && File.Exists(FilePath); }
		}

		public void Play()
		{
			_player.LoadFile(FilePath);
			_player.Play();
		}


		public void StopPlaying()
		{
			_player.Stop();
		}


		public bool CanRecord
		{
			get { return _recorder.RecordingState == RecordingState.Monitoring; }
		}

		public void StopRecordingAndSaveAsWav()
		{
			_recorder.Stop();
			//with naudio, saving is not a separate operation.
		}

		public double LastRecordingMilliseconds
		{
			get { return _recorder.RecordedTime.Milliseconds; }
		}

		public bool IsRecording
		{
			get { return _recorder.RecordingState == RecordingState.Recording; }
		}


		void ISimpleAudioSession.SaveAsWav(string filePath)
		{
			//with naudio, saving is not a separate operation.
		}


		public void StartRecording()
		{
//			_recorder.BeginMonitoring();
			_recorder.BeginRecording(FilePath);
		}

		public void Dispose()
		{
			_player.Dispose();
			_recorder.Dispose();
		}
	}
}