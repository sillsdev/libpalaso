using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using SIL.IO;
using SIL.Media.Tests.Properties;
using SIL.Media.Naudio;
using NAudio.Wave;

namespace SIL.Media.Tests
{
	/// <summary>
	/// SkipOnTeamCity does not affect CI build using GHA, but I'm keeping it here for posterity's sake or if we ever
	/// need to go back to building on TC. The comment here used to say that this test fixture compiled to an exe so
	/// the tests would be skipped on TC anyway, but that is no longer true.
	/// </summary>
	[Category("SkipOnTeamCity")]
	[TestFixture]
	public class AudioPlayerTests
	{
		[Test]
		public void LoadFile_ThenDispose_FileCanBeDeleted()
		{
			var file = TempFile.FromResource(Resources._2Channel, ".wav");
			using (var player = new AudioPlayer())
			{
				player.LoadFile(file.Path);
			}

			Assert.DoesNotThrow(() => File.Delete(file.Path));
		}

		/// <summary>
		/// This test shows what caused HearThis to abandon NAudio; previous to a change to this class in 2/2012, it is believed that all was ok.
		/// </summary>
		[Test, Ignore("Known to Fail (hangs forever")]
		public void PlayFile_ThenDispose_FileCanBeDeleted()
		{
			var file = TempFile.FromResource(Resources._2Channel, ".wav");
			using (var player = new AudioPlayer())
			{
				player.LoadFile(file.Path);
				//player.Stopped += (s, e) => { Assert.DoesNotThrow(() => File.Delete(file.Path)); };

				player.StartPlaying();
				var giveUpTime = DateTime.Now.AddSeconds(3);
				while (player.PlaybackState != PlaybackState.Stopped && DateTime.Now < giveUpTime)
					Thread.Sleep(100);
			}

			Assert.DoesNotThrow(() => File.Delete(file.Path));
			Assert.False(File.Exists(file.Path));
		}
	}
}
