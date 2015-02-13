using System;
using NUnit.Framework;
using Palaso.DictionaryServices;
using Palaso.DictionaryServices.Model;
using Palaso.TestUtilities;
using SIL.IO;
using SIL.Tests.Data;

namespace WeSay.LexicalModel.Tests
{
	[TestFixture]
	public class LiftLexEntryRepositoryDeleteIdTransitionTests :
		IRepositoryDeleteIdTransitionTests<LexEntry>
	{
		private TempFile _persistedFilePath;
		private TemporaryFolder _tempFolder;

		[SetUp]
		public override void SetUp()
		{
			_tempFolder = new TemporaryFolder("LiftLexEntryRepositoryDeleteIdTransitionTests");
			_persistedFilePath = _tempFolder.GetNewTempFile(false);
			DataMapperUnderTest = new LiftLexEntryRepository(_persistedFilePath.Path);
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
			_tempFolder.Delete();
		}

		[Test]
		public override void SaveItem_ItemDoesNotExist_Throws()
		{
			SetState();
			Item.Senses.Add(new LexSense());
			Assert.Throws<ArgumentOutOfRangeException>(
				() => DataMapperUnderTest.SaveItem(Item)
			);
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			DataMapperUnderTest.Dispose();
			DataMapperUnderTest = new LiftLexEntryRepository(_persistedFilePath.Path);
		}
	}
}