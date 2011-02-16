using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Migration;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemStoreMigrator
	{
		private Migrator _migrator;

		public LdmlInFolderWritingSystemStoreMigrator(int versionToMigrateTo, string pathOfFileToMigrate)
		{
			_migrator = new Migrator(versionToMigrateTo, pathOfFileToMigrate);
		}

		public LdmlInFolderWritingSystemStoreMigrator(string pathOfFileToMigrate):this(LdmlInFolderWritingSystemStore.LatestVersion, pathOfFileToMigrate)
		{
		}

		public void MigrateIfNecassary()
		{
			throw new NotImplementedException();
		}
	}
}
