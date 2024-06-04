using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace SIL.Windows.Forms.Media.AlsaAudio
{
	/// <summary>
	/// Simplified wrapper around the standard Linux Alsa audio library.  This wrapper defaults
	/// to recording simple 16-bit PCM mono WAVE files at 22KHz, and plays back using libsndfile
	/// to read the sound data.  (The number of input channels and the sample rate can be changed
	/// if desired and if the hardware supports the new setting.)
	/// </summary>
	public class AlsaAudioDevice
	{
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_open(ref IntPtr pcm, string pc_name, int stream, int mode);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_close(IntPtr pcm);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_drain(IntPtr pcm);
		// The next five methods are really all the same method, just with the data being passed
		// in different forms.  (yes, this appears to work okay!)
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_writei(IntPtr pcm, byte[] buf, int size);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_writei(IntPtr pcm, short[] buf, int size);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_writei(IntPtr pcm, int[] buf, int size);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_writei(IntPtr pcm, float[] buf, int size);
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_writei(IntPtr pcm, double[] buf, int size);
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
		const int SND_PCM_STREA_PLAYBACK = 0;
		const int SND_PCM_STREA_CAPTURE = 1;
		const int SND_PCM_FORMAT_U8 = 1;
		const int SND_PCM_FORMAT_S16_LE = 2;
		const int SND_PCM_FORMAT_S24_LE = 6;
		const int SND_PCM_FORMAT_S32_LE = 10;
		const int SND_PCM_FORMAT_FLOAT_LE = 14;
		const int SND_PCM_FORMAT_FLOAT64_LE = 16;
		const int SND_PCM_FORMAT_S24_3LE = 32;
		const int SND_PCM_ACCESS_RW_INTERLEAVED = 3;

		// This constant is cribbed from Microsoft.
		const ushort WAV_FMT_PCM = 1;

		/// <summary>
		/// The SF_Mode enum is adapted from sndfile.h
		/// </summary>
		internal enum SF_Mode
		{
			/* True and false */
			FALSE	= 0,
			TRUE	= 1,

			/* Modes for opening files. */
			READ	= 0x10,
			WRITE	= 0x20,
			RDWR	= 0x30,

			AMBISONIC_NONE		= 0x40,
			AMBISONIC_B_FORMAT	= 0x41
		}

		/// <summary>
		/// The SF_Format enum is adapted from sndfile.h
		/// </summary>
		[Flags]
		internal enum SF_Format : uint
		{
			/* Major formats. */
			WAV			= 0x010000,		/* Microsoft WAV format (little endian default). */
			AIFF		= 0x020000,		/* Apple/SGI AIFF format (big endian). */
			AU			= 0x030000,		/* Sun/NeXT AU format (big endian). */
			RAW			= 0x040000,		/* RAW PCM data. */
			PAF			= 0x050000,		/* Ensoniq PARIS file format. */
			SVX			= 0x060000,		/* Amiga IFF / SVX8 / SV16 format. */
			NIST		= 0x070000,		/* Sphere NIST format. */
			VOC			= 0x080000,		/* VOC files. */
			IRCAM		= 0x0A0000,		/* Berkeley/IRCAM/CARL */
			W64			= 0x0B0000,		/* Sonic Foundry's 64 bit RIFF/WAV */
			MAT4		= 0x0C0000,		/* Matlab (tm) V4.2 / GNU Octave 2.0 */
			MAT5		= 0x0D0000,		/* Matlab (tm) V5.0 / GNU Octave 2.1 */
			PVF			= 0x0E0000,		/* Portable Voice Format */
			XI			= 0x0F0000,		/* Fasttracker 2 Extended Instrument */
			HTK			= 0x100000,		/* HMM Tool Kit format */
			SDS			= 0x110000,		/* Midi Sample Dump Standard */
			AVR			= 0x120000,		/* Audio Visual Research */
			WAVEX		= 0x130000,		/* MS WAVE with WAVEFORMATEX */
			SD2			= 0x160000,		/* Sound Designer 2 */
			FLAC		= 0x170000,		/* FLAC lossless file format */
			CAF			= 0x180000,		/* Core Audio File format */
			WVE			= 0x190000,		/* Psion WVE format */
			OGG			= 0x200000,		/* Xiph OGG container */
			MPC2K		= 0x210000,		/* Akai MPC 2000 sampler */
			RF64		= 0x220000,		/* RF64 WAV file */

			/* Subtypes from here on. */

			PCM_S8		= 0x0001,		/* Signed 8 bit data */
			PCM_16		= 0x0002,		/* Signed 16 bit data */
			PCM_24		= 0x0003,		/* Signed 24 bit data */
			PCM_32		= 0x0004,		/* Signed 32 bit data */

			PCM_U8		= 0x0005,		/* Unsigned 8 bit data (WAV and RAW only) */

			FLOAT		= 0x0006,		/* 32 bit float data */
			DOUBLE		= 0x0007,		/* 64 bit float data */

			ULAW		= 0x0010,		/* U-Law encoded. */
			ALAW		= 0x0011,		/* A-Law encoded. */
			IMA_ADPCM	= 0x0012,		/* IMA ADPCM. */
			MS_ADPCM	= 0x0013,		/* Microsoft ADPCM. */

			GSM610		= 0x0020,		/* GSM 6.10 encoding. */
			VOX_ADPCM	= 0x0021,		/* OKI / Dialogix ADPCM */

			G721_32		= 0x0030,		/* 32kbs G721 ADPCM encoding. */
			G723_24		= 0x0031,		/* 24kbs G723 ADPCM encoding. */
			G723_40		= 0x0032,		/* 40kbs G723 ADPCM encoding. */

			DWVW_12		= 0x0040, 		/* 12 bit Delta Width Variable Word encoding. */
			DWVW_16		= 0x0041, 		/* 16 bit Delta Width Variable Word encoding. */
			DWVW_24		= 0x0042, 		/* 24 bit Delta Width Variable Word encoding. */
			DWVW_N		= 0x0043, 		/* N bit Delta Width Variable Word encoding. */

			DPCM_8		= 0x0050,		/* 8 bit differential PCM (XI only) */
			DPCM_16		= 0x0051,		/* 16 bit differential PCM (XI only) */

			VORBIS		= 0x0060,		/* Xiph Vorbis encoding. */

			/* Endian-ness options. */

			ENDIAN_FILE			= 0x00000000,	/* Default file endian-ness. */
			ENDIAN_LITTLE		= 0x10000000,	/* Force little endian-ness. */
			ENDIAN_BIG			= 0x20000000,	/* Force big endian-ness. */
			ENDIAN_CPU			= 0x30000000,	/* Force CPU endian-ness. */

			SUBMASK		= 0x0000FFFF,
			TYPEMASK	= 0x0FFF0000,
			ENDMASK		= 0x30000000
		}

		/// <summary>
		/// The SF_INFO struct is adapted from sndfile.h
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct SF_INFO
		{
			public long frames;
			public int samplerate;
			public int channels;
			public SF_Format format;
			public int sections;
			public int seekable;
		}

		[DllImport("libsndfile.so.1")]
		internal static extern IntPtr sf_open(string path, SF_Mode mode, ref SF_INFO info);
		[DllImport("libsndfile.so.1")]
		internal static extern long sf_readf_short(IntPtr sndfile, short[] ptr, long frames) ;
		[DllImport("libsndfile.so.1")]
		internal static extern long sf_readf_int(IntPtr sndfile, int[] ptr, long frames) ;
		[DllImport("libsndfile.so.1")]
		internal static extern long sf_readf_float(IntPtr sndfile, float[] ptr, long frames) ;
		[DllImport("libsndfile.so.1")]
		internal static extern long sf_readf_double(IntPtr sndfile, double[] ptr, long frames) ;
		[DllImport("libsndfile.so.1")]
		internal static extern int sf_close(IntPtr sndfile);

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
		IntPtr _hsf;
		SF_INFO _info;

		#region Construction and Destruction

		/// <summary>
		/// Initialize a new instance of the <see cref="SIL.Windows.Forms.Media.AlsaAudio.AlsaAudioDevice"/> class.
		/// </summary>
		public AlsaAudioDevice()
		{
			// Set the defaults for recording.
			DesiredSampleRate = 22000;
			DesiredChannelCount = 1;
			DesiredInputDevice = "default";
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
		/// Play the specified sound file.
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
			_info = new SF_INFO();
			_hsf = sf_open(fileName, SF_Mode.READ, ref _info);
			if (_hsf == IntPtr.Zero)
			{
				ShowError(String.Format("Sound player cannot open {0}", fileName));
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
		/// true iff a sound file is playing.
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

		public uint DesiredSampleRate { get; set; }
		public ushort DesiredChannelCount { get; set; }
		public string DesiredInputDevice { get; set; }

		#endregion

		/// <summary>
		/// Notify the user about an error (not that he'll be able to do anything about it...).
		/// </summary>
		void ShowError(string msg)
		{
			Console.WriteLine("Audio Device Error: {0}", msg);
		}

		/// <summary>
		/// Initialize the library for recording.
		/// </summary>
		/// <returns>
		/// true if successful, false if an error occurs
		/// </returns>
		bool InitializeForRecording()
		{
			_pcmFormat = SND_PCM_FORMAT_S16_LE;
			_channelCount = DesiredChannelCount;
			_sampleRate = DesiredSampleRate;
			_startDelay = 1;
			int res = snd_pcm_open(ref _hpcm, DesiredInputDevice, SND_PCM_STREA_CAPTURE, 0);
			if (res < 0)
			{
				ShowError(String.Format("Cannot open {0} sound device for recording", DesiredInputDevice));
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
			res = snd_pcm_hw_params_set_access(_hpcm, _hwparams, SND_PCM_ACCESS_RW_INTERLEAVED);
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
			if (_hsf != IntPtr.Zero)
			{
				sf_close(_hsf);
				_hsf = IntPtr.Zero;
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
				if (count < 0)
				{
					Console.WriteLine("AlsaAudioDevice: stopping because returned count ({0}) signals an error", count);
					break;
				}
				if (count != frameCount)
				{
					Debug.WriteLine(String.Format("AlsaAudioDevice: short read ({0} < requested {1} frames)", count, frameCount));
					byteCount = (count * _bitsPerFrame) / 8;
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
		/// Initialize the library for playing a sound file.
		/// </summary>
		/// <returns>
		/// true if successful, false if an error occurs
		/// </returns>
		bool InitializeForPlayback()
		{
			if (_hsf == IntPtr.Zero)
				return false;
			int res = snd_pcm_open(ref _hpcm, "default", SND_PCM_STREA_PLAYBACK, 0);
			if (res < 0)
			{
				ShowError("Cannot open default sound device for recording");
				_hpcm = IntPtr.Zero;
				return false;
			}
			switch (_info.format & SF_Format.SUBMASK)
			{
			case SF_Format.PCM_24:
			case SF_Format.PCM_32:
				_pcmFormat = SND_PCM_FORMAT_S32_LE;
				break;
			case SF_Format.FLOAT:
				_pcmFormat = SND_PCM_FORMAT_FLOAT_LE;
				break;
			case SF_Format.DOUBLE:
				_pcmFormat = SND_PCM_FORMAT_FLOAT64_LE;
				break;
			default:
				_pcmFormat = SND_PCM_FORMAT_S16_LE;
				break;
			}
			_channelCount = (ushort)_info.channels;
			_sampleRate = (uint)_info.samplerate;
			_startDelay = 0;
			SetParams();
			return true;
		}

		/// <summary>
		/// Play the sound file that we've opened.  The playback occurs on its own thread so that
		/// the user can asynchronously stop the playback.
		/// </summary>
		void Play()
		{
			if (!InitializeForPlayback())
				return;
			try
			{
				int remaining = (int)_info.frames;
				switch (_pcmFormat)
				{
				case SND_PCM_FORMAT_S32_LE:
					var ints = new int[1024 * _info.channels];
					while (remaining > 0)
					{
						int num = (remaining >= 1024) ? 1024 : remaining;
						long cf = sf_readf_int(_hsf, ints, num);
						if (cf != num)
							break;
						int num2 = snd_pcm_writei(_hpcm, ints, num);
						if (num2 != num)
							break;
						remaining -= num;
					}
					break;
				case SND_PCM_FORMAT_FLOAT_LE:
					var floats = new float[1024 * _info.channels];
					while (remaining > 0)
					{
						int num = (remaining >= 1024) ? 1024 : remaining;
						long cf = sf_readf_float(_hsf, floats, num);
						if (cf != num)
							break;
						int num2 = snd_pcm_writei(_hpcm, floats, num);
						if (num2 != num)
							break;
						remaining -= num;
					}
					break;
				case SND_PCM_FORMAT_FLOAT64_LE:
					var doubles = new double[1024 * _info.channels];
					while (remaining > 0)
					{
						int num = (remaining >= 1024) ? 1024 : remaining;
						long cf = sf_readf_double(_hsf, doubles, num);
						if (cf != num)
							break;
						int num2 = snd_pcm_writei(_hpcm, doubles, num);
						if (num2 != num)
							break;
						remaining -= num;
					}
					break;
				default:
					var shorts = new short[1024 * _info.channels];
					while (remaining > 0)
					{
						int num = (remaining >= 1024) ? 1024 : remaining;
						long cf = sf_readf_short(_hsf, shorts, num);
						if (cf != num)
							break;
						int num2 = snd_pcm_writei(_hpcm, shorts, num);
						if (num2 != num)
							break;
						remaining -= num;
					}
					break;
				}
				if (remaining > 0)
				{
					ShowError(String.Format("Error trying to play {0}", _filename));
				}
				snd_pcm_nonblock(_hpcm, 0);
				snd_pcm_drain(_hpcm);
				snd_pcm_nonblock(_hpcm, 0);
			}
			finally
			{
				Cleanup();
				_fAsyncQuit = false;
			}
		}

		/// <summary>
		/// The common heading of every "chunk" of a WAVE file.
		/// </summary>
		public class WaveChunk
		{
			/// <summary>The chunk identifier (four 8-bit chars in the file)</summary>
			public string chunkId;
			/// <summary>The size of the chunk following this header</summary>
			public UInt32 chunkSize;
		}

		/// <summary>
		/// Wave format block: chunkId == "fmt ", chunkSize >= 16
		/// </summary>
		public class WaveFormatChunk : WaveChunk
		{
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
			/// <summary>extra information found in some files</summary>
			public byte[] extraInfo;
		}

		/// <summary>
		/// Utility class for writing a WAVE file.
		/// </summary>
		public class WaveFileWriter
		{
			BinaryWriter _writer;

			/// <summary>
			/// Initializes a new instance of the <see cref="SIL.Windows.Forms.Media.AlsaAudio.AlsaAudioDevice.WaveFileWriter"/> class.
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
