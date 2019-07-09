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
	/// <summary>
	/// All these tests are skipped on TeamCity (even if you remove this category) because SIL.Media.Tests compiles to an exe,
	/// and the project that builds libpalaso on TeamCity (build/Palaso.proj, task Test) invokes RunNUnitTC which
	/// selects the test assemblies using Include="$(RootDir)/output/$(Configuration)/*.Tests.dll" which excludes exes.
	/// I have not tried to verify that all of these tests would actually have problems on TeamCity, but it seemed
	/// helpful to document in the usual way that they are not, in fact, run there.
	/// </summary>
	/// <remarks>Some of these tests will fail if a microphone isn't available.</remarks>
	[NUnit.Framework.Category("SkipOnTeamCity")]
	[TestFixture]
	[NUnit.Framework.Category("AudioTests")]
	public class AudioRecorderTests
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

		[Test]
		public void StopRecording_NotRecording_Throws()
		{
			using (var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName()))
			{
				Assert.Throws<ApplicationException>(() => x.StopRecordingAndSaveAsWav());
			}
		}

		[Test]
		public void StopPlaying_WhilePlaying_Ok()
		{
			using (var session = new RecordingSession(1000))
			{
				session.Recorder.Play();
				Thread.Sleep(100);
				session.Recorder.StopPlaying();
			}
		}

		[Test]
		public void Record_AfterRecordThenStop_Ok()
		{
			using (var session = new RecordingSession(100))
			{
				session.Recorder.StartRecording();
				Thread.Sleep(100);
				session.Recorder.StopRecordingAndSaveAsWav();
			}
		}

		[Test]
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
				Assert.IsTrue(x.CanRecord);
			}
		}

		[Test]
		public void CanStop_NonExistantFile_False()
		{
			using (var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName()))
			{
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
					Assert.Throws<FileLoadException>(() => x.Play());
				}
			}
		}

		[Test]
		public void Play_FileDoesNotExist_Throws()
		{
			using (var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName()))
			{
				Assert.Throws<FileNotFoundException>(() => x.Play());
			}
		}

		[Test]
		public void CanStop_WhilePlaying_True()
		{
			using (var session = new RecordingSession(1000))
			{
				session.Recorder.Play();
				Thread.Sleep(100);
				Assert.IsTrue(session.Recorder.CanStop);
			}
		}


		[Test]
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
					(x as ISimpleAudioWithEvents).PlaybackStopped += (o, args) =>
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
		public void Play_GiveThaiFileName_ShouldHearTwoSounds()
		{
			using (var d = new TemporaryFolder("palaso media test"))
			{
				var soundPath = d.Combine("ก.wav");
				File.Create(soundPath).Close();
				using (var f = TempFile.TrackExisting(soundPath))
				{
					var w = new BackgroundWorker();
					// ReSharper disable once RedundantDelegateCreation
					w.DoWork += new DoWorkEventHandler((o, args) => SystemSounds.Exclamation.Play());

					using (var x = AudioFactory.CreateAudioSession(f.Path))
					{
						x.StartRecording();
						w.RunWorkerAsync();
						Thread.Sleep(2000);
						x.StopRecordingAndSaveAsWav();
					}

					using (var y = AudioFactory.CreateAudioSession(f.Path))
					{
						y.Play();
						Thread.Sleep(1000);
						y.StopPlaying();
					}
				}
			}
		}


		/// <summary>
		/// for testing things while recording is happening
		/// </summary>
		class RecordingSession : IDisposable
		{
			private TempFile _tempFile;
			private ISimpleAudioSession _recorder;

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

			public ISimpleAudioSession Recorder
			{
				get { return _recorder; }
			}

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
		public void CanStop_WhileRecording_True()
		{
			using (var session = new RecordingSession())
			{
				Assert.IsTrue(session.Recorder.CanStop);
			}
		}

		[Test]
		public void CanPlay_WhileRecording_False()
		{
			using (var session = new RecordingSession())
			{
				Assert.IsFalse(session.Recorder.CanPlay);
			}
		}

		[Test]
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

		[Test]
		public void CanRecord_WhilePlaying_False()
		{
			using (var session = new RecordingSession(1000))
			{
				session.Recorder.Play();
				Thread.Sleep(100);
				Assert.IsTrue(session.Recorder.IsPlaying);
				Assert.IsFalse(session.Recorder.CanRecord);
			}
		}

		[Test]
		public void CanPlay_WhilePlaying_False()
		{
			using (var session = new RecordingSession(1000))
			{
				session.Recorder.Play();
				Thread.Sleep(100);
				Assert.IsFalse(session.Recorder.CanPlay);
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

		private ISimpleAudioSession RecordSomething(TempFile f)
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
			var file = TempFile.FromResource(Resources.ShortMp3, ".mp3");
			using (var x = AudioFactory.CreateAudioSession(file.Path))
			{
				(x as ISimpleAudioWithEvents).PlaybackStopped += (e, f) =>
				{
					Debug.WriteLine(f);
					Thread.Sleep(100);
					file.Dispose();
				};
				Assert.That(x.IsPlaying, Is.False);
				Assert.DoesNotThrow(() => x.Play());
				Assert.That(x.IsPlaying, Is.True);
				Assert.DoesNotThrow(() => x.StopPlaying());
				Assert.That(x.IsPlaying, Is.False);
			}
		}

		[Test]
		public void Record_DoesRecord ()
		{
			using (var folder = new TemporaryFolder("Record_DoesRecord"))
			{
				string fpath = Path.Combine(folder.Path, "dump.ogg");
				using (var x = AudioFactory.CreateAudioSession(fpath))
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
		[Ignore("this test is to be run manually to test long recordings. Recite John 3:16")]
		public void Record_LongRecording()
		{
			using (var folder = new TemporaryFolder("Record_LongRecording"))
			{
				string fpath = Path.Combine(folder.Path, "long.wav");
				using (var x = AudioFactory.CreateAudioSession(fpath))
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
