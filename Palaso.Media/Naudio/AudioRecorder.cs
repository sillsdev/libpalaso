﻿using System;
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
		event EventHandler SelectedDeviceChanged;
		double MicrophoneLevel { get; set; }
		RecordingState RecordingState { get; }
		/// <summary>Fired when the transition from recording to monitoring is complete</summary>
		event Action<IAudioRecorder, ErrorEventArgs> Stopped;
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
		protected double _microphoneLevel = -1;//unknown
		protected RecordingState _recordingState = RecordingState.NotYetStarted;
		protected WaveFormat _recordingFormat;
		protected RecordingProgressEventArgs _recProgressEventArgs = new RecordingProgressEventArgs();
		protected PeakLevelEventArgs _peakLevelEventArgs = new PeakLevelEventArgs();
		protected double _prevRecordedTime;

		public SampleAggregator SampleAggregator { get; protected set; }
		private RecordingDevice _selectedDevice;

		public RecordingDevice SelectedDevice
		{
			get
			{
				lock (this)
				{
					return _selectedDevice;
				}
			}
			set
			{
				lock (this)
				{
					if (_selectedDevice == value)
						return;
					if (IsRecording)
						throw new InvalidOperationException(
							"Cannot switch recording devices while recording is in progress.");
					if (_waveIn != null)
					{
						/// Switch device after we have started monitoring, typically because the user plugged in a new one.
						/// The usual way to achieve this is to display a RecordingDeviceIndicator connected to this Recorder.
						/// See the implementation of RecordingDeviceIndicator.checkNewMicTimer_Tick.
						CloseWaveIn();
					}
					_selectedDevice = value;
					if (RecordingState != RecordingState.NotYetStarted && _selectedDevice != null)
						BeginMonitoring();
					if (SelectedDeviceChanged != null)
						SelectedDeviceChanged(this, new EventArgs());
				}
			}
		}

		public event EventHandler SelectedDeviceChanged;
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
		public event Action<IAudioRecorder, ErrorEventArgs> Stopped;

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
			lock (this)
			{
				if (_fileWriterThread != null)
					_fileWriterThread.Stop();
				CloseWaveIn();
				RecordingState = RecordingState.NotYetStarted;
			}
		}

		/// ------------------------------------------------------------------------------------
		protected void AbortRecording()
		{
			RecordedTime = TimeSpan.Zero;
			RecordingState = RecordingState.Monitoring;
			_fileWriterThread.Abort();
			if (Stopped != null)
				Stopped(this, new ErrorEventArgs(new Exception("NAudio recording stopped unexpectedly")));
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void CloseWaveIn()
		{
			if (_waveIn != null)
			{
				try { _waveIn.StopRecording(); }
				catch { /* It's amazing all the bizarre things that can go wrong */ }
				_waveIn.DataAvailable -= waveIn_DataAvailable;
				_waveIn.RecordingStopped -= OnRecordingStopped;
				try { _waveIn.Dispose(); }
				catch { /* It's amazing all the bizarre things that can go wrong */ }
				_waveIn = null;
				RecordingState = RecordingState.Stopped;
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
			lock (this)
			{
				if (_waveIn != null || (_recordingState != RecordingState.NotYetStarted && _recordingState != RecordingState.Stopped))
					throw new InvalidOperationException("only call this once for a new WaveIn device");
				try
				{
					Debug.Assert(_waveIn == null);
					_waveIn = new WaveIn();
					InitializeWaveIn();

					_waveIn.DataAvailable += waveIn_DataAvailable;
					_waveIn.RecordingStopped += OnRecordingStopped;
					_waveIn.WaveFormat = _recordingFormat;
					try
					{
						_waveIn.StartRecording();
					}
					catch (MmException error)
					{
						if (error.Result != MmResult.AlreadyAllocated)
							//TODO: I get this most of the time, but I don't know how to prevent it... maybe it's a hold over from previous runs? In which case, we need to make this disposable and stop the recording
							throw;
					}

					ConnectWithVolumeControl();
					RecordingState = RecordingState.Monitoring;
				}
				catch (Exception e)
				{
					CloseWaveIn();
					ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), e,
						 "There was a problem starting up volume monitoring.");
				}
			}
		}

		/// <summary>
		/// As of NAudio 1.6, this can occur because something went wrong, for example, someone unplugged the microphone.
		/// We won't get a DataAvailable notification, so make sure we aren't stuck in a state where we expect it.
		/// Note that we can get this even when nothing but monitoring is happening.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void OnRecordingStopped(object sender, StoppedEventArgs eventArgs)
		{
			lock (this)
			{
				if (RecordingState == RecordingState.Stopped || RecordingState == RecordingState.NotYetStarted)
					return;
				if (eventArgs.Exception != null)
				{
					// Something went wrong, typically the user unplugged the microphone.
					// We are not going to get any more data until we BeginMonitoring again.
					// So make sure we get into a state where that can be done.
					if (_fileWriterThread != null)
						AbortRecording();
					CloseWaveIn();
				}
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
		/// as far as naudio is concerned, we are still "recording" (i.e., accepting waveIn
		/// data), but we want to stop writing the data to the file.
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
				Stopped(this, null);
		}

		/// ------------------------------------------------------------------------------------
		public virtual void BeginRecording(string waveFileName)
		{
			BeginRecording(waveFileName, false);
		}

		/// ------------------------------------------------------------------------------------
		public virtual void BeginRecording(string waveFileName, bool appendToFile)
		{
			if (_recordingState == RecordingState.NotYetStarted)
				BeginMonitoring();

			if (_recordingState != RecordingState.Monitoring)
			{
				throw new InvalidOperationException("Can't begin recording while we are in this state: " + _recordingState.ToString());
			}

			lock (this)
			{
				if (_waveInBuffersChanged)
				{
					CloseWaveIn();
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
					Debug.WriteLine("Setting RequestedStop");
					// Don't stop because we'll lose any buffer(s) that have not been processed.
					// Then when we re-start, NAudio can crash because the buffers for which it has
					// queued messages will be disposed
					//   _waveIn.StopRecording();
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void ConnectWithVolumeControl()
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

						//REVIEW: was this (from original author). This would boost us to the max (as the original code had a 100 for _microphoneLevel)
						//MicrophoneLevel = _microphoneLevel;
						//Now, we do the opposite. Give preference to the system volume. If your application supports independent volume setting, that's
						//fine, but you'll have to explicity set it via the MicrophoneLevel property.

						_microphoneLevel = _volumeControl.Percent;

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
										//REVIEW: was this (from original author). This would boost us to the max (as the original code had a 100 for _microphoneLevel)
										//MicrophoneLevel = _microphoneLevel;
										//Now, we do the opposite. Give preference to the system volume. If your application supports independent volume setting, that's
										//fine, but you'll have to explicity set it via the MicrophoneLevel property.

										_microphoneLevel = _volumeControl.Percent;

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
				if (_volumeControl == null)
					ConnectWithVolumeControl();

				if (_volumeControl != null) //did we get it?
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
				{
					Debug.WriteLine("Writing " + bytesRecorded + " bytes of data to file");
					hitMaximumFileSize = !WriteToFile(buffer, bytesRecorded);
				}

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
						Debug.WriteLine("Transition to monitoring from DataAvailable");
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
