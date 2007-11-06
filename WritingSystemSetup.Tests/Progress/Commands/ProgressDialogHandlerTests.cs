using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.Progress;
using Palaso.UI.WindowsForms.Progress;

namespace PalasoUIWindowsForms.Tests.Progress
{
	[TestFixture]
	public class ProgressDialogHandlerTests
	{
		private StringBuilder _logBuilder;
		private bool _commandFinishedCalled;
		private ProgressDialogHandler _progressHandler;
		private ProgressState _progressState;
		private TestCommand _command;

		[SetUp]
		public void Setup()
		{
			_logBuilder = new StringBuilder();
			Form dummyParentForm = new Form();
			dummyParentForm.Show();
			_command = new TestCommand();
			//_progressLog = "";
			_progressHandler = new ProgressDialogHandler(dummyParentForm, _command);
			_progressState = new ProgressDialogProgressState(_progressHandler);
			_commandFinishedCalled = false;
		}

		[TearDown]
		public void TearDown()
		{
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


		private void OnProgressStateLog(object sender, ProgressState.LogEvent e)
		{
			_logBuilder.AppendLine(e.message);
		}

		private void WaitOnBool(ref bool waitForThisToBeTrue)
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
				Thread.Sleep(10);
			}
		}
	}

	internal class TestCommand : BasicCommand
	{
		public bool wasCancelled = false;
		public event EventHandler FinishBypassForTests;

		protected override void DoWork(InitializeProgressCallback initializeCallback, ProgressCallback progressCallback,
									   StatusCallback primaryStatusTextCallback,
									   StatusCallback secondaryStatusTextCallback)
		{
			int countForWork = 0;
			while (countForWork < 100)
			{
				if (this.Canceling)
				{
					wasCancelled = true;
					return;
				}
				DoPretendWork();
				countForWork++;
			}
		}

		private void DoPretendWork()
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