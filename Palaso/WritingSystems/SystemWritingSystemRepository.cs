using System;
using System.IO;

namespace Palaso.WritingSystems
{
	///<summary>
	/// A system wide writing system repoistory
	///</summary>
	public class SystemWritingSystemRepository : LdmlInFolderWritingSystemRepository
	{
		///<summary>
		/// Constructor.
		/// The default path for the system wide writing system repository is under:
		///  - ...LocalApplicationData/SIL/1
		///</summary>
		public SystemWritingSystemRepository()
		{
			string rootPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIL"
			);
			PathToWritingSystems = MakePathForVersion(rootPath);
			LoadAllDefinitions();
		}

		///<summary>
		/// Specify the location of the System Writing System repository explicitly.
		/// This is mostly useful for tests.
		///</summary>
		///<param name="basePath"></param>
		public SystemWritingSystemRepository(string basePath)
		{
			PathToWritingSystems = MakePathForVersion(basePath);
			LoadAllDefinitions();
		}

		private static string MakePathForVersion(string basePath)
		{
			string repoPath = basePath;
			Directory.CreateDirectory(repoPath);
			repoPath = Path.Combine(repoPath, "WritingSystemRepository");
			Directory.CreateDirectory(repoPath);
			repoPath = Path.Combine(repoPath, LatestVersion.ToString());
			Directory.CreateDirectory(repoPath);
			return repoPath;
		}

	}
}
