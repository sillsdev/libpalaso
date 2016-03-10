﻿using NUnit.Framework;
using Palaso.IO;
using Palaso.Media.Tests.Properties;

namespace Palaso.Media.Tests
{
	[TestFixture]
	public class MediaInfoTests
	{
		[TestFixtureSetUp]
		public void CheckRequirements()
		{
			if (!MediaInfo.HaveNecessaryComponents)
				Assert.Ignore("These tests require ffmpeg to be installed.");
		}

		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void HaveNecessaryComponents_ReturnsTrue()
		{
			Assert.IsTrue(MediaInfo.HaveNecessaryComponents);
		}

		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void VideoInfo_Duration_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(3060, info.Video.Duration.TotalMilliseconds);
				Assert.AreEqual(3060, info.Audio.Duration.TotalMilliseconds);
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void VideoInfo_Encoding_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual("mpeg4", info.Video.Encoding);
			}
		}


		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void VideoInfo_Resolution_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual("176x144", info.Video.Resolution);
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void VideoInfo_MessedUpFramesPerSecond_LeavesEmpty()
		{
			//NB: our current sample has ffmpeg saying:
			//Seems stream 1 codec frame rate differs from container frame rate: 1000.00 (1000/1) -> 30.00 (30/1)
			//TODO: fix the sample so we test the more normal situation of a good file
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(0, info.Video.FramesPerSecond);
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void AudioInfo_Duration_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(910, info.Audio.Duration.TotalMilliseconds);
			}
		}


		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void AudioInfo_SampleFrequency_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(44100, info.Audio.SamplesPerSecond);
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void AudioInfo_Channels_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(1, info.Audio.ChannelCount);
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void AudioInfo_BitDepth_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(16, info.Audio.BitDepth);
			}
		}

			[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void AudioInfo_H4N24BitStereoBitDepth_Correct()
		{
			using (var file = TempFile.FromResource(Resources._24bitH4NSample, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(24, info.Audio.BitDepth);
			}
		}
	[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void GetMediaInfo_AudioFile_VideoInfoAndImageInfoAreNull()
		{
			using(var file = TempFile.FromResource(Resources.finished,".wav"))
			{
				var info =MediaInfo.GetInfo(file.Path);
				Assert.IsNull(info.Video);
				//Assert.IsNull(info.Image);
			}
		}

	}
}
