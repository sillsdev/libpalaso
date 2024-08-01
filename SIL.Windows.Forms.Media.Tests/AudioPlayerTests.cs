using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using SIL.IO;
using SIL.Windows.Forms.Media.Tests.Properties;
using SIL.Windows.Forms.Media.Naudio;
using NAudio.Wave;

namespace SIL.Windows.Forms.Media.Tests
{
	/// <summary>
	/// All these tests are skipped on TeamCity (even if you remove this category) because SIL.Windows.Forms.Media.Tests compiles to an exe,
	/// and the project that builds libpalaso on TeamCity (build/Palaso.proj, task Test) invokes RunNUnitTC which
	/// selects the test assemblies using Include="$(RootDir)/output/$(Configuration)/*.Tests.dll" which excludes exes.
	/// I have not tried to verify that all of these tests would actually have problems on TeamCity, but it seemed
	/// helpful to document in the usual way that they are not, in fact, run there. 
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
		/// This test shows what caused hearthis to abandon the naudio; previous to a change to this class in 2/2012, it is believed that all was ok.
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
