using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SIL.IO.FileLock.FileSys;
using SIL.PlatformUtilities;

namespace SIL.IO.FileLock
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
				if (!ProcessIsRunning((int)lockContent.PID, lockContent.ProcessName))
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

		private static bool ProcessIsRunning(int processId, string processName)
		{
			// First, look for a process with this processId
			var process = Process.GetProcesses().FirstOrDefault(x => x.Id == processId);

			// If there is no process with this processId, it is not running.
			if (process == null) return false;

			// Next, check for a match on processName.
			var isRunning = GetProcessNameSafely(process) == processName;

			// If a match was found or this is running on Windows, this is as far as we need to go.
			if (isRunning || Platform.IsWindows) return isRunning;

			// We need to look a little deeper on Linux.

			// If the name of the process is not "mono" or does not start with "mono-", this is not
			// a mono application, and therefore this is not the process we are looking for.
			var lowername = GetProcessNameSafely(process).ToLowerInvariant();
			if (lowername != "mono" && !lowername.StartsWith("mono-"))
				return false;

			// The mono application will have a module with the same name as the process, with ".exe" added.
			var moduleName = processName.ToLower() + ".exe";
			return process.Modules.Cast<ProcessModule>().Any(mod => mod.ModuleName.ToLower() == moduleName);
		}

		/// <summary>
		/// Tests can create dummy data based on process 1 on Linux, but the library
		/// throws an error trying to get the name of that process unless the program
		/// is running as root.  So we'll use a dummy name for process 1 on Linux (which
		/// has to be active).
		/// </summary>
		private static string GetProcessNameSafely(Process process)
		{
			if (Platform.IsWindows)
				return process.ProcessName;
			else
				return (process.Id == 1) ? "init" : process.ProcessName;
		}
		#endregion

		#region Create methods

		public static SimpleFileLock Create(string lockName, [Optional] TimeSpan lockTimeout)
		{
			if (string.IsNullOrEmpty(lockName))
				throw new ArgumentNullException("lockName", "lockName cannot be null or empty.");

			return new SimpleFileLock(lockName, lockTimeout) { LockFilePath = LockIO.GetFilePath(lockName) };
		}

		public static SimpleFileLock CreateFromFilePath(string lockFilePath, [Optional] TimeSpan lockTimeout)
		{
			if (string.IsNullOrEmpty(lockFilePath))
				throw new ArgumentNullException("lockFilePath");

			string lockName = Path.GetFileName(lockFilePath);
			return new SimpleFileLock(lockName, lockTimeout) { LockFilePath = lockFilePath };
		}

		#endregion
	}
}