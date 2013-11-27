using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palaso.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryMigrator : FolderMigrator
	{

		private readonly List<WritingSystemRepositoryProblem> _migrationProblems = new List<WritingSystemRepositoryProblem>();

		public LdmlInFolderWritingSystemRepositoryMigrator(
			string ldmlPath,
			LdmlVersion0MigrationStrategy.MigrationHandler onMigrationCallback
		) : this(ldmlPath, onMigrationCallback, WritingSystemCompatibility.Strict)
		{
		}

		public LdmlInFolderWritingSystemRepositoryMigrator(
			string ldmlPath,
			LdmlVersion0MigrationStrategy.MigrationHandler migrationHandler,
			WritingSystemCompatibility compatibilityMode
		) : base(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, ldmlPath)
		{
			SearchPattern = "*.ldml";

			//The first versiongetter checks for the palaso:version node.
			//The DefaultVersion is a catchall that identifies any file as version 0 that the first version getter can't identify
			AddVersionStrategy(new WritingSystemLdmlVersionGetter(compatibilityMode));
			AddVersionStrategy(new DefaultVersion(0, 0));

			var auditLog = new WritingSystemChangeLog(
				new WritingSystemChangeLogDataMapper(Path.Combine(ldmlPath, "idchangelog.xml"))
			);
			AddMigrationStrategy(new LdmlVersion0MigrationStrategy(migrationHandler, auditLog, 0, compatibilityMode));
			// Version 0 strategy has been enhanced to also migrate version 1.
			AddMigrationStrategy(new LdmlVersion0MigrationStrategy(migrationHandler, auditLog, 1, compatibilityMode));
		}

		public IEnumerable<WritingSystemRepositoryProblem> MigrationProblems
		{
			get { return _migrationProblems; }
		}

		///<summary>
		/// Converts FolderMigrationProblem probelms to WritingSystemRepositoryProblem stored in MigrationProblems property.
		///</summary>
		protected override void OnFolderMigrationProblem(IEnumerable<FolderMigratorProblem> problems)
		{
			_migrationProblems.AddRange(problems.Select(
				problem => new WritingSystemRepositoryProblem
					{
						Exception = problem.Exception, FilePath = problem.FilePath
					}
			));
		}


	}
}