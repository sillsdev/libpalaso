using System;
using Palaso.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryMigrator : FolderMigrator
	{
		public LdmlInFolderWritingSystemRepositoryMigrator(string ldmlPath, LdmlVersion0MigrationStrategy.OnMigrationFn onMigrationCallback)
			: base(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, ldmlPath)
		{
			SearchPattern = "*.ldml";

			AddVersionStrategy(new WritingSystemLdmlVersionGetter());
			AddVersionStrategy(new DefaultVersion(0, 0));

			AddMigrationStrategy(new LdmlVersion0MigrationStrategy(onMigrationCallback));
		}

		public void Migrate()
		{
			base.Migrate();
		}
	}
}