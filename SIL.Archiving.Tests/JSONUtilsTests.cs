using System.Collections.Generic;
using NUnit.Framework;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	public class JSONUtilsTests
	{
		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeKeyValuePair_BothAreEmtpy_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeKeyValuePair("", ""));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeKeyValuePair_KeyIsNull_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeKeyValuePair(null, "blah"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeKeyValuePair_ValueIsNull_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeKeyValuePair("blah", null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeKeyValuePair_KeyAndValueAreGood_ReturnsCorrectString()
		{
			Assert.AreEqual("\"key\":\"value\"", JSONUtils.MakeKeyValuePair("key", "value"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeKeyValuePair_BracketValue_ReturnsBracketedValue()
		{
			Assert.AreEqual("\"key\":[\"value\"]", JSONUtils.MakeKeyValuePair("key", "value", true));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeArrayFromValues_GoodValues_MakesArray()
		{
			var list = new List<string>();
			list.Add(JSONUtils.MakeKeyValuePair("corvette", "car"));
			list.Add(JSONUtils.MakeKeyValuePair("banana", "fruit"));
			list.Add(JSONUtils.MakeKeyValuePair("spot", "dog"));

			Assert.AreEqual("\"stuff\":{\"0\":{\"corvette\":\"car\"},\"1\":{\"banana\":\"fruit\"},\"2\":{\"spot\":\"dog\"}}",
				JSONUtils.MakeArrayFromValues("stuff", list));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeBracketedListFromValues_NullKey_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeBracketedListFromValues(null, new[] { "blah" }));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeBracketedListFromValues_EmptyKey_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeBracketedListFromValues(string.Empty, new[] { "blah" }));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeBracketedListFromValues_NullList_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeBracketedListFromValues("blah", null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeBracketedListFromValues_EmptyList_ReturnsNull()
		{
			Assert.IsNull(JSONUtils.MakeBracketedListFromValues("blah", new string[] { }));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeBracketedListFromValues_GoodKeyAndList_ReturnsBracketedList()
		{
			Assert.AreEqual("\"key\":[\"red\",\"green\",\"blue\"]",
				JSONUtils.MakeBracketedListFromValues("key", new[] { "red", "green", "blue" }));
		}
	}
}
