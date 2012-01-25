using System;
using System.Diagnostics;
using NAudio;
using NAudio.Wave;
using NAudio.Mixer;
using Palaso.Reporting;

namespace Palaso.Media.Naudio
{
	public class PeakLevelEvent : EventArgs
	{
		public float Level;
	}

	public interface IAudioRecorder
	{
		event EventHandler<PeakLevelEvent> PeakLevelChanged;
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

	public class AudioRecorder : IAudioRecorder
	{
		private readonly int _maxMinutes;

		/// <summary>
		/// This guy is always working, whether we're playing, recording, or just idle (monitoring)
		/// </summary>
		WaveIn _waveIn;

		/// <summary>
		/// This guy is recreated for each recording, and disposed of when recording stops.
		/// </summary>
		WaveFileWriter _writer;


		SampleAggregator _sampleAggregator;
		UnsignedMixerControl _volumeControl;
		double _microphoneLevel = 100;
		RecordingState _recordingState;

		WaveFormat _recordingFormat;

		public event EventHandler Stopped = delegate { };

		/// <summary>
		///
		/// </summary>
		/// <param name="maxMinutes">REVIW: why does this max time even exist?  I don't see that it affects buffer size</param>
		public AudioRecorder(int maxMinutes)
		{
			_maxMinutes = maxMinutes;
			_sampleAggregator = new SampleAggregator();
			_sampleAggregator.MaximumCalculated += new EventHandler<MaxSampleEventArgs>(
				delegate
				{
					if (null != PeakLevelChanged)
					{
						//var peakLevel = Math.Max(e.MaxSample, Math.Abs(e.MinSample));

						PeakLevelChanged(this,
										 new PeakLevelEvent() { Level = _sampleAggregator.maxValue });
					}
				});

			RecordingFormat = new WaveFormat(44100, 1);
		}


		public WaveFormat RecordingFormat
		{
			get
			{
				return _recordingFormat;
			}
			set
			{
				_recordingFormat = value;
				_sampleAggregator.NotificationCount = value.SampleRate / 10;
			}
		}


		public RecordingDevice SelectedDevice { get; set; }

		public event EventHandler<PeakLevelEvent> PeakLevelChanged;


		public void BeginMonitoring()
		{
			Debug.Assert(_waveIn==null, "only call this once");
			try
			{
				if (_recordingState != RecordingState.Stopped)
				{
					throw new InvalidOperationException("Can't begin monitoring while we are in this state: " +
														_recordingState.ToString());
				}
				Debug.Assert(_waveIn==null);
				_waveIn = new WaveIn();
				_waveIn.DeviceNumber = SelectedDevice.DeviceNumber;

				_waveIn.DataAvailable += waveIn_DataAvailable;
				//_waveIn.RecordingStopped += new EventHandler(waveIn_RecordingStopped);
				_waveIn.WaveFormat = _recordingFormat;
				try
				{
					_waveIn.StartRecording();
				}
				catch(MmException error)
				{
					if(error.Result  != NAudio.MmResult.AlreadyAllocated)  //TODO: I get this most of the time, but I don't know how to prevent it... maybe it's a hold over from previous runs? In which case, we need to make this disposable and stop the recording
						throw error;
				}

				TryGetVolumeControl();
				RecordingState =RecordingState.Monitoring;
			}
			catch(Exception e)
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
			if (_writer != null)
			{
				_writer.Dispose();
				_writer = null;
			}
			Stopped(this, EventArgs.Empty);
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
			if (_recordingState != RecordingState.Monitoring)
			{
				throw new InvalidOperationException("Can't begin recording while we are in this state: " + _recordingState.ToString());
			}
			_writer = new WaveFileWriter(waveFileName, _recordingFormat);
			RecordingState =RecordingState.Recording;
		}

		public void Stop()
		{
			if (_recordingState == RecordingState.Recording)
			{
				RecordingState =RecordingState.RequestedStop;
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
						this._volumeControl = control as UnsignedMixerControl;
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

		public TimeSpan RecordedTime { get; set; }




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
		}

		private void WriteToFile(byte[] buffer, int bytesRecorded)
		{
			//REVIEW: why does this max time even exist?  I don't see that it affects buffer size
			long maxFileLength = this._recordingFormat.AverageBytesPerSecond * 60*_maxMinutes;

			if (_recordingState == RecordingState.Recording
				|| _recordingState == RecordingState.RequestedStop)
			{
				int toWrite = (int)Math.Min(maxFileLength - _writer.Length, bytesRecorded);
				if (toWrite > 0)
				{
					_writer.WriteData(buffer, 0, bytesRecorded);
				}
				else
				{
					Stop();
				}
			}
		}
	}
}
