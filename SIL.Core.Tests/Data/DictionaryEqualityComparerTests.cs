using System;
using System.Collections.Generic;
using NUnit.Framework;
using SIL.Data;

namespace SIL.Tests.Data
{
	[TestFixture]
	public class DictionaryEqualityComparerTests
	{
		private DictionaryEqualityComparer<string, string> _comparer;
		private Dictionary<string, string> _x;
		private Dictionary<string, string> _y;

		[SetUp]
		public void SetUp()
		{
			_comparer = new DictionaryEqualityComparer<string, string>();
			_x = new Dictionary<string, string>();
			_y = new Dictionary<string, string>();
		}

		[Test]
		public void Equals_xNullyNotNull_false()
		{
			_x = null;
			_y.Add("0", "Zero");
			Assert.IsFalse(_comparer.Equals(_x, _y));
		}

		[Test]
		public void Equals_xNotNullyNull_false()
		{
			_x.Add("0", "Zero");
			_y = null;
			Assert.IsFalse(_comparer.Equals(_x, _y));
		}

		[Test]
		public void Equals_xMoreEntriesThany_false()
		{
			_x.Add("0", "Zero");
			Assert.IsFalse(_comparer.Equals(_x, _y));
		}

		[Test]
		public void Equals_xFewerEntriesThany_false()
		{
			_y.Add("0", "Zero");
			Assert.IsFalse(_comparer.Equals(_x, _y));
		}

		[Test]
		public void Equals_xKeyIsDifferentThany_false()
		{
			_x.Add("0", "Zero");
			_y.Add("1", "Zero");
			Assert.IsFalse(_comparer.Equals(_x, _y));
		}

		[Test]
		public void Equals_xValueIsDifferentThany_false()
		{
			_x.Add("0", "Zero");
			_y.Add("0", "One");
			Assert.IsFalse(_comparer.Equals(_x, _y));
		}

		[Test]
		public void Equals_xHasSameLengthKeysAndValuesAsy_true()
		{
			_x.Add("0", "Zero");
			_y.Add("0", "Zero");
			Assert.IsTrue(_comparer.Equals(_x, _y));
		}

		[Test]
		public void Equals_xAndyAreBothEmpty_true()
		{
			Assert.IsTrue(_comparer.Equals(_x, _y));
		}

		[Test]
		public void Equals_xIsNullyIsNull_true()
		{
			_x = null;
			_y = null;
			Assert.IsTrue(_comparer.Equals(_x, _y));

		}

		[Test]
		public void GetHashCode_Null_Throws()
		{
			var comparer = new DictionaryEqualityComparer<string, string>();
			Assert.Throws<ArgumentNullException>(
				() => comparer.GetHashCode(null));
		}

		[Test]
		public void GetHashCode_TwoDictionariesAreEqual_ReturnSameHashCodes()
		{
			var reference = new Dictionary<string, string> {{"key1", "value1"}, {"key2", "value2"}};
			var other = new Dictionary<string, string> {{"key1", "value1"}, {"key2", "value2"}};
			var comparer = new DictionaryEqualityComparer<string, string>();
			Assert.AreEqual(comparer.GetHashCode(reference), comparer.GetHashCode(other));
		}

		[Test]
		public void GetHashCode_TwoDictionariesHaveDifferentLength_ReturnDifferentHashCodes()
		{
			var reference = new Dictionary<string, string> {{"key1", "value1"}, {"key2", "value2"}};
			var other = new Dictionary<string, string> {{"key1", "value1"}};
			var comparer = new DictionaryEqualityComparer<string, string>();
			Assert.AreNotEqual(comparer.GetHashCode(reference), comparer.GetHashCode(other));
		}

		[Test]
		public void GetHashCode_TwoDictionariesHaveDifferentKey_ReturnDifferentHashCodes()
		{
			var reference = new Dictionary<string, string> {{"key1", "value1"}, {"key2", "value2"}};
			var other = new Dictionary<string, string> {{"key1", "value1"}, {"key3", "value2"}};
			var comparer = new DictionaryEqualityComparer<string, string>();
			Assert.AreNotEqual(comparer.GetHashCode(reference), comparer.GetHashCode(other));
		}

		[Test]
		public void GetHashCode_TwoDictionariesHaveDifferentValue_ReturnDifferentHashCodes()
		{
			var reference = new Dictionary<string, string> {{"key1", "value1"}, {"key2", "value2"}};
			var other = new Dictionary<string, string> {{"key1", "value1"}, {"key2", "value3"}};
			var comparer = new DictionaryEqualityComparer<string, string>();
			Assert.AreNotEqual(comparer.GetHashCode(reference), comparer.GetHashCode(other));
		}
	}
}