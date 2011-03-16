using System;
using System.Collections.Generic;
using System.IO;
using Palaso.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems.Migration
{
	public class WritingSystemDefinitionLdmlMigrator:Migrator
	{

		public WritingSystemDefinitionLdmlMigrator(string pathToFileToMigrate):this(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, pathToFileToMigrate)
		{
		}

		public WritingSystemDefinitionLdmlMigrator(int versionToMigrateTo, string pathToFileToMigrate):base(versionToMigrateTo, pathToFileToMigrate)
		{
			AddVersionStrategy(new WritingSystemLdmlVersionGetter());
			AddMigrationStrategy(new Version0MigrationStrategy());
		}
	}
}
