using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SIL.CommandLineProcessing;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Progress;
using static System.Environment;
using static System.IO.Path;
using static System.String;
using Version = System.Version;

namespace SIL.Media
{
	///<summary>
	/// FFmpeg is an open source media processing commandline library. Note that there is
	/// a nuget package called FFmpegCore that wraps the exe to provide this functionality.
	/// It has support for asynchronous processing, and with some poking around we could
	/// maybe figure out how to use that to provide progress reporting, but it's not clear
	/// that it would be worth it.
	///</summary>
	public static class FFmpegRunner
	{
		internal const string kLinuxBinFolder = "/usr/bin";
		internal const string kFFmpeg = "ffmpeg";
		private const string kFFmpegExe = "ffmpeg.exe";
		private const string kMp3LameCodecArg = "-acodec libmp3lame";

		/// <summary>
		/// If your app knows where FFmpeg lives, set this before making any calls.
		/// Unless the exe is on the system path, this is the full path to the executable,
		/// including the executable file itself.
		/// </summary>
		public static string FFmpegLocation;
		private static bool s_locationSetByClient = true;

		/// <summary>
		/// If your app has a known minimum version of FFMpeg tools that it will work with, you can
		/// set this to prevent this library from attempting to use a version that is not going to
		/// meet your requirements. This will be ignored if you set FFmpegLocation, if the ffmpeg
		/// installation is based on a Linux package dependency, or if FFmpeg is colocated with the
		/// applications, since it seems safe to assume that you would not specify or install a
		/// version that does not satisfy your needs. Note that this also applies to FFprobe (used
		/// in MediaInfo).
		/// </summary>
		public static Version MinimumVersion
		{
			get => s_minimumVersion;
			set
			{
				s_minimumVersion = value;
				if (!s_locationSetByClient)
					FFmpegLocation = null;
			}
		}
		private static Version s_minimumVersion;

		/// <summary>
		/// Find the path to FFmpeg, and remember it (some apps (like SayMore) call FFmpeg a lot)
		/// </summary>
		/// <returns></returns>
		internal static string LocateAndRememberFFmpeg()
		{
			// Do not change this to IsNullOrEmpty(FFmpegLocation) because Empty means we already
			// looked and didn't find it.
			if (null != FFmpegLocation)
				return FFmpegLocation;

			s_locationSetByClient = false;

			FFmpegLocation = LocateFFmpeg();
			return FFmpegLocation;

			// This returns the exe path if found; otherwise Empty.
			static string LocateFFmpeg()
			{
				if (Platform.IsLinux)
				{
					// On Linux, we can assume the package has included the needed dependency.
					var convExePath = Combine(kLinuxBinFolder, kFFmpeg);
					if (File.Exists(convExePath))
						return convExePath;
					// Try avconv, the new name of ffmpeg on Linux
					convExePath = Combine(kLinuxBinFolder, "avconv");
					return File.Exists(convExePath) ? convExePath : Empty;
				}

				// On Windows FFmpeg will typically be distributed with the SIL software that
				// accompanies the SIL.Media DLL.
				var withApplicationDirectory = GetPathToBundledFFmpeg();
				if (withApplicationDirectory != null && File.Exists(withApplicationDirectory))
					return withApplicationDirectory;

				// Failing that, if a program wants to use this library and work with a version of
				// it that the user downloaded or compiled locally, this logic tries to find it.
				var fromChoco = GetFFmpegFolderFromChocoInstall(kFFmpegExe);
				if (fromChoco != null)
				{
					var pathToFFmpeg = Combine(fromChoco, kFFmpegExe);
					return pathToFFmpeg;
				}

				// Try to just run ffmpeg from the path, if it works then we can use that directly.
				if (MeetsMinimumVersionRequirement(kFFmpeg))
					return kFFmpeg;

				// REVIEW: I just followed the instructions in the current version of Audacity for
				// installing FFmpeg for Audacity and the result is a folder that does not contain
				// ffmpeg.exe. This maybe used to work, but I don't think we'll ever find ffmpeg this
				// way now.
				// https://support.audacityteam.org/basics/installing-ffmpeg
				return new[] {
						GetFolderPath(SpecialFolder.ProgramFiles),
						GetFolderPath(SpecialFolder.ProgramFilesX86) }
					.Select(path => Combine(path, "FFmpeg for Audacity", kFFmpegExe))
					.FirstOrDefault(exePath => File.Exists(exePath) &&
						MeetsMinimumVersionRequirement(exePath)) ?? Empty;
			}
		}

		internal static bool MeetsMinimumVersionRequirement(string exe)
		{
			try
			{
				var version = new Regex(GetFileNameWithoutExtension(exe).ToLowerInvariant() +
					@" version (?<version>\d+\.\d+(\.\d+)?)");
				var results = CommandLineRunner.Run(exe, "-version", ".", 5, new NullProgress());
				var match = version.Match(results.StandardOutput);
				if (!match.Success)
					return false;
				if (MinimumVersion == null)
					return true;
				var actualVersion = Version.Parse(match.Groups["version"].Value);
				actualVersion = new Version(actualVersion.Major, actualVersion.Minor,
					actualVersion.Build >= 0 ? actualVersion.Build : 0,
					actualVersion.Revision >= 0 ? actualVersion.Revision : 0);
				return actualVersion >= MinimumVersion;
			}
			catch
			{
				return false;
			}
		}

		internal static string GetFFmpegFolderFromChocoInstall(string exeNeeded)
		{
			try
			{
				var programData = GetFolderPath(SpecialFolder
					.CommonApplicationData);

				var folder = Combine(programData, "chocolatey", "lib", kFFmpeg, "tools",
					kFFmpeg, "bin");
				var pathToExe = Combine(folder, exeNeeded);
				if (!File.Exists(pathToExe)|| !MeetsMinimumVersionRequirement(pathToExe))
					folder = null;
				return folder;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}

		private static string GetPathToBundledFFmpeg()
		{
			try
			{
				return FileLocationUtilities.GetFileDistributedWithApplication(kFFmpeg, kFFmpegExe);
			}
			catch (Exception)
			{
				return null;
			}
		}

		///<summary>
		/// Returns false if it can't find ffmpeg
		///</summary>
		public static bool HaveNecessaryComponents => LocateAndRememberFFmpeg() != Empty;

		private static ExecutionResult NoFFmpeg =>
			new ExecutionResult { StandardError = "Could not locate FFmpeg" };

		/// <summary>
		/// Extracts the audio from a video. Note, it will fail if the file exists, so the client
		/// is responsible for verifying with the user and deleting the file before calling this.
		/// </summary>
		/// <param name="inputPath"></param>
		/// <param name="outputPath"></param>
		/// <param name="channels">1 for mono, 2 for stereo</param>
		/// <param name="progress"></param>
		/// <returns>log of the run</returns>
		public static ExecutionResult ExtractMp3Audio(string inputPath, string outputPath, int channels, IProgress progress)
		{
			if (LocateAndRememberFFmpeg() == null)
				return NoFFmpeg;

			var arguments = $"-i \"{inputPath}\" -vn {kMp3LameCodecArg} -ac {channels} \"{outputPath}\"";
			var result = RunFFmpeg(arguments, progress);

			// Hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small")
				&& File.Exists(outputPath))
			{
				var doctoredResult = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = Empty
				};
				return doctoredResult;
			}
			if (result.StandardError.ToLower().Contains("error")) //ffmpeg always outputs config info to standarderror
				progress.WriteError(result.StandardError);

			return result;
		}

		/// <summary>
		/// Extracts the audio from a video. Note, it will fail if the file exists, so the client
		/// is responsible for verifying with the user and deleting the file before calling this.
		/// </summary>
		/// <param name="inputPath"></param>
		/// <param name="outputPath"></param>
		/// <param name="channels">1 for mono, 2 for stereo</param>
		/// <param name="progress"></param>
		/// <returns>log of the run</returns>
		public static ExecutionResult ExtractOggAudio(string inputPath, string outputPath, int channels, IProgress progress)
		{
			if (LocateAndRememberFFmpeg() == null)
				return NoFFmpeg;

			var arguments = $"-i \"{inputPath}\" -vn -acodec vorbis -ac {channels} \"{outputPath}\"";
			var result = RunFFmpeg(arguments, progress);

			// Hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small")
				&& File.Exists(outputPath))
			{
				var doctoredResult = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = Empty
				};
				return doctoredResult;
			}
			if (result.StandardError.ToLower().Contains("error")) //ffmpeg always outputs config info to standarderror
				progress.WriteError(result.StandardError);

			return result;
		}

		/// <summary>
		/// Extracts the audio from a video. Note, it will fail if the file exists, so the client
		/// is responsible for verifying with the user and deleting the file before calling this.
		/// </summary>
		/// <param name="inputPath"></param>
		/// <param name="outputPath"></param>
		/// <param name="channels">0 for same, 1 for mono, 2 for stereo</param>
		/// <param name="progress"></param>
		/// <returns>log of the run</returns>
		[PublicAPI]
		public static ExecutionResult ExtractBestQualityWavAudio(string inputPath, string outputPath, int channels, IProgress progress)
		{
			return ExtractAudio(inputPath, outputPath, "copy", 0, channels, progress);
		}

		/// <summary>
		/// Extracts the audio from a video. Note, it will fail if the file exists, so the client
		/// is responsible for verifying with the user and deleting the file before calling this.
		/// </summary>
		/// <param name="inputPath"></param>
		/// <param name="outputPath"></param>
		/// <param name="bitsPerSample">e.g. 8, 16, 24, 32</param>
		/// <param name="sampleRate">e.g. 22050, 44100, 4800</param>
		/// <param name="channels">0 for same, 1 for mono, 2 for stereo</param>
		/// <param name="progress"></param>
		/// <returns>log of the run</returns>
		[PublicAPI]
		public static ExecutionResult ExtractPcmAudio(string inputPath, string outputPath,
			int bitsPerSample, int sampleRate, int channels, IProgress progress)
		{
			var audioCodec = "copy";

			switch (bitsPerSample)
			{
				case 8: audioCodec = "pcm_s8"; break;
				case 16: audioCodec = "pcm_s16le"; break;
				case 24: audioCodec = "pcm_s24le"; break;
				case 32: audioCodec = "pcm_s32le"; break;
			}

			return ExtractAudio(inputPath, outputPath, audioCodec, sampleRate, channels, progress);
		}

		/// <summary>
		/// Extracts the audio from a video. Note, it will fail if the file exists, so the client
		/// is responsible for verifying with the user and deleting the file before calling this.
		/// </summary>
		/// <param name="inputPath"></param>
		/// <param name="outputPath"></param>
		/// <param name="audioCodec">e.g. copy, pcm_s16le, pcm_s32le, etc.</param>
		/// <param name="sampleRate">e.g. 22050, 44100, 4800. Use 0 to use ffmpeg's default</param>
		/// <param name="channels">0 for same, 1 for mono, 2 for stereo</param>
		/// <param name="progress"></param>
		/// <returns>log of the run</returns>
		private static ExecutionResult ExtractAudio(string inputPath, string outputPath,
			string audioCodec, int sampleRate, int channels, IProgress progress)
		{
			if (LocateAndRememberFFmpeg() == null)
				return NoFFmpeg;

			var sampleRateArg = "";
			if (sampleRate > 0)
				sampleRateArg = $"-ar {sampleRate}";

			//TODO: this will output whatever mp3 or wav or whatever is in the video... might not be wav at all!
			var channelsArg = "";
			if (channels > 0)
				channelsArg = $" -ac {channels}";

			var arguments = $"-i \"{inputPath}\" -vn -acodec {audioCodec} {sampleRateArg} {channelsArg} \"{outputPath}\"";

			var result = RunFFmpeg(arguments, progress);

			// Hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small")
				&& File.Exists(outputPath))
			{
				var doctoredResult = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = Empty
				};
				return doctoredResult;
			}
			if (result.StandardError.ToLower().Contains("error")) //ffmpeg always outputs config info to standarderror
				progress.WriteError(result.StandardError);

			return result;
		}

		/// <summary>
		/// Creates an audio file, using the received one as the bases, with the specified number
		/// of channels. For example, this can be used to convert a 2-channel audio file to a
		/// single channel audio file.
		/// </summary>
		/// <returns>log of the run</returns>
		public static ExecutionResult ChangeNumberOfAudioChannels(string inputPath,
			string outputPath, int channels, IProgress progress)
		{
			if (LocateAndRememberFFmpeg() == null)
				return NoFFmpeg;

			var arguments = $"-i \"{inputPath}\" -vn -ac {channels} \"{outputPath}\"";

			var result = RunFFmpeg(arguments, progress);

			// Hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small") && File.Exists(outputPath))
			{
				var doctoredResult = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = Empty
				};

				return doctoredResult;
			}

			// ffmpeg always outputs config info to standarderror
			if (result.StandardError.ToLower().Contains("error"))
				progress.WriteError(result.StandardError);

			return result;
		}

		/// <summary>
		/// Converts to low-quality, mono mp3
		/// </summary>
		/// <returns>log of the run</returns>
		public static ExecutionResult MakeLowQualityCompressedAudio(string inputPath, string outputPath, IProgress progress)
		{
			if (IsNullOrEmpty(LocateAndRememberFFmpeg()))
				return NoFFmpeg;

			var arguments = $"-i \"{inputPath}\" {kMp3LameCodecArg} -ac 1 -ar 8000 \"{outputPath}\"";

			var result = RunFFmpeg(arguments, progress);

			// Hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small")
				&& File.Exists(outputPath))
			{
				result = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = Empty
				};
			}
			if (result.StandardError.ToLower().Contains("error")
				|| result.StandardError.ToLower().Contains("unable to")
				|| result.StandardError.ToLower().Contains("invalid")
				|| result.StandardError.ToLower().Contains("could not")
				) //ffmpeg always outputs config info to standarderror
				progress.WriteError(result.StandardError);

			return result;
		}

		/// <summary>
		/// Converts to low-quality, small video
		/// </summary>
		/// <param name="inputPath">file location for original file</param>
		/// <param name="outputPath">file location for converted file</param>
		/// <param name="maxSeconds">0 if you don't want to truncate at all</param>
		/// <param name="progress">progress bar</param>
		/// <returns>log of the run</returns>
		public static ExecutionResult MakeLowQualitySmallVideo(string inputPath, string outputPath, int maxSeconds, IProgress progress)
		{
			if (IsNullOrEmpty(LocateAndRememberFFmpeg()))
				return NoFFmpeg;

			// isn't working: var arguments = "-i \"" + inputPath + "\" -vcodec mpeg4 -s 160x120 -b 800  -acodec libmp3lame -ar 22050 -ab 32k -ac 1 \"" + outputPath + "\"";
			var arguments = $"-i \"{inputPath}\" -vcodec mpeg4 -s 160x120 -b 800 " +
				$"{kMp3LameCodecArg} -ar 22050 -ab 32k -ac 1 ";
			if (maxSeconds > 0)
				arguments += $" -t {maxSeconds} ";
			arguments += $"\"{outputPath}\"";

			var result = RunFFmpeg(arguments, progress);

			// Hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small")
				&& File.Exists(outputPath))
			{
				result = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = Empty
				};
			}
			if (result.StandardError.ToLower().Contains("error") //ffmpeg always outputs config info to standarderror
				|| result.StandardError.ToLower().Contains("unable to")
				|| result.StandardError.ToLower().Contains("invalid")
				|| result.StandardError.ToLower().Contains("could not"))
				progress.WriteWarning(result.StandardError);

			return result;
		}

		/// <summary>
		/// Converts to low-quality, small picture
		/// </summary>
		/// <returns>log of the run</returns>
		[PublicAPI]
		public static ExecutionResult MakeLowQualitySmallPicture(string inputPath, string outputPath, IProgress progress)
		{
			if (IsNullOrEmpty(LocateAndRememberFFmpeg()))
				return NoFFmpeg;

			// ENHANCE: how to lower the quality?

			var arguments = $"-i \"{inputPath}\" -f image2  -s 176x144 \"{outputPath}\"";

			var result = RunFFmpeg(arguments, progress);

			if (result.StandardError.ToLower().Contains("error")) //ffmpeg always outputs config info to standarderror
				progress.WriteError(result.StandardError);

			return result;
		}

		private static ExecutionResult RunFFmpeg(string arguments, IProgress progress)
		{
			progress.WriteMessage($"{GetFileNameWithoutExtension(LocateAndRememberFFmpeg())} {arguments}");

			const int timeout = 600; // 60 * 10 = 10 minutes
			var result = CommandLineRunner.Run(LocateAndRememberFFmpeg(), arguments,
				CurrentDirectory, timeout, progress);

			progress.WriteVerbose(result.StandardOutput);

			return result;
		}
	}
}
