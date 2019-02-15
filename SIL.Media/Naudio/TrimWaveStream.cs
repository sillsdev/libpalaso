using System;
using NAudio.Wave;

namespace SIL.Media.Naudio
{
	public class TrimWaveStream : WaveStream
	{
		protected WaveStream _source;
		protected long _startBytePosition;
		protected long _endBytePosition;
		protected TimeSpan _startPosition;
		protected TimeSpan _endPosition;

		public TrimWaveStream(WaveStream source)
		{
			_source = source;
			EndPosition = source.TotalTime;
		}

		public TimeSpan StartPosition
		{
			get { return _startPosition; }
			set
			{
				_startPosition = value;
				_startBytePosition = (int)(WaveFormat.AverageBytesPerSecond * _startPosition.TotalSeconds);
				_startBytePosition = _startBytePosition - (_startBytePosition % WaveFormat.BlockAlign);
				Position = 0;
			}
		}

		public TimeSpan EndPosition
		{
			get { return _endPosition; }
			set
			{
				_endPosition = value;
				_endBytePosition = (int)Math.Round(WaveFormat.AverageBytesPerSecond * _endPosition.TotalSeconds);
				_endBytePosition = _endBytePosition - (_endBytePosition % WaveFormat.BlockAlign);
			}
		}

		public override WaveFormat WaveFormat
		{
			get { return _source.WaveFormat; }
		}

		public override long Length
		{
			get { return _endBytePosition - _startBytePosition; }
		}

		public override long Position
		{
			get { return _source.Position - _startBytePosition; }
			set { _source.Position = value + _startBytePosition; }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int bytesRequired = (int)Math.Min(count, Length - Position);
			int bytesRead = 0;
			if (bytesRequired > 0)
				bytesRead = _source.Read(buffer, offset, bytesRequired);

			return bytesRead;
		}

		protected override void Dispose(bool disposing)
		{
			_source.Dispose();
			base.Dispose(disposing);
		}
	}
}
