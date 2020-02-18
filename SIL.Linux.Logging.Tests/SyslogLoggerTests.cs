using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace SIL.Linux.Logging.Tests
{
	/// <summary>
	/// CAUTION: These tests assume that the machine you're running them on logs its messages
	/// to /var/log/syslog. If that's not the case, all the tests will fail!
	/// </summary>
	[TestFixture]
	[Platform(Include="Linux")]
	[Explicit("by hand only")] // Don't want to log to syslog during automated tests
    public class SyslogLoggerTests
	{
		public string FakeAppName = "FakeAppNameForTestingPurposes";

		/// <summary>
		/// If you need to watch a different file rather than /var/log/syslog, change this field.
		/// </summary>
		public string SyslogFileToWatch = "/var/log/syslog";

		[Test]
		public void SyslogLogger_WritesOneLine()
		{
			var watcher = new SyslogWatcher(FakeAppName, SyslogFileToWatch);
			var logger = new SyslogLogger(FakeAppName);
			watcher.StartWatching();
			logger.Error("This string should be in the results");
			IEnumerable<string> data = watcher.WaitForData();
			watcher.StopWatching();
			Assert.That(data, Has.Some.Contains("This string should be in the results"));
		}

		[Test]
		public void SyslogLogger_LogMany_WritesManyLines()
		{
			var watcher = new SyslogWatcher(FakeAppName, SyslogFileToWatch);
			var logger = new SyslogLogger(FakeAppName);
			watcher.StartWatching();
			string[] messages = 
			{
				"First log message",
				"Second log message",
				"Third log message",
				"Fourth log message",
				"Fifth log message"
			};
			logger.LogMany(SyslogPriority.Error, messages);
			string[] data = watcher.WaitForData(5).ToArray();
			watcher.StopWatching();
			for (int i = 0; i < messages.Length; i++)
				Assert.That(data[i], Does.Contain(messages[i]));
		}

		[Test]
		public void SyslogLogger_TwoLoggers_WillNotInterefereWithEachOther()
		{
			var watcher = new SyslogWatcher(FakeAppName, SyslogFileToWatch);
			var logger1 = new SyslogLogger(FakeAppName + "1");
			var logger2 = new SyslogLogger(FakeAppName + "2");
			watcher.StartWatching();
			string[] messages = 
			{
				"First log message",
				"Second log message",
				"Third log message",
				"Fourth log message",
				"Fifth log message"
			};
			foreach (string s in messages)
			{
				logger1.Error(s);
				logger2.Error(s);
			}
			string[] data = watcher.WaitForData(10).ToArray();
			watcher.StopWatching();
			for (int i = 0; i < messages.Length; i++)
			{
				// Expect messages perfectly interleaved, logger1 followed by logger2 all five times
				string expectedAppName = FakeAppName + (i % 2 == 0 ? "1" : "2");
				Assert.That(data[i], Does.Contain(expectedAppName));
				Assert.That(data[i], Does.Contain(messages[i / 2]));
			}
		}

		[Test]
		public void SyslogLogger_TwoLogManyCalls_WillRunSequentially()
		{
			var watcher = new SyslogWatcher(FakeAppName, SyslogFileToWatch);
			var logger1 = new SyslogLogger(FakeAppName + "1");
			var logger2 = new SyslogLogger(FakeAppName + "2");
			watcher.StartWatching();
			string[] messages = 
			{
				"First log message",
				"Second log message",
				"Third log message",
				"Fourth log message",
				"Fifth log message"
			};
			logger1.LogMany(SyslogPriority.Error, messages);
			logger2.LogMany(SyslogPriority.Error, messages);
			string[] data = watcher.WaitForData(10).ToArray();
			watcher.StopWatching();
			for (int i = 0; i < messages.Length; i++)
			{
				// Expect messages in sequence, logger1's five lines followed by logger2's five lines
				string expectedAppName = FakeAppName + (i / 5 == 0 ? "1" : "2");
				Assert.That(data[i], Does.Contain(expectedAppName));
				Assert.That(data[i], Does.Contain(messages[i % 5]));
			}
		}
	}
}
