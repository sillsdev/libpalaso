using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Palaso.Migration
{
	///<summary>
	/// Migrates a set of files (of one type) in a folder up to the given version using a set of strategies to migrate each
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

		private readonly List<FileVersionInfo> _versionCache = new List<FileVersionInfo>();
		private string _versionCachePath;

		///<summary>
		/// Constructor
		///</summary>
		///<param name="versionToMigrateTo">Target version to migrate to</param>
		///<param name="path">Folder path containing files to migrate</param>
		public FolderMigrator(int versionToMigrateTo, string path) :
			base(versionToMigrateTo)
		{
			SourcePath = path;
		}

		///<summary>
		/// Perform the migration
		///</summary>
		///<exception cref="InvalidOperationException"></exception>
		public void Migrate()
		{
			// Clean up root backup path
			if (Directory.Exists(MigrationPath))
			{
				DeleteFolderAvoidingDeletionBug(MigrationPath);
			}
			// Backup current folder to backup path under backup root
			CopyDirectory(SourcePath, BackupPath);
			CopyDirectory(SourcePath, WorkingPath);

			int lowestVersionInFolder;
			while ((lowestVersionInFolder = GetLowestVersionInFolder(WorkingPath)) != ToVersion)
			{
				int currentVersion = lowestVersionInFolder;
				// Get all files in folder with this version
				var fileNamesToMigrate = GetFilesOfVersion(currentVersion, WorkingPath);

				// Find a strategy to migrate this version
				IMigrationStrategy strategy = _migrationStrategies.Find(s => s.FromVersion == currentVersion);
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
				// Migrate all the files
				foreach (string fileName in fileNamesToMigrate)
				{
					string sourceFilePath = Path.Combine(WorkingPath, fileName);
					string targetFilePath = Path.Combine(destinationPath, fileName);
					strategy.Migrate(sourceFilePath, targetFilePath);
				}
				// Copy into the working folder
				foreach (var fileName in fileNamesToMigrate)
				{
					string sourceFilePath = Path.Combine(destinationPath, fileName);
					string targetFilePath = Path.Combine(WorkingPath, fileName);
					File.Copy(sourceFilePath, targetFilePath);
				}
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

		private string SourcePath { get; set; }

		private string MigrationPath
		{
			get
			{
				return Path.Combine(SourcePath, "Migration");
			}
		}

		private string WorkingPath
		{
			get
			{
				return Path.Combine(MigrationPath, "Working");
			}
		}

		private string BackupPath {
			get
			{
				return Path.Combine(MigrationPath, "Backup");
			}
		}

		//This method works  around an issue where creating a folder shortly after deleting it, fails. Documented here:
		//http://social.msdn.microsoft.com/Forums/en/netfxbcl/thread/c7c4557b-a940-40dc-9fdf-1d8e8b64c46c
		private static void DeleteFolderAvoidingDeletionBug(string folderToDelete)
		{
			string deletionPath = folderToDelete + "ToBeDeleted";
			Directory.Move(folderToDelete, deletionPath);
			DeleteFolderThatMayBeInUse(deletionPath);
		}

		private static void CopyDirectory(string sourcePath, string targetPath)
		{
			CopyDirectory(new DirectoryInfo(sourcePath), new DirectoryInfo(targetPath));
		}

		// Gleaned from http://xneuron.wordpress.com/2007/04/12/copy-directory-and-its-content-to-another-directory-in-c/
		private static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
		{
			// Check if the target directory exists, if not, create it.
			if (Directory.Exists(target.FullName) == false)
			{
				Directory.CreateDirectory(target.FullName);
			}

			// Copy each file into it’s new directory.
			foreach (FileInfo fi in source.GetFiles())
			{
				fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
			}

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
				CopyDirectory(diSourceSubDir, nextTargetSubDir);
			}
		}

		//Copied from Testutilities. should this become an extension method on directory?
		private static void DeleteFolderThatMayBeInUse(string folder)
		{
			if (Directory.Exists(folder))
			{
				try
				{
					Directory.Delete(folder, true);
				}
				catch (Exception e)
				{
					try
					{
						Console.WriteLine(e.Message);
						//maybe we can at least clear it out a bit
						string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
						foreach (string s in files)
						{
							File.Delete(s);
						}
						//sleep and try again (seems to work)
						Thread.Sleep(1000);
						Directory.Delete(folder, true);
					}
					// ReSharper disable EmptyGeneralCatchClause
					catch
					{
					}
					// ReSharper restore EmptyGeneralCatchClause
				}
			}
		}
	}
}
