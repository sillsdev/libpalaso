using System;
using System.IO;
using NUnit.Framework;
using SIL.IO;

namespace SIL.Media.Tests
{
	[TestFixture]
	[Category("AudioTests")]
	[Category("RequiresAudioOutputDevice")] // These will not work if a speaker is not available.
	public class AudioFactoryTests
	{
		[OneTimeSetUp]
		public void CheckPlatformSupport()
		{
#if NET8_0_OR_GREATER
			Assert.Ignore("AudioFactory tests are not supported on .NET 8+ because WindowsAudioSession functionality is not available on this platform.");
#endif
		}

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
