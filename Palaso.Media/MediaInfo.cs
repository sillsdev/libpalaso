using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Palaso.Extensions;
using Palaso.IO;


namespace Palaso.Media
{
	/// <summary>
	/// This class uses ffmpeg to gather information about media streams
	/// </summary>
	public class MediaInfo
	{
		/// <summary>
		/// The actual ffmpeg output
		/// </summary>
		public string RawData;

		private  MediaInfo(string ffmpegOutput)
		{
			RawData = ffmpegOutput;
			Audio = new AudioInfo(ffmpegOutput);
			if (ffmpegOutput.Contains("Video:"))
			{
				Video = new VideoInfo(ffmpegOutput);
			}

		}

		private static string _sFFmpegLocation;

		/// <summary>
		/// Find the path to ffmpeg, and remember it (some apps (like SayMore) call ffmpeg a lot)
		/// </summary>
		/// <returns></returns>
		static private string LocateAndRememberFFmpeg()
		{
			if(string.Empty==_sFFmpegLocation) //NO! string.empty means we looked and didn't find: string.IsNullOrEmpty(s_ffmpegLocation))
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
				if(File.Exists(exePath))
					return exePath;
			}
			return string.Empty;
#endif
		}

		private static string GetPathToBundledFFmpeg()
		{
			return FileLocator.DirectoryOfApplicationOrSolution.CombineForPath("ffmpeg","ffmpeg.exe");
		}

		///<summary>
		/// Returns false if it can't find ffmpeg
		///</summary>
		static public bool HaveNecessaryComponents
		{
			get
			{
				#if !MONO
				if(!string.IsNullOrEmpty(LocateFFmpeg()))
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

		static public MediaInfo GetInfo(string path)
		{
			var p = new Process();
			p.StartInfo.FileName = LocateAndRememberFFmpeg();
			p.StartInfo.Arguments = ( "-i " + "\""+path+"\"");
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;
			p.Start();

			string ffmpegOutput = p.StandardError.ReadToEnd();
			if(!p.WaitForExit(3000))
			{
				try
				{
					p.Kill();
				}
				catch(Exception)
				{ //couldn't stop it, oh well.
				}
				throw new ApplicationException("FFMpeg seems to have hung");
			}
			return new MediaInfo(ffmpegOutput);
		}

		public AudioInfo Audio { get; private set;}
		public VideoInfo Video { get; private set;}
		//future public ImageInfo Image { get; private set;}

		/// <summary>
		/// used by both audio and video, so it lives up here
		/// </summary>
		internal static TimeSpan GetDuration(string ffmpegOutput)
		{
				var match = Regex.Match(ffmpegOutput, "Duration: ([^,]*),");
				if (match.Groups.Count == 2)
				{
					var durationString = match.Groups[1].Value;

					TimeSpan durationTime;
					if (TimeSpan.TryParse(durationString, out durationTime))
					{
						return durationTime;
					}
				}
				return default(TimeSpan);
		}


		public class AudioInfo
		{

			internal AudioInfo(string ffmpegOutput)
			{
				ExtractDuration(ffmpegOutput);
				ExtractBitDepth(ffmpegOutput);
				ExtractSamplesPerSecond(ffmpegOutput);
				ExtractChannels(ffmpegOutput);
				ExtractEncoding(ffmpegOutput);
			}

			private void ExtractEncoding(string ffmpegOutput)
			{

				var match = Regex.Match(ffmpegOutput, "Audio: ([^,]*)");
				if (match.Groups.Count == 2)
				{
					Encoding = match.Groups[1].Value;
				}
			}

			private void ExtractChannels(string ffmpegOutput)
			{
				var match = Regex.Match(ffmpegOutput, ", ([^,]*) channels");
				if (match.Groups.Count == 2)
				{
					var value = match.Groups[1].Value;

					int channelCount;

					if (int.TryParse(value, out channelCount))
					{
						this.ChannelCount = channelCount;
					}
				}
			}

			private void ExtractSamplesPerSecond(string ffmpegOutput)
			{
				var match = Regex.Match(ffmpegOutput, ", ([^,]*) Hz");
				if (match.Groups.Count == 2)
				{
					var value = match.Groups[1].Value;

					int frequency;

					if (int.TryParse(value, out frequency))
					{
						this.SamplesPerSecond = frequency;
					}
				}
			}

			private void ExtractBitDepth(string ffmpegOutput)
			{
				var match = Regex.Match(ffmpegOutput, @", s(\d+),");
				if (match.Groups.Count == 2)
				{
					var value = match.Groups[1].Value;

					//todo: find out if we will really get s16, s24, s96, etc.
					int depth;
					if (int.TryParse(value, out depth))
					{
						BitDepth = depth;
					}
				}
			}

			private void ExtractDuration(string ffmpegOutput)
			{
				Duration = MediaInfo.GetDuration(ffmpegOutput);
			}



			public TimeSpan Duration { get; private set;}
			public int ChannelCount { get; private set;}
			public int SamplesPerSecond { get; private set;}
			public int BitDepth { get; private set;}
			public string Encoding { get; private set;}
		}


		public class VideoInfo
		{
			internal VideoInfo(string ffmpegOutput)
			{
				ExtractDuration(ffmpegOutput);
				ExtractEncoding(ffmpegOutput);
				ExtractFramesPerSecond(ffmpegOutput);
				ExtractResolution(ffmpegOutput);
			}

			private void ExtractResolution(string ffmpegOutput)
			{
				//e.g. , 176x144 [PAR 12:11 DAR 4:3],
				//e.g. p, 320x240, 1
				var match = Regex.Match(ffmpegOutput, @" (\d+x\d+)[, ]");//sometimes followed by space, sometimes comma
				if (match.Groups.Count == 2)
				{
					Resolution = match.Groups[1].Value;
				}
			}

			private void ExtractFramesPerSecond(string ffmpegOutput)
			{
				var match = Regex.Match(ffmpegOutput, ", ([^,]*) fps");
				if (match.Groups.Count == 2)
				{
					int fps;
					if (int.TryParse(match.Groups[1].Value, out fps))
					{
						FramesPerSecond = fps;
					}
				}
			}

			private void ExtractEncoding(string ffmpegOutput)
			{
				var match = Regex.Match(ffmpegOutput, "Video: ([^,]*)");
				if (match.Groups.Count == 2)
				{
					Encoding = match.Groups[1].Value;
				}
			}

			private void ExtractDuration(string ffmpegOutput)
			{
				Duration = MediaInfo.GetDuration(ffmpegOutput);
			}

			public TimeSpan Duration { get; private set;}
			public int FramesPerSecond { get; private set;}
			public string Resolution { get; private set;}
			public string Encoding { get; private set;}
		}
		/* future
		public class ImageInfo
		{
			internal  ImageInfo()
			{
			}
			public int HorizontalResolution { get; private set;}
			public int VerticalResolution { get; private set;}
		}
		 */
	}
}
