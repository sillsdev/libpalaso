using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Palaso.CommandLineProcessing;
using Palaso.Extensions;
using Palaso.IO;

namespace Palaso.Media
{
	///<summary>
	/// FFmpeg is an open source media processing commandline library
	///</summary>
	public class FFmpegRunner
	{
		private static string _sFFmpegLocation;

		/// <summary>
		/// Find the path to ffmpeg, and remember it (some apps (like SayMore) call ffmpeg a lot)
		/// </summary>
		/// <returns></returns>
		static internal string LocateAndRememberFFmpeg()
		{
			if (string.Empty == _sFFmpegLocation) //NO! string.empty means we looked and didn't find: string.IsNullOrEmpty(s_ffmpegLocation))
				return _sFFmpegLocation;
			_sFFmpegLocation = LocateFFmpeg();
			return _sFFmpegLocation;
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
			//on linux, we can safely assume the package has included the needed dependency
#if MONO
						return "ffmpeg";
#endif

#if !MONO
			string withApplicationDirectory = GetPathToBundledFFmpeg();

			if (File.Exists(withApplicationDirectory))
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
#endif
		}

		private static string GetPathToBundledFFmpeg()
		{
			return FileLocator.DirectoryOfApplicationOrSolution.CombineForPath("ffmpeg", "ffmpeg.exe");
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
#if !MONO
				if (!string.IsNullOrEmpty(LocateFFmpeg()))
					return true;
#endif

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
		/// <param name="progress"></param>
		/// <returns>log of the run</returns>
		public static ExecutionResult ExtractMp3Audio(string inputPath, string outputPath, IProgress progress)
		{
			if(string.IsNullOrEmpty(LocateFFmpeg()))
			{
				return new ExecutionResult(){StandardError = "Could not locate FFMpeg"};
			}

			var arguments = "-i \"" + inputPath + "\" -vn -acodec libmp3lame \"" + outputPath + "\"";
			var result = CommandLineProcessing.CommandLineRunner.Run(LocateAndRememberFFmpeg(),
														arguments,
														Environment.CurrentDirectory,
														60*10, //10 minutes
														progress
				);

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
			return result;
		}
	}
}
