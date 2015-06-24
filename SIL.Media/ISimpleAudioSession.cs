using System;

namespace SIL.Media
{
	public interface ISimpleAudioSession
	{
		string FilePath { get; }
		void StartRecording();
		void StopRecordingAndSaveAsWav();
		double LastRecordingMilliseconds { get; }
		bool IsRecording { get; }
		bool IsPlaying { get; }
		bool CanRecord { get; }
		bool CanStop { get; }
		bool CanPlay { get; }
		void Play();
		void SaveAsWav(string filePath);
		void StopPlaying();
	}

	public interface ISimpleAudioWithEvents
	{
		event EventHandler PlaybackStopped;
}
}
