using System;

namespace Palaso.Media
{
	public class AudioFactory
	{
		public static ISimpleAudioSession AudioSession(string filePath)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return new AudioAlsaSession(filePath);
			}
			return new AudioIrrKlangSession(filePath);
		}
	}
}
