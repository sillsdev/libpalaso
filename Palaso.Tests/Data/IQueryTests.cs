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
			SimpleObject item1 = _repo.CreateItem();
			SimpleObject item2 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1,2);
			AddValuesToField(item1.Field2, 0,0,0);
			AddValuesToField(item2.Field1, 3,4);
			AddValuesToField(item2.Field2, 0,0,0);
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
			SimpleObject item1 = _repo.CreateItem();
			SimpleObject item2 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1,2);
			AddValuesToField(item1.Field2, 0,0,0);
			AddValuesToField(item2.Field1, 3,1);
			AddValuesToField(item2.Field2, 0,0,0);
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
			SimpleObject item1 = _repo.CreateItem();
			SimpleObject item2 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 0,0);
			AddValuesToField(item1.Field2, 1,3,6);
			AddValuesToField(item2.Field1, 0,0);
			AddValuesToField(item2.Field2, 4,2,5);
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
			SimpleObject item1 = _repo.CreateItem();
			SimpleObject item2 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 0,0);
			AddValuesToField(item1.Field2, 1,3,5);
			AddValuesToField(item2.Field1, 0,0);
			AddValuesToField(item2.Field2, 4,2,5);
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
		public void JoinInner_TwoQueriesWithDifferentNumberOfResults_ReturnsProductOfNumberOfResultsOfQueries()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1,2);
			AddValuesToField(item1.Field2, 1, 4, 5);
			IQuery<SimpleObject> field1Query = new Field1Query().JoinInner(new Field2Query());
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(6, results.Count);
		}

		[Test]
		public void JoinInner_TwoQueriesHaveIdenticalFieldLabelsButDifferentContent_NotJoined()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1);
			AddValuesToField(item1.Field2, 2);
			KeyMap keyMap = new KeyMap { { "Field2", "Field1" } };
			IQuery<SimpleObject> field1Query = new Field1Query().JoinInner(new Field2Query().RemapKeys(keyMap));
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void JoinInner_TwoQueriesHaveFieldsWithIdenticalFieldLabelsAndIdenticalContent_AreJoined()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 2);
			AddValuesToField(item1.Field2, 2);
			KeyMap keyMap = new KeyMap { { "Field2", "Field1" } };
			IQuery<SimpleObject> field1Query = new Field1Query().JoinInner(new Field2Query().RemapKeys(keyMap));
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(1, results.Count);
		}

		[Test]
		public void JoinInner_TwoQueriesHaveSomeFieldsWithIdenticalFieldLabelsAndIdenticalContentAndSomeWithDifferentContent_ThoseWithIdenticalContentAreJoined()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1,2);
			AddValuesToField(item1.Field2, 2);
			KeyMap keyMap = new KeyMap { { "Field2", "Field1" } };
			IQuery<SimpleObject> field1Query = new Field1Query().JoinInner(new Field2Query().RemapKeys(keyMap));
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual(2, results[0]["Field1"]);
		}

		[Test]
		public void JoinInner_OneQueryHasNoResults_ZeroResults()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1, 2);
			IQuery<SimpleObject> field1Query = new Field1Query().JoinInner(new Field2Query().StripAllUnpopulatedEntries());
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void JoinInner_TwoQueriesTwoItems_OnlyResultsFromSameItemsAreJoined()
		{
			SimpleObject item1 = _repo.CreateItem();
			SimpleObject item2 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1);
			AddValuesToField(item1.Field2, 2);
			AddValuesToField(item2.Field1, 3);
			AddValuesToField(item2.Field2, 4);
			IQuery<SimpleObject> field1Query = new Field1Query().JoinInner(new Field2Query());
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual(1, results[0]["Field1"]);
			Assert.AreEqual(2, results[0]["Field2"]);

			Assert.AreEqual(3, results[1]["Field1"]);
			Assert.AreEqual(4, results[1]["Field2"]);
		}

		[Test]
		public void JoinInner_MultipleQueries_AllAreJoined()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1);
			AddValuesToField(item1.Field2, 2,3);
			AddValuesToField(item1.Field3, 3,4,5);
			List<IQuery<SimpleObject>> queriesToJoin = new List<IQuery<SimpleObject>>{new Field2Query(), new Field3Query()};
			IQuery<SimpleObject> field1Query = new Field1Query().JoinInner(queriesToJoin);
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(6, results.Count);
			Assert.AreEqual(1, results[0]["Field1"]);
			Assert.AreEqual(3, results[0]["Field2"]);
			Assert.AreEqual(5, results[0]["Field3"]);
			Assert.AreEqual(1, results[1]["Field1"]);
			Assert.AreEqual(3, results[1]["Field2"]);
			Assert.AreEqual(4, results[1]["Field3"]);
			Assert.AreEqual(1, results[2]["Field1"]);
			Assert.AreEqual(3, results[2]["Field2"]);
			Assert.AreEqual(3, results[2]["Field3"]);
			Assert.AreEqual(1, results[3]["Field1"]);
			Assert.AreEqual(2, results[3]["Field2"]);
			Assert.AreEqual(5, results[3]["Field3"]);
			Assert.AreEqual(1, results[4]["Field1"]);
			Assert.AreEqual(2, results[4]["Field2"]);
			Assert.AreEqual(4, results[4]["Field3"]);
			Assert.AreEqual(1, results[5]["Field1"]);
			Assert.AreEqual(2, results[5]["Field2"]);
			Assert.AreEqual(3, results[5]["Field3"]);
		}

		[Test]
		public void JoinInner_QueriesHaveDifferingSortOrders_RecordTokensAreSortedByFirstQueryPrimarilyAndSecondQuerySecondarily()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1, 4);
			AddValuesToField(item1.Field2, 2, 3, 6);
			IQuery<SimpleObject> field1Query = new Field1Query().JoinInner(new Field2Query());
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(field1Query);
			Assert.AreEqual(1, results[0]["Field1"]);
			Assert.AreEqual(6, results[0]["Field2"]);

			Assert.AreEqual(1, results[1]["Field1"]);
			Assert.AreEqual(3, results[1]["Field2"]);

			Assert.AreEqual(1, results[2]["Field1"]);
			Assert.AreEqual(2, results[2]["Field2"]);

			Assert.AreEqual(4, results[3]["Field1"]);
			Assert.AreEqual(6, results[3]["Field2"]);

			Assert.AreEqual(4, results[4]["Field1"]);
			Assert.AreEqual(3, results[4]["Field2"]);

			Assert.AreEqual(4, results[5]["Field1"]);
			Assert.AreEqual(2, results[5]["Field2"]);
		}

		[Test]
		public void Merge_QueriesReturnDifferentNumberOfResults_ReturnsSumOfFields()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1, 4);
			AddValuesToField(item1.Field2, 1, 3, 6);
			KeyMap keyMap = new KeyMap { { "Field2", "Field1" } };
			IQuery<SimpleObject> mergeQuery = new Field1Query().Merge(new Field2Query().RemapKeys(keyMap));
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(mergeQuery);
			Assert.AreEqual(5, results.Count);
		}

		[Test]
		public void Merge_MultipleItems_ReturnsFieldsForBothItems()
		{
			SimpleObject item1 = _repo.CreateItem();
			SimpleObject item2 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1, 4);
			AddValuesToField(item1.Field2, 1, 3, 6);
			AddValuesToField(item2.Field1, 3, 2);
			AddValuesToField(item2.Field2, 4, 2, 5);
			KeyMap keyMap = new KeyMap { { "Field2", "Field1" } };
			IQuery<SimpleObject> mergeQuery = new Field1Query().Merge(new Field2Query().RemapKeys(keyMap));
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(mergeQuery);
			Assert.AreEqual(10, results.Count);
		}

		[Test]
		public void Merge_QueriesHaveDifferentSortOrders_SortOfFirstQueryWins()
		{
			SimpleObject item1 = _repo.CreateItem();
			SimpleObject item2 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1,4);
			AddValuesToField(item1.Field2, 1,9,6);
			AddValuesToField(item2.Field1, 3,7);
			AddValuesToField(item2.Field2, 4,2,5);
			KeyMap keyMap = new KeyMap { { "Field1", "Field2" } };
			IQuery<SimpleObject> mergeQuery = new Field2Query().Merge(new Field1Query().RemapKeys(keyMap));
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(mergeQuery);
			Assert.AreEqual(9, results[0]["Field2"]);
			Assert.AreEqual(7, results[1]["Field2"]);

			Assert.AreEqual(6, results[2]["Field2"]);
			Assert.AreEqual(5, results[3]["Field2"]);

			Assert.AreEqual(4, results[4]["Field2"]);
			Assert.AreEqual(4, results[5]["Field2"]);

			Assert.AreEqual(3, results[6]["Field2"]);
			Assert.AreEqual(2, results[7]["Field2"]);

			Assert.AreEqual(1, results[8]["Field2"]);
			Assert.AreEqual(1, results[9]["Field2"]);
		}

		[Test]
		public void GetAlternative_NothingInField_ReturnsAlternateField()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field2, 1);
			KeyMap keyMap = new KeyMap { { "Field2", "Field1" } };
			IQuery<SimpleObject> mergeQuery = new Field1Query().GetAlternative(new Field2Query().RemapKeys(keyMap));
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(mergeQuery);
			Assert.AreEqual(1, results[0]["Field1"]);
		}

		[Test]
		public void GetAlternative_NothingInFieldButAlternateNotEmpty_SortsAccordingToFirstQuery()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field2, 1,3);
			KeyMap keyMap = new KeyMap { { "Field2", "Field1" } };
			IQuery<SimpleObject> mergeQuery = new Field1Query().GetAlternative(new Field2Query().RemapKeys(keyMap));
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(mergeQuery);
			Assert.AreEqual(1, results[0]["Field1"]);
			Assert.AreEqual(3, results[1]["Field1"]);
		}

		[Test]
		public void GetAlternative_FirstFieldNotEmpty_ReturnsValuesInField()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1);
			AddValuesToField(item1.Field2, 9);
			KeyMap keyMap = new KeyMap { { "Field2", "Field1" } };
			IQuery<SimpleObject> mergeQuery = new Field1Query().GetAlternative(new Field2Query());
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(mergeQuery);
			Assert.AreEqual(1, results[0]["Field1"]);
		}

		[Test]
		public void GetAlternative_BothFieldsEmpty_ReturnsUnpopulatedResults()
		{
			SimpleObject item1 = _repo.CreateItem();
			KeyMap keyMap = new KeyMap { { "Field2", "Field1" } };
			IQuery<SimpleObject> mergeQuery = new Field1Query().GetAlternative(new Field2Query());
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(mergeQuery);
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual(null, results[0]["Field1"]);
		}

		[Test]
		public void StripUnpopulated_ResultsContainOnlyNullValues_AreStripped()
		{
			SimpleObject item1 = _repo.CreateItem();
			SimpleObject item2 = _repo.CreateItem();
			IQuery<SimpleObject> strippedQuery = new Field1Query().StripAllUnpopulatedEntries();
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(strippedQuery);
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void StripDuplicates_ResultContainDuplicates_AreStripped()
		{
			SimpleObject item1 = _repo.CreateItem();
			AddValuesToField(item1.Field1, 1,1,2);
			AddValuesToField(item1.Field2, 1,3);
			KeyMap keyMap = new KeyMap { { "Field2", "Field1" } };
			IQuery<SimpleObject> strippedQuery = (new Field1Query().Merge(new Field2Query().RemapKeys(keyMap))).StripAllUnpopulatedEntries().StripDuplicates();
			ResultSet<SimpleObject> results = _repo.GetItemsMatching(strippedQuery);
			Assert.AreEqual(3, results.Count);
			Assert.AreEqual(1, results[0]["Field1"]);
			Assert.AreEqual(2, results[1]["Field1"]);
			Assert.AreEqual(3, results[2]["Field1"]);
		}

		private void AddValuesToField(List<int> field, params int[] valuesToAdd)
		{
			foreach (int value in valuesToAdd)
			{
				field.Add(value);
			}
		}

		private class SimpleObject
		{
			public List<int> Field1 = new List<int>();
			public List<int> Field2 = new List<int>();
			public List<int> Field3 = new List<int>();
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
					tokenFieldLabelsAndValues.Add("ObjectHash", item.GetHashCode());
					results.Add(tokenFieldLabelsAndValues);
				}
				if(results.Count == 0)
				{
					Dictionary<string, object> tokenFieldLabelsAndValues = new Dictionary<string, object>();
					tokenFieldLabelsAndValues.Add("Field1", null);
					tokenFieldLabelsAndValues.Add("ObjectHash", item.GetHashCode());
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
								   new SortDefinition("ObjectHash", new GreaterThan())
							   };
				}
			}

			public override string UniqueLabel
			{
				get { return "Field1Query"; }
			}

			public override bool IsUnpopulated(IDictionary<string, object> resultToCheck)
			{
				return resultToCheck["Field1"] == null;
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
					tokenFieldLabelsAndValues.Add("ObjectHash", item.GetHashCode());
					results.Add(tokenFieldLabelsAndValues);
				}
				if (results.Count == 0)
				{
					Dictionary<string, object> tokenFieldLabelsAndValues = new Dictionary<string, object>();
					tokenFieldLabelsAndValues.Add("Field2", null);
					tokenFieldLabelsAndValues.Add("ObjectHash", item.GetHashCode());
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
								   new SortDefinition("ObjectHash", new LessThan())
							   };
				}
			}

			public override string UniqueLabel
			{
				get { return "Field2Query"; }
			}

			public override bool IsUnpopulated(IDictionary<string, object> resultToCheck)
			{
				return resultToCheck["Field2"] == null;
			}
		}

		private class Field3Query : IQuery<SimpleObject>
		{
			public override IEnumerable<IDictionary<string, object>> GetResults(SimpleObject item)
			{
				List<IDictionary<string, object>> results = new List<IDictionary<string, object>>();
				for (int i = 0; i < item.Field3.Count; i++)
				{
					Dictionary<string, object> tokenFieldLabelsAndValues = new Dictionary<string, object>();
					tokenFieldLabelsAndValues.Add("Field3", item.Field3[i]);
					tokenFieldLabelsAndValues.Add("ObjectHash", item.GetHashCode());
					results.Add(tokenFieldLabelsAndValues);
				}
				if (results.Count == 0)
				{
					Dictionary<string, object> tokenFieldLabelsAndValues = new Dictionary<string, object>();
					tokenFieldLabelsAndValues.Add("Field3", null);
					tokenFieldLabelsAndValues.Add("ObjectHash", item.GetHashCode());
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
								   new SortDefinition("Field3", new LessThan())
							   };
				}
			}

			public override string UniqueLabel
			{
				get { return "Field3Query"; }
			}

			public override bool IsUnpopulated(IDictionary<string, object> resultToCheck)
			{
				return resultToCheck["Field3"] == null;
			}
		}
	}
}
