using System;
using NAudio.Wave;
using Timer = System.Timers.Timer;

#if NET462 || NET48
namespace SIL.Media
{
	internal class TestNAudioWaveOutEvent : INAudioOutputDevice
	{
		public const int kDelayWhenStopping = 50;
		private readonly double _simulatedMediaTime;
		private ISampleProvider _provider;
		private Timer _timer;
		private object _lock = new object();

		public event EventHandler<StoppedEventArgs> PlaybackStopped;

		internal TestNAudioWaveOutEvent(double simulatedMediaTime = 10000)
		{
			_simulatedMediaTime = simulatedMediaTime;
		}

		public void Dispose()
		{
		}

		public PlaybackState PlaybackState { get; private set; }
		public void Init(IWaveProvider provider)
		{
			if (_provider != null)
				throw new InvalidOperationException("Do not call Init more than once!");
			_provider = provider.ToSampleProvider() ?? throw new ArgumentNullException(nameof(provider));
			PlaybackState = PlaybackState.Stopped;
		}

		public void Play()
		{
			_provider.Take(new TimeSpan(1)); // This will throw if not backed by a legit file.
			lock (_lock)
			{
				_timer = new Timer(_simulatedMediaTime);
				_timer.Elapsed += (sender, args) =>
				{
					lock (_lock)
						MediaPlaybackStopped();
				};
				_timer.Start();
				PlaybackState = PlaybackState.Playing;
			}
		}

		public void Stop()
		{
			lock (_lock)
			{
				if (_provider != null && PlaybackState == PlaybackState.Playing)
					_timer.Interval = kDelayWhenStopping;
				else
					MediaPlaybackStopped();
			}
		}

		private void MediaPlaybackStopped()
		{
			_timer?.Dispose();

			if (PlaybackState == PlaybackState.Playing)
			{
				PlaybackState = PlaybackState.Stopped;
				PlaybackStopped?.Invoke(this, new StoppedEventArgs());
			}
		}
	}
}
#endif