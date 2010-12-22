using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Palaso.Code;
using Palaso.Reporting;
using Palaso.Extensions;

namespace Palaso.IO
{
	public interface IFileLocator
	{
		string LocateFile(string fileName);
		string LocateFile(string fileName, string descriptionForErrorMessage);
	}

	public class FileLocator :IFileLocator
	{
		private readonly IEnumerable<string> _searchPaths;

		public FileLocator(IEnumerable<string> searchPaths  )
		{
			_searchPaths = searchPaths;
		}

		public string LocateFile(string fileName)
		{
			foreach (var path in _searchPaths)
			{
				var fullPath = Path.Combine(path, fileName);
				if(File.Exists(fullPath))
					return fullPath;
			}
			return string.Empty;
		}

		public string LocateFile(string fileName, string descriptionForErrorMessage)
		{

			var path = LocateFile(fileName);
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				ErrorReport.NotifyUserOfProblem(
					"{0} could not find the {1}.  It expected to find it in one of these locations: {2}",
					UsageReporter.AppNameToUseInDialogs, descriptionForErrorMessage, _searchPaths.Concat(", ")
					);
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
					path = Application.ExecutablePath;
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
		public static string GetFileDistributedWithApplication(params string[] partsOfTheSubPath)
		{
			var path = FileLocator.DirectoryOfApplicationOrSolution;
			foreach (var part in partsOfTheSubPath)
			{
				path = System.IO.Path.Combine(path, part);
			}
			if (File.Exists(path))
				return path;

			//try distfiles
			path = FileLocator.DirectoryOfApplicationOrSolution;
			path = Path.Combine(path, "DistFiles");
			foreach (var part in partsOfTheSubPath)
			{
				path = System.IO.Path.Combine(path, part);
			}
			RequireThat.File(path).Exists();
			return path;
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

			if(!optional)
				RequireThat.Directory(path).Exists();
			return path;
		}
		/// <summary>
		/// Find a file which, on a development machine, lives in [solution]/DistFiles/[subPath],
		/// and when installed, lives in
		/// [applicationFolder]/[subPath1]/[subPathN]
		/// </summary>
		/// <example>GetFileDistributedWithApplication("info", "releaseNotes.htm");</example>
		public static string GetDirectoryDistributedWithApplication(params string[] partsOfTheSubPath)
		{
			return GetDirectoryDistributedWithApplication(false, partsOfTheSubPath);
		}
	}
}