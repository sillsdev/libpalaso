using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
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
			RedirectIrrKlangAssembly();
			return CreateIrrKlangSession(filePath);
		}

		private static ISimpleAudioSession CreateIrrKlangSession(string filePath)
		{
			return new WindowsAudioSession(filePath);
		}

		[Obsolete("This was a unfortunate method name. Use CreateAudioSession Instead.")]
		public static ISimpleAudioSession AudioSession(string filePath)
		{
			return CreateAudioSession(filePath);
		}

		///<summary>Adds an AssemblyResolve handler to redirect all attempts to load the
		/// irrKlang.Net.dll to the architecture specific subdirectory.</summary>
		private static void RedirectIrrKlangAssembly()
		{
			// https://blog.slaks.net/2013-12-25/redirecting-assembly-loads-at-runtime/
			Debug.Assert(Platform.IsWindows);

			ResolveEventHandler handler = null;

			handler = (sender, args) => {
				const string irrklangNet4Dll = "irrKlang.NET4";

				var requestedAssembly = new AssemblyName(args.Name);
				if (requestedAssembly.Name != irrklangNet4Dll)
					return null;

				Debug.WriteLine($"Redirecting assembly load of {args.Name}, " +
					$"loaded by {(args.RequestingAssembly == null ? "(unknown)" : args.RequestingAssembly.FullName)}");

				var assemblyLocation = Assembly.GetExecutingAssembly().CodeBase;
				if (!string.IsNullOrEmpty(assemblyLocation))
				{
					var uri = new Uri(assemblyLocation);
					assemblyLocation = uri.LocalPath;
				}
				else
					assemblyLocation = Assembly.GetExecutingAssembly().Location;

				var directory = Path.GetDirectoryName(assemblyLocation);
				if (string.IsNullOrEmpty(directory))
					directory = Directory.GetCurrentDirectory();
				requestedAssembly.CodeBase = Path.Combine(directory, "lib",
					$"win-{Platform.ProcessArchitecture}", irrklangNet4Dll + ".dll");

				AppDomain.CurrentDomain.AssemblyResolve -= handler;

				return Assembly.Load(requestedAssembly);
			};
			AppDomain.CurrentDomain.AssemblyResolve += handler;
		}
	}
}
