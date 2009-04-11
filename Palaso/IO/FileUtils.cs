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

				catch (IOException)
				{
					//nb: we don't want to provide an option to cancel.  Better to crash than cancel.
					ErrorReport.NotifyUserOfProblem(Application.ProductName +
													" was unable to get at the dictionary file to update it.  Please ensure that WeSay isn't running with it open, then click the 'OK' button below. If you cannot figure out what program has the LIFT file open, the best choice is to kill WeSay Configuration Tool using the Task Manager (ctrl+alt+del), so that the configuration does not fall out of sync with the LIFT file.");
				}
			}
			while (!succeeded);
		}

	}
}