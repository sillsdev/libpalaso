using System;

namespace SIL.Media
{
	public class PlaybackProgressEventArgs : EventArgs
	{
		public TimeSpan PlaybackPosition;
	}

	public interface IAudioPlayer : IDisposable
	{
		void LoadFile(string path);
		void StartPlaying();
		void Stop();
		TimeSpan CurrentPosition { get; set; }
		TimeSpan StartPosition { get; set; }
		TimeSpan EndPosition { get; set; }
		event EventHandler PlaybackStarted;
		event EventHandler Stopped;
		event EventHandler<PlaybackProgressEventArgs> PlaybackProgress;
	}
}
