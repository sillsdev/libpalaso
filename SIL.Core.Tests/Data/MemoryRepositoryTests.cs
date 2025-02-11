using NUnit.Framework;
using SIL.Data;

namespace SIL.Tests.Data
{
	[TestFixture]
	public class MemoryRepositoryStateUninitializedTests: IRepositoryStateUninitializedTests<PalasoTestItem>
	{
		[SetUp]
		public override void SetUp()
		{
			DataMapperUnderTest = new MemoryDataMapper<PalasoTestItem>();
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
		}
	}

	[TestFixture]
	public class MemoryRepositoryCreateItemTransitionTests:
		IRepositoryCreateItemTransitionTests<PalasoTestItem>
	{
		[SetUp]
		public override void SetUp()
		{
			DataMapperUnderTest = new MemoryDataMapper<PalasoTestItem>();
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
		}

		/* todo move to query tests
		[Test]
		protected override void GetItemsMatchingQuery_QueryWithShow_ReturnAllItemsMatchingQuery_v()
		{
			Item.StoredInt = 123;
			Item.StoredString = "I was stored!";
			QueryAdapter<PalasoTestItem> query = new QueryAdapter<PalasoTestItem>();
			query.Show("StoredInt").Show("StoredString");
			ResultSet<PalasoTestItem> resultsOfQuery = DataMapperUnderTest.GetItemsMatching(query);
			Assert.AreEqual(1, resultsOfQuery.Count);
			Assert.AreEqual(123, resultsOfQuery[0]["StoredInt"]);
			Assert.AreEqual("I was stored!", resultsOfQuery[0]["StoredString"]);
		}
		*/
		protected override void CreateNewRepositoryFromPersistedData()
		{
			//Do nothing.
		}
	}

	[TestFixture]
	public class MemoryRepositoryDeleteItemTransitionTests:
		IRepositoryDeleteItemTransitionTests<PalasoTestItem>
	{
		[SetUp]
		public override void SetUp()
		{
			DataMapperUnderTest = new MemoryDataMapper<PalasoTestItem>();
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			//Do nothing.
		}
	}

	[TestFixture]
	public class MemoryRepositoryDeleteIdTransitionTests:
		IRepositoryDeleteIdTransitionTests<PalasoTestItem>
	{
		[SetUp]
		public override void SetUp()
		{
			DataMapperUnderTest = new MemoryDataMapper<PalasoTestItem>();
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			//Do nothing.
		}
	}

	[TestFixture]
	public class MemoryRepositoryDeleteAllItemsTransitionTests:
		IRepositoryDeleteAllItemsTransitionTests<PalasoTestItem>
	{
		[SetUp]
		public override void SetUp()
		{
			DataMapperUnderTest = new MemoryDataMapper<PalasoTestItem>();
		}

		[TearDown]
		public override void TearDown()
		{
			DataMapperUnderTest.Dispose();
		}

		protected override void RepopulateRepositoryFromPersistedData()
		{
			//Do nothing.
		}
	}
}