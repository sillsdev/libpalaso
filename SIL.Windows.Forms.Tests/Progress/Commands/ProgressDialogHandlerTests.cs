using System;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Progress;
using SIL.Progress.Commands;
using SIL.Windows.Forms.Progress;
using SIL.Windows.Forms.Progress.Commands;

namespace SIL.Windows.Forms.Tests.Progress.Commands
{
	[TestFixture]
	public class ProgressDialogHandlerTests
	{
		private bool _commandFinishedCalled;
		private ProgressDialogHandler _progressHandler;
		private ProgressState _progressState;
		private TestCommand _command;
		private Form _dummyParentForm;

		[SetUp]
		public void Setup()
		{
			_dummyParentForm = new Form();
			_dummyParentForm.Name = "dummy form";
			_dummyParentForm.Text = "Dummy Form";
			_dummyParentForm.Show();
			Application.DoEvents();
			_command = new TestCommand();
			//_progressLog = "";
			_progressHandler = new ProgressDialogHandler(_dummyParentForm, _command);
			_progressState = new ProgressDialogProgressState(_progressHandler);
			_commandFinishedCalled = false;
		}

		[TearDown]
		public void TearDown()
		{
			WaitForHandlerToFinishUp();

			_dummyParentForm.Close();
			_dummyParentForm.Dispose();
		}


		[Test]
		public void EndsInFinishedState()
		{
		  _progressHandler.Finished += new EventHandler(OnCommandFinished);
			_command.BeginInvoke(_progressState);
			WaitOnProgressState(ref _progressState, ProgressState.StateValue.Finished);
		}

		[Test, Ignore("Not implemented")]
		public void InvokesFinishedCallback()
		{
			_progressHandler.Finished += new EventHandler(OnCommandFinished);
			_command.BeginInvoke(_progressState);
			WaitOnBool(ref _commandFinishedCalled);
		}

		[Test]
		public void OkIfNoEventsWired()
		{
			_command.BeginInvoke(_progressState);
		  //  _command.FinishBypassForTests += new EventHandler(_command_FinishBypassForTests);
			WaitOnProgressState(ref _progressState, ProgressState.StateValue.Finished);
		}


		[Test]
		public void OutputsLog()
		{
			_progressHandler.Finished += new EventHandler(OnCommandFinished);
			// _progressState.Log += new EventHandler<ProgressState.LogEvent>(OnProgressState_Log);
			_command.BeginInvoke(_progressState);
			WaitOnProgressState(ref _progressState, ProgressState.StateValue.Finished);
		}

		private void OnCommandFinished(object sender, EventArgs e)
		{
			_commandFinishedCalled = true;
		}


		private void WaitForHandlerToFinishUp()
		{
			DateTime giveUpTime = DateTime.Now.AddSeconds(5);
			while (!_progressHandler.TestEverythingClosedUp )
			{
				if (DateTime.Now > giveUpTime)
				{
					Assert.IsTrue(_progressHandler.TestEverythingClosedUp,"Gave up waiting");
				}
				Application.DoEvents();
			}
		}

		private static void WaitOnBool(ref bool waitForThisToBeTrue)
		{
			DateTime giveUpTime = DateTime.Now.AddSeconds(5);
			while (!waitForThisToBeTrue)
			{
				if (DateTime.Now > giveUpTime)
				{
					Assert.IsTrue(waitForThisToBeTrue);
				}
				Thread.Sleep(10);
			}
		}
		private static void WaitOnProgressState(ref ProgressState _progressState, ProgressState.StateValue expectedState)
		{
			DateTime giveUpTime = DateTime.Now.AddSeconds(5);
			while (_progressState.State != expectedState)
			{
				if (DateTime.Now > giveUpTime)
				{
					Assert.AreEqual(expectedState, _progressState.State);
				}
				//  Thread.Sleep(10);
				Application.DoEvents();
			}
		}
	}

	internal class TestCommand : BasicCommand
	{
		public bool wasCancelled = false;
		protected override void DoWork(InitializeProgressCallback initializeCallback, ProgressCallback progressCallback,
									   StatusCallback primaryStatusTextCallback,
									   StatusCallback secondaryStatusTextCallback)
		{
			int countForWork = 0;
			while (countForWork < 100)
			{
				if (Canceling)
				{
					wasCancelled = true;
					return;
				}
				DoPretendWork();
				countForWork++;
			}
		}

		private static void DoPretendWork()
		{


			DateTime end = DateTime.Now.AddMilliseconds(2);
			while (DateTime.Now < end)
			{
				Thread.Sleep(1);
			}
		}

		protected override void DoWork2(ProgressState progress)
		{

			int countForWork = 0;
			progress.TotalNumberOfSteps = 100;
			while (countForWork < 100)
			{
				DoPretendWork();
				countForWork++;
				progress.NumberOfStepsCompleted = countForWork;
			}
			progress.State = ProgressState.StateValue.Finished;
		}
	}
}