using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Palaso.Progress;


namespace Palaso.Tests.Progress
{
	[TestFixture]
	public class ConsoleProgressTests
	{
		private static int _countForWork;
		private StringBuilder _logBuilder;

		[SetUp]
		public void Setup()
		{
			_logBuilder = new StringBuilder();
			_countForWork = 0;
		}

		[TearDown]
		public void TearDown()
		{
		}

		[Test]
		public void LongRunningMethodUsingConsoleHandler_ProducesLog()
		{
			Assert.IsFalse((_logBuilder.ToString().Contains("99")));
			ConsoleProgress progress = new ConsoleProgress();
			progress.Log += new EventHandler<ProgressState.LogEvent>(OnProgressStateLog);
			BackgroundWorker cacheBuildingWork = new BackgroundWorker();
			cacheBuildingWork.DoWork += new DoWorkEventHandler(OnDoBackgroundWorkerWork);
			cacheBuildingWork.RunWorkerAsync(progress);

			WaitForFinish(progress, ProgressState.StateValue.Finished);
			Assert.IsTrue(_logBuilder.ToString().Contains("99"));
		}

		private void WaitForFinish(ConsoleProgress progress, ProgressState.StateValue expectedResult)
		{
			DateTime giveUpTime = DateTime.Now.AddSeconds(5);
			while (progress.State != expectedResult)
			{
				if (DateTime.Now > giveUpTime)
				{
					Assert.Fail("Didn't get expected result");
				}
				Thread.Sleep(10);
			}
		}

		private void OnDoBackgroundWorkerWork(object sender, DoWorkEventArgs e)
		{
			ProgressState state = (ProgressState) e.Argument;
			state.StatusLabel = "working hard...";
			while (_countForWork < 100)
			{
				DoPretendWork();
				state.WriteToLog(_countForWork.ToString());
				_countForWork++;
				state.NumberOfStepsCompleted = _countForWork;
			}
			e.Result = ((ProgressState) e.Argument).State = ProgressState.StateValue.Finished;
		}

		private void DoPretendWork()
		{
			DateTime end = DateTime.Now.AddMilliseconds(2);
			while (DateTime.Now < end)
			{
				Thread.Sleep(1);
			}
		}

		private void OnProgressStateLog(object sender, ProgressState.LogEvent e)
		{
			_logBuilder.AppendLine(e.message);
		}
	}
}