using System;
using System.Threading;
using NUnit.Framework;
using SIL.Progress;
using SIL.Progress.Commands;

namespace SIL.Tests.Progress.Commands
{
	[TestFixture]
	public class CommandTests
	{
		private static int _countForWork;
		private bool _onFinishCalled;

		[SetUp]
		public void Setup()
		{
			_countForWork = 0;
			_onFinishCalled = false;
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
			TestCommand cmd = new TestCommand();
			cmd.Finish += new EventHandler(OnFinish);
			cmd.BeginInvoke();
			WaitOnBool(ref _onFinishCalled);
		}

		[Test]
		public void CommandCanBeCancelled()
		{
			TestCommand cmd = new TestCommand();
			cmd.BeginInvoke();
			Thread.Sleep(10);
			Assert.IsFalse(cmd.Enabled);
			cmd.Cancel();
			Assert.IsTrue(cmd.Canceling);
			WaitOnBool(ref cmd.wasCancelled);
			//no: it doesn't do this            Assert.IsTrue(cmd.Enabled);

			//todo: I can't see a way to know that it ended, if you cancel it.
			//finish isn't called. Should it be ?
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
					if(Canceling )
					{
						wasCancelled = true;
						return;
					}
					DoPretendWork();
					_countForWork++;
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
