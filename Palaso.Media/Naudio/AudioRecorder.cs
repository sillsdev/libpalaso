using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
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
		/// <summary>Fired when the transition from recording to monitoring is complete</summary>
		event EventHandler Stopped;
		WaveFormat RecordingFormat { get; set; }
		TimeSpan RecordedTime { get; }
	}

	public class AudioRecorder : IAudioRecorder, IDisposable
	{
		protected readonly int _maxMinutes;

		/// <summary>
		/// This guy is always working, whether we're playing, recording, or just idle (monitoring)
		/// </summary>
		protected WaveIn _waveIn;

		/// <summary>
		/// This guy is disposed each time the client calls stop to stop recording and gets recreated
		/// each time the client starts recording (i.e. using BeginRecording).
		/// </summary>
		private FileWriterThread _fileWriterThread;

		protected UnsignedMixerControl _volumeControl;
		protected double _microphoneLevel = 100;
		protected RecordingState _recordingState = RecordingState.Stopped;
		protected WaveFormat _recordingFormat;
		protected RecordingProgressEventArgs _recProgressEventArgs = new RecordingProgressEventArgs();
		protected PeakLevelEventArgs _peakLevelEventArgs = new PeakLevelEventArgs();
		protected double _prevRecordedTime;

		public SampleAggregator SampleAggregator { get; protected set; }
		public RecordingDevice SelectedDevice { get; set; }
		public TimeSpan RecordedTime { get; set; }

		private int _bufferSize = -1;
		private int _bufferCount = -1;
		private bool _waveInBuffersChanged;
		private DateTime _recordingStartTime;
		private DateTime _recordingStopTime;
		private int _bytesRecorded;

		public event EventHandler<PeakLevelEventArgs> PeakLevelChanged;
		public event EventHandler<RecordingProgressEventArgs> RecordingProgress;
		public event EventHandler RecordingStarted;
		/// <summary>Fired when the transition from recording to monitoring is complete</summary>
		public event EventHandler Stopped;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="maxMinutes">REVIW: why does this max time even exist?  I don't see that it affects buffer size</param>
		/// ------------------------------------------------------------------------------------
		public AudioRecorder(int maxMinutes)
		{
			_maxMinutes = maxMinutes;
			SampleAggregator = new SampleAggregator();
			SampleAggregator.MaximumCalculated += delegate
			{
				_peakLevelEventArgs.Level = SampleAggregator.maxValue;
				if (PeakLevelChanged != null)
					PeakLevelChanged.BeginInvoke(this, _peakLevelEventArgs, null, null);
			};

			RecordingFormat = new WaveFormat(44100, 1);
		}

		/// ------------------------------------------------------------------------------------
		public virtual void Dispose()
		{
			if (_fileWriterThread != null)
				_fileWriterThread.Stop();
			CloseWaveIn();
		}

		/// ------------------------------------------------------------------------------------
		protected void AbortRecording()
		{
			RecordedTime = TimeSpan.Zero;
			RecordingState = RecordingState.Monitoring;
			_fileWriterThread.Abort();
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void CloseWaveIn()
		{
			if (_waveIn != null)
			{
				_waveIn.DataAvailable -= waveIn_DataAvailable;
				try { _waveIn.Dispose(); }
				catch { }
				_waveIn = null;
			}
		}

		/// ------------------------------------------------------------------------------------
		public virtual WaveFormat RecordingFormat
		{
			get { return _recordingFormat; }
			set
			{
				_recordingFormat = value;
				SampleAggregator.NotificationCount = value.SampleRate / 10;
			}
		}

		/// ------------------------------------------------------------------------------------
		public virtual void BeginMonitoring()
		{
			Debug.Assert(_waveIn == null, "only call this once");
			try
			{
				lock (this)
				{
					if (_recordingState != RecordingState.Stopped)
					{
						throw new InvalidOperationException(
							"Can't begin monitoring while we are in this state: " + _recordingState.ToString());
					}
					Debug.Assert(_waveIn == null);
					_waveIn = new WaveIn();
					InitializeWaveIn();

					_waveIn.DataAvailable += waveIn_DataAvailable;
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
			}
			catch (Exception e)
			{
				CloseWaveIn();
				ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), e, "There was a problem starting up volume monitoring.");
			}
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void InitializeWaveIn()
		{
			_waveIn.DeviceNumber = SelectedDevice.DeviceNumber;
			if (_bufferCount > 0)
				_waveIn.NumberOfBuffers = _bufferCount;
			if (_bufferSize > 0)
				_waveIn.BufferMilliseconds = _bufferSize;
			_waveInBuffersChanged = false;

			// Get the defaults (or previous values)
			_bufferCount = _waveIn.NumberOfBuffers;
			_bufferSize = _waveIn.BufferMilliseconds;
		}

		/// ------------------------------------------------------------------------------------
		public int NumberOfBuffers
		{
			set
			{
				_bufferCount = value;
				_waveInBuffersChanged = true;
			}
		}

		/// ------------------------------------------------------------------------------------
		public int BufferMilliseconds
		{
			set
			{
				_bufferSize = value;
				_waveInBuffersChanged = true;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// as far as naudio is concerned, we are still "recording", but we aren't writing this file anymore
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected virtual void TransitionFromRecordingToMonitoring()
		{
			RecordingState = RecordingState.Stopping;
			_fileWriterThread.Stop();
			RecordedTime = _fileWriterThread.RecordedTimeInSeconds;
			_fileWriterThread = null;
			RecordingState = RecordingState.Monitoring;
			if (Stopped != null)
				Stopped(this, EventArgs.Empty);
		}

		/// ------------------------------------------------------------------------------------
		public virtual void BeginRecording(string waveFileName)
		{
			BeginRecording(waveFileName, false);
		}

		/// ------------------------------------------------------------------------------------
		public virtual void BeginRecording(string waveFileName, bool appendToFile)
		{
			if (_recordingState != RecordingState.Monitoring)
			{
				throw new InvalidOperationException("Can't begin recording while we are in this state: " + _recordingState.ToString());
			}

			lock (this)
			{
				if (_waveInBuffersChanged)
				{
					CloseWaveIn();
					RecordingState = RecordingState.Stopped;
					BeginMonitoring();
				}

				_bytesRecorded = 0;

				WaveFileWriter writer;
				if (!File.Exists(waveFileName) || !appendToFile)
					writer = new WaveFileWriter(waveFileName, _recordingFormat);
				else
				{
					var buffer = GetAudioBufferToAppendTo(waveFileName);
					writer = new WaveFileWriter(waveFileName, _recordingFormat);
					writer.Write(buffer, 0, buffer.Length);
				}
				_fileWriterThread = new FileWriterThread(writer);

				_recordingStartTime = DateTime.Now;
				_prevRecordedTime = 0d;
				RecordingState = RecordingState.Recording;
			}
			if (RecordingStarted != null)
				RecordingStarted(this, EventArgs.Empty);
		}

		/// ------------------------------------------------------------------------------------
		protected virtual byte[] GetAudioBufferToAppendTo(string waveFileName)
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

		/// ------------------------------------------------------------------------------------
		public virtual void Stop()
		{
			lock (this)
			{
				if (_recordingState == RecordingState.Recording)
				{
					_recordingStopTime = DateTime.Now;
					RecordingState = RecordingState.RequestedStop;
					// Don't stop because we'll lose any buffer(s) that have not been processed.
					// Then when we re-start, NAudio can crash because the buffers for which it has
					// queued messages will be disposed
					//   _waveIn.StopRecording();
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void TryGetVolumeControl()
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

		/// ------------------------------------------------------------------------------------
		public virtual double MicrophoneLevel
		{
			get { return _microphoneLevel; }
			set
			{
				_microphoneLevel = value;
				if (_volumeControl != null)
					_volumeControl.Percent = value;
			}
		}

		/// ------------------------------------------------------------------------------------
		public virtual bool IsRecording
		{
			get
			{
				lock (this)
				{
					return _recordingState == RecordingState.Recording ||
						_recordingState == RecordingState.RequestedStop ||
						_recordingState == RecordingState.Stopping;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		public virtual RecordingState RecordingState
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

		/// ------------------------------------------------------------------------------------
		protected virtual void waveIn_DataAvailable(object sender, WaveInEventArgs e)
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
			lock (this)
			{
				var buffer = e.Buffer;
				int bytesRecorded = e.BytesRecorded;
				bool hitMaximumFileSize = false;
				if (_recordingState == RecordingState.Recording || _recordingState == RecordingState.RequestedStop)
					hitMaximumFileSize = !WriteToFile(buffer, bytesRecorded);

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

				if (_fileWriterThread == null)
					return;

				// Only fire the progress event every 10th of a second.
				var currRecordedTime = (double)_bytesRecorded / _recordingFormat.AverageBytesPerSecond;
				if (currRecordedTime - _prevRecordedTime >= 0.05d)
				{
					_prevRecordedTime = currRecordedTime;
					_recProgressEventArgs.RecordedLength = TimeSpan.FromSeconds(currRecordedTime);
					if (RecordingProgress != null)
						RecordingProgress.BeginInvoke(this, _recProgressEventArgs, null, null);
				}

				if (RecordingState == RecordingState.RequestedStop)
				{
					if (DateTime.Now > _recordingStopTime.AddSeconds(2) || hitMaximumFileSize ||
						_recordingStartTime.AddSeconds(currRecordedTime) >= _recordingStopTime)
					{
						TransitionFromRecordingToMonitoring();
					}
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		private bool WriteToFile(byte[] buffer, int bytesRecorded)
		{
			if (_bytesRecorded < _recordingFormat.AverageBytesPerSecond * 60 * _maxMinutes)
			{
				_bytesRecorded += buffer.Length;
				_fileWriterThread.AddData(buffer, bytesRecorded);

				return true;
			}
			Stop();
			return false;
		}
	}
}
