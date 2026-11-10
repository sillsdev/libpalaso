using System;
using System.IO;
using System.Reflection;
using FFMpegCore;
using Moq;
using NUnit.Framework;
using SIL.IO;
using SIL.Media.Tests.Properties;

namespace SIL.Media.Tests
{
	[Category("RequiresFFprobe")]
	[TestFixture]
	public class MediaInfoTests
	{
		[OneTimeSetUp]
		public void CheckRequirements()
		{
			if (!MediaInfo.HaveNecessaryComponents)
			{
				if (Environment.GetEnvironmentVariable("CI") == null)
					Assert.Ignore(MediaInfo.MissingComponentMessage);
				else
					Assert.Fail("On CI build using GHA, FFMpeg should have been installed before running tests.");
			}
		}

		[Test]
		public void HaveNecessaryComponents_ReturnsTrue()
		{
			Assert.IsTrue(MediaInfo.HaveNecessaryComponents,
				"FFprobe was expected to have been found on system path or in a known location.");
		}

		[TestCase(null)]
		[TestCase("")]
		public void SetFFprobeFolder_ToNullOrEmpty_HaveNecessaryComponentsReturnsTrue(string presetFolder)
		{
			MediaInfo.FFprobeFolder = presetFolder;
			Assert.IsTrue(MediaInfo.HaveNecessaryComponents,
				"FFprobe was expected to have been found on system path or in a known location.");
		}

		[Test]
		public void SetFFprobeFolder_ToNonexistentFolder_ThrowsDirectoryNotFoundException()
		{
			Assert.That(() =>
			{
				MediaInfo.FFprobeFolder = "D:\\ThereIsNoWayThi5F0lderShould\\exist";
			}, Throws.Exception.InstanceOf<DirectoryNotFoundException>());
		}

		[Test]
		public void SetFFprobeFolder_ToFolderWithoutFFprobe_ThrowsFileNotFoundException()
		{
			Assert.That(() =>
			{
				MediaInfo.FFprobeFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
			}, Throws.Exception.InstanceOf<FileNotFoundException>());
		}

		[Test]
		public void VideoInfo_Duration_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.That(info.Video.Duration.TotalMilliseconds, Is.EqualTo(3069).Within(0.1));
				// This equality holds for this single-video/single-audio fixture, but is not
				// guaranteed in general; see the remarks on VideoInfo/AudioInfo.Duration about
				// how AnalysisData.Duration is computed across all tracks as of FFMpegCore 6.0.
				Assert.That(info.Video.Duration, Is.EqualTo(info.AnalysisData.Duration).Within(1).Milliseconds);
				Assert.That(info.Audio.Duration.TotalMilliseconds, Is.EqualTo(3029).Within(0.1));
			}
		}

		[Test]
		public void VideoInfo_Encoding_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual("mpeg4", info.Video.Encoding);
			}
		}

		[Test]
		public void VideoInfo_Resolution_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual("176x144", info.Video.Resolution);
			}
		}

		[Test]
		public void VideoInfo_FramesPerSecond_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(30, info.Video.FramesPerSecond);
				Assert.That(info.Video.FrameRate, Is.EqualTo(29.97d).Within(.0001));
			}
		}

		[Test]
		public void AudioInfo_Duration_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(918, info.Audio.Duration.TotalMilliseconds);
			}
		}


		[Test]
		public void AudioInfo_SampleFrequency_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(44100, info.Audio.SamplesPerSecond);
			}
		}

		[Test]
		public void AudioInfo_Channels_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(1, info.Audio.ChannelCount);
			}
		}

		[Test]
		public void AudioInfo_BitDepth_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(16, info.Audio.BitDepth);
			}
		}

		[Test]
		public void AudioInfo_H4N24BitStereoBitDepth_Correct()
		{
			using (var file = TempFile.FromResource(Resources._24bitH4NSample, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(24, info.Audio.BitDepth);
			}
		}

		[Test]
		public void GetMediaInfo_AudioFile_VideoInfoAndImageInfoAreNull()
		{
			using (var file = TempFile.FromResource(Resources.finished,".wav"))
			{
				var info =MediaInfo.GetInfo(file.Path);
				Assert.IsNull(info.Video);
			}
		}

	}

	/// <summary>
	/// Covers the cases where ffprobe doesn't report certain stream properties (surfaced as
	/// nullable properties on FFMpegCore's VideoStream/AudioStream as of FFMpegCore 6.0), which
	/// can't be exercised using a real media fixture. These construct AudioInfo/VideoInfo
	/// directly against a mocked IMediaAnalysis, bypassing FFprobe entirely, so (unlike
	/// MediaInfoTests above) they don't require FFprobe to be installed.
	/// </summary>
	[TestFixture]
	public class MediaInfoNullableStreamDataTests
	{
		private static T CreateStream<T>() where T : MediaStream =>
			(T)Activator.CreateInstance(typeof(T), nonPublic: true);

		private static void SetProperty(object target, string propertyName, object value)
		{
			target.GetType().GetProperty(propertyName)?.SetValue(target, value);
		}

		[Test]
		public void VideoInfo_WidthAndHeightNull_ResolutionIsEmpty()
		{
			var videoStream = CreateStream<VideoStream>();
			SetProperty(videoStream, nameof(VideoStream.Width), null);
			SetProperty(videoStream, nameof(VideoStream.Height), null);
			SetProperty(videoStream, nameof(VideoStream.CodecName), "mpeg4");

			var analysis = new Mock<IMediaAnalysis>();
			analysis.SetupGet(a => a.PrimaryVideoStream).Returns(videoStream);

			var videoInfo = new MediaInfo.VideoInfo(analysis.Object);

			Assert.That(videoInfo.Resolution, Is.Empty);
		}

		[Test]
		public void VideoInfo_WidthAndHeightSet_ResolutionIsFormatted()
		{
			var videoStream = CreateStream<VideoStream>();
			SetProperty(videoStream, nameof(VideoStream.Width), 160);
			SetProperty(videoStream, nameof(VideoStream.Height), 120);
			SetProperty(videoStream, nameof(VideoStream.CodecName), "mpeg4");

			var analysis = new Mock<IMediaAnalysis>();
			analysis.SetupGet(a => a.PrimaryVideoStream).Returns(videoStream);

			var videoInfo = new MediaInfo.VideoInfo(analysis.Object);

			Assert.That(videoInfo.Resolution, Is.EqualTo("160x120"));
		}

		[Test]
		public void VideoInfo_CodecNameNull_EncodingIsEmpty()
		{
			var videoStream = CreateStream<VideoStream>();
			SetProperty(videoStream, nameof(VideoStream.Width), 160);
			SetProperty(videoStream, nameof(VideoStream.Height), 120);
			SetProperty(videoStream, nameof(VideoStream.CodecName), null);

			var analysis = new Mock<IMediaAnalysis>();
			analysis.SetupGet(a => a.PrimaryVideoStream).Returns(videoStream);

			var videoInfo = new MediaInfo.VideoInfo(analysis.Object);

			Assert.That(videoInfo.Encoding, Is.Empty);
		}

		[Test]
		public void AudioInfo_CodecNameNull_EncodingIsEmpty()
		{
			var audioStream = CreateStream<AudioStream>();
			SetProperty(audioStream, nameof(AudioStream.CodecName), null);
			SetProperty(audioStream, nameof(AudioStream.Channels), 2);
			SetProperty(audioStream, nameof(AudioStream.SampleRateHz), 44100);

			var analysis = new Mock<IMediaAnalysis>();
			analysis.SetupGet(a => a.PrimaryAudioStream).Returns(audioStream);

			var audioInfo = new MediaInfo.AudioInfo(analysis.Object);

			Assert.That(audioInfo.Encoding, Is.Empty);
		}

		[Test]
		public void AudioInfo_ChannelsNull_ChannelCountIsZero()
		{
			var audioStream = CreateStream<AudioStream>();
			SetProperty(audioStream, nameof(AudioStream.CodecName), "pcm_s16le");
			SetProperty(audioStream, nameof(AudioStream.Channels), null);

			var analysis = new Mock<IMediaAnalysis>();
			analysis.SetupGet(a => a.PrimaryAudioStream).Returns(audioStream);

			var audioInfo = new MediaInfo.AudioInfo(analysis.Object);

			Assert.That(audioInfo.ChannelCount, Is.EqualTo(0));
		}
	}
}
