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
	}
}
