using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Palaso.Migration;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryVersion0MigrationStrategy : IMigrationStrategy
	{
		public int FromVersion
		{
			get { return 0; }
		}

		public int ToVersion
		{
			get { return WritingSystemDefinition.LatestWritingSystemDefinitionVersion; }
		}

		public void Migrate(string directoryToMigrate, string destinationDirectory)
		{
			foreach (var pathtoFileToMigrate in Directory.GetFiles(directoryToMigrate))
			{
				var individualFileMigrator = new WritingSystemDefinitionLdmlMigrator(ToVersion, pathtoFileToMigrate);
				if(individualFileMigrator.NeedsMigration())
				{
					individualFileMigrator.Migrate();
				}
				File.Move(pathtoFileToMigrate, Path.Combine(destinationDirectory, Path.GetFileName(pathtoFileToMigrate)));
			}
		}
	}
}
