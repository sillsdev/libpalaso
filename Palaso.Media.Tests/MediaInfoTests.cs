using System;
using System.IO;
using NUnit.Framework;
using Palaso.Media.Tests.Properties;
using Palaso.TestUtilities;


namespace Palaso.Media.Tests
{
	[TestFixture]
	public class MediaInfoTests
	{

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
