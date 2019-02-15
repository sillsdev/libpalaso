using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SIL.CommandLineProcessing;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Progress;

namespace SIL.Media
{
	///<summary>
	/// FFmpeg is an open source media processing commandline library
	///</summary>
	public class FFmpegRunner
	{
		/// <summary>
		/// If your app knows where FFMPEG lives, you can tell us before making any calls.
		/// </summary>
		public static string FFmpegLocation;

		/// <summary>
		/// Find the path to ffmpeg, and remember it (some apps (like SayMore) call ffmpeg a lot)
		/// </summary>
		/// <returns></returns>
		static internal string LocateAndRememberFFmpeg()
		{
			if (null != FFmpegLocation) //NO! string.empty means we looked and didn't find: string.IsNullOrEmpty(s_ffmpegLocation))
				return FFmpegLocation;
			FFmpegLocation = LocateFFmpeg();
			return FFmpegLocation;
		}

		/// <summary>
		/// ffmpeg is more of a "compile it yourself" thing, and yet
		/// SIL doesn't necessarily want to be redistributing something
		/// which may violate software patents (e.g. mp3) in certain countries, so
		/// we ask users to get it themselves.
		/// See: http://www.ffmpeg.org/legal.html
		/// This tries to find where they put it.
		/// </summary>
		/// <returns>the path, if found, else null</returns>
		static private string LocateFFmpeg()
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

			if (!string.IsNullOrEmpty(withApplicationDirectory) && File.Exists(withApplicationDirectory))
				return withApplicationDirectory;

			//nb: this is sensitive to whether we are compiled against win32 or not,
			//not just the host OS, as you might guess.
			var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);


			var progFileDirs = new List<string>()
									{
										pf.Replace(" (x86)", ""),			//native (win32 or 64, depending)
										pf.Replace(" (x86)", "")+" (x86)"	//win32
									};

			/* We DON't SUPPORT THIS ONE (it lacks some information on the output, at least as of
			 * Julu 2010)
			 //from http://www.arachneweb.co.uk/software/windows/avchdview/ffmpeg.html
			foreach (var path in progFileDirs)
			{
				var exePath = (Path.Combine(path, "ffmpeg/win32-static/bin/ffmpeg.exe"));
				if(File.Exists(exePath))
					return exePath;
			}
			 */

			//http://manual.audacityteam.org/index.php?title=FAQ:Installation_and_Plug-Ins#installffmpeg
			foreach (var path in progFileDirs)
			{
				var exePath = (Path.Combine(path, "FFmpeg for Audacity/ffmpeg.exe"));
				if (File.Exists(exePath))
					return exePath;
			}
			return string.Empty;
		}

		private static string GetPathToBundledFFmpeg()
		{
			try
			{
				return FileLocationUtilities.GetFileDistributedWithApplication("ffmpeg", "ffmpeg.exe");
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}


		///<summary>
		/// Returns false if it can't find ffmpeg
		///</summary>
		static public bool HaveNecessaryComponents
		{
			get
			{
				return !string.IsNullOrEmpty(LocateFFmpeg());
			}
		}

		///<summary>
		/// Returns false if it can't find ffmpeg
		///</summary>
		static private bool HaveValidFFMpegOnPath
		{
			get
			{
				if (Platform.IsWindows)
				{
					if (!string.IsNullOrEmpty(LocateFFmpeg()))
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
		/// is resonsible for verifying with the user and deleting the file before calling this.
		/// </summary>
		/// <param name="inputPath"></param>
		/// <param name="outputPath"></param>
		/// <param name="channels">1 for mono, 2 for stereo</param>
		/// <param name="progress"></param>
		/// <returns>log of the run</returns>
		public static ExecutionResult ExtractMp3Audio(string inputPath, string outputPath, int channels, IProgress progress)
		{
			if (string.IsNullOrEmpty(LocateFFmpeg()))
			{
				return new ExecutionResult() { StandardError = "Could not locate FFMpeg" };
			}

			var arguments = string.Format("-i \"{0}\" -vn -acodec libmp3lame -ac {1} \"{2}\"", inputPath, channels, outputPath);
			var result = CommandLineRunner.Run(LocateAndRememberFFmpeg(),
														arguments,
														Environment.CurrentDirectory,
														60 * 10, //10 minutes
														progress
				);

			progress.WriteVerbose(result.StandardOutput);

			//hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small")
				&& File.Exists(outputPath))
			{
				var doctoredResult = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = string.Empty
				};
				return doctoredResult;
			}
			if (result.StandardError.ToLower().Contains("error")) //ffmpeg always outputs config info to standarderror
				progress.WriteError(result.StandardError);

			return result;
		}
		/// <summary>
		/// Extracts the audio from a video. Note, it will fail if the file exists, so the client
		/// is resonsible for verifying with the user and deleting the file before calling this.
		/// </summary>
		/// <param name="inputPath"></param>
		/// <param name="outputPath"></param>
		/// <param name="channels">1 for mono, 2 for stereo</param>
		/// <param name="progress"></param>
		/// <returns>log of the run</returns>
		public static ExecutionResult ExtractOggAudio(string inputPath, string outputPath, int channels, IProgress progress)
		{
			if (string.IsNullOrEmpty(LocateFFmpeg()))
			{
				return new ExecutionResult() { StandardError = "Could not locate FFMpeg" };
			}

			var arguments = string.Format("-i \"{0}\" -vn -acodec vorbis -ac {1} \"{2}\"", inputPath, channels, outputPath);
			progress.WriteMessage("ffmpeg " + arguments);
			var result = CommandLineRunner.Run(LocateAndRememberFFmpeg(),
														arguments,
														Environment.CurrentDirectory,
														60 * 10, //10 minutes
														progress
				);

			progress.WriteVerbose(result.StandardOutput);

			//hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small")
				&& File.Exists(outputPath))
			{
				var doctoredResult = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = string.Empty
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
			if (string.IsNullOrEmpty(LocateFFmpeg()))
			{
				return new ExecutionResult() { StandardError = "Could not locate FFMpeg" };
			}

			var sampleRateArg = "";
			if (sampleRate > 0)
				sampleRateArg = string.Format("-ar {0}", sampleRate);

			//TODO: this will output whatever mp3 or wav or whatever is in the video... might not be wav at all!
			var channelsArg = "";
			if (channels > 0)
				channelsArg = string.Format(" -ac {0}", channels);

			var arguments = string.Format("-i \"{0}\" -vn -acodec {1}  {2} {3} \"{4}\"",
				inputPath, audioCodec, sampleRateArg, channelsArg, outputPath);

			progress.WriteMessage("ffmpeg " + arguments);

			var result = CommandLineRunner.Run(LocateAndRememberFFmpeg(),
														arguments,
														Environment.CurrentDirectory,
														60 * 10, //10 minutes
														progress);

			progress.WriteVerbose(result.StandardOutput);

			//hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small")
				&& File.Exists(outputPath))
			{
				var doctoredResult = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = string.Empty
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
			if (string.IsNullOrEmpty(LocateFFmpeg()))
				return new ExecutionResult { StandardError = "Could not locate FFMpeg" };

			var arguments = string.Format("-i \"{0}\" -vn -ac {1} \"{2}\"",
				inputPath, channels, outputPath);

			var result = CommandLineRunner.Run(LocateAndRememberFFmpeg(),
							arguments,
							Environment.CurrentDirectory,
							60 * 10, //10 minutes
							progress);

			progress.WriteVerbose(result.StandardOutput);

			//hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small") && File.Exists(outputPath))
			{
				var doctoredResult = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = string.Empty
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
			if (string.IsNullOrEmpty(LocateAndRememberFFmpeg()))
			{
				return new ExecutionResult() { StandardError = "Could not locate FFMpeg" };
			}

			var arguments = "-i \"" + inputPath + "\" -acodec libmp3lame -ac 1 -ar 8000 \"" + outputPath + "\"";


			progress.WriteMessage("ffmpeg " + arguments);


			var result = CommandLineRunner.Run(LocateAndRememberFFmpeg(),
														arguments,
														Environment.CurrentDirectory,
														60 * 10, //10 minutes
														progress
				);

			progress.WriteVerbose(result.StandardOutput);


			//hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small")
				&& File.Exists(outputPath))
			{
				result = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = string.Empty
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
			if (string.IsNullOrEmpty(LocateAndRememberFFmpeg()))
			{
				return new ExecutionResult() { StandardError = "Could not locate FFMpeg" };
			}

			// isn't working: var arguments = "-i \"" + inputPath + "\" -vcodec mpeg4 -s 160x120 -b 800  -acodec libmp3lame -ar 22050 -ab 32k -ac 1 \"" + outputPath + "\"";
			var arguments = "-i \"" + inputPath +
							"\" -vcodec mpeg4 -s 160x120 -b 800 -acodec libmp3lame -ar 22050 -ab 32k -ac 1 ";
			if (maxSeconds > 0)
				arguments += " -t " + maxSeconds + " ";
			arguments += "\"" + outputPath + "\"";

			progress.WriteMessage("ffmpeg " + arguments);

			var result = CommandLineRunner.Run(LocateAndRememberFFmpeg(),
														arguments,
														Environment.CurrentDirectory,
														60 * 10, //10 minutes
														progress
				);

			progress.WriteVerbose(result.StandardOutput);


			//hide a meaningless error produced by some versions of liblame
			if (result.StandardError.Contains("lame: output buffer too small")
				&& File.Exists(outputPath))
			{
				result = new ExecutionResult
				{
					ExitCode = 0,
					StandardOutput = result.StandardOutput,
					StandardError = string.Empty
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
		public static ExecutionResult MakeLowQualitySmallPicture(string inputPath, string outputPath, IProgress progress)
		{
			if (string.IsNullOrEmpty(LocateAndRememberFFmpeg()))
			{
				return new ExecutionResult() { StandardError = "Could not locate FFMpeg" };
			}

			//enhance: how to lower the quality?

			var arguments = "-i \"" + inputPath + "\" -f image2  -s 176x144 \"" + outputPath + "\"";

			progress.WriteMessage("ffmpeg " + arguments);

			var result = CommandLineRunner.Run(LocateAndRememberFFmpeg(),
														arguments,
														Environment.CurrentDirectory,
														60 * 10, //10 minutes
														progress
				);

			progress.WriteVerbose(result.StandardOutput);
			if (result.StandardError.ToLower().Contains("error")) //ffmpeg always outputs config info to standarderror
				progress.WriteError(result.StandardError);

			return result;
		}
	}
}
