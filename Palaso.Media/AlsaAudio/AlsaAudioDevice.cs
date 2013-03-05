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
		static extern int snd_pcm_open(ref IntPtr pcm, string pcm_name, int stream, int mode);
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
		const int SND_PCM_STREAM_PLAYBACK = 0;
		const int SND_PCM_STREAM_CAPTURE = 1;
		const int SND_PCM_FORMAT_U8 = 1;
		const int SND_PCM_FORMAT_S16_LE = 2;
		const int SND_PCM_FORMAT_S24_LE = 6;
		const int SND_PCM_FORMAT_S32_LE = 10;
		const int SND_PCM_FORMAT_FLOAT_LE = 14;
		const int SND_PCM_FORMAT_S24_3LE = 32;
		const int SND_PCM_ACCESS_RW_INTERLEAVED = 3;

		// These constants are cribbed from Microsoft.
		const ushort WAV_FMT_PCM = 1;

		IntPtr m_hpcm;
		IntPtr m_hwparams;
		IntPtr m_swparams;
		Thread m_recordingThread;
		Thread m_playbackThread;
		bool m_fAsyncQuit;
		int m_pcmFormat;
		ushort m_channelCount;
		uint m_sampleRate;
		int m_startDelay;
		int m_chunkSize = 1024;
		int m_bufferSize;
		byte[] m_audiobuf;
		ushort m_bitsPerFrame;
		string m_tempfile;
		int m_chunkBytes;
		int m_cbWritten;
		string m_filename;
		int m_cbRemaining;
		string m_error;

		#region Construction

		/// <summary>
		/// Initialize a new instance of the <see cref="Palaso.Media.AlsaAudioDevice"/> class.
		/// </summary>
		public AlsaAudioDevice()
		{
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
			m_error = null;
			if (!ValidateFile(fileName))
			{
				ShowError(m_error);
				return false;
			}
			m_filename = fileName;
			m_fAsyncQuit = false;

			// Create the thread object, passing in the Record method
			// via a ThreadStart delegate. This does not start the thread.
			m_playbackThread = new Thread(new ThreadStart(Play));

			// Start the thread
			m_playbackThread.Start();

			// Wait for the started thread to become alive:
			while (!m_playbackThread.IsAlive)
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

			m_fAsyncQuit = false;

			// Create the thread object, passing in the Record method
			// via a ThreadStart delegate. This does not start the thread.
			m_recordingThread = new Thread(new ThreadStart(Record));

			// Start the thread
			m_recordingThread.Start();

			// Wait for the started thread to become alive:
			while (!m_recordingThread.IsAlive)
				;
			return true;
		}

		/// <summary>
		/// true iff recording is underway.
		/// </summary>
		public bool IsRecording
		{
			get { return m_recordingThread != null && m_recordingThread.IsAlive; }
		}

		/// <summary>
		/// Stop the recording, waiting for the thread to die.
		/// </summary>
		public void StopRecording()
		{
			if (!IsRecording)
				return;
			m_fAsyncQuit = true;
			m_recordingThread.Join(1000);
			if (m_recordingThread.IsAlive)
				m_recordingThread.Abort();
			m_recordingThread = null;
		}

		/// <summary>
		/// Write the stored audio data as a WAVE file.
		/// </summary>
		public void SaveAsWav(string filePath)
		{
			if (String.IsNullOrEmpty(m_tempfile) || !File.Exists(m_tempfile) || m_cbWritten == 0)
				return;
			FileInfo fi = new FileInfo(m_tempfile);
			Debug.Assert(fi.Length == m_cbWritten);
			WaveFileWriter writer = new WaveFileWriter(filePath);
			writer.WriteFileHeader((int)fi.Length);
			WaveFormatChunk format = new WaveFormatChunk();
			format.chunkId = "fmt ";
			format.chunkSize = 16;				// size of the struct in bytes - 8
			format.audioFormat = WAV_FMT_PCM;
			format.channelCount = m_channelCount;
			format.sampleRate = m_sampleRate;
			format.byteRate = (uint)(m_sampleRate * m_channelCount * m_bitsPerFrame / 8);
			format.blockAlign = (ushort)(m_channelCount * m_bitsPerFrame / 8);
			format.bitsPerSample = m_bitsPerFrame;
			writer.WriteFormatChunk(format);
			writer.WriteDataHeader((int)fi.Length);
			byte[] data = File.ReadAllBytes(m_tempfile);
			Debug.Assert(data.Length == m_cbWritten);
			writer.WriteData(data);
			writer.Close();
		}

		/// <summary>
		/// true iff a WAVE file is playing.
		/// </summary>
		public bool IsPlaying
		{
			get { return m_playbackThread != null && m_playbackThread.IsAlive; }
		}

		/// <summary>
		/// Stop the playing, waiting for the thread to die.
		/// </summary>
		public void StopPlaying()
		{
			if (!IsPlaying)
				return;
			m_fAsyncQuit = true;
			m_playbackThread.Join(1000);
			if (m_playbackThread.IsAlive)
				m_playbackThread.Abort();
			m_playbackThread = null;
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
			m_pcmFormat = SND_PCM_FORMAT_S16_LE;
			m_channelCount = 1;
			m_sampleRate = 22000;
			m_startDelay = 1;
			int res = snd_pcm_open(ref m_hpcm, "default", SND_PCM_STREAM_CAPTURE, 0);
			if (res < 0)
			{
				ShowError("Cannot open default sound device for recording");
				m_hpcm = IntPtr.Zero;
				return false;
			}
			m_audiobuf = new byte[m_chunkSize];
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
			if (m_hwparams != IntPtr.Zero)
				snd_pcm_hw_params_free(m_hwparams);
			int res = snd_pcm_hw_params_malloc(ref m_hwparams);
			if (res < 0)
			{
				return false;
			}
			if (m_swparams != IntPtr.Zero)
				snd_pcm_sw_params_free(m_swparams);
			res = snd_pcm_sw_params_malloc(ref m_swparams);
			if (res < 0)
			{
				return false;
			}
			// Set the values we want for sound processing.
			res = snd_pcm_hw_params_any(m_hpcm, m_hwparams);
			if (res < 0)
			{
				return false;
			}
			res = snd_pcm_hw_params_set_access(m_hpcm, m_hwparams, SND_PCM_ACCESS_RW_INTERLEAVED);
			if (res < 0)
			{
				ShowError("Interleaved sound channel access is not available");
				Cleanup();
				return false;
			}
			res = snd_pcm_hw_params_set_format(m_hpcm, m_hwparams, m_pcmFormat);
			if (res < 0)
			{
				ShowError("The desired sound format is not available");
				Cleanup();
				return false;
			}
			res = snd_pcm_hw_params_set_channels(m_hpcm, m_hwparams, (uint)m_channelCount);
			if (res < 0)
			{
				ShowError("The desired sound channel count is not available");
				Cleanup();
				return false;
			}

			uint rate = m_sampleRate;
			int dir = 0;
			res = snd_pcm_hw_params_set_rate_near(m_hpcm, m_hwparams, ref rate, ref dir);
			System.Diagnostics.Debug.Assert(res >= 0);
			m_sampleRate = rate;

			res = snd_pcm_hw_params(m_hpcm, m_hwparams);
			if (res < 0)
			{
				ShowError("Unable to install hw params:");
				Cleanup();
				return false;
			}
			snd_pcm_hw_params_get_period_size(m_hwparams, ref m_chunkSize, ref dir);
			snd_pcm_hw_params_get_buffer_size(m_hwparams, ref m_bufferSize);
			if (m_chunkSize == m_bufferSize)
			{
				ShowError(String.Format("Can't use period equal to buffer size (%lu == %lu)", m_chunkSize, m_bufferSize));
				Cleanup();
				return false;
			}

			snd_pcm_sw_params_current(m_hpcm, m_swparams);
			ulong n = (ulong)m_chunkSize;
			res = snd_pcm_sw_params_set_avail_min(m_hpcm, m_swparams, n);

			/* round up to closest transfer boundary */
			n = (ulong)m_bufferSize;
			ulong start_threshold;
			if (m_startDelay <= 0)
			{
				start_threshold = n + (ulong)((double) rate * m_startDelay / 1000000);
			}
			else
			{
				start_threshold = (ulong)((double) rate * m_startDelay / 1000000);
			}
			if (start_threshold < 1)
				start_threshold = 1;
			if (start_threshold > n)
				start_threshold = n;
			res = snd_pcm_sw_params_set_start_threshold(m_hpcm, m_swparams, start_threshold);
			Debug.Assert(res >= 0);
			ulong stop_threshold = (ulong)m_bufferSize;
			res = snd_pcm_sw_params_set_stop_threshold(m_hpcm, m_swparams, stop_threshold);
			Debug.Assert(res >= 0);

			if (snd_pcm_sw_params(m_hpcm, m_swparams) < 0)
			{
				ShowError("unable to install sw params:");
				Cleanup();
				return false;
			}
			int bitsPerSample = snd_pcm_format_physical_width(m_pcmFormat);
			m_bitsPerFrame = (ushort)(bitsPerSample * m_channelCount);
			m_chunkBytes = m_chunkSize * m_bitsPerFrame / 8;
			m_audiobuf = new byte[m_chunkBytes];
			return true;
		}

		/// <summary>
		/// Cleanup this instance by releasing any allocated resources.
		/// </summary>
		void Cleanup()
		{
			if (m_audiobuf != null)
				m_audiobuf = null;
			if (m_hwparams != IntPtr.Zero)
			{
				snd_pcm_hw_params_free(m_hwparams);
				m_hwparams = IntPtr.Zero;
			}
			if (m_swparams != IntPtr.Zero)
			{
				snd_pcm_sw_params_free(m_swparams);
				m_swparams = IntPtr.Zero;
			}
			if (m_hpcm != IntPtr.Zero)
			{
				snd_pcm_close(m_hpcm);
				m_hpcm = IntPtr.Zero;
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

			m_tempfile = Path.GetTempFileName();
			var file = File.Create(m_tempfile);
			int rest = Int32.MaxValue;
			m_cbWritten = 0;
			while (rest > 0 && !m_fAsyncQuit)
			{
				int byteCount = (rest < m_chunkBytes) ? rest : m_chunkBytes;
				int frameCount = byteCount * 8 / m_bitsPerFrame;
				Debug.Assert(frameCount == m_chunkSize);
				int count = (int)snd_pcm_readi(m_hpcm, m_audiobuf, (long)frameCount);
				if (count != frameCount)
				{
					Console.WriteLine("AlsaAudioDevice: stopping because returned count ({0}) != frameCount ({1})", count, frameCount);
					break;
				}
				file.Write(m_audiobuf, 0, byteCount);
				rest -= byteCount;
				m_cbWritten += byteCount;
			}
			file.Close();
			file.Dispose();
			Cleanup();
			m_fAsyncQuit = false;
		}

		/// <summary>
		/// Initialize the library for playing a WAVE file.
		/// </summary>
		/// <returns>
		/// true if successful, false if an error occurs
		/// </returns>
		bool InitializeForPlayback()
		{
			m_pcmFormat = 0;
			m_channelCount = 0;
			m_sampleRate = 0;
			m_startDelay = 0;
			int res = snd_pcm_open(ref m_hpcm, "default", SND_PCM_STREAM_PLAYBACK, 0);
			if (res < 0)
			{
				ShowError("Cannot open default sound device for recording");
				m_hpcm = IntPtr.Zero;
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
		bool ValidateFile(string fileName)
		{
			using (var reader = new WaveFileReader(fileName))
			{
				var header = reader.ReadWaveFileHeader();
				if (header.chunkId != "RIFF" || header.riffType != "WAVE")
				{
					m_error = String.Format("{0} is not a WAVE file.", m_filename);
					return false;
				}
				var formatPlaying = reader.ReadWaveFormatChunk();
				WaveDataHeader dataheader = reader.ReadWaveDataHeader();
				if (formatPlaying.chunkId != "fmt " || formatPlaying.chunkSize != 16 || dataheader.chunkId != "data")
				{
					m_error = String.Format("{0} is not a recognized type of WAVE file.", m_filename);
					return false;
				}
				if (formatPlaying.audioFormat != WAV_FMT_PCM)
				{
					m_error = String.Format("{0} is not PCM encoded ({1}).", m_filename, formatPlaying.audioFormat);
					return false;
				}
				if (formatPlaying.channelCount < 1)
				{
					m_error = String.Format("{0} has an invalid number of channels ({1}).", m_filename, formatPlaying.channelCount);
					return false;
				}
				if (ComputePcmFormatFromWaveFormat(formatPlaying) < 0)
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
			using (var reader = new WaveFileReader(m_filename))
			{
				reader.ReadWaveFileHeader();
				var formatPlaying = reader.ReadWaveFormatChunk();
				var dataheader = reader.ReadWaveDataHeader();
				m_pcmFormat = ComputePcmFormatFromWaveFormat(formatPlaying);
				m_channelCount = formatPlaying.channelCount;
				m_sampleRate = formatPlaying.sampleRate;
				m_cbRemaining = (int)dataheader.chunkSize;
				SetParams();
				while (m_cbRemaining >= m_chunkBytes && !m_fAsyncQuit)
				{
					var bytes = reader.ReadWaveData(m_chunkBytes);
					if (bytes == null || bytes.Length != m_chunkBytes)
						break;
					int size = snd_pcm_writei(m_hpcm, bytes, m_chunkSize);
					if (size != m_chunkSize)
						break;
					m_cbRemaining -= m_chunkBytes;
				}
			}
			snd_pcm_nonblock(m_hpcm, 0);
			snd_pcm_drain(m_hpcm);
			snd_pcm_nonblock(m_hpcm, 0);
			Cleanup();
			m_fAsyncQuit = false;
		}

		/// <summary>
		/// Computes the library's PCM format from wave file's format.
		/// </summary>
		/// <returns>
		/// -1 if the format is unplayable, otherwise the (positive) value representing
		/// the PCM format.
		/// </returns>
		int ComputePcmFormatFromWaveFormat(WaveFormatChunk formatPlaying)
		{
			switch (formatPlaying.bitsPerSample)
			{
			case 8:		return SND_PCM_FORMAT_U8;
			case 16:	return SND_PCM_FORMAT_S16_LE;
			case 24:
				switch (formatPlaying.blockAlign / formatPlaying.channelCount)
				{
				case 3: return SND_PCM_FORMAT_S24_3LE;
				case 4: return SND_PCM_FORMAT_S24_LE;
				}
				break;
			case 32:	return SND_PCM_FORMAT_S32_LE;
			}
			m_error = String.Format("Cannot play WAVE files with {0}-bit samples in {1} bytes ({2} channels)",
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
			BinaryReader m_reader;

			/// <summary>
			/// Initializes a new instance of the <see cref="Palaso.Media.AlsaAudioDevice.WaveFileReader"/> class.
			/// </summary>
			public WaveFileReader(string filename)
			{
				m_reader = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
			}

			#region General Utility Methods

			/// <summary>
			/// Reads a 4 byte segment and tries to convert the bytes into a string.
			/// </summary>
			protected string ReadRiffTypeName()
			{
				byte[] bytes = m_reader.ReadBytes(4);
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
				header.chunkSize = m_reader.ReadUInt32();
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
				chunk.chunkSize = m_reader.ReadUInt32();
				chunk.audioFormat = m_reader.ReadUInt16();
				chunk.channelCount = m_reader.ReadUInt16();
				chunk.sampleRate = m_reader.ReadUInt32();
				chunk.byteRate = m_reader.ReadUInt32();
				chunk.blockAlign = m_reader.ReadUInt16();
				chunk.bitsPerSample = m_reader.ReadUInt16();
				return chunk;
			}

			/// <summary>
			/// Reads the WAVE data header, without any verification.
			/// </summary>
			public WaveDataHeader ReadWaveDataHeader()
			{
				var header = new WaveDataHeader();
				header.chunkId = ReadRiffTypeName();
				header.chunkSize =  m_reader.ReadUInt32();
				return header;
			}

			/// <summary>
			/// Reads the requested amount of sound data from the WAVE file.
			/// </summary>
			public byte[] ReadWaveData(int count)
			{
				return m_reader.ReadBytes(count);
			}
			#endregion

			#region IDisposable Members

			/// <summary>
			/// Release all resources used by the <see cref="Palaso.Media.AlsaAudioDevice.WaveFileReader"/> object.
			/// </summary>
			public void Dispose()
			{
				if(m_reader != null)
					m_reader.Close();
			}

			#endregion
		}

		/// <summary>
		/// Utility class for writing a WAVE file.
		/// </summary>
		public class WaveFileWriter
		{
			BinaryWriter m_writer;

			/// <summary>
			/// Initializes a new instance of the <see cref="Palaso.Media.AlsaAudioDevice.WaveFileWriter"/> class.
			/// </summary>
			public WaveFileWriter(string filename)
			{
				m_writer = new BinaryWriter(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None));
			}

			/// <summary>
			/// Writes the WAVE file header.
			/// </summary>
			public void WriteFileHeader(int dataSize)
			{
				if (m_writer == null)
					return;
				m_writer.Write((byte)'R');
				m_writer.Write((byte)'I');
				m_writer.Write((byte)'F');
				m_writer.Write((byte)'F');
				m_writer.Write((Int32)(dataSize + 4 + 24 + 8));
				m_writer.Write((byte)'W');
				m_writer.Write((byte)'A');
				m_writer.Write((byte)'V');
				m_writer.Write((byte)'E');
			}

			/// <summary>
			/// Writes the WAVE format block.
			/// </summary>
			public void WriteFormatChunk(WaveFormatChunk format)
			{
				if (m_writer == null)
					return;
				m_writer.Write((byte)'f');
				m_writer.Write((byte)'m');
				m_writer.Write((byte)'t');
				m_writer.Write((byte)' ');
				m_writer.Write(format.chunkSize);
				m_writer.Write(format.audioFormat);
				m_writer.Write(format.channelCount);
				m_writer.Write(format.sampleRate);
				m_writer.Write(format.byteRate);
				m_writer.Write(format.blockAlign);
				m_writer.Write(format.bitsPerSample);
			}

			/// <summary>
			/// Writes the WAVE data header.
			/// </summary>
			public void WriteDataHeader(int dataSize)
			{
				if (m_writer == null)
					return;
				m_writer.Write((byte)'d');
				m_writer.Write((byte)'a');
				m_writer.Write((byte)'t');
				m_writer.Write((byte)'a');
				m_writer.Write(dataSize);
			}

			/// <summary>
			/// Writes the WAVE sound data.
			/// </summary>
			public void WriteData(byte[] data)
			{
				if (m_writer == null)
					return;
				m_writer.Write(data);
			}

			/// <summary>
			/// Close this instance.
			/// </summary>
			public void Close()
			{
				if (m_writer != null)
				{
					m_writer.Close();
					m_writer = null;
				}
			}
		}
	}
}
