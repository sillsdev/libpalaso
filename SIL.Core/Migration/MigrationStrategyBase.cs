using System;

namespace SIL.Migration
{
	public abstract class MigrationStrategyBase : IMigrationStrategy
	{
		protected MigrationStrategyBase(int fromVersion, int toVersion)
		{
			if (toVersion <= fromVersion)
			{
				throw new ArgumentException(String.Format(
					"Migration to version '{0}' must be greater than from version '{1}'",
					toVersion,
					fromVersion
				));
			}
			FromVersion = fromVersion;
			ToVersion = toVersion;
		}

		public int FromVersion { get; private set; }

		public int ToVersion { get; private set; }

		public abstract void Migrate(string sourceFilePath, string destinationFilePath);

		public virtual void PreMigrate()
		{
		}

		public virtual void PostMigrate(string sourcePath, string destinationPath)
		{
		}
	}
}
