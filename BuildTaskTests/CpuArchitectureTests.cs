using System;
using Palaso.BuildTasks;
using NUnit.Framework;

namespace Palaso.BuildTask.Tests
{
	[TestFixture]
	public class CpuArchitectureTests
	{
		[Test]
		public void Test_CpuArchitecture()
		{
			var task = new CpuArchitecture();
			Assert.IsTrue(task.Execute());
			if (Environment.OSVersion.Platform == System.PlatformID.Unix)
			{
				if (Environment.Is64BitOperatingSystem)
					Assert.AreEqual("x86_64", task.Value);
				else
					Assert.AreEqual("i686", task.Value);
			}
			else
			{
				if (Environment.Is64BitOperatingSystem)
					Assert.AreEqual("x64", task.Value);
				else
					Assert.AreEqual("x86", task.Value);
			}
		}
	}
}
