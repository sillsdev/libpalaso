using System.IO;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Media.Tests.Properties;
using Palaso.Progress;

namespace Palaso.Media.Tests
{
	[TestFixture]
	public class FFmpegRunnerTests
	{
		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void HaveNecessaryComponents_ReturnsTrue()
		{
			Assert.IsTrue(MediaInfo.HaveNecessaryComponents);
		}

		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
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
		[NUnit.Framework.Category("RequiresFfmpeg")]
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
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void MakeLowQualityCompressedAudio_CreatesFile()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var originalAudioPath = file.Path.Replace("wmv", "mp3");
				FFmpegRunner.ExtractMp3Audio(file.Path, originalAudioPath, 1, new NullProgress());

				var outputPath = originalAudioPath.Replace("mp3", "low.mp3");
				FFmpegRunner.MakeLowQualityCompressedAudio(originalAudioPath, outputPath, new ConsoleProgress());
				Assert.IsTrue(File.Exists(outputPath));
				System.Diagnostics.Process.Start(outputPath);
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresFfmpeg")]
		public void MakeLowQualitySmallVideo_CreatesFile()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var outputPath = file.Path.Replace("wmv", "low.wmv");
				FFmpegRunner.MakeLowQualitySmallVideo(file.Path, outputPath, 0, new ConsoleProgress());
				Assert.IsTrue(File.Exists(outputPath));
				System.Diagnostics.Process.Start(outputPath);
			}
		}
	}
}
