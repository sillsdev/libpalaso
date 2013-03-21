using System;

namespace Palaso.Media
{
	public class AudioFactory
	{
		public static ISimpleAudioSession AudioSession(string filePath)
		{
#if MONO
				return new AudioGStreamerSession(filePath);
				//return new AudioAlsaSession(filePath);
#else
			return new AudioIrrKlangSession(filePath);
#endif
		}
	}
}
