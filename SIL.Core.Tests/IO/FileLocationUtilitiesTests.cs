using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SIL.PlatformUtilities;
using static SIL.IO.FileLocationUtilities;

namespace SIL.Tests.IO
{
	[TestFixture]
	class FileLocationUtilitiesTests
	{
		private const int ciDeepSearchTimeoutInSeconds = 15;

		private string WindowsProgramFileThatShouldExist => "msinfo32.exe";

		private string SystemProgramFileThatShouldExist =>
			Platform.IsMono ? "bash" : WindowsProgramFileThatShouldExist;

		[Test]
		public void GetFileDistributedWithApplication_MultipleParts_FindsCorrectly()
		{
			var path = GetFileDistributedWithApplication("DirectoryForTests", "SampleFileForTests.txt");
			Assert.That(File.Exists(path));
		}
		
		[Test]
		public void GetDirectoryDistributedWithApplication_MultipleParts_FindsCorrectly()
		{
			var path = GetDirectoryDistributedWithApplication("DirectoryForTests");
			Assert.That(Directory.Exists(path));
		}

		[Test]
		public void GetDirectoryDistributedWithApplication_WhenFails_ReportsAllTried()
		{
			try
			{
				GetDirectoryDistributedWithApplication("LookHere", "ThisWillNotExist");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message, Does.Contain(Path.Combine("LookHere", "ThisWillNotExist")));
				Assert.That(ex.Message, Does.Contain(DirectoryOfApplicationOrSolution));
				Assert.That(ex.Message, Does.Contain("DistFiles"));
				Assert.That(ex.Message, Does.Contain("src"));
			}
		}

		[Test]
		public void DirectoryOfApplicationOrSolution_OnDevMachine_FindsOutputDirectory()
		{
			var path = DirectoryOfTheApplicationExecutable;
			Assert.That(Directory.Exists(path));
			Assert.That(path.Contains("output"));
		}

		[Test]
		public void LocateInProgramFiles_SendInvalidProgramNoDeepSearch_ReturnsNull()
		{
			Assert.IsNull(LocateInProgramFiles("blah.exe", false));
		}

		// 12 SEP 2013, Phil Hopper: This test not valid on Mono.
		[Test]
		[Platform(Exclude = "Unix")]
		[Category("SkipOnTeamCity;KnownMonoIssue")]
		public void LocateInProgramFiles_SendValidProgramNoDeepSearch_ReturnsNull()
		{
			Assert.IsNull(LocateInProgramFiles(WindowsProgramFileThatShouldExist, false));
		}

		[Test]
		public void LocateInProgramFiles_SendValidProgramDeepSearch_ReturnsProgramPath()
		{
			var findFile = SystemProgramFileThatShouldExist;

			// On CI build (GHA) this can time out
			if (Environment.GetEnvironmentVariable("CI") == "true")
			{
				var task = Task.Run(() => LocateInProgramFiles(findFile, true));

				if (!task.Wait(TimeSpan.FromSeconds(ciDeepSearchTimeoutInSeconds)))
					Assert.Inconclusive("Test timed out on CI build.");

				Assert.IsNotNull(task.Result);
			}
			else
				Assert.IsNotNull(LocateInProgramFiles(findFile, true));
		}

		[Test]
		public void LocateInProgramFiles_SendValidProgramDeepSearch_SubFolderSpecified_ReturnsProgramPath()
		{
			// This should work on Mono because it ignores the subFoldersToSearch parameter.

			var findFile = SystemProgramFileThatShouldExist;

			// On CI build (GHA) this can time out
			if (Environment.GetEnvironmentVariable("CI") == "true")
			{
				var task = Task.Run(() => LocateInProgramFiles(findFile, true, "Common Files"));

				if (!task.Wait(TimeSpan.FromSeconds(ciDeepSearchTimeoutInSeconds)))
					Assert.Inconclusive("Test timed out on CI build.");

				Assert.IsNotNull(task.Result);
			}
			else
				Assert.IsNotNull(LocateInProgramFiles(findFile, true, "Common Files"));
		}

		[Test]
		public void LocateInProgramFiles_SendInvalidSubFolder_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => LocateInProgramFiles(SystemProgramFileThatShouldExist,
				true, "!~@blah"));
		}

		[Test]
		[Platform(Include = "Linux")]
		public void LocateInProgramFiles_DeepSearch_FindsFileInSubDir()
		{
			// This simulates finding RAMP which is installed as /opt/RAMP/ramp. We can't put
			// anything in /opt for testing, so we add our tmp directory to the path.

			// Setup
			var simulatedOptDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var pathVariable = Environment.GetEnvironmentVariable("PATH");
			try
			{
				Directory.CreateDirectory(simulatedOptDir);
				Directory.CreateDirectory(Path.Combine(simulatedOptDir, "RAMP"));
				var file = Path.Combine(simulatedOptDir, "RAMP", "ramp");
				File.WriteAllText(file, "Simulated RAMP starter");
				Environment.SetEnvironmentVariable("PATH", $"{simulatedOptDir}{Path.PathSeparator}{pathVariable}");

				// Exercise/Verify
				Assert.That(LocateInProgramFiles("ramp", true),
					Is.EqualTo(file));
			}
			finally
			{
				try
				{
					Environment.SetEnvironmentVariable("PATH", pathVariable);
					Directory.Delete(simulatedOptDir, true);
				}
				catch
				{
					// just ignore
				}
			}
		}

		[Test]
		[Platform(Include = "Linux")]
		public void LocateInProgramFiles_ShallowSearch_FindsNothing()
		{
			// This simulates finding RAMP which is installed as /opt/RAMP/ramp. We can't put
			// anything in /opt for testing, so we add our tmp directory to the path.

			// Setup
			var simulatedOptDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var pathVariable = Environment.GetEnvironmentVariable("PATH");
			try
			{
				Directory.CreateDirectory(simulatedOptDir);
				Directory.CreateDirectory(Path.Combine(simulatedOptDir, "RAMP"));
				var file = Path.Combine(simulatedOptDir, "RAMP", "ramp");
				File.WriteAllText(file, "Simulated RAMP starter");
				Environment.SetEnvironmentVariable("PATH", $"{simulatedOptDir}{Path.PathSeparator}{pathVariable}");

				// Exercise/Verify
				Assert.That(LocateInProgramFiles("ramp", false),
					Is.Null);
			}
			finally
			{
				try
				{
					Environment.SetEnvironmentVariable("PATH", pathVariable);
					Directory.Delete(simulatedOptDir, true);
				}
				catch
				{
					// just ignore
				}
			}
		}

		//TODO: this could use lots more tests

		[Test]
		public void LocateExecutable_DistFiles()
		{
			Assert.That(LocateExecutable("DirectoryForTests", "SampleExecutable.exe"),
				Does.EndWith(string.Format("DistFiles{0}DirectoryForTests{0}SampleExecutable.exe",
					Path.DirectorySeparatorChar)));
		}

		[Test]
		[Platform(Exclude = "Linux")]
		public void LocateExecutable_PlatformSpecificInDistFiles_Windows()
		{
			Assert.That(LocateExecutable("DirectoryForTests", "dummy.exe"),
				Does.EndWith(string.Format("DistFiles{0}Windows{0}DirectoryForTests{0}dummy.exe",
				Path.DirectorySeparatorChar)));
		}

		[Test]
		[Platform(Include = "Linux")]
		public void LocateExecutable_PlatformSpecificInDistFiles_LinuxWithoutExtension()
		{
			Assert.That(LocateExecutable("DirectoryForTests", "dummy.exe"),
				Does.EndWith(string.Format("DistFiles{0}Linux{0}DirectoryForTests{0}dummy",
				Path.DirectorySeparatorChar)));
		}

		[Test]
		[Platform(Include = "Linux")]
		public void LocateExecutable_PlatformSpecificInDistFiles_Linux()
		{
			Assert.That(LocateExecutable("DirectoryForTests", "dummy2.exe"),
				Does.EndWith(string.Format("DistFiles{0}Linux{0}DirectoryForTests{0}dummy2.exe",
				Path.DirectorySeparatorChar)));
		}

		[Test]
		public void LocateExecutable_NonexistentFile()
		{
			Assert.That(LocateExecutable(false, "dummy", "__nonexistent.exe"), Is.Null);
		}

		[Test]
		public void LocateExecutable_NonexistentFileThrows()
		{
			Assert.That(() => LocateExecutable("dummy", "__nonexistent.exe"),
				Throws.Exception.TypeOf<ApplicationException>());
		}
	}
}
