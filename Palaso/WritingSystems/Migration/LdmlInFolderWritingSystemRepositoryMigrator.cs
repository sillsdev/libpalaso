using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Palaso.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryMigrator:Migrator
	{

		public LdmlInFolderWritingSystemRepositoryMigrator(string pathToFolderWithLdml):base(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, pathToFolderWithLdml)
		{
			AddVersionStrategy(new LdmlInFolderWritingSystemRepositoryVersionGetter());
			AddMigrationStrategy(new LdmlInFolderWritingSystemRepositoryVersion0MigrationStrategy());
		}

		//This was overridden to handle a directory
		public void Migrate()
		{
			var DirectorysToDelete = new List<string>();
			if (Directory.Exists(BackupFilePath))
			{
				DeleteFolderThatMayBeInUse(BackupFilePath);

			}
			CopyDirectory(new DirectoryInfo(SourceFilePath), new DirectoryInfo(BackupFilePath));
			DirectorysToDelete.Add(BackupFilePath);
			int currentVersion = GetFileVersion();
			string sourceDirectoryPath = SourceFilePath;
			string destinationDirectoryPath = "";
			while (currentVersion != _versionToMigrateTo)
			{
				IMigrationStrategy strategy = _migrationStrategies.Find(s => s.FromVersion == currentVersion);
				if (strategy == null)
				{
					throw new InvalidOperationException(
						String.Format("No migration strategy could be found for version {0}", currentVersion)
						);
				}
				destinationDirectoryPath = String.Format("{0}.Migrate_{1}_{2}", SourceFilePath,
														 strategy.FromVersion, strategy.ToVersion);
				Directory.CreateDirectory(destinationDirectoryPath);
				strategy.Migrate(sourceDirectoryPath, destinationDirectoryPath);
				DirectorysToDelete.Add(destinationDirectoryPath);
				currentVersion = strategy.ToVersion;
				sourceDirectoryPath = destinationDirectoryPath;
			}
			Directory.Delete(SourceFilePath, true);
			Directory.Move(destinationDirectoryPath, SourceFilePath);
			foreach (var DirectoryPath in DirectorysToDelete)
			{
				if (Directory.Exists(DirectoryPath))
				{
					DeleteFolderThatMayBeInUse(DirectoryPath);
				}
			}
		}

		// Gleaned from http://xneuron.wordpress.com/2007/04/12/copy-directory-and-its-content-to-another-directory-in-c/
		private void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
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
				DirectoryInfo nextTargetSubDir =
					target.CreateSubdirectory(diSourceSubDir.Name);
				CopyDirectory(diSourceSubDir, nextTargetSubDir);
			}
		}

		//Copied from Testutilities. should this become an extension method on directory?
		private void DeleteFolderThatMayBeInUse(string folder)
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
					catch (Exception)
					{
					}
				}
			}
		}
	}
}
