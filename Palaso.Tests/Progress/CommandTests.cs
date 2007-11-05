using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Palaso.Progress;


namespace Palaso.Tests.Progress
{
	[TestFixture]
	public class CommandTests
	{
		private static int _countForWork;
		private StringBuilder _logBuilder;
		private bool _onInitializeCalled;
		private bool _onFinishCalled;
		private bool _beginCancelCalled;

		[SetUp]
		public void Setup()
		{
			_logBuilder = new StringBuilder();
			_countForWork = 0;
			_onFinishCalled = false;
			_beginCancelCalled = false;
		}

		[TearDown]
		public void TearDown()
		{
		}

		[Test]
		public void CommandNotCancellingBeforeStarting()
		{
			TestCommand cmd= new TestCommand();
			Assert.IsFalse(cmd.Canceling);
		}

		[Test]
		public void CommandEnabledByDefault()
		{
			TestCommand cmd = new TestCommand();
			Assert.IsTrue(cmd.Enabled);
		}

		/* It is up to the concrete subclass of command to call Initialize, so no reason to test it here.
		 * [Test]
				public void CommandCallsInitializeCallback()
				{
					_onInitializeCalled = false;
					TestCommand cmd = new TestCommand();
					cmd.InitializeCallback = OnInitialize;
					cmd.BeginInvoke();
					WaitOnBool(ref _onInitializeCalled);
				}
		*/
		[Test]
		public void CommandCallsFinalize()
		{
			_onInitializeCalled = false;
			TestCommand cmd = new TestCommand();
			cmd.Finish += new EventHandler(OnFinish);
			cmd.BeginInvoke();
			WaitOnBool(ref _onFinishCalled);
		}

		[Test]
		public void CommandCanBeCancelled()
		{
			_onInitializeCalled = false;
			TestCommand cmd = new TestCommand();
			cmd.BeginCancel += new EventHandler(OnBeginCancel);
			cmd.BeginInvoke();
			Thread.Sleep(10);
			Assert.IsFalse(cmd.Enabled);
			cmd.Cancel();
			Assert.IsTrue(cmd.Canceling);
			//WaitOnBool(ref _onFinishCalled);
			Thread.Sleep(10);
			Assert.IsTrue(cmd.wasCancelled);
//no: it doesn't do this            Assert.IsTrue(cmd.Enabled);

			//todo: I can't see a way to know that it ended, if you cancel it.
			//finish isn't called. Should it be?
		}

		private void OnBeginCancel(object sender, EventArgs e)
		{
			_beginCancelCalled = true;
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

		void OnFinish(object sender, EventArgs e)
		{
			_onFinishCalled = true;
		}

//        private void OnInitialize(int minimum, int maximum)
//        {
//            _onInitializeCalled=true;
//        }

		internal class TestCommand : BasicCommand
		{
			public bool wasCancelled = false;

			protected override void DoWork(InitializeProgressCallback initializeCallback, ProgressCallback progressCallback,
										   StatusCallback primaryStatusTextCallback,
										   StatusCallback secondaryStatusTextCallback)
			{
				while (_countForWork < 100)
				{
					if(this.Canceling )
					{
						wasCancelled = true;
						return;
					}
					DoPretendWork();
					_countForWork++;
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
				progress.TotalNumberOfSteps = 100;
				while (_countForWork < 100)
				{
					DoPretendWork();
					_countForWork++;
					progress.NumberOfStepsCompleted = _countForWork;
				}
				progress.State = ProgressState.StateValue.Finished;
			}
		}

	}
}
