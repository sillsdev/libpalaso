using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Palaso.Media
{
	/// <summary>
	/// Implementation of ISimpleAudioSession that uses the standard ALSA sound
	/// library on Linux.
	/// </summary>
	public class AudioAlsaSession : ISimpleAudioSession
	{
		DateTime m_startRecordingTime = DateTime.MinValue;
		DateTime m_stopRecordingTime = DateTime.MinValue;
		AlsaAudioDevice m_device;

		#region Construction and Disposal

		/// <summary>
		/// Initialize a new instance of the <see cref="Palaso.Media.AudioAlsaSession"/> class.
		/// </summary>
		public AudioAlsaSession(string filePath)
		{
			FilePath = filePath;
			m_device = new AlsaAudioDevice();
		}

		#endregion

		#region Implementation of ISimpleAudioSession

		/// <summary>
		/// Gets the path to the sound file, as established by the constructor.
		/// </summary>
		public string FilePath
		{
			get;
			private set;
		}

		/// <summary>
		/// Start recording.
		/// </summary>
		public void StartRecording()
		{
			if (!CanRecord)
				throw new ApplicationException("AlsaAudioSession: Already recording or playing on the ALSA sound device");
			bool fOk = m_device.StartRecording();
			if (!fOk)
				throw new Exception("AlsaAudioSession: Cannot open the ALSA sound device");
			m_stopRecordingTime = DateTime.MinValue;
			m_startRecordingTime = DateTime.Now;
			//Console.WriteLine("AlsaAudioSession: Recording started at {0}", m_startRecordingTime);
		}

		/// <summary>
		/// Stop the recording and save it as a WAVE file.
		/// </summary>
		public void StopRecordingAndSaveAsWav()
		{
			if (!IsRecording)
				throw new ApplicationException("AlsaAudioSession: Not recording on the ALSA sound device");
			m_device.StopRecording();
			m_stopRecordingTime = DateTime.Now;
			SaveAsWav(FilePath);
		}

		/// <summary>
		/// Get the length of the most recent recording in milliseconds.
		/// </summary>
		public double LastRecordingMilliseconds
		{
			get
			{
				if (m_startRecordingTime == DateTime.MinValue || m_stopRecordingTime == DateTime.MinValue)
					return 0.0;
				else
					return (m_stopRecordingTime - m_startRecordingTime).Milliseconds;
			}
		}

		/// <summary>
		/// true iff recording is underway.
		/// </summary>
		public bool IsRecording
		{
			get { return m_device.IsRecording; }
		}

		/// <summary>
		/// true iff playing a WAVE file
		/// </summary>
		public bool IsPlaying
		{
			get { return m_device.IsPlaying; }
		}

		/// <summary>
		/// true iff neither recording nor playing.
		/// </summary>
		public bool CanRecord
		{
			get { return !IsPlaying && !IsRecording; }
		}

		/// <summary>
		/// true iff either playing or recording.
		/// </summary>
		public bool CanStop
		{
			get { return IsPlaying || IsRecording; }
		}

		/// <summary>
		/// true iff neither playing nor recording.
		/// </summary>
		public bool CanPlay
		{
			get { return !IsPlaying && !IsRecording; }
		}

		/// <summary>
		/// Play the sound file set by the constructor.
		/// </summary>
		public void Play()
		{
			if (!CanPlay)
				throw new ApplicationException("AlsaAudioSession: Already recording or playing on the ALSA sound device");
			if (!File.Exists(FilePath))
				throw new FileNotFoundException(String.Format("AlsaAudioSession: {0} does not exist", FilePath));
			bool fOk = m_device.StartPlaying(FilePath);
			if (!fOk)
				throw new Exception("AlsaAudioSession: Cannot open the ALSA sound device");
		}

		/// <summary>
		/// Saves the sound recording as a WAVE file.  (I don't see why this is a separate interface method.)
		/// </summary>
		public void SaveAsWav(string filePath)
		{
			m_device.SaveAsWav(filePath);
		}

		/// <summary>
		/// Stop playing the sound file.
		/// </summary>
		public void StopPlaying()
		{
			m_device.StopPlaying();
		}

		#endregion
	}
}
