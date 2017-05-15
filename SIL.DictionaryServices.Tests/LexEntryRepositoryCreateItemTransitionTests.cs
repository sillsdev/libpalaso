using System.Collections.Generic;
using NUnit.Framework;
using SIL.DictionaryServices.Model;
using SIL.IO;
using SIL.Tests.Data;

namespace SIL.DictionaryServices.Tests
{
	[TestFixture]
	public class LiftLexEntryRepositoryCreateItemTransitionTests :
		IRepositoryCreateItemTransitionTests<LexEntry>
	{
		private TempFile _tempFile;
		private string _persistedFilePath;

		public LiftLexEntryRepositoryCreateItemTransitionTests()
		{
			_hasPersistOnCreate = false;
		}

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