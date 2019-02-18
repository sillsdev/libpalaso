using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SIL.Reporting;

namespace SIL.Linux.Logging.Tests
{
	[TestFixture]
	[Platform(Include = "Linux")]
	[Explicit("by hand only")] // Don't want to log to syslog during automated tests
	public class SyslogExceptionHandlerTests
	{
		public string FakeAppName = "FakeAppNameForTestingPurposes";
		public string SyslogFileToWatch = "/var/log/syslog";

		[Test]
		public void SyslogLogger_CatchesDivisionByZeroError()
		{
			var watcher = new SyslogWatcher(FakeAppName, SyslogFileToWatch);
			watcher.StartWatching();
			Thread errorThread = new Thread(DivideByZero);
			errorThread.Start();
			IEnumerable<string> data = watcher.WaitForData();
			Assert.That(data, Is.Not.Empty);
			Assert.That(data.First(), Is.StringContaining("Division by zero").Or.StringContaining("divide by zero"));
		}

		private void DivideByZero()
		{
			var handler = new SyslogExceptionHandler(FakeAppName);
			ExceptionHandler.Init(handler);
			int zero = 0;
			int i = 1/zero;
			System.Console.WriteLine(i);
		}
	}
}