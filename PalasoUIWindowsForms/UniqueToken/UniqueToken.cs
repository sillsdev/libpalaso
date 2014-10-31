using System;
using System.Threading;
using Palaso.Code;
using Palaso.Reporting;

namespace Palaso.UI.WindowsForms.UniqueToken
{
	/// <summary>
	/// Though it could have uses outside of this, the orginal use of this class is to accomplish
	/// enforcement of a single instance application.
	/// Though it has not been tested, it should also work to enforce an application being able
	/// to have multiple instances where each instance has a unique project open.
	/// For the former, pass the application name as the uniqueIdetifier (e.g. "Bloom").
	/// For the latter, the client could pass the path the project file as the uniqueIdentifier.
	/// </summary>
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
		public static bool AcquireTokenQuietly(string uniqueIdentifier)
		{
			Guard.AgainstNull(uniqueIdentifier, "uniqueIdentifier");

			// Each process may only acquire one token
			if (s_mutex != null)
				return false;

			bool tokenAcquired = false;
			try
			{
				s_mutex = Mutex.OpenExisting(uniqueIdentifier);
				tokenAcquired = s_mutex.WaitOne(TimeSpan.FromMilliseconds(1 * 1000), false);
			}
			catch (WaitHandleCannotBeOpenedException) //doesn't exist, we're the first.
			{
				s_mutex = new Mutex(true, uniqueIdentifier, out tokenAcquired);
				tokenAcquired = true;
			}
			catch (AbandonedMutexException)
			{
			}
			return tokenAcquired;
		}

		/// <summary>
		/// First, we try to get the token quickly and quietly.
		/// If that fails, we put up a dialog and wait a number of seconds while we wait for the token to come free.
		/// </summary>
		/// <param name="uniqueIdentifier"></param>
		/// <param name="applicationName">Optional; used for user messaging</param>
		/// <param name="secondsToWait">Default = 10; Number of seconds the dialog will be displayed while waiting for the token to come free</param>
		/// <returns>True if we successfully acquired the token, false otherwise</returns>
		public static bool AcquireToken(string uniqueIdentifier, string applicationName = null, int secondsToWait = 10)
		{
			Guard.AgainstNull(uniqueIdentifier, "uniqueIdentifier");

			bool tokenAcquired = AcquireTokenQuietly(uniqueIdentifier);

			string waitingMsg = applicationName == null ?
				L10NSharp.LocalizationManager.GetString("Application.WaitingFinish.General", "Waiting for other application to finish...") :
				L10NSharp.LocalizationManager.GetString("Application.WaitingFinish.Specific", "Waiting for other {0} to finish...", "{0} is the application name");
			using (var dlg = new SimpleMessageDialog(waitingMsg, applicationName))
			{
				dlg.Show();
				try
				{
					s_mutex = Mutex.OpenExisting(uniqueIdentifier);
					tokenAcquired = s_mutex.WaitOne(TimeSpan.FromMilliseconds(secondsToWait * 1000), false);
				}
				catch (AbandonedMutexException)
				{
					s_mutex = new Mutex(true, uniqueIdentifier, out tokenAcquired);
					tokenAcquired = true;
				}
				catch (Exception e)
				{
					string errorMsg = applicationName == null ?
						L10NSharp.LocalizationManager.GetString("Application.ProblemStarting.General", 
						"There was a problem starting the application which might require that you restart your computer.") :
						L10NSharp.LocalizationManager.GetString("Application.ProblemStarting.Specific", 
						"There was a problem starting {0} which might require that you restart your computer.", "{0} is the application name");
					ErrorReport.NotifyUserOfProblem(e, errorMsg);
				}
			}

			if (!tokenAcquired) // cannot acquire
			{
				s_mutex = null;
				string errorMsg = applicationName == null ?
						L10NSharp.LocalizationManager.GetString("Application.AlreadyRunning.General", 
						"Another copy of the application is already running. If you cannot find it, restart your computer.") :
						L10NSharp.LocalizationManager.GetString("Application.AlreadyRunning.Specific",
						"Another copy of the application is already running. If you cannot find that {0}, restart your computer.", "{0} is the application name");
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
