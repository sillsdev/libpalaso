using System;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.IO
{
	/// <summary>
	/// Desktop-specific utility methods for processing files, e.g. methods that report to the user.
	/// </summary>
	public static class FileUtils
	{
		public static StringCollection TextFileExtensions => Properties.Settings.Default.TextFileExtensions;

		public static StringCollection AudioFileExtensions => Properties.Settings.Default.AudioFileExtensions;

		public static StringCollection VideoFileExtensions => Properties.Settings.Default.VideoFileExtensions;

		public static StringCollection ImageFileExtensions => Properties.Settings.Default.ImageFileExtensions;

		public static StringCollection DatasetFileExtensions => Properties.Settings.Default.DatasetFileExtensions;

		public static StringCollection SoftwareAndFontFileExtensions => Properties.Settings.Default.SoftwareAndFontFileExtensions;

		public static StringCollection PresentationFileExtensions => Properties.Settings.Default.PresentationFileExtensions;

		public static StringCollection MusicalNotationFileExtensions => Properties.Settings.Default.MusicalNotationFileExtensions;

		public static StringCollection ZipFileExtensions => Properties.Settings.Default.ZipFileExtensions;

		public static bool GetIsZipFile(string path)
		{
			return GetIsSpecifiedFileType(ZipFileExtensions, path);
		}

		public static bool GetIsText(string path)
		{
			return GetIsSpecifiedFileType(TextFileExtensions, path);
		}

		public static bool GetIsAudio(string path)
		{
			return GetIsSpecifiedFileType(AudioFileExtensions, path);
		}

		public static bool GetIsVideo(string path)
		{
			return GetIsSpecifiedFileType(VideoFileExtensions, path);
		}

		public static bool GetIsMusicalNotation(string path)
		{
			return GetIsSpecifiedFileType(MusicalNotationFileExtensions, path);
		}

		public static bool GetIsDataset(string path)
		{
			return GetIsSpecifiedFileType(DatasetFileExtensions, path);
		}

		public static bool GetIsSoftwareOrFont(string path)
		{
			return GetIsSpecifiedFileType(SoftwareAndFontFileExtensions, path);
		}

		public static bool GetIsPresentation(string path)
		{
			return GetIsSpecifiedFileType(PresentationFileExtensions, path);
		}

		public static bool GetIsImage(string path)
		{
			return GetIsSpecifiedFileType(ImageFileExtensions, path);
		}

		private static bool GetIsSpecifiedFileType(StringCollection extensions, string path)
		{
			var extension = Path.GetExtension(path);
			return (extension != null) && extensions.Contains(extension.ToLower());
		}

		/// <summary>
		/// NB: This will show a dialog if the file writing can't be done (subject to SIL.Reporting settings).
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
		///
		/// To help with situations where something may temporarily be holding on to the file,
		/// this will retry for up to 5 seconds.
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
					if (UnsafeForFileReplaceMethod(sourcePath) || UnsafeForFileReplaceMethod(destinationPath)
						||
						(!string.IsNullOrEmpty(backupPath) && !PathHelper.AreOnSameVolume(sourcePath,backupPath))
						||
						!PathHelper.AreOnSameVolume(sourcePath, destinationPath)
						||
						!RobustFile.Exists(destinationPath)
						)
					{
						//can't use File.Replace or File.Move across volumes (sigh)
						RobustFile.ReplaceByCopyDelete(sourcePath, destinationPath, backupPath);
					}
					else
					{
						var giveUpTime = DateTime.Now.AddSeconds(5);
						Exception theProblem;
						do
						{
							theProblem = null;
							try
							{
								RobustFile.Replace(sourcePath, destinationPath, backupPath);
							}
							catch (UnauthorizedAccessException uae)
							{
								// We were getting this while trying to Replace on a JAARS network drive.
								// The network drive is U:\ which maps to \\waxhaw\users\{username},
								// so it doesn't get caught by the checks above.
								// Both files were in the same directory and there were no permissions issues,
								// but the Replace command was failing with "Access to the path is denied." anyway.
								// I never could figure out why. See http://issues.bloomlibrary.org/youtrack/issue/BL-4179
								try
								{
									RobustFile.ReplaceByCopyDelete(sourcePath, destinationPath, backupPath);
								}
								catch
								{
									// Though it probably doesn't matter, report the original exception since we prefer Replace to CopyDelete.
									theProblem = uae;
									Thread.Sleep(100);
								}
							}
							catch (Exception e)
							{
								theProblem = e;
								Thread.Sleep(100);
							}

						} while (theProblem != null && DateTime.Now < giveUpTime);
						if (theProblem != null)
							throw theProblem;
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

		// NB: I don't actually know for sure that we can't do the replace on these paths; this dev doesn't
		// have a network to test on. What I do know is that the code checking to see if they are on the
		// same drive fails for UNC paths, so this at least gets us past that problem
		private static bool UnsafeForFileReplaceMethod(string path)
		{
			if (Platform.IsWindows)
				return path.StartsWith("//") || path.StartsWith("\\\\");

			return false; // we will soon be requesting some testing with networks on Linux;
			//as a result of that, we might need to do something here, too. Or maybe not.
		}

		private static void ReportFailedReplacement(string destinationPath, Exception error)
		{
			var message = string.Format("{0} was unable to update the file '{1}'.\n"+
			                                  "Possible causes:\n"+
			                                  "* Another copy of this program could be running, or some other program might have the file open or locked (including things like Dropbox and antivirus software).\n" +
			                                  "* The file may be set to 'Read Only'\n"+
			                                  "* The security permissions of this file may be set to deny you access to it.\n\n" +
			                                  "The error was: \n{2}",
				EntryAssembly.ProductName, destinationPath, error.Message);
			message = message.Replace("\n", Environment.NewLine);
			//enhance: this would be clearer if the "OK" button read "Retry", but that's not easily changeable.
			var result = ErrorReport.NotifyUserOfProblem(new ShowAlwaysPolicy(), "Give Up", ErrorResult.No,
				message);
			if (result == ErrorResult.No)
			{
				throw error; // pass it up to the caller
			}
		}
	}
}