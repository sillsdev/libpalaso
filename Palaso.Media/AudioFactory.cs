using System;

namespace Palaso.Media
{
	public class AudioFactory
	{
		public static ISimpleAudioSession CreateAudioSession(string filePath)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return new AudioAlsaSession(filePath);
			}
			return new AudioIrrKlangSession(filePath);
		}

		[Obsolete("This was a unfortunate method name. Use CreateAudioSessionInstead.")]
		public static ISimpleAudioSession AudioSession(string filePath)
		{
			return CreateAudioSession(filePath);
	}
}
}
