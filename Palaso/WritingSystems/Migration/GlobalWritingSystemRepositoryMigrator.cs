using System;
using System.IO;
using Palaso.IO;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems.Migration
{
	public class GlobalWritingSystemRepositoryMigrator
	{
		private LdmlVersion0MigrationStrategy.OnMigrationFn _migrationInfoCallback;

		public GlobalWritingSystemRepositoryMigrator(string basePath, LdmlVersion0MigrationStrategy.OnMigrationFn onMigrationCallback)
		{
			_migrationInfoCallback = onMigrationCallback;
			BasePath = basePath;
		}

		public bool NeedsMigration()
		{
			string currentVersionPath = VersionPath(WritingSystemDefinition.LatestWritingSystemDefinitionVersion);
			return !Directory.Exists(currentVersionPath);
		}

		public void Migrate()
		{
			// Progress through our options backwards.
			// General strategy is to copy the entire folder to the appropriate VersionPath and then migrate it,
			// leaving the old version behind for apps possibly using an older palaso lib to still use.

			// 1) Any versions in %CommonApplicationData% (c:\ProgramData\... on win 7)
			for (int version = WritingSystemDefinition.LatestWritingSystemDefinitionVersion - 1; version >= 0; --version)
			{
				string sourceVersionPath = VersionPath(version);
				if (Directory.Exists(sourceVersionPath))
				{
					CopyLdmlFromFolder(sourceVersionPath);
					MigrateLdmlInFolder();
					return;
				}
			}

			// 2) Copy files from %ApplicationData% and the Flex store then migrate
			bool haveLdmlToCopy = false;
			string oldPalasoPath = PalasoLdmlPathPre0;
			if (Directory.Exists(oldPalasoPath))
			{
				CopyLdmlFromFolder(oldPalasoPath);
				haveLdmlToCopy = true;
			}
			string oldFlexPath = FlexLdmlPathPre0;
			if (Directory.Exists(oldFlexPath))
			{
				CopyLdmlFromFolder(oldFlexPath);
				haveLdmlToCopy = true;
			}
			if (haveLdmlToCopy)
			{
				MigrateLdmlInFolder();
			}
		}

		private void MigrateLdmlInFolder()
		{
			var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(CurrentVersionPath, _migrationInfoCallback);
			migrator.Migrate();
		}

		private void CopyLdmlFromFolder(string sourcePath)
		{
			FolderUtils.CopyFolderWithException(sourcePath, CurrentVersionPath);
		}

		public string FlexLdmlPathPre0
		{
			get {
				string result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIL");
				result = Path.Combine(result, "WritingSystemRepository");
				return result;
			}
		}

		public static string PalasoLdmlPathPre0
		{
			get {
				string result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIL");
				result = Path.Combine(result, "WritingSystemRepository");
				return result;
			}
		}

		public string BasePath { get; private set; }

		public string VersionPath(int version)
		{
			return Path.Combine(BasePath, version.ToString());
		}

		public string CurrentVersionPath {
			get { return VersionPath(WritingSystemDefinition.LatestWritingSystemDefinitionVersion); }
		}
	}
}
