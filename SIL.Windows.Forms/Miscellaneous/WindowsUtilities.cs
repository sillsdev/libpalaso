using System;
using System.IO;
using JetBrains.Annotations;
using L10NSharp;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Reporting;
using static System.Environment;
using static System.Environment.SpecialFolder;
using static System.IO.Path;

namespace SIL.Windows.Forms.Miscellaneous
{
	/// summary
	/// Utilities that are specific to the Windows OS.
	/// /summary
	public static class WindowsUtilities
	{
		/// <summary>
		/// Returns 'true' unless we find we can't write to all the specified folders, in which
		/// case it (normally) reports the condition to the user.
		/// In Oct of 2017, a Windows update to Defender on some machines set Protections on
		/// certain basic folders, like MyDocuments! This resulted in throwing an exception any
		/// time Bloom tried to write out CollectionSettings files! More recently (especially on
		/// Windows 11), some other apps have been having similar failures due to enhanced anti-
		/// ransomware checking.
		/// </summary>
		/// <param name="applicationName">Name of the application to use when formatting the error.
		/// (Can pass null to just return false to the caller without reporting the problem, but it
		/// will not be obvious which folder was not writable.)</param>
		/// <param name="foldersToCheck">The folders to test for write access.</param>
		[PublicAPI]
		public static bool CanWriteToDirectories(string applicationName,
			params string[] foldersToCheck)
		{
			return CanWriteToDirectories(GetReportingMethodFor(applicationName), foldersToCheck);
		}
		/// <summary>
		/// Returns 'true' unless we find we can't write to all the specified folders, in which
		/// case it performs the specified report action.
		/// In Oct of 2017, a Windows update to Defender on some machines set Protections on
		/// certain basic folders, like MyDocuments! This resulted in throwing an exception any
		/// time Bloom tried to write out CollectionSettings files! More recently (especially on
		/// Windows 11), some other apps have been having similar failures due to enhanced anti-
		/// ransomware checking.
		/// </summary>
		/// <param name="report">Action to perform if a folder is found which is not writable (or
		/// null to jst return false)
		/// </param>
		/// <param name="foldersToCheck">The folders to test for write access.</param>
		[PublicAPI]
		public static bool CanWriteToDirectories(Action<Exception, string> report,
			params string[] foldersToCheck)
		{
			foreach (var folder in foldersToCheck)
			{
				if (!CanWriteToDirectory(report, folder))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns 'true' unless we find we can't write to the specified folder, in which case
		/// it (normally) reports the condition to the user.
		/// In Oct of 2017, a Windows update to Defender on some machines set Protections on
		/// certain basic folders, like MyDocuments! This resulted in throwing an exception any
		/// time Bloom tried to write out CollectionSettings files! More recently (especially on
		/// Windows 11), some other apps have been having similar failures due to enhanced anti-
		/// ransomware checking.
		/// </summary>
		/// <param name="applicationName">Name of the application to use when formatting the error.
		/// (Leave as null to just return false to the caller without reporting the problem.)
		/// </param>
		/// <param name="folderToCheck">An optional folder to test for write access.</param>
		[PublicAPI]
		public static bool CanWriteToDirectory(string applicationName = null,
			string folderToCheck = null)
		{
			return CanWriteToDirectory(GetReportingMethodFor(applicationName), folderToCheck);
		}

		/// <summary>
		/// Returns 'true' unless we find we can't write to the specified folder, in which
		/// case it performs the specified report action.
		/// In Oct of 2017, a Windows update to Defender on some machines set Protections on
		/// certain basic folders, like MyDocuments! This resulted in throwing an exception any
		/// time Bloom tried to write out CollectionSettings files! More recently (especially on
		/// Windows 11), some other apps have been having similar failures due to enhanced anti-
		/// ransomware checking.
		/// </summary>
		/// <param name="report">Action to perform if a folder is found which is not writable
		/// </param>
		/// <param name="folderToCheck">An optional folder to test for write access.</param>
		[PublicAPI]
		public static bool CanWriteToDirectory(Action<Exception, string> report,
			string folderToCheck = null)
		{
			if (!Platform.IsWindows)
				return true;

			if (string.IsNullOrEmpty(folderToCheck))
				folderToCheck = GetFolderPath(MyDocuments);
			else
			{
				if (!Directory.Exists(folderToCheck))
					throw new ArgumentException(
						"Folder provided should be an existing directory.",
						nameof(folderToCheck));
			}

			string testPath = GetTestFileName(folderToCheck);

			try
			{
				RobustFile.WriteAllText(testPath, "test contents");
			}
			catch (Exception exc)
			{
				// Creating a previously non-existent file under these conditions just gives a WinIOError, "could not find file".
				report?.Invoke(exc, GetDirectoryName(testPath));
				return false;
			}
			finally
			{
				Cleanup(testPath);
			}
			return true;
		}

		private static string GetTestFileName(string folderToCheck)
		{
			while (true)
			{
				var testPath = Combine(folderToCheck,
					GetFileName(TempFile.CreateAndGetPathButDontMakeTheFile().Path));
				// Really unlikely that the temp file name would exist, but let's check to be sure.
				if (!File.Exists(testPath) && !Directory.Exists(testPath))
					return testPath;
			}
		}

		private static Action<Exception, string> GetReportingMethodFor(string applicationName)
		{
			if (applicationName == null)
				return null;
			return (e, s) => ReportDefenderProblem(applicationName, e, s);
		}

		[PublicAPI]
		public static void ReportDefenderProblem(string applicationName, Exception exc, string failingFolder)
		{
			var heading = LocalizationManager.GetString("Errors.DefenderFolderProtectionHeading",
				"{0} cannot write to the folder {1}.");
			var mainMsg = LocalizationManager.GetString("Errors.DefenderFolderProtection",
				"This might be caused by Windows Defender \"Controlled Folder Access\" or some " +
				"other virus or malware protection. You can search online or contact a local " +
				"support person for help with configuring Windows Defender (and other antivirus " +
				"software) to allow this program to work.");
			var msg = string.Format(heading, applicationName, failingFolder) + NewLine + NewLine + mainMsg;
			ErrorReport.NotifyUserOfProblem(exc, msg);
		}

		private static void Cleanup(string testPath)
		{
			// try to clean up behind ourselves
			try
			{
				RobustFile.Delete(testPath);
			}
			catch (Exception)
			{
				// but don't try too hard
			}
		}
	}
}
