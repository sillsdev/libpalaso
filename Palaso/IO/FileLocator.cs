using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using Palaso.Code;
using Palaso.Reporting;
using Palaso.Extensions;

namespace Palaso.IO
{
	public interface IFileLocator
	{
		string LocateFile(string fileName);
		string LocateFile(string fileName, string descriptionForErrorMessage);
		string LocateOptionalFile(string fileName);
		string LocateFileWithThrow(string fileName);
		string LocateDirectory(string directoryName);
		string LocateDirectoryWithThrow(string directoryName);
		string LocateDirectory(string directoryName, string descriptionForErrorMessage);
		IFileLocator CloneAndCustomize(IEnumerable<string> addedSearchPaths);
	}

	public interface IChangeableFileLocator : IFileLocator
	{
		void AddPath(string path);
		void RemovePath(string path);
	}

	public class FileLocator : IChangeableFileLocator
	{
		protected readonly List<string> _searchPaths;

		public FileLocator(IEnumerable<string> searchPaths  )
		{
			_searchPaths = new List<string>(searchPaths);
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
		virtual protected IEnumerable<string> GetSearchPaths()
		{
			return _searchPaths;
		}

		public string LocateFile(string fileName, string descriptionForErrorMessage)
		{

			var path = LocateFile(fileName);
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				ErrorReport.NotifyUserOfProblem(
					"{0} could not find the {1}.  It expected to find it in one of these locations: {2}",
					UsageReporter.AppNameToUseInDialogs, descriptionForErrorMessage, GetSearchPaths().Concat(", ")
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
					UsageReporter.AppNameToUseInDialogs, descriptionForErrorMessage, _searchPaths.Concat(", ")
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
					directoryName, _searchPaths.Concat(Environment.NewLine)));
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
				throw new ApplicationException("Could not find " + fileName + ". It expected to find it in one of these locations: " + Environment.NewLine + _searchPaths.Concat(Environment.NewLine));
			}
			return path;
		}
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

		public static string DirectoryOfTheApplicationExecutable
		{
			get
			{
				string path;
				bool unitTesting = Assembly.GetEntryAssembly() == null;
				if (unitTesting)
				{
					path = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
					path = Uri.UnescapeDataString(path);
				}
				else
				{
					path = EntryAssembly.Location;
				}
				return Directory.GetParent(path).FullName;
			}
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
			foreach (var directoryHoldingFiles in new []{null, "DistFiles", "common" /*for wesay*/})
			{
				var path = FileLocator.DirectoryOfApplicationOrSolution;
				path = FileLocator.DirectoryOfApplicationOrSolution;
				if(directoryHoldingFiles!=null)
					path = Path.Combine(path, directoryHoldingFiles);

				foreach (var part in partsOfTheSubPath)
				{
					path = System.IO.Path.Combine(path, part);
				}
				if (File.Exists(path))
					return path;
			}

			if (optional)
				return null;
			string subpath="";
			foreach (var part in partsOfTheSubPath)
				{
					subpath = System.IO.Path.Combine(subpath, part);
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
			var path = FileLocator.DirectoryOfApplicationOrSolution;
			foreach (var part in partsOfTheSubPath)
			{
				path = System.IO.Path.Combine(path, part);
			}
			if (Directory.Exists(path))
				return path;

			//try distfiles
			path = FileLocator.DirectoryOfApplicationOrSolution;
			path = Path.Combine(path,"DistFiles");
			foreach (var part in partsOfTheSubPath)
			{
				path = System.IO.Path.Combine(path, part);
			}
			if (Directory.Exists(path))
				return path;

			//try src (e.g. Bloom keeps its javascript under source directory (and in distfiles only when installed)
			path = FileLocator.DirectoryOfApplicationOrSolution;
			path = Path.Combine(path, "src");
			foreach (var part in partsOfTheSubPath)
			{
				path = System.IO.Path.Combine(path, part);
			}

			if (optional && !Directory.Exists(path))
				return null;

			RequireThat.Directory(path).Exists();
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

		/// <summary>
		/// Use this when you can't mess with the whole application's filelocator, but you want to add a path or two, e.g., the folder of the current book in Bloom.
		/// </summary>
		/// <param name="addedSearchPaths"></param>
		/// <returns></returns>
		public virtual IFileLocator CloneAndCustomize(IEnumerable<string> addedSearchPaths)
		{
			return new FileLocator(new List<string>(_searchPaths.Concat(addedSearchPaths)));
		}
		#region Methods for locating file in program files folders
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
#if !__MonoCS__
			var tgtPath = LocateInProgramFilesUsingShallowSearch(exeName, subFoldersToSearch);
			if (tgtPath != null)
				return tgtPath;

			return (!fallBackToDeepSearch ? null :
				LocateInProgramFilesUsingDeepSearch(exeName, subFoldersToSearch));
#else
			// For Mono, the deep search and shallow search are the same because there
			// normally are no sub-directories in the program files directories.

			// The subFoldersToSearch parameter is not valid on Linux.
			return LocateInProgramFilesUsingDeepSearch(exeName);
#endif
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
				foreach (var path in subFoldersToSearch.Select(sf => Path.Combine(progFolder, sf)).Where(Directory.Exists))
				{
					try
					{
						var tgtPath = Directory.GetFiles(path, exeName, srcOption).FirstOrDefault();
						if (tgtPath != null)
							return tgtPath;
					}
					catch (Exception)
					{
						//swallow. Some paths, we aren't allowed to look in (like Google Chrome crash reports)
					}

				}
			}

			return null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Searches the registry and returns the full path to the application program used to
		/// open files having the specified extention. The fileExtension can be with or without
		/// the preceding period. If the command cannot be found in the registry, then null is
		/// returned. If a command in the registry is found, but it refers to a program file
		/// that does not exist, null is returned.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string GetFromRegistryProgramThatOpensFileType(string fileExtension)
		{
#if __MonoCS__
			//------------------------------------------------------------------------------------
			// The following command will output the mime type of an existing file, Phil.html:
			//    file -b --mime-type ~/Phil.html
			//
			// This command will tell you the default application to open the file Phil.html:
			//    ext=$(grep "$(file -b --mime-type ~/Phil.html)" /etc/mime.types
			//        | awk '{print $1}') && xdg-mime query default $ext
			//
			// This command will open the file Phil.html using the default application:
			//    xdg-open ~/Page.html
			//------------------------------------------------------------------------------------

			throw new NotImplementedException("GetFromRegistryProgramThatOpensFileType not implemented on Mono yet.");
#endif
			var ext = fileExtension.Trim();
			if (!ext.StartsWith("."))
				ext = "." + ext;

			var key = Registry.ClassesRoot.OpenSubKey(ext);
			if (key == null)
				return null;

			var value = key.GetValue(string.Empty) as string;
			key.Close();

			if (value == null)
				return null;

			key = Registry.ClassesRoot.OpenSubKey(string.Format("{0}\\shell\\open\\command", value));
			if (key == null)
				return null;

			value = key.GetValue(string.Empty) as string;
			key.Close();

			if (value == null)
				return null;

			value = value.Trim('\"', '%', '1', ' ');
			return (!File.Exists(value) ? null : value);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a list of the possible paths to the program files folder, taking into
		/// account that 2 often (or always?) exist in a Win64 OS (i.e. "Program Files" and
		/// "Program Files (x86)").
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static IEnumerable<string> GetPossibleProgramFilesFolders()
		{
#if !__MonoCS__
			var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			yield return pf.Replace(" (x86)", string.Empty);
			yield return pf.Replace(" (x86)", string.Empty) + " (x86)";
#else
			yield return "/opt"; // RAMP is installed in the /opt directory by default
			yield return "/usr/local/bin";
			yield return "/usr/bin";
			yield return "/bin";
#endif
		}

		#endregion

		public virtual void AddPath(string path)
		{
			_searchPaths.Add(path);
		}

		public void RemovePath(string path)
		{
			_searchPaths.Remove(path);
		}
	}
}