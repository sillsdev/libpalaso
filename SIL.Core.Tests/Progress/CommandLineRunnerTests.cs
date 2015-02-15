using NUnit.Framework;
using SIL.CommandLineProcessing;
using SIL.Progress;

namespace SIL.Tests.Progress
{
	[TestFixture]
	public class CommandLineRunnerTests
	{

		[Test, Category("KnownMonoIssue")]
		[Platform(Exclude="Linux", Reason = "Test has problems on Mono")]
		public void CommandWith10Line_NoCallbackOption_Get10LinesSynchronously()
		{
			var app = "SIL.Windows.Forms.TestApp.exe";// FileLocator.GetFileDistributedWithApplication("PalasoUIWindowsForms.TestApp.exe");
			var progress = new StringBuilderProgress();
			var result = CommandLineRunner.Run(app, "CommandLineRunnerTest", null, string.Empty, 100, progress, null);
			Assert.IsTrue(result.StandardOutput.Contains("0"));
			Assert.IsTrue(result.StandardOutput.Contains("9"));
		}

		[Test, Category("KnownMonoIssue")]
		[Platform(Exclude="Linux", Reason = "Test has problems on Mono")]
		public void CommandWith10Line_CallbackOption_Get10LinesAsynchronously()
		{
			var app = "SIL.Windows.Forms.TestApp.exe";// FileLocator.GetFileDistributedWithApplication("PalasoUIWindowsForms.TestApp.exe");
			var progress = new StringBuilderProgress();
			int linesReceivedAsynchronously = 0;
			CommandLineRunner.Run(app, "CommandLineRunnerTest", null, string.Empty, 100, progress, s => { ++linesReceivedAsynchronously; });
			Assert.AreEqual(10, linesReceivedAsynchronously);
		}
	}
}