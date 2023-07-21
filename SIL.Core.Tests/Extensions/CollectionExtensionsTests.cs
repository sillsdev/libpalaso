using System;
using System.Collections.Generic;
using NUnit.Framework;
using SIL.Extensions;

namespace SIL.Tests.Extensions
{
	[TestFixture]
	public class CollectionExtensionsTests
	{
		#region IEnumerable<T>.ToString extension method tests
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ToString extension method for a collection of integers with no special
		/// function to convert them to strings and a null separator string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void EnumerableToString_ValueType_NoFuncNullSeparator()
		{
			IEnumerable<int> list = new[] { 5, 6, 2, 3 };
			Assert.AreEqual("5623", list.ToString(null));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ToString extension method for a collection of integers with no special
		/// function to convert them to strings and a specified separator string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void EnumerableToString_ValueType_NoFunc_IgnoreEmpty()
		{
			IEnumerable<int> list = new[] { 5, 0, 2, 3 };
			Assert.AreEqual("5,0,2,3", list.ToString(true, ","));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ToString extension method for a collection of chars with no special
		/// function to convert them to strings and a comma and space as the separator string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void EnumerableToString_NoFuncCommaSeparator()
		{
			IEnumerable<char> list = new[] { '#', 'r', 'p', '3' };
			Assert.AreEqual("#, r, p, 3", list.ToString(", "));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ToString extension method for a collection of strings with a special
		/// function to convert the strings to lowercase and the newline as the separator string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void EnumerableToString_Func()
		{
			IEnumerable<string> list = new[] { "ABC", "XYz", "p", "3w", "ml" };
			Assert.AreEqual("abc" + Environment.NewLine + "xyz" + Environment.NewLine + "p" + Environment.NewLine + "3w" + Environment.NewLine + "ml",
				list.ToString(Environment.NewLine, item => item.ToLowerInvariant()));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ToString extension method for an empty dictionary.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void EnumerableToString_EmptyList()
		{
			Dictionary<string, int> list = new Dictionary<string, int>();
			Assert.AreEqual(string.Empty, list.ToString(Environment.NewLine, item => item.Key));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ToString extension method for a collection of strings that has nulls
		/// and empty strings which must be excluded, with a special function to convert the
		/// strings to lowercase and a space-padded ampersand as the separator string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void EnumerableToString_ExcludeNullAndEmptyItems()
		{
			IEnumerable<string> list = new[] { "ABC", null, "p", string.Empty };
			Assert.AreEqual("abc & p", list.ToString(" & ", item => item.ToLowerInvariant()));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ToString extension method for a collection of strings that has nulls
		/// and empty strings whose positions must be preserved in the list, with a special
		/// function to convert the strings to lowercase and a space-padded ampersand as the
		/// separator string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void EnumerableToString_IncludeNullAndEmptyItems()
		{
			IEnumerable<string> list = new[] { string.Empty, "ABC", null, "p", string.Empty };
			Assert.AreEqual("|ABC||p|", list.ToString(false, "|"));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ToString extension method for a collection of strings that has nulls
		/// and empty strings whose positions must be preserved in the list, with a special
		/// function to convert the strings to lowercase and a space-padded ampersand as the
		/// separator string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void EnumerableToString_IgnoreNullAndEmptyItems()
		{
			IEnumerable<string> list = new[] { string.Empty, "ABC", null, "p", string.Empty };
			Assert.AreEqual("ABC|p", list.ToString(true, "|"));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ToString extension method for a collection of objects of both class and
		/// value types that includes nulls and empty strings, with no special function to
		/// convert them to strings and a space-padded ampersand as the separator string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void EnumerableToString_ObjectsOfValueAndClassTypes()
		{
			IEnumerable<object> list = new object[] { "ABC", null, 0, string.Empty, 'r' };
			Assert.AreEqual("ABC & 0 & r", list.ToString(" & "));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ToString extension method for a collection of objects of both class and
		/// value types that includes nulls and empty strings, with a method that adds the
		/// strings plus the accumulated length of the builder and a comma as the separator
		/// string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void EnumerableToString_FuncThatTakesStrBuilder()
		{
			IEnumerable<string> list = new[] { "A", "BC", "XYZ" };
			const string kSep = ",";
			int kcchSep = kSep.Length;
			Assert.AreEqual("A1,BC5,XYZ10", list.ToString(kSep, (item, bldr) =>
			{
				int cch = bldr.Length > 0 ? kcchSep : 0;
				bldr.Append(item);
				bldr.Append(bldr.Length + cch);
			}));
		}
		#endregion

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

		#region ContainsSequence extension method tests
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ContainsSequence extension method when the super-sequence contains the
		/// sub-sequence at the beginning.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ContainsSequence_AtBeginning()
		{
			Assert.IsTrue(new List<int>(new [] {1, 23, 4}).ContainsSequence(new List<int>(new [] {1, 23})));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ContainsSequence extension method when the super-sequence contains the
		/// sub-sequence at the end.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ContainsSequence_AtEnd()
		{
			Assert.IsTrue(new List<int>(new[] { 1, 2, 3, 5 }).ContainsSequence(new List<int>(new[] { 2, 3, 5 })));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ContainsSequence extension method when the super-sequence contains the
		/// sub-sequence in the middle.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ContainsSequence_InMiddle()
		{
			Assert.IsTrue(new List<object>(new object[] { 34, "A", "C", true, "34" }).ContainsSequence(
				new List<object>(new object[] { "A", "C", true })));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ContainsSequence extension method when list 1 is shorter than list 2.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ContainsSequence_2LongerThan1()
		{
			Assert.IsFalse(new List<object>(new object[] { 34 }).ContainsSequence(new List<object>(new object[] { 34, true })));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ContainsSequence extension method when list 1 and list 2 are the same
		/// length and end the same but begin differently.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ContainsSequence_DifferenceAtBeginning()
		{
			Assert.IsFalse(new List<object>(new object[] { 56, false, "A", 34 }).ContainsSequence(
				new List<object>(new object[] { 34, true, "A", 34 })));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ContainsSequence extension method when list 1 is empty.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ContainsSequence_List1Empty()
		{
			Assert.IsFalse(new List<object>().ContainsSequence(new List<object>(new object[] { 34, true, "A", 34 })));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ContainsSequence extension method when list 2 is empty (in which case we
		/// expect true).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ContainsSequence_TrueIfList2IsEmpty()
		{
			Assert.IsTrue(new List<object>(new object[] { 34, true, "A", 34 }).ContainsSequence(new List<object>()));
			Assert.IsTrue(new List<object>().ContainsSequence(new List<object>()));
		}
		#endregion


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
