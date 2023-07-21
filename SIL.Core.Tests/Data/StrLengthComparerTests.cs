using NUnit.Framework;
using SIL.Data;

namespace SIL.Tests.Data
{
	[TestFixture]
	public class StrLengthComparerTests
	{
		[TestCase("abc", "xyz", true)]
		[TestCase("abcd", "abcd", false)]
		[TestCase("", "", true)]
		[TestCase("", "", false)]
		[TestCase(null, null, true)]
		[TestCase(null, null, false)]
		public void Compare_EqualLengthStrings_ReturnsZero(string x, string y, bool asc)
		{
			var comparer = new StrLengthComparer(asc);
			Assert.AreEqual(comparer.Compare(x, y), 0);
		}

		[TestCase("abcd", "xyz")]
		[TestCase("abc", "bc")]
		[TestCase("a", "")]
		[TestCase("a", null)] // See https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icomparer-1.compare?view=net-7.0#remarks
		[TestCase("", null)]
		public void AscendingComparer_Compare_XLongerThanY_ReturnsGreaterThanZero(string x, string y)
		{
			var comparer = new StrLengthComparer();
			Assert.That(comparer.Compare(x, y), Is.GreaterThan(0));
			comparer = new StrLengthComparer(true);
			Assert.That(comparer.Compare(x, y), Is.GreaterThan(0));
		}

		[TestCase("abcd", "xyz")]
		[TestCase("abc", "bc")]
		[TestCase("a", "")]
		[TestCase("a", null)] // See https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icomparer-1.compare?view=net-7.0#remarks
		[TestCase("", null)]
		public void AscendingComparer_Compare_XShorterThanY_ReturnsLessThanZero(string y, string x)
		{
			var comparer = new StrLengthComparer();
			Assert.That(comparer.Compare(x, y), Is.LessThan(0));
			comparer = new StrLengthComparer(true);
			Assert.That(comparer.Compare(x, y), Is.LessThan(0));
		}

		[TestCase("abcd", "xyz")]
		[TestCase("abc", "bc")]
		[TestCase("a", "")]
		[TestCase("a", null)] // See https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icomparer-1.compare?view=net-7.0#remarks
		[TestCase("", null)]
		public void Descending_Compare_XLongerThanY_ReturnsLessThanZero(string x, string y)
		{
			var comparer = new StrLengthComparer(false);
			Assert.That(comparer.Compare(x, y), Is.LessThan(0));
		}

		[TestCase("abcd", "xyz")]
		[TestCase("abc", "bc")]
		[TestCase("a", "")]
		[TestCase("a", null)] // See https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icomparer-1.compare?view=net-7.0#remarks
		[TestCase("", null)]
		public void Descending_Compare_XShorterThanY_ReturnsGreaterThanZero(string y, string x)
		{
			var comparer = new StrLengthComparer(false);
			Assert.That(comparer.Compare(x, y), Is.GreaterThan(0));
		}
	}
}