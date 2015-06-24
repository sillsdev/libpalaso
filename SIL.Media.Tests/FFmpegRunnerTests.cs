using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.Media.Tests.Properties;
using SIL.Progress;

namespace SIL.Media.Tests
{
	[TestFixture]
	public class FFmpegRunnerTests
	{
		[Test]
		[Category("RequiresFfmpeg")]
		public void HaveNecessaryComponents_ReturnsTrue()
		{
			Assert.IsTrue(MediaInfo.HaveNecessaryComponents);
		}

		[Test]
		[Category("RequiresFfmpeg")]
		[Platform(Exclude="Linux", Reason="MP3 is not licensed on Linux")]
		public void ExtractMp3Audio_CreatesFile()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var outputPath = file.Path.Replace("wmv", "mp3");
				FFmpegRunner.ExtractMp3Audio(file.Path, outputPath, 1, new NullProgress());
				Assert.IsTrue(File.Exists(outputPath));
			}
		}

		[Test]
		[Category("RequiresFfmpeg")]
		public void ExtractOggAudio_CreatesFile()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var outputPath = file.Path.Replace("wmv", "ogg");
				FFmpegRunner.ExtractOggAudio(file.Path, outputPath, 1, new NullProgress());
				Assert.IsTrue(File.Exists(outputPath));
			}
		}

		[Test]
		[Category("RequiresFfmpeg")]
		public void ChangeNumberOfAudioChannels_CreatesFile()
		{
			using (var file = TempFile.FromResource(Resources._2Channel, ".wav"))
			{
				var outputPath = file.Path.Replace(".wav", "1ch.wav");
				FFmpegRunner.ChangeNumberOfAudioChannels(file.Path, outputPath, 1, new NullProgress());
				Assert.IsTrue(File.Exists(outputPath));
			}
		}

		[Test]
		[Category("RequiresFfmpeg")]
		[Platform(Exclude="Linux", Reason="MP3 is not licensed on Linux")]
		public void MakeLowQualityCompressedAudio_CreatesFile()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var originalAudioPath = file.Path.Replace("wmv", "mp3");
				FFmpegRunner.ExtractMp3Audio(file.Path, originalAudioPath, 1, new NullProgress());

				var outputPath = originalAudioPath.Replace("mp3", "low.mp3");
				FFmpegRunner.MakeLowQualityCompressedAudio(originalAudioPath, outputPath, new ConsoleProgress());
				Assert.IsTrue(File.Exists(outputPath));
#if !MONO
				System.Diagnostics.Process.Start(outputPath);
#endif
			}
		}

		[Test]
		[Category("RequiresFfmpeg")]
		[Platform(Exclude="Linux", Reason="MP3 is not licensed on Linux")]
		public void MakeLowQualitySmallVideo_CreatesFile()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var outputPath = file.Path.Replace("wmv", "low.wmv");
				FFmpegRunner.MakeLowQualitySmallVideo(file.Path, outputPath, 0, new ConsoleProgress());
				Assert.IsTrue(File.Exists(outputPath));
#if !MONO
				System.Diagnostics.Process.Start(outputPath);
#endif
			}
		}
	}
}
