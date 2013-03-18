using System;

namespace Palaso.Media
{
	public class AudioNullSession : ISimpleAudioSession
	{
		public string FilePath
		{
			get { return String.Empty; }
		}

		public void StartRecording()
		{
			throw new System.NotImplementedException();
		}

		public void StopRecordingAndSaveAsWav()
		{
			throw new System.NotImplementedException();
		}

		public double LastRecordingMilliseconds
		{
			get { return 0.0; }
		}

		public bool IsRecording
		{
			get { return false; }
		}

		public bool IsPlaying
		{
			get { return false; }
		}

		public bool CanRecord
		{
			get { return false; }
		}

		public bool CanStop
		{
			get { return false; }
		}

		public bool CanPlay
		{
			get { return false; }
		}

		public void Play()
		{
			throw new System.NotImplementedException();
		}

		public void SaveAsWav(string filePath)
		{
			throw new System.NotImplementedException();
		}

		public void StopPlaying()
		{
			throw new System.NotImplementedException();
		}
	}
}