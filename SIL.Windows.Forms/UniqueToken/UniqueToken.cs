using System;
using System.Threading;
using SIL.Code;
using SIL.IO.FileLock;
using SIL.Reporting;

namespace SIL.Windows.Forms.UniqueToken
{
	/// <summary>
	/// At a high level, this class is used to ensure that a given token (based on a unique string)
	/// can only be obtained by one process on the whole system.
	/// Each process is only allowed to acquire one token.
	/// The process should release the token when it is no longer being used;
	/// however, my testing shows that if the process crashes, it will still release the token.
	/// 
	/// Though it could have uses outside of this, the orginal use of this class is to accomplish
	/// enforcement of a single instance application.
	/// Though it has not been tested, it should also work to enforce an application being able
	/// to have multiple instances where each instance has a unique project open.
	/// For the former, pass the application name as the uniqueIdetifier (e.g. "Bloom").
	/// For the latter, the client could pass the path of the project file as the uniqueIdentifier.
	/// </summary>
	public static class UniqueToken
	{
		private const string FileExtension = ".locktoken";

		private static SimpleFileLock s_fileLock;

		/// <summary>
		/// Try to acquire the token quietly
		/// </summary>
		/// <param name="uniqueIdentifier"></param>
		/// <returns>True if we successfully acquired the token, false otherwise</returns>
		public static bool AcquireTokenQuietly(string uniqueIdentifier)
		{
			Guard.AgainstNull(uniqueIdentifier, "uniqueIdentifier");

			// Each process may only acquire one token
			if (s_fileLock != null)
				return false;

			s_fileLock = SimpleFileLock.Create(uniqueIdentifier + FileExtension);
			return s_fileLock.TryAcquireLock();
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
			if (tokenAcquired)
				return true;

			string waitingMsg = applicationName == null ?
				L10NSharp.LocalizationManager.GetString("Application.WaitingFinish.General", "Waiting for other application to finish...") :
				String.Format(
					L10NSharp.LocalizationManager.GetString("Application.WaitingFinish.Specific", "Waiting for {0} to finish...", "{0} is the application name"),
					applicationName);
			using (var dlg = new SimpleMessageDialog(waitingMsg, applicationName))
			{
				dlg.Show();
				try
				{
					var timeoutTime = DateTime.Now.AddSeconds(secondsToWait);
					while(DateTime.Now < timeoutTime && !tokenAcquired)
					{
						tokenAcquired = s_fileLock.TryAcquireLock();
						Thread.Sleep(500);
					}
				}
				catch (Exception e)
				{
					string errorMsg = applicationName == null ?
						L10NSharp.LocalizationManager.GetString("Application.ProblemStarting.General", 
						"There was a problem starting the application which might require that you restart your computer.") :
						String.Format(
							L10NSharp.LocalizationManager.GetString("Application.ProblemStarting.Specific", 
								"There was a problem starting {0} which might require that you restart your computer.", "{0} is the application name"),
							applicationName);
					ErrorReport.NotifyUserOfProblem(e, errorMsg);
				}
			}

			if (!tokenAcquired) // cannot acquire
			{
				string errorMsg = applicationName == null ?
						L10NSharp.LocalizationManager.GetString("Application.AlreadyRunning.General",
						"Another copy of the application is already running. If you cannot find it, restart your computer.") :
						String.Format(
							L10NSharp.LocalizationManager.GetString("Application.AlreadyRunning.Specific",
								"Another copy of {0} is already running. If you cannot find that copy of {0}, restart your computer.",
								"{0} is the application name"),
							applicationName);
				ErrorReport.NotifyUserOfProblem(errorMsg);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Releases the token when client is finished with it
		/// 
		/// Even though it is a very good idea to release the token when finished with it,
		/// in the event of a crash, the file is released anyway meaning another application can get it.
		/// </summary>
		public static void ReleaseToken()
		{
			if (s_fileLock != null)
			{
				s_fileLock.ReleaseLock();
				s_fileLock = null;
			}
		}
	}
}
