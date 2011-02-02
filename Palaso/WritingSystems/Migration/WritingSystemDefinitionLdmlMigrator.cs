using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Migration;

namespace Palaso.WritingSystems.Migration
{
	public class WritingSystemDefinitionLdmlMigrator
	{
		private Migrator _migrator;

		public WritingSystemDefinitionLdmlMigrator(string pathToFileToMigrate)
		{
			_migrator = new Migrator(WritingSystemDefinition.LatestVersion, pathToFileToMigrate);
		}

		public WritingSystemDefinitionLdmlMigrator(int versionToMigrateTo, string pathToFileToMigrate)
		{
			_migrator = new Migrator(versionToMigrateTo, pathToFileToMigrate);
		}

		public void MigrateIfNecassary()
		{
			throw new NotImplementedException();
		}
	}
}
