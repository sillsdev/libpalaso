using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.IO;
using SIL.Media.Naudio;

namespace SIL.Media.Tests
{
	[TestFixture]
	[Category("AudioTests")]
	[Category("RequiresAudioInputDevice")]
	public class AudioRecorderTests
	{
		private RecordingDevice _defaultRecordingDevice;

		[OneTimeSetUp]
		public void SetUpFixture()
		{
			_defaultRecordingDevice = RecordingDevice.Devices.FirstOrDefault() as RecordingDevice;
			Assert.That(_defaultRecordingDevice, Is.Not.Null,
				"These tests require a microphone.");
		}

		[Test]
		public void BeginRecording_OnNonUiThread_Throws()
		{
			using (var f = new TempFile())
			{
				using (var recorder = new AudioRecorder(1))
				{
					recorder.SelectedDevice = _defaultRecordingDevice;
					Assert.That(() => recorder.BeginRecording(f.Path, false), Throws.Exception);
				}
			}
		}

		[Test]
		public void BeginRecording_ThenStop_RecordingSavedToFile()
		{
			using (var ctrl = new Control())
			{
				bool recordingStopped = false;
				var f = new TempFile();
				try
				{
					var recorder = new AudioRecorder(1);
					// Note: BeginRecording
					ctrl.HandleCreated += delegate
					{
						recorder.SelectedDevice = _defaultRecordingDevice;
						recorder.BeginRecording(f.Path, false);
						Thread.Sleep(100);
						recorder.Stop();
					};

					recorder.Stopped += delegate
					{
						Assert.That(f.Path, Does.Exist);
						Assert.That(new FileInfo(f.Path).Length, Is.GreaterThan(0));
						recordingStopped = true;
						recorder.Dispose();
					};

					ctrl.CreateControl();

					for (var i = 0; i < 3 && !recordingStopped; i++)
					{
						Thread.Sleep(90);
						Application.DoEvents();
					}

					Assert.That(recordingStopped);
				}
				finally
				{
					f.Dispose();
				}
			}
		}

		[Test]
		public void BeginRecording_WhileRecording_ThrowsInvalidOperationException()
		{
			using (var ctrl = new Control())
			{
				var f = new TempFile();
				var recorder = new AudioRecorder(1);
				try
				{
					// Note: BeginRecording
					ctrl.HandleCreated += delegate
					{
						recorder.SelectedDevice = _defaultRecordingDevice;
						recorder.BeginRecording(f.Path, false);
					};

					ctrl.CreateControl();

					Thread.Sleep(10);
					Assert.That(() => recorder.BeginRecording("blah.wav"), Throws.InvalidOperationException);

				}
				finally
				{
					recorder.Dispose();
					f.Dispose();
				}
			}
		}

		[Test]
		public void BeginMonitoring_WhileRecording_ThrowsInvalidOperationException()
		{
			using (var ctrl = new Control())
			{
				var f = new TempFile();
				var recorder = new AudioRecorder(1);
				try
				{
					// Note: BeginRecording
					ctrl.HandleCreated += delegate
					{
						recorder.SelectedDevice = _defaultRecordingDevice;
						recorder.BeginRecording(f.Path, false);
					};

					ctrl.CreateControl();

					Thread.Sleep(10);
					Assert.That(() => recorder.BeginMonitoring(), Throws.InvalidOperationException);

				}
				finally
				{
					recorder.Dispose();
					f.Dispose();
				}
			}
		}

		[Test]
		public void BeginRecording_OnNonUiThreadAfterMonitoringStartedOnUIThread_NoException()
		{
			using (var ctrl = new Control())
			{
				bool monitoringStarted = false;
				var f = new TempFile();
				var recorder = new AudioRecorder(1);
				try
				{
					ctrl.HandleCreated += delegate
					{
						recorder.SelectedDevice = _defaultRecordingDevice;
						recorder.BeginMonitoring();
						monitoringStarted = true;
					};

					ctrl.CreateControl();

					// Note: This loop appears to be unnecessary. CreateControl apparently does not
					// return until the code in the HandleCreated handler executes to completion.
					// But in case this ever fails, try uncommenting this code to get it to pass.
					//for (var i = 0; i < 3 && !monitoringStarted; i++)
					//{
					//	Thread.Sleep(20);
					//	Application.DoEvents();
					//}
					Assert.That(monitoringStarted);

					recorder.BeginRecording(f.Path);
					Thread.Sleep(100);
					recorder.Stop();
					Thread.Sleep(100);
				}
				finally
				{
					recorder.Dispose();
					f.Dispose();
				}
			}
		}
	}
}
