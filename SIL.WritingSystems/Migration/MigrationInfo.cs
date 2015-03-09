
namespace SIL.WritingSystems.Migration
{
	/// <summary>
	/// This class keeps track of a file's name change due to updates in the ieft language tag
	/// </summary>
	public class MigrationInfo
	{
		public string FileName;
		public string IetfLanguageTagBeforeMigration;
		public string IetfLanguageTagAfterMigration;
	}
}
