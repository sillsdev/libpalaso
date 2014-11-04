using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Palaso.IO.FileLock.FileSys;

namespace Palaso.IO.FileLock
{
	public class SimpleFileLock : IFileLock
	{
		protected SimpleFileLock(string lockName, [Optional] TimeSpan lockTimeout)
		{
			LockName = lockName;
			LockTimeout = lockTimeout;
		}

		public TimeSpan LockTimeout { get; private set; }

		public string LockName { get; private set; }

		private string LockFilePath { get; set; }

		public bool TryAcquireLock()
		{
			if (LockIO.LockExists(LockFilePath))
			{
				var lockContent = LockIO.ReadLock(LockFilePath);

				//Someone else owns the lock
				if (lockContent.GetType() == typeof(OtherProcessOwnsFileLockContent))
				{
					return false;
				}

				//the file no longer exists
				if (lockContent.GetType() == typeof(MissingFileLockContent))
				{
					return AcquireLock();
				}

				//This lock belongs to this process - we can reacquire the lock
				if (lockContent.PID == Process.GetCurrentProcess().Id)
				{
					return AcquireLock();
				}

				//The process which created it is no longer running
				if (!ProcessIsRunning((int)lockContent.PID))
				{
					return AcquireLock();
				}

				if (LockTimeout != TimeSpan.Zero)
				{
					var lockWriteTime = new DateTime(lockContent.Timestamp);
					if (!(Math.Abs((DateTime.Now - lockWriteTime).TotalSeconds) > LockTimeout.TotalSeconds))
						return false; //The lock has not timed out - we can't acquire it
				}
				else
					return false;
			}

			//Acquire the lock
			
			return AcquireLock();
		}



		public bool ReleaseLock()
		{
			//Need to own the lock in order to release it (and we can reacquire the lock inside the current process)
			if (LockIO.LockExists(LockFilePath) && TryAcquireLock())
				LockIO.DeleteLock(LockFilePath);
			return true;
		}

		#region Internal methods

		protected FileLockContent CreateLockContent()
		{
			var process = Process.GetCurrentProcess();
			return new FileLockContent()
			{
				PID = process.Id,
				Timestamp = DateTime.Now.Ticks,
				ProcessName = process.ProcessName
			};
		}

		private bool AcquireLock()
		{
			return LockIO.WriteLock(LockFilePath, CreateLockContent());
		}

		private bool ProcessIsRunning(int processId)
		{
			// I tried including the process name in the check, but on Linux, the names didn't match
			return Process.GetProcesses().Any(x => x.Id == processId);
		}

		#endregion

		#region Create methods

		public static SimpleFileLock Create(string lockName, [Optional] TimeSpan lockTimeout)
		{
			if (string.IsNullOrEmpty(lockName))
				throw new ArgumentNullException("lockName", "lockName cannot be null or emtpy.");

			return new SimpleFileLock(lockName, lockTimeout) { LockFilePath = LockIO.GetFilePath(lockName) };
		}

		#endregion
	}
}