using System;
using System.Diagnostics;
#if MONO
using SIL.Media.AlsaAudio;
#endif

namespace SIL.Media
{
	public class AudioFactory
	{
		public static ISimpleAudioSession CreateAudioSession(string filePath, IProcessStarter processStarter = null)
		{
#if MONO
			return new AudioAlsaSession(filePath);
#else
			return new AudioIrrKlangSession(filePath, processStarter);
#endif
		}

		[Obsolete("This was a unfortunate method name. Use CreateAudioSession Instead.")]
		public static ISimpleAudioSession AudioSession(string filePath)
		{
			return CreateAudioSession(filePath);
		}

		public class ProcessStarter : IProcessStarter
		{
			public void Start(string filePath)
			{
				Process.Start(filePath);
			}
		}
	}
}
