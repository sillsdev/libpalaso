using System;
using NUnit.Framework;

namespace SIL.Windows.Forms.Tests.Progress.LogBox
{
	[TestFixture]
	public class LogBoxTests
	{
		private Windows.Forms.Progress.LogBox progress;
		[Test]
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
	}
}
