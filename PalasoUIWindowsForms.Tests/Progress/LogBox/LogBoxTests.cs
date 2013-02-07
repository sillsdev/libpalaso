using System;
using NUnit.Framework;
using Palaso.Progress.LogBox;
using Palaso.Progress;
using Palaso.UI.WindowsForms.Progress;
using System.ComponentModel;
using System.Threading;

namespace Palaso.Tests.Progress.LogBoxTests
{
	[TestFixture]
	public class LogBoxTests
	{
		private LogBox progress;
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
