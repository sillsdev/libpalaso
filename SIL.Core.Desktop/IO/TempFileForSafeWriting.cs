using System;
using System.IO;
using SIL.Reporting;

namespace SIL.IO
{
	/// <summary>
	/// Provides an easy way to write to a file and then, if there are no problems, safely replace
	/// the existing copy with the new one as atomically as we can, moving the previous version
	/// to a ".bak" copy.
	/// </summary>
	public class TempFileForSafeWriting
	{
		private readonly string _realFilePath;
		private readonly string _tempPath;

		public TempFileForSafeWriting(string realFilePath)
		{
			_realFilePath = realFilePath;
			_tempPath = Path.GetTempFileName();
		}

		public string TempFilePath
		{
			get { return _tempPath; }
		}

		public bool SimulateVolumeThatCannotHandleFileReplace;

		public void WriteWasSuccessful()
		{
			//get it onto the same volume for sure
			string pending = _realFilePath + ".pending";
			if (RobustFile.Exists(pending))
			{
				RobustFile.Delete(pending);
			}
			RobustFile.Move(_tempPath, pending); //NB: Replace() is tempting but fails across volumes
			if (RobustFile.Exists(_realFilePath))
			{
				SafeReplace(pending, _realFilePath, _realFilePath + ".bak");
			}
			else
			{
				RobustFile.Move(pending, _realFilePath);
			}
		}

		private void SafeReplace(string sourceFilePath, string destinationFilePath, string backupFilePath)
		{
			try
			{
				if (SimulateVolumeThatCannotHandleFileReplace)
				{
					throw new IOException("dummy");
				}

				if (RobustFile.Exists(destinationFilePath))
				{
					//this one *might* be atomic, though no guarantees, apparently
					RobustFile.Replace(sourceFilePath, destinationFilePath, backupFilePath);
				}
				else // I think it's confusing to find empty ".bak" files, we don't mention the bak file if the original is missing
				{
					// "Pass null to the destinationBackupFileName parameter if you do not want to create a backup of the file being replaced."
					RobustFile.Replace(sourceFilePath, destinationFilePath, null);
				}
			}
			//NB: UnauthorizedAccessException, which we get in BL-322, is not a subclass of IOException
			catch (Exception error)
			{
				Logger.WriteMinorEvent("TempFileForSafeWriting got " + error.Message + "  Will use fall back method.");

				// This one for where on JAARS network-mapped volumes, the RobustFile.Replace fails
				// See https://silbloom.myjetbrains.com/youtrack/issue/BL-3222 

				if (RobustFile.Exists(destinationFilePath))
				{
					if (RobustFile.Exists(backupFilePath))
					{
						RobustFile.Delete(backupFilePath);
					}

					RobustFile.Move(destinationFilePath, backupFilePath);
					RobustFile.Delete(destinationFilePath);
				}
				RobustFile.Move(sourceFilePath, destinationFilePath);
			}
		}
	}
}