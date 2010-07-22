using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Data;

namespace Palaso.Tests.Data
{
	[TestFixture]
	public class IQueryTests
	{
		private MemoryDataMapper<SimpleObject> _repo;

		[SetUp]
		public void Setup()
		{
			_repo = new MemoryDataMapper<SimpleObject>();
		}

		[TearDown]
		public void TearDown()
		{
			_repo.Dispose();
		}

		[Test]
		public void Query1_ReturnsField1SortedAscending()
		{
			CreateItemInRepo(1, 2, 0, 0, 0);
			CreateItemInRepo(3, 4, 0, 0, 0);
			IQuery<SimpleObject> field1Query = new Field1Query();
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(1, results[0]["Field1"]);
			Assert.AreEqual(2, results[1]["Field1"]);
			Assert.AreEqual(3, results[2]["Field1"]);
			Assert.AreEqual(4, results[3]["Field1"]);
		}

		[Test]
		public void Query1_DuplicateValues_DoesNotThrow()
		{
			CreateItemInRepo(1, 2, 0, 0, 0);
			CreateItemInRepo(3, 1, 0, 0, 0);
			IQuery<SimpleObject> field1Query = new Field1Query();
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(1, results[0]["Field1"]);
			Assert.AreEqual(1, results[1]["Field1"]);
			Assert.AreEqual(2, results[2]["Field1"]);
			Assert.AreEqual(3, results[3]["Field1"]);
		}

		[Test]
		public void Query2_ReturnsField2SortedDescending()
		{
			CreateItemInRepo(0, 0, 1, 3, 6);
			CreateItemInRepo(0, 0, 4, 2, 5);
			Field2Query field2Query = new Field2Query();
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field2Query);
			Assert.AreEqual(6, results[0]["Field2"]);
			Assert.AreEqual(5, results[1]["Field2"]);
			Assert.AreEqual(4, results[2]["Field2"]);
			Assert.AreEqual(3, results[3]["Field2"]);
			Assert.AreEqual(2, results[4]["Field2"]);
			Assert.AreEqual(1, results[5]["Field2"]);
		}

		[Test]
		public void Query2_DuplicateValues_DoesNotThrow()
		{
			CreateItemInRepo(0, 0, 1, 3, 5);
			CreateItemInRepo(0, 0, 4, 2, 5);
			Field2Query field2Query = new Field2Query();
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field2Query);
			Assert.AreEqual(5, results[0]["Field2"]);
			Assert.AreEqual(5, results[1]["Field2"]);
			Assert.AreEqual(4, results[2]["Field2"]);
			Assert.AreEqual(3, results[3]["Field2"]);
			Assert.AreEqual(2, results[4]["Field2"]);
			Assert.AreEqual(1, results[5]["Field2"]);
		}

		[Test]
		public void TwoQueries_Anded_ReturnsNumberOfFieldsOfQueryResultWithGreaterNumberOfFields()
		{
			CreateItemInRepo(1, 4, 1, 3, 6);
			CreateItemInRepo(3, 2, 4, 2, 5);
			IQuery<SimpleObject> field1Query = new Field1Query() & new Field2Query();
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(12, results.Count);
		}

		[Test]
		public void TwoQueries_Anded_RecordTokensAreSortedByFirstQueryPrimarilyAndSecondQuerySecondarily()
		{
			CreateItemInRepo(1, 4, 1, 3, 6);
			CreateItemInRepo(3, 2, 4, 2, 5);
			IQuery<SimpleObject> field1Query = new Field1Query() & new Field2Query();
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(1, results[0]["Field1"]);
			Assert.AreEqual(6, results[0]["Field2"]);

			Assert.AreEqual(1, results[1]["Field1"]);
			Assert.AreEqual(3, results[1]["Field2"]);

			Assert.AreEqual(1, results[2]["Field1"]);
			Assert.AreEqual(1, results[2]["Field2"]);

			Assert.AreEqual(2, results[3]["Field1"]);
			Assert.AreEqual(5, results[3]["Field2"]);

			Assert.AreEqual(2, results[4]["Field1"]);
			Assert.AreEqual(4, results[4]["Field2"]);

			Assert.AreEqual(2, results[5]["Field1"]);
			Assert.AreEqual(2, results[5]["Field2"]);
		}

		private SimpleObject CreateItemInRepo(int field1_0, int field1_1, int field2_0, int field2_1, int field2_2)
		{
			SimpleObject item = _repo.CreateItem();
			item.Field1 = new List<int>(){ field1_0, field1_1 };
			item.Field2 = new List<int>(){field2_0, field2_1, field2_2};
			_repo.SaveItem(item);
			return item;
		}

		private class SimpleObject
		{
			public List<int> Field1;
			public List<int> Field2;
		}

		private class GreaterThan:Comparer<int>
		{

			public override int Compare(int x, int y)
			{
				if (x > y)
				{
					return 1;
				}
				if (x < y)
				{
					return -1;
				}
				return 0;
			}
		}

		private class LessThan : Comparer<int>
		{

			public override int Compare(int x, int y)
			{
				if (x < y)
				{
					return 1;
				}
				if (x > y)
				{
					return -1;
				}
				return 0;
			}
		}

		private class Field1Query:IQuery<SimpleObject>
		{

			public override IEnumerable<IDictionary<string, object>> GetResults(SimpleObject item)
			{
				List<IDictionary<string, object>> results = new List<IDictionary<string, object>>();
				for (int i = 0; i < item.Field1.Count; i++)
				{
					Dictionary<string, object> tokenFieldLabelsAndValues = new Dictionary<string, object>();
					tokenFieldLabelsAndValues.Add("Field1", item.Field1[i]);
					tokenFieldLabelsAndValues.Add("Field1ListItem", i);
					results.Add(tokenFieldLabelsAndValues);
				}
				return results;
			}

			public override IEnumerable<SortDefinition> SortDefinitions
			{
				get
				{
					return new List<SortDefinition>
							   {
								   new SortDefinition("Field1", new GreaterThan()),
								   new SortDefinition("Field1ListItem", new GreaterThan())
							   };
				}
			}

			public override string UniqueLabel
			{
				get { return "Field1Query"; }
			}
		}

		private class Field2Query : IQuery<SimpleObject>
		{
			public override IEnumerable<IDictionary<string, object>> GetResults(SimpleObject item)
			{
				List<IDictionary<string, object>> results = new List<IDictionary<string, object>>();
				for(int i=0;i<item.Field2.Count;i++)
				{
					Dictionary<string, object> tokenFieldLabelsAndValues = new Dictionary<string, object>();
					tokenFieldLabelsAndValues.Add("Field2", item.Field2[i]);
					tokenFieldLabelsAndValues.Add("Field2ListItem", i);
					results.Add(tokenFieldLabelsAndValues);
				}
				return results;
			}

			public override IEnumerable<SortDefinition> SortDefinitions
			{
				get
				{
					return new List<SortDefinition>
							   {
								   new SortDefinition("Field2", new LessThan()),
								   new SortDefinition("Field2ListItem", new GreaterThan())
							   };
				}
			}

			public override string UniqueLabel
			{
				get { return "Field2Query"; }
			}
		}
	}
}
