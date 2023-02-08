using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.Media.Tests.Properties;
using SIL.Progress;

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
	public class FFMpegRunnerTests
	{
		[OneTimeSetUp]
		public void CheckRequirements()
		{
			if (!FFmpegRunner.HaveNecessaryComponents)
				Assert.Ignore("These tests require ffmpeg to be installed.");
		}

		[Test]
		[Category("RequiresFfmpeg")]
		public void HaveNecessaryComponents_ReturnsTrue()
		{
			Assert.IsTrue(FFmpegRunner.HaveNecessaryComponents);
		}

		[Test]
		[Category("RequiresFfmpeg")]
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
		[Category("RequiresFfmpeg")]
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
