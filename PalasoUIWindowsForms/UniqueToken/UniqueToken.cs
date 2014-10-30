using System;
using System.Threading;
using Palaso.Code;
using Palaso.Reporting;

namespace Palaso.UI.WindowsForms.UniqueToken
{
	public static class UniqueToken
	{
		// The current use of this class in Linux requires use of shared memory which is disabled by default.
		// Mono decided (as of 2.8) that shared memory should not be enabled by default because of potential bugs.
		// This may prove to be a poor implementation which can be replaced by a lock file at some point.
		//
		// If shared memory is not enabled, a mutex will always be acquired.  
		static UniqueToken()
		{
			if (PlatformUtilities.Platform.IsLinux)
			{
				Guard.Against(Environment.GetEnvironmentVariable("MONO_ENABLE_SHM") != "1", 
					"With the current implementation of this class, MONO_ENABLE_SHM must be set to 1");
			}
		}

		private static Mutex s_mutex;

		/// <summary>
		/// Try to acquire the token quietly
		/// </summary>
		/// <param name="uniqueIdentifier"></param>
		/// <returns>True if we successfully acquired the token, false otherwise</returns>
		public static bool TryToAcquireTokenQuietly(string uniqueIdentifier)
		{
			bool mutexAcquired = false;
			string mutexId = uniqueIdentifier;
			try
			{
				s_mutex = Mutex.OpenExisting(mutexId);
				mutexAcquired = s_mutex.WaitOne(TimeSpan.FromMilliseconds(1 * 1000), false);
			}
			catch (WaitHandleCannotBeOpenedException) //doesn't exist, we're the first.
			{
				s_mutex = new Mutex(true, mutexId, out mutexAcquired);
				mutexAcquired = true;
			}
			catch (AbandonedMutexException)
			{
			}
			return mutexAcquired;
		}

		/// <summary>
		/// First, we try to get the mutex quickly and quietly.
		/// If that fails, we put up a dialog and wait a number of seconds,
		/// while we wait for the token to come free.
		/// </summary>
		/// <param name="uniqueIdentifier"></param>
		/// <param name="applicationName"></param>
		/// <param name="secondsToWait"></param>
		/// <returns>True if we successfully acquired the token, false otherwise</returns>
		public static bool AcquireToken(string uniqueIdentifier, string applicationName = null, int secondsToWait = 10)
		{
			Guard.AgainstNull(uniqueIdentifier, "uniqueIdentifier");

			bool mutexAcquired = TryToAcquireTokenQuietly(uniqueIdentifier);
			string mutexId = uniqueIdentifier;

			string waitingMsg = "Waiting for other " + (applicationName ?? "application") + " to finish...";
			using (var dlg = new SimpleMessageDialog(waitingMsg, applicationName))
			{
				dlg.Show();
				try
				{
					s_mutex = Mutex.OpenExisting(mutexId);
					mutexAcquired = s_mutex.WaitOne(TimeSpan.FromMilliseconds(secondsToWait * 1000), false);
				}
				catch (AbandonedMutexException)
				{
					s_mutex = new Mutex(true, mutexId, out mutexAcquired);
					mutexAcquired = true;
				}
				catch (Exception e)
				{
					string errorMsg = "There was a problem starting " + (applicationName ?? "the application") + " which might require that you restart your computer.";
					ErrorReport.NotifyUserOfProblem(e, errorMsg);
				}
			}

			if (!mutexAcquired) // cannot acquire
			{
				s_mutex = null;
				string errorMsg = "Another copy of " + (applicationName ?? "the application") + 
					" is already running. If you cannot find " + (applicationName!=null ? "that " + applicationName : "it") + ", restart your computer.";
				ErrorReport.NotifyUserOfProblem(errorMsg);
				return false;
			}
			return true;
		}

		public static void ReleaseToken()
		{
			if (s_mutex != null)
			{
				s_mutex.ReleaseMutex();
				s_mutex = null;
			}
		}
	}
}
