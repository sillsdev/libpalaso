using System;
using Gst;
using GstSharp;

namespace Palaso.Media
{
	public class AudioGStreamerSession : ISimpleAudioSession
	{
		public static bool gst_is_init;
		private readonly string _path;
		private DateTime _startRecordingTime;
		private DateTime _stopRecordingTime;
		private bool _playInit, _recordInit, _recordOgg;
		private bool _isPlaying, _isRecording;
		private Gst.BasePlugins.PlayBin2 _playBin;
		private Gst.Pipeline _pipeline;
		private Gst.CorePlugins.FileSink _eleFileSink;
		private Gst.Element _eleAudioSource, _eleAudioConvert, _eleWavPack, _eleVorbisEnc, _eleOggMux;


		public AudioGStreamerSession(string filePath)
		{
			// don't forget problem with gstreamerglue lib that needs to be in lib path
			// not just current dir
			// modified package?

			System.Console.WriteLine("cur dir: " + System.Environment.CurrentDirectory);
			if (!gst_is_init)
			{
				Gst.Application.Init();
				gst_is_init = true;
			}
			_path = filePath;
			System.Console.WriteLine("path: " + filePath);
			_recordOgg = true;
		}


		public string FilePath
		{
			get { return _path; }
		}

		public void StartRecording ()
		{
			if (_isRecording) {
				StopRecordingAndSaveAsWav();
				throw new ApplicationException("recording while already recording");
			}
			if (!_recordInit) {
				SetupRecordingPipeline();
			}
			_pipeline.SetState(Gst.State.Playing);
			_startRecordingTime = DateTime.Now;
			_isRecording = true;
			_pipeline.Bus.AddWatch (new BusFunc (BusCb));
		}

// may try to use to find EOS on pipeline
 private static bool BusCb (Bus bus, Message message)
		{
			switch (message.Type) {
			case MessageType.Error:
				Enum err;
				string msg;
				message.ParseError (out err, out msg);
				Console.WriteLine ("Gstreamer error: {0}", msg);
				//loop.Quit ();
				break;
			case MessageType.Eos:
				//_isRecording = false;
				//SaveAsWav (_path);
				//loop.Quit();
				Console.WriteLine ("End of stream");
				break;
			}
			return true;
		}

		public void StopRecordingAndSaveAsWav()
		{
			if (!_recordInit)
			{
				throw new ApplicationException("trying to stop when recording not initialised");
			}
			if (!_isRecording)
			{
				throw new ApplicationException("trying to stop when recording not started");
			}

			_pipeline.SetState(Gst.State.Null);
			_stopRecordingTime = DateTime.Now;
			_isRecording = false;
			SaveAsWav (_path);

		}




		public double LastRecordingMilliseconds
		{
			get
			{
				if(_startRecordingTime == default(DateTime) || _stopRecordingTime == default(DateTime))
					return 0.0;
				return _stopRecordingTime.Subtract(_startRecordingTime).TotalMilliseconds;
			}
		}

		public bool IsRecording
		{
			get { return _isRecording; }
		}

		public bool IsPlaying
		{
			get { return _isPlaying; }
		}

		public bool CanRecord
		{
			get { return !IsPlaying && !IsRecording;  }
		}

		public bool CanStop
		{
			get { return IsPlaying || IsRecording; }
		}

		public bool CanPlay
		{
			get { return !IsPlaying && !IsRecording && System.IO.File.Exists(_path); }
		}

		private void SetupPlaybin ()
		{
			if (!System.IO.File.Exists(_path))
			{
				throw new System.IO.FileNotFoundException();
			}
			_playBin = new Gst.BasePlugins.PlayBin2();
			//_playBin.PlayFlags &= ~((Gst.BasePlugins.PlayBin2.PlayFlagsType)(1 << 1));
			//_playBin.Bus.AddSignalWatch();
			//_playBin.Bus.EnableSyncMessageEmission();
			//_playBin.Bus.Message += new Gst.MessageHandler(OnPlayBinMessage);

			_playBin.SetState(Gst.State.Ready);
			_playBin.Uri = @"file:///" + _path.Replace('\\', '/');
			System.Console.WriteLine(_playBin.Uri);
			_playBin.SetState(Gst.State.Paused);
			_playInit = true;
		}

		private void SetupRecordingPipeline ()
		{
			// set up pipeline and elements
			// gst-launch-0.10 autoaudiosrc ! audioconvert ! vorbisenc ! oggmux ! filesink location=dump.ogg
			// Create new pipeline.

			_pipeline = new Gst.Pipeline ();

			//Pipeline pipeline = new Pipeline("pipeline");

			// Construct pipeline filesrc -> avidemux -> mpeg4 -> directdrawsink

			//_eleAudioSource = ElementFactory.Make("autoaudiosrc", "autoaudiosrc");
			_eleAudioSource = ElementFactory.Make ("pulsesrc");
			_eleAudioConvert = ElementFactory.Make ("audioconvert");
			if (_recordOgg) {
				_eleVorbisEnc = ElementFactory.Make ("vorbisenc");
				_eleOggMux = ElementFactory.Make ("oggmux");
			} else {
				_eleWavPack = ElementFactory.Make ("wavpackenc");
			}
			_eleFileSink = new Gst.CorePlugins.FileSink ();
			//_eleFileSink = ElementFactory.Make("filesink", "filesink");
			_eleFileSink ["location"] = _path;

			// Add and link pipeline.
			if (_recordOgg) {
				_pipeline.Add (_eleAudioSource, _eleAudioConvert, _eleVorbisEnc, _eleOggMux, _eleFileSink);
			} else {
				_pipeline.Add (_eleAudioSource, _eleAudioConvert, _eleWavPack, _eleFileSink);
			}

			// Play video.
			_pipeline.SetState (Gst.State.Ready);
			_pipeline.SetState (Gst.State.Paused);

			if (!_eleAudioSource.Link (_eleAudioConvert)) {
				Console.WriteLine ("link failed between source and converter");
			}
			if (_recordOgg) {
				if (!_eleAudioConvert.Link (_eleVorbisEnc)) {
					Console.WriteLine ("link failed between converter and encoder");
				}

				if (!_eleVorbisEnc.Link (_eleOggMux)) {
					Console.WriteLine ("link failed between e and parser");
				}

				if (!_eleOggMux.Link (_eleFileSink)) {
					Console.Error.WriteLine ("link failed between parser and sink");
				}
			} else {

				if (!_eleAudioConvert.Link (_eleWavPack)) {
					Console.WriteLine ("link failed between converter and encoder");
				}

				if (!_eleWavPack.Link (_eleFileSink)) {
					Console.Error.WriteLine ("link failed between encoder and sink");
				}
			}

			_recordInit = true;
		}

		public void Play ()
		{
			if (_isRecording) {
				throw new ApplicationException ("trying to play while recording");
			}
			// move playbin creation to here if it isn't already created?
			if (!_playInit) {
				SetupPlaybin ();
			}
			_playBin.SetState(Gst.State.Playing);
			_isPlaying = true;
		}

		public void SaveAsWav (string filePath)
		{
			// null op as currently the recording pipeline saves to wav as it goes
			// just check that it did record
			if (!System.IO.File.Exists (filePath)) {
				throw new ApplicationException ("recorded file not saved");
			}
			System.IO.FileInfo f = new System.IO.FileInfo (filePath);
			System.Console.WriteLine ("file exists after record");
			if (f.Length <= 4) {
				throw new ApplicationException ("recorded file zero length");
			}
		}

		public void StopPlaying()
		{
			// is this to be stop or pause?
			_playBin.SetState(Gst.State.Null);
			_isPlaying = false;
		}
	}
}
