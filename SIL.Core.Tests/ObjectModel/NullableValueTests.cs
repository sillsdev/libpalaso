using NUnit.Framework;
using SIL.ObjectModel;

namespace SIL.Tests.ObjectModel
{
	[TestFixture]
	public class NullableValueTests
	{
		[Test]
		public void Equals_SameValues_ReturnsTrue()
		{
			var value1 = new NullableValue<int>(0);
			var value2 = new NullableValue<int>(0);
			Assert.That(value1.Equals(value2), Is.True);
		}

		[Test]
		public void Equals_DifferentValues_ReturnsFalse()
		{
			var value1 = new NullableValue<int>(0);
			var value2 = new NullableValue<int>(1);
			Assert.That(value1.Equals(value2), Is.False);
		}

		[Test]
		public void Equals_DifferentValuesOneIsNull_ReturnsFalse()
		{
			var value1 = new NullableValue<int>(0);
			var value2 = new NullableValue<int>();
			Assert.That(value1.Equals(value2), Is.False);
		}

		[Test]
		public void Equals_BothNull_ReturnsTrue()
		{
			var value1 = new NullableValue<int>();
			var value2 = new NullableValue<int>();
			Assert.That(value1.Equals(value2), Is.True);
		}

		[Test]
		public void GetHashCode_SameValues_HashCodesEqual()
		{
			var value1 = new NullableValue<int>(0);
			var value2 = new NullableValue<int>(0);
			Assert.That(value1.GetHashCode(), Is.EqualTo(value2.GetHashCode()));
		}

		[Test]
		public void GetHashCode_DifferentValues_HashCodesNotEqual()
		{
			var value1 = new NullableValue<int>(0);
			var value2 = new NullableValue<int>(1);
			Assert.That(value1.GetHashCode(), Is.Not.EqualTo(value2.GetHashCode()));
		}

		[Test]
		public void GetHashCode_DifferentValuesOneIsNull_HashCodesNotEqual()
		{
			var value1 = new NullableValue<int>();
			var value2 = new NullableValue<int>(1);
			Assert.That(value1.GetHashCode(), Is.Not.EqualTo(value2.GetHashCode()));
		}

		[Test]
		public void GetHashCode_BothNull_HashCodesEqual()
		{
			var value1 = new NullableValue<int>();
			var value2 = new NullableValue<int>();
			Assert.That(value1.GetHashCode(), Is.EqualTo(value2.GetHashCode()));
		}
	}
}
