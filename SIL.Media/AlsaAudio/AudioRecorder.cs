// Copyright (c) 2017-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using NAudio.Wave;

namespace SIL.Media.AlsaAudio
{
	/// <summary>
	/// AudioRecorder rewritten to work on Linux using AlsaAudio instead of Naudio.  The things I can't
	/// figure out how to implement (or which aren't needed by AlsaAudio) have do-nothing stubs to allow
	/// source code compatibility.  This implementation may or may not be useful outside of Bloom.
	/// </summary>
	[PublicAPI]
	public class AudioRecorder : IAudioRecorder, IDisposable
	{
		// variables copied from SIL.Media.Naudio implementation.
		protected readonly int _maxMinutes;
		protected RecordingState _recordingState = RecordingState.NotYetStarted;
		//private DateTime _recordingStartTime;
		//private DateTime _recordingStopTime;
		public TimeSpan RecordedTime { get; set; }
#pragma warning disable CS0067
		public event EventHandler<PeakLevelEventArgs> PeakLevelChanged;				// IGNORED, not used anyway
		public event EventHandler<RecordingProgressEventArgs> RecordingProgress;	// IGNORED, not used anyway
		public event EventHandler RecordingStarted;									// IGNORED, not used anyway
		public event EventHandler SelectedDeviceChanged;							// IGNORED, not used anyway
#pragma warning restore CS0067
		/// <summary>Fired when the transition from recording to monitoring is complete</summary>
		public event Action<IAudioRecorder, ErrorEventArgs> Stopped;

		// variables added for this Alsa implementation.
		private AudioAlsaSession _session;
		private Object lockObj = new Object();

		public AudioRecorder(int maxMinutes)
		{
			_maxMinutes = maxMinutes;	// ignored -- not sure what to do with this.
			RecordingFormat = new WaveFormat(44100, 1);
			SelectedDevice = RecordingDevice.DefaultDevice;
			MicrophoneLevel = -1;	// unknown level.
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_session != null && _session.IsRecording)
					_session.StopRecordingAndSaveAsWav();
				_session = null;
			}
			RecordingState = RecordingState.NotYetStarted;
		}

		public void Dispose()
		{
			lock (lockObj)
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

		public virtual RecordingState RecordingState
		{
			get
			{
				lock (lockObj)
				{
					return _recordingState;
				}
			}
			protected set
			{
				lock (lockObj)
				{
					_recordingState = value;
					Debug.WriteLine("recorder state--> " + value.ToString());
				}
			}
		}

		public IRecordingDevice SelectedDevice { get; set; }

		public void BeginMonitoring()
		{
			lock (lockObj)
			{
				// Alsa is too simple-minded to really need this, but let's play along.
				RecordingState = RecordingState.Monitoring;
			}
		}

		public virtual void BeginRecording(string waveFileName)
		{
			if (_recordingState == RecordingState.NotYetStarted)
				BeginMonitoring();
			if (_recordingState != RecordingState.Monitoring)
				throw new InvalidOperationException("Can't begin recording while we are in this state: " + _recordingState.ToString());

			lock (lockObj)
			{
				RecordingState = RecordingState.Recording;
				_session = new AudioAlsaSession(waveFileName);
				var device = "default";
				if (SelectedDevice != null && !SelectedDevice.Equals(RecordingDevice.DefaultDevice))
					device = $"plughw:{SelectedDevice.DeviceNumber}";
				_session.SetInputDevice(device);
				_session.StartRecording((uint)RecordingFormat.SampleRate, (ushort)RecordingFormat.Channels);
			}
		}

		public virtual void Stop()
		{
			lock (lockObj)
			{
				if (_recordingState == RecordingState.Recording)
				{
					// _recordingStopTime = DateTime.Now;
					RecordingState = RecordingState.RequestedStop;
					Debug.WriteLine("Setting RequestedStop");
					_session.StopRecordingAndSaveAsWav();
					RecordingState = RecordingState.Monitoring;		// not really, but who cares?
					if (Stopped != null)
						Stopped(this, null);
				}
			}
		}

		public static void TrimWavFile(string inPath, string outPath, TimeSpan cutFromStart, TimeSpan cutFromEnd, TimeSpan minimumDesiredDuration)
		{
			// TODO/REVIEW: what if we just ignore the trimming function on Linux for now?  I'm not sure how to implement it on Linux.
			File.Copy(inPath, outPath);
		}

		// TODO/REVIEW: I don't know that we can do anything with this on Linux.
		public virtual double MicrophoneLevel { get; set; }

		public virtual WaveFormat RecordingFormat { get; set; }
	}
}
