// Copyright (c) 2016-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SIL.BuildTasks.Tests.Helper
{
	/// <summary>
	/// This class defines some tests used for testing the NUnitTask class
	/// </summary>
	[TestFixture]
	public class Tests
	{
		[Test]
		[Category("Success")]
		public void Success()
		{
			Assert.True(true, "This test always passes");
		}

		[Test]
		[Category("Failing")]
		public void Failing()
		{
			Assert.Fail("This test intentionally fails");
		}

		[Test]
		[Category("Exception")]
		public void Exception()
		{
			throw new ApplicationException("This test throws an exception");
		}

		[DllImport("ForceCrash")]
		private static extern void ForceCrash();

		[Test]
		[Category("Crash")]
		public void Crash()
		{
			ForceCrash();
			Assert.Fail("Should have crashed");
		}

		[DllImport("ForceCrash")]
		private static extern void OutputOnStderr();

		[Test]
		[Category("Stderr")]
		public void Stderr()
		{
			OutputOnStderr();
		}

		[Test]
		[Category("ErrorOnStdErr")]
		public void ErrorOnStdErr()
		{
			Console.Error.WriteLine("Error testing");
		}

		[Test]
		[Category("WarningOnStdErr")]
		public void WarningOnStdErr()
		{
			Console.Error.WriteLine("Just some warning");
		}
	}
}
