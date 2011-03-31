using Palaso.Migration;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryMigrator : FolderMigrator
	{
		public LdmlInFolderWritingSystemRepositoryMigrator(string pathToLdml, WritingSystemIDChanged onWritingSystemIDChanged)
			: base(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, pathToLdml)
		{
			AddVersionStrategy(new LdmlInFolderWritingSystemRepositoryVersionGetter());
			AddMigrationStrategy(new LdmlInFolderWritingSystemRepositoryVersion0MigrationStrategy(onWritingSystemIDChanged));
		}
	}
}