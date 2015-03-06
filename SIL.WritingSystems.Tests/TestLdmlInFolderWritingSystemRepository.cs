namespace SIL.WritingSystems.Tests
{
	public class TestLdmlInFolderWritingSystemRepository : LdmlInFolderWritingSystemRepository
	{
		public TestLdmlInFolderWritingSystemRepository(string basePath, GlobalWritingSystemRepository globalRepository = null)
			: base(basePath, globalRepository)
		{
		}

		protected override IWritingSystemFactory<WritingSystemDefinition> CreateDefaultWritingSystemFactory()
		{
			return new TestLdmlInFolderWritingSystemFactory(this);
		}

		public new TestLdmlInFolderWritingSystemFactory WritingSystemFactory
		{
			get { return (TestLdmlInFolderWritingSystemFactory) base.WritingSystemFactory; }
		}
	}
}
