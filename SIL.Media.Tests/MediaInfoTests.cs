using System;
using System.IO;
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
}
