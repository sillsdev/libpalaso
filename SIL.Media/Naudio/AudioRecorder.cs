using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio;
using NAudio.Mixer;
using NAudio.Wave;
using SIL.Reporting;

namespace SIL.Media.Naudio
{
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
		private IRecordingDevice _selectedDevice;

		public IRecordingDevice SelectedDevice
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
						// Switch device after we have started monitoring, typically because the user plugged in a new one.
						// The usual way to achieve this is to display a RecordingDeviceIndicator connected to this Recorder.
						// See the implementation of RecordingDeviceIndicator.checkNewMicTimer_Tick.
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
		///Constructs an NAudio-based recorder.
		/// </summary>
		/// <param name="maxMinutes">Maximum number of minutes allowed in a recorded file. If a
		/// recording goes beyond this maximum, the recording will stop. (This allows a caller
		/// to prevent insanely large files that might cause issues.)</param>
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
			Stopped?.Invoke(this, new ErrorEventArgs(new Exception("NAudio recording stopped unexpectedly")));
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void CloseWaveIn()
		{
			if (_waveIn != null)
			{
				try
				{
					_waveIn.StopRecording();
				}
				catch (Exception e)
				{
					// It's amazing all the bizarre things that can go wrong.
					Logger.WriteError(e);
				}
				_waveIn.DataAvailable -= waveIn_DataAvailable;
				_waveIn.RecordingStopped -= OnRecordingStopped;
				try
				{
					_waveIn.Dispose();
				}
				catch (Exception e)
				{
					// It's amazing all the bizarre things that can go wrong.
					Logger.WriteError(e);
				}
				_waveIn = null;
				RecordingState = RecordingState.Stopped;
			}
		}

		/// ------------------------------------------------------------------------------------
		public virtual WaveFormat RecordingFormat
		{
			get => _recordingFormat;
			set
			{
				_recordingFormat = value;
				SampleAggregator.NotificationCount = value.SampleRate / 10;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// For the IAudioRecorder interface. <see cref="BeginMonitoring(bool)"/> </summary>
		/// ------------------------------------------------------------------------------------
		public virtual void BeginMonitoring()
		{
			BeginMonitoring(true);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Begins monitoring incoming data from the selected device but not
		/// processing it for the purpose of recording to a file.
		/// </summary>
		/// <exception cref="InvalidOperationException">Called when in an invalid state. It is
		/// only valid to call this method when the <see cref="RecordingState"/> is
		/// <see cref="Media.RecordingState.NotYetStarted"/> or
		/// <see cref="SIL.Media.RecordingState.Stopped"/>. In other words, this
		/// should be called (either directly or as a side-effect of setting the
		/// <see cref="SelectedDevice"/> or calling <see cref="BeginRecording(string)"/> only
		/// once to create and initialize a new WaveIn device. For example, if merely setting
		/// <see cref="SelectedDevice"/> for an <see cref="AudioRecorder"/> that is already
		/// monitoring, do not call this method again. Likewise, when completing a recording
		/// normally or aborting it (at the caller's request), this will automatically go back
		/// into a <see cref="SIL.Media.RecordingState.Monitoring"/> state.
		/// <see cref="BeginRecording(string)"/> also begins monitoring, so if that is called
		/// directly, this method should not be called.</exception>
		/// <param name="catchAndReportExceptions"> If true, any unhandled exception
		/// thrown as a result of this call (besides the documented exception) will be caught
		/// and reported via <see cref="ErrorReport.NotifyUserOfProblem(string,object[])"/>.
		/// </param>
		/// ------------------------------------------------------------------------------------
		public virtual void BeginMonitoring(bool catchAndReportExceptions)
		{
			bool monitoringStarted = false;
			try
			{
				monitoringStarted = BeginMonitoringIfNeeded();
			}
			catch (Exception e)
			{
				if (catchAndReportExceptions)
				{
					ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
						e, "There was a problem starting up volume monitoring.");
					return;	
				}
				else
				{
					throw;
				}
			}
			if (!monitoringStarted)
				throw new InvalidOperationException("Only call this once for a new WaveIn device" +
					" (i.e., when RecordingState is NotYetStarted or Stopped.");
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>Begins monitoring if in the correct state to do so</summary>
		/// <returns><c>true</c> if monitoring was started; <c>false</c> if not in a state where
		/// monitoring could be started (e.g., already monitoring or recording, or stopped
		/// with a waveIn that is still connected to the stopped/disconnected device).</returns>
		/// ------------------------------------------------------------------------------------
		private bool BeginMonitoringIfNeeded()
		{
			lock (this)
			{
				if (_waveIn != null || (_recordingState != RecordingState.NotYetStarted &&
					    _recordingState != RecordingState.Stopped))
				{
					return false;
				}

				try
				{
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
						// REVIEW: I get this most of the time, but I don't know how to prevent
						// it... maybe it's a hold over from previous runs? In which case, we
						// need to stop the recording
						if (error.Result != MmResult.AlreadyAllocated)
							throw;
					}

					ConnectWithVolumeControl();
					RecordingState = RecordingState.Monitoring;
					return true;
				}
				catch (Exception)
				{
					CloseWaveIn();
					throw;
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
		/// as far as NAudio is concerned, we are still "recording" (i.e., accepting waveIn
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
			try
			{
				// This is intentionally done outside of the lock obtained below. In the unusual
				// case where two different threads (one perhaps being the UI thread) are trying to
				// start recording (or one is trying to start monitoring while another tries to
				// start recording). If they are both trying to start recording, we will end up
				// throwing an InvalidOperationException on one of them, but we don't care which
				// one. If one is merely trying to start monitoring and it happens to get there
				// first, the other will find it is no longer in a valid state to begin monitoring
				// but will happily find that it is now in a valid state to begin recording. If the
				// one that is trying to begin recording gets there first, the other one will get
				// the exception. In any case, handling an InvalidOperationException is the
				// caller's responsibility.
				// The return result is ignored because it doesn't matter whether we started
				// monitoring or found we were already monitoring. If we were in some other state,
				// the test below (inside the lock) will throw an appropriate exception indicating
				// that we were not in a valid state to start recording.
				BeginMonitoringIfNeeded();

				lock (this)
				{
					if (_recordingState != RecordingState.Monitoring)
					{
						throw new InvalidOperationException(
							$"Can't begin recording while we are in this state: {_recordingState}");
					}

					// Kind of a weird race condition here, but for some reason we allow the
					// caller to change the buffers without getting a lock or checking the state.
					// (Even more oddly, if this happens while recording, it doesn't take effect
					// until the next time they start recording, but if it happens in the
					// nanoseconds between calling BeginMonitoring_Internal and here, then we
					// reset. But this was the existing behavior, and changing it now could break
					// some exiting code.)
					if (_waveInBuffersChanged)
					{
						CloseWaveIn();
						if (!BeginMonitoringIfNeeded())
						{
							// Originally I was going to make this an InvalidOperationException
							// (like the one above), but in fact, CloseWaveIn should guarantee
							// that the state is correct to begin monitoring, and we have this
							// locked, so no other thread should be able to change the state. So
							// if this fails, something changed in the program, and this is really
							// a programming error.
							throw new Exception(
								"CloseWaveIn failed to put the recorder into a valid state to call" +
								$" BeginMonitoringIfNeeded. _recordingState = {_recordingState};" +
								$" WaveInInfo = {WaveInInfo}");
						}
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
			catch (InvalidOperationException e)
			{
				Logger.WriteError(e);
				throw;
			}
			catch (Exception e)
			{
				ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
					e, "There was a problem starting recording.");
			}
		}

		private string WaveInInfo =>
			_waveIn == null ? "null" : $"_waveIn.DeviceNumber = {_waveIn.DeviceNumber}; " +
				$"_waveIn.WaveFormat = {_waveIn.WaveFormat}; " +
				$"_waveIn.NumberOfBuffers = {_waveIn.NumberOfBuffers}" +
				$"_waveIn.BufferMilliseconds = {_waveIn.BufferMilliseconds}";

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
		private IEnumerable<MixerControl> MixerLineControls
		{
			get
			{
				int attempts = 0;
				while (attempts++ < 2)
				{
					try
					{
						if (Environment.OSVersion.Version.Major >= 6) // Vista and over
						{
							var mixerLine = _waveIn.GetMixerLine();
							//new MixerLine((IntPtr)waveInDeviceNumber, 0, MixerFlags.WaveIn);
							return mixerLine.Controls;
						}

						int waveInDeviceNumber = _waveIn.DeviceNumber;
						var mixer = new Mixer(waveInDeviceNumber);
						foreach (var source in from destination in mixer.Destinations
							where destination.ComponentType == MixerLineComponentType.DestinationWaveIn
							from source in destination.Sources
							where source.ComponentType == MixerLineComponentType.SourceMicrophone
							select source)
							return source.Controls;
					}
					catch (MmException e)
					{
						Logger.WriteEvent("MmException caught: {0}", e.Message);
						if (attempts > 2)
							throw;
					}

					Thread.Sleep(50); // User might have been making changes in Control Panel that messed us up. Let's see if the problem fixes itself.
				}
				return new MixerControl[0];
			}
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void ConnectWithVolumeControl()
		{
			foreach (var control in MixerLineControls)
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

		/// <summary>
		/// Given a wav file, produce a different wav file with the given start and stop times
		/// </summary>
		/// <remarks>This is in this file because ideally it should be part of the recorder...
		/// the use case at the moment is trimming off the last 200ms or so in order to remove
		/// the sound of the click that ends the recording. That "amount to trim off the end"
		/// would be a natural parameter
		/// to add to the recording, perhaps under user-settings control. But doing that is
		/// beyond the time I have to give to this now, so I'm at least positioning this code
		/// in this library rather than in my app, as a first step. I call it when the AudioRecorder
		/// raises the Stopped event.</remarks>
		/// <param name="inPath"></param>
		/// <param name="outPath"></param>
		/// <param name="cutFromStart"></param>
		/// <param name="cutFromEnd"></param>
		/// <param name="minimumDesiredDuration">The start and end cuts will be reduced if the result would be less than this. Start has priority.</param>
		public static void TrimWavFile(string inPath, string outPath, TimeSpan cutFromStart, TimeSpan cutFromEnd, TimeSpan minimumDesiredDuration)
		{
			using (var reader = new WaveFileReader(inPath))
			{
				long totalMilliseconds = 1000*reader.Length/reader.WaveFormat.AverageBytesPerSecond;

				//we  can't trim more than we have, and more than the stated minimum size
				var cutFromStartMilliseconds = (long)Math.Min(totalMilliseconds - minimumDesiredDuration.TotalMilliseconds, cutFromStart.TotalMilliseconds);
				cutFromStartMilliseconds = (long) Math.Max(0, cutFromStartMilliseconds); // has to be 0 or positive

				totalMilliseconds -= cutFromStartMilliseconds;
				var cutFromEndMilliseconds = (long)Math.Min(totalMilliseconds - minimumDesiredDuration.TotalMilliseconds, cutFromEnd.TotalMilliseconds);
				cutFromEndMilliseconds = (long)Math.Max(0, cutFromEndMilliseconds); // has to be 0 or positive

				//from https://stackoverflow.com/a/6488629/723299
				using (var writer = new WaveFileWriter(outPath, reader.WaveFormat))
				{
					var bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;
					var startPos = cutFromStartMilliseconds * bytesPerMillisecond;
					startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

					var endBytes = cutFromEndMilliseconds * bytesPerMillisecond;
					endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
					var endPos = reader.Length - endBytes;
					TrimWavFileInternal(reader, writer, startPos, endPos);
				}
			}
		}

		private static void TrimWavFileInternal(WaveFileReader reader, WaveFileWriter writer, long startPos, long endPos)
		{
			//from https://stackoverflow.com/a/6488629/723299, added the break if we aren't getting any more data
			reader.Position = startPos;
			var buffer = new byte[1024];
			while (reader.Position < endPos)
			{
				var bytesRequired = (int)(endPos - reader.Position);
				if (bytesRequired > 0)
				{
					var bytesToRead = Math.Min(bytesRequired, buffer.Length);
					var bytesRead = reader.Read(buffer, 0, bytesToRead);
					if (bytesRead > 0)
					{
						writer.Write(buffer, 0, bytesRead);
					}
					else
					{
						//assumption here is that we tried to trim more than the whole file has
						break;
					}
				}
			}
		}
	}
}
