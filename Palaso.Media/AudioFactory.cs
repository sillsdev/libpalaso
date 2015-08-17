using System;

namespace Palaso.Media
{
	public class AudioFactory
	{
		public static ISimpleAudioSession CreateAudioSession(string filePath)
		{
#if MONO
				return new AudioAlsaSession(filePath);
#else
			return new AudioIrrKlangSession(filePath);
#endif
		}

		[Obsolete("This was a unfortunate method name. Use CreateAudioSession Instead.")]
		public static ISimpleAudioSession AudioSession(string filePath)
		{
			return CreateAudioSession(filePath);
	}
}
}
