using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SIL.IO;

namespace SIL.Migration
{
	///<summary>
	/// Migrates a set of files (of one type) in a folder matching an optional SearchPattern up to the given version using a set of strategies to migrate each
	/// version of the file.
	///</summary>
	public class FolderMigrator : MigratorBase
	{

		private class FileVersionInfo
		{
			public readonly int Version;
			public readonly string FileName;

			public FileVersionInfo(string fileName, int version)
			{
				FileName = fileName;
				Version = version;
			}
		}

		///<summary>
		/// A handler delegate for notification of issues encountered during migration.
		///</summary>
		///<param name="problems"></param>
		public delegate void FolderMigratorProblemHandler(IEnumerable<FolderMigratorProblem> problems);

		private readonly FolderMigratorProblemHandler _problemHandler;

		///<summary>
		/// FolderMigrationProblem contains information about exceptions thrown during execution
		/// of a migration strategy.
		///</summary>
		public class FolderMigratorProblem
		{
			///<summary>
			/// The exceptions that was thrown by the migration stragegy.
			///</summary>
			public Exception Exception { get; set; }

			///<summary>
			/// The file being migrated when the problem occurred.
			///</summary>
			public string FilePath { get; set; }
		}

		private readonly List<FileVersionInfo> _versionCache = new List<FileVersionInfo>();
		private string _versionCachePath;

		///<summary>
		/// Constructor
		///</summary>
		///<param name="versionToMigrateTo">Target version to migrate to</param>
		///<param name="sourcePath">FolderMigratorProblemHandler callback to pass exceptions thrown during migration</param>
		public FolderMigrator(int versionToMigrateTo, string sourcePath) :
			base(versionToMigrateTo)
		{
			SourcePath = sourcePath;
			_problemHandler = OnFolderMigrationProblem;
		}

		///<summary>
		/// Constructor with FolderMigratorProblemHandler which is passed a collection of exceptions thrown during migration.
		///</summary>
		///<param name="versionToMigrateTo">Target version to migrate to</param>
		///<param name="sourcePath">Folder path containing files to migrate</param>
		///<param name="problemHandler">Folder path containing files to migrate</param>
		public FolderMigrator(int versionToMigrateTo, string sourcePath, FolderMigratorProblemHandler problemHandler) :
			this(versionToMigrateTo, sourcePath)
		{
			_problemHandler = problemHandler;
		}

		///<summary>
		/// Default FolderMigrationProblemHandler, does nothing.
		///</summary>
		protected virtual void OnFolderMigrationProblem(IEnumerable<FolderMigratorProblem> problems)
		{
		}

		///<summary>
		/// Perform the migration
		///</summary>
		///<exception cref="InvalidOperationException"></exception>
		public virtual void Migrate()
		{
			var problems = new List<FolderMigratorProblem>();

			// Clean up root backup path
			if (Directory.Exists(MigrationPath))
			{
				if (!RobustIO.DeleteDirectoryAndContents(MigrationPath))
				{
					//for user, ah well
					//for programmer, tell us
					Debug.Fail("(Debug mode only) Couldn't delete the migration folder");
				}
			}

			//check if there is anything to migrate. If not, don't do anything
			if(!Directory.Exists(SourcePath)) return;


			if (GetLowestVersionInFolder(SourcePath) == ToVersion) return;

			// Backup current folder to backup path under backup root
			CopyDirectory(SourcePath, BackupPath, MigrationPath);

			string currentPath = BackupPath;
			int lowestVersionInFolder;
			int lowestVersoinInFolder1 = -1;
			while ((lowestVersionInFolder = GetLowestVersionInFolder(currentPath)) != ToVersion)
			{
				//This guards against an empty Folder
				if(lowestVersionInFolder == int.MaxValue)
				{
					break;
				}
				if (lowestVersionInFolder == lowestVersoinInFolder1)
				{
					var fileNotMigrated = _versionCache.First(info => info.Version == lowestVersionInFolder).FileName;
					throw new ApplicationException(
						String.Format("The migration strategy for {0} failed to migrate the file '{1} from version {2}",
						SearchPattern,fileNotMigrated,
						lowestVersoinInFolder1)
					);
				}
				int currentVersion = lowestVersionInFolder;
				// Get all files in folder with this version
				var fileNamesToMigrate = GetFilesOfVersion(currentVersion, currentPath);

				// Find a strategy to migrate this version
				IMigrationStrategy strategy = MigrationStrategies.Find(s => s.FromVersion == currentVersion);
				if (strategy == null)
				{
					throw new InvalidOperationException(
						String.Format("No migration strategy could be found for version {0}", currentVersion)
					);
				}
				string destinationPath = Path.Combine(
					MigrationPath, String.Format("{0}_{1}", strategy.FromVersion, strategy.ToVersion)
				);
				Directory.CreateDirectory(destinationPath);
				// Migrate all the files of the current version
				foreach (var filePath in Directory.GetFiles(currentPath, SearchPattern))
				{
					string fileName = Path.GetFileName(filePath);
					if (fileName == null)
						continue;
					string sourceFilePath = Path.Combine(currentPath, fileName);
					string targetFilePath = Path.Combine(destinationPath, fileName);
					if (fileNamesToMigrate.Contains(sourceFilePath))
					{
						strategy.Migrate(sourceFilePath, targetFilePath);
					}
					else
					{
						File.Copy(sourceFilePath, targetFilePath);
					}
				}

				try
				{
					strategy.PostMigrate(currentPath, destinationPath);
				}
				catch (Exception e)
				{
					problems.Add(new FolderMigratorProblem { Exception = e, FilePath = currentPath });
				}

				// Setup for the next iteration
				currentPath = destinationPath;
				lowestVersoinInFolder1 = lowestVersionInFolder;
			}

			// Delete all the files in SourcePath matching SearchPattern
			foreach (var filePath in Directory.GetFiles(SourcePath, SearchPattern))
			{
				File.Delete(filePath);
			}

			// Copy the migration results into SourcePath
			CopyDirectory(currentPath, SourcePath, "");
			if (!RobustIO.DeleteDirectoryAndContents(MigrationPath))
			{
				//for user, ah well
				//for programmer, tell us
				Debug.Fail("(Debug mode only) Couldn't delete the migration folder");
			}

			// Call the problem handler if we encountered problems during migration.
			if (problems.Count > 0)
			{
				_problemHandler(problems);
			}
		}

		internal IEnumerable<string> GetFilesOfVersion(int currentVersion, string path)
		{
			UpdateVersionInfoCache(path);
			return _versionCache.Where(i => i.Version == currentVersion).Select(i => i.FileName);
		}

		internal int GetLowestVersionInFolder(string path)
		{
			UpdateVersionInfoCache(path);
			return _versionCache.Aggregate(int.MaxValue, (current, i) => Math.Min(i.Version, current));
		}

		private void UpdateVersionInfoCache(string path)
		{
			if (path == _versionCachePath)
			{
				return;
			}

			_versionCache.Clear();
			foreach (var fileName in Directory.GetFiles(path, SearchPattern))
			{
				int fileVersion = GetFileVersion(fileName);
				_versionCache.Add(new FileVersionInfo(fileName, fileVersion));
			}
			_versionCachePath = path;
		}

		///<summary>
		/// The pattern used to match files to migrate.
		///</summary>
		public string SearchPattern { get; set; }

		/// <summary>
		/// Gets the source path.
		/// </summary>
		protected string SourcePath { get; private set; }

		private string MigrationPath
		{
			get
			{
				return Path.Combine(SourcePath, "Migration");
			}
		}

		private string BackupPath {
			get
			{
				return Path.Combine(MigrationPath, "Backup");
			}
		}


		private static void CopyDirectory(string sourcePath, string targetPath, string excludePath)
		{
			CopyDirectory(new DirectoryInfo(sourcePath), new DirectoryInfo(targetPath),
				string.IsNullOrEmpty(excludePath) ? null : new DirectoryInfo(excludePath));
		}


		// Gleaned from http://xneuron.wordpress.com/2007/04/12/copy-directory-and-its-content-to-another-directory-in-c/
		public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target, DirectoryInfo excludePath)
		{
			// Check if the target directory exists, if not, create it.
			if (Directory.Exists(target.FullName) == false)
			{
				Directory.CreateDirectory(target.FullName);
			}

			// Copy each file into its new directory.
			foreach (FileInfo fileInfo in source.GetFiles())
			{
				fileInfo.CopyTo(Path.Combine(target.ToString(), fileInfo.Name), true);
			}

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo directoryInfo in source.GetDirectories())
			{
				if (excludePath != null && directoryInfo.FullName == excludePath.FullName)
				{
					continue;
				}
				DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(directoryInfo.Name);
				CopyDirectory(directoryInfo, nextTargetSubDir, excludePath);
			}
		}


	}
}
