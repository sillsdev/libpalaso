using System;
using System.Timers;
using NAudio.Wave;

namespace SIL.Windows.Forms.Media.Naudio
{
	public class AudioPlayer : IAudioPlayer
	{
		private WaveOut _waveOut;
		private TrimWaveStream _inStream;
		private bool _wasStreamCreatedLocally;
		private Timer _timer;
		private PlaybackProgressEventArgs _playbackProgressEventArgs = new PlaybackProgressEventArgs();

		public event EventHandler Stopped = delegate { };
		public event EventHandler PlaybackStarted = delegate { };
		public event EventHandler<PlaybackProgressEventArgs> PlaybackProgress = delegate { };

		public AudioPlayer()
		{
			_timer = new Timer(100);
			_timer.Elapsed += delegate
			{
				_playbackProgressEventArgs.PlaybackPosition = _inStream.CurrentTime;
				PlaybackProgress.Invoke(this, _playbackProgressEventArgs);
			};
		}

		public void LoadStream(WaveStream stream)
		{
			InternalLoad(stream as TrimWaveStream ?? new TrimWaveStream(stream));
			_wasStreamCreatedLocally = false;
		}

		public void LoadFile(string path)
		{
			InternalLoad(new TrimWaveStream(new WaveFileReader(path)));
			_wasStreamCreatedLocally = true;
		}

		private void InternalLoad(TrimWaveStream stream)
		{
			CloseWaveOut();
			CloseInStream();
			_inStream = stream;
		}

		public void StartPlaying()
		{
			if (PlaybackState != PlaybackState.Stopped || _inStream == null)
				return;

			CreateWaveOut();
			_inStream.Position = 0;
			_waveOut.Play();
			PlaybackStarted.Invoke(this, EventArgs.Empty);
			_timer.Start();
		}

		private void CreateWaveOut()
		{
			if (_waveOut == null)
			{
				_waveOut = new WaveOut();
				_waveOut.Init(_inStream);
				_waveOut.PlaybackStopped += HandleWaveOutPlaybackStopped;
			}
		}

		void HandleWaveOutPlaybackStopped(object sender, EventArgs e)
		{
			CloseWaveOut();
			Stopped.Invoke(sender, e);
		}

		public void Stop()
		{
			_waveOut.Stop();
			_timer.Stop();
			_inStream.Position = 0;
		}

		public TimeSpan StartPosition
		{
			get { return _inStream.StartPosition; }
			set { _inStream.StartPosition = value; }
		}

		public TimeSpan EndPosition
		{
			get { return _inStream.EndPosition; }
			set { _inStream.EndPosition = value; }
		}

		public TimeSpan CurrentPosition { get; set; }

		public PlaybackState PlaybackState
		{
			get { return _waveOut == null ? PlaybackState.Stopped : _waveOut.PlaybackState; }
		}

		public void Dispose()
		{
			CloseWaveOut();
			CloseInStream();

			if (_timer != null)
			{
				_timer.Stop();
				_timer.Dispose();
				_timer = null;
			}
		}

		private void CloseInStream()
		{
			if (_wasStreamCreatedLocally && _inStream != null)
			{
				_inStream.Dispose();
				_inStream = null;
			}
		}

		private void CloseWaveOut()
		{
			_timer.Stop();

			if (_waveOut != null)
			{
				_waveOut.PlaybackStopped -= HandleWaveOutPlaybackStopped;
				_waveOut.Dispose();
				_waveOut = null;
			}
		}
	}
}
