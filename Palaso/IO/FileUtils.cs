using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Palaso.Reporting;

namespace Palaso.IO
{
	public class FileUtils
	{

		/// <summary>
		/// NB: This will show a dialog if the file writing can't be done (subject to Palaso.Reporting settings).
		/// It will throw whatever exception was encountered, if the user can't resolve it.
		/// </summary>
		/// <param name="inputPath"></param>
		/// <param name="pattern"></param>
		/// <param name="replaceWith"></param>
		public static void GrepFile(string inputPath, string pattern, string replaceWith)
		{
			Regex regex = new Regex(pattern, RegexOptions.Compiled);
			string tempPath = inputPath + ".tmp";

			using (StreamReader reader = File.OpenText(inputPath))
			{
				using (StreamWriter writer = new StreamWriter(tempPath))
				{
					while (!reader.EndOfStream)
					{
						writer.WriteLine(regex.Replace(reader.ReadLine(), replaceWith));
					}
					writer.Close();
				}
				reader.Close();
			}
			//string backupPath = GetUniqueFileName(inputPath);
			string backupPath = inputPath + ".bak";

			ReplaceFileWithUserInteractionIfNeeded(tempPath, inputPath, backupPath);
		}

		/// <summary>
		/// If there is a problem doing the replace, a dialog is shown which tells the user
		/// what happened, and lets them try to fix it.  It also lets them "Give up", in
		/// which case this returns False.
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="destinationPath"></param>
		/// <param name="backupPath">can be null if you don't want a replacement</param>
		/// <returns>if the user gives up, throws whatever exception the file system gave</returns>
		public static void ReplaceFileWithUserInteractionIfNeeded(string sourcePath,
																  string destinationPath,
																  string backupPath)
		{
			bool succeeded = false;
			do
			{
				try
				{
					if ((Path.GetPathRoot(sourcePath) != Path.GetPathRoot(destinationPath))
						||
						((!string.IsNullOrEmpty(backupPath)) && (Path.GetPathRoot(sourcePath) != Path.GetPathRoot(backupPath))))
					{
						//can't use File.Replace or File.Move across volumes (sigh)
						if(!string.IsNullOrEmpty(backupPath) && File.Exists(destinationPath))
						{
							File.Copy(destinationPath,backupPath,true);
						}
						File.Copy(sourcePath, destinationPath, true);
						File.Delete(sourcePath);
					}
					else
					{
						File.Replace(sourcePath, destinationPath, backupPath);
					}
					succeeded = true;
				}
				catch (UnauthorizedAccessException error)
				{
					ReportFailedReplacement(destinationPath, error);
				}
				catch (IOException error)
				{
					ReportFailedReplacement(destinationPath, error);
				}
			}
			while (!succeeded);
		}

		private static void ReportFailedReplacement(string destinationPath, Exception error)
		{
			var result = ErrorReport.NotifyUserOfProblem(new ShowAlwaysPolicy(), "Give Up", DialogResult.No,
				Application.ProductName + " was unable to update the file '" + destinationPath + "'.  Please ensure there is not another copy of this program running, nor any other program that might have that file open, then click the 'OK' button below.\r\nThe error was: \r\n" + error.Message);
			if(result == DialogResult.No)
			{
				throw error; // pass it up to the caller
			}
		}
	}
}