using System;
#if MONO
using SIL.Media.AlsaAudio;
#endif

namespace SIL.Media
{
	public class AudioFactory
	{
		public static ISimpleAudioSession CreateAudioSession(string filePath)
		{
#if MONO
			//return new AudioGStreamerSession(filePath);
			return new AudioAlsaSession(filePath);
#else
			return new AudioIrrKlangSession(filePath);
#endif
		}

		[Obsolete("This was a unfortunate method name. Use CreateAudioSessionInstead.")]
		public static ISimpleAudioSession AudioSession(string filePath)
		{
			return CreateAudioSession(filePath);
	}
}
}
