using System;
using System.Text;
using NUnit.Framework;

namespace SIL.Windows.Forms.Tests.Progress.LogBox
{
	[TestFixture]
	public class LogBoxTests
	{
		private Windows.Forms.Progress.LogBox progress;
		[Test]
		[Category("KnownMonoIssue")] // this test hangs on TeamCity for Linux
		public void ShowLogBox()
		{
			Console.WriteLine("Showing LogBox");
			using (var e = new LogBoxFormForTest())
			{
				progress = e.progress;
				progress.WriteMessage("LogBox test");
				progress.ShowVerbose = true;
				for (int i = 0; i < 1000; i++)
				{
					progress.WriteVerbose(".");
				}

				progress.WriteMessage("done");

				Console.WriteLine(progress.Text);
				Console.WriteLine(progress.Rtf);
				Console.WriteLine("Finished");
			}
		}

		[Test]
		[Category("Long Running")]
		public void WriteVerbose_AtMaximumLength_RtfContainsMaximumLengthExceeded()
		{
			Console.WriteLine("Showing LogBox");
			using (var e = new LogBoxFormForTest())
			{
				var sb = new StringBuilder();
				sb.Append('a', Int32.MaxValue / 1000 - 1);
				var hugestr = sb.ToString();
				progress = e.progress;
				for (int i = 0; i < 999; i++)
					progress.WriteVerbose(hugestr);
				sb.Clear();
				sb.Append('a', Int32.MaxValue - (hugestr.Length + 1) * 999 - 1);
				progress.WriteVerbose(sb.ToString());
				progress.WriteVerbose(".");
				Assert.IsFalse(progress.Rtf.Contains("."));
				Assert.IsTrue(progress.Rtf.Contains("Maximum length exceeded!"));
			}
		}

		[Test]
		[Category("KnownMonoIssue")] // this test hangs on TeamCity for Linux
		public void WriteVerbose_PartOfMessageExceedsMaximumLength_PartialMessageWrittenFollowedByMaximumLengthExceeded()
		{
			Console.WriteLine("Showing LogBox");
			using (var e = new LogBoxFormForTest())
			{
				var sb = new StringBuilder();
				progress = e.progress;
				var lengthOfBoxLabels = progress.Text.Length;
				sb.Append('a', 83 - progress.MaxLengthErrorMessage.Length);
				progress.MaxLength = 100;
				progress.WriteVerbose(sb.ToString());
				const string partThatWillFit = "Only this much.";
				progress.WriteVerbose($"{partThatWillFit}~will fit!");
				Assert.AreEqual(progress.MaxLength + lengthOfBoxLabels, progress.Text.Length);
				var iTruncatedMessage = progress.Rtf.IndexOf(partThatWillFit);
				Assert.IsTrue(iTruncatedMessage > 83);
				Assert.AreNotEqual("~", progress.Rtf[iTruncatedMessage + partThatWillFit.Length]);
				Assert.IsTrue(iTruncatedMessage < progress.Rtf.IndexOf(progress.MaxLengthErrorMessage));
			}
		}

		[Test]
		[Category("KnownMonoIssue")] // this test hangs on TeamCity for Linux
		public void WriteMessage_PartOfMessageExceedsMaximumLength_PartialMessageWrittenFollowedByMaximumLengthExceeded()
		{
			Console.WriteLine("Showing LogBox");
			using (var e = new LogBoxFormForTest())
			{
				var sb = new StringBuilder();
				progress = e.progress;
				var lengthOfBoxLabels = progress.Text.Length;
				sb.Append('a', 83 - progress.MaxLengthErrorMessage.Length);
				progress.MaxLength = 100;
				progress.WriteMessage(sb.ToString());
				const string partThatWillFit = "Only this much.";
				progress.WriteMessage($"{partThatWillFit}~will fit!");
				// Turns out that the Text property returns the text twice, once labeled "Box:" and once labeled "Verbose:"
				Assert.AreEqual(progress.MaxLength * 2 + lengthOfBoxLabels, progress.Text.Length);
				var iTruncatedMessage = progress.Rtf.IndexOf(partThatWillFit);
				Assert.IsTrue(iTruncatedMessage > 83);
				Assert.AreNotEqual("~", progress.Rtf[iTruncatedMessage + partThatWillFit.Length]);
				Assert.IsTrue(iTruncatedMessage < progress.Rtf.IndexOf(progress.MaxLengthErrorMessage));
			}
		}
	}
}
