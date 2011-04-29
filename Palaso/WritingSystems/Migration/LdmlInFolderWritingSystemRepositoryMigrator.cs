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

			var xPathVersion = new XPathVersion(1, "/ldml/special/palaso:version/@value");
			xPathVersion.NamespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			AddVersionStrategy(xPathVersion);
			AddVersionStrategy(new DefaultVersion(0, 0));

			AddMigrationStrategy(new LdmlVersion0MigrationStrategy(onMigrationCallback));
		}
	}
}