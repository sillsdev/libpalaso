using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FFMpegCore;
using FFMpegCore.Exceptions;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Reporting;
using static System.Environment;
using static System.Environment.SpecialFolder;

namespace SIL.Media
{
	/// <summary>
	/// This class uses FFprobe to gather information about media streams
	/// </summary>
	public class MediaInfo
	{
		private const string kProbeExe = "ffprobe.exe";

		/// <summary>
		/// If your app knows where FFProbe lives, you can set this before making any calls.
		/// If this is explicitly set to null, the next time we need to use FFprobe, we will
		/// try to find it the usual way.
		/// </summary>
		public static string FFprobeLocation { get; set; }

		/// <summary>
		/// Find the path to ffmpeg, and remember it (some apps (like SayMore) call ffmpeg a lot)
		/// </summary>
		/// <returns></returns>
		private static string LocateAndRememberFFprobe()
		{
			if (null != FFprobeLocation) // NOTE: string.Empty means we already looked and didn't find it.
				return FFprobeLocation;
			FFprobeLocation = LocateFFprobe() ?? string.Empty;
			if (!string.IsNullOrEmpty(FFprobeLocation))
				GlobalFFOptions.Configure(new FFOptions
					{ BinaryFolder = Path.GetDirectoryName(FFprobeLocation) });
			return FFprobeLocation;
		}

		/// <summary>
		/// Although we used to require users to download FFmpeg separately because of licensing
		/// and patent concerns, now FFprobe will typically be distributed with SIL software on
		/// Windows or automatically installed via package dependencies on other platforms, but if
		/// something wants to use this library and work with a version of it that the user
		/// downloaded or compiled locally, this tries to find where they put it.
		/// </summary>
		/// <returns>the path, if found, else null</returns>
		private static string LocateFFprobe()
		{
			if (!Platform.IsWindows)
			{
				// On Linux or MacOS, we can safely assume the package has included the needed
				// dependency and will have been installed in an expected location
				return new [] {"/usr/bin/ffprobe", "/usr/bin/avprobe", "/usr/local/bin/ffprobe",
					"/usr/local/bin/avprobe"}.FirstOrDefault(File.Exists);
			}

			string withApplicationDirectory = GetPathToBundledFFprobe();

			if (withApplicationDirectory != null)
				return withApplicationDirectory;

			// The user might have installed it using choco, so let's look there.
			var chocoInstallLocation = Path.Combine(GetFolderPath(CommonApplicationData),
				"chocolatey", "lib", "ffmpeg", "tools", "ffmpeg", "bin", kProbeExe);

			if (File.Exists(chocoInstallLocation))
				return chocoInstallLocation;
			return null;
		}

		private static string GetPathToBundledFFprobe()
		{
			try
			{
				return FileLocationUtilities.GetFileDistributedWithApplication(true, "ffmpeg", kProbeExe) ??
					FileLocationUtilities.GetFileDistributedWithApplication(true, "ffprobe", kProbeExe);
			}
			catch (Exception e)
			{
				Logger.WriteError(e);
				return null;
			}
		}

		///<summary>
		/// Returns false if it can't find FFprobe
		///</summary>
		public static bool HaveNecessaryComponents => !string.IsNullOrEmpty(LocateAndRememberFFprobe());

		public IMediaAnalysis AnalysisData { get; }

		private MediaInfo(IMediaAnalysis mediaAnalysis)
		{
			AnalysisData = mediaAnalysis;
			if (mediaAnalysis != null)
			{
				if (mediaAnalysis.PrimaryAudioStream != null)
					Audio = new AudioInfo(mediaAnalysis);
				if (mediaAnalysis.PrimaryVideoStream != null)
					Video = new VideoInfo(mediaAnalysis);
			}
		}

		/// <summary>
		/// Gets the media information for the requested media file. If media information
		/// the error is logged and an object is returned with null Audio and Video.
		/// </summary>
		public static MediaInfo GetInfo(string path)
		{
			var info = GetInfo(path, out var e);
			if (e != null)
			{
				Logger.WriteError(e);
				return new MediaInfo(null);
			}

			return info;
		}

		/// <summary>
		/// Gets the media information for the requested media file
		/// </summary>
		/// <param name="path">Path of the media file</param>
		/// <param name="error">If this method returns null, this will be set to indicate the error
		/// that occurred. If FFprobe could not be found to do the analysis, this will be an
		/// <see cref="ApplicationException"/></param>; otherwise, it will probably be an
		/// <see cref="FFMpegException"/>.
		/// <returns>A new MediaInfo object, or null if media information could not be retrieved.</returns>
		public static MediaInfo GetInfo(string path, out Exception error)
		{
			if (!HaveNecessaryComponents)
			{
				error = new ApplicationException("Could not locate FFprobe");
				return null;
			}

			try
			{
				error = null;
				return new MediaInfo(FFProbe.Analyse(path));
			}
			catch (Exception e)
			{
				error = e;
				return null;
			}
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
			internal AudioInfo(IMediaAnalysis mediaAnalysis)
			{
				mediaAnalysis.Format.
				AudioStreams = mediaAnalysis.AudioStreams;
				var primaryAudio = mediaAnalysis.PrimaryAudioStream;
				Debug.Assert(primaryAudio != null);
				Duration = primaryAudio.Duration;
				Encoding = primaryAudio.CodecName;
				ChannelCount = primaryAudio.Channels;
				BitDepth = primaryAudio.SampleRateHz
				ExtractBitDepth(ffmpegOutput);
				ExtractSamplesPerSecond(ffmpegOutput);
				ExtractEncoding(ffmpegOutput);
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

				var match = Regex.Match(ffmpegOutput, @"pcm_s(\d+)");
				if (match.Groups.Count == 2)
				{
					var value = match.Groups[1].Value;

					//todo: find out if we will really get s16, s24, s96, etc.
					int depth;
					if (int.TryParse(value, out depth))
					{
						BitDepth = depth;
						return;
					}
				}
				//no pcm found, use the mysterious "s24, s32" stuff, which may be misleading

				 match = Regex.Match(ffmpegOutput, @", s(\d+),");
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

			private IReadOnlyList<AudioStream> AudioStreams { get; }
			public TimeSpan Duration { get; }
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
				// DG 201303: ffmpeg gives more info like mpeg4 (Simple Profile)
				// revert match to "Video: ([^,]*)" if that would be useful
				var match = Regex.Match(ffmpegOutput, "Video: ([^,^(]*)");
				if (match.Groups.Count == 2)
				{
					Encoding = match.Groups[1].Value.Trim();
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
