using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Palaso.Migration;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryVersionGetter : IFileVersion
	{
		private Migrator _migrator;

		public int GetFileVersion(string pathToLdmlRepository)
		{
			int versionOfLowestVersionFileInRepo = WritingSystemDefinition.LatestWritingSystemDefinitionVersion + 1;
			foreach (var filePath in Directory.GetFiles(pathToLdmlRepository))
			{
				Migrator migrator = new WritingSystemDefinitionLdmlMigrator(filePath);
				int currentFileVersion = migrator.GetFileVersion();
				if(currentFileVersion < versionOfLowestVersionFileInRepo)
				{
					versionOfLowestVersionFileInRepo = currentFileVersion;
				}
			}
			return versionOfLowestVersionFileInRepo;
		}

		public int StrategyGoodToVersion
		{
			get { return _migrator.GoodToVersion; }
		}

		public int StrategyGoodFromVersion
		{
			get { return _migrator.GetFileVersion(); }
		}
	}
}
