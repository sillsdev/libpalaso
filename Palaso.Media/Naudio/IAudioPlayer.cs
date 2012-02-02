using System;

namespace Palaso.Media.Naudio
{
	public interface IAudioPlayer : IDisposable
	{
		void LoadFile(string path);
		void Play();
		void Stop();
		TimeSpan CurrentPosition { get; set; }
		TimeSpan StartPosition { get; set; }
		TimeSpan EndPosition { get; set; }
		event EventHandler PlaybackStarted;
		event EventHandler Stopped;
		event EventHandler<PlaybackProgressEventArgs> PlaybackProgress;
	}
}
