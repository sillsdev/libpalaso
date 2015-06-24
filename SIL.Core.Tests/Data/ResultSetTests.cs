using System.Collections.Generic;
using NUnit.Framework;
using SIL.Data;

namespace SIL.Tests.Data
{
	[TestFixture]
	public class ResultSetTests
	{
		private ResultSet<PalasoTestItem> _resultSet;
		List<RecordToken<PalasoTestItem>> _results;
		MemoryDataMapper<PalasoTestItem> _dataMapper;


		[SetUp]
		public void Setup()
		{
			_dataMapper = new MemoryDataMapper<PalasoTestItem>();
			_results = new List<RecordToken<PalasoTestItem>>();

			_results.Add(new RecordToken<PalasoTestItem>(_dataMapper, new TestRepositoryId(8)));
			_results.Add(new RecordToken<PalasoTestItem>(_dataMapper, new TestRepositoryId(12)));
			_results.Add(new RecordToken<PalasoTestItem>(_dataMapper, new TestRepositoryId(1)));
			_results.Add(new RecordToken<PalasoTestItem>(_dataMapper, new TestRepositoryId(3)));

			_resultSet = new ResultSet<PalasoTestItem>(_dataMapper, _results);
		}

		[TearDown]
		public void Teardown()
		{
			_dataMapper.Dispose();
		}

		[Test]
		public void FindFirstIndex_RepositoryIdEqualToOneInList_Index()
		{
			int index = _resultSet.FindFirstIndex(new TestRepositoryId(1));
			Assert.AreEqual(2, index);
		}

		[Test]
		public void Constructor_HasNoDuplicate_OrderRetained()
		{
			List<RecordToken<PalasoTestItem>> results = new List<RecordToken<PalasoTestItem>>();

			results.Add(new RecordToken<PalasoTestItem>(_dataMapper, new TestRepositoryId(8)));
			results.Add(new RecordToken<PalasoTestItem>(_dataMapper, new TestRepositoryId(2)));

			ResultSet<PalasoTestItem> resultSet = new ResultSet<PalasoTestItem>(_dataMapper, results);
			Assert.AreEqual(2, resultSet.Count);

			int i = 0;
			foreach (RecordToken<PalasoTestItem> token in resultSet)
			{
				Assert.AreEqual(results[i], token);
				++i;
			}
		}

		[Test]
		public void Constructor_ResultsModifiedAfter_ResultSetNotModified()
		{
			ResultSet<PalasoTestItem> resultSet = new ResultSet<PalasoTestItem>(_dataMapper, _results);
			_results.Add(new RecordToken<PalasoTestItem>(_dataMapper, _resultSet[2].Id));
			Assert.AreNotEqual(_results.Count, resultSet.Count);

		}

	}

	[TestFixture ]
	public class ResultSetWithQueryTests
	{
		MemoryDataMapper<PalasoTestItem> dataMapper;
		Dictionary<string, object> _queryResultsEmpty;
		Dictionary<string, object> _queryResultsB;
		Dictionary<string, object> _queryResultsA;

		[SetUp]
		public void Setup()
		{
			dataMapper = new MemoryDataMapper<PalasoTestItem>();
			_queryResultsA = new Dictionary<string, object>();
			_queryResultsA.Add("string", "A");
			_queryResultsB = new Dictionary<string, object>();
			_queryResultsB.Add("string", "B");
			_queryResultsEmpty = new Dictionary<string, object>();
			_queryResultsEmpty.Add("string", string.Empty);
		}

		[TearDown]
		public void Teardown()
		{
			dataMapper.Dispose();
		}

		[Test]
		public void Constructor_HasNoDuplicates_NoneRemoved()
		{
			List<RecordToken<PalasoTestItem>> results = new List<RecordToken<PalasoTestItem>>();

			results.Add(new RecordToken<PalasoTestItem>(dataMapper, new TestRepositoryId(8)));
			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsEmpty, new TestRepositoryId(8)));
			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsA, new TestRepositoryId(8)));
			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsB, new TestRepositoryId(8)));

			ResultSet<PalasoTestItem> resultSet = new ResultSet<PalasoTestItem>(dataMapper, results);
			Assert.AreEqual(4, resultSet.Count);
		}

		[Test]
		public void Coalesce_NoItemsCanBeRemoved_NoneRemoved()
		{
			List<RecordToken<PalasoTestItem>> results = new List<RecordToken<PalasoTestItem>>();

			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsA, new TestRepositoryId(8)));
			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsB, new TestRepositoryId(12)));

			ResultSet<PalasoTestItem> resultSet = new ResultSet<PalasoTestItem>(dataMapper, results);
			resultSet.Coalesce("string", delegate(object o)
											 {
												 return string.IsNullOrEmpty((string)o);
											 });
			Assert.AreEqual(2, resultSet.Count);
		}

		[Test]
		public void Coalesce_ItemCanBeRemovedButNoItemThatCannot_NoneRemoved()
		{
			List<RecordToken<PalasoTestItem>> results = new List<RecordToken<PalasoTestItem>>();

			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsEmpty, new TestRepositoryId(8)));
			results.Add(new RecordToken<PalasoTestItem>(dataMapper, new TestRepositoryId(12)));

			ResultSet<PalasoTestItem> resultSet = new ResultSet<PalasoTestItem>(dataMapper, results);
			resultSet.Coalesce("string", delegate(object o)
											 {
												 return string.IsNullOrEmpty((string)o);
											 });

			Assert.AreEqual(2, resultSet.Count);
		}

		[Test]
		public void Coalesce_ItemThatCanBeRemovedItemThatCannot_ItemRemoved()
		{
			List<RecordToken<PalasoTestItem>> results = new List<RecordToken<PalasoTestItem>>();

			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsEmpty, new TestRepositoryId(8)));
			results.Add(new RecordToken<PalasoTestItem>(dataMapper, new TestRepositoryId(12)));
			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsA, new TestRepositoryId(8)));
			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsB, new TestRepositoryId(12)));

			ResultSet<PalasoTestItem> resultSet = new ResultSet<PalasoTestItem>(dataMapper, results);
			resultSet.Coalesce("string", delegate(object o)
											 {
												 return string.IsNullOrEmpty((string)o);
											 });

			Assert.AreEqual(2, resultSet.Count);

		}

		[Test]
		public void Coalesce_ItemsCanBeRemoved_DoesNotChangeOrder()
		{
			List<RecordToken<PalasoTestItem>> results = new List<RecordToken<PalasoTestItem>>();

			results.Add(new RecordToken<PalasoTestItem>(dataMapper, new TestRepositoryId(12)));
			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsEmpty, new TestRepositoryId(8)));
			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsB, new TestRepositoryId(12)));
			results.Add(new RecordToken<PalasoTestItem>(dataMapper, _queryResultsA, new TestRepositoryId(8)));

			ResultSet<PalasoTestItem> resultSet = new ResultSet<PalasoTestItem>(dataMapper, results);
			resultSet.Coalesce("string", delegate(object o)
											 {
												 return string.IsNullOrEmpty((string)o);
											 });

			Assert.AreEqual("B", resultSet[0]["string"]);
			Assert.AreEqual("A", resultSet[1]["string"]);

		}

	}
}