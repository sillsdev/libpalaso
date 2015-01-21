using System;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Palaso.Reporting;

namespace Palaso.IO
{
	public class FileUtils
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

		public static bool IsFileLocked(string filePath)
		{
			if (filePath == null || !File.Exists(filePath))
				return false;

			try
			{
				File.OpenWrite(filePath).Close();
				return false;
			}
			catch
			{
				return true;
			}
		}

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

		public static bool GrepFile(string inputPath, string pattern)
		{
			Regex regex = new Regex(pattern, RegexOptions.Compiled);

			using (StreamReader reader = File.OpenText(inputPath))
			{
				while (!reader.EndOfStream)
				{
					if (regex.IsMatch(reader.ReadLine()))
					{
						return true;
					}
				}
				reader.Close();
			}
			return false;
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
		public static bool CheckValidPathname(string pathToFile, string expectedExtension)
		{
			var extension = ((expectedExtension == null) || (expectedExtension.Trim() == String.Empty))
								? null
								: expectedExtension.StartsWith(".") ? expectedExtension.Substring(1) : expectedExtension;

			if (string.IsNullOrEmpty(pathToFile) || !File.Exists(pathToFile))
				return false;

			var actualExtension = Path.GetExtension(pathToFile);
			if (actualExtension == String.Empty)
				actualExtension = null;
			return (actualExtension == null && extension == null) || (actualExtension != null && extension != null &&
				   actualExtension.ToLowerInvariant() == "." + extension.ToLowerInvariant());
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
						(!string.IsNullOrEmpty(backupPath) && !PathUtilities.PathsAreOnSameVolume(sourcePath,backupPath))
						||
						!PathUtilities.PathsAreOnSameVolume(sourcePath, destinationPath)
						||
						!File.Exists(destinationPath)
						)
					{
						//can't use File.Replace or File.Move across volumes (sigh)
						if (!string.IsNullOrEmpty(backupPath) && File.Exists(destinationPath))
						{
							File.Copy(destinationPath, backupPath, true);
						}
						File.Copy(sourcePath, destinationPath, true);
						File.Delete(sourcePath);
					}
					else
					{
						var giveUpTime = DateTime.Now.AddSeconds(5);
						Exception theProblem;
						do
						{
							try
							{
								theProblem = null;
								File.Replace(sourcePath, destinationPath, backupPath);
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
#if !MONO
			return path.StartsWith("//") || path.StartsWith("\\\\");
#else
			return false; // we will soon be requesting some testing with networks on Linux; 
			//as a result of that, we might need to do something here, too. Or maybe not.
#endif
		}

		private static void ReportFailedReplacement(string destinationPath, Exception error)
		{
			var result = ErrorReport.NotifyUserOfProblem(new ShowAlwaysPolicy(), "Give Up", ErrorResult.No,
				EntryAssembly.ProductName + " was unable to update the file '" + destinationPath + "'.  Please ensure there is not another copy of this program running, nor any other program that might have that file open, then click the 'OK' button below.\r\nThe error was: \r\n" + error.Message);
			if (result == ErrorResult.No)
			{
				throw error; // pass it up to the caller
			}
		}

#if !MONO
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern uint GetShortPathName(
		   [MarshalAs(UnmanagedType.LPTStr)]string lpszLongPath,
		   [MarshalAs(UnmanagedType.LPTStr)]StringBuilder lpszShortPath,
		   uint cchBuffer);
#endif

		/// <summary>
		/// When calling external exe's on Windows any non-ascii characters can get converted to '?'. This
		/// will convert them to 8.3 format which is all ascii (and do nothing on Linux).
		/// </summary>
		public static string MakePathSafeFromEncodingProblems(string path)
		{
#if MONO
			return path;//Linux doesn't have these problems, far as I know
#else
			const int MAXPATH = 260;
			var shortBuilder = new StringBuilder(MAXPATH);
			GetShortPathName(path, shortBuilder, (uint)shortBuilder.Capacity);
			return shortBuilder.ToString();
#endif
		}

		/// <summary>
		/// Normalize the path so that it uses forward slashes instead of backslashes. This is
		/// useful when a path gets read from a file that gets shared between Windows and Linux -
		/// if the path contains backslashes it can't be found on Linux.
		/// </summary>
		public static string NormalizePath(string path)
		{
			return path.Replace('\\', '/');
		}
	}
}