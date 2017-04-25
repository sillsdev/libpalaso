using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Threading;
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
	[NUnit.Framework.Category("SkipOnTeamCity")]
	[TestFixture]
	[NUnit.Framework.Category("AudioTests")]
	public class AudioRecorderTests
	{
		[Test]
		public void Construct_FileDoesNotExist_OK()
		{
			var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName());
		}

		[Test]
		public void Construct_FileDoesExistButEmpty_OK()
		{
			using (var f = new TempFile())
			{
				var x = AudioFactory.CreateAudioSession(f.Path);
			}
		}


		[Test]
		public void Construct_FileDoesNotExist_DoesNotCreateFile()
		{
			var path = Path.GetRandomFileName();
			var x = AudioFactory.CreateAudioSession(path);
			Assert.IsFalse(File.Exists(path));
		}

		[Test]
		public void StopRecording_NotRecording_Throws()
		{
			var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName());
			Assert.Throws<ApplicationException>(() => x.StopRecordingAndSaveAsWav());
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
			var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName());
			Assert.IsTrue(x.CanRecord);
		}

		[Test]
		public void CanStop_NonExistantFile_False()
		{
			var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName());
			Assert.IsFalse(x.CanStop);
		}

		[Test]
		public void CanRecord_ConstructWithEmptyFile_True()
		{
			using (var f = new TempFile())
			{
				var x = AudioFactory.CreateAudioSession(f.Path);
				Assert.IsTrue(x.CanRecord);
			}
		}

		[Test]
		[Platform(Exclude ="Windows", Reason="IrrKlang doesn't throw, so we don't really know")]
		public void Play_FileEmpty_Throws()
		{
			using (var f = new TempFile())
			{
				var x = AudioFactory.CreateAudioSession(f.Path);
				Assert.Throws<Exception>(() => x.Play());
			}
		}

		[Test]
		public void Play_FileDoesNotExist_Throws()
		{
			var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName());
			Assert.Throws<FileNotFoundException>(() => x.Play());
		}

		// It's not completely reproducible, but typically including one of the tests marked like this
		// will cause aborting the whole test sequence (or cause it to lock up, if you aren't using a
		// test runner with a time limit). As near as I can make out, things fail at so high a level
		// that no useful stack trace emerges. The problem seems to be the one described at
		// http://lambert.geek.nz/2007/05/unmanaged-appdomain-callback/. Specifically, something
		// in IrrKlang's unmanaged code calls a managed-code function. The common case appears to be
		// to report that Play has stopped, but there may be other callbacks. This works fine in an
		// app that is only running in one AppDomain, but NUnit uses two, one for the test management
		// code and one for the SUT. Apparently unmanaged code has no way to know which AppDomain to
		// make the call-back in and guesses the first and that is wrong, leading to an exception
		// "Cannot pass a GCHandle across AppDomains". Somehow this is at such a high level that
		// it freezes up the whole test sequence, possibly because the exception is thrown in
		// NUnit's management AppDomain. There doesn't seem to be any way to fix this in our code
		// (the link above suggests something IrrKlang could do) so I've disabled the offending tests.
		[Ignore("Aborts with 'GC across AppDomain not allowed' IrrKlang.Play/Nunit incompatibility")]
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
				var old = File.GetLastWriteTimeUtc(f.Path);
				var oldInfo = new FileInfo(f.Path);
				var oldLength = oldInfo.Length;
				Assert.AreEqual(0, oldLength);
				var oldTimestamp = oldInfo.LastWriteTimeUtc;
				var x = AudioFactory.CreateAudioSession(f.Path);
				x.StartRecording();
				Thread.Sleep(1000);
				x.StopRecordingAndSaveAsWav();
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
				var x = AudioFactory.CreateAudioSession(f.Path);
				x.StartRecording();
				Thread.Sleep(100);
				Assert.IsTrue(x.IsRecording);
				x.StopRecordingAndSaveAsWav();
			}
		}

		[Ignore("Aborts with 'GC across AppDomain not allowed' IrrKlang.Play/Nunit incompatibility")]
		[Test]
		public void RecordThenPlay_SmokeTest()
		{
			using (var f = new TempFile())
			{
				var w = new BackgroundWorker();
				w.DoWork+=new DoWorkEventHandler((o,args)=> SystemSounds.Exclamation.Play());

				var x = AudioFactory.CreateAudioSession(f.Path);
				x.StartRecording();
				w.RunWorkerAsync();
				Thread.Sleep(1000);
				x.StopRecordingAndSaveAsWav();
				x.Play();
				Thread.Sleep(1000);
				x.StopPlaying();
			}
		}

		[Ignore("Aborts with 'GC across AppDomain not allowed' IrrKlang.Play/Nunit incompatibility")]
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
					w.DoWork += new DoWorkEventHandler((o, args) => SystemSounds.Exclamation.Play());

					var x = AudioFactory.CreateAudioSession(f.Path);
					x.StartRecording();
					w.RunWorkerAsync();
					Thread.Sleep(1000);
					x.StopRecordingAndSaveAsWav();

					var y = AudioFactory.CreateAudioSession(f.Path);
					y.Play();
					Thread.Sleep(1000);
					y.StopPlaying();
				}
			}
		}


		/// <summary>
		/// for testing things while recording is happening
		/// </summary>
		class RecordingSession:IDisposable
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
				Thread.Sleep(1000);//record a second
				_recorder.StopRecordingAndSaveAsWav();
			}

			public ISimpleAudioSession Recorder
			{
				get { return _recorder; }
			}

			public void Stop()
			{
				_recorder.StopRecordingAndSaveAsWav();
			}
			public void Dispose()
			{
				if (_recorder.IsRecording)
				{
					_recorder.StopRecordingAndSaveAsWav();
				}
				_tempFile.Dispose();
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

		[Ignore("IrrKlang Play not compatible with NUnit")]
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

		[Ignore("Aborts with 'GC across AppDomain not allowed' IrrKlang.Play/Nunit incompatibility")]
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
				ISimpleAudioSession x = RecordSomething(f);
				Assert.IsFalse(x.IsRecording);
			}
		}

		[Test]
		public void RecordThenStop_CanPlay_IsTrue()
		{
			using (var f = new TempFile())
			{
				ISimpleAudioSession x = RecordSomething(f);
				Assert.IsTrue(x.CanPlay);
			}
		}

		[Ignore("Aborts with 'GC across AppDomain not allowed' IrrKlang.Play/Nunit incompatibility")]
		[Test]
		public void RecordThenPlay_OK()
		{
			using (var f = new TempFile())
			{
				ISimpleAudioSession x = RecordSomething(f);
				x.Play();
				Thread.Sleep(100);	// Ensure file exists to be played.
				x.StopPlaying();
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

		[Ignore("Aborts with 'GC across AppDomain not allowed' IrrKlang.Play/Nunit incompatibility")]
		[Test]
		public void Play_DoesPlay ()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav"))
			{
				var x = AudioFactory.AudioSession (file.Path);
				Assert.DoesNotThrow( () => x.Play() );
				Assert.DoesNotThrow( () => x.StopPlaying() );
			}
		}

		[Test]
		public void Record_DoesRecord ()
		{

			using (var folder = new TemporaryFolder("Record_DoesRecord"))
			{
				string fpath = Path.Combine(folder.Path, "dump.ogg");
				var x = AudioFactory.AudioSession(fpath);
				Assert.DoesNotThrow(() => x.StartRecording());
				Assert.IsTrue(x.IsRecording);
				Thread.Sleep(1000);
				Assert.DoesNotThrow(() => x.StopRecordingAndSaveAsWav());
			}
		}
	}
}
