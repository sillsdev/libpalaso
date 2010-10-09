using System;
using System.Collections.Generic;
using System.IO;

namespace Palaso.Migration
{
	public class Migrator
	{
		private readonly List<IMigrationStrategy> _migrationStrategies;
		private readonly List<IFileVersion> _versionStrategies;
		private readonly int _codeVersion;
		private IEnumerable<IMigrationStrategy> MigrationStrategies { get { return _migrationStrategies; } }

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

		public Migrator(int codeVersion, string sourceFilePath)
		{
			SourceFilePath = sourceFilePath;
			_codeVersion = codeVersion;
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
			return GetFileVersion() != _codeVersion;
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
			while (currentVersion != _codeVersion)
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
