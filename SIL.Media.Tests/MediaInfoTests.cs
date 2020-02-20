using NUnit.Framework;
using SIL.IO;
using SIL.Media.Tests.Properties;

namespace SIL.Media.Tests
{
	/// <summary>
	/// All these tests are skipped on TeamCity (even if you remove this category) because SIL.Media.Tests compiles to an exe,
	/// and the project that builds libpalaso on TeamCity (build/Palaso.proj, task Test) invokes RunNUnitTC which
	/// selects the test assemblies using Include="$(RootDir)/output/$(Configuration)/*.Tests.dll" which excludes exes.
	/// I have not tried to verify that all of these tests would actually have problems on TeamCity, but it seemed
	/// helpful to document in the usual way that they are not, in fact, run there. 
	/// </summary>
	[Category("SkipOnTeamCity")]
	[TestFixture]
	public class MediaInfoTests
	{
		[OneTimeSetUp]
		public void CheckRequirements()
		{
			if (!MediaInfo.HaveNecessaryComponents)
				Assert.Ignore("These tests require ffmpeg to be installed.");
		}

		[Test]
		[Category("RequiresFfmpeg")]
		public void HaveNecessaryComponents_ReturnsTrue()
		{
			Assert.IsTrue(MediaInfo.HaveNecessaryComponents);
		}

		[Test]
		[Category("RequiresFfmpeg")]
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
		[Category("RequiresFfmpeg")]
		public void VideoInfo_Encoding_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual("mpeg4", info.Video.Encoding);
			}
		}

		[Test]
		[Category("RequiresFfmpeg")]
		public void VideoInfo_Resolution_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual("176x144", info.Video.Resolution);
			}
		}

		[Test]
		[Category("RequiresFfmpeg")]
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
		[Category("RequiresFfmpeg")]
		public void AudioInfo_Duration_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(910, info.Audio.Duration.TotalMilliseconds);
			}
		}


		[Test]
		[Category("RequiresFfmpeg")]
		public void AudioInfo_SampleFrequency_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(44100, info.Audio.SamplesPerSecond);
			}
		}

		[Test]
		[Category("RequiresFfmpeg")]
		public void AudioInfo_Channels_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(1, info.Audio.ChannelCount);
			}
		}

		[Test]
		[Category("RequiresFfmpeg")]
		public void AudioInfo_BitDepth_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(16, info.Audio.BitDepth);
			}
		}

		[Test]
		[Category("RequiresFfmpeg")]
		public void AudioInfo_H4N24BitStereoBitDepth_Correct()
		{
			using (var file = TempFile.FromResource(Resources._24bitH4NSample, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(24, info.Audio.BitDepth);
			}
		}

		[Test]
		[Category("RequiresFfmpeg")]
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
