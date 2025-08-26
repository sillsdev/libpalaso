// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.IO;

namespace SIL.Media.AlsaAudio
{
	/// <summary>
	/// Implementation of ISimpleAudioSession that uses the standard ALSA sound
	/// library on Linux.
	/// </summary>
	internal class AudioAlsaSession : ISimpleAudioSession
	{
		DateTime _startRecordingTime = DateTime.MinValue;
		DateTime _stopRecordingTime = DateTime.MinValue;
		AlsaAudioDevice _device;

#region Construction and Disposal

		/// <summary>
		/// Initialize a new instance of the <see cref="AudioAlsaSession"/> class.
		/// </summary>
		public AudioAlsaSession(string filePath)
		{
			FilePath = filePath;
			_device = new AlsaAudioDevice();
		}

#endregion

#region Implementation of ISimpleAudioSession

		/// <summary>
		/// Gets the path to the sound file, as established by the constructor.
		/// </summary>
		public string FilePath { get; }

		/// <summary>
		/// Start recording.
		/// </summary>
		public void StartRecording()
		{
			if (!CanRecord)
				throw new ApplicationException("AlsaAudioSession: Already recording or playing on the ALSA sound device");
			_stopRecordingTime = DateTime.MinValue;
			_startRecordingTime = DateTime.Now;
			bool fOk = _device.StartRecording();
			if (!fOk)
				throw new Exception("AlsaAudioSession: Cannot open the ALSA sound device");
		}

		/// <summary>
		/// Stop the recording and save it as a WAVE file.
		/// </summary>
		public void StopRecordingAndSaveAsWav()
		{
			if (!IsRecording)
				throw new ApplicationException("AlsaAudioSession: Not recording on the ALSA sound device");
			_device.StopRecording();
			_stopRecordingTime = DateTime.Now;
			SaveAsWav(FilePath);
		}

		/// <summary>
		/// Get the length of the most recent recording in milliseconds.
		/// </summary>
		public double LastRecordingMilliseconds
		{
			get
			{
				if (_startRecordingTime == DateTime.MinValue || _stopRecordingTime == DateTime.MinValue)
					return 0.0;
				else
					return (_stopRecordingTime - _startRecordingTime).TotalMilliseconds;
			}
		}

		/// <summary>
		/// true iff recording is underway.
		/// </summary>
		public bool IsRecording => _device.IsRecording;

		/// <summary>
		/// true iff playing a WAVE file
		/// </summary>
		public bool IsPlaying => _device.IsPlaying;

		/// <summary>
		/// true iff neither recording nor playing.
		/// </summary>
		public bool CanRecord => !IsPlaying && !IsRecording;

		/// <summary>
		/// true iff either playing or recording.
		/// </summary>
		public bool CanStop => IsPlaying || IsRecording;

		/// <summary>
		/// true iff neither playing nor recording.
		/// </summary>
		public bool CanPlay => !IsPlaying && !IsRecording;

		/// <summary>
		/// Play the sound file set by the constructor.
		/// </summary>
		public void Play()
		{
			if (!CanPlay)
				throw new ApplicationException("AlsaAudioSession: Already recording or playing on the ALSA sound device");
			if (!File.Exists(FilePath))
				throw new FileNotFoundException($"AlsaAudioSession: {FilePath} does not exist");
			if (new FileInfo(FilePath).Length == 0)
				throw new FileLoadException("Trying to play empty file");
			if(!_device.StartPlaying(FilePath))
			{
				// If the Alsa device can't play the file, it's probably a format we don't recognize. See if the OS knows how to play it.
				// UseShellExecute defaults to true in .net framework (including .net 4) and to false in .net core (including .net 8)
				// We are explicitly setting it to true for consistency with the old behavior
				// but have not checked if it is necessary here.
				Process.Start(new ProcessStartInfo(FilePath) { UseShellExecute = true });
			}
		}

		/// <summary>
		/// Saves the sound recording as a WAVE file.  (I don't see why this is a separate interface method.)
		/// </summary>
		public void SaveAsWav(string filePath)
		{
			_device.SaveAsWav(filePath);
		}

		/// <summary>
		/// Stop playing the sound file.
		/// </summary>
		public void StopPlaying()
		{
			_device.StopPlaying();
		}

#endregion

		/// <summary>
		/// Start recording at the desired sample rate and number of channels.  The device remembers
		/// these settings.
		/// </summary>
		/// <remarks>
		/// This method is not in the interface, but may be used by the calling program to record better audio files
		/// (e.g., 44K instead of 22K sample rate).
		/// </remarks>
		public void StartRecording(uint sampleRate, ushort channelCount)
		{
			_device.DesiredSampleRate = sampleRate;
			_device.DesiredChannelCount = channelCount;
			StartRecording();
		}

		/// <summary>
		/// Set the name of the input device to open for recording.  The default value is "default".  I have no idea
		/// what other valid values would be, and those would be system dependent anyway.  Use this at your own risk!
		/// </summary>
		/// <remarks>
		/// This method is not in the interface, but may be used to allow the user to choose the input device
		/// from inside the calling program.
		/// </remarks>
		public void SetInputDevice(string device)
		{
			_device.DesiredInputDevice = device;
		}

		public void Dispose()
		{
			// There is nothing to dispose
		}
	}
}
