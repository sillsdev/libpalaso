using System;
using System.IO;
using Palaso.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryMigrator : FolderMigrator
	{
		public LdmlInFolderWritingSystemRepositoryMigrator(string ldmlPath, LdmlVersion0MigrationStrategy.MigrationHandler migrationCallback)
			: base(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, ldmlPath)
		{
			SearchPattern = "*.ldml";

			//The first versiongetter checks for the palaso:version node.
			//The DefaultVersion is a catchall that identifies any file as version 0 that the first version getter can't identify
			AddVersionStrategy(new WritingSystemLdmlVersionGetter());
			AddVersionStrategy(new DefaultVersion(0, 0));

			var auditLog =
				new WritingSystemChangeLog(
					new WritingSystemChangeLogDataMapper(Path.Combine(ldmlPath, "idchangelog.xml")));
			AddMigrationStrategy(new LdmlVersion0MigrationStrategy(migrationCallback, auditLog));
		}

		public void Migrate()
		{
			base.Migrate();
		}
	}
}