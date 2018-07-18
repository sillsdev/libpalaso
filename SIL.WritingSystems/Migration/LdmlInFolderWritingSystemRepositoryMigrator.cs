using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Migration;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV2To3Migration;

namespace SIL.WritingSystems.Migration
{
	public class LdmlInFolderWritingSystemRepositoryMigrator : FolderMigrator
	{
		private readonly Action<int, IEnumerable<LdmlMigrationInfo>> _migrationHandler;
		private readonly List<WritingSystemRepositoryProblem> _migrationProblems = new List<WritingSystemRepositoryProblem>();
		private readonly Dictionary<string, List<Action<WritingSystemDefinition>>> _removedPropertiesSetters; 

		public LdmlInFolderWritingSystemRepositoryMigrator(
			string ldmlPath,
			Action<int, IEnumerable<LdmlMigrationInfo>> migrationHandler,
			int versionToMigrateTo = LdmlDataMapper.CurrentLdmlLibraryVersion
		) : base(versionToMigrateTo, ldmlPath)
		{
			_removedPropertiesSetters = new Dictionary<string, List<Action<WritingSystemDefinition>>>();
			_migrationHandler = migrationHandler;
			SearchPattern = "*.ldml";

			//The first versiongetter checks for the palaso:version node.
			//The DefaultVersion is a catchall that identifies any file as version 0 that the first version getter can't identify
			AddVersionStrategy(new WritingSystemLdmlVersionGetter());
			AddVersionStrategy(new DefaultVersion(0, 0));

			var auditLog = new WritingSystemChangeLog(
				new WritingSystemChangeLogDataMapper(Path.Combine(ldmlPath, "idchangelog.xml"))
			);
			AddMigrationStrategy(new LdmlVersion0MigrationStrategy(OnLdmlMigrated, auditLog, 0));
			// Version 0 strategy has been enhanced to also migrate version 1.
			AddMigrationStrategy(new LdmlVersion0MigrationStrategy(OnLdmlMigrated, auditLog, 1));
			AddMigrationStrategy(new LdmlVersion2MigrationStrategy(OnLdmlMigrated, auditLog));
		}

		private void OnLdmlMigrated(int toVersion, IEnumerable<LdmlMigrationInfo> infos)
		{
			LdmlMigrationInfo[] infosArray = infos.ToArray();
			foreach (LdmlMigrationInfo mi in infosArray)
			{
				List<Action<WritingSystemDefinition>> setters;
				if (!_removedPropertiesSetters.TryGetValue(mi.LanguageTagBeforeMigration, out setters))
					setters = new List<Action<WritingSystemDefinition>>();
				if (mi.RemovedPropertiesSetter != null)
					setters.Add(mi.RemovedPropertiesSetter);
				_removedPropertiesSetters[mi.LanguageTagAfterMigration] = setters;
			}
			if (_migrationHandler != null)
				_migrationHandler(toVersion, infosArray);
		}

		/// <summary>
		/// Resets any property values that were removed during migration so that they are not lost.
		/// </summary>
		public void ResetRemovedProperties(IWritingSystemRepository repo)
		{
			foreach (KeyValuePair<string, List<Action<WritingSystemDefinition>>> kvp in _removedPropertiesSetters)
			{
				WritingSystemDefinition ws;
				if (repo.TryGet(kvp.Key, out ws))
				{
					foreach (Action<WritingSystemDefinition> setter in kvp.Value)
						setter(ws);
				}
			}
			repo.Save();
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