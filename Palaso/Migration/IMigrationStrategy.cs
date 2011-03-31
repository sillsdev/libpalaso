using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.Migration
{
	///<summary>
	/// Used by Migrator to migrate from one version to another.
	///</summary>
	public interface IMigrationStrategy
	{
		int FromVersion { get; }
		int ToVersion { get; }
		void Migrate(string sourceFilePath, string destinationFilePath);

		void PreMigrate();
		void PostMigrate();
	}
}
