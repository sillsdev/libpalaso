// Copyright (c) 2016-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Build.Evaluation;
using NUnit.Framework;

namespace SIL.BuildTasks.Tests.UnitTestTasks
{
	[TestFixture]
	public class NUnitTests
	{
		private static string OutputDirectory => Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

		private static string _nunitDir;

		private static string NUnitDir
		{
			get
			{
				if (string.IsNullOrEmpty(_nunitDir))
				{
					CopyNUnit();
				}

				return _nunitDir;
			}
		}

		private static void CopyNUnit()
		{
			_nunitDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(_nunitDir);
			var sourceDir = Path.Combine(OutputDirectory, "..", "..", "packages",
				"NUnit.Runners.Net4.2.6.4", "tools");
			foreach (var file in Directory.EnumerateFiles(sourceDir, "*.*",
				SearchOption.AllDirectories))
			{
				var dir = Path.GetDirectoryName(file);
				if (dir != null && dir.EndsWith("addins"))
				{
					// skip addins. This is required for TeamCity where we might use the
					// TC addin to report the progress of the tests. However, for
					// the unit tests we run as a subprocess we don't want the addin
					// so that we don't get the intentionally failing tests of the
					// subprocess reported as failed.
					continue;
				}

				var relativeDir = dir.Substring(sourceDir.Length).TrimStart('\\', '/');
				var targetDir = Path.Combine(_nunitDir, relativeDir);
				Directory.CreateDirectory(targetDir);
				File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)));
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
				Path.Combine(OutputDirectory, "SIL.BuildTasks.dll"),
				Path.Combine(OutputDirectory, "SIL.BuildTasks.Tests.Helper.dll"),
				NUnitDir,
				category, !Environment.Is64BitProcess));

			return buildFile;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			if (string.IsNullOrEmpty(_nunitDir))
				return;

			Directory.Delete(_nunitDir, true);
			_nunitDir = null;
		}

		[Test]
		public void Success_DoesntFailBuild()
		{
			var xmlReader = new XmlTextReader(GetBuildFilename("Success"));
			var project = new Project(xmlReader);
			var result = project.Build("Test");
			Assert.That(result, Is.True, "Passing tests shouldn't fail the build");
		}

		[Test]
		public void FailingTests_DoesntFailBuild()
		{
			var xmlReader = new XmlTextReader(GetBuildFilename("Failing"));
			var project = new Project(xmlReader);
			var result = project.Build("Test");
			Assert.That(result, Is.True, "Failing tests shouldn't fail the build");
		}

		[Test]
		public void Exception_FailsTestButNotBuild()
		{
			var xmlReader = new XmlTextReader(GetBuildFilename("Exception"));
			var project = new Project(xmlReader);
			var result = project.Build("Test");
			Assert.That(result, Is.True, "Exception in test shouldn't fail the build");
		}

		[Test]
		[Platform(Exclude = "Win", Reason = "Doesn't crash on Windows. Instead we get an AccessViolationException that .NET handles")]
		public void TestCrash_FailsBuild()
		{
			var xmlReader = new XmlTextReader(GetBuildFilename("Crash"));
			var project = new Project(xmlReader);
			var result = project.Build("Test");
			Assert.That(result, Is.False, "Crash should fail the build");
		}

		[Test]
		public void OutputOnStderr_DoesntFailBuild()
		{
			var xmlReader = new XmlTextReader(GetBuildFilename("Stderr"));
			var project = new Project(xmlReader);
			var result = project.Build("Test");
			Assert.That(result, Is.True, "Output on Stderr shouldn't fail the build");
		}

		[Test]
		public void ErrorOnStderr_FailsBuild()
		{
			var xmlReader = new XmlTextReader(GetBuildFilename("ErrorOnStdErr"));
			var project = new Project(xmlReader);
			var result = project.Build("Test");
			Assert.That(result, Is.False, "Errors on Stderr should fail the build");
		}

		[Test]
		public void WarningOnStderr_DoesntFailBuild()
		{
			var xmlReader = new XmlTextReader(GetBuildFilename("WarningOnStdErr"));
			var project = new Project(xmlReader);
			var result = project.Build("Test");
			Assert.That(result, Is.True, "Warnings on Stderr shouldn't fail the build");
		}
	}
}
