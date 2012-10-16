using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Progress;

namespace Palaso.Tests.Progress
{
	[TestFixture]
	public class MultiProgressTests
	{
		[Test]
		public void ErrorsEncountered_ErrorOriginatesWithMultiProgress_BothHandlersHaveErrorsEncountered()
		{
			var multiProgress = new MultiProgress();
			var statusProgress = new StatusProgress();
			multiProgress.Add(statusProgress);
			var consoleProgress = new ConsoleProgress();
			multiProgress.AddMessageProgress(consoleProgress);
			multiProgress.WriteError("error!");
			Assert.That(consoleProgress.ErrorEncountered, Is.True);
			Assert.That(statusProgress.ErrorEncountered, Is.True);
		}

		[Test]
		public void ErrorsEncountered_ErrorOriginatesWithOneHandler_MultiProgressHasErrorsEncountered()
		{
			var multiProgress = new MultiProgress();
			var statusProgress = new StatusProgress();
			multiProgress.Add(statusProgress);
			var consoleProgress = new ConsoleProgress();
			multiProgress.AddMessageProgress(consoleProgress);
			statusProgress.WriteError("some error happened!");
			Assert.That(multiProgress.ErrorEncountered, Is.True);
		}
	}
}
