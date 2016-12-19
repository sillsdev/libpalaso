// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Build.Evaluation;
using NUnit.Framework;
using Palaso.PlatformUtilities;

namespace Palaso.BuildTask.Tests.UnitTestTasks
{
	[TestFixture]
	[Category("SkipOnTeamCity")]
	public class NUnitTests
	{
		private static string OutputDirectory
		{
			get
			{
				return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			}
		}

		private static string GetBuildFilename(string category)
		{
			var buildFile = Path.GetTempFileName();
			File.WriteAllText(buildFile, string.Format(@"
<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<UsingTask TaskName='NUnit' AssemblyFile='{0}' />
	<Target Name='Test'>
		<NUnit Assemblies='{1}' ToolPath='{2}' TestInNewThread='false' Force32Bit='{4}'
			IncludeCategory='{3}' Verbose='true' />
	</Target>
</Project>",
				Path.Combine(OutputDirectory, "Palaso.BuildTasks.dll"),
				Path.Combine(OutputDirectory, "Palaso.BuildTask.Tests.Helper.dll"),
				Path.Combine(OutputDirectory, "..", "..", "packages", "NUnit.Runners.Net4.2.6.4", "tools"),
				category, Platform.IsWindows));

			return buildFile;
		}

		[Test]
		public void Success_DoesntFailBuild()
		{
			var xmlReader = new XmlTextReader(GetBuildFilename("Success"));
			var project = new Project(xmlReader);
			var result = project.Build("Test");
			Assert.That(result, Is.True);
		}

		[Test]
		public void FailingTests_DoesntFailBuild()
		{
			var xmlReader = new XmlTextReader(GetBuildFilename("Failing"));
			var project = new Project(xmlReader);
			var result = project.Build("Test");
			Assert.That(result, Is.True);
		}

		[Test]
		public void Exception_FailsTestButNotBuild()
		{
			var xmlReader = new XmlTextReader(GetBuildFilename("Exception"));
			var project = new Project(xmlReader);
			var result = project.Build("Test");
			Assert.That(result, Is.True);
		}

		[Test]
		[Platform(Exclude = "Linux",Reason = "We don't get an exception from finalizer; probably runs too late")]
		public void ExceptionInFinalizer_FailsBuild()
		{
			var xmlReader = new XmlTextReader(GetBuildFilename("Finalizer"));
			var project = new Project(xmlReader);
			var result = project.Build("Test");
			Assert.That(result, Is.False);
		}
	}
}
