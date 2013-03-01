using System;

namespace Palaso.Media
{
	public class AudioFactory
	{
		public static ISimpleAudioSession AudioSession(string filePath)
		{
#if MONO
				return new AudioNullSession();
#else
			return new AudioIrrKlangSession(filePath);
#endif
		}
	}

}