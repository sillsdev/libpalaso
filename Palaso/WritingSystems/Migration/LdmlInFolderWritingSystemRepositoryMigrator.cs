using System;
using System.IO;
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

			var auditLog =
				new WritingSystemChangeLog(
					new WritingSystemChangeLogDataMapper(Path.Combine(ldmlPath, "idchangelog.xml")));
			AddMigrationStrategy(new LdmlVersion0MigrationStrategy(onMigrationCallback, auditLog));
		}

		public void Migrate()
		{
			base.Migrate();
		}
	}
}