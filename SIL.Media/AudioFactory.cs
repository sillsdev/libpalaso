using System;
using SIL.Media.AlsaAudio;
using SIL.PlatformUtilities;

namespace SIL.Media
{
	public class AudioFactory
	{
		public static ISimpleAudioSession CreateAudioSession(string filePath)
		{
			if (Platform.IsLinux)
				return new AudioAlsaSession(filePath);
			return new AudioIrrKlangSession(filePath);
		}

		[Obsolete("This was a unfortunate method name. Use CreateAudioSession Instead.")]
		public static ISimpleAudioSession AudioSession(string filePath)
		{
			return CreateAudioSession(filePath);
		}
	}
}
