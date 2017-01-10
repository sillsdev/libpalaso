using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

#if __MonoCS__
namespace SIL.Media.AlsaAudio
{
	/// <summary>
	/// AudioRecorder rewritten to work on Linux using AlsaAudio instead of Naudio.  The things I can't
	/// figure out how to implement (or which aren't needed by AlsaAudio) have do-nothing stubs to allow
	/// source code compatibility.  This implementation may or may not be useful outside of Bloom.
	/// </summary>
	public class AudioRecorder : SIL.Media.Naudio.IAudioRecorder, IDisposable
	{
		// variables copied from SIL.Media.Naudio implementation.
		protected readonly int _maxMinutes;
		protected SIL.Media.Naudio.RecordingState _recordingState = SIL.Media.Naudio.RecordingState.NotYetStarted;
		protected WaveFormat _recordingFormat;
		protected double _microphoneLevel = -1;//unknown
		private DateTime _recordingStartTime;
		private DateTime _recordingStopTime;
		public TimeSpan RecordedTime { get; set; }
		public event EventHandler<SIL.Media.Naudio.PeakLevelEventArgs> PeakLevelChanged;				// IGNORED, not used anyway
		public event EventHandler<SIL.Media.Naudio.RecordingProgressEventArgs> RecordingProgress;	// IGNORED, not used anyway
		public event EventHandler RecordingStarted;									// IGNORED, not used anyway
		public event EventHandler SelectedDeviceChanged;							// IGNORED, not used anyway
		/// <summary>Fired when the transition from recording to monitoring is complete</summary>
		public event Action<SIL.Media.Naudio.IAudioRecorder, ErrorEventArgs> Stopped;

		// variables added for this Bloom.ToPalaso implementation.
		private AudioAlsaSession _session;

		public AudioRecorder(int maxMinutes)
		{
			_maxMinutes = maxMinutes;	// ignored -- not sure what to do with this.
			RecordingFormat = new WaveFormat(44100, 1);
			SelectedDevice = RecordingDevice.DefaultDevice;
		}

		public virtual void Dispose()
		{
			lock (this)
			{
				RecordingState = SIL.Media.Naudio.RecordingState.NotYetStarted;
			}
		}

		public virtual SIL.Media.Naudio.RecordingState RecordingState
		{
			get
			{
				lock (this)
				{
					return _recordingState;
				}
			}
			protected set
			{
				lock (this)
				{
					_recordingState = value;
					Debug.WriteLine("recorder state--> " + value.ToString());
				}
			}
		}

		RecordingDevice _selectedDevice;
		public RecordingDevice SelectedDevice
		{
			get { return _selectedDevice; }
			set
			{
				if (_selectedDevice != null && _selectedDevice.Equals(value))
					return;
				_selectedDevice = value;
			}
		}

		public void BeginMonitoring()
		{
			lock (this)
			{
				// Alsa is too simple-minded to really need this, but let's play along.
				RecordingState = SIL.Media.Naudio.RecordingState.Monitoring;
			}
		}

		public virtual void BeginRecording(string waveFileName)
		{
			if (_recordingState == SIL.Media.Naudio.RecordingState.NotYetStarted)
				BeginMonitoring();
			if (_recordingState != SIL.Media.Naudio.RecordingState.Monitoring)
				throw new InvalidOperationException("Can't begin recording while we are in this state: " + _recordingState.ToString());

			lock (this)
			{
				RecordingState = SIL.Media.Naudio.RecordingState.Recording;
				_session = new AudioAlsaSession(waveFileName);
				var device = String.Format("plughw:{0}", SelectedDevice.DeviceNumber);
				if (SelectedDevice.Equals(RecordingDevice.DefaultDevice))
					device = "default";		// otherwise it fails.
				_session.SetInputDevice(device);
				_session.StartRecording((uint)RecordingFormat.SampleRate, (ushort)RecordingFormat.Channels);
			}
		}

		public virtual void Stop()
		{
			lock (this)
			{
				if (_recordingState == SIL.Media.Naudio.RecordingState.Recording)
				{
					_recordingStopTime = DateTime.Now;
					RecordingState = SIL.Media.Naudio.RecordingState.RequestedStop;
					Debug.WriteLine("Setting RequestedStop");
					_session.StopRecordingAndSaveAsWav();
					RecordingState = SIL.Media.Naudio.RecordingState.Monitoring;		// not really, but who cares?
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
		public virtual double MicrophoneLevel
		{
			get { return _microphoneLevel; }
			set { _microphoneLevel = value; }
		}

		public virtual WaveFormat RecordingFormat
		{
			get { return _recordingFormat; }
			set { _recordingFormat = value; }
		}
	}
}
#endif
