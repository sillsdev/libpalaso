using System;
using System.IO;
using Palaso.IO;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems.Migration
{

	///<summary>
	/// This migrates the global writing system repository. This migrator will not remove older versions of the repository, rather
	/// it leaves the older versions behind for apps that may be running older versions of the palaso library.
	/// Note that the migrator does not continue to harvest data from older versions of the library.
	///</summary>
	public class GlobalWritingSystemRepositoryMigrator
	{
		private readonly LdmlVersion0MigrationStrategy.OnMigrationFn _migrationInfoCallback;

		///<summary>
		/// Initializes the migrator with the given basePath (i.e. excluding the version), and a callback
		/// for informing the consumer of changes to writing system tags during migration. Applications
		/// should ensure that all writing systems tags they store are updated accordingly when the callback
		/// is called.
		///</summary>
		///<param name="basePath"></param>
		///<param name="onMigrationCallback"></param>
		public GlobalWritingSystemRepositoryMigrator(string basePath, LdmlVersion0MigrationStrategy.OnMigrationFn onMigrationCallback)
		{
			_migrationInfoCallback = onMigrationCallback;
			BasePath = basePath;
		}

		///<summary>
		/// Returns true if migration is required. Simply returns true if the destination folder is present.
		///</summary>
		///<returns></returns>
		public bool NeedsMigration()
		{
			string currentVersionPath = VersionPath(WritingSystemDefinition.LatestWritingSystemDefinitionVersion);
			return !Directory.Exists(currentVersionPath);
		}

		///<summary>
		/// Performs the migration. If existing, but old, ldml is present then it is migrated. Failing that
		/// ldml is harvested from older versions of the library, or the older flex location.
		/// If both old palaso, and flex locations have the same ldml file the flex version is preferred.
		///</summary>
		public void Migrate()
		{
			// Progress through our options backwards.
			// General strategy is to copy the entire folder to the appropriate VersionPath and then migrate it,
			// leaving the old version behind for apps possibly using an older palaso lib to still use.

			// 1) Migrate the current folder forwards if it exists
			string currentVersionPath = VersionPath(WritingSystemDefinition.LatestWritingSystemDefinitionVersion);
			if (Directory.Exists(currentVersionPath))
			{
				MigrateLdmlInFolder();
				return;
			}

			// 2) Harvest any older ldml in %CommonApplicationData% (c:\ProgramData\SIL\WritingSystemRepository\N on win 7)
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

			// 3) Harvest ldml files from %ApplicationData% and the Flex store then migrate
			bool haveLdmlToCopy = false;
			string oldFlexPath = FlexLdmlPathPre0;
			if (Directory.Exists(oldFlexPath))
			{
				CopyLdmlFromFolder(oldFlexPath);
				haveLdmlToCopy = true;
			}
			string oldPalasoPath = PalasoLdmlPathPre0;
			if (Directory.Exists(oldPalasoPath))
			{
				try
				{
					CopyLdmlFromFolder(oldPalasoPath);
					haveLdmlToCopy = true;
				}
				catch (IOException)
				{
				}
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

		///<summary>
		/// The path to old flex ldml files.
		///</summary>
		public static string FlexLdmlPathPre0
		{
			get {
				string result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIL");
				result = Path.Combine(result, "WritingSystemRepository");
				return result;
			}
		}

		///<summary>
		/// The path to old palaso ldml files.
		///</summary>
		public static string PalasoLdmlPathPre0
		{
			get {
				string result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIL");
				result = Path.Combine(result, "WritingSystemRepository");
				return result;
			}
		}

		///<summary>
		/// The base path (i.e. excluding the version) to the global writing system store.
		///</summary>
		public string BasePath { get; private set; }

		///<summary>
		/// Returns the versioned path to the global writing system store (i.e. including the version).
		///</summary>
		///<param name="version"></param>
		///<returns></returns>
		public string VersionPath(int version)
		{
			return Path.Combine(BasePath, version.ToString());
		}

		///<summary>
		/// Returns the versioned path to the current global writing system store.
		///</summary>
		///<returns></returns>
		public string CurrentVersionPath
		{
			get { return VersionPath(WritingSystemDefinition.LatestWritingSystemDefinitionVersion); }
		}
	}
}
