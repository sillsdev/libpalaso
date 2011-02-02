using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Palaso.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV1To2Migration;

namespace Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	public class Version0Migrator : IMigrationStrategy
	{
		public int FromVersion
		{
			get { return 0; }
		}

		public int ToVersion
		{
			get { return 1; }
		}

		public void Migrate(string sourceFilePath, string destinationFilePath)
		{
			var adaptorToReadLdmlV0 = new LdmlAdaptorV0();
			var adaptorToWriteLdmlV1 = new LdmlAdaptorV1();

			var wsToMigrate = new WritingSystemDefinitionV0();
			var migratedWs = new WritingSystemDefinitionV1();

			adaptorToReadLdmlV0.Read(sourceFilePath, wsToMigrate);
			adaptorToWriteLdmlV1.Write(destinationFilePath, migratedWs, new FileStream(sourceFilePath, FileMode.Open));
		}
	}
}
