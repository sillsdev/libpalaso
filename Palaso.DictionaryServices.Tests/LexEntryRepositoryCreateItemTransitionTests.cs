using System.Collections.Generic;
using NUnit.Framework;
using Palaso.DictionaryServices;
using Palaso.DictionaryServices.Model;
using Palaso.Tests.Data;
using Palaso.TestUtilities;

namespace WeSay.LexicalModel.Tests
{
	[TestFixture]
	public class LiftLexEntryRepositoryCreateItemTransitionTests :
		IRepositoryCreateItemTransitionTests<LexEntry>
	{
		private TemporaryFolder _tempFolder;
		private string _persistedFilePath;

		public LiftLexEntryRepositoryCreateItemTransitionTests()
		{
			_hasPersistOnCreate = false;
		}

		[SetUp]
		public override void SetUp()
		{
			_tempFolder = new TemporaryFolder();
			_persistedFilePath = _tempFolder.GetTemporaryFile();
			DataMapperUnderTest = new LiftLexEntryRepository(_persistedFilePath);
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
			_tempFolder.Delete();
		}

		[Test]
		public void SaveItem_LexEntryIsDirtyIsFalse()
		{
			SetState();
			DataMapperUnderTest.SaveItem(Item);
			Assert.IsFalse(Item.IsDirty);
		}

		[Test]
		public void SaveItems_LexEntryIsDirtyIsFalse()
		{
			SetState();
			var itemsToBeSaved = new List<LexEntry>();
			itemsToBeSaved.Add(Item);
			DataMapperUnderTest.SaveItems(itemsToBeSaved);
			Assert.IsFalse(Item.IsDirty);
		}

		[Test]
		public void Constructor_LexEntryIsDirtyIsTrue()
		{
			SetState();
			Assert.IsTrue(Item.IsDirty);
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			DataMapperUnderTest.Dispose();
			DataMapperUnderTest = new LiftLexEntryRepository(_persistedFilePath);
		}
	}
}