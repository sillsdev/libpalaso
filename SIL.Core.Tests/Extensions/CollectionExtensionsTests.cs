using System;
using System.Collections.Generic;
using System.Linq;
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
		public void Sort_UsingOrdinalIComparer()
		{
			IList<string> list = GetUnsortedList();
			list.Sort(new MyStringComparer());
			Assert.AreEqual(4, list.Count);
			Assert.AreEqual("A", list[0]);
			Assert.AreEqual("B", list[1]);
			Assert.AreEqual("C", list[2]);
			Assert.AreEqual("D", list[3]);
		}

		[Test]
		public void Sort_UsingPointOfArticulationIComparer()
		{
			IList<string> list = GetUnsortedList();
			list.Sort(new EnglishPlaceOfArticulationComparer());
			Assert.AreEqual(4, list.Count);
			Assert.AreEqual("B", list[0]);
			Assert.AreEqual("D", list[1]);
			Assert.AreEqual("C", list[2]);
			Assert.AreEqual("A", list[3]);
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

		[Test]
		public void KeyedSetsEqual_SimpleType()
		{
			Assert.IsTrue(new[] { 1, 5, 9, 10, 27 }.KeyedSetsEqual(new[] { 1, 5, 9, 10, 27 }));
			Assert.IsTrue(new[] { 1, 5, 9, 10, 27 }.KeyedSetsEqual(new[] { 27, 1, 10, 9, 5 }));
			Assert.IsTrue(new int[0].KeyedSetsEqual(new int[0]));

			Assert.IsFalse(new[] { 1, 5, 9, 10, 27 }.KeyedSetsEqual(new[] { 27, 10, 9, 5 }));
			Assert.IsFalse(new int[0].KeyedSetsEqual(new[] { 27, 1 }));
			Assert.IsFalse(new[] { 27, 1 }.KeyedSetsEqual(new int[0]));
		}

		[Test]
		public void KeyedSetsEqual_KeyValuePair()
		{
			Assert.IsTrue(new[] { new KeyValuePair<string, string>("here", "there"), new KeyValuePair<string, string>("john", "smith") }
				.KeyedSetsEqual(new[] { new KeyValuePair<string, string>("john", "smith"), new KeyValuePair<string, string>("here", "there") }));

			Assert.IsTrue(new[] { new KeyValuePair<TstCmp, TstCmp>(new TstCmp(1, 2), new TstCmp(3, 5)),
				new KeyValuePair<TstCmp, TstCmp>(new TstCmp(8, 9), new TstCmp(6, 7)) }
				.KeyedSetsEqual(new[] { new KeyValuePair<TstCmp, TstCmp>(new TstCmp(8, 9), new TstCmp(6, 7)),
					new KeyValuePair<TstCmp, TstCmp>(new TstCmp(1, 2), new TstCmp(3, 5)) }));

			Assert.IsFalse(new[] { new KeyValuePair<string, string>("here", "there"), new KeyValuePair<string, string>("john", "smith") }
				.KeyedSetsEqual(new[] { new KeyValuePair<string, string>("smith", "john"), new KeyValuePair<string, string>("there", "here") }));

			Assert.IsFalse(new[] { new KeyValuePair<TstCmp, TstCmp>(new TstCmp(1, 2), new TstCmp(3, 5)),
				new KeyValuePair<TstCmp, TstCmp>(new TstCmp(8, 9), new TstCmp(6, 7)) }
				.KeyedSetsEqual(new[] { new KeyValuePair<TstCmp, TstCmp>(new TstCmp(7, 0), new TstCmp(6, 7)),
					new KeyValuePair<TstCmp, TstCmp>(new TstCmp(1, 2), new TstCmp(3, 5)) }));
		}

		[Test]
		public void KeyedSetsEqual_UsingDictionary()
		{
			Assert.IsTrue(new Dictionary<string, string> { { "here", "there" }, { "john", "smith" } }
				.KeyedSetsEqual(new Dictionary<string, string> { { "john", "smith" }, { "here", "there" } }));

			Assert.IsTrue(new Dictionary<TstCmp, TstCmp> { { new TstCmp(1, 2), new TstCmp(3, 5) }, { new TstCmp(8, 9), new TstCmp(6, 7) } }
				.KeyedSetsEqual(new Dictionary<TstCmp, TstCmp> { { new TstCmp(8, 9), new TstCmp(6, 7) }, { new TstCmp(1, 2), new TstCmp(3, 5) } }));

			Assert.IsFalse(new Dictionary<string, string> { { "here", "there" }, { "john", "smith" } }
				.KeyedSetsEqual(new Dictionary<string, string> { { "smith", "john" }, { "there", "here" } }));

			Assert.IsFalse(new Dictionary<string, string> { { "here", "there" }, { "john", "smith" } }
				.KeyedSetsEqual(new Dictionary<string, string> { { "john", "jones" }, { "here", "stay" } }));

			Assert.IsFalse(new Dictionary<TstCmp, TstCmp> { { new TstCmp(1, 2), new TstCmp(3, 5) }, { new TstCmp(8, 9), new TstCmp(6, 7) } }
				.KeyedSetsEqual(new Dictionary<TstCmp, TstCmp> { { new TstCmp(7, 0), new TstCmp(6, 7) }, { new TstCmp(1, 2), new TstCmp(3, 5) } }));
		}

		[TestCase(0)]
		[TestCase(63)]
		public void FirstOrDefault_ItemFound_SameAsLinqFirstOrDefault(int defVal)
		{
			var enumeration = new[] {1, 2, 3};
			Func<int, bool> predicate = i => i % 2 == 0;

			Assert.AreEqual(enumeration.FirstOrDefault(predicate), enumeration.FirstOrDefault(predicate, defVal));
		}

		[Test]
		public void FirstOrDefault_ItemNotFoundDefaultDefVal_SameAsLinqFirstOrDefault()
		{
			var enumeration = new[] {1, 2, 3};
			Func<int, bool> predicate = i => i > 100;

			Assert.AreEqual(enumeration.FirstOrDefault(predicate), enumeration.FirstOrDefault(predicate, 0));
		}

		[TestCase(-1)]
		[TestCase(100)]
		public void FirstOrDefault_ItemNotFound_ReturnsDefaultVal(int defVal)
		{
			var enumeration = new[] {1, 2, 3};
			Func<int, bool> predicate = i => i > 100;

			Assert.AreEqual(defVal, enumeration.FirstOrDefault(predicate, defVal));
		}

		#region TstCmp class
		private sealed class TstCmp
		{
			private readonly int V1;
			private readonly int V2;

			public TstCmp(int v1, int v2)
			{
				V1 = v1;
				V2 = v2;
			}

			public override int GetHashCode()
			{
				return V1 ^ V2;
			}

			public override bool Equals(object obj)
			{
				TstCmp other = obj as TstCmp;
				return other != null && other.V1 == V1 && other.V2 == V2;
			}
		}
		#endregion
	}
}
