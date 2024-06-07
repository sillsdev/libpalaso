using System;

namespace SIL.Windows.Forms.Media
{
	public class AudioNullSession : ISimpleAudioSession
	{
		public string FilePath
		{
			get { return String.Empty; }
		}

		public void StartRecording()
		{
			ReportNotImplemented();
		}

		public void StopRecordingAndSaveAsWav()
		{
			ReportNotImplemented();
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
			ReportNotImplemented();
		}

		public void SaveAsWav(string filePath)
		{
			ReportNotImplemented();
		}

		public void StopPlaying()
		{
			ReportNotImplemented();
		}

		public void Dispose()
		{
			// There is nothing to dispose
		}

		/// <Summary>
		/// MessageBoxes are much less destructive than exceptions, but almost as intrusive.
		/// </Summary>
		private void ReportNotImplemented()
		{
			System.Windows.Forms.MessageBox.Show(
				"This feature has not yet been implemented. Please check back in a later version.",
				"Not Implemented");
		}
	}
}
