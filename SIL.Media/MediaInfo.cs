using System;
using System.Diagnostics;
using System.IO;
using FFMpegCore;
using FFMpegCore.Exceptions;
using JetBrains.Annotations;
using SIL.PlatformUtilities;
using SIL.Reporting;
using static System.String;
using static SIL.IO.FileLocationUtilities;

namespace SIL.Media
{
	/// <summary>
	/// This class uses FFprobe (via FFMpegCore) to gather information about media streams.
	/// </summary>
	public class MediaInfo
	{
		private static string FFprobeExe => Platform.IsWindows ? "ffprobe.exe" : "ffprobe";

		private static string s_ffProbeFolder;
		
		/// <summary>
		/// Returns false if it can't find FFprobe
		/// </summary>
		public static bool HaveNecessaryComponents => !IsNullOrEmpty(LocateAndRememberFFprobe());

		/// <summary>
		/// The folder where FFprobe should be found. An application can use this to set
		/// the location before making any calls.
		/// </summary>
		/// <exception cref="DirectoryNotFoundException">Folder does not exist</exception>
		/// <exception cref="FileNotFoundException">Folder does not contain the ffprobe
		/// executable</exception>
		/// <remarks>If set to <see cref="Empty"/>, indicates to this library that
		/// FFprobe is not installed, and therefore methods to retrieve media info will
		/// fail unconditionally (i.e. they will throw an exception)</remarks>
		[PublicAPI]
		public static string FFprobeFolder
		{
			get => s_ffProbeFolder;
			set
			{
				if (value != Empty)
				{
					if (!Directory.Exists(value))
						throw new DirectoryNotFoundException("Directory not found: " + value);

					if (!File.Exists(Path.Combine(value, FFprobeExe)))
					{
						throw new FileNotFoundException(
							"Path is not a folder containing FFprobe.exe: " + value);
					}
				}

				s_ffProbeFolder = value;
			}
		}

		/// <summary>
		/// Find the path to FFprobe, and remember it.
		/// </summary>
		/// <returns></returns>
		private static string LocateAndRememberFFprobe()
		{
			if (null != FFprobeFolder)
				return FFprobeFolder;
			try
			{
				FFprobeFolder = GetPresumedFFprobeFolder();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
			return FFprobeFolder;
		}

		/// <summary>
		/// On Windows, FFprobe will typically be distributed with SIL software, but if something
		/// wants to use this library and work with a version of it that the user downloaded or
		/// compiled locally, this tries to find where they put it. On other platforms, ffprobe
		/// should be installed in the standard location by the package manager.
		/// </summary>
		/// <returns>A folder where FFprobe can be found; else <see cref="Empty"/></returns>
		private static string GetPresumedFFprobeFolder()
		{
			var folder = GlobalFFOptions.Current.BinaryFolder;

			// On Linux, we assume the package has included the needed dependency.
			// ENHANCE: Make it possible to find in a non-default location
			if (!Platform.IsWindows)
			{
				if (IsNullOrEmpty(folder) && File.Exists(Path.Combine("/usr/bin", FFprobeExe)))
					folder = "/usr/bin";
			}
			else
			{
				if (IsNullOrEmpty(folder) || !File.Exists(Path.Combine(folder, FFprobeExe)))
				{
					var withApplicationDirectory =
						GetFileDistributedWithApplication(true, "ffmpeg", FFprobeExe) ??
						GetFileDistributedWithApplication(true, "ffprobe", FFprobeExe);

					folder = !IsNullOrEmpty(withApplicationDirectory) ?
						Path.GetDirectoryName(withApplicationDirectory) :
						GetFFmpegFolderFromChocoInstall(FFprobeExe);

					if (folder != null)
					{
						GlobalFFOptions.Configure(new FFOptions { BinaryFolder = folder });
					}
					else
						folder = Empty;
				}
			}

			return folder;
		}

		internal static string GetFFmpegFolderFromChocoInstall(string exeNeeded)
		{
			try
			{
				var programData = Environment.GetFolderPath(Environment.SpecialFolder
					.CommonApplicationData);

				var folder = Path.Combine(programData, "chocolatey", "lib", "ffmpeg", "tools",
					"ffmpeg", "bin");
				if (!File.Exists(Path.Combine(folder, exeNeeded)))
					folder = null;
				return folder;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}

		[PublicAPI]
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

		[PublicAPI]
		public static string MissingComponentMessage => Format(
			Localizer.GetString("FFprobeMissing", "Could not locate {0}",
				"Param is the name of a utility program required to be installed."),
			FFprobeExe);

		/// <summary>
		/// Gets the media information for the requested media file. If media information
		/// cannot be obtained from the file (e.g., because it is not a valid media file),
		/// the error is logged and an object is returned with null Audio and Video.
		/// </summary>
		[PublicAPI]
		public static MediaInfo GetInfo(string path)
		{
			if (!HaveNecessaryComponents)
				throw new ApplicationException(MissingComponentMessage);

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
		/// that occurred. If FFprobe cannot be located, this will be an
		/// <see cref="ApplicationException"/>. Otherwise, it will probably be an
		/// <see cref="FFMpegException"/>.</param>
		/// <returns>A new MediaInfo object, or null if media information could not be retrieved.</returns>
		[PublicAPI]
		public static MediaInfo GetInfo(string path, out Exception error)
		{
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

		/// <summary>
		/// Information about the primary audio track, if any
		/// </summary>
		[PublicAPI]
		public AudioInfo Audio { get; }
		/// <summary>
		/// Information about the primary video track, if any
		/// </summary>
		[PublicAPI]
		public VideoInfo Video { get; }

		//future public ImageInfo Image { get; private set;}

		public class AudioInfo
		{
			internal AudioInfo(IMediaAnalysis mediaAnalysis)
			{
				var primaryAudio = mediaAnalysis.PrimaryAudioStream;
				Debug.Assert(primaryAudio != null);
				Duration = primaryAudio.Duration;
				Encoding = primaryAudio.CodecName;
				ChannelCount = primaryAudio.Channels;
				BitDepth = primaryAudio.BitDepth ?? default;
				SamplesPerSecond = mediaAnalysis.PrimaryAudioStream?.SampleRateHz ?? default;
			}

			/// <summary>
			/// Note that since a media file might contain multiple audio and video tracks
			/// and they might start and stop at different times, the total duration reported
			/// by <see cref="MediaInfo.AnalysisData"/> could be greater than this duration.
			/// </summary>
			[PublicAPI]
			public TimeSpan Duration { get; }
			[PublicAPI]
			public int ChannelCount { get; }
			[PublicAPI]
			public int SamplesPerSecond { get; }
			[PublicAPI]
			public int BitDepth { get; }
			[PublicAPI]
			public string Encoding { get; }
		}

		public class VideoInfo
		{
			internal VideoInfo(IMediaAnalysis mediaAnalysis)
			{
				var primaryVideo = mediaAnalysis.PrimaryVideoStream;
				Debug.Assert(primaryVideo != null);
				Duration = primaryVideo.Duration;
				Encoding = primaryVideo.CodecName;
				FrameRate = primaryVideo.AvgFrameRate;
				Resolution = $"{primaryVideo.Width}x{primaryVideo.Height}";
			}

			/// <summary>
			/// Note that since a media file might contain multiple audio and video tracks
			/// and they might start and stop at different times, the total duration reported
			/// by <see cref="MediaInfo.AnalysisData"/> could be greater than this duration.
			/// </summary>
			[PublicAPI]
			public TimeSpan Duration { get; }
			// For backward compatibility, we want to support FramesPerSecond as an integer value,
			// but in case clients want the real rate, we supply that as well. Note that while
			// this is the average frame rate, the detailed analysis data can be used to access the
			// time base, real (tbr), aka, the target frame rate (which can be slightly different
			// from the actual average).
			// See https://www.hdhead.com/?p=108
			[PublicAPI]
			public double FrameRate { get; }
			[PublicAPI]
			public int FramesPerSecond => (int)Math.Round(FrameRate, 0);
			[PublicAPI]
			public string Resolution { get; }
			[PublicAPI]
			public string Encoding { get; }
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
