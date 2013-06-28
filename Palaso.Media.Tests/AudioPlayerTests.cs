using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Threading;
using NAudio.Wave;
using NUnit.Framework;
using Palaso.IO;
#if !MONO
using Palaso.Media.Naudio;
#endif
using Palaso.Media.Tests.Properties;
using Palaso.TestUtilities;


namespace Palaso.Media.Tests
{
   [TestFixture]
	public class AudioPlayerTests
	{
	   [Test]
	   public void LoadFile_ThenDispose_FileCanBeDeleted()
	   {
#if !MONO
		   var file = TempFile.FromResource(Resources._2Channel, ".wav");
		   using (var player = new AudioPlayer())
		   {
			   player.LoadFile(file.Path);
		   }
		   Assert.DoesNotThrow(() => File.Delete(file.Path));
#endif
	   }


	   /// <summary>
	   /// This test shows what caused hearthis to abandon the naudio; previous to a change to this class in 2/2012, it is believed that all was ok.
	   /// </summary>
	   [Test, Ignore("Known to Fail (hangs forever")]
	   public void PlayFile_ThenDispose_FileCanBeDeleted()
	   {
#if !MONO
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
#endif
	   }
	}
}
