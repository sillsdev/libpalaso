using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace Palaso.Media
{
	/// <summary>
	/// This class uses ffmpeg to gather information about media streams
	/// </summary>
	public class MediaInfo
	{
		private  MediaInfo(string ffmpegOutput)
		{
			Audio = new AudioInfo(ffmpegOutput);

		}

		static public MediaInfo GetInfo(string path)
		{
			var p = new Process();
			p.StartInfo.FileName = "ffmpeg";
			p.StartInfo.Arguments = ( "-i " + "\""+path+"\"");
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;
			p.Start();

			string error = p.StandardError.ReadToEnd();
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
			return new MediaInfo(error);
		}

		public AudioInfo Audio { get; private set;}
		public VideoInfo Video { get; private set;}
		//future public ImageInfo Image { get; private set;}

		public class AudioInfo
		{
			public enum AudioEncoding {Other, Wav, Mp3}

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
			}

			private void ExtractChannels(string ffmpegOutput)
			{
			}

			private void ExtractSamplesPerSecond(string ffmpegOutput)
			{
			//	var match = Regex.Match(ffmpegOutput, "bitrate: (.*) kb");
				var match = Regex.Match(ffmpegOutput, ", (.*) Hz");
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
			}

			private void ExtractDuration(string ffmpegOutput)
			{
				var match  = Regex.Match(ffmpegOutput, "Duration: (.*),");
				if (match.Groups.Count == 2)
				{
					var durationString = match.Groups[1].Value;

					TimeSpan durationTime;
					if (TimeSpan.TryParse(durationString, out durationTime))
					{
						Duration = durationTime;
					}
				}
			}

			public TimeSpan Duration { get; private set;}
			public int ChannelCount { get; private set;}
			public int SamplesPerSecond { get; private set;}
			public int BitDepth { get; private set;}
			public AudioEncoding Encoding { get; private set;}
		}
		public class VideoInfo
		{
			public enum VideoEncoding {Other}//todo

			internal VideoInfo()
			{
			}
			public TimeSpan Duration { get; private set;}
			public int FramesPerSecond { get; private set;}
			public int HorizontalResolution { get; private set;}
			public int VerticalResolution { get; private set;}
			public VideoEncoding Encoding { get; private set;}
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
