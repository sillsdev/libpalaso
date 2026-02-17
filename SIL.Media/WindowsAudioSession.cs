// Copyright (c) 2015-2026 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

// Not supported in .NET 8+ due to IrrKlang
#if NET462 || NET48

using System;
using System.ComponentModel;
using System.IO;
using IrrKlang;
using NAudio.Wave;
using SIL.Code;
using SIL.Reporting;

namespace SIL.Media
{
	/// <summary>
	/// A Windows implementation of an ISimpleAudioSession.
	/// Uses <see cref="IrrKlang"/> for recording and <see cref="NAudio"/> for playback.
	/// </summary>
	internal class WindowsAudioSession : ISimpleAudioWithEvents
	{
		private readonly IrrKlang.IAudioRecorder _recorder;
		private readonly ISoundEngine _engine = CreateSoundEngine();
		private bool _thinkWeAreRecording;
		private DateTime _startRecordingTime;
		private DateTime _stopRecordingTime;
		private readonly SoundFile _soundFile;
		private WaveOutEvent _outputDevice;
		private AudioFileReader _audioFile;
		private readonly object _lock = new object();
		/// <summary>
		/// Will be raised when playing is over
		/// </summary>
		public event EventHandler PlaybackStopped;

		private static ISoundEngine CreateSoundEngine()
		{
			try
			{
				// By default, try to auto-detect the sound driver. Normally on an end-user
				// computer this will succeed, but even though we're trying to use irrKlang
				// for recording only, if there's no default audio output device, it will
				// fail.
				return new ISoundEngine();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				// As a fallback, we'll try to create the engine with a null output driver.
				// This should succeed even if there's no default audio output device, and
				// it gets a bunch of tests to pass in that scenario, but I'm not 100% sure
				// it will work for actual recording in all situations as it might rely on
				// it for timing or internal buffer synchronization, even when only recording.
				return new ISoundEngine(SoundOutputDriver.NullDriver);
			}
		}

		/// <summary>
		/// Constructor for an AudioSession using the <see cref="IrrKlang"/> library
		/// </summary>
		public WindowsAudioSession(string filePath)
		{
			Guard.AgainstNull(filePath, "filePath");
			_soundFile = new SoundFile(filePath);
			_engine.AddFileFactory(_soundFile);
			_recorder = new IrrKlang.IAudioRecorder(_engine);
			FilePath = filePath;
		}
		public string FilePath { get; }

		public void StartRecording()
		{
			if (_thinkWeAreRecording)
				throw new ApplicationException("Can't begin recording when we're already recording.");

			_thinkWeAreRecording = true;
			_recorder.ClearRecordedAudioDataBuffer();

			_recorder.StartRecordingBufferedAudio(22000, SampleFormat.Signed16Bit, 1);
			_startRecordingTime = DateTime.Now;
		}

		public void StopRecordingAndSaveAsWav()
		{
			if (!_thinkWeAreRecording)
				throw new ApplicationException("Stop Recording called when we weren't recording. Use IsRecording to check first.");

			_thinkWeAreRecording = false;
			_recorder.StopRecordingAudio();
			if (_recorder.RecordedAudioData != null)
			{
				SaveAsWav(FilePath);
			}
			_recorder.ClearRecordedAudioDataBuffer();
			_stopRecordingTime = DateTime.Now;
		}

		public double LastRecordingMilliseconds
		{
			get
			{
				if (_startRecordingTime == default || _stopRecordingTime == default)
					return 0;
				return _stopRecordingTime.Subtract(_startRecordingTime).TotalMilliseconds;
			}
		}

		public bool IsRecording => _recorder != null && _recorder.IsRecording;

		public bool IsPlaying { get; private set; }

		public bool CanRecord => !IsPlaying && !IsRecording;

		public bool CanStop => IsPlaying || IsRecording;

		public bool CanPlay => !IsPlaying && !IsRecording && File.Exists(FilePath);

		private void OnPlaybackStopped(object sender, StoppedEventArgs args)
		{
			lock (_lock)
				CleanupPlaybackResources();

			PlaybackStopped?.Invoke(this, args);
		}

		private void CleanupPlaybackResources()
		{
			if (_outputDevice != null)
			{
				_outputDevice.PlaybackStopped -= OnPlaybackStopped;
				try
				{
					// Dispose calls Stop, which can thow.
					_outputDevice.Dispose();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					// We're disposing. We probably don't care what went wrong.
				}
				_outputDevice = null;
			}
			
			if (_audioFile != null)
			{
				_audioFile.Dispose();
				_audioFile = null;
			}

			IsPlaying = false;
		}

		/// <summary>
		/// The current version of Play uses NAudio for playback. <see cref="IrrKlang"/> had issues
		/// with playback. In the future it may be best to try the latest version of
		/// <see cref="IrrKlang"/> and see if true safe cross-platform recording and playback can
		/// be accomplished now. This would eliminate the need for the <see cref="AlsaAudio"/>
		/// classes on linux. Note: <see cref="IrrKlang"/> was upgraded to v. 1.6 in Nov 2024, but
		/// I did not re-check to see if it works for playback on all platforms.
		/// </summary>
		public void Play()
		{
			if (IsRecording)
				throw new ApplicationException("Can't play while recording.");
			if (!File.Exists(FilePath))
				throw new FileNotFoundException("Could not find sound file");
			if (new FileInfo(FilePath).Length == 0)
				throw new FileLoadException("Trying to play empty file");

			var worker = new BackgroundWorker();
			IsPlaying = true;
			worker.DoWork += (sender, args) =>
			{
				try
				{
					lock (_lock)
					{
						if (_outputDevice == null)
						{
							_outputDevice = new WaveOutEvent();
							_outputDevice.PlaybackStopped += OnPlaybackStopped;
						}
						if (_audioFile == null)
						{
							_audioFile = new AudioFileReader(FilePath);
							_outputDevice.Init(_audioFile);
						}
						_outputDevice.Play();
					}
				}
				catch (Exception e)
				{
					// Try to clean things up as best we can...no easy way to test this, though.
					// We don't want to be permanently in the playing state.
					IsPlaying = false;
					// And, in case something critical is waiting for this...
					OnPlaybackStopped(this, new StoppedEventArgs(e));
					// Maybe the system has another way of playing it that works? e.g., most default players will handle mp3.
					// But it seems risky...maybe we will be trying to play another sound or do some recording?
					// Decided not to do this for now.
					ErrorReport.ReportNonFatalException(e);
				}
			};
			worker.RunWorkerAsync();
		}

		private class SoundFile : IFileFactory
		{
			private readonly string _soundFiledPath;
			private FileStream _soundFileStream;

			public SoundFile(string filePath)
			{
				_soundFiledPath = filePath;
			}

			public Stream openFile(string dummy)
			{
				CloseFile();
				// Ensure that the file is not locked from other users
				_soundFileStream = File.Open(_soundFiledPath, FileMode.Open, FileAccess.Read);
				return _soundFileStream;
			}

			public void CloseFile()
			{
				_soundFileStream?.Close();
				_soundFileStream = null;
			}
		}

		public void SaveAsWav(string path)
		{

			if (File.Exists(path))
				File.Delete(path);

			short formatType = 1;
			var numChannels = _recorder.AudioFormat.ChannelCount;
			var sampleRate = _recorder.AudioFormat.SampleRate;
			var bitsPerChannel = _recorder.AudioFormat.SampleSize * 8;
			var bytesPerSample = _recorder.AudioFormat.FrameSize;
			var bytesPerSecond = _recorder.AudioFormat.BytesPerSecond;
			var dataLen = _recorder.AudioFormat.SampleDataSize;

			const int fmtChunkLen = 16;
			const int waveHeaderLen = 4 + 8 + fmtChunkLen + 8;

			var totalLen = waveHeaderLen + dataLen;

			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
			{

				using (BinaryWriter bw = new BinaryWriter(fs))
				{
					bw.Write(new char[4] {'R', 'I', 'F', 'F'});

					bw.Write(totalLen);

					bw.Write(new [] {'W', 'A', 'V', 'E', 'f', 'm', 't', ' '});

					bw.Write(fmtChunkLen);

					bw.Write(formatType);
					bw.Write((short)numChannels);

					bw.Write(sampleRate);

					bw.Write(bytesPerSecond);

					bw.Write((short)bytesPerSample);

					bw.Write((short)bitsPerChannel);

					bw.Write(new [] {'d', 'a', 't', 'a'});
					bw.Write(_recorder.RecordedAudioData.Length);

					bw.Write(_recorder.RecordedAudioData);
					bw.Close();
				}
				fs.Close();
			}

			_recorder.ClearRecordedAudioDataBuffer();
		}

		public void StopPlaying()
		{
			lock (_lock)
			{
				if (!IsPlaying || _outputDevice == null)
					return;

				_outputDevice.Stop();
			}
		}

		public void Dispose()
		{
			lock (_lock)
				CleanupPlaybackResources();

			try
			{
				_engine.RemoveAllSoundSources();
			}
			catch (Exception)
			{
				// We'll just ignore any errors on stopping the sounds (We don't use irrKlang for playback anyway).
			}
			
			_engine?.Dispose();
			_recorder.Dispose();
			_soundFile.CloseFile();
		}
	}
}
#endif
