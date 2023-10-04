using System.IO;
using System.Text;
using NUnit.Framework;
using SIL.CommandLineProcessing;
using SIL.Progress;

namespace SIL.Tests.CommandLineProcessing
{
	[TestFixture]
	public class CommandLineRunnerTests
	{
		private string App { get; set; }

		[OneTimeSetUp]
		public void TestFixtureSetup()
		{
			const string kTestApp = "SIL.Windows.Forms.TestApp.exe";

			var testAssemblyFolder =
				Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			Assert.IsNotNull(testAssemblyFolder, "test setup problem");
			var appPath = Path.Combine(testAssemblyFolder, kTestApp);
			Assert.True(File.Exists(appPath), "test setup problem");
			App = appPath;
		}

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

		// Note: this test was formerly categorized as "ByHand", but since it usually works (on Windows) and since
		// we don't exclude the "ByHand" category on TeamCity for libpalaso tests anyway, I removed that category.
		[Test]
		[Platform(Exclude = "Linux", Reason = "See comment in test")]
		public void CommandWith10Line_CallbackOption_Get10LinesAsynchronously()
		{
			var progress = new StringBuilderProgress();
			var linesReceivedAsynchronously = 0;
			var bldr = new StringBuilder();
			CommandLineRunner.Run(App, "CommandLineRunnerTest", null, string.Empty, 100,
				progress, s => bldr.AppendLine($"{++linesReceivedAsynchronously}: '{s}'"), null, true);
			// The test fails on Linux because progress gets called 10x for StdOutput plus
			// 1x for StdError (probably on the closing of the stream), so linesReceivedAsync is 11.
			// It also fails about 4% of the time on TC Windows agents
			Assert.AreEqual(10, linesReceivedAsynchronously, bldr.ToString());
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