using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using SIL.Data;

namespace SIL.Tests.Data
{
	internal class TestQuery<T> : IQuery<T> where T : class, new()
	{
		public IEnumerable<IDictionary<string, object>> GetResults(T item)
		{
			var result = new Dictionary<string, object> { { "key1", null } };
			return new[] { result };
		}
	}

	public abstract class IRepositoryStateUninitializedTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
						"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		[Test]
		public void CreateItem_NotNull()
		{
			Assert.IsNotNull(DataMapperUnderTest.CreateItem());
		}

		[Test]
		public void DeleteItem_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(() =>
				DataMapperUnderTest.DeleteItem((T) null));
		}

		[Test]
		[Category("FailsDueToSomeTeamCityProblemWhenInvokeFromWeSayTest")]
		public void DeleteItem_ItemDoesNotExist_Throws()
		{
			T item = new T();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.DeleteItem(item));
		}

		[Test]
		public void DeleteItemById_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(() =>
				DataMapperUnderTest.DeleteItem((RepositoryId) null));
		}

		[Test]
		public void DeleteItemById_ItemDoesNotExist_Throws()
		{
			MyRepositoryId id = new MyRepositoryId();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.DeleteItem(id));
		}

		[Test]
		public void DeleteAllItems_NothingInRepository_StillNothingInRepository()
		{
			DataMapperUnderTest.DeleteAllItems();
			Assert.AreEqual(0, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void CountAllItems_NoItemsInTheRepository_ReturnsZero()
		{
			Assert.AreEqual(0, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsEmptyArray()
		{
			Assert.IsEmpty(DataMapperUnderTest.GetAllItems());
		}

		[Test]
		public void GetId_ItemNotInRepository_Throws()
		{
			T item = new T();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.GetId(item));
		}

		[Test]
		public void GetItem_IdNotInRepository_Throws()
		{
			MyRepositoryId id = new MyRepositoryId();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.GetItem(id));
		}

		[Test]
		public void GetItemsMatchingQuery_CanQueryIsFalse_Throws()
		{
			if (!DataMapperUnderTest.CanQuery)
			{
				Assert.Throws<NotSupportedException>(() =>
					DataMapperUnderTest.GetItemsMatching(null));
			}
			else
			{
				Assert.Ignore("Test not relevant. This repository supports queries.");
			}
		}

		[Test]
		public void GetItemMatchingQuery_CanQueryIsTrue_ReturnsOne()
		{
			if (DataMapperUnderTest.CanQuery)
			{
				Assert.AreEqual(0, DataMapperUnderTest.GetItemsMatching(new TestQuery<T>()).Count);
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void LastModified_ReturnsMinimumPossibleTime()
		{
			Assert.AreEqual(new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc), DataMapperUnderTest.LastModified);
		}

		[Test]
		public void Save_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(() =>
				DataMapperUnderTest.SaveItem(null));
		}

		[Test]
		public void Save_ItemDoesNotExist_Throws()
		{
			T item = new T();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.SaveItem(item));
		}

		[Test]
		public void SaveItems_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(() =>
				DataMapperUnderTest.SaveItems(null));
		}

		[Test]
		public void SaveItems_ItemDoesNotExist_Throws()
		{
			T item = new T();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(item);
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.SaveItems(itemsToSave));
		}

		[Test]
		public void SaveItems_ListIsEmpty_DoNotChangeLastModified()
		{
			List<T> itemsToSave = new List<T>();
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DataMapperUnderTest.SaveItems(itemsToSave);
			Assert.AreEqual(modifiedTimePreTestedStateSwitch, DataMapperUnderTest.LastModified);
		}

		private class MyRepositoryId: RepositoryId
		{
			public override int CompareTo(RepositoryId other)
			{
				return 0;
			}

			public override bool Equals(RepositoryId other)
			{
				return true;
			}
		}
	}

	public abstract class IRepositoryCreateItemTransitionTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;

		protected bool _hasPersistOnCreate;

		protected IRepositoryCreateItemTransitionTests()
		{
			_hasPersistOnCreate = true;
		}

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
						"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		protected T Item { get; set; }

		protected RepositoryId Id { get; set; }

		public void SetState()
		{
			Item = DataMapperUnderTest.CreateItem();
			Id = DataMapperUnderTest.GetId(Item);
		}

		//This method is used to test whether data has been persisted.
		//This method should dispose of the current repository and reload it from persisted data
		//For repositories that don't support persistence this method should do nothing
		protected abstract void CreateNewRepositoryFromPersistedData();

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		[Test]
		public void CreateItem_ReturnsUniqueItem()
		{
			SetState();
			Assert.AreNotSame(Item, DataMapperUnderTest.CreateItem());
		}

		[Test]
		public void CreatedItemHasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository cannot be persisted.");
			}
			else
			{
				if (_hasPersistOnCreate)
				{
					CreateNewRepositoryFromPersistedData();
					RepositoryId[] listOfItems = DataMapperUnderTest.GetAllItems();
					Assert.AreEqual(1, listOfItems.Length);
					//Would be nice if this worked.. but it doesn't because we have equals for LexEntry is still by reference
					//T itemFromPersistedData = DataMapperUnderTest.GetItem(listOfItems[0]);
					//Assert.AreEqual(item, itemFromPersistedData);
				}
				else
				{
					Assert.Ignore("This repository does not persist on CreateItem");
				}
			}
		}

		[Test]
		public void CountAllItems_ReturnsOne()
		{
			SetState();
			Assert.AreEqual(1, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsIdItem()
		{
			SetState();
			Assert.AreEqual(DataMapperUnderTest.GetId(Item), DataMapperUnderTest.GetAllItems()[0]);
		}

		[Test]
		public void GetAllItems_ReturnsCorrectNumberOfExistingItems()
		{
			SetState();
			Assert.AreEqual(1, DataMapperUnderTest.GetAllItems().Length);
		}

		[Test]
		public void GetId_CalledTwiceWithSameItem_ReturnsSameId()
		{
			SetState();
			Assert.AreEqual(DataMapperUnderTest.GetId(Item), DataMapperUnderTest.GetId(Item));
		}

		[Test]
		public void GetId_Item_ReturnsIdOfItem()
		{
			SetState();
			Assert.AreEqual(Id, DataMapperUnderTest.GetId(Item));
		}

		[Test]
		public void GetItem_Id_ReturnsItemWithId()
		{
			SetState();
			Assert.AreSame(Item, DataMapperUnderTest.GetItem(Id));
		}

		[Test]
		public void GetItem_CalledTwiceWithSameId_ReturnsSameItem()
		{
			SetState();
			Assert.AreSame(DataMapperUnderTest.GetItem(Id), DataMapperUnderTest.GetItem(Id));
		}

		[Test]
		public void SaveItem_LastModifiedIsChangedToLaterTime()
		{
			SetState();
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DataMapperUnderTest.SaveItem(Item);
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreTestedStateSwitch);
		}

		[Test]
		public void SaveItem_LastModifiedIsSetInUTC()
		{
			SetState();
			DataMapperUnderTest.SaveItem(Item);
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		[Test]
		public void SaveItem_ItemHasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository cannot be persisted.");
			}
			else
			{
				DataMapperUnderTest.SaveItem(Item);
				CreateNewRepositoryFromPersistedData();
				Assert.AreEqual(1, DataMapperUnderTest.CountAllItems());
			}
		}

		[Test]
		public void SaveItems_LastModifiedIsChangedToLaterTime()
		{
			SetState();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(Item);
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DataMapperUnderTest.SaveItems(itemsToSave);
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreTestedStateSwitch);
		}

		[Test]
		public void SaveItems_LastModifiedIsSetInUTC()
		{
			SetState();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(Item);
			Thread.Sleep(50);
			DataMapperUnderTest.SaveItems(itemsToSave);
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		[Test]
		public void SaveItems_ItemHasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository cannot be persisted.");
			}
			else
			{
				List<T> itemsToBeSaved = new List<T> { Item };
				DataMapperUnderTest.SaveItems(itemsToBeSaved);
				CreateNewRepositoryFromPersistedData();
				Assert.AreEqual(1, DataMapperUnderTest.CountAllItems());
			}
		}
	}

	public abstract class IRepositoryPopulateFromPersistedTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
						"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		protected T Item { get; set; }

		protected RepositoryId Id { get; set; }

		public void SetState()
		{
			RepositoryId[] idsFromPersistedData = DataMapperUnderTest.GetAllItems();
			Id = idsFromPersistedData[0];
			Item = DataMapperUnderTest.GetItem(Id);
		}

		//This method is used to test whether data has been persisted.
		//This method should dispose of the current repository and reload it from persisted data
		//For repositories that don't support persistence this method should do nothing
		protected abstract void CreateNewRepositoryFromPersistedData();

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		[Test]
		public void CreateItem_ReturnsUniqueItem()
		{
			SetState();
			Assert.AreNotSame(Item, DataMapperUnderTest.CreateItem());
		}

		[Test]
		public void CreatedItemHasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository cannot be persisted.");
			}
			else
			{
				CreateNewRepositoryFromPersistedData();
				RepositoryId[] listOfItems = DataMapperUnderTest.GetAllItems();
				Assert.AreEqual(1, listOfItems.Length);
				//Would be nice if this worked.. but it doesn't because we have equals for LexEntry is still by reference
				//T itemFromPersistedData = DataMapperUnderTest.GetItem(listOfItems[0]);
				//Assert.AreEqual(item, itemFromPersistedData);
			}
		}

		[Test]
		public void CountAllItems_ReturnsOne()
		{
			SetState();
			Assert.AreEqual(1, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsIdItem()
		{
			SetState();
			Assert.AreEqual(DataMapperUnderTest.GetId(Item), DataMapperUnderTest.GetAllItems()[0]);
		}

		[Test]
		public void GetAllItems_ReturnsCorrectNumberOfExistingItems()
		{
			SetState();
			Assert.AreEqual(1, DataMapperUnderTest.GetAllItems().Length);
		}

		[Test]
		public void GetId_CalledTwiceWithSameItem_ReturnsSameId()
		{
			SetState();
			Assert.AreEqual(DataMapperUnderTest.GetId(Item), DataMapperUnderTest.GetId(Item));
		}

		[Test]
		public void GetId_Item_ReturnsIdOfItem()
		{
			SetState();
			Assert.AreEqual(Id, DataMapperUnderTest.GetId(Item));
		}

		[Test]
		public void GetItem_Id_ReturnsItemWithId()
		{
			SetState();
			Assert.AreSame(Item, DataMapperUnderTest.GetItem(Id));
		}

		[Test]
		public void GetItem_CalledTwiceWithSameId_ReturnsSameItem()
		{
			SetState();
			Assert.AreSame(DataMapperUnderTest.GetItem(Id), DataMapperUnderTest.GetItem(Id));
		}

		[Test]
		public void LastModified_IsSetToMostRecentItemInPersistedDatasLastModifiedTime()
		{
			CreateNewRepositoryFromPersistedData();
			SetState();
			LastModified_IsSetToMostRecentItemInPersistedDatasLastModifiedTime_v();
		}

		protected virtual void LastModified_IsSetToMostRecentItemInPersistedDatasLastModifiedTime_v()
		{
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository cannot be persisted.");
			}
			else
			{
				Assert.Fail(
					"This test is dependant on how you are persisting your data, please override this test.");
			}
		}

		[Test]
		public void LastModified_IsSetInUTC()
		{
			SetState();
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		//This test is virtual because LexEntryRepository needs a special implementation
		[Test]
		public virtual void SaveItem_LastModifiedIsChangedToLaterTime()
		{
			SetState();
			DateTime modifiedTimePreSave = DataMapperUnderTest.LastModified;
			DataMapperUnderTest.SaveItem(Item);
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreSave);
		}

		//This test is virtual because LexEntryRepository needs a special implementation
		[Test]
		public virtual void SaveItem_LastModifiedIsSetInUTC()
		{
			SetState();
			DataMapperUnderTest.SaveItem(Item);
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		//This test is virtual because LexEntryRepository needs a special implementation
		[Test]
		public virtual void SaveItems_LastModifiedIsChangedToLaterTime()
		{
			SetState();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(Item);
			DateTime modifiedTimePreSave = DataMapperUnderTest.LastModified;
			DataMapperUnderTest.SaveItems(itemsToSave);
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreSave);
		}

		[Test]
		public void SaveItems_LastModifiedIsSetInUTC()
		{
			SetState();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(Item);
			Thread.Sleep(50);
			DataMapperUnderTest.SaveItems(itemsToSave);
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}
	}

	public abstract class IRepositoryDeleteItemTransitionTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;
		private RepositoryId id;

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
						"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		public T Item { get; private set; }

		//This method is used to test whether data has been persisted.
		//This method should dispose of the current repository and reload it from persisted data
		//For repositories that don't support persistence this method should do nothing
		protected abstract void CreateNewRepositoryFromPersistedData();

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		public void SetState()
		{
			CreateInitialItem();
			DeleteItem();
		}

		private void DeleteItem()
		{
			DataMapperUnderTest.DeleteItem(Item);
		}

		private void CreateInitialItem()
		{
			Item = DataMapperUnderTest.CreateItem();
			id = DataMapperUnderTest.GetId(Item);
		}

		[Test]
		public void DeleteItem_ItemDoesNotExist_Throws()
		{
			SetState();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.DeleteItem(Item));
		}

		[Test]
		[Category("FailsDueToSomeTeamCityProblemWhenInvokeFromWeSayTest")]
		public void DeleteItem_HasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository cannot be persisted.");
			}
			else
			{
				CreateNewRepositoryFromPersistedData();
				Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.GetItem(id));
			}
		}

		[Test]
		public void CountAllItems_ReturnsZero()
		{
			SetState();
			Assert.AreEqual(0, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsEmptyArray()
		{
			SetState();
			Assert.IsEmpty(DataMapperUnderTest.GetAllItems());
		}

		[Test]
		[Category("FailsDueToSomeTeamCityProblemWhenInvokeFromWeSayTest")]
		public void GetId_DeletedItemWithId_Throws()
		{
			SetState();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.GetId(Item));
		}

		[Test]
		[Category("FailsDueToSomeTeamCityProblemWhenInvokeFromWeSayTest")]
		public void GetItem_DeletedItem_Throws()
		{
			SetState();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.GetItem(id));
		}

		[Test]
		public void GetItemMatchingQuery_CanQuery_ReturnsZero()
		{
			SetState();
			if (DataMapperUnderTest.CanQuery)
			{
				Assert.AreEqual(0, DataMapperUnderTest.GetItemsMatching(new TestQuery<T>()).Count);
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void LastModified_IsChangedToLaterTime()
		{
			CreateInitialItem();
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DeleteItem();
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreTestedStateSwitch);
		}

		[Test]
		public void LastModified_IsSetInUTC()
		{
			SetState();
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		//This test is virtual because LexEntryRepository needs to override it
		[Test]
		public virtual void SaveItem_ItemDoesNotExist_Throws()
		{
			SetState();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.SaveItem(Item));
		}

		[Test]
		[Category("FailsDueToSomeTeamCityProblemWhenInvokeFromWeSayTest")]
		public void SaveItems_ItemDoesNotExist_Throws()
		{
			SetState();
			T itemNotInRepository = new T();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(itemNotInRepository);
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.SaveItems(itemsToSave));
		}
	}

	public abstract class IRepositoryDeleteIdTransitionTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;
		private RepositoryId id;

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
						"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		public T Item { get; private set; }

		//This method is used to test whether data has been persisted.
		//This method should dispose of the current repository and reload it from persisted data
		//For repositories that don't support persistence this method should do nothing
		protected abstract void CreateNewRepositoryFromPersistedData();

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		public void SetState()
		{
			CreateItemToTest();
			DeleteItem();
		}

		private void DeleteItem()
		{
			DataMapperUnderTest.DeleteItem(this.id);
		}

		private void CreateItemToTest()
		{
			Item = DataMapperUnderTest.CreateItem();
			this.id = DataMapperUnderTest.GetId(Item);
		}

		[Test]
		public void DeleteItem_ItemDoesNotExist_Throws()
		{
			SetState();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.DeleteItem(id));
		}

		[Test]
		public void DeleteItem_HasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository cannot be persisted.");
			}
			else
			{
				CreateNewRepositoryFromPersistedData();
				Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.GetItem(id));
			}
		}

		[Test]
		public void CountAllItems_ReturnsZero()
		{
			SetState();
			Assert.AreEqual(0, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsEmptyArray()
		{
			SetState();
			Assert.IsEmpty(DataMapperUnderTest.GetAllItems());
		}

		[Test]
		[Category("FailsDueToSomeTeamCityProblemWhenInvokeFromWeSayTest")]
		public void GetId_DeletedItemWithId_Throws()
		{
			SetState();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.GetId(Item));
		}

		[Test]
		[Category("FailsDueToSomeTeamCityProblemWhenInvokeFromWeSayTest")]
		public void GetItem_DeletedItem_Throws()
		{
			SetState();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.GetItem(id));
		}

		[Test]
		public void GetItemMatchingQuery_CanQuery_ReturnsZero()
		{
			SetState();
			if (DataMapperUnderTest.CanQuery)
			{
				Assert.AreEqual(0, DataMapperUnderTest.GetItemsMatching(new TestQuery<T>()).Count);
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void LastModified_ItemIsDeleted_IsChangedToLaterTime()
		{
			CreateItemToTest();
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DeleteItem();
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreTestedStateSwitch);
		}

		[Test]
		public void LastModified_ItemIsDeleted_IsSetInUTC()
		{
			SetState();
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		//This test is virtual because LexEntryRepository needs to override it
		[Test]
		public virtual void SaveItem_ItemDoesNotExist_Throws()
		{
			SetState();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.SaveItem(Item));
		}

		[Test]
		[Category("FailsDueToSomeTeamCityProblemWhenInvokeFromWeSayTest")]
		public void SaveItems_ItemDoesNotExist_Throws()
		{
			SetState();
			T itemNotInRepository = new T();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(itemNotInRepository);
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.SaveItems(itemsToSave));
		}
	}

	public abstract class IRepositoryDeleteAllItemsTransitionTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;
		private T item;
		private RepositoryId id;

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
						"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		//This method is used to test whether data has been persisted.
		//This method should dispose of the current repository and reload it from persisted data
		//For repositories that don't support persistence this method should do nothing
		protected abstract void RepopulateRepositoryFromPersistedData();

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		public void SetState()
		{
			CreateInitialItem();
			DeleteAllItems();
		}

		private void DeleteAllItems()
		{
			DataMapperUnderTest.DeleteAllItems();
		}

		private void CreateInitialItem()
		{
			this.item = DataMapperUnderTest.CreateItem();
			this.id = DataMapperUnderTest.GetId(this.item);
		}

		[Test]
		public void DeleteAllItems_ItemDoesNotExist_DoesNotThrow()
		{
			SetState();
			DataMapperUnderTest.DeleteAllItems();
		}

		[Test]
		public void DeleteAllItems_HasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository cannot be persisted.");
			}
			else
			{
				RepopulateRepositoryFromPersistedData();
				Assert.IsEmpty(DataMapperUnderTest.GetAllItems());
			}
		}

		[Test]
		public void CountAllItems_ReturnsZero()
		{
			SetState();
			Assert.AreEqual(0, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsEmptyArray()
		{
			SetState();
			Assert.IsEmpty(DataMapperUnderTest.GetAllItems());
		}

		[Test]
		public void GetId_DeletedItemWithId_Throws()
		{
			SetState();
			Assert.Throws<ArgumentOutOfRangeException>(() => DataMapperUnderTest.GetId(item));
		}

		[Test]
		public void GetItem_DeletedItem_Throws()
		{
			SetState();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.GetItem(id));
		}

		[Test]
		public void GetItemMatchingQuery_CanQuery_ReturnsZero()
		{
			SetState();
			if (DataMapperUnderTest.CanQuery)
			{
				Assert.AreEqual(0, DataMapperUnderTest.GetItemsMatching(new TestQuery<T>()).Count);
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void LastModified_IsChangedToLaterTime()
		{
			CreateInitialItem();
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DeleteAllItems();
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreTestedStateSwitch);
		}

		[Test]
		public void LastModified_IsSetInUTC()
		{
			SetState();
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		[Test]
		[Category("FailsDueToSomeTeamCityProblemWhenInvokeFromWeSayTest")]
		public void Save_ItemDoesNotExist_Throws()
		{
			SetState();
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.SaveItem(item));
		}

		[Test]
		[Category("FailsDueToSomeTeamCityProblemWhenInvokeFromWeSayTest")]
		public void SaveItems_ItemDoesNotExist_Throws()
		{
			T itemNotInRepository = new T();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(itemNotInRepository);
			Assert.Throws<ArgumentOutOfRangeException>(() =>DataMapperUnderTest.SaveItems(itemsToSave));
		}
	}
}
