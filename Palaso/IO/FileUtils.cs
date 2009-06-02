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
		/// NB: this is maybe not as general purpose as it looks.  It is  written for the case where
		/// failing is not really an option, so it lets the user retry and tells them to kill the app
		/// if they can't resolve it.  Aside from the fact that having a Die Die! button would be nice,
		/// not all cases are going to need to fail like that.  We could change it:
		/// 1) provide a "give up" button
		/// 2) return FALSE if they give up
		/// 3) leave it to the caller to abort the action in that case.
		/// </summary>
		public static void ReplaceFileWithUserInteractionIfNeeded(string tempPath,
																  string inputPath,
																  string backupPath)
		{
			bool succeeded = false;
			do
			{
				try
				{
					File.Replace(tempPath, inputPath, backupPath);
					succeeded = true;
				}
				catch (UnauthorizedAccessException error)
				{
					ErrorReport.NotifyUserOfProblem(Application.ProductName +
						" was unable to update the file '"+inputPath+"'. The program was trying to copy the temp file from '"+tempPath+"' and create a backup at '"+backupPath+"'. If you cannot resolve this now, you'll need to kill this program so that files don't fall out of sync.  The error was: \r\n"+error.Message);
				}

				catch (IOException error)
				{
					ErrorReport.NotifyUserOfProblem(Application.ProductName +
													" was unable to update the file '"+inputPath+"'.  Please ensure there is not another copy of this program running, nor any other program that might have that file open, then click the 'OK' button below. If you cannot figure out what program has the file open, the best choice is to kill this program (on Windows, use the Task Manager (ctrl+alt+del)). The error was: \r\n"+error.Message);
				}
			}
			while (!succeeded);
		}

	}
}