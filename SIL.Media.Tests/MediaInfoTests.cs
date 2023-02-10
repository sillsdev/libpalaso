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
				Assert.Ignore(MediaInfo.MissingComponentMessage);
		}

		[Test]
		[Category("RequiresFFprobe")]
		public void HaveNecessaryComponents_ReturnsTrue()
		{
			Assert.IsTrue(MediaInfo.HaveNecessaryComponents);
		}

		[Test]
		[Category("RequiresFFprobe")]
		public void VideoInfo_Duration_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.That(info.Video.Duration.TotalMilliseconds, Is.EqualTo(3069).Within(0.1));
				Assert.That(info.Video.Duration, Is.EqualTo(info.AnalysisData.Duration).Within(0.5).Milliseconds);
				Assert.That(info.Audio.Duration.TotalMilliseconds, Is.EqualTo(3029).Within(0.1));
			}
		}

		[Test]
		[Category("RequiresFFprobe")]
		public void VideoInfo_Encoding_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual("mpeg4", info.Video.Encoding);
			}
		}

		[Test]
		[Category("RequiresFFprobe")]
		public void VideoInfo_Resolution_Correct()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual("176x144", info.Video.Resolution);
			}
		}

		[Test]
		[Category("RequiresFFprobe")]
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
		[Category("RequiresFFprobe")]
		public void AudioInfo_Duration_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(918, info.Audio.Duration.TotalMilliseconds);
			}
		}


		[Test]
		[Category("RequiresFFprobe")]
		public void AudioInfo_SampleFrequency_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(44100, info.Audio.SamplesPerSecond);
			}
		}

		[Test]
		[Category("RequiresFFprobe")]
		public void AudioInfo_Channels_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(1, info.Audio.ChannelCount);
			}
		}

		[Test]
		[Category("RequiresFFprobe")]
		public void AudioInfo_BitDepth_Correct()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(16, info.Audio.BitDepth);
			}
		}

		[Test]
		[Category("RequiresFFprobe")]
		public void AudioInfo_H4N24BitStereoBitDepth_Correct()
		{
			using (var file = TempFile.FromResource(Resources._24bitH4NSample, ".wav"))
			{
				var info = MediaInfo.GetInfo(file.Path);
				Assert.AreEqual(24, info.Audio.BitDepth);
			}
		}

		[Test]
		[Category("RequiresFFprobe")]
		public void GetMediaInfo_AudioFile_VideoInfoAndImageInfoAreNull()
		{
			using (var file = TempFile.FromResource(Resources.finished,".wav"))
			{
				var info =MediaInfo.GetInfo(file.Path);
				Assert.IsNull(info.Video);
				//Assert.IsNull(info.Image);
			}
		}

	}
}
