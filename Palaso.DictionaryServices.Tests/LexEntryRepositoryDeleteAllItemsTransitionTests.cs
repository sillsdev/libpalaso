using NUnit.Framework;
using Palaso.DictionaryServices;
using Palaso.DictionaryServices.Model;
using Palaso.TestUtilities;
using SIL.Tests.Data;

namespace WeSay.LexicalModel.Tests
{
	[TestFixture]
	public class LiftLexEntryRepositoryDeleteAllItemsTransitionTests :
		IRepositoryDeleteAllItemsTransitionTests<LexEntry>
	{
		private string _persistedFilePath;
		private TemporaryFolder _tempFolder;

		[SetUp]
		public override void SetUp()
		{
			_tempFolder = new TemporaryFolder("LiftLexEntryRepositoryDeleteAllItemsTransitionTests");
			_persistedFilePath = _tempFolder.GetTemporaryFile();
			DataMapperUnderTest = new LiftLexEntryRepository(_persistedFilePath);
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
			_tempFolder.Delete();
		}

		protected override void RepopulateRepositoryFromPersistedData()
		{
			DataMapperUnderTest.Dispose();
			DataMapperUnderTest = new LiftLexEntryRepository(_persistedFilePath);
		}
	}
}