using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Migration;

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

			throw new NotImplementedException();
		}
	}
}
