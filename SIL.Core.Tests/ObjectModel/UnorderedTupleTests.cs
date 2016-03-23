using NUnit.Framework;
using SIL.ObjectModel;

namespace SIL.Tests.ObjectModel
{
	[TestFixture]
	public class UnorderedTupleTests
	{
		[Test]
		public void Equals_SameTuples_ReturnsTrue()
		{
			var tuple1 = UnorderedTuple.Create("A", "B", "C");
			var tuple2 = UnorderedTuple.Create("A", "B", "C");
			Assert.That(tuple1.Equals(tuple2), Is.True);
		}

		[Test]
		public void Equals_SameTuplesInDifferentOrder_ReturnsTrue()
		{
			var tuple1 = UnorderedTuple.Create("A", "B", "C");
			var tuple2 = UnorderedTuple.Create("C", "A", "B");
			Assert.That(tuple1.Equals(tuple2), Is.True);
		}

		[Test]
		public void Equals_DifferentTuples_ReturnsFalse()
		{
			var tuple1 = UnorderedTuple.Create("A", "B", "D");
			var tuple2 = UnorderedTuple.Create("A", "B", "C");
			Assert.That(tuple1.Equals(tuple2), Is.False);
		}

		[Test]
		public void Equals_SameTuplesDifferentTypesInDifferentOrder_ReturnsTrue()
		{
			var tuple1 = UnorderedTuple.Create("A", "B", 0);
			var tuple2 = UnorderedTuple.Create("B", "A", 0);
			Assert.That(tuple1.Equals(tuple2), Is.True);
		}

		[Test]
		public void GetHashCode_SameTuples_HashCodesEqual()
		{
			var tuple1 = UnorderedTuple.Create("A", "B", "C");
			var tuple2 = UnorderedTuple.Create("A", "B", "C");
			Assert.That(tuple1.GetHashCode(), Is.EqualTo(tuple2.GetHashCode()));
		}

		[Test]
		public void GetHashCode_SameTuplesInDifferentOrder_HashCodesEqual()
		{
			var tuple1 = UnorderedTuple.Create("A", "B", "C");
			var tuple2 = UnorderedTuple.Create("C", "A", "B");
			Assert.That(tuple1.GetHashCode(), Is.EqualTo(tuple2.GetHashCode()));
		}

		[Test]
		public void GetHashCode_DifferentTuples_HashCodesNotEqual()
		{
			var tuple1 = UnorderedTuple.Create("A", "B", "D");
			var tuple2 = UnorderedTuple.Create("A", "B", "C");
			Assert.That(tuple1.GetHashCode(), Is.Not.EqualTo(tuple2.GetHashCode()));
		}

		[Test]
		public void GetHashCode_SameTuplesDifferentTypesInDifferentOrder_HashCodesEqual()
		{
			var tuple1 = UnorderedTuple.Create("A", "B", 0);
			var tuple2 = UnorderedTuple.Create("B", "A", 0);
			Assert.That(tuple1.GetHashCode(), Is.EqualTo(tuple2.GetHashCode()));
		}
	}
}
