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
		public static StringCollection TextFileExtensions
		{
			get { return Properties.Settings.Default.TextFileExtensions; }
		}

		public static StringCollection AudioFileExtensions
		{
			get { return Properties.Settings.Default.AudioFileExtensions; }
		}

		public static StringCollection VideoFileExtensions
		{
			get { return Properties.Settings.Default.VideoFileExtensions; }
		}

		public static StringCollection ImageFileExtensions
		{
			get { return Properties.Settings.Default.ImageFileExtensions; }
		}

		public static StringCollection DatasetFileExtensions
		{
			  get { return Properties.Settings.Default.DatasetFileExtensions; }
		}

		public static StringCollection SoftwareAndFontFileExtensions
		{
			  get { return Properties.Settings.Default.SoftwareAndFontFileExtensions; }
		}

		public static StringCollection PresentationFileExtensions
		{
			  get { return Properties.Settings.Default.PresentationFileExtensions; }
		}

		public static StringCollection MusicalNotationFileExtensions
		{
			  get { return Properties.Settings.Default.MusicalNotationFileExtensions; }
		}

		public static StringCollection ZipFileExtensions
		{
			  get { return Properties.Settings.Default.ZipFileExtensions; }
		}

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
						!File.Exists(destinationPath)
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
								File.Replace(sourcePath, destinationPath, backupPath);
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
			//enhance: this would be clearer if the "OK" button read "Retry", but that's not easily changable.
			var result = ErrorReport.NotifyUserOfProblem(new ShowAlwaysPolicy(), "Give Up", ErrorResult.No,
				message);
			if (result == ErrorResult.No)
			{
				throw error; // pass it up to the caller
			}
		}

		[Obsolete("Use FileHelper.IsLocked()")]
		public static bool IsFileLocked(string filePath)
		{
			return FileHelper.IsLocked(filePath);
		}

		[Obsolete("Use FileHelper.Grep()")]
		public static bool GrepFile(string inputPath, string pattern)
		{
			return FileHelper.Grep(inputPath, pattern);
		}

		/// <summary>
		/// Make sure the given <paramref name="pathToFile"/> file is 'valid'.
		///
		/// Valid means that
		///		1. <paramref name="pathToFile"/> must not be null or an empty string
		///		2. <paramref name="pathToFile"/> must exist, and
		///		3. The extension for <paramref name="pathToFile"/> must equal <paramref name="expectedExtension"/>
		///			(Or both must be null)
		/// </summary>
		[Obsolete("Use PathHelper.CheckValidPathname()")]
		public static bool CheckValidPathname(string pathToFile, string expectedExtension)
		{
			return PathHelper.CheckValidPathname(pathToFile, expectedExtension);
		}

		[Obsolete("Use RobustFile.ReplaceByCopyDelete()")]
		public static void ReplaceByCopyDelete(string sourcePath, string destinationPath, string backupPath)
		{
			RobustFile.ReplaceByCopyDelete(sourcePath, destinationPath, backupPath);
		}

		/// <summary>
		/// When calling external exe's on Windows any non-ascii characters can get converted to '?'. This
		/// will convert them to 8.3 format which is all ascii (and do nothing on Linux).
		/// </summary>
		[Obsolete("Use PathHelper.MakePathSafeFromEncodingProblems()")]
		public static string MakePathSafeFromEncodingProblems(string path)
		{
			return PathHelper.MakePathSafeFromEncodingProblems(path);
		}

		/// <summary>
		/// Normalize the path so that it uses forward slashes instead of backslashes. This is
		/// useful when a path gets read from a file that gets shared between Windows and Linux -
		/// if the path contains backslashes it can't be found on Linux.
		/// </summary>
		[Obsolete("Use PathHelper.NormalizePath()")]
		public static string NormalizePath(string path)
		{
			return PathHelper.NormalizePath(path);
		}

		/// <summary>
		/// Strips file URI prefix from the beginning of a file URI string, and keeps
		/// a beginning slash if in Linux.
		/// eg "file:///C:/Windows" becomes "C:/Windows" in Windows, and
		/// "file:///usr/bin" becomes "/usr/bin" in Linux.
		/// Returns the input unchanged if it does not begin with "file:".
		///
		/// Does not convert the result into a valid path or a path using current platform
		/// path separators.
		///
		/// See uri.LocalPath, http://en.wikipedia.org/wiki/File_URI , and
		/// http://blogs.msdn.com/b/ie/archive/2006/12/06/file-uris-in-windows.aspx .
		/// </summary>
		[Obsolete("Use PathHelper.StripFilePrefix()")]
		public static string StripFilePrefix(string fileString)
		{
			return PathHelper.StripFilePrefix(fileString);
		}

	}
}