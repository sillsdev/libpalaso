using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.IO;
using SIL.Media.Tests.Properties;
using SIL.TestUtilities;

namespace SIL.Media.Tests
{
	// Some of these tests require a microphone (or a virtual audio input device).
	[TestFixture]
	[NUnit.Framework.Category("AudioTests")]
	public class AudioSessionTests
	{
		[OneTimeSetUp]
		public void CheckPlatformSupport()
		{
#if NET8_0_OR_GREATER
			Assert.Ignore("AudioSession tests are not supported on .NET 8+ because WindowsAudioSession functionality is not available on this platform.");
#endif
		}

		[Test]
		public void StopRecordingAndSaveAsWav_NotRecording_Throws()
		{
			using (var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName()))
			{
				Assert.Throws<ApplicationException>(() => x.StopRecordingAndSaveAsWav());
			}
		}

		[Test]
		public void StopRecordingAndSaveAsWav_WhileRecording_NoExceptionThrown()
		{
			using (var session = new RecordingSession(100))
			{
				session.Recorder.StartRecording();
				Thread.Sleep(100);
				session.Recorder.StopRecordingAndSaveAsWav();
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresAudioInputDevice")]
		public void Play_WhileRecording_Throws()
		{
			using (var session = new RecordingSession())
			{
				Assert.Throws<ApplicationException>(() => session.Recorder.Play());
			}
		}

		[Test]
		public void CanRecord_FileDoesNotExist_True()
		{
			using (var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName()))
			{
				Assert.That(x.FilePath, Does.Not.Exist, "SETUP condition not met");
				Assert.IsTrue(x.CanRecord);
			}
		}

		[Test]
		public void CanStop_NonExistentFile_False()
		{
			using (var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName()))
			{
				Assert.That(x.FilePath, Does.Not.Exist, "SETUP condition not met");
				Assert.IsFalse(x.CanStop);
			}
		}

		[Test]
		public void CanRecord_ConstructWithEmptyFile_True()
		{
			using (var f = new TempFile())
			{
				using (var x = AudioFactory.CreateAudioSession(f.Path))
				{
					Assert.That(x.FilePath, Is.EqualTo(f.Path), "SETUP condition not met");
					Assert.That(x.FilePath, Does.Exist, "SETUP condition not met");
					Assert.IsTrue(x.CanRecord);
				}
			}
		}

		[Test]
		public void Play_FileEmpty_Throws()
		{
			using (var f = new TempFile())
			{
				using (var x = AudioFactory.CreateAudioSession(f.Path))
				{
					Assert.That(x.FilePath, Is.EqualTo(f.Path), "SETUP condition not met");
					Assert.That(x.FilePath, Does.Exist, "SETUP condition not met");
					Assert.Throws<FileLoadException>(() => x.Play());
				}
			}
		}

		[Test]
		public void Play_FileDoesNotExist_Throws()
		{
			using (var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName()))
			{
				Assert.That(x.FilePath, Does.Not.Exist, "SETUP condition not met");
				Assert.Throws<FileNotFoundException>(() => x.Play());
			}
		}

		/// <summary>
		/// For reasons which I don't entirely understand, this test will actually pass when run
		/// by itself against a single target framework without an audio output device, but
		/// to get it to pass when running as part of the fixture or when testing against both
		/// frameworks, it is necessary to have an audio output device.
		/// </summary>
		[Test]
		[NUnit.Framework.Category("RequiresAudioOutputDevice")]
		public void CanStop_WhilePlaying_True()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				using (var x = AudioFactory.CreateAudioSession(file.Path))
				{
					x.Play();
					Thread.Sleep(200);
					Assert.That(x.CanStop, Is.True, "Playback should last more than 200 ms.");
					// We used to test this in a separate test:
					// StopPlaying_WhilePlaying_NoExceptionThrown
					// But now that would be redundant since we need to stop playback in order to
					// safely dispose.
					x.StopPlaying();
				}
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresAudioInputDevice")]
		public void RecordAndStop_FileAlreadyExists_FileReplaced()
		{
			using (var f = new TempFile())
			{
				var oldInfo = new FileInfo(f.Path);
				var oldLength = oldInfo.Length;
				Assert.AreEqual(0, oldLength);
				var oldTimestamp = oldInfo.LastWriteTimeUtc;
				using (var x = AudioFactory.CreateAudioSession(f.Path))
				{
					x.StartRecording();
					Thread.Sleep(1000);
					x.StopRecordingAndSaveAsWav();
				}
				var newInfo = new FileInfo(f.Path);
				Assert.Greater(newInfo.LastWriteTimeUtc, oldTimestamp);
				Assert.Greater(newInfo.Length, oldLength);
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresAudioInputDevice")]
		public void IsRecording_WhileRecording_True()
		{
			using (var f = new TempFile())
			{
				using (var x = AudioFactory.CreateAudioSession(f.Path))
				{
					x.StartRecording();
					Thread.Sleep(100);
					Assert.IsTrue(x.IsRecording);
					x.StopRecordingAndSaveAsWav();
				}
			}
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "AudioAlsaSession doesn't implement ISimpleAudioWithEvents")]
		[NUnit.Framework.Category("RequiresAudioInputDevice")]
		public void RecordThenPlay_SmokeTest()
		{
			using (var f = new TempFile())
			{
				var w = new BackgroundWorker();
				// ReSharper disable once RedundantDelegateCreation
				w.DoWork += new DoWorkEventHandler((o, args) => SystemSounds.Exclamation.Play());

				using (var x = AudioFactory.CreateAudioSession(f.Path))
				{
					x.StartRecording();
					w.RunWorkerAsync();
					Thread.Sleep(1000);
					x.StopRecordingAndSaveAsWav();
					bool stopped = false;
					bool isPlayingInEventHandler = false;
					((ISimpleAudioWithEvents)x).PlaybackStopped += (o, args) =>
					{
						// assert here is swallowed, probably because not receiving exceptions from background worker.
						// We want to check that isPlaying is false even during the event handler.
						isPlayingInEventHandler = x.IsPlaying;
						stopped = true;
					};
					var watch = Stopwatch.StartNew();
					x.Play();
					while (!stopped)
					{
						Thread.Sleep(20);
						Application.DoEvents();
						if (watch.ElapsedMilliseconds > 2000)
						{
							x.StopPlaying();
							Assert.Fail("stop event not received");
						}
					}
					// After playback is stopped we shouldn't be reporting that it is playing
					Assert.That(isPlayingInEventHandler, Is.False);
					Assert.That(x.IsPlaying, Is.False);
				}
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresAudioInputDevice")]
		public void Play_GiveThaiFileName_ShouldHearTinklingSounds()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				using (var d = TemporaryFolder.Create(TestContext.CurrentContext))
				{
					var soundPath = d.Combine("ก.wav");
					RobustFile.Copy(file.Path, soundPath);
					using (var f = TempFile.TrackExisting(soundPath))
					{
						using (var y = AudioFactory.CreateAudioSession(f.Path))
						{
							y.Play();
							Thread.Sleep(1000);
							y.StopPlaying();
						}
					}
				}
			}
		}

		/// <summary>
		/// for testing things while recording is happening
		/// </summary>
		class RecordingSession : IDisposable
		{
			private readonly TempFile _tempFile;
			private readonly ISimpleAudioSession _recorder;

			public RecordingSession()
			{
				_tempFile = new TempFile();
				_recorder = AudioFactory.CreateAudioSession(_tempFile.Path);
				_recorder.StartRecording();
				Thread.Sleep(100);
			}

			public RecordingSession(int millisecondsToRecordBeforeStopping)
				: this()
			{
				if (millisecondsToRecordBeforeStopping == 0) millisecondsToRecordBeforeStopping = 1000;
				Thread.Sleep(millisecondsToRecordBeforeStopping);
				_recorder.StopRecordingAndSaveAsWav();
			}

			public ISimpleAudioSession Recorder => _recorder;

			public void Dispose()
			{
				try
				{
					if (_recorder.IsRecording)
					{
						_recorder.StopRecordingAndSaveAsWav();
					}
				}
				finally
				{
					_recorder.Dispose();
					_tempFile.Dispose();
				}
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresAudioInputDevice")]
		public void CanStop_WhileRecording_True()
		{
			using (var session = new RecordingSession())
			{
				Assert.IsTrue(session.Recorder.CanStop);
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresAudioInputDevice")]
		public void CanPlay_WhileRecording_False()
		{
			using (var session = new RecordingSession())
			{
				Assert.IsFalse(session.Recorder.CanPlay);
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresAudioInputDevice")]
		public void CanRecord_WhileRecording_False()
		{
			using (var session = new RecordingSession())
			{
				Assert.IsFalse(session.Recorder.CanRecord);
			}
		}

		[Test]
		public void StartRecording_WhileRecording_Throws()
		{
			using (var session = new RecordingSession())
			{
				Assert.Throws<ApplicationException>(() => session.Recorder.StartRecording());
			}
		}

		/// <summary>
		/// For reasons which I don't entirely understand, this test will actually pass when run
		/// by itself against a single target framework without an audio output device, but
		/// to get it to pass when running as part of the fixture or when testing against both
		/// frameworks, it is necessary to have an audio output device.
		/// </summary>
		[Test]
		[NUnit.Framework.Category("RequiresAudioOutputDevice")]
		[NUnit.Framework.Category("RequiresAudioInputDevice")]
		public void CanRecord_WhilePlaying_False()
		{
			using (var session = new RecordingSession(1000))
			{
				session.Recorder.Play();
				Thread.Sleep(100);
				Assert.That(session.Recorder.IsPlaying, Is.True,
					"Should be playing, not recording.");
				Assert.That(session.Recorder.CanRecord, Is.False);
			}
		}

		/// <summary>
		/// For reasons I don't entirely understand, this test will actually pass when run
		/// by itself against a single target framework without an audio output device, but
		/// to get it to pass when running as part of the fixture or when testing against both
		/// frameworks, it is necessary to have an audio output device. I tried setting the
		/// ParallelScope to None, but even that didn't work, so it does not seem to be an
		/// issue with parallel test runs affecting each other.
		/// </summary>
		[Test]
		[NUnit.Framework.Category("RequiresAudioOutputDevice")]
		public void CanPlay_WhilePlaying_False()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				using (var x = AudioFactory.CreateAudioSession(file.Path))
				{
					x.Play();
					Thread.Sleep(200);
					Assert.That(x.CanPlay, Is.False, "Playback should last more than 200 ms.");
					x.StopPlaying();
				}
			}
		}

		[Test]
		public void IsRecording_AfterRecording_False()
		{
			using (var f = new TempFile())
			{
				using (ISimpleAudioSession x = RecordSomething(f))
				{
					Assert.IsFalse(x.IsRecording);
				}
			}
		}

		[Test]
		public void RecordThenStop_CanPlay_IsTrue()
		{
			using (var f = new TempFile())
			{
				using (ISimpleAudioSession x = RecordSomething(f))
				{
					Assert.IsTrue(x.CanPlay);
				}
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresAudioInputDevice")]
		public void RecordThenPlay_OK()
		{
			using (var f = new TempFile())
			{
				using (ISimpleAudioSession x = RecordSomething(f))
				{
					x.Play();
					Thread.Sleep(100);
					x.StopPlaying();
				}
			}
		}

		private static ISimpleAudioSession RecordSomething(TempFile f)
		{
			var x = AudioFactory.CreateAudioSession(f.Path);
			x.StartRecording();
			Thread.Sleep(100);
			x.StopRecordingAndSaveAsWav();
			return x;
		}

		[Test]
		public void Play_DoesPlay ()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				using (var x = AudioFactory.CreateAudioSession(file.Path))
				{
					Assert.DoesNotThrow(() => x.Play());
					Assert.DoesNotThrow(() => x.StopPlaying());
				}
			}
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "AudioAlsaSession doesn't implement ISimpleAudioWithEvents")]
		public void Play_DoesPlayMp3_SmokeTest()
		{
			// file disposed after playback stopped
			using var file = TempFile.FromResource(Resources.ShortMp3, ".mp3");
			using (var x = AudioFactory.CreateAudioSession(file.Path))
			{
				((ISimpleAudioWithEvents)x).PlaybackStopped += (e, f) =>
				{
					Debug.WriteLine(f);
				};
				Assert.That(x.IsPlaying, Is.False);
				Assert.DoesNotThrow(() => x.Play());
				Assert.That(x.IsPlaying, Is.True);
				Assert.DoesNotThrow(() => x.StopPlaying());
				Assert.That(x.IsPlaying, Is.False);
			}
		}

		[Test]
		[NUnit.Framework.Category("RequiresAudioInputDevice")]
		public void Record_DoesRecord ()
		{
			using (var folder = TemporaryFolder.Create(TestContext.CurrentContext))
			{
				string fPath = folder.Combine("dump.ogg");
				using (var x = AudioFactory.CreateAudioSession(fPath))
				{
					Assert.DoesNotThrow(() => x.StartRecording());
					Assert.IsTrue(x.IsRecording);
					Thread.Sleep(1000);
					Assert.DoesNotThrow(() => x.StopRecordingAndSaveAsWav());
				}
			}
		}

		[Test]
		[NUnit.Framework.Category("ByHand")]
		[Explicit] // This test is to be run manually to test long recordings. After the first beep, recite John 3:16"
		public void Record_LongRecording()
		{
			using (var folder = TemporaryFolder.Create(TestContext.CurrentContext))
			{
				string fPath = Path.Combine(folder.Path, "long.wav");
				using (var x = AudioFactory.CreateAudioSession(fPath))
				{
					SystemSounds.Beep.Play();
					Assert.DoesNotThrow(() => x.StartRecording());
					Assert.IsTrue(x.IsRecording);
					Thread.Sleep(10000); // Records 10 seconds
					Assert.DoesNotThrow(() => x.StopRecordingAndSaveAsWav());
					SystemSounds.Beep.Play();
					Assert.IsTrue(x.CanPlay);
					Assert.DoesNotThrow(() => x.Play());
					Thread.Sleep(4000); // Plays the first 4 seconds
					Assert.DoesNotThrow(() => x.StopPlaying());
					Thread.Sleep(500); // Pause
					Assert.DoesNotThrow(() => x.Play());
					Thread.Sleep(6000); // Plays the first 6 seconds
					Assert.DoesNotThrow(() => x.StopPlaying());
				}
			}
		}
	}
}
