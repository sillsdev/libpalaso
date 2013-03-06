using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Palaso.Media
{
	/// <summary>
	/// Simplified wrapper around the standard Linux Alsa audio library.  This wrapper
	/// records only mono WAVE files at 22KHz, and plays back only simple PCM encoded
	/// WAVE files.
	/// </summary>
	public class AlsaAudioDevice
	{
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_open(ref IntPtr pcm, string pc_name, int stream, int mode);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_close(IntPtr pcm);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_drain(IntPtr pcm);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_writei(IntPtr pcm, byte[] buf, int size);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_set_params(IntPtr pcm, int format, int access, int channels, int rate, int soft_resample, int latency);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_malloc(ref IntPtr hwparams);
		[DllImport ("libasound.so.2")]
		static extern void snd_pcm_hw_params_free(IntPtr hwparams);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_sw_params_malloc(ref IntPtr swparams);
		[DllImport ("libasound.so.2")]
		static extern void snd_pcm_sw_params_free(IntPtr swparams);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_any(IntPtr pcm, IntPtr hwparams);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_set_access(IntPtr pcm, IntPtr hwparams, int access);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_set_format(IntPtr pcm, IntPtr hwparams, int format);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_set_channels(IntPtr pcm, IntPtr hwparams, uint val);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_set_rate_near(IntPtr pcm, IntPtr hwparams, ref uint val, ref int dir);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params(IntPtr pcm, IntPtr hwparams);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_get_period_size(IntPtr hwparams, ref int val, ref int dir);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_get_buffer_size(IntPtr hwparams, ref int val);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_sw_params_current(IntPtr pcm, IntPtr swparams);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_sw_params_set_avail_min(IntPtr pcm, IntPtr swparams, ulong val);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_sw_params_set_start_threshold(IntPtr pcm,	IntPtr swparams, ulong val);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_sw_params_set_stop_threshold(IntPtr pcm, IntPtr swparams, ulong val);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_sw_params(IntPtr pcm, IntPtr swparams);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_format_physical_width(int format);
		[DllImport ("libasound.so.2")]
		static extern long snd_pcm_readi(IntPtr pcm, byte[] buffer, long size);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_nonblock(IntPtr pcm, int nonblock);

		// These constants are cribbed from alsa/pcm.h.
		const int SND_pcm_STREA_PLAYBACK = 0;
		const int SND_pcm_STREA_CAPTURE = 1;
		const int SND_pcm_FORMAT_U8 = 1;
		const int SND_pcm_FORMAT_S16_LE = 2;
		const int SND_pcm_FORMAT_S24_LE = 6;
		const int SND_pcm_FORMAT_S32_LE = 10;
		const int SND_pcm_FORMAT_FLOAT_LE = 14;
		const int SND_pcm_FORMAT_S24_3LE = 32;
		const int SND_pcm_ACCESS_RW_INTERLEAVED = 3;

		// These constants are cribbed from Microsoft.
		const ushort WAV_FMT_PCM = 1;

		IntPtr _hpcm;
		IntPtr _hwparams;
		IntPtr _swparams;
		Thread _recordingThread;
		Thread _playbackThread;
		/// <summary>flag to stop recording or playing</summary>
		bool _fAsyncQuit;
		int _pcmFormat;
		ushort _channelCount;
		uint _sampleRate;
		int _startDelay;
		int _chunkSize = 1024;
		int _bufferSize;
		byte[] _audiobuf;
		ushort _bitsPerFrame;
		string _tempfile;
		int _chunkBytes;
		int _cbWritten;
		string _filename;

		#region Construction and Destruction

		/// <summary>
		/// Initialize a new instance of the <see cref="Palaso.Media.AlsaAudioDevice"/> class.
		/// </summary>
		public AlsaAudioDevice()
		{
		}

		/// <summary>
		/// Delete any temporary file created during recording that somehow still exists.
		/// </summary>
		~AlsaAudioDevice()
		{
			if (!String.IsNullOrEmpty(_tempfile) && File.Exists(_tempfile))
				File.Delete(_tempfile);
		}
		#endregion

		#region Public methods and properties

		/// <summary>
		/// Play the specified WAVE file.
		/// </summary>
		/// <param name='fileName'>
		/// true if successful, false if an error occurs
		/// </param>
		public bool StartPlaying(string fileName)
		{
			if (!File.Exists(fileName))
				return false;
			if (IsPlaying || IsRecording)
				return false;
			string errorMsg = null;
			if (!ValidateFile(fileName, out errorMsg))
			{
				ShowError(errorMsg);
				return false;
			}
			_filename = fileName;
			_fAsyncQuit = false;

			// Create the thread object, passing in the Record method
			// via a ThreadStart delegate. This does not start the thread.
			_playbackThread = new Thread(new ThreadStart(Play));

			// Start the thread
			_playbackThread.Start();

			// Wait for the started thread to become alive:
			while (!_playbackThread.IsAlive)
				;
			return true;
		}

		/// <summary>
		/// Start recording into a temporary file.  The actual recording is done on its own
		/// thread, so that it can be stopped asynchronously by user action.
		/// </summary>
		/// <returns>
		/// true if successful, false if an error occurs
		/// </returns>
		public bool StartRecording()
		{
			if (IsRecording || IsPlaying)
				return false;

			_fAsyncQuit = false;

			// Create the thread object, passing in the Record method
			// via a ThreadStart delegate. This does not start the thread.
			_recordingThread = new Thread(new ThreadStart(Record));

			// Start the thread
			_recordingThread.Start();

			// Wait for the started thread to become alive:
			while (!_recordingThread.IsAlive)
				;
			return true;
		}

		/// <summary>
		/// true iff recording is underway.
		/// </summary>
		public bool IsRecording
		{
			get { return _recordingThread != null && _recordingThread.IsAlive; }
		}

		/// <summary>
		/// Stop the recording, waiting for the thread to die.
		/// </summary>
		public void StopRecording()
		{
			if (!IsRecording)
				return;
			_fAsyncQuit = true;
			_recordingThread.Join(1000);
			if (_recordingThread.IsAlive)
				_recordingThread.Abort();
			_recordingThread = null;
		}

		/// <summary>
		/// Write the stored audio data as a WAVE file.
		/// </summary>
		public void SaveAsWav(string filePath)
		{
			// Make sure we have data.
			if (String.IsNullOrEmpty(_tempfile))
			{
				_cbWritten = 0;
				return;
			}
			if (!File.Exists(_tempfile))
			{
				_tempfile = null;
				_cbWritten = 0;
				return;
			}
			if (_cbWritten == 0)
			{
				File.Delete(_tempfile);
				_tempfile = null;
				return;
			}
			FileInfo fi = new FileInfo(_tempfile);
			Debug.Assert(fi.Length == _cbWritten);
			WaveFileWriter writer = new WaveFileWriter(filePath);
			writer.WriteFileHeader((int)fi.Length);
			WaveFormatChunk format = new WaveFormatChunk();
			format.chunkId = "fmt ";
			format.chunkSize = 16;				// size of the struct in bytes - 8
			format.audioFormat = WAV_FMT_PCM;
			format.channelCount = _channelCount;
			format.sampleRate = _sampleRate;
			format.byteRate = (uint)(_sampleRate * _channelCount * _bitsPerFrame / 8);
			format.blockAlign = (ushort)(_channelCount * _bitsPerFrame / 8);
			format.bitsPerSample = _bitsPerFrame;
			writer.WriteFormatChunk(format);
			writer.WriteDataHeader((int)fi.Length);
			byte[] data = File.ReadAllBytes(_tempfile);
			Debug.Assert(data.Length == _cbWritten);
			writer.WriteData(data);
			writer.Close();
			// Clean up the temporary data from the recording process.
			File.Delete(_tempfile);
			_tempfile = null;
			_cbWritten = 0;
		}

		/// <summary>
		/// true iff a WAVE file is playing.
		/// </summary>
		public bool IsPlaying
		{
			get { return _playbackThread != null && _playbackThread.IsAlive; }
		}

		/// <summary>
		/// Stop the playing, waiting for the thread to die.
		/// </summary>
		public void StopPlaying()
		{
			if (!IsPlaying)
				return;
			_fAsyncQuit = true;
			_playbackThread.Join(1000);
			if (_playbackThread.IsAlive)
				_playbackThread.Abort();
			_playbackThread = null;
		}

		#endregion

		/// <summary>
		/// Notify the user about an error (not that he'll be able to do anything about it...).
		/// </summary>
		void ShowError(string msg)
		{
			System.Windows.Forms.MessageBox.Show(msg, "Audio Device Error");
		}

		/// <summary>
		/// Initialize the library for recording.
		/// </summary>
		/// <returns>
		/// true if successful, false if an error occurs
		/// </returns>
		bool InitializeForRecording()
		{
			_pcmFormat = SND_pcm_FORMAT_S16_LE;
			_channelCount = 1;
			_sampleRate = 22000;
			_startDelay = 1;
			int res = snd_pcm_open(ref _hpcm, "default", SND_pcm_STREA_CAPTURE, 0);
			if (res < 0)
			{
				ShowError("Cannot open default sound device for recording");
				_hpcm = IntPtr.Zero;
				return false;
			}
			_audiobuf = new byte[_chunkSize];
			return true;
		}

		/// <summary>
		/// Sets the parameters for either recording or playing a sound file.
		/// </summary>
		/// <returns>
		/// true if successful, false if an error occurs.
		/// </returns>
		bool SetParams()
		{
			// allocate fresh data structures
			if (_hwparams != IntPtr.Zero)
				snd_pcm_hw_params_free(_hwparams);
			int res = snd_pcm_hw_params_malloc(ref _hwparams);
			if (res < 0)
			{
				return false;
			}
			if (_swparams != IntPtr.Zero)
				snd_pcm_sw_params_free(_swparams);
			res = snd_pcm_sw_params_malloc(ref _swparams);
			if (res < 0)
			{
				return false;
			}
			// Set the values we want for sound processing.
			res = snd_pcm_hw_params_any(_hpcm, _hwparams);
			if (res < 0)
			{
				return false;
			}
			res = snd_pcm_hw_params_set_access(_hpcm, _hwparams, SND_pcm_ACCESS_RW_INTERLEAVED);
			if (res < 0)
			{
				ShowError("Interleaved sound channel access is not available");
				Cleanup();
				return false;
			}
			res = snd_pcm_hw_params_set_format(_hpcm, _hwparams, _pcmFormat);
			if (res < 0)
			{
				ShowError("The desired sound format is not available");
				Cleanup();
				return false;
			}
			res = snd_pcm_hw_params_set_channels(_hpcm, _hwparams, (uint)_channelCount);
			if (res < 0)
			{
				ShowError("The desired sound channel count is not available");
				Cleanup();
				return false;
			}

			uint rate = _sampleRate;
			int dir = 0;
			res = snd_pcm_hw_params_set_rate_near(_hpcm, _hwparams, ref rate, ref dir);
			System.Diagnostics.Debug.Assert(res >= 0);
			_sampleRate = rate;

			res = snd_pcm_hw_params(_hpcm, _hwparams);
			if (res < 0)
			{
				ShowError("Unable to install hw params:");
				Cleanup();
				return false;
			}
			snd_pcm_hw_params_get_period_size(_hwparams, ref _chunkSize, ref dir);
			snd_pcm_hw_params_get_buffer_size(_hwparams, ref _bufferSize);
			if (_chunkSize == _bufferSize)
			{
				ShowError(String.Format("Can't use period equal to buffer size (%lu == %lu)", _chunkSize, _bufferSize));
				Cleanup();
				return false;
			}

			snd_pcm_sw_params_current(_hpcm, _swparams);
			ulong n = (ulong)_chunkSize;
			res = snd_pcm_sw_params_set_avail_min(_hpcm, _swparams, n);

			/* round up to closest transfer boundary */
			n = (ulong)_bufferSize;
			ulong start_threshold;
			if (_startDelay <= 0)
			{
				start_threshold = n + (ulong)((double) rate * _startDelay / 1000000);
			}
			else
			{
				start_threshold = (ulong)((double) rate * _startDelay / 1000000);
			}
			if (start_threshold < 1)
				start_threshold = 1;
			if (start_threshold > n)
				start_threshold = n;
			res = snd_pcm_sw_params_set_start_threshold(_hpcm, _swparams, start_threshold);
			Debug.Assert(res >= 0);
			ulong stop_threshold = (ulong)_bufferSize;
			res = snd_pcm_sw_params_set_stop_threshold(_hpcm, _swparams, stop_threshold);
			Debug.Assert(res >= 0);

			if (snd_pcm_sw_params(_hpcm, _swparams) < 0)
			{
				ShowError("unable to install sw params:");
				Cleanup();
				return false;
			}
			int bitsPerSample = snd_pcm_format_physical_width(_pcmFormat);
			_bitsPerFrame = (ushort)(bitsPerSample * _channelCount);
			_chunkBytes = _chunkSize * _bitsPerFrame / 8;
			_audiobuf = new byte[_chunkBytes];
			return true;
		}

		/// <summary>
		/// Cleanup this instance by releasing any allocated resources.
		/// </summary>
		void Cleanup()
		{
			if (_audiobuf != null)
				_audiobuf = null;
			if (_hwparams != IntPtr.Zero)
			{
				snd_pcm_hw_params_free(_hwparams);
				_hwparams = IntPtr.Zero;
			}
			if (_swparams != IntPtr.Zero)
			{
				snd_pcm_sw_params_free(_swparams);
				_swparams = IntPtr.Zero;
			}
			if (_hpcm != IntPtr.Zero)
			{
				snd_pcm_close(_hpcm);
				_hpcm = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Record until out of room for a WAVE file, or until interrupted by the user.
		/// Write the raw audio data to a temporary file.
		/// </summary>
		void Record()
		{
			if (!InitializeForRecording())
				return;
			SetParams();

			_tempfile = Path.GetTempFileName();
			var file = File.Create(_tempfile);
			int rest = Int32.MaxValue;
			_cbWritten = 0;
			while (rest > 0 && !_fAsyncQuit)
			{
				int byteCount = (rest < _chunkBytes) ? rest : _chunkBytes;
				int frameCount = byteCount * 8 / _bitsPerFrame;
				Debug.Assert(frameCount == _chunkSize);
				int count = (int)snd_pcm_readi(_hpcm, _audiobuf, (long)frameCount);
				if (count != frameCount)
				{
					Console.WriteLine("AlsaAudioDevice: stopping because returned count ({0}) != frameCount ({1})", count, frameCount);
					break;
				}
				file.Write(_audiobuf, 0, byteCount);
				rest -= byteCount;
				_cbWritten += byteCount;
			}
			file.Close();
			file.Dispose();
			Cleanup();
			_fAsyncQuit = false;
		}

		/// <summary>
		/// Initialize the library for playing a WAVE file.
		/// </summary>
		/// <returns>
		/// true if successful, false if an error occurs
		/// </returns>
		bool InitializeForPlayback()
		{
			_pcmFormat = 0;
			_channelCount = 0;
			_sampleRate = 0;
			_startDelay = 0;
			int res = snd_pcm_open(ref _hpcm, "default", SND_pcm_STREA_PLAYBACK, 0);
			if (res < 0)
			{
				ShowError("Cannot open default sound device for recording");
				_hpcm = IntPtr.Zero;
				return false;
			}
			return true;
		}

		/// <summary>
		/// Check the given file to make sure it's a WAVE file that we know how to play.
		/// </summary>
		/// <returns>
		/// true if the file is a WAVE file we can play, otherwise false
		/// </returns>
		bool ValidateFile(string fileName, out string errorMsg)
		{
			errorMsg = null;
			using (var reader = new WaveFileReader(fileName))
			{
				var header = reader.ReadWaveFileHeader();
				if (header.chunkId != "RIFF" || header.riffType != "WAVE")
				{
					errorMsg = String.Format("{0} is not a WAVE file.", _filename);
					return false;
				}
				var formatPlaying = reader.ReadWaveFormatChunk();
				WaveDataHeader dataheader = reader.ReadWaveDataHeader();
				if (formatPlaying.chunkId != "fmt " || formatPlaying.chunkSize != 16 || dataheader.chunkId != "data")
				{
					errorMsg = String.Format("{0} is not a recognized type of WAVE file.", _filename);
					return false;
				}
				if (formatPlaying.audioFormat != WAV_FMT_PCM)
				{
					errorMsg = String.Format("{0} is not PCM encoded ({1}).", _filename, formatPlaying.audioFormat);
					return false;
				}
				if (formatPlaying.channelCount < 1)
				{
					errorMsg = String.Format("{0} has an invalid number of channels ({1}).", _filename, formatPlaying.channelCount);
					return false;
				}
				if (ComputePcmFormatFromWaveFormat(formatPlaying, out errorMsg) < 0)
					return false;
				//Console.WriteLine("{0} is a WAVE file recorded at {1}Hz, using {2} bits/sample and {3} channels.",
				//	fileName, formatPlaying.sampleRate, formatPlaying.bitsPerSample, formatPlaying.channelCount);
			}
			return true;
		}

		/// <summary>
		/// Play the WAVE file that we've prepared.  This occurs on its own thread so that
		/// the user can asynchronously stop the playback.
		/// </summary>
		void Play()
		{
			if (!InitializeForPlayback())
				return;
			using (var reader = new WaveFileReader(_filename))
			{
				reader.ReadWaveFileHeader();
				var formatPlaying = reader.ReadWaveFormatChunk();
				var dataheader = reader.ReadWaveDataHeader();
				string errorMsg;
				_pcmFormat = ComputePcmFormatFromWaveFormat(formatPlaying, out errorMsg);
				_channelCount = formatPlaying.channelCount;
				_sampleRate = formatPlaying.sampleRate;
				SetParams();
				int cbRemaining = (int)dataheader.chunkSize;
				while (cbRemaining >= _chunkBytes && !_fAsyncQuit)
				{
					var bytes = reader.ReadWaveData(_chunkBytes);
					if (bytes == null || bytes.Length != _chunkBytes)
						break;
					int size = snd_pcm_writei(_hpcm, bytes, _chunkSize);
					if (size != _chunkSize)
						break;
					cbRemaining -= _chunkBytes;
				}
			}
			snd_pcm_nonblock(_hpcm, 0);
			snd_pcm_drain(_hpcm);
			snd_pcm_nonblock(_hpcm, 0);
			Cleanup();
			_fAsyncQuit = false;
		}

		/// <summary>
		/// Computes the library's PCM format from wave file's format.
		/// </summary>
		/// <returns>
		/// -1 if the format is unplayable, otherwise the (positive) value representing
		/// the PCM format.
		/// </returns>
		int ComputePcmFormatFromWaveFormat(WaveFormatChunk formatPlaying, out string errorMsg)
		{
			errorMsg = null;
			switch (formatPlaying.bitsPerSample)
			{
			case 8:		return SND_pcm_FORMAT_U8;
			case 16:	return SND_pcm_FORMAT_S16_LE;
			case 24:
				switch (formatPlaying.blockAlign / formatPlaying.channelCount)
				{
				case 3: return SND_pcm_FORMAT_S24_3LE;
				case 4: return SND_pcm_FORMAT_S24_LE;
				}
				break;
			case 32:	return SND_pcm_FORMAT_S32_LE;
			}
			errorMsg = String.Format("Cannot play WAVE files with {0}-bit samples in {1} bytes ({2} channels)",
				formatPlaying.bitsPerSample, formatPlaying.blockAlign, formatPlaying.channelCount);
			return -1;
		}

		/// <summary>
		/// WAVE file header.
		/// </summary>
		public struct WaveFileHeader
		{
			/// <summary>must == "RIFF" (actually stored as four bytes in the file)</summary>
			public string chunkId;
			/// <summary>file size - 8</summary>
			public UInt32 chunkSize;
			/// <summary>must == "WAVE" (actually stored as four bytes in the file)</summary>
			public string riffType;
		}

		/// <summary>
		/// Wave format block that immediately follows the file header.
		/// </summary>
		public struct WaveFormatChunk
		{
			/// <summary>must == "fmt " (actually stored as bytes in the file)</summary>
			public string chunkId;
			/// <summary>must == 16 for us to work with this file</summary>
			public UInt32 chunkSize;
			/// <summary>must == 1 (PCM) for us to work with this file</summary>
			public UInt16 audioFormat;
			/// <summary>number of channels (tracks) in the recording</summary>
			public UInt16 channelCount;
			/// <summary>number of samples per second</summary>
			public UInt32 sampleRate;
			/// <summary>number of bytes consumed per second</summary>
			public UInt32 byteRate;
			/// <summary>number of bytes per sample</summary>
			public UInt16 blockAlign;
			/// <summary>number of bits per sample (better be a small multiple of 8!)</summary>
			public UInt16 bitsPerSample;
		}

		/// <summary>
		/// Wave data header that immediately follows the format block.
		/// </summary>
		public struct WaveDataHeader
		{
			/// <summary>must == "data" (actually stored as bytes in the file)</summary>
			public string chunkId;
			/// <summary>data size must be file size - 44 for us to work with this file</summary>
			public UInt32 chunkSize;
		}

		/// <summary>
		/// Utility class for reading a WAVE file.
		/// </summary>
		public class WaveFileReader : IDisposable
		{
			BinaryReader _reader;

			/// <summary>
			/// Initializes a new instance of the <see cref="Palaso.Media.AlsaAudioDevice.WaveFileReader"/> class.
			/// </summary>
			public WaveFileReader(string filename)
			{
				_reader = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
			}

			#region General Utility Methods

			/// <summary>
			/// Reads a 4 byte segment and tries to convert the bytes into a string.
			/// </summary>
			protected string ReadRiffTypeName()
			{
				byte[] bytes = _reader.ReadBytes(4);
				char[] chars = new char[4];
				try
				{
					for (int i = 0; i < 4; ++i)
						chars[i] = (char)bytes[i];
					return new string(chars);
				}
				catch
				{
					return "????";
				}
			}

			/// <summary>
			/// Reads the WAVE file header, without any verification.
			/// </summary>
			public WaveFileHeader ReadWaveFileHeader()
			{
				var header = new WaveFileHeader();
				header.chunkId = ReadRiffTypeName();
				header.chunkSize = _reader.ReadUInt32();
				header.riffType = ReadRiffTypeName();
				return header;
			}

			/// <summary>
			/// Reads the WAVE format block, without any verification.
			/// </summary>
			public WaveFormatChunk ReadWaveFormatChunk()
			{
				var chunk = new WaveFormatChunk();
				chunk.chunkId = ReadRiffTypeName();
				chunk.chunkSize = _reader.ReadUInt32();
				chunk.audioFormat = _reader.ReadUInt16();
				chunk.channelCount = _reader.ReadUInt16();
				chunk.sampleRate = _reader.ReadUInt32();
				chunk.byteRate = _reader.ReadUInt32();
				chunk.blockAlign = _reader.ReadUInt16();
				chunk.bitsPerSample = _reader.ReadUInt16();
				return chunk;
			}

			/// <summary>
			/// Reads the WAVE data header, without any verification.
			/// </summary>
			public WaveDataHeader ReadWaveDataHeader()
			{
				var header = new WaveDataHeader();
				header.chunkId = ReadRiffTypeName();
				header.chunkSize =  _reader.ReadUInt32();
				return header;
			}

			/// <summary>
			/// Reads the requested amount of sound data from the WAVE file.
			/// </summary>
			public byte[] ReadWaveData(int count)
			{
				return _reader.ReadBytes(count);
			}
			#endregion

			#region IDisposable Members

			/// <summary>
			/// Release all resources used by the <see cref="Palaso.Media.AlsaAudioDevice.WaveFileReader"/> object.
			/// </summary>
			public void Dispose()
			{
				if(_reader != null)
					_reader.Close();
			}

			#endregion
		}

		/// <summary>
		/// Utility class for writing a WAVE file.
		/// </summary>
		public class WaveFileWriter
		{
			BinaryWriter _writer;

			/// <summary>
			/// Initializes a new instance of the <see cref="Palaso.Media.AlsaAudioDevice.WaveFileWriter"/> class.
			/// </summary>
			public WaveFileWriter(string filename)
			{
				_writer = new BinaryWriter(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None));
			}

			/// <summary>
			/// Writes the WAVE file header.
			/// </summary>
			public void WriteFileHeader(int dataSize)
			{
				if (_writer == null)
					return;
				_writer.Write((byte)'R');
				_writer.Write((byte)'I');
				_writer.Write((byte)'F');
				_writer.Write((byte)'F');
				_writer.Write((Int32)(dataSize + 4 + 24 + 8));
				_writer.Write((byte)'W');
				_writer.Write((byte)'A');
				_writer.Write((byte)'V');
				_writer.Write((byte)'E');
			}

			/// <summary>
			/// Writes the WAVE format block.
			/// </summary>
			public void WriteFormatChunk(WaveFormatChunk format)
			{
				if (_writer == null)
					return;
				_writer.Write((byte)'f');
				_writer.Write((byte)'m');
				_writer.Write((byte)'t');
				_writer.Write((byte)' ');
				_writer.Write(format.chunkSize);
				_writer.Write(format.audioFormat);
				_writer.Write(format.channelCount);
				_writer.Write(format.sampleRate);
				_writer.Write(format.byteRate);
				_writer.Write(format.blockAlign);
				_writer.Write(format.bitsPerSample);
			}

			/// <summary>
			/// Writes the WAVE data header.
			/// </summary>
			public void WriteDataHeader(int dataSize)
			{
				if (_writer == null)
					return;
				_writer.Write((byte)'d');
				_writer.Write((byte)'a');
				_writer.Write((byte)'t');
				_writer.Write((byte)'a');
				_writer.Write(dataSize);
			}

			/// <summary>
			/// Writes the WAVE sound data.
			/// </summary>
			public void WriteData(byte[] data)
			{
				if (_writer == null)
					return;
				_writer.Write(data);
			}

			/// <summary>
			/// Close this instance.
			/// </summary>
			public void Close()
			{
				if (_writer != null)
				{
					_writer.Close();
					_writer = null;
				}
			}
		}
	}
}
