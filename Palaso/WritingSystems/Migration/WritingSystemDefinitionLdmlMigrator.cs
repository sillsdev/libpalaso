using Palaso.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems.Migration
{
	public class WritingSystemDefinitionLdmlMigrator
	{
		private readonly Migrator _migrator;
		private readonly int _versionToMigrateTo;
		private readonly string _pathOfFileToMigrate;

		public WritingSystemDefinitionLdmlMigrator(string pathToFileToMigrate):this(WritingSystemDefinition.LatestVersion, pathToFileToMigrate)
		{
		}

		public WritingSystemDefinitionLdmlMigrator(int versionToMigrateTo, string pathToFileToMigrate)
		{
			_migrator = new Migrator(versionToMigrateTo, pathToFileToMigrate);
			_versionToMigrateTo = versionToMigrateTo;
			_pathOfFileToMigrate = pathToFileToMigrate;
			_migrator.AddVersionStrategy(new WritingSystemLdmlVersionGetter());
			_migrator.AddMigrationStrategy(new Version0Migrator());
		}

		public void MigrateIfNecassary()
		{
			if(FileNeedsMigrating)
			{
				_migrator.Migrate();
			}
		}

		public bool FileNeedsMigrating
		{
			get
			{
				var versionGetter = new WritingSystemLdmlVersionGetter();
				if(_versionToMigrateTo == versionGetter.GetFileVersion(_pathOfFileToMigrate))
				{
					return false;
				}
				return true;
			}
		}
	}
}
