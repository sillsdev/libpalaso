using System.IO;
using System.Text;
using NUnit.Framework;
using SIL.CommandLineProcessing;
using SIL.PlatformUtilities;
using SIL.Progress;

namespace SIL.Tests.CommandLineProcessing
{
	[TestFixture]
	public class CommandLineRunnerTests
	{
		private string App { get; set; }

		// On Linux and macOS, a .NET Framework .exe has to be run with Mono, but a .NET (Core)
		// build produces a native app host, without an extension, that runs on its own.
		private static bool IsManagedProcess => Platform.IsMono;

		[OneTimeSetUp]
		public void TestFixtureSetup()
		{
			var testApp = Platform.IsWindows || Platform.IsMono
				? "SIL.CommandLineProcessing.TestApp.exe"
				: "SIL.CommandLineProcessing.TestApp";

			var testAssemblyFolder =
				Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			Assert.IsNotNull(testAssemblyFolder, "test setup problem");
			var appPath = Path.Combine(testAssemblyFolder, testApp);
			Assert.True(File.Exists(appPath), "test setup problem");
			App = appPath;
		}

		[Test]
		public void CommandWith10Line_NoCallbackOption_Get10LinesSynchronously()
		{
			var progress = new StringBuilderProgress();
			var result = CommandLineRunner.Run(App, "CommandLineRunnerTest", null, string.Empty, 100,
				progress, null, null, IsManagedProcess);
			Assert.IsTrue(result.StandardOutput.Contains("0"));
			Assert.IsTrue(result.StandardOutput.Contains("9"));
		}

		[Test]
		public void CommandWith10Line_NoCallbackOption_TimeoutAfter3s()
		{
			var progress = new StringBuilderProgress();
			var result = CommandLineRunner.Run(App, "CommandLineRunnerTest", null, string.Empty, 3,
				progress, null, null, IsManagedProcess);
			Assert.That(result.DidTimeOut, Is.True);
			Assert.That(result.StandardOutput, Is.Null);
			Assert.That(result.StandardError, Contains.Substring("Timed Out after waiting 3 seconds."));
		}

		[Test]
		public void CommandWith10Line_CallbackOption_Get10LinesAsynchronously()
		{
			var progress = new StringBuilderProgress();
			var linesReceivedAsynchronously = 0;
			var bldr = new StringBuilder();
			CommandLineRunner.Run(App, "CommandLineRunnerTest", null, string.Empty, 100,
				progress, s => bldr.AppendLine($"{++linesReceivedAsynchronously}: '{s}'"), null, IsManagedProcess);
			Assert.AreEqual(10, linesReceivedAsynchronously, bldr.ToString());
		}

		[Test]
		public void CommandWith10Line_CallbackOption_TimeoutAfter3s()
		{
			var progress = new StringBuilderProgress();
			int linesReceivedAsynchronously = 0;
			var result = CommandLineRunner.Run(App, "CommandLineRunnerTest", null, string.Empty, 3,
				progress, s => ++linesReceivedAsynchronously, null, IsManagedProcess);
			Assert.That(result.DidTimeOut, Is.True);
			Assert.That(result.StandardOutput, Is.Null);
			Assert.That(result.StandardError, Contains.Substring("Timed Out after waiting 3 seconds."));
		}
	}
}