// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
#if __MonoCS__
using SIL.Media.AlsaAudio;
#else
using NAudio.Wave;
#endif

namespace SIL.Media.Naudio
{
	public class PeakLevelEventArgs : EventArgs
	{
		public float Level;
	}

	public class RecordingProgressEventArgs : EventArgs
	{
		public TimeSpan RecordedLength;
	}

	public interface IAudioRecorder
	{
		event EventHandler<RecordingProgressEventArgs> RecordingProgress;
		event EventHandler<PeakLevelEventArgs> PeakLevelChanged;
		event EventHandler RecordingStarted;
		void BeginMonitoring();
		void BeginRecording(string path);
		void Stop();
		RecordingDevice SelectedDevice { get; set; }
		event EventHandler SelectedDeviceChanged;
		double MicrophoneLevel { get; set; }
		RecordingState RecordingState { get; }
		/// <summary>Fired when the transition from recording to monitoring is complete</summary>
		event Action<IAudioRecorder, ErrorEventArgs> Stopped;
		WaveFormat RecordingFormat { get; set; }
		TimeSpan RecordedTime { get; }
	}
}
