using System;
using System.IO;
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
		private static string m_uniqueIdentifier;
		private static FileStream m_lockFile;

		/// <summary>
		/// Try to acquire the token quietly
		/// </summary>
		/// <param name="uniqueIdentifier"></param>
		/// <returns>True if we successfully acquired the token, false otherwise</returns>
		public static bool AcquireTokenQuietly(string uniqueIdentifier)
		{
			Guard.AgainstNull(uniqueIdentifier, "uniqueIdentifier");

			// Each process may only acquire one token
			if (m_lockFile != null)
				return false;

			bool tokenAcquired = false;
			try
			{
				m_lockFile = File.Open(Path.GetTempPath() + uniqueIdentifier + ".locktoken", FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
				m_uniqueIdentifier = uniqueIdentifier;
				tokenAcquired = true;
			}
			catch (IOException)
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
			if (tokenAcquired)
				return true;

			string waitingMsg = applicationName == null ?
				L10NSharp.LocalizationManager.GetString("Application.WaitingFinish.General", "Waiting for other application to finish...") :
				String.Format(
					L10NSharp.LocalizationManager.GetString("Application.WaitingFinish.Specific", "Waiting for other {0} to finish...", "{0} is the application name"),
					applicationName);
			using (var dlg = new SimpleMessageDialog(waitingMsg, applicationName))
			{
				dlg.Show();
				dlg.Update();
				try
				{
					for (int i=0; i < secondsToWait; i++)
				{
						tokenAcquired = AcquireTokenQuietly(uniqueIdentifier);
						if (tokenAcquired)
							break;
						Thread.Sleep(1000);
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
								"Another copy of {0} is already running. If you cannot find that {0}, restart your computer.",
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
			if (m_lockFile != null)
			{
				m_lockFile.Close();
				File.Delete(Path.GetTempPath() + m_uniqueIdentifier + ".locktoken");
				m_lockFile = null;
			}
		}
	}
}
