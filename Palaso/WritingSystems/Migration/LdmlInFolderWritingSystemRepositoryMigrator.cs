using System;
using System.IO;
using Palaso.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryMigrator : FolderMigrator
	{
		public LdmlInFolderWritingSystemRepositoryMigrator(string ldmlPath,
														   LdmlVersion0MigrationStrategy.OnMigrationFn
															   onMigrationCallback)
			: this(ldmlPath, onMigrationCallback, false){}

		public LdmlInFolderWritingSystemRepositoryMigrator(string ldmlPath, LdmlVersion0MigrationStrategy.OnMigrationFn onMigrationCallback, bool roundtripFlex70PrivateUse)
			: base(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, ldmlPath)
		{
			SearchPattern = "*.ldml";

			//The first versiongetter checks for the palaso:version node.
			//The DefaultVersion is a catchall that identifies any file as version 0 that the first version getter can't identify
			AddVersionStrategy(new WritingSystemLdmlVersionGetter(roundtripFlex70PrivateUse));
			AddVersionStrategy(new DefaultVersion(0, 0));

			var auditLog =
				new WritingSystemChangeLog(
					new WritingSystemChangeLogDataMapper(Path.Combine(ldmlPath, "idchangelog.xml")));
			AddMigrationStrategy(new LdmlVersion0MigrationStrategy(onMigrationCallback, auditLog, 0, roundtripFlex70PrivateUse));
			// Version 0 strategy has been enhanced to also migrate version 1.
			AddMigrationStrategy(new LdmlVersion0MigrationStrategy(onMigrationCallback, auditLog, 1, roundtripFlex70PrivateUse));
		}

		public void Migrate()
		{
			base.Migrate();
		}
	}
}