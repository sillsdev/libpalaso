namespace SIL.Migration
{
	public class DelegateMigrationStrategy:MigrationStrategyBase
	{
		public delegate void MigrationFunction(string sourceFilePath, string destinationFilePath);

		private MigrationFunction _migrationDelegate;

		public DelegateMigrationStrategy(int fromVersion, int toVersion, MigrationFunction migrationDelegate) : base(fromVersion, toVersion)
		{
			_migrationDelegate = migrationDelegate;
		}

		public override void Migrate(string sourceFilePath, string destinationFilePath)
		{
			_migrationDelegate(sourceFilePath, destinationFilePath);
		}
	}
}
