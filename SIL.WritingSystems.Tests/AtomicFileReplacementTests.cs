using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using SIL.TestUtilities;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class AtomicFileReplacementTests
	{
		private static TemporaryFolder CreateTemporaryFolder(string testName)
		{
			return new TemporaryFolder($"{testName}_{Path.GetRandomFileName()}");
		}

		[Test]
		public void GetTempPath_IsBesideTheTargetAndNamedForThisProcess()
		{
			string targetPath = Path.Combine("some", "where", "en.ldml");

			string tempPath = AtomicFileReplacement.GetTempPath(targetPath, "tmp");

			using (Process process = Process.GetCurrentProcess())
				Assert.That(tempPath, Is.EqualTo($"{targetPath}.{process.Id}.tmp"));
			Assert.That(Path.GetDirectoryName(tempPath), Is.EqualTo(Path.GetDirectoryName(targetPath)));
		}

		[Test]
		public void SwapIntoPlace_TargetExists_TakesItsPlaceAndIsConsumed()
		{
			using (var folder = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				string targetPath = Path.Combine(folder.Path, "target.txt");
				File.WriteAllText(targetPath, "previous contents");
				string tempPath = AtomicFileReplacement.GetTempPath(targetPath, "tmp");
				File.WriteAllText(tempPath, "new contents");

				AtomicFileReplacement.SwapIntoPlace(tempPath, targetPath);

				Assert.That(File.ReadAllText(targetPath), Is.EqualTo("new contents"));
				Assert.That(File.Exists(tempPath), Is.False);
			}
		}

		[Test]
		public void SwapIntoPlace_TargetDoesNotExist_MovesIntoPlace()
		{
			using (var folder = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				string targetPath = Path.Combine(folder.Path, "target.txt");
				string tempPath = AtomicFileReplacement.GetTempPath(targetPath, "tmp");
				File.WriteAllText(tempPath, "new contents");

				AtomicFileReplacement.SwapIntoPlace(tempPath, targetPath);

				Assert.That(File.ReadAllText(targetPath), Is.EqualTo("new contents"));
				Assert.That(File.Exists(tempPath), Is.False);
			}
		}

		[Test]
		public void DeleteIfPresent_FilePresent_RemovesIt()
		{
			using (var folder = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				string tempPath = Path.Combine(folder.Path, "leftover.tmp");
				File.WriteAllText(tempPath, "abandoned part way through");

				AtomicFileReplacement.DeleteIfPresent(tempPath);

				Assert.That(File.Exists(tempPath), Is.False);
			}
		}

		/// <summary>
		/// Cleanup runs while the caller is handling a more important failure, so it must not raise one
		/// of its own over a file that was never created.
		/// </summary>
		[Test]
		public void DeleteIfPresent_FileMissing_DoesNothing()
		{
			using (var folder = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				string tempPath = Path.Combine(folder.Path, "never-written.tmp");

				Assert.That(() => AtomicFileReplacement.DeleteIfPresent(tempPath), Throws.Nothing);
			}
		}
	}
}
