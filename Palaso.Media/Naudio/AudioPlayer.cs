using System;
using NAudio.Wave;

namespace Palaso.Media.Naudio
{
	public class AudioPlayer : IAudioPlayer
	{
		private WaveOut _waveOut;
		private TrimWaveStream _inStream;
		public event EventHandler Stopped;

		public AudioPlayer()
		{
		}

		public void LoadFile(string path)
		{
			CloseWaveOut();
			CloseInStream();
			_inStream = new TrimWaveStream(new WaveFileReader(path));
		}

		public void Play()
		{
			if (PlaybackState == PlaybackState.Stopped)
			{
				CreateWaveOut();
				_inStream.Position = 0;
				_waveOut.Play();
			}
		}

		private void CreateWaveOut()
		{
			if (_waveOut == null)
			{
				_waveOut = new WaveOut();
				_waveOut.Init(_inStream);
				_waveOut.PlaybackStopped += new EventHandler(waveOut_PlaybackStopped);
			}
		}

		void waveOut_PlaybackStopped(object sender, EventArgs e)
		{
			CloseWaveOut();
			CloseInStream();
			if (Stopped != null)
				Stopped.Invoke(sender,e);
		}

		public void Stop()
		{
			_waveOut.Stop();
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
			get
			{
				if (_waveOut == null)
					return PlaybackState.Stopped;
				return _waveOut.PlaybackState;
			}
		}

		public void Dispose()
		{
			CloseWaveOut();
			CloseInStream();
		}

		private void CloseInStream()
		{
			if (_inStream != null)
			{
				_inStream.Dispose();
				_inStream = null;
			}
		}

		private void CloseWaveOut()
		{
			if (_waveOut != null)
			{
				_waveOut.Dispose();
				_waveOut = null;
			}
		}
	}
}
