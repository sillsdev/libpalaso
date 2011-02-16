using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Migration;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryMigrator
	{
		private Migrator _migrator;

		public LdmlInFolderWritingSystemRepositoryMigrator(int versionToMigrateTo, string pathOfFileToMigrate)
		{
			_migrator = new Migrator(versionToMigrateTo, pathOfFileToMigrate);
		}

		public LdmlInFolderWritingSystemRepositoryMigrator(string pathOfFileToMigrate):this(LdmlInFolderWritingSystemRepository.LatestVersion, pathOfFileToMigrate)
		{
		}

		public void MigrateIfNecassary()
		{
			throw new NotImplementedException();
		}
	}
}
