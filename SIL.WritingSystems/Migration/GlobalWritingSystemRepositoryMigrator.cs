using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SIL.IO;

namespace SIL.WritingSystems.Migration
{
	///<summary>
	/// This migrates the global writing system repository. This migrator will not remove older versions of the repository, rather
	/// it leaves the older versions behind for apps that may be running older versions of the palaso library.
	/// Note that the migrator does not continue to harvest data from older versions of the library.
	///</summary>
	public class GlobalWritingSystemRepositoryMigrator : LdmlInFolderWritingSystemRepositoryMigrator
	{
		private readonly string _basePath;

		///<summary>
		/// Initializes the migrator with the given basePath (i.e. excluding the version), and a callback
		/// for informing the consumer of changes to writing system tags during migration. Applications
		/// should ensure that all writing systems tags they store are updated accordingly when the callback
		/// is called.
		///</summary>
		public GlobalWritingSystemRepositoryMigrator(string basePath, Action<int, IEnumerable<LdmlMigrationInfo>> migrationHandler = null,
			int versionToMigrateTo = WritingSystemDefinition.LatestWritingSystemDefinitionVersion)
			: base(Path.Combine(basePath, versionToMigrateTo.ToString(CultureInfo.InvariantCulture)), migrationHandler, versionToMigrateTo)
		{
			_basePath = basePath;
		}

		///<summary>
		/// Returns true if migration is required. Simply returns true if the destination folder is present.
		///</summary>
		///<returns></returns>
		public bool NeedsMigration()
		{
			return !Directory.Exists(SourcePath);
		}

		///<summary>
		/// Performs the migration. If existing, but old, ldml is present then it is migrated. Failing that
		/// ldml is harvested from older versions of the library, or the older flex location.
		/// If both old palaso, and flex locations have the same ldml file the flex version is preferred.
		///</summary>
		public override void Migrate()
		{
			// Progress through our options backwards.
			// General strategy is to copy the entire folder to the appropriate VersionPath and then migrate it,
			// leaving the old version behind for apps possibly using an older palaso lib to still use.

			// 1) Migrate the current folder forwards if it exists
			if (Directory.Exists(SourcePath))
			{
				base.Migrate();
				return;
			}

			// 2) Harvest any older ldml in %CommonApplicationData% (c:\ProgramData\SIL\WritingSystemRepository\N on win 7)
			for (int version = WritingSystemDefinition.LatestWritingSystemDefinitionVersion - 1; version >= 0; --version)
			{
				string sourceVersionPath = Path.Combine(_basePath, version.ToString(CultureInfo.InvariantCulture));
				if (Directory.Exists(sourceVersionPath))
				{
					CopyLdmlFromFolder(sourceVersionPath);
					base.Migrate();
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
				base.Migrate();
		}

		private void CopyLdmlFromFolder(string sourcePath)
		{
			DirectoryUtilities.CopyDirectoryWithException(sourcePath, SourcePath);
		}

		///<summary>
		/// The path to old flex ldml files.
		///</summary>
		public static string FlexLdmlPathPre0
		{
			get {
				string result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIL");
				result = Path.Combine(result, "WritingSystemRepository");
				if (!Directory.Exists(result))
					Directory.CreateDirectory(result);
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
				if (!Directory.Exists(result))
					Directory.CreateDirectory(result);
				return result;
			}
		}
	}
}
