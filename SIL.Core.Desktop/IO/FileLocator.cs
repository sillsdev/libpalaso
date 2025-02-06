// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.IO
{
	public interface IFileLocator
	{
		string LocateFile(string fileName);
		string LocateFile(string fileName, string descriptionForErrorMessage);
		[PublicAPI]
		string LocateOptionalFile(string fileName);
		[PublicAPI]
		string LocateFileWithThrow(string fileName);
		string LocateDirectory(string directoryName);
		[PublicAPI]
		string LocateDirectoryWithThrow(string directoryName);
		string LocateDirectory(string directoryName, string descriptionForErrorMessage);
		[PublicAPI]
		IFileLocator CloneAndCustomize(IEnumerable<string> addedSearchPaths);
	}

	public interface IChangeableFileLocator : IFileLocator
	{
		[PublicAPI]
		void AddPath(string path);
		void RemovePath(string path);
	}

	public class FileLocator : IChangeableFileLocator
	{
		public FileLocator(IEnumerable<string> searchPaths)
		{
			SearchPaths = new List<string>(searchPaths);
		}

		public string LocateFile(string fileName)
		{
			foreach (var path in GetSearchPaths())
			{
				var fullPath = Path.Combine(path, fileName);
				if(File.Exists(fullPath))
					return fullPath;
			}
			return string.Empty;
		}

		/// <summary>
		/// Subclasses (e.g. in Bloom) override this to provide a dynamic set of paths
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<string> GetSearchPaths()
		{
			return SearchPaths;
		}

		public string LocateFile(string fileName, string descriptionForErrorMessage)
		{

			var path = LocateFile(fileName);
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				ErrorReport.NotifyUserOfProblem(
					"{0} could not find the {1}.  It expected to find it in one of these locations: {2}",
					UsageReporter.AppNameToUseInDialogs, descriptionForErrorMessage, string.Join(", ", GetSearchPaths())
					);
			}
			return path;
		}

		public string LocateDirectory(string directoryName)
		{
			foreach (var path in GetSearchPaths())
			{
				var fullPath = Path.Combine(path, directoryName);
				if (Directory.Exists(fullPath))
					return fullPath;
			}
			return string.Empty;
		}
		public string LocateDirectory(string directoryName, string descriptionForErrorMessage)
		{

			var path = LocateDirectory(directoryName);
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
			{
				ErrorReport.NotifyUserOfProblem(
					"{0} could not find the {1}.  It expected to find it in one of these locations: {2}",
					UsageReporter.AppNameToUseInDialogs, descriptionForErrorMessage, string.Join(", ", SearchPaths)
					);
			}
			return path;
		}
		public string LocateDirectoryWithThrow(string directoryName)
		{
			var path = LocateDirectory(directoryName);
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
			{
				throw new ApplicationException(String.Format("Could not find {0}.  It expected to find it in one of these locations: {1}",
					directoryName, string.Join(Environment.NewLine, SearchPaths)));
			}
			return path;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>null if not found</returns>
		public string LocateOptionalFile(string fileName)
		{
			var path = LocateFile(fileName);
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return null;
			}
			return path;
		}

		/// <summary>
		/// Throws ApplicationException if not found.
		/// </summary>
		public string LocateFileWithThrow(string fileName)
		{
			var path = LocateFile(fileName);
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				throw new ApplicationException("Could not find " + fileName + ". It expected to find it in one of these locations: " + Environment.NewLine + string.Join(Environment.NewLine, SearchPaths));
			}
			return path;
		}

		protected List<string> SearchPaths { get; }

		/// <summary>
		/// Use this when you can't mess with the whole application's FileLocator, but you want to add a path or two, e.g., the folder of the current book in Bloom.
		/// </summary>
		/// <param name="addedSearchPaths"></param>
		/// <returns></returns>
		public virtual IFileLocator CloneAndCustomize(IEnumerable<string> addedSearchPaths)
		{
			return new FileLocator(new List<string>(SearchPaths.Concat(addedSearchPaths)));
		}

		#region Methods for locating program file associated with a file
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// returns the full path to the application program used to open files having the
		/// specified extension/type. The fileExtension can be with or without
		/// the preceding period. If no associated application can be found or the associated
		/// program does not actually exist, null is returned.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string GetDefaultProgramForFileType(string fileExtension)
		{
			if (!fileExtension.StartsWith("."))
				fileExtension = "." + fileExtension;

			if (Platform.IsWindows)
				return GetDefaultWindowsProgramForFileType(fileExtension);

			if (Platform.IsMac)
				return GetDefaultMacProgramForFileType(fileExtension);

			if (Platform.IsLinux)
				return GetDefaultLinuxProgramForFileType(fileExtension);

			throw new PlatformNotSupportedException("This operating system is not supported.");
		}

		[DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
		private static extern uint AssocQueryString(
			uint flags,
			int str,
			string pszAssoc,
			string pszExtra,
			[Out] StringBuilder pszOut,
			ref uint pcchOut);

		private static string GetDefaultWindowsProgramForFileType(string fileExtension)
		{
			const int assocStrExecutable = 2;
			uint length = 260;
			var sb = new StringBuilder((int)length);

			var result = AssocQueryString(0, assocStrExecutable, fileExtension, null, sb, ref length);

			if (result != 0 || sb.Length == 0)
				return null;

			var path = sb.ToString();
			return Path.GetFileName(path) != "OpenWith.exe" && File.Exists(path) ? path : null;
		}

		private static string GetDefaultMacProgramForFileType(string fileExtension)
		{
			try
			{
				string filePath = $"/tmp/dummy{fileExtension}";
				Process.Start("touch", filePath)?.WaitForExit();

				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "open",
						Arguments = "-Ra " + filePath,
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true
					}
				};

				process.Start();
				var output = process.StandardOutput.ReadToEnd().Trim();
				process.WaitForExit();

				return string.IsNullOrEmpty(output) ? null : output;
			}
			catch
			{
				return null;
			}
		}

		private static string GetDefaultLinuxProgramForFileType(string fileExtension)
		{
			try
			{
				var mimeProcess = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "xdg-mime",
						Arguments = "query default " + fileExtension,
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true
					}
				};

				mimeProcess.Start();
				var desktopEntry = mimeProcess.StandardOutput.ReadToEnd().Trim();
				mimeProcess.WaitForExit();

				if (string.IsNullOrEmpty(desktopEntry))
					return null;

				// Check if the executable associated with the desktop entry exists
				var whichProcess = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "which",
						Arguments = desktopEntry,
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true
					}
				};

				whichProcess.Start();
				string executablePath = whichProcess.StandardOutput.ReadToEnd().Trim();
				whichProcess.WaitForExit();

				return string.IsNullOrEmpty(executablePath) ? null : executablePath;
			}
			catch
			{
				return null;
			}
		}
		#endregion

		public virtual void AddPath(string path)
		{
			SearchPaths.Add(path);
		}

		public void RemovePath(string path)
		{
			SearchPaths.Remove(path);
		}
	}
}
