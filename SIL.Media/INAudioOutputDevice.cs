using System;
using NAudio.Wave;

namespace SIL.Media
{
	internal interface INAudioOutputDevice : IDisposable
	{
		event EventHandler<StoppedEventArgs> PlaybackStopped;
		PlaybackState PlaybackState { get; }
		void Init(IWaveProvider provider);
		void Play();
		void Stop();
	}
}
