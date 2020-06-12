using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Reporting;
using SIL.Windows.Forms.Reporting;

namespace SIL.Windows.Forms.Tests.ErrorReporting
{

	[TestFixture]
	[Category("SkipOnTeamCity")]
	public class ErrorReportingTests
	{
		private bool Is64BitProcess;
		private bool HasLargePhysicalMemory;

		[OneTimeSetUp]
		public void FixtureSetUp()
		{
			Is64BitProcess = IntPtr.Size == 8;
			HasLargePhysicalMemory = MemoryManagement.GetMemoryInformation().TotalPhysicalMemory >= 8192000000L;
		}

		[Test]
		[Explicit("By hand only")]
		public void NotifyUserOfProblem_Message()
		{
			string message = "Oh no! This is quite a long message to see if it will wrap so I will have to keep typing to see if this will work now. And then some more.";
			ErrorReport.NotifyUserOfProblem(message);
		}

		[Test]
		[Explicit("By hand only")]
		public void NotifyUserOfProblem_OncePerSession()
		{
			ShowOncePerSessionBasedOnExactMessagePolicy.Reset();
			string message = "Oh no! This is quite a long message to see if it will wrap so I will have to keep typing to see if this will work now. And then some more.";
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);

			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);
		}

		[Test]
		[Explicit("By hand only")]
		public void NotifyUserOfProblem_WithAlternateButton()
		{
			ShowOncePerSessionBasedOnExactMessagePolicy.Reset();
			string message = "Oh no! This is quite a long message to see if it will wrap so I will have to keep typing to see if this will work now. And then some more.";
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
											"&Caller Defined",
											DialogResult.Cancel,
											message);
		}

		[Test]
		[Explicit("By hand only")]
		public void NotifyUserOfProblem_SmallMessage()
		{
			string message = "Oh no!";
			ErrorReport.NotifyUserOfProblem(message);
		}

		[Test]
		[Explicit("By hand only")]
		public void NotifyUserOfProblem_SmallWithAlternateButton()
		{
			ShowOncePerSessionBasedOnExactMessagePolicy.Reset();
			string message = "Oh no!";
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
											"&Caller Defined",
											DialogResult.Cancel,
											message);
		}

		[Test]
		[Explicit("By hand only")]
		public void NotifyUserOfProblem_ReallyLong()
		{
			string message = "Oh no! This is quite a long message to see if it will wrap so I will have to keep typing to see if this will work now. And then some more." +
				"And then keep going because I want to see what happens for a really long one," +
				"especially what happens with the resizing and if it works or not" + Environment.NewLine +
				Environment.NewLine + "and a newline as well.";
			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);

			ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(), message);
		}

		[Test]
		public void CheckMemory_NotMuchUsed_ReturnsFalse()
		{
			GC.Collect(); // In case another test (e.g, CheckMemory_1GUsed_ReturnsTrue) just used a lot
			Assert.That(MemoryManagement.CheckMemory(true, "not much done", false), Is.False);
		}

		[Test]
		public void CheckMemory_1GUsed_ReturnsTrue()
		{
			// We'll grab some big chunks but not demand we can get it all. Keep them small enough
			// to stay out of Large Object Heap
			var desiredNumberOfChunks = Is64BitProcess && HasLargePhysicalMemory ? 42000 : 21000;
			var chunks = new List<byte[]>();
			for (int i = 0; i < desiredNumberOfChunks; i++)
				chunks.Add(new byte[50000]);
			Assert.That(MemoryManagement.CheckMemory(true, "not much done", false), Is.True,
				"CheckMemory didn't detect danger");
		}

		[Test]
		[Explicit("By hand only")]
		public void CheckMemory_1GUsed_DisplaysDialogOnlyOnce()
		{
			// We'll grab some big chunks but not demand we can get it all. Keep them small enough to stay out of Large Object Heap
			var chunks = new List<byte[]>();
			for (int i = 0; i < 21000; i++)
				chunks.Add(new byte[50000]);
			Assert.That(MemoryManagement.CheckMemory(true, "not much done", true), Is.True);
			// Doing it again should still return true, but you should NOT see the dialog twice.
			Assert.That(MemoryManagement.CheckMemory(true, "not much done", true), Is.True);
		}

		[Test]
		public void CheckMemory_WritesPlausibleNumbersToLogger()
		{
			Logger.Init();
			var dummy = Logger.LogText; // counter-intuitive, but the only way I can find to clear out minor events, in case anything else used logger
			MemoryManagement.CheckMemory(true, "this is a test", false); // since not doing GC we really can't predict the return value
			var result = Logger.MinorEventsLog;
			Assert.That(result, Does.Contain("this is a test"));
			var re = new Regex(@"\d+,\d+K");
			var matches = re.Matches(result).Cast<Match>().ToArray();
			// This is a pretty weak test; just proves we're outputting at least 3 numbers over 1000K. (Heap memory is sometimes <1M; virtual is unknown on Linux)
			Assert.That(matches, Has.Length.AtLeast(3));
			Logger.ShutDown();
			Logger.Init();
			MemoryManagement.CheckMemory(false, "this is a test", false); // since not doing GC we really can't predict the return value
			result = Logger.LogText;
			Assert.That(result, Does.Contain("this is a test"));
			matches = re.Matches(result).Cast<Match>().ToArray();
			// Using WriteEvent means out data is in the eventual output twice.
			Assert.That(matches, Has.Length.AtLeast(6));
		}

		[Test]
		public void GetMemoryInformation_ReturnsMemoryInformation()
		{
			var memInfo = MemoryManagement.GetMemoryInformation();
			Assert.Greater(memInfo.TotalPhysicalMemory, 0);
			Assert.Greater(memInfo.TotalVirtualMemory, 0);
		}
	}
}
