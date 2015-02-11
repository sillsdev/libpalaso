using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palaso.Migration;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV2To3Migration;

namespace SIL.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryMigrator : FolderMigrator
	{
		private readonly List<WritingSystemRepositoryProblem> _migrationProblems = new List<WritingSystemRepositoryProblem>();

		public LdmlInFolderWritingSystemRepositoryMigrator(
			string ldmlPath,
			LdmlVersion0MigrationStrategy.MigrationHandler migrationHandler,
			IEnumerable<ICustomDataMapper> customDataMappers = null,
			WritingSystemCompatibility compatibilityMode = WritingSystemCompatibility.Strict,
			int versionToMigrateTo = WritingSystemDefinition.LatestWritingSystemDefinitionVersion
		) : base(versionToMigrateTo, ldmlPath)
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
			AddMigrationStrategy(new LdmlVersion2MigrationStrategy(customDataMappers ?? Enumerable.Empty<ICustomDataMapper>()));
		}

		public IEnumerable<WritingSystemRepositoryProblem> MigrationProblems
		{
			get { return _migrationProblems; }
		}

		public override void Migrate()
		{
			_migrationProblems.Clear();
			base.Migrate();
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