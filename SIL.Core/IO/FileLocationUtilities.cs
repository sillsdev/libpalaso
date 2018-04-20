using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SIL.PlatformUtilities;
using SIL.Reflection;

namespace SIL.IO
{
	public class FileLocationUtilities
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
				string path = DirectoryOfTheApplicationExecutable;
				char sep = Path.DirectorySeparatorChar;
				int i = path.ToLower().LastIndexOf(sep + "output" + sep);

				if (i > -1)
				{
					path = path.Substring(0, i + 1);
				}
				return path;
			}
		}

		public static string DirectoryOfTheApplicationExecutable => ReflectionHelper.DirectoryOfTheApplicationExecutable;

		private static string LocateExecutableDistributedWithApplication(string[] partsOfTheSubPath)
		{
			var exe = GetFileDistributedWithApplication(true, partsOfTheSubPath);
			if (string.IsNullOrEmpty(exe))
			{
				var newParts = new List<string>(partsOfTheSubPath);
				newParts.Insert(0, Platform.IsWindows ? "Windows" : "Linux");
				exe = GetFileDistributedWithApplication(true, newParts.ToArray());
			}
			return exe;
		}

		/// <summary>
		/// Find an executable file which, on a development machine, lives in
		/// [solution]/[distFileFolderName]/[subPath1]/[subPathN] or
		/// [solution]/[distFileFolderName]/[platform]/[subPath1]/[subPathN]
		/// and when installed, lives in
		/// [applicationFolder]/[distFileFolderName]/[subPath1]/[subPathN] or
		/// [applicationFolder]/[distFileFolderName]/[platform]/[subPath1]/[subPathN] or
		/// [applicationFolder]/[subPath1]/[subPathN]. If the executable can't be found we
		/// search in the ProgramFiles folder ([programfiles]/[subPath1]/[subPathN]) on Windows,
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
			if (throwExceptionIfNotFound)
			{
				var subPath = string.Empty;
				foreach (var part in partsOfTheSubPath)
				{
					subPath = Path.Combine(subPath, part);
				}
				throw new ApplicationException("Could not locate the required executable, " + subPath);
			}

			return null;
		}

		public static string LocateExecutable(params string[] partsOfTheSubPath)
		{
			return LocateExecutable(true, partsOfTheSubPath);
		}

		/// <summary>
		/// Find a file which, on a development machine, lives in [solution]/[distFileFolderName]/[subPath],
		/// and when installed, lives in
		/// [applicationFolder]/[distFileFolderName]/[subPath1]/[subPathN]  or
		/// [applicationFolder]/[subPath]/[subPathN]
		/// </summary>
		/// <example>GetFileDistributedWithApplication("info", "releaseNotes.htm");</example>
		public static string GetFileDistributedWithApplication(bool optional, params string[] partsOfTheSubPath)
		{
			foreach (var directoryHoldingFiles in new[] {"", "DistFiles", "common" /*for wesay*/, "src" /*for Bloom*/})
			{
				var path = Path.Combine(FileLocationUtilities.DirectoryOfApplicationOrSolution, directoryHoldingFiles);

				foreach (var part in partsOfTheSubPath)
				{
					path = Path.Combine(path, part);
				}
				if (File.Exists(path))
					return path;
			}

			if (optional)
				return null;
			string subpath="";
			foreach (var part in partsOfTheSubPath)
			{
				subpath = Path.Combine(subpath, part);
			}
			throw new ApplicationException("Could not locate the required file, "+ subpath);
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
		/// Find a file which, on a development machine, lives in [solution]/DistFiles/[subPath],
		/// and when installed, lives in
		/// [applicationFolder]/[subPath1]/[subPathN]
		/// </summary>
		/// <example>GetFileDistributedWithApplication("info", "releaseNotes.htm");</example>
		public static string GetDirectoryDistributedWithApplication(bool optional, params string[] partsOfTheSubPath)
		{
			var path = FileLocationUtilities.DirectoryOfApplicationOrSolution;
			foreach (var part in partsOfTheSubPath)
			{
				path = System.IO.Path.Combine(path, part);
			}
			if (Directory.Exists(path))
				return path;

			var directoriesHoldingFiles = new[] {"DistFiles", "common" /*for wesay*/, "src" /*for Bloom*/};
			foreach (var directoryHoldingFiles in directoriesHoldingFiles)
			{
				path = Path.Combine(FileLocationUtilities.DirectoryOfApplicationOrSolution, directoryHoldingFiles);
				foreach (var part in partsOfTheSubPath)
				{
					path = System.IO.Path.Combine(path, part);
				}
				if (Directory.Exists(path))
					return path;
			}

			if (optional && !Directory.Exists(path))
				return null;

			if (!Directory.Exists(path))
			{
				var message = new StringBuilder("Could not find the directory ");
				message.Append(Path.Combine(partsOfTheSubPath));
				message.Append(". We looked in ");
				message.Append(FileLocationUtilities.DirectoryOfApplicationOrSolution);
				message.Append(" and in its subdirectories ");
				message.Append(String.Join(", ", directoriesHoldingFiles));
				throw new ArgumentException(message.ToString());
			}
			return path;
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
		///		  (sub folders) is searched.
		///		- If fallBackToDeepSearch is false, then only the top-level program files
		///		  folder is searched.
		///
		/// Note: For Mono the deep search and shallow search are the same because there
		///		normally are no sub-directories in the program files directories.
		///
		/// Note: For Mono the subFoldersToSearch parameter is ignored.
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

				return (!fallBackToDeepSearch
					? null
					: LocateInProgramFilesUsingDeepSearch(exeName, subFoldersToSearch));
			}

			// For Mono, the deep search and shallow search are the same because there
			// normally are no sub-directories in the program files directories.

			// The subFoldersToSearch parameter is not valid on Linux.
			return LocateInProgramFilesUsingShallowSearch(exeName);
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

			foreach (var progFolder in Enumerable.Where<string>(GetPossibleProgramFilesFolders(), Directory.Exists))
			{
				// calling Directory.GetFiles("C:\Program Files", exeName, SearchOption.AllDirectories) will fail
				// if even one of the children of the Program Files doesn't allow you to search it.
				// So instead we first gather up all the children directories, and then search those.
				// Some will give us access denied, and that's fine, we skip them.
				// But we don't want to look in child directories on Linux because GetPossibleProgramFilesFolders()
				// gives us the individual elements of the PATH environment variable, and these specify exactly
				// those directories that should be searched for program files.
				foreach (var path in subFoldersToSearch.Select(sf => Path.Combine(progFolder, sf)).Where(Directory.Exists))
				{
					if (Platform.IsWindows)
					{
						foreach (var subDir in DirectoryHelper.GetSafeDirectories(path))
						{
							var tgtPath = GetFiles(exeName, srcOption, subDir);
							if (!string.IsNullOrEmpty(tgtPath))
								return tgtPath;
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
				return Directory.GetFiles(subDir, exeName, srcOption).FirstOrDefault();
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
				foreach (var dir in Environment.GetEnvironmentVariable("PATH").Split(':'))
					yield return dir;
				yield return "/opt"; // RAMP is installed in the /opt directory by default
			}

			var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			yield return pf.Replace(" (x86)", string.Empty);
			yield return pf.Replace(" (x86)", string.Empty) + " (x86)";
		}
	}
}