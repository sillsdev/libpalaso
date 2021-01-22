using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using SIL.IO.FileLock;
using SIL.IO.FileLock.FileSys;
using SIL.PlatformUtilities;

namespace SIL.Tests.IO.FileLock
{
	[TestFixture]
	public class SimpleFileLockTests
	{
		private static readonly string LockPath = Path.Combine(Path.GetTempPath(), "SimpleFileLockTests");
		// some tests require the Id of an active process.
		// on Windows, we use 0, which is the system idle process.
		// on Unix, we use 1, which is the init process.
		// we know that these will always be active.
		private static readonly int ActiveProcessId = Platform.IsUnix ? 1 : 0;

		[SetUp]
		public void SetUp()
		{
			CleanupLockFile();
		}

		[TearDown]
		public void TearDown()
		{
			CleanupLockFile();
		}

		private static void CleanupLockFile()
		{
			if (File.Exists(LockPath))
				File.Delete(LockPath);
		}

		[Test]
		public void TryAcquireLock_Unlocked_ReturnsTrue()
		{
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests");
			Assert.That(fileLock.TryAcquireLock(), Is.True);
			fileLock.ReleaseLock();
		}

		[Test]
		public void TryAcquireLock_LockedByActiveProcess_ReturnsFalse()
		{
			LockIO.WriteLock(LockPath, new FileLockContent
			{
				PID = ActiveProcessId,
				ProcessName = GetSafeActiveProcessName(),
				Timestamp = DateTime.Now.Ticks
			});
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests");
			Assert.That(fileLock.TryAcquireLock(), Is.False);
		}

		[Test]
		public void TryAcquireLock_LockedByDeadProcess_ReturnsTrue()
		{
			LockIO.WriteLock(LockPath, new FileLockContent
			{
				PID = 9999,
				ProcessName = Path.GetRandomFileName(),
				Timestamp = DateTime.Now.Ticks
			});
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests");
			Assert.That(fileLock.TryAcquireLock(), Is.True);
			fileLock.ReleaseLock();
		}

		[Test]
		public void TryAcquireLock_HasLock_ReturnsTrue()
		{
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests");
			Assert.That(fileLock.TryAcquireLock(), Is.True);
			Assert.That(fileLock.TryAcquireLock(), Is.True);
			fileLock.ReleaseLock();
		}

		[Test]
		public void TryAcquireLock_LockFileCurrentlyOpen_ReturnsFalse()
		{
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests");
			using (File.Open(LockPath, FileMode.Create))
			{
				Assert.That(fileLock.TryAcquireLock(), Is.False);
			}
		}

		[Test]
		public void TryAcquireLock_OldEmptyFileLock_ReturnsTrue()
		{
			File.WriteAllText(LockPath, "");
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests");
			Assert.That(fileLock.TryAcquireLock(), Is.True);
			fileLock.ReleaseLock();
		}

		[Test]
		public void TryAcquireLock_LockedByActiveProcessTimedOut_ReturnsTrue()
		{
			LockIO.WriteLock(LockPath, new FileLockContent
			{
				PID = ActiveProcessId,
				ProcessName = GetSafeActiveProcessName(),
				Timestamp = (DateTime.Now - TimeSpan.FromHours(2)).Ticks
			});
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests", TimeSpan.FromHours(1));
			Assert.That(fileLock.TryAcquireLock(), Is.True);
		}

		[Test]
		public void TryAcquireLock_LockedByActiveProcessNotTimedOut_ReturnsFalse()
		{
			LockIO.WriteLock(LockPath, new FileLockContent
			{
				PID = ActiveProcessId,
				ProcessName = GetSafeActiveProcessName(),
				Timestamp = DateTime.Now.Ticks
			});
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests", TimeSpan.FromHours(1));
			Assert.That(fileLock.TryAcquireLock(), Is.False);
		}

		[Test]
		public void ReleaseLock_HasLock_LockFileDoesNotExist()
		{
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests");
			Assert.That(fileLock.TryAcquireLock(), Is.True);
			fileLock.ReleaseLock();
			Assert.That(File.Exists(LockPath), Is.False);
		}

		[Test]
		public void ReleaseLock_Unlocked_LockFileDoesNotExist()
		{
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests");
			fileLock.ReleaseLock();
			Assert.That(File.Exists(LockPath), Is.False);
		}

		[Test]
		public void ReleaseLock_LockedByActiveProcess_LockFileExists()
		{
			LockIO.WriteLock(LockPath, new FileLockContent
			{
				PID = ActiveProcessId,
				ProcessName = GetSafeActiveProcessName(),
				Timestamp = DateTime.Now.Ticks
			});
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests");
			fileLock.ReleaseLock();
			Assert.That(File.Exists(LockPath), Is.True);
		}

		/// <summary>
		/// Trying to get the process name for process 1 on Linux throws an exception unless the program
		/// is running as root.  So use a dummy name for the init process instead.
		/// </summary>
		private string GetSafeActiveProcessName()
		{
			return Platform.IsUnix && (ActiveProcessId == 1) ? "init" : Process.GetProcessById(ActiveProcessId).ProcessName;
		}

		[Test]
		public void ReleaseLock_LockFileCurrentlyOpen_LockFileExists()
		{
			SimpleFileLock fileLock = SimpleFileLock.Create("SimpleFileLockTests");
			using (File.Open(LockPath, FileMode.Create))
			{
				fileLock.ReleaseLock();
			}
			Assert.That(File.Exists(LockPath), Is.True);
		}
	}
}
