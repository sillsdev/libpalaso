// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2014, SIL International.   
// <copyright from='2002' to='2014' company='SIL International'>
//		Copyright (c) 2014, SIL International.   
//    
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright> 
#endregion
// 
// File: MultilingScrBooksTest.cs
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SIL.Scripture.Tests
{
	/// <summary>
	/// Test the <see cref="MultilingScrBooks"/> class.
	/// </summary>
	[TestFixture]
	public class MultilingScrBooksTest
	{
		private MultilingScrBooks m_mlscrBook;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Setups this instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void TestSetup()
		{
			m_mlscrBook = new MultilingScrBooks();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the <see cref="MultilingScrBooks.GetBookAbbrev"/> method. Test with English
		/// (default) encoding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(1, ExpectedResult = "Gen")]
		[TestCase(40, ExpectedResult = "Mat")]
		[TestCase(66, ExpectedResult = "Rev")]
		public string GetBookAbbrev(int bookNum)
		{
			return m_mlscrBook.GetBookAbbrev(bookNum);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the <see cref="MultilingScrBooks.GetBookAbbrev"/> method. Test with 
		/// non-existing encoding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase(1, ExpectedResult = "GEN")]
		[TestCase(40, ExpectedResult = "MAT")]
		[TestCase(66, ExpectedResult = "REV")]
		public string GetBookAbbrevDifferentEncoding(int bookNum)
		{
			List<string> array = new List<string>();
			array.Add("99"); // some arbitrary should-never-exist value
			m_mlscrBook.RequestedEncodings = array;

			return m_mlscrBook.GetBookAbbrev(bookNum);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ValidateReferencesWithoutDB()
		{
			Assert.AreEqual("Genesis".ToLower(), m_mlscrBook.GetBookName(1).ToLower(),
				"Genesis not found");
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase("jud", ExpectedResult = 65001001)]
		[TestCase("ju", ExpectedResult = 7001001)]
		[TestCase("JUD", ExpectedResult = 65001001)]
		[TestCase("jU", ExpectedResult = 7001001)]
		public int ParseRefString(string input)
		{
			return m_mlscrBook.ParseRefString(input).BBCCCVVV;
		}

		[Test]
		public void ParseRefString()
		{
			Assert.AreEqual(65001001, m_mlscrBook.ParseRefString("Ju", 34).BBCCCVVV);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests parsing the full English name
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ParseRefString_FullNameEnglish()
		{
			BCVRef judeRef = m_mlscrBook.ParseRefString("Jude 99:1");
			Assert.AreEqual(65099001, judeRef.BBCCCVVV);
            Assert.IsTrue(judeRef.Valid);
			BCVRef judgesRef = m_mlscrBook.ParseRefString("Judges 3:6");
			Assert.AreEqual(7003006, judgesRef.BBCCCVVV);
			Assert.IsTrue(judgesRef.Valid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests parsing the English abbreviation.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ParseRefString_AbbrEnglish()
		{
			BCVRef timRef = m_mlscrBook.ParseRefString("2Tim 1:5");
			Assert.AreEqual(55001005, timRef.BBCCCVVV);
			Assert.IsTrue(timRef.Valid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests parsing the full Spanish name.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ParseRefString_FullNameSpanish()
		{
			BCVRef timRef = m_mlscrBook.ParseRefString("2 Timoteo 1:5");
			Assert.AreEqual(55001005, timRef.BBCCCVVV);
			Assert.IsTrue(timRef.Valid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests parsing the Spanish abbreviation.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ParseRefString_AbbrSpanish()
		{
			BCVRef timRef = m_mlscrBook.ParseRefString("2 Tim 1:5");
			Assert.AreEqual(55001005, timRef.BBCCCVVV);
			Assert.IsTrue(timRef.Valid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests parsing the SIL name.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ParseRefString_SILName()
		{
			BCVRef timRef = m_mlscrBook.ParseRefString("2TI 1:5");
			Assert.AreEqual(55001005, timRef.BBCCCVVV);
			Assert.IsTrue(timRef.Valid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests parsing the SIL abbreviation.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ParseRefString_AbbrSIL()
		{
			BCVRef timRef = m_mlscrBook.ParseRefString("4T 1:5");
			Assert.AreEqual(55001005, timRef.BBCCCVVV);
			Assert.IsTrue(timRef.Valid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests edge cases when parsing a reference string
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TestCase("Luk 5,15", ExpectedResult = 42005015)]
		[TestCase("luk 5.15", ExpectedResult = 42005015)]
		[TestCase("LUK5:15", ExpectedResult = 42005015)]
		[TestCase("LUK5/15", ExpectedResult = 42005015)]
		[TestCase("LUK5_15", ExpectedResult = 42005015)]
		[TestCase("LUK5;15", ExpectedResult = 42005015)]
		[TestCase("4T1:5", ExpectedResult = 55001005)]
		public int ParseRefString_EdgeCases(string input)
		{
			return m_mlscrBook.ParseRefString(input).BBCCCVVV;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test parsing invalid reference strings. We expect to get an invalid BCVRef,
		/// but not an exception.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ParseRefString_EmptyString_Invalid()
		{
			Assert.IsFalse(m_mlscrBook.ParseRefString(string.Empty).Valid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that passing in a null value as string to parse throws an NullReferenceException.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ParseRefString_NullArgument_ThrowsNullReferenceException()
		{
			Assert.Throws<NullReferenceException>(() => m_mlscrBook.ParseRefString(null));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ParseRefString method when called with a verse bridge
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ParseRefString_VerseBridge_GetsStartRefOfBridge()
		{
			BCVRef scrRef = m_mlscrBook.ParseRefString("LUK 23:50-51");

			Assert.IsTrue(scrRef.Valid);
			Assert.AreEqual(42023050, scrRef.BBCCCVVV);
		}
	}
}
