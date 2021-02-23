using System;
using System.Collections.Generic;
using NUnit.Framework;
using SIL.Data;
using SIL.Progress;

namespace SIL.Tests.Data
{
	[TestFixture]
	public class RecordTokenTests
	{
		private MemoryDataMapper<PalasoTestItem> _dataMapper;

		[SetUp]
		public void Setup()
		{
			_dataMapper = new MemoryDataMapper<PalasoTestItem>();
		}

		[TearDown]
		public void Teardown()
		{
			_dataMapper.Dispose();
		}

		[Test]
		public void Construct()
		{
			Assert.IsNotNull(new RecordToken<PalasoTestItem>(_dataMapper, new TestRepositoryId(8)));
		}

		[Test]
		public void Construct_NullRepository_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => Assert.IsNotNull(new RecordToken<PalasoTestItem>(null, new TestRepositoryId(8))));
		}

		[Test]
		public void Construct_NullRepositoryId_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => Assert.IsNotNull(new RecordToken<PalasoTestItem>(_dataMapper, null)));
		}

		[Test]
		public void ConstructWithResults()
		{
			Assert.IsNotNull(new RecordToken<PalasoTestItem>(_dataMapper,
													   new Dictionary<string, object>(),
													   new TestRepositoryId(8)));
		}

		[Test]
		public void ConstructWithResults_NullRepository_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => Assert.IsNotNull(new RecordToken<PalasoTestItem>(null, new Dictionary<string, object>(), new TestRepositoryId(8))));
		}

		[Test]
		public void ConstructWithResults_NullResults_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => Assert.IsNotNull(new RecordToken<PalasoTestItem>(_dataMapper, null, new TestRepositoryId(8))));
		}

		[Test]
		public void ConstructWithResults_NullRepositoryId_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => Assert.IsNotNull(new RecordToken<PalasoTestItem>(_dataMapper, new Dictionary<string, object>(), null)));
		}
	}

	public abstract class RecordTokenTestsBase
	{
		private RecordToken<PalasoTestItem> _token;

		public RecordToken<PalasoTestItem> Token
		{
			get
			{
				if (_token == null)
				{
					throw new InvalidOperationException(
						"Token must be initialized before tests can be run");
				}
				return _token;
			}
			set { _token = value; }
		}

		[Test]
		public void GetIndexer_NullFieldName_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => Token[null] = null);
		}

		[Test]
		public void GetIndexer_AnyFieldName_Null()
		{
			Assert.IsNull(Token["anything"]);
		}

		[Test]
		public void GetIndexer_EmptyName_Null()
		{
			Assert.IsNull(Token[""]);
		}

		[Test]
		public void SetIndexer_AnthingFieldName_Null_KeepsNull()
		{
			Token["anything"] = null;
			Assert.IsNull(Token["anything"]);
		}

		[Test]
		public void SetIndexer_AnthingFieldName_Value_KeepsValue()
		{
			object o = new object();
			Token["anything"] = o;
			Assert.AreSame(o, Token["anything"]);
		}

		[Test]
		public void SetIndexer_EmptyFieldName_Value_KeepsValue()
		{
			object o = new object();
			Token[""] = o;
			Assert.AreSame(o, Token[""]);
		}

		[Test]
		public void Id_AsConstructed()
		{
			Assert.AreEqual(Token.Id, new TestRepositoryId(8));
		}
	}

	[TestFixture]
	public class RecordTokenConstructedWithoutResultsTests: RecordTokenTestsBase
	{
		private MemoryDataMapper<PalasoTestItem> dataMapper;


		[SetUp]
		public void Setup()
		{
			dataMapper = new MemoryDataMapper<PalasoTestItem>();
			Token = new RecordToken<PalasoTestItem>(dataMapper, new TestRepositoryId(8));
		}

		[TearDown]
		public void Teardown()
		{
			dataMapper.Dispose();
		}
	}

	[TestFixture]
	public class RecordTokenConstructedWithResultsTests: RecordTokenTestsBase
	{
		private MemoryDataMapper<PalasoTestItem> dataMapper;

		[SetUp]
		public void Setup()
		{
			dataMapper = new MemoryDataMapper<PalasoTestItem>();
			Dictionary<string, object> results = new Dictionary<string, object>();
			results["string"] = "result";
			results["int"] = 12;
			Token = new RecordToken<PalasoTestItem>(dataMapper, results, new TestRepositoryId(8));
		}

		[TearDown]
		public void Teardown()
		{
			dataMapper.Dispose();
		}

		[Test]
		public void RecordTokensAddedToSortedDictionary_RecordTokensDifferOnlyInForm_DoesNotThrow()
		{
			var results1 = new Dictionary<string, object>();
			results1.Add("Form", "សង្ឃនៃអំបូរអឺរ៉ុន");
			//results1.Add("Form", "form1");
			results1.Add("Sense", 0);
			var results2 = new Dictionary<string, object>();
			results2.Add("Form", "បូជាចារ្យនៃអំបូរអឺរ៉ុន");
			//results2.Add("Form", "form2");
			results2.Add("Sense", 0);
			var rt1 = new RecordToken<PalasoTestItem>(dataMapper, results1, new TestRepositoryId(8));
			var rt2 = new RecordToken<PalasoTestItem>(dataMapper, results2, new TestRepositoryId(8));
			var sortedDictionary = new SortedDictionary<RecordToken<PalasoTestItem>, object>();
			sortedDictionary.Add(rt1, null);
			sortedDictionary.Add(rt2, null);
			Console.WriteLine("");
		}




		[Test]
		public void GetIndexer_ExistingFieldName_ConstructedValue()
		{
			Assert.AreEqual("result", Token["string"]);
			Assert.AreEqual(12, Token["int"]);
		}

		[Test]
		public void SetIndexer_ExistingFieldName_OverwritesValue()
		{
			Token["string"] = "new result";
			Assert.AreEqual("new result", Token["string"]);
		}
	}

	[TestFixture]
	public class RecordTokenUsesRepositoryTests
	{
		private class TestRepository: IDataMapper<PalasoTestItem>
		{
			private readonly PalasoTestItem _itemToReturn;

			public TestRepository(PalasoTestItem itemToReturn)
			{
				_itemToReturn = itemToReturn;
			}

			public DateTime LastModified
			{
				get { throw new NotImplementedException(); }
			}

			public PalasoTestItem CreateItem()
			{
				throw new NotImplementedException();
			}

			public int CountAllItems()
			{
				throw new NotImplementedException();
			}

			public RepositoryId GetId(PalasoTestItem item)
			{
				throw new NotImplementedException();
			}

			public PalasoTestItem GetItem(RepositoryId id)
			{
				Assert.AreEqual(id, new TestRepositoryId(8));
				return _itemToReturn;
			}

			public void DeleteItem(PalasoTestItem item)
			{
				throw new NotImplementedException();
			}

			public void DeleteItem(RepositoryId id)
			{
				throw new NotImplementedException();
			}

			public void DeleteAllItems()
			{
				throw new NotImplementedException();
			}

			public RepositoryId[] GetAllItems()
			{
				throw new NotImplementedException();
			}

			public void SaveItem(PalasoTestItem item)
			{
				throw new NotImplementedException();
			}

			public bool CanQuery
			{
				get { return false; }
			}

			public bool CanPersist
			{
				get { return false; }
			}

			public void SaveItems(IEnumerable<PalasoTestItem> items)
			{
				throw new NotImplementedException();
			}

			public ResultSet<PalasoTestItem> GetItemsMatching(IQuery<PalasoTestItem> query)
			{
				throw new NotImplementedException();
			}

			public void Dispose() {}
		}

		private TestRepository _repository;
		private RecordToken<PalasoTestItem> _token;
		private PalasoTestItem _testItem;

		[SetUp]
		public void Setup()
		{
			this._testItem = new PalasoTestItem();
			_repository = new TestRepository(this._testItem);
			this._token = new RecordToken<PalasoTestItem>(this._repository, new TestRepositoryId(8));
		}

		[TearDown]
		public void Teardown()
		{
			_repository.Dispose();
		}

		[Test]
		public void RealObject_DelegatesToRepository()
		{
			Assert.AreSame(_testItem, _token.RealObject);
		}
	}
}