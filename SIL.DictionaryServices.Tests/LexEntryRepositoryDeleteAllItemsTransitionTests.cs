using NUnit.Framework;
using SIL.DictionaryServices.Model;
using SIL.IO;
using SIL.Tests.Data;

namespace SIL.DictionaryServices.Tests
{
	[TestFixture]
	public class LiftLexEntryRepositoryDeleteAllItemsTransitionTests :
		IRepositoryDeleteAllItemsTransitionTests<LexEntry>
	{
		private string _persistedFilePath;
		private TempFile _tempFile;

		[SetUp]
		public override void SetUp()
		{
			_tempFile = new TempFile();
			_persistedFilePath = _tempFile.Path;
			DataMapperUnderTest = new LiftLexEntryRepository(_persistedFilePath);
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
			_tempFile.Dispose();
		}

		protected override void RepopulateRepositoryFromPersistedData()
		{
			DataMapperUnderTest.Dispose();
			DataMapperUnderTest = new LiftLexEntryRepository(_persistedFilePath);
		}
	}
}