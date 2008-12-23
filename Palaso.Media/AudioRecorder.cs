using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using IrrKlang;

namespace Palaso.Media
{
	public class AudioRecorder
	{
		private IAudioRecorder _recorder;
		private ISoundEngine _engine;
		private ISound _sound;
		private string _path;
		private bool _thinkWeAreRecording;


		public AudioRecorder(string path)
		{
			_path = path;
			_engine = new ISoundEngine();
			_recorder = new IAudioRecorder(_engine);
		}

		public void Test()
		{
			var engine = new ISoundEngine();
			var recorder = new IAudioRecorder(engine);
			var data = recorder.RecordedAudioData; //throws exception.  Should set data to null.
		}

		public void StartRecording()
		{
			_thinkWeAreRecording = true;
			_recorder.ClearRecordedAudioDataBuffer();
			_recorder.StartRecordingBufferedAudio();

		}
		public void StopRecording()
		{
			_thinkWeAreRecording = false;
			_recorder.StopRecordingAudio();
			SaveAsWav(_path);
			_recorder.ClearRecordedAudioDataBuffer();
		}
		public bool IsRecording
		{
			get
			{
				//doesn't work: return _recorder.IsRecording; (bug has been reported)

				return _thinkWeAreRecording;
			}
		}

		private byte[] GetRecordedAudioDataSafely()
		{
			//there is a bug (which I've reported) that we need to step carefully around
			try
			{
				return _recorder.RecordedAudioData;// will through if there's no data
			}
			catch (Exception)
			{
				return null;
			}
		}

		public bool IsPlaying
		{
			get { return (_sound !=null && !_sound.Finished); }
		}

		public bool CanRecord
		{
			get { return !IsPlaying && !IsRecording; }
		}

		public bool CanStop
		{
			get { return IsPlaying || IsRecording; }
		}

		public bool CanPlay
		{
			get { return !IsPlaying && !IsRecording; }
		}

		public void Play()
		{
			if (_sound != null)
			{
				_engine.StopAllSounds();
			}


			//turns out, the silly engine will keep playing the same recording, even
			//after we've chaned the contents of the file or even delete it.
			//so, we need to make a new engine.
			//   NO   _sound = _engine.Play2D(path, false);

			_sound = new IrrKlang.ISoundEngine().Play2D(_path);
		}

		public void SaveAsWav(string path)
		{

			if(File.Exists(path))
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
					bw.Write(new char[4] { 'R', 'I', 'F', 'F' });

					bw.Write(totalLen);

					bw.Write(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });

					bw.Write((int)fmtChunkLen);

					bw.Write((short)formatType);
					bw.Write((short)numChannels);

					bw.Write((int)sampleRate);

					bw.Write((int)bytesPerSecond);

					bw.Write((short)bytesPerSample);

					bw.Write((short)bitsPerChannel);

					bw.Write(new char[4] { 'd', 'a', 't', 'a' });
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
			_engine.StopAllSounds();
		}
	}
}

/*
 * from forum:
 *
 * I'm currently making a childs game where I need to play pianosounds(multi voice). Thought I'd use IrrKlang. Worked just fine for a start. Then I discovered that the audio stopped working after running the app for about 40-50 seconds in idle mode.
 *
 * Solved it!

I use an option on the constructor of the soundEngine(SoundOutputDriver.WinMM);

And I set "nostreaming" and "preload" true - now it works like a charm!
 */
