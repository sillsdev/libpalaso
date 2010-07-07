using System;
using System.IO;
using NUnit.Framework;
using Palaso.CommandLineProcessing;
using Palaso.Media.Tests.Properties;
using Palaso.TestUtilities;


namespace Palaso.Media.Tests
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
		public void ExtractMp3Audio_CreatesFile()
		{
			using (var file = TempFile.FromResource(Resources.tiny, ".wmv"))
			{
				var outputPath = file.Path.Replace("wmv", "mp3");
				FFmpegRunner.ExtractMp3Audio(file.Path, outputPath, new NullProgress());
				Assert.IsTrue(File.Exists(outputPath));
			}
		}


	}
}
