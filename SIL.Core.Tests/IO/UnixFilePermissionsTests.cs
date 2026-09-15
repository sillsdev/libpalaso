// Copyright (c) 2026 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;

namespace SIL.Tests.IO
{
	[TestFixture]
	public class UnixFilePermissionsTests
	{
		private const uint GroupWritable = 0x1B4; // 0664: rw-rw-r--
		private const uint OwnerOnly = 0x180;     // 0600: rw-------

		private static TemporaryFolder CreateTemporaryFolder(string testName)
		{
			return new TemporaryFolder($"{testName}_{Path.GetRandomFileName()}");
		}

		/// <summary>
		/// Callers use the return value to decide whether to apply anything, so a platform without
		/// permission bits has to report that rather than throw or claim a mode it did not read.
		/// </summary>
		[Test]
		[Platform(Exclude = "Linux,MacOsX", Reason = "this is the behavior off Unix")]
		public void TryGetMode_PlatformHasNoPermissionBits_ReturnsFalse()
		{
			using (var folder = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				string path = Path.Combine(folder.Path, "file.txt");
				File.WriteAllText(path, "contents");

				Assert.That(UnixFilePermissions.TryGetMode(path, out uint mode), Is.False);
				Assert.That(mode, Is.EqualTo(0));
				Assert.That(UnixFilePermissions.TrySetMode(path, GroupWritable), Is.False);
			}
		}

		[Test]
		[Platform(Include = "Linux,MacOsX", Reason = "permission bits of this kind exist only on Unix")]
		public void TrySetMode_ModeApplied_TryGetModeReadsItBack()
		{
			using (var folder = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				string path = Path.Combine(folder.Path, "file.txt");
				File.WriteAllText(path, "contents");

				Assert.That(UnixFilePermissions.TrySetMode(path, OwnerOnly), Is.True);
				Assert.That(UnixFilePermissions.TryGetMode(path, out uint owner), Is.True);
				Assert.That(owner, Is.EqualTo(OwnerOnly));

				Assert.That(UnixFilePermissions.TrySetMode(path, GroupWritable), Is.True);
				Assert.That(UnixFilePermissions.TryGetMode(path, out uint group), Is.True);
				Assert.That(group, Is.EqualTo(GroupWritable));
			}
		}

		[Test]
		[Platform(Include = "Linux,MacOsX", Reason = "permission bits of this kind exist only on Unix")]
		public void TryGetMode_FileDoesNotExist_ReturnsFalse()
		{
			using (var folder = CreateTemporaryFolder(TestContext.CurrentContext.Test.Name))
			{
				string path = Path.Combine(folder.Path, "never-written.txt");

				Assert.That(UnixFilePermissions.TryGetMode(path, out _), Is.False);
			}
		}
	}
}
