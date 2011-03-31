using System.Collections.Generic;

namespace Palaso.Migration
{
	public class MigratorBase
	{
		protected readonly List<IMigrationStrategy> _migrationStrategies;
		protected readonly List<IFileVersion> _versionStrategies;

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

		public MigratorBase(int versionToMigrateTo)
		{
			ToVersion = versionToMigrateTo;
			_migrationStrategies = new List<IMigrationStrategy>();
			_versionStrategies = new List<IFileVersion>();
		}

		public int ToVersion { get; private set; }

		public void AddVersionStrategy(IFileVersion strategy)
		{
			_versionStrategies.Add(strategy);
		}

		public void AddMigrationStrategy(IMigrationStrategy strategy)
		{
			_migrationStrategies.Add(strategy);
		}

		public int GetFileVersion(string filePath)
		{
			int result = -1;
			_versionStrategies.Sort(new VersionComparerDescending());
			foreach (IFileVersion strategy in _versionStrategies)
			{
				result = strategy.GetFileVersion(filePath);
				if (result >= 0)
				{
					break;
				}
			}
			return result;
		}

		public bool NeedsMigration()
		{
			return GetFileVersion() != ToVersion;
		}

	}
}