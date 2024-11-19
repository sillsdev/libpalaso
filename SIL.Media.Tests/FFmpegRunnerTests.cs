using System;
using System.IO;
using FFMpegCore;
using NUnit.Framework;
using SIL.IO;
using SIL.Media.Tests.Properties;
using SIL.Progress;

namespace SIL.Media.Tests
{
	[Category("RequiresFfmpeg")]
	[TestFixture]
	public class FFmpegRunnerTests
	{
		[OneTimeSetUp]
		public void CheckRequirements()
		{
			if (!FFmpegRunner.HaveNecessaryComponents)
			{
				if (Environment.GetEnvironmentVariable("CI") == null)
					Assert.Ignore("These tests require ffmpeg to be installed.");
				else
					Assert.Fail("On CI build using GHA, FFMpeg should have been installed before running tests.");
			}
		}

		[Test]
		public void HaveNecessaryComponents_NoExplicitMinVersion_ReturnsTrue()
		{
			Assert.IsTrue(FFmpegRunner.HaveNecessaryComponents);
		}

		[TestCase(5, 1)]
		[TestCase(4, 9)]
		public void HaveNecessaryComponents_TwoDigitMinVersion_ReturnsTrue(int major, int minor)
		{
			FFmpegRunner.FfmpegMinimumVersion = new Version(major, minor);
			Assert.IsTrue(FFmpegRunner.HaveNecessaryComponents);
		}

		[TestCase(5, 1, 1)]
		[TestCase(5, 0, 0)]
		public void HaveNecessaryComponents_ThreeDigitMinVersion_ReturnsTrue(int major, int minor, int build)
		{
			FFmpegRunner.FfmpegMinimumVersion = new Version(major, minor, build);
			Assert.IsTrue(FFmpegRunner.HaveNecessaryComponents);
		}

		[TestCase(5, 1, 1, 0)]
		[TestCase(5, 0, 0, 9)]
		public void HaveNecessaryComponents_FourDigitMinVersion_ReturnsTrue(int major, int minor, int build, int revision)
		{
			FFmpegRunner.FfmpegMinimumVersion = new Version(major, minor, build, revision);
			Assert.IsTrue(FFmpegRunner.HaveNecessaryComponents);
		}

		[Test]
		public void HaveNecessaryComponents_ReallyHighVersionThatDoesNotExist_ReturnsFalse()
		{
			FFmpegRunner.FfmpegMinimumVersion = new Version(int.MaxValue, int.MaxValue);
			Assert.IsFalse(FFmpegRunner.HaveNecessaryComponents);
		}

		[Test]
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
		public void MakeLowQualityCompressedAudio_CreatesFile()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var originalAudioPath = file.Path.Replace("wmv", "mp3");
				FFmpegRunner.ExtractMp3Audio(file.Path, originalAudioPath, 1, new NullProgress());

				var outputPath = originalAudioPath.Replace("mp3", "low.mp3");
				FFmpegRunner.MakeLowQualityCompressedAudio(originalAudioPath, outputPath, new ConsoleProgress());
				Assert.IsTrue(File.Exists(outputPath));
				
				var mediaInfoOrig = FFProbe.Analyse(file.Path);
				var mediaInfo = FFProbe.Analyse(outputPath);

				// Validate resolution and bit rate
				Assert.That(mediaInfo.PrimaryVideoStream, Is.Null);
				Assert.That(mediaInfo.PrimaryAudioStream, Is.Not.Null);
				Assert.That(mediaInfo.AudioStreams.Count, Is.EqualTo(1));
				Assert.That(mediaInfo.PrimaryAudioStream.Channels, Is.EqualTo(1));
				Assert.That(mediaInfo.Format.BitRate, Is.LessThan(mediaInfoOrig.Format.BitRate));
				Assert.That(mediaInfo.PrimaryAudioStream.SampleRateHz, Is.EqualTo(8000));
				try
				{
					RobustFile.Delete(outputPath);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}

		[Test]
		public void MakeLowQualitySmallVideo_CreatesFile()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var outputPath = file.Path.Replace("wmv", "low.wmv");
				FFmpegRunner.MakeLowQualitySmallVideo(file.Path, outputPath, 0, new ConsoleProgress());
				Assert.IsTrue(File.Exists(outputPath));

				var mediaInfoOrig = FFProbe.Analyse(file.Path);
				var mediaInfo = FFProbe.Analyse(outputPath);

				// Validate resolution and bit rate
				Assert.That(mediaInfo.PrimaryVideoStream, Is.Not.Null);
				Assert.That(mediaInfo.VideoStreams.Count, Is.EqualTo(1));
				Assert.That(mediaInfo.PrimaryAudioStream, Is.Not.Null);
				Assert.That(mediaInfo.AudioStreams.Count, Is.EqualTo(1));
				Assert.That(mediaInfo.PrimaryVideoStream.Width, Is.EqualTo(160));
				Assert.That(mediaInfo.PrimaryVideoStream.Height, Is.EqualTo(120));
				Assert.That(mediaInfo.Format.BitRate, Is.LessThan(mediaInfoOrig.Format.BitRate));
				try
				{
					// When running the by-hand test, the default media player might leave this
					// locked, so this cleanup will fail.
					RobustFile.Delete(outputPath);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}
	}
}
