using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SIL.Progress;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace SIL.Tests.Progress
{
	[TestFixture]
	public class ConsoleProgressTests
	{
		private static int _countForWork;
		private StringWriter _logBuilder;

		[SetUp]
		public void Setup()
		{
			_logBuilder = new StringWriter();
			_countForWork = 0;
		}

		[TearDown]
		public void TearDown()
		{
			_logBuilder.Dispose();
		}

		[Test]
		[Category("ByHand")]
		[Explicit]
		public void LongRunningMethodUsingConsoleHandler_ProducesLog()
		{
			var progress = new ConsoleProgress { ProgressIndicator = new NullProgressIndicator() };
			Console.SetOut(_logBuilder);
			Console.SetError(_logBuilder);
			using (var cacheBuildingWork = new BackgroundWorker())
			{
				cacheBuildingWork.DoWork += OnDoBackgroundWorkerWork;
				cacheBuildingWork.RunWorkerAsync(progress);

				WaitForFinish(progress);
				// Fails about 2% of the time on TC Windows agents, likely due to the Asynchronous aspects of this test
				Assert.That(_logBuilder.ToString(), Does.Contain("99"));
			}
		}

		private static void WaitForFinish(IProgress progress)
		{
			var giveUpTime = DateTime.Now.AddSeconds(5);
			while (progress.ProgressIndicator.PercentCompleted < 100)
			{
				if (DateTime.Now > giveUpTime)
				{
					Assert.Fail("Didn't get expected result");
				}
				Thread.Sleep(10);
			}
		}

		private static void OnDoBackgroundWorkerWork(object sender, DoWorkEventArgs e)
		{
			var progress = (IProgress) e.Argument;
			progress.WriteStatus("working hard...");
			while (_countForWork < 100)
			{
				DoPretendWork();
				progress.WriteMessage(_countForWork.ToString());
				_countForWork++;
				progress.ProgressIndicator.PercentCompleted = _countForWork;
			}
			e.Result = true;
		}

		private static void DoPretendWork()
		{
			Thread.Sleep(2);
		}

	}
}