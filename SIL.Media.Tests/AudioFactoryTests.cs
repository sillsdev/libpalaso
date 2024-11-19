using System.IO;
using NUnit.Framework;
using SIL.IO;

namespace SIL.Media.Tests
{
	// These will not work if a speaker is not available.
	[TestFixture]
	[Category("AudioTests")]
	public class AudioFactoryTests
	{
		[Test]
		public void Construct_FileDoesNotExist_OK()
		{
			// ReSharper disable once UnusedVariable
			using (var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName())) { }
		}

		[Test]
		public void Construct_FileDoesExistButEmpty_OK()
		{
			using (var f = new TempFile())
			{
				// ReSharper disable once UnusedVariable
				using (var x = AudioFactory.CreateAudioSession(f.Path)) { }
			}
		}


		[Test]
		public void Construct_FileDoesNotExist_DoesNotCreateFile()
		{
			var path = Path.GetRandomFileName();
			// ReSharper disable once UnusedVariable
			using (var x = AudioFactory.CreateAudioSession(path)) { }
			Assert.IsFalse(File.Exists(path)); // File doesn't exist after disposal
		}
	}
}
