using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using SIL.CommandLineProcessing;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Progress;
using static System.String;

namespace SIL.Media
{
	///<summary>
	/// FFmpeg is an open source media processing commandline library. Note that there is
	/// a nuget package called FFmpegCore that wraps the exe to provide this functionality.
	/// It has support for asynchronous processing, and with some poking around we could
	/// maybe figure out how to use that to provide progress reporting, but it's not clear
	/// that it would be worth it.
	///</summary>
	public class FFmpegRunner
	{
		private const string kFFmpegExe = "ffmpeg.exe";
		private const string mp3LameCodecArg = "-acodec libmp3lame";

		/// <summary>
		/// If your app knows where FFmpeg lives, you can tell us before making any calls.
		/// </summary>
		public static string FFmpegLocation;

		/// <summary>
		/// Find the path to FFmpeg, and remember it (some apps (like SayMore) call FFmpeg a lot)
		/// </summary>
		/// <returns></returns>
		internal static string LocateAndRememberFFmpeg()
		{
			if (null != FFmpegLocation) //NO! string.empty means we looked and didn't find: string.IsNullOrEmpty(s_ffmpegLocation))
				return FFmpegLocation;
			FFmpegLocation = LocateFFmpeg() ?? Empty;
			return FFmpegLocation;
		}

		/// <summary>
		/// FFmpeg will typically be distributed with SIL software on Windows or automatically
		/// installed via package dependencies on other platforms, but if something wants to
		/// use this library and work with a version of it that the user downloaded or compiled
		/// locally, this tries to find where they put it.
		/// </summary>
		/// <returns>the path, if found, else null</returns>
		private static string LocateFFmpeg()
		{
			if (Platform.IsLinux)
			{
				//on linux, we can safely assume the package has included the needed dependency
				if (File.Exists("/usr/bin/ffmpeg"))
					return "/usr/bin/ffmpeg";
				if (File.Exists("/usr/bin/avconv"))
					return "/usr/bin/avconv"; // the new name of ffmpeg on Linux

				return null;
			}

			string withApplicationDirectory = GetPathToBundledFFmpeg();

			if (withApplicationDirectory != null && File.Exists(withApplicationDirectory))
				return withApplicationDirectory;

			var fromChoco = MediaInfo.GetFFmpegFolderFromChocoInstall(kFFmpegExe);
			if (fromChoco != null)
				return Path.Combine(fromChoco, kFFmpegExe);

			var progFileDirs = new List<string> {
				Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
				Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
			};

			/* We DON'T SUPPORT THIS ONE (it lacks some information on the output, at least as of
			 * July 2010)
			 //from http://www.arachneweb.co.uk/software/windows/avchdview/ffmpeg.html
			foreach (var path in progFileDirs)
			{
				var exePath = (Path.Combine(path, "ffmpeg/win32-static/bin/ffmpeg.exe"));
				if(File.Exists(exePath))
					return exePath;
			}
			 */

			// REVIEW: I just followed the instructions in the current version of Audacity for
			// installing FFmpeg for Audacity and the result is a folder that does not contain
			// ffmpeg.exe. This maybe used to work, but I don't think we'll ever find ffmpeg this
			// way now.
			//http://manual.audacityteam.org/index.php?title=FAQ:Installation_and_Plug-Ins#installffmpeg
			foreach (var path in progFileDirs)
			{
				var exePath = (Path.Combine(path, "FFmpeg for Audacity", kFFmpegExe));
				if (File.Exists(exePath))
					return exePath;
			}
			return null;
		}

		private static string GetPathToBundledFFmpeg()
		{
			try
			{
				return FileLocationUtilities.GetFileDistributedWithApplication("ffmpeg", kFFmpegExe);
			}
			catch (Exception)
			{
				return null;
			}
		}

		///<summary>
		/// Returns false if it can't find ffmpeg
		///</summary>
		public static bool HaveNecessaryComponents => LocateFFmpeg() != null;

		private static ExecutionResult NoFFmpeg =>
			new ExecutionResult { StandardError = "Could not locate FFmpeg" };

		///<summary>
		/// Returns false if it can't find ffmpeg
		///</summary>
		private static bool HaveValidFFmpegOnPath
		{
			get
			{
				if (Platform.IsWindows)
				{
					if (LocateFFmpeg() != null)
						return true;
				}

				//see if there is one on the %path%

				var p = new Process();
				p.StartInfo.FileName = "ffmpeg";
				p.StartInfo.RedirectStandardError = true;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.CreateNoWindow = true;
				try
				{
					p.Start();
				}
				catch (Exception)
				{
					return false;
				}
				return true;
			}
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
		public static ExecutionResult ExtractMp3Audio(string inputPath, string outputPath, int channels, IProgress progress)
		{
			if (LocateFFmpeg() != null)
				return NoFFmpeg;

			var arguments = $"-i \"{inputPath}\" -vn {mp3LameCodecArg} -ac {channels} \"{outputPath}\"";
			var result = RunFFmpeg(arguments, progress);

			//hide a meaningless error produced by some versions of liblame
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
			if (LocateFFmpeg() != null)
				return NoFFmpeg;

			var arguments = $"-i \"{inputPath}\" -vn -acodec vorbis -ac {channels} \"{outputPath}\"";
			var result = RunFFmpeg(arguments, progress);

			//hide a meaningless error produced by some versions of liblame
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
			if (LocateFFmpeg() != null)
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

			//hide a meaningless error produced by some versions of liblame
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
			if (LocateFFmpeg() != null)
				return NoFFmpeg;

			var arguments = $"-i \"{inputPath}\" -vn -ac {channels} \"{outputPath}\"";

			var result = RunFFmpeg(arguments, progress);

			//hide a meaningless error produced by some versions of liblame
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

			var arguments = $"-i \"{inputPath}\" {mp3LameCodecArg} -ac 1 -ar 8000 \"{outputPath}\"";

			var result = RunFFmpeg(arguments, progress);

			//hide a meaningless error produced by some versions of liblame
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
				$"{mp3LameCodecArg} -ar 22050 -ab 32k -ac 1 ";
			if (maxSeconds > 0)
				arguments += $" -t {maxSeconds} ";
			arguments += $"\"{outputPath}\"";

			var result = RunFFmpeg(arguments, progress);

			//hide a meaningless error produced by some versions of liblame
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
			progress.WriteMessage("ffmpeg " + arguments);

			const int timeout = 600; // 60 * 10 = 10 minutes
			var result = CommandLineRunner.Run(LocateAndRememberFFmpeg(), arguments,
				Environment.CurrentDirectory, timeout, progress);

			progress.WriteVerbose(result.StandardOutput);

			return result;
		}
	}
}
