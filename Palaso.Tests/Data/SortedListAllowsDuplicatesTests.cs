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
	public class SortedListAllowsDuplicatesTests
	{

		[Test]
		public void Add_ListIsEmpty_ItemIsAddedToEnd()
		{
			SortedListAllowsDuplicates<string> sortedList= new SortedListAllowsDuplicates<string>(new StringLengthComparer());
			sortedList.Add("bb");
			Assert.AreEqual("bb", sortedList[0]);
		}

		[Test]
		public void Add_ListHasOneElementAndNewGreaterElementIsAdded_ItemIsAddedAfter()
		{
			SortedListAllowsDuplicates<string> sortedList= new SortedListAllowsDuplicates<string>(new StringLengthComparer());
			sortedList.Add("bb");
			sortedList.Add("aaa");
			Assert.AreEqual("bb", sortedList[0]);
			Assert.AreEqual("aaa", sortedList[1]);
		}

		[Test]
		public void Add_ListHasOneElementAndNewLesserElementIsAdded_ItemIsAddedBefore()
		{
			SortedListAllowsDuplicates<string> sortedList= new SortedListAllowsDuplicates<string>(new StringLengthComparer());
			sortedList.Add("bb");
			sortedList.Add("c");
			Assert.AreEqual("c", sortedList[0]);
			Assert.AreEqual("bb", sortedList[1]);
		}

		[Test]
		public void Add_ListHasTwoElementsAndNewLesserGreaterElementIsAdded_ItemIsAddedInBetween()
		{
			SortedListAllowsDuplicates<string> sortedList= new SortedListAllowsDuplicates<string>(new StringLengthComparer());
			sortedList.Add("c");
			sortedList.Add("aaa");
			sortedList.Add("bb");
			Assert.AreEqual("c", sortedList[0]);
			Assert.AreEqual("bb", sortedList[1]);
			Assert.AreEqual("aaa", sortedList[2]);
		}

		[Test]
		public void Add_ListHasThreeElementsAndNewDuplicateElementIsAdded_ItemIsAddedAdjacentToDuplicate()
		{
			SortedListAllowsDuplicates<string> sortedList= new SortedListAllowsDuplicates<string>(new StringLengthComparer());
			sortedList.Add("c");
			sortedList.Add("aaa");
			sortedList.Add("bb");
			sortedList.Add("bb");
			Assert.AreEqual("c", sortedList[0]);
			Assert.AreEqual("bb", sortedList[1]);
			Assert.AreEqual("bb", sortedList[2]);
			Assert.AreEqual("aaa", sortedList[3]);
		}

		private class StringLengthComparer:IComparer<string>
		{
			public int Compare(string x, string y)
			{
				if (x.Length < y.Length) return -1;
				if (x.Length > y.Length) return 1;
				else return 0;
			}
		}
	}
}
