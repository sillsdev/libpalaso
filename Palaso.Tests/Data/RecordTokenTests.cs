using System;
using System.Collections.Generic;
using NUnit.Framework;
using Palaso.Data;
using Palaso.Progress;

namespace Palaso.Tests.Data
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
		[ExpectedException(typeof (ArgumentNullException))]
		public void Construct_NullRepository_Throws()
		{
			Assert.IsNotNull(new RecordToken<PalasoTestItem>(null, new TestRepositoryId(8)));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void Construct_NullRepositoryId_Throws()
		{
			Assert.IsNotNull(new RecordToken<PalasoTestItem>(_dataMapper, null));
		}

		[Test]
		public void ConstructWithResults()
		{
			Assert.IsNotNull(new RecordToken<PalasoTestItem>(_dataMapper,
													   new Dictionary<string, object>(),
													   new TestRepositoryId(8)));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ConstructWithResults_NullRepository_Throws()
		{
			Assert.IsNotNull(new RecordToken<PalasoTestItem>(null,
													   new Dictionary<string, object>(),
													   new TestRepositoryId(8)));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ConstructWithResults_NullResults_Throws()
		{
			Assert.IsNotNull(new RecordToken<PalasoTestItem>(_dataMapper, null, new TestRepositoryId(8)));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ConstructWithResults_NullRepositoryId_Throws()
		{
			Assert.IsNotNull(new RecordToken<PalasoTestItem>(_dataMapper,
													   new Dictionary<string, object>(),
													   null));
		}
	}

	/// <summary>
	/// NOTE: starting with Resharper 5, these tests get run by themselves, and they all fail
	/// I think the (old) and expected behavior was to ignore them in isolation, because the
	/// class is *not* marked as a [TestFixture].  I don't know the correct way to fix this,
	/// so I'm just adding this note, for now.
	/// </summary>
	public class RecordTokenTestsBase
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
		[ExpectedException(typeof (ArgumentNullException))]
		public void GetIndexer_NullFieldName_Throws()
		{
			Token[null] = null;
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIndexer_AnyFieldName_Throws()
		{
			Assert.IsNull(Token["anything"]);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIndexer_EmptyName_Throws()
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

			public void Startup(ProgressState state)
			{
				throw new NotImplementedException();
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