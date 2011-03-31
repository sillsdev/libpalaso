using System;
using System.Collections.Generic;
using System.IO;

namespace Palaso.Migration
{
	public class FileMigrator : MigratorBase
	{
		public FileMigrator(int versionToMigrateTo, string sourceFilePath) :
			base(versionToMigrateTo)
		{
			SourceFilePath = sourceFilePath;
		}

		public string SourceFilePath { get; private set; }

		public int GetFileVersion()
		{
			return base.GetFileVersion(SourceFilePath);
		}

		public string BackupFilePath
		{
			get { return SourceFilePath +  ".bak"; }
		}

		public void Migrate()
		{
			var filesToDelete = new List<string>();
			if (File.Exists(BackupFilePath))
			{
				File.Delete(BackupFilePath);
			}
			File.Copy(SourceFilePath, BackupFilePath);
			filesToDelete.Add(BackupFilePath);
			int currentVersion = GetFileVersion();
			string sourceFilePath = SourceFilePath;
			string destinationFilePath = "";
			while (currentVersion != ToVersion)
			{
				IMigrationStrategy strategy = _migrationStrategies.Find(s => s.FromVersion == currentVersion);
				if (strategy == null)
				{
					throw new InvalidOperationException(
						String.Format("No migration strategy could be found for version {0}", currentVersion)
					);
				}
				destinationFilePath = String.Format("{0}.Migrate_{1}_{2}", SourceFilePath, strategy.FromVersion, strategy.ToVersion);
				strategy.Migrate(sourceFilePath, destinationFilePath);
				filesToDelete.Add(destinationFilePath);
				currentVersion = strategy.ToVersion;
				sourceFilePath = destinationFilePath;
			}
			File.Delete(SourceFilePath);
			File.Move(destinationFilePath, SourceFilePath);
			foreach(var filePath in filesToDelete)
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
			}
		}
	}
}
