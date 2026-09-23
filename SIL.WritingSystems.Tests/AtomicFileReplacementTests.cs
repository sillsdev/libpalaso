using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class AtomicFileReplacementTests
	{

		[Test]
		public void GetTempPath_IsBesideTheTargetAndNamedForThisProcess()
		{
			string targetPath = Path.Combine("some", "where", "en.ldml");

			string tempPath = AtomicFileReplacement.GetTempPath(targetPath, "tmp");

			using (Process process = Process.GetCurrentProcess())
				Assert.That(tempPath, Does.StartWith($"{targetPath}.{process.Id}."));
			Assert.That(tempPath, Does.EndWith(".tmp"));
			Assert.That(Path.GetDirectoryName(tempPath), Is.EqualTo(Path.GetDirectoryName(targetPath)));
		}

		/// <summary>
		/// A process ID is only unique within its namespace, and the lock that would otherwise keep
		/// writers apart is reduced to a local-only one in exactly the containers that have their own:
		/// two of them sharing a store can hold the same ID.
		/// </summary>
		[Test]
		public void GetTempPath_CalledTwiceForOneTarget_DiffersEachTime()
		{
			string targetPath = Path.Combine("some", "where", "en.ldml");

			string first = AtomicFileReplacement.GetTempPath(targetPath, "tmp");
			string second = AtomicFileReplacement.GetTempPath(targetPath, "tmp");

			Assert.That(second, Is.Not.EqualTo(first));
		}

		[Test]
		public void SwapIntoPlace_TargetExists_TakesItsPlaceAndIsConsumed()
		{
			using (var folder = TemporaryFolder.Create(TestContext.CurrentContext))
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
			using (var folder = TemporaryFolder.Create(TestContext.CurrentContext))
			{
				string targetPath = Path.Combine(folder.Path, "target.txt");
				string tempPath = AtomicFileReplacement.GetTempPath(targetPath, "tmp");
				File.WriteAllText(tempPath, "new contents");

				AtomicFileReplacement.SwapIntoPlace(tempPath, targetPath);

				Assert.That(File.ReadAllText(targetPath), Is.EqualTo("new contents"));
				Assert.That(File.Exists(tempPath), Is.False);
			}
		}

		/// <summary>
		/// Replacing a file can carry the permissions of either the replacement or the file it
		/// displaces, and which one varies by file system. The replacement's are the ones the write
		/// intended, so they are the ones that have to survive.
		/// </summary>
		[Test]
		[Platform(Include = "Linux,MacOsX", Reason = "permission bits of this kind exist only on Unix")]
		public void SwapIntoPlace_TargetExists_KeepsTheReplacementsPermissions()
		{
			const uint groupWritable = 0x1B4; // 0664: rw-rw-r--
			const uint ownerOnly = 0x180;     // 0600: rw-------

			using (var folder = TemporaryFolder.Create(TestContext.CurrentContext))
			{
				string targetPath = Path.Combine(folder.Path, "target.txt");
				File.WriteAllText(targetPath, "previous contents");
				Assert.That(UnixFilePermissions.TrySetMode(targetPath, ownerOnly), Is.True);

				string tempPath = AtomicFileReplacement.GetTempPath(targetPath, "tmp");
				File.WriteAllText(tempPath, "new contents");
				Assert.That(UnixFilePermissions.TrySetMode(tempPath, groupWritable), Is.True);

				AtomicFileReplacement.SwapIntoPlace(tempPath, targetPath);

				Assert.That(UnixFilePermissions.TryGetMode(targetPath, out uint mode), Is.True);
				Assert.That(mode, Is.EqualTo(groupWritable));
			}
		}

		/// <summary>
		/// Where the file system cannot replace one file with another, the target has to be copied
		/// over, and copying truncates it first. If anything then fails, the contents being displaced
		/// are all that is left of the writing system, so they have to survive somewhere.
		/// </summary>
		[Test]
		[Platform(Exclude = "Linux,MacOsX",
			Reason = "an open handle does not stop a file being replaced on Unix")]
		public void SwapIntoPlace_ReplacementCannotBeWritten_KeepsTheDisplacedContents()
		{
			using (var folder = TemporaryFolder.Create(TestContext.CurrentContext))
			{
				string targetPath = Path.Combine(folder.Path, "target.txt");
				File.WriteAllText(targetPath, "previous contents");
				string tempPath = AtomicFileReplacement.GetTempPath(targetPath, "tmp");
				File.WriteAllText(tempPath, "new contents");

				// Hold the target open for reading only. Taking the backup reads it and succeeds;
				// writing the replacement over it needs exclusive access, and that is what fails.
				using (new FileStream(targetPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					Assert.That(() => AtomicFileReplacement.SwapIntoPlace(tempPath, targetPath),
						Throws.Exception);
				}

				string[] surviving = Directory.GetFiles(folder.Path, "*.bak");
				Assert.That(surviving.Length, Is.EqualTo(1),
					"the displaced contents must not be discarded when the swap fails");
				Assert.That(File.ReadAllText(surviving[0]), Is.EqualTo("previous contents"));
			}
		}

		[Test]
		public void SwapIntoPlace_TargetExists_LeavesNoBackupBehind()
		{
			using (var folder = TemporaryFolder.Create(TestContext.CurrentContext))
			{
				string targetPath = Path.Combine(folder.Path, "target.txt");
				File.WriteAllText(targetPath, "previous contents");
				string tempPath = AtomicFileReplacement.GetTempPath(targetPath, "tmp");
				File.WriteAllText(tempPath, "new contents");

				AtomicFileReplacement.SwapIntoPlace(tempPath, targetPath);

				Assert.That(Directory.GetFiles(folder.Path), Is.EquivalentTo(new[] { targetPath }));
			}
		}

		[Test]
		public void DeleteIfPresent_FilePresent_RemovesIt()
		{
			using (var folder = TemporaryFolder.Create(TestContext.CurrentContext))
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
			using (var folder = TemporaryFolder.Create(TestContext.CurrentContext))
			{
				string tempPath = Path.Combine(folder.Path, "never-written.tmp");

				Assert.That(() => AtomicFileReplacement.DeleteIfPresent(tempPath), Throws.Nothing);
			}
		}
	}
}
