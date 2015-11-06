using System.Collections.Generic;

namespace SIL.WritingSystems.Tests
{
	public class TestLdmlInFolderWritingSystemRepository : LdmlInFolderWritingSystemRepository
	{
		public TestLdmlInFolderWritingSystemRepository(string basePath, GlobalWritingSystemRepository globalRepository = null)
			: base(basePath, globalRepository)
		{
		}

		public TestLdmlInFolderWritingSystemRepository(string basePath, IEnumerable<ICustomDataMapper<WritingSystemDefinition>> customDataMappers,
			GlobalWritingSystemRepository globalRepository = null)
			: base(basePath, customDataMappers, globalRepository)
		{
		}

		protected override IWritingSystemFactory<WritingSystemDefinition> CreateWritingSystemFactory()
		{
			return new TestLdmlInFolderWritingSystemFactory(this);
		}

		public new TestLdmlInFolderWritingSystemFactory WritingSystemFactory
		{
			get { return (TestLdmlInFolderWritingSystemFactory) base.WritingSystemFactory; }
		}
	}
}
