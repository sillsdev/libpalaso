using System;
using System.Collections.Generic;
using System.Threading;
using NAudio.Wave;

namespace SIL.Windows.Forms.Media.Naudio
{
	internal class FileWriterThread
	{
		private Thread _thread;
		private Queue<byte[]> _data;
		protected WaveFileWriter _writer;

		private volatile bool _finished;
		private TimeSpan _recordedTimeInSeconds;

		/// ------------------------------------------------------------------------------------
		public FileWriterThread(WaveFileWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");
			_writer = writer;
			_data = new Queue<byte[]>();
			_thread = new Thread(ProcessData);
			_thread.Name = GetType().Name;
			_thread.Priority = ThreadPriority.BelowNormal;
			_thread.Start();
		}

		/// ------------------------------------------------------------------------------------
		public TimeSpan RecordedTimeInSeconds
		{
			get
			{
				if (_recordedTimeInSeconds == null)
					throw new InvalidOperationException();
				return _recordedTimeInSeconds;
			}
		}

		/// ------------------------------------------------------------------------------------
		private void ProcessData()
		{
			try
			{
				while (true)
				{
					byte[] buffer = null;
					lock (_data)
					{
						if (_data.Count > 0)
							buffer = _data.Dequeue();
					}
					if (buffer != null)
					{
						// write it to the file
						_writer.Write(buffer, 0, buffer.Length);
					}
					else
					{
						if (_finished)
							break;
						Thread.Sleep(20);
					}
				}
				_recordedTimeInSeconds = TimeSpan.FromSeconds((double)_writer.Length / _writer.WaveFormat.AverageBytesPerSecond);
			}
			finally
			{
				_writer.Dispose();
				_writer = null;
			}
		}

		/// ------------------------------------------------------------------------------------
		public void AddData(byte[] buffer, int byteCount)
		{
			if (_finished)
				throw new InvalidOperationException("Cannot add data after stopping!");
			var copy = new byte[byteCount];
			Array.Copy(buffer, copy, byteCount);
			lock (_data)
			{
				_data.Enqueue(copy);
			}
		}

		/// ------------------------------------------------------------------------------------
		public void Stop()
		{
			_finished = true;
			_thread.Join();
		}

		/// ------------------------------------------------------------------------------------
		public void Abort()
		{
			_finished = true;
			lock (_data)
			{
				_data.Clear();
			}
			_thread.Join();
		}
	}
}
