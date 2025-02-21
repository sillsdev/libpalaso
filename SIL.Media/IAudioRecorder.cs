// Copyright (c) 2017-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using JetBrains.Annotations;
using NAudio.Wave;

namespace SIL.Media
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
		[PublicAPI]
		event EventHandler<RecordingProgressEventArgs> RecordingProgress;
		[PublicAPI]
		event EventHandler<PeakLevelEventArgs> PeakLevelChanged;
		[PublicAPI]
		event EventHandler RecordingStarted;
		void BeginMonitoring();
		[PublicAPI]
		void BeginRecording(string path);
		void Stop();
		IRecordingDevice SelectedDevice { get; set; }
		event EventHandler SelectedDeviceChanged;
		[PublicAPI]
		double MicrophoneLevel { get; set; }
		RecordingState RecordingState { get; }
		/// <summary>Fired when the transition from recording to monitoring is complete</summary>
		event Action<IAudioRecorder, ErrorEventArgs> Stopped;
		WaveFormat RecordingFormat { get; set; }
		TimeSpan RecordedTime { get; }
	}
}
