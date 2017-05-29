using NUnit.Framework;
using SIL.CommandLineProcessing;
using SIL.Progress;

namespace SIL.Tests.CommandLineProcessing
{
	[TestFixture]
	public class CommandLineRunnerTests
	{
		private const string App = "SIL.Windows.Forms.TestApp.exe";

		[Test]
		public void CommandWith10Line_NoCallbackOption_Get10LinesSynchronously()
		{
			var progress = new StringBuilderProgress();
			var result = CommandLineRunner.Run(App, "CommandLineRunnerTest", null, string.Empty, 100,
				progress, null, null, true);
			Assert.IsTrue(result.StandardOutput.Contains("0"));
			Assert.IsTrue(result.StandardOutput.Contains("9"));
		}

		[Test]
		public void CommandWith10Line_NoCallbackOption_TimeoutAfter3s()
		{
			var progress = new StringBuilderProgress();
			var result = CommandLineRunner.Run(App, "CommandLineRunnerTest", null, string.Empty, 3,
				progress, null, null, true);
			Assert.That(result.DidTimeOut, Is.True);
			Assert.That(result.StandardOutput, Is.Null);
			Assert.That(result.StandardError, Contains.Substring("Timed Out after waiting 3 seconds."));
		}

		[Test, Category("ByHand")]
		[Platform(Exclude = "Linux", Reason = "See comment in test")]
		public void CommandWith10Line_CallbackOption_Get10LinesAsynchronously()
		{
			var progress = new StringBuilderProgress();
			int linesReceivedAsynchronously = 0;
			CommandLineRunner.Run(App, "CommandLineRunnerTest", null, string.Empty, 100,
				progress, s => ++linesReceivedAsynchronously, null, true);
			// The test fails on Linux because progress gets called 10x for StdOutput plus
			// 1x for StdError (probably on the closing of the stream), so linesReceivedAsync is 11.
			// It also failes about 4% of the time on TC Windows agents
			Assert.AreEqual(10, linesReceivedAsynchronously);
		}

		[Test]
		public void CommandWith10Line_CallbackOption_TimeoutAfter3s()
		{
			var progress = new StringBuilderProgress();
			int linesReceivedAsynchronously = 0;
			var result = CommandLineRunner.Run(App, "CommandLineRunnerTest", null, string.Empty, 3,
				progress, s => ++linesReceivedAsynchronously, null, true);
			Assert.That(result.DidTimeOut, Is.True);
			Assert.That(result.StandardOutput, Is.Null);
			Assert.That(result.StandardError, Contains.Substring("Timed Out after waiting 3 seconds."));
		}
	}
}