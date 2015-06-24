namespace SIL.WritingSystems.Tests
{
	public class TestLdmlInXmlWritingSystemRepository : LdmlInXmlWritingSystemRepository
	{
		protected override IWritingSystemFactory<WritingSystemDefinition> CreateWritingSystemFactory()
		{
			return new TestWritingSystemFactory();
		}

		public new TestWritingSystemFactory WritingSystemFactory
		{
			get { return (TestWritingSystemFactory) base.WritingSystemFactory; }
		}
	}
}
