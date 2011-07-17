using System;
using System.IO;
using Palaso.WritingSystems.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.WritingSystems
{
	///<summary>
	/// A system wide writing system repoistory
	///</summary>
	public class GlobalWritingSystemRepository : LdmlInFolderWritingSystemRepository
	{

		private static GlobalWritingSystemRepository _instance = null;
		private static readonly object _padlock = new object();

		///<summary>
		/// Returns an instance of the global writing system reposistory.  Apps must call Intialize prior to calling this.
		/// Apps are not obliged to use this Singleton Instance of GlobalWritingSystemRepository.  Apps can manage their
		/// own instance(s) of GlobalWritingSystemRepository returned from Initialize if they so choose.
		///</summary>
		///<returns>A global IWritingSystemRepository</returns>
		public static IWritingSystemRepository Instance
		{
			get
			{
				lock (_padlock)
				{
					if (_instance == null)
					{
						throw new NullReferenceException("The GlobalWritingSystemRepository has not been initialized. Please call Initialize(...) first.");
					}
					return _instance;
				}
			}
		}

		///<summary>
		/// Initializes the global writing system repository.  Migrates any ldml files if required,
		/// notifying of any changes of writing system id that occured during migration.
		///</summary>
		///<param name="onMigrationCallback"></param>
		public static void Initialize(LdmlVersion0MigrationStrategy.OnMigrationFn onMigrationCallback)
		{
			InitializeWithBasePath(onMigrationCallback, DefaultBasePath);
		}

		///<summary>
		/// This initializer is intended for tests as it allows setting of the basePath explicitly.
		///</summary>
		///<param name="onMigrationCallback">Callback if during the initialization any writing system id's are changed</param>
		///<param name="basePath">base location of the global writing system repository</param>
		internal static void InitializeWithBasePath(LdmlVersion0MigrationStrategy.OnMigrationFn onMigrationCallback, string basePath)
		{
			lock (_padlock)
			{
				if (_instance == null)
				{
					_instance = new GlobalWritingSystemRepository(onMigrationCallback, basePath);
				}
			}
		}

		///<summary>
		/// Specify the location of the System Writing System repository explicitly.
		/// This is mostly useful for tests.
		///</summary>
		///<param name="onMigrationCallback"></param>
		///<param name="basePath"></param>
		internal GlobalWritingSystemRepository(LdmlVersion0MigrationStrategy.OnMigrationFn onMigrationCallback, string basePath)
		{
			BasePath = basePath;
			PathToWritingSystems = CurrentVersionPath;

			var migrator = new GlobalWritingSystemRepositoryMigrator(basePath, onMigrationCallback);
			migrator.Migrate();

			Directory.CreateDirectory(CurrentVersionPath);
			LoadAllDefinitions();
		}

		///<summary>
		/// The DefaultBasePath is %CommonApplicationData%\SIL\WritingSystemRepository
		/// On Windows 7 this is \ProgramData\SIL\WritingSystemRepository\
		///</summary>
		public static string DefaultBasePath
		{
			get
			{
				string result = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					"SIL"
				);
				result = Path.Combine(result, "WritingSystemRepository");
				return result;
			}
		}

		///<summary>
		/// The BasePath of this instance of GloablWritingSystemRepository.
		/// e.g. c:\ProgramData\SIL\WritingSystemRepository\
		///</summary>
		public string BasePath { get; private set; }

		///<summary>
		/// The CurrentVersionPath is %CommonApplicationData%\SIL\WritingSystemRepository\LatestVersion
		/// e.g. On Windows 7 this is \ProgramData\SIL\WritingSystemRepository\1
		///</summary>
		public string CurrentVersionPath
		{
			get { return Path.Combine(BasePath, WritingSystemDefinition.LatestWritingSystemDefinitionVersion.ToString()); }
		}

	}
}
