using System;
using System.Diagnostics;
using System.IO;
using NAudio;
using NAudio.Wave;
using NAudio.Mixer;
using Palaso.Reporting;

namespace Palaso.Media.Naudio
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
		double MicrophoneLevel { get; set; }
		RecordingState RecordingState { get; }
		event EventHandler Stopped;
		WaveFormat RecordingFormat { get; set; }
		TimeSpan RecordedTime { get; }
	}

	public class AudioRecorder : IAudioRecorder, IDisposable
	{
		private readonly int _maxMinutes;

		/// <summary>
		/// This guy is always working, whether we're playing, recording, or just idle (monitoring)
		/// </summary>
		WaveIn _waveIn;

		/// <summary>
		/// This guy is disposed each time the client calls stop to stop recording and gets recreated
		/// each time the client starts recording (i.e. using BeginRecording).
		/// </summary>
		WaveFileWriter _writer;

		SampleAggregator _sampleAggregator;
		UnsignedMixerControl _volumeControl;
		double _microphoneLevel = 100;
		RecordingState _recordingState = RecordingState.Stopped;
		WaveFormat _recordingFormat;
		RecordingProgressEventArgs _recProgressEventArgs = new RecordingProgressEventArgs();
		PeakLevelEventArgs _peakLevelEventArgs = new PeakLevelEventArgs();
		double _prevRecordedTime;

		public RecordingDevice SelectedDevice { get; set; }
		public TimeSpan RecordedTime { get; set; }
		public event EventHandler<PeakLevelEventArgs> PeakLevelChanged = delegate { };
		public event EventHandler<RecordingProgressEventArgs> RecordingProgress = delegate { };
		public event EventHandler RecordingStarted = delegate { };
		public event EventHandler Stopped = delegate { };

		/// <summary>
		///
		/// </summary>
		/// <param name="maxMinutes">REVIW: why does this max time even exist?  I don't see that it affects buffer size</param>
		public AudioRecorder(int maxMinutes)
		{
			_maxMinutes = maxMinutes;
			_sampleAggregator = new SampleAggregator();
			_sampleAggregator.MaximumCalculated += delegate
			{
				//var peakLevel = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
				_peakLevelEventArgs.Level = _sampleAggregator.maxValue;
				PeakLevelChanged.Invoke(this, _peakLevelEventArgs);
			};

			RecordingFormat = new WaveFormat(44100, 1);
		}

		/// ------------------------------------------------------------------------------------
		public void Dispose()
		{
			CloseWriter();
		}

		/// ------------------------------------------------------------------------------------
		private void CloseWriter()
		{
			if (_writer != null)
			{
				_writer.Dispose();
				_writer = null;
			}
		}

		public WaveFormat RecordingFormat
		{
			get { return _recordingFormat; }
			set
			{
				_recordingFormat = value;
				_sampleAggregator.NotificationCount = value.SampleRate / 10;
			}
		}

		public void BeginMonitoring()
		{
			Debug.Assert(_waveIn == null, "only call this once");
			try
			{
				if (_recordingState != RecordingState.Stopped)
				{
					throw new InvalidOperationException(
						"Can't begin monitoring while we are in this state: " + _recordingState.ToString());
				}
				Debug.Assert(_waveIn == null);
				_waveIn = new WaveIn();
				_waveIn.DeviceNumber = SelectedDevice.DeviceNumber;

				_waveIn.DataAvailable += waveIn_DataAvailable;
				//_waveIn.RecordingStopped += new EventHandler(waveIn_RecordingStopped);
				_waveIn.WaveFormat = _recordingFormat;
				try
				{
					_waveIn.StartRecording();
				}
				catch (MmException error)
				{
					if (error.Result != MmResult.AlreadyAllocated)  //TODO: I get this most of the time, but I don't know how to prevent it... maybe it's a hold over from previous runs? In which case, we need to make this disposable and stop the recording
						throw;
				}

				TryGetVolumeControl();
				RecordingState = RecordingState.Monitoring;
			}
			catch (Exception e)
			{
				ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), e, "There was a problem starting up volume monitoring.");
				if (_waveIn != null)
				{
					_waveIn.Dispose();
					_waveIn = null;
				}
			}
		}

		/// <summary>
		/// as far as naudio is concerned, we are still "recording", but we aren't writing this file anymore
		/// </summary>
		void TransitionFromRecordingToMonitoring()
		{
			RecordedTime = TimeSpan.FromSeconds((double)_writer.Length / _writer.WaveFormat.AverageBytesPerSecond);
			RecordingState = RecordingState.Monitoring;
			CloseWriter();
			Stopped.Invoke(this, EventArgs.Empty);
		}

/*        void waveIn_RecordingStopped(object sender, EventArgs e)
		{
			RecordedTime = TimeSpan.FromSeconds((double)_writer.Length / _writer.WaveFormat.AverageBytesPerSecond);
			RecordingState = RecordingState.Monitoring;
			if (_writer != null)
			{
				_writer.Dispose();
				_writer = null;
			}
			Stopped(this, EventArgs.Empty);
		}
*/
		public void BeginRecording(string waveFileName)
		{
			BeginRecording(waveFileName, false);
		}

		public void BeginRecording(string waveFileName, bool appendToFile)
		{
			if (_recordingState != RecordingState.Monitoring)
			{
				throw new InvalidOperationException("Can't begin recording while we are in this state: " + _recordingState.ToString());
			}

			if (_writer != null)
				CloseWriter();

			if (!File.Exists(waveFileName) || !appendToFile)
				_writer = new WaveFileWriter(waveFileName, _recordingFormat);
			else
			{
				var buffer = GetAudioBufferToAppendTo(waveFileName);
				_writer = new WaveFileWriter(waveFileName, _recordingFormat);
				_writer.Write(buffer, 0, buffer.Length);
			}

			_prevRecordedTime = 0d;
			RecordingState = RecordingState.Recording;
			RecordingStarted.Invoke(this, EventArgs.Empty);
		}

		/// ------------------------------------------------------------------------------------
		private byte[] GetAudioBufferToAppendTo(string waveFileName)
		{
			using (var stream = new WaveFileReader(waveFileName))
			{
				var buffer = new byte[stream.Length];
				var count = (int)Math.Min(stream.Length, int.MaxValue);
				int offset = 0;
				while (stream.Read(buffer, offset, count) > 0)
					offset += count;

				stream.Close();
				return buffer;
			}
		}

		public void Stop()
		{
			if (_recordingState == RecordingState.Recording)
			{
				RecordingState = RecordingState.RequestedStop;
				//_waveIn.StopRecording();
			}

			TransitionFromRecordingToMonitoring();
		}

		private void TryGetVolumeControl()
		{
			int waveInDeviceNumber = _waveIn.DeviceNumber;
			if (Environment.OSVersion.Version.Major >= 6) // Vista and over
			{
				var mixerLine = _waveIn.GetMixerLine();
				//new MixerLine((IntPtr)waveInDeviceNumber, 0, MixerFlags.WaveIn);
				foreach (var control in mixerLine.Controls)
				{
					if (control.ControlType == MixerControlType.Volume)
					{
						_volumeControl = control as UnsignedMixerControl;
						MicrophoneLevel = _microphoneLevel;
						break;
					}
				}
			}
			else
			{
				var mixer = new Mixer(waveInDeviceNumber);
				foreach (var destination in mixer.Destinations)
				{
					if (destination.ComponentType == MixerLineComponentType.DestinationWaveIn)
					{
						foreach (var source in destination.Sources)
						{
							if (source.ComponentType == MixerLineComponentType.SourceMicrophone)
							{
								foreach (var control in source.Controls)
								{
									if (control.ControlType == MixerControlType.Volume)
									{
										_volumeControl = control as UnsignedMixerControl;
										MicrophoneLevel = _microphoneLevel;
										break;
									}
								}
							}
						}
					}
				}
			}

		}

		public double MicrophoneLevel
		{
			get
			{
				return _microphoneLevel;
			}
			set
			{
				_microphoneLevel = value;
				if (_volumeControl != null)
				{
					_volumeControl.Percent = value;
				}
			}
		}

		public SampleAggregator SampleAggregator
		{
			get
			{
				return _sampleAggregator;
			}
		}

		public RecordingState RecordingState
		{
			get
			{
				return _recordingState;
			}
			private set
			{
				_recordingState = value;
				Debug.WriteLine("recorder state--> " + value.ToString());
			}
		}

		void waveIn_DataAvailable(object sender, WaveInEventArgs e)
		{
 /*original from codeplex
  byte[] buffer = e.Buffer;
			int bytesRecorded = e.BytesRecorded;
			WriteToFile(buffer, bytesRecorded);

			for (int index = 0; index < e.BytesRecorded; index += 2)
			{
				short sample = (short)((buffer[index + 1] << 8) |
										buffer[index + 0]);
				float sample32 = sample / 32768f;
				_sampleAggregator.Add(sample32);
			}
  */

			//David's version:

			var buffer = e.Buffer;
			int bytesRecorded = e.BytesRecorded;
			WriteToFile(buffer, bytesRecorded);

			var bytesPerSample = _waveIn.WaveFormat.BitsPerSample / 8;

			// It appears the data only occupies 2 bytes of those in a sample and that
			// those 2 are always the last two in each sample. The other bytes are zero
			// filled. Therefore, when getting those two bytes, the first index into a
			// sample needs to be 0 for 16 bit samples, 1 for 24 bit samples and 2 for
			// 32 bit samples. I'm not sure what to do for 8 bit samples. I could never
			// figure out the correct conversion of a byte in an 8 bit per sample buffer
			// to a float sample value. However, I doubt folks are going to be recording
			// at 8 bits/sample so I'm ignoring that problem.
			for (var index = bytesPerSample - 2; index < bytesRecorded - 1; index += bytesPerSample)
			{
				var sample = (short)((buffer[index + 1] << 8) | buffer[index]);
				var sample32 = sample / 32768f;
				SampleAggregator.Add(sample32);
			}

			if (_writer == null)
				return;

			// Only fire the progress event every 10th of a second.
			var currRecordedTime = (double)_writer.Position / _writer.WaveFormat.AverageBytesPerSecond;
			if (currRecordedTime - _prevRecordedTime >= 0.05d)
			{
				_prevRecordedTime = currRecordedTime;
				_recProgressEventArgs.RecordedLength = TimeSpan.FromSeconds(currRecordedTime);
				RecordingProgress.Invoke(this, _recProgressEventArgs);
			}
		}

		private void WriteToFile(byte[] buffer, int bytesRecorded)
		{
			//REVIEW: why does this max time even exist?  I don't see that it affects buffer size
			long maxFileLength = _recordingFormat.AverageBytesPerSecond * 60 * _maxMinutes;

			if (_recordingState == RecordingState.Recording || _recordingState == RecordingState.RequestedStop)
			{
				int toWrite = (int)Math.Min(maxFileLength - _writer.Length, bytesRecorded);
				if (toWrite > 0)
					_writer.Write(buffer, 0, bytesRecorded);
				else
					Stop();
			}
		}
	}
}
