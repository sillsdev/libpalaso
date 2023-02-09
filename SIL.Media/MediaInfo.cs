using System;
using System.Diagnostics;
using System.IO;
using FFMpegCore;
using FFMpegCore.Exceptions;
using SIL.PlatformUtilities;
using SIL.Reporting;
using static System.String;
using static SIL.IO.FileLocationUtilities;

namespace SIL.Media
{
	/// <summary>
	/// This class uses FFProbe (via FFMpegCore) to gather information about media streams.
	/// </summary>
	public class MediaInfo
	{
		private static string FFProbeExe => Platform.IsWindows ? "ffprobe.exe" : "ffprobe";

		private static string s_ffProbeFolder;
		
		/// <summary>
		/// Returns false if it can't find FFProbe
		/// </summary>
		public static bool HaveNecessaryComponents => !IsNullOrEmpty(LocateAndRememberFFProbe());

		/// <summary>
		/// The folder where FFProbe should be found. An application can use this to set
		/// the location before making any calls.
		/// </summary>
		/// <exception cref="DirectoryNotFoundException">Folder does not exist</exception>
		/// <exception cref="FileNotFoundException">Folder does not contain FFProbe.exe</exception>
		/// <remarks>If set to <see cref="Empty"/>, indicates to this library that
		/// FFProbe is not installed, and therefore methods to retrieve media info will
		/// fail unconditionally (i.e. they will throw an exception)</remarks>
		public static string FFProbeFolder
		{
			get => s_ffProbeFolder;
			set
			{
				if (value != Empty)
				{
					if (!Directory.Exists(value))
						throw new DirectoryNotFoundException("Directory not found: " + value);

					if (!File.Exists(Path.Combine(value, FFProbeExe)))
					{
						throw new FileNotFoundException(
							"Path is not a folder containing FFProbe.exe: " + value);
					}
				}

				s_ffProbeFolder = value;
			}
		}

		/// <summary>
		/// Find the path to FFProbe, and remember it.
		/// </summary>
		/// <returns></returns>
		private static string LocateAndRememberFFProbe()
		{
			if (null != FFProbeFolder)
				return FFProbeFolder;
			try
			{
				FFProbeFolder = GetPresumedFFProbeFolder();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
			return FFProbeFolder;
		}

		/// <summary>
		/// On Windows, FFProbe will typically be distributed with SIL software, but if something
		/// wants to use this library and work with a version of it that the user downloaded or
		/// compiled locally, this tries to find where they put it. On other platforms, ffprobe
		/// should be installed in the standard location by the package manager.
		/// </summary>
		/// <returns>A folder where FFProbe can be found; else <see cref="Empty"/></returns>
		private static string GetPresumedFFProbeFolder()
		{
			var folder = GlobalFFOptions.Current.BinaryFolder;

			if (Platform.IsWindows)
			{
				if (IsNullOrEmpty(folder) || !File.Exists(Path.Combine(folder, FFProbeExe)))
				{
					var withApplicationDirectory =
						GetFileDistributedWithApplication(true, "ffmpeg", FFProbeExe) ??
						GetFileDistributedWithApplication(true, "ffprobe", FFProbeExe);

					folder = !IsNullOrEmpty(withApplicationDirectory) ?
						Path.GetDirectoryName(withApplicationDirectory) :
						GetFFMpegFolderFromChocoInstall(FFProbeExe);

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

		internal static string GetFFMpegFolderFromChocoInstall(string exeNeeded)
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

		public static string MissingComponentMessage => Format(
			Localizer.GetString("FFProbeMissing", "Could not locate {0}",
				"Param is the name of a utility program required to be installed."),
			"FFProbe");

		/// <summary>
		/// Gets the media information for the requested media file. If media information
		/// the error is logged and an object is returned with null Audio and Video.
		/// </summary>
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
		/// that occurred. If FFProbe cannot be located, this will be an
		/// <see cref="ApplicationException"/>. Otherwise, it will probably be an
		/// <see cref="FFMpegException"/>.</param>
		/// <returns>A new MediaInfo object, or null if media information could not be retrieved.</returns>
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
		public AudioInfo Audio { get; }
		/// <summary>
		/// Information about the primary video track, if any
		/// </summary>
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
			public TimeSpan Duration { get; }
			public int ChannelCount { get; }
			public int SamplesPerSecond { get; }
			public int BitDepth { get; }
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
				FrameRate = primaryVideo.FrameRate;
				Resolution = $"{primaryVideo.Width}x{primaryVideo.Height}";
			}

			/// <summary>
			/// Note that since a media file might contain multiple audio and video tracks
			/// and they might start and stop at different times, the total duration reported
			/// by <see cref="MediaInfo.AnalysisData"/> could be greater than this duration.
			/// </summary>
			public TimeSpan Duration { get; }
			// For backward compatibility, we want to support FramesPerSecond as an integer value,
			// but in case clients want the real rate, we supply that as well.
			// See https://www.hdhead.com/?p=108
			public double FrameRate { get; }
			public int FramesPerSecond => (int)Math.Round(FrameRate, 0);
			public string Resolution { get; }
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
