// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

#if __MonoCS__
namespace SIL.Media.AlsaAudio
{
	/// <summary>
	/// Minimal Wave format class
	/// </summary>
	public class WaveFormat
	{
		public int SampleRate { get; set; }
		public int Channels { get; set; }
		public int BitsPerSample { get; set; }

		public WaveFormat()
		{
			SampleRate = 44100;
			Channels = 1;
			BitsPerSample = 16;
		}

		public WaveFormat(int rate, int channels)
		{
			SampleRate = rate;
			Channels = channels;
			BitsPerSample = 16;
		}
	}
}
#endif

