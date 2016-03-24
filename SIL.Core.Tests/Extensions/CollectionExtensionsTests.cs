using System;
using System.Collections.Generic;
using NUnit.Framework;
using SIL.Extensions;

namespace SIL.Tests.Extensions
{
	[TestFixture]
	public class CollectionExtensionsTests
	{
		[Test]
		public void Sort_UsingComparison()
		{
			IList<string> list = GetUnsortedList();
			list.Sort((s, s1) => String.Compare(s, s1, StringComparison.Ordinal));
			Assert.AreEqual(4, list.Count);
			Assert.AreEqual("A", list[0]);
			Assert.AreEqual("B", list[1]);
			Assert.AreEqual("C", list[2]);
			Assert.AreEqual("D", list[3]);
		}

		[Test]
		public void Sort_UsingIComparer()
		{
			IList<string> list = GetUnsortedList();
			list.Sort(new MyStringComparer());
			Assert.AreEqual(4, list.Count);
			Assert.AreEqual("A", list[0]);
			Assert.AreEqual("B", list[1]);
			Assert.AreEqual("C", list[2]);
			Assert.AreEqual("D", list[3]);
		}

		private IList<string> GetUnsortedList()
		{
			return new List<string> { "B", "D", "C", "A" };
		}

		private class MyStringComparer : IComparer<string>
		{
			public int Compare(string x, string y)
			{
				return string.Compare(x, y, StringComparison.Ordinal);
			}
		}

		[Test]
		public void SequenceCompare_SameItems_ReturnsZero()
		{
			string[] array1 = {"A", "B", "C", "D"};
			string[] array2 = {"A", "B", "C", "D"};
			Assert.That(array1.SequenceCompare(array2), Is.EqualTo(0));
		}

		[Test]
		public void SequenceCompare_GreaterThan_ReturnsOne()
		{
			string[] array1 = {"A", "B", "C", "E"};
			string[] array2 = {"A", "B", "C", "D"};
			Assert.That(array1.SequenceCompare(array2), Is.EqualTo(1));
		}

		[Test]
		public void SequenceCompare_LessThan_ReturnsNegativeOne()
		{
			string[] array1 = {"A", "B", "C", "D"};
			string[] array2 = {"A", "B", "D", "D"};
			Assert.That(array1.SequenceCompare(array2), Is.EqualTo(-1));
		}

		[Test]
		public void SequenceCompare_CountNotEqual_ReturnsNegativeOne()
		{
			string[] array1 = {"A", "B", "C"};
			string[] array2 = {"A", "B", "C", "D"};
			Assert.That(array1.SequenceCompare(array2), Is.EqualTo(-1));
		}
	}
}
