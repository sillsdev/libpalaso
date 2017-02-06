using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Threading;
using NUnit.Framework;
using Palaso.IO;
using Palaso.TestUtilities;
using Palaso.Media.Tests.Properties;


namespace Palaso.Media.Tests
{
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
		Assert.Throws<ApplicationException>(() =>
x.StopRecordingAndSaveAsWav());
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
			Assert.Throws<ApplicationException>(() =>
session.Recorder.Play());
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
	   [Platform(Exclude="Windows", Reason="IrrKlang doesn't throw, so we don't really know")]
	   public void Play_FileEmpty_Throws()
	   {
		   using (var f = new TempFile())
		   {
			   var x = AudioFactory.CreateAudioSession(f.Path);
			   Assert.Throws<Exception>(() =>
 x.Play());
		   }
	   }

	   [Test]
	   public void Play_FileDoesNotExist_Throws()
	   {
		   var x = AudioFactory.CreateAudioSession(Path.GetRandomFileName());
			   Assert.Throws<FileNotFoundException>(() =>
x.Play());
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
			Assert.Throws<ApplicationException>(() =>
session.Recorder.StartRecording());
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

	   [Test]
		public void Play_DoesPlay ()
		{
			using (var file = TempFile.FromResource(Resources.finished, ".wav")) {
				var x = AudioFactory.CreateAudioSession (file.Path);
				Assert.DoesNotThrow( () => x.Play() );
				Assert.DoesNotThrow( () => x.StopPlaying() );
			}
	   }

	   [Test]
		public void Record_DoesRecord ()
		{

			string fpath = "/tmp/dump.ogg";
			var x = AudioFactory.CreateAudioSession (fpath);
			Assert.DoesNotThrow( () => x.StartRecording() );
			Assert.IsTrue(x.IsRecording);
			Thread.Sleep(1000);
			Assert.DoesNotThrow( () => x.StopRecordingAndSaveAsWav() );
	   }
	}
}
