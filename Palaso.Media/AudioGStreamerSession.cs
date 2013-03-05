using System;
using Gst;
using GstSharp;

namespace Palaso.Media
{
	public class AudioGStreamerSession : ISimpleAudioSession
	{
		private readonly string _path;
		private Gst.BasePlugins.PlayBin2 _playBin;

		public AudioGStreamerSession(string filePath)
		{
			System.Console.WriteLine("cur dir: " + System.Environment.CurrentDirectory);
			Gst.Application.Init ();
			_path = filePath;
			_playBin = new Gst.BasePlugins.PlayBin2();
			//_playBin.PlayFlags &= ~((Gst.BasePlugins.PlayBin2.PlayFlagsType)(1 << 1));
			//_playBin.Bus.AddSignalWatch();
			//_playBin.Bus.EnableSyncMessageEmission();
			//_playBin.Bus.Message += new Gst.MessageHandler(OnPlayBinMessage);

			_playBin.SetState(Gst.State.Ready);
			_playBin.Uri = @"file:///" + filePath.Replace('\\', '/');
			System.Console.WriteLine(_playBin.Uri);
			_playBin.SetState(Gst.State.Paused);
			//_uripath = Uri(filePath);
		}


		public string FilePath
		{
			get { return _path; }
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
			//implement is currently playing
			get { return false; }
		}

		public bool CanRecord
		{
			get { return false; }
		}

		public bool CanStop
		{
			get { return IsPlaying || IsRecording; }
		}

		public bool CanPlay
		{
			get { return !IsPlaying && !IsRecording && System.IO.File.Exists(_path); }
		}

		public void Play()
		{
			if (!System.IO.File.Exists(_path))
			{
				throw new System.IO.FileNotFoundException();
			}
			_playBin.SetState(Gst.State.Playing);
		}

		public void SaveAsWav(string filePath)
		{
			throw new System.NotImplementedException();
		}

		public void StopPlaying()
		{
			// implement
			_playBin.SetState(Gst.State.Paused);
		}
	}
}
