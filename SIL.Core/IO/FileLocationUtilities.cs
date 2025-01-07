using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SIL.PlatformUtilities;
using SIL.Reflection;
using SIL.Reporting;

namespace SIL.IO
{
	public static class FileLocationUtilities
	{
		/// <summary>
		/// Gives the directory of either the project folder (if running from visual studio), or
		/// the installation folder.  Helpful for finding templates and things; by using this,
		/// you don't have to copy those files into the build directory during development.
		/// It assumes your build directory has "output" as part of its path.
		/// </summary>
		/// <returns></returns>
		public static string DirectoryOfApplicationOrSolution
		{
			get
			{
				var path = DirectoryOfTheApplicationExecutable;
				return GetProjectDirectory(path);
			}
		}

		/// <summary>
		/// Gives the directory of either the project folder (if running from visual studio), or
		/// the installation folder.  Helpful for finding templates and things; by using this,
		/// you don't have to copy those files into the build directory during development.
		/// It assumes your build directory has "output" as part of its path.
		/// </summary>
		/// <returns></returns>
		private static string GetProjectDirectory(string path)
		{
			var sep = Path.DirectorySeparatorChar;
			var i = path.ToLower().LastIndexOf(sep + "output" + sep, StringComparison.Ordinal);

			return (i > -1) ? path.Substring(0, i + 1) : path;
		}

		public static string DirectoryOfTheApplicationExecutable => ReflectionHelper.DirectoryOfTheApplicationExecutable;

		private static string LocateExecutableDistributedWithApplication(string[] partsOfTheSubPath)
		{
			var exe = GetFileDistributedWithApplication(true, partsOfTheSubPath);
			if (!string.IsNullOrEmpty(exe))
				return exe;

			var newParts = new List<string>(partsOfTheSubPath);
			newParts.Insert(0, Platform.IsWindows ? "Windows" : "Linux");
			return GetFileDistributedWithApplication(true, newParts.ToArray());
		}

		/// <summary>
		/// Find an executable file which, on a development machine, lives in
		/// [solution]/[distFileFolderName]/[subPath1]/[subPathN] or
		/// [solution]/[distFileFolderName]/[platform]/[subPath1]/[subPathN]
		/// and when installed, lives in
		/// [applicationFolder]/[distFileFolderName]/[subPath1]/[subPathN] or
		/// [applicationFolder]/[distFileFolderName]/[platform]/[subPath1]/[subPathN] or
		/// [applicationFolder]/[subPath1]/[subPathN]. If the executable can't be found we
		/// search in the ProgramFiles folder ([ProgramFiles]/[subPath1]/[subPathN]) on Windows,
		/// and in the folders included in the PATH environment variable on Linux.
		/// When the executable has a prefix of ".exe" we're running on Linux we also
		/// search for files without the prefix.
		/// </summary>
		/// <example>GetFileDistributedWithApplication("exiftool.exe");</example>
		public static string LocateExecutable(bool throwExceptionIfNotFound, params string[] partsOfTheSubPath)
		{
			var exe = LocateExecutableDistributedWithApplication(partsOfTheSubPath);
			if (string.IsNullOrEmpty(exe) && Platform.IsUnix)
			{
				var newParts = new List<string>(partsOfTheSubPath);
				newParts[newParts.Count - 1] = Path.GetFileNameWithoutExtension(newParts.Last());
				exe = LocateExecutableDistributedWithApplication(newParts.ToArray());
			}

			if (string.IsNullOrEmpty(exe))
			{
				var newParts = new List<string>(partsOfTheSubPath);
				var exeFileName = newParts.Last();
				newParts.Remove(exeFileName);
				exe = LocateInProgramFiles(exeFileName, true, newParts.ToArray());

				if (string.IsNullOrEmpty(exe) && Platform.IsUnix)
				{
					exeFileName = Path.GetFileNameWithoutExtension(exeFileName);
					exe = LocateInProgramFiles(exeFileName, true, newParts.ToArray());
				}
			}

			if (!string.IsNullOrEmpty(exe))
				return exe;

			if (!throwExceptionIfNotFound)
				return null;

			throw new ApplicationException($"Could not locate the required executable, {Path.Combine(partsOfTheSubPath)}");

		}

		public static string LocateExecutable(params string[] partsOfTheSubPath)
		{
			return LocateExecutable(true, partsOfTheSubPath);
		}

		private static string[] DirectoriesHoldingFiles => new[] {string.Empty, "DistFiles",
			"common" /*for WeSay*/, "src" /*for Bloom*/};

		/// <summary>
		/// Find a file which, on a development machine, lives in [solution]/[distFileFolderName]/[subPath],
		/// and when installed, lives in
		/// [applicationFolder]/[distFileFolderName]/[subPath1]/[subPathN]  or
		/// [applicationFolder]/[subPath]/[subPathN]
		/// </summary>
		/// <returns>The path to the file, or <c>null</c> if the file is not found
		/// and <paramref name="optional"/> is set to <c>true</c>.</returns>
		/// <exception cref="ApplicationException">If the file is not found and
		/// <paramref name="optional"/> is <c>false</c>.</exception>
		/// <example>GetFileDistributedWithApplication(false, "info", "releaseNotes.htm");</example>
		public static string GetFileDistributedWithApplication(bool optional, params string[] partsOfTheSubPath)
		{
			foreach (var directoryHoldingFiles in DirectoriesHoldingFiles)
			{
				var path = Path.Combine(DirectoryOfApplicationOrSolution,
					directoryHoldingFiles, Path.Combine(partsOfTheSubPath));
				if (File.Exists(path))
					return path;
			}

			if (optional)
				return null;

			throw new ApplicationException($"Could not locate the required file, {Path.Combine(partsOfTheSubPath)}");
		}

		/// <summary>
		/// Find a file which MUST EXIST. On a development machine, lives in [solution]/[distFileFolderName]/[subPath],
		/// and when installed, lives in
		/// [applicationFolder]/[distFileFolderName]/[subPath1]/[subPathN]  or
		/// [applicationFolder]/[subPath]/[subPathN]
		/// </summary>
		/// <example>GetFileDistributedWithApplication("info", "releaseNotes.htm");</example>
		public static string GetFileDistributedWithApplication(params string[] partsOfTheSubPath)
		{
			return GetFileDistributedWithApplication(false, partsOfTheSubPath);
		}

		/// <summary>
		/// Find a directory which, on a development machine, lives in [solution]/DistFiles/[subPath],
		/// and when installed, lives in [applicationFolder]/[subPath1]/[subPathN]
		/// </summary>
		/// <returns>The path to the directory, or <c>null</c> if the directory is not found
		/// and <paramref name="optional"/> is set to <c>true</c>.</returns>
		/// <exception cref="ArgumentException">If the directory is not found and
		/// <paramref name="optional"/> is <c>false</c>.</exception>
		/// <example>GetDirectoryDistributedWithApplication(false, "info", "releaseNotes.htm");</example>
		public static string GetDirectoryDistributedWithApplication(bool optional, params string[] partsOfTheSubPath)
		{
			var subPath = Path.Combine(partsOfTheSubPath);
			var path = GetDirectoryDistributedWithApplication(DirectoryOfApplicationOrSolution,
				subPath);
			if (Directory.Exists(path))
				return path;

			var thisDirectory = GetProjectDirectory(Path.GetDirectoryName(typeof(FileLocationUtilities).Assembly.Location));
			if (thisDirectory != DirectoryOfApplicationOrSolution)
			{
				path = GetDirectoryDistributedWithApplication(thisDirectory, subPath);
			}

			if (Directory.Exists(path))
				return path;

			if (optional)
				return null;

			var message = new StringBuilder("Could not find the directory ");
			message.Append(subPath);
			message.Append(". We looked in ");
			message.Append(DirectoryOfApplicationOrSolution);
			if (thisDirectory != DirectoryOfApplicationOrSolution)
				message.Append($" and {thisDirectory} ");

			message.Append(" and in its subdirectories ");
			message.Append(string.Join(", ", DirectoriesHoldingFiles));
			throw new ArgumentException(message.ToString());
		}

		private static string GetDirectoryDistributedWithApplication(string directory, string subPath)
		{
			var path = Path.Combine(directory, subPath);
			if (Directory.Exists(path))
				return path;

			foreach (var directoryHoldingFiles in DirectoriesHoldingFiles)
			{
				path = Path.Combine(directory, directoryHoldingFiles, subPath);
				if (Directory.Exists(path))
					return path;
			}

			return null;
		}

		/// <summary>
		/// Find a directory which MUST EXIST. On a development machine, lives in [solution]/DistFiles/[subPath],
		/// and when installed, lives in
		/// [applicationFolder]/[subPath1]/[subPathN]
		/// </summary>
		/// <example>GetFileDistributedWithApplication("info", "releaseNotes.htm");</example>
		public static string GetDirectoryDistributedWithApplication(params string[] partsOfTheSubPath)
		{
			return GetDirectoryDistributedWithApplication(false, partsOfTheSubPath);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Locates the specified program in the program files folder, returning the full path
		/// (including the exeName). Searching is only done in the OS's program files folder(s)
		/// and sub folders. On a 64-bit Windows OS, both program files folders are searched.
		///
		/// When subFoldersToSearch are specified:
		///		- The specified sub folders are searched first.
		///		- If fallBackToDeepSearch is true, then after the search in the specified sub
		///		  folders fails, a deep search will be made in each of the sub folders, and
		///		  if that fails, the entire program files folder (and all sub folders)
		///		  is searched.
		///		- If fallBackToDeepSearch is false, only subFoldersToSearch are searched.
		///
		/// When subFoldersToSearch are not specified:
		///		- If fallBackToDeepSearch is true, then the entire program files folder (and all
		///		  sub folders) is searched.
		///		- If fallBackToDeepSearch is false, then only the top-level program files
		///		  folder is searched.
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string LocateInProgramFiles(string exeName, bool fallBackToDeepSearch,
			params string[] subFoldersToSearch)
		{
			if (Platform.IsWindows)
			{
				var tgtPath = LocateInProgramFilesUsingShallowSearch(exeName, subFoldersToSearch);
				if (tgtPath != null)
					return tgtPath;

				return !fallBackToDeepSearch
					? null
					: LocateInProgramFilesUsingDeepSearch(exeName, subFoldersToSearch);
			}

			return fallBackToDeepSearch ? LocateInProgramFilesUsingDeepSearch(exeName) :
				LocateInProgramFilesUsingShallowSearch(exeName);
		}

		private static string LocateInProgramFilesUsingShallowSearch(string exeName,
			params string[] subFoldersToSearch)
		{
			return LocateInProgramFilesFolder(exeName, SearchOption.TopDirectoryOnly, subFoldersToSearch);
		}

		/// ------------------------------------------------------------------------------------
		private static string LocateInProgramFilesUsingDeepSearch(string exeName,
			params string[] subFoldersToSearch)
		{
			return LocateInProgramFilesFolder(exeName, SearchOption.AllDirectories, subFoldersToSearch);
		}

		/// ------------------------------------------------------------------------------------
		private static string LocateInProgramFilesFolder(string exeName, SearchOption srcOption,
			params string[] subFoldersToSearch)
		{
			if (subFoldersToSearch.Length == 0)
				subFoldersToSearch = new[] { string.Empty };

			foreach (var progFolder in GetPossibleProgramFilesFolders().Where(Directory.Exists))
			{
				// calling Directory.GetFiles("C:\Program Files", exeName, SearchOption.AllDirectories) will fail
				// if even one of the children of the Program Files doesn't allow you to search it.
				// So instead we first gather all the child directories, and then search those.
				// Some will give us access denied, and that's fine, we skip them.
				// But we don't want to look in child directories on Linux because GetPossibleProgramFilesFolders()
				// gives us the individual elements of the PATH environment variable, and these specify exactly
				// those directories that should be searched for program files.
				foreach (var path in subFoldersToSearch.Select(sf => Path.Combine(progFolder, sf)).Where(Directory.Exists))
				{
					if (Platform.IsWindows)
					{
						string[] subDirectories = null;
						try
						{
							subDirectories = DirectoryHelper.GetSafeDirectories(path);
						}
						catch (Exception e)
						{
							Logger.WriteError(e);
						}

						if (subDirectories != null)
						{
							foreach (var subDir in subDirectories)
							{
								var tgtPath = GetFiles(exeName, srcOption, subDir);
								if (!string.IsNullOrEmpty(tgtPath))
									return tgtPath;
							}
						}
					}
					else
					{
						var tgtPath = GetFiles(exeName, srcOption, path);
						if (!string.IsNullOrEmpty(tgtPath))
							return tgtPath;
					}
				}
			}

			return null;
		}

		private static string GetFiles(string exeName, SearchOption srcOption, string subDir)
		{
			try
			{
				return Directory.GetFiles(subDir, exeName, srcOption)
					.OrderBy(filename => filename).FirstOrDefault();
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				//swallow. Some paths, we aren't allowed to look in (like Google Chrome crash reports)
			}
			return null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a list of the possible paths to the program files folder, taking into
		/// account that 2 often (or always?) exist in a Win64 OS (i.e. "Program Files" and
		/// "Program Files (x86)"). On Mono we return the folders of the PATH variable.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static IEnumerable<string> GetPossibleProgramFilesFolders()
		{
			if (!Platform.IsWindows)
			{
				var path = Environment.GetEnvironmentVariable("PATH");
				if (!string.IsNullOrEmpty(path))
				{
					foreach (var dir in path.Split(':'))
						yield return dir;
				}
				yield return "/opt"; // RAMP is installed in the /opt directory by default
			}

			yield return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			if (Environment.Is64BitProcess)
				yield return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
		}
	}
}