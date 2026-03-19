using System;
using NAudio.Wave;

#if NET462 || NET48
namespace SIL.Media
{
	internal class NAudioWaveOutEventAdapter : INAudioOutputDevice
	{
		private readonly WaveOutEvent _impl;

		public event EventHandler<StoppedEventArgs> PlaybackStopped;

		internal NAudioWaveOutEventAdapter()
		{
			_impl = new WaveOutEvent();
			_impl.PlaybackStopped += PlaybackStoppedImplementation;
		}

		private void PlaybackStoppedImplementation(object sender, StoppedEventArgs e)
		{
			PlaybackStopped?.Invoke(this, e);
		}

		public void Dispose()
		{
			_impl.Dispose();
		}

		public PlaybackState PlaybackState => _impl.PlaybackState;
		public void Init(IWaveProvider provider)
		{
			_impl.Init(provider);
		}

		public void Play()
		{
			_impl.Play();
		}

		public void Stop()
		{
			_impl.Stop();
		}
	}
}
#endif