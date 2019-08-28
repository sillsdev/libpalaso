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
		[Category("LongRunning")]
		[Category("SkipOnTeamCity")]
		public void WriteVerbose_AtMaximumLength_RtfContainsMaximumLengthExceeded()
		{
			Console.WriteLine("Showing LogBox");
			using (var e = new LogBoxFormForTest())
			{
				progress = e.progress;
				var sb = new StringBuilder();
				sb.Append('a', progress.MaxLength / 1000 - 1);
				var hugestr = sb.ToString();
				for (int i = 0; i < 999; i++)
					progress.WriteVerbose(hugestr);
				sb.Clear();
				sb.Append('a', progress.MaxLength - (hugestr.Length + 1) * 999 - 1);
				progress.WriteVerbose(sb.ToString());
				progress.WriteVerbose(".");
				Assert.IsTrue(progress.ErrorEncountered);
				Assert.IsFalse(progress.Rtf.Contains("."));
				Assert.IsTrue(progress.Rtf.Contains("Maximum length exceeded!"));
			}
		}

		[Test]
		[Category("KnownMonoIssue")] // this test hangs on TeamCity for Linux
		public void WriteVerbose_MessagesExceedMaximumLength_ErrorMessageOnlyWrittenOnceToTerseLogBox()
		{
			Console.WriteLine("Showing LogBox");
			using (var e = new LogBoxFormForTest())
			{
				var sb = new StringBuilder();
				progress = e.progress;
				sb.Append('a', 22);
				progress.MaxLength = 200;
				for (int i = 0; i < 13; i++)
				{
					progress.WriteVerbose(sb.ToString());
					if (i % 5 == 0)
						progress.WriteMessage("No problem.");
				}

				Assert.IsTrue(progress.ErrorEncountered);
				var textTerse = progress.Text;
				progress.ShowVerbose = true;
				var textVerbose = progress.Text;
				Assert.AreEqual(progress.MaxLength, textVerbose.Length);
				Assert.IsTrue(textTerse.Length < textVerbose.Length);
				var iFirst = textTerse.IndexOf(progress.MaxLengthErrorMessage);
				int iEndOfErrorMessageInTerseBox = iFirst + progress.MaxLengthErrorMessage.Length;
				Assert.IsTrue(textTerse.Length > iEndOfErrorMessageInTerseBox);
				Assert.AreEqual(-1, textTerse.IndexOf(progress.MaxLengthErrorMessage, iEndOfErrorMessageInTerseBox));
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
				progress.ShowVerbose = true;
				sb.Append('a', 83 - progress.MaxLengthErrorMessage.Length);
				progress.MaxLength = 100;
				progress.WriteVerbose(sb.ToString());
				const string partThatWillFit = "Only this much.";
				progress.WriteVerbose($"{partThatWillFit}~will fit!");
				Assert.IsTrue(progress.ErrorEncountered);
				Assert.AreEqual(progress.MaxLength, progress.Text.Length);
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
				sb.Append('a', 83 - progress.MaxLengthErrorMessage.Length);
				progress.MaxLength = 100;
				progress.WriteMessage(sb.ToString());
				const string partThatWillFit = "Only this much.";
				progress.WriteMessage($"{partThatWillFit}~will fit!");
				Assert.IsTrue(progress.ErrorEncountered);
				Assert.IsTrue(progress.Text.EndsWith(progress.MaxLengthErrorMessage));
				progress.ShowVerbose = true;
				Assert.AreEqual(progress.MaxLength, progress.Text.Length);
				var iTruncatedMessage = progress.Rtf.IndexOf(partThatWillFit);
				Assert.IsTrue(iTruncatedMessage > 83);
				Assert.AreNotEqual("~", progress.Rtf[iTruncatedMessage + partThatWillFit.Length]);
				Assert.IsTrue(iTruncatedMessage < progress.Rtf.IndexOf(progress.MaxLengthErrorMessage));
			}
		}
	}
}
