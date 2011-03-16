using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Palaso.Migration
{
	public class Migrator
	{
		protected readonly List<IMigrationStrategy> _migrationStrategies;
		protected readonly List<IFileVersion> _versionStrategies;
		protected readonly int _versionToMigrateTo;

		private class VersionComparerDescending : IComparer<IFileVersion>
		{
			public int Compare(IFileVersion x, IFileVersion y)
			{
				if (x.StrategyGoodToVersion == y.StrategyGoodToVersion)
					return 0;
				if (x.StrategyGoodToVersion < y.StrategyGoodToVersion)
					return 1;
				return -1;
			}
		}

		public int MaximumVersionThatFileCanBeMigratedTo
		{
			get
			{
				int highestVersionWeCanMigrateThisFileTo = GetFileVersion();
				foreach (var migrationStrategy in _migrationStrategies)
				{
					if(migrationStrategy.FromVersion< highestVersionWeCanMigrateThisFileTo &&
						highestVersionWeCanMigrateThisFileTo < migrationStrategy.ToVersion)
					{
						highestVersionWeCanMigrateThisFileTo = migrationStrategy.ToVersion;
					}
				}
				while(_migrationStrategies.Any(m=>m.FromVersion==highestVersionWeCanMigrateThisFileTo))
				{
					highestVersionWeCanMigrateThisFileTo =
						_migrationStrategies.Where(m => m.FromVersion == highestVersionWeCanMigrateThisFileTo).First().ToVersion;
				}

				return highestVersionWeCanMigrateThisFileTo;
			}
		}

		public Migrator(int versionToMigrateTo, string sourceFilePath)
		{
			SourceFilePath = sourceFilePath;
			_versionToMigrateTo = versionToMigrateTo;
			_migrationStrategies = new List<IMigrationStrategy>();
			_versionStrategies = new List<IFileVersion>();
		}

		public string SourceFilePath { get; private set; }

		public void AddVersionStrategy(IFileVersion strategy)
		{
			_versionStrategies.Add(strategy);
		}

		public void AddMigrationStrategy(IMigrationStrategy strategy)
		{
			_migrationStrategies.Add(strategy);
		}

		public int GetFileVersion()
		{
			int result = -1;
			_versionStrategies.Sort(new VersionComparerDescending());
			foreach (IFileVersion strategy in _versionStrategies)
			{
				result = strategy.GetFileVersion(SourceFilePath);
				if (result >= 0)
				{
					break;
				}
			}
			return result;
		}

		public bool NeedsMigration()
		{
			return GetFileVersion() != _versionToMigrateTo;
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
			while (currentVersion != _versionToMigrateTo)
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
