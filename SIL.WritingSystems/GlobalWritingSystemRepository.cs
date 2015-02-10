using System;
using System.Globalization;
using System.IO;
using SIL.WritingSystems.Migration;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace SIL.WritingSystems
{
	///<summary>
	/// A system wide writing system repoistory
	///</summary>
	public class GlobalWritingSystemRepository : LdmlInFolderWritingSystemRepository
	{

		private static GlobalWritingSystemRepository _instance;
		private static readonly object Padlock = new object();

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
				lock (Padlock)
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
		public static GlobalWritingSystemRepository Initialize(LdmlVersion0MigrationStrategy.MigrationHandler migrationHandler)
		{
			return InitializeWithBasePath(DefaultBasePath, migrationHandler);
		}

		///<summary>
		/// This initializer is intended for tests as it allows setting of the basePath explicitly.
		///</summary>
		internal static GlobalWritingSystemRepository InitializeWithBasePath(string basePath,
			LdmlVersion0MigrationStrategy.MigrationHandler migrationHandler)
		{
			lock (Padlock)
			{
				if (_instance == null)
				{
					var migrator = new GlobalWritingSystemRepositoryMigrator(basePath, migrationHandler);
					if (migrator.NeedsMigration())
					{
						migrator.Migrate();
					}

					_instance = new GlobalWritingSystemRepository(basePath);
					_instance.LoadAllDefinitions();
				}
			}
			return _instance;
		}

		///<summary>
		/// Specify the location of the System Writing System repository explicitly.
		/// This is mostly useful for tests.
		///</summary>
		///<param name="basePath"></param>
		internal GlobalWritingSystemRepository(string basePath) :
			base(CurrentVersionPath(basePath))
		{
			BasePath = basePath;
		}

		///<summary>
		/// The DefaultBasePath is %CommonApplicationData%\SIL\WritingSystemRepository
		/// On Windows 7 this is \ProgramData\SIL\WritingSystemRepository\
		/// On Linux this must be in /var/lib so that it may be edited
		///</summary>
		public static string DefaultBasePath
		{
			get
			{
				string result = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				if (result == "/usr/share")
					result = "/var/lib";
				result = Path.Combine(result,"SIL");
				result = Path.Combine(result, "WritingSystemRepository");
				return result;
			}
		}

		///<summary>
		/// The BasePath of this instance of GlobalWritingSystemRepository.
		/// e.g. c:\ProgramData\SIL\WritingSystemRepository\
		///</summary>
		public string BasePath { get; private set; }

		///<summary>
		/// The CurrentVersionPath is %CommonApplicationData%\SIL\WritingSystemRepository\LatestVersion
		/// e.g. On Windows 7 this is \ProgramData\SIL\WritingSystemRepository\1
		///</summary>
		public static string CurrentVersionPath(string basePath)
		{
			return Path.Combine(basePath, WritingSystemDefinition.LatestWritingSystemDefinitionVersion.ToString(CultureInfo.InvariantCulture));
		}

	}
}
