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

namespace SIL.ScriptureUtils
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
		[Test]
		public void GetBookAbbrev()
		{
			Assert.AreEqual("Gen", m_mlscrBook.GetBookAbbrev(1));
			Assert.AreEqual("Mat", m_mlscrBook.GetBookAbbrev(40));
			Assert.AreEqual("Rev", m_mlscrBook.GetBookAbbrev(66));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test the <see cref="MultilingScrBooks.GetBookAbbrev"/> method. Test with 
		/// non-existing encoding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetBookAbbrevDifferentEncoding()
		{
			List<string> array = new List<string>();
			array.Add("99"); // some arbitrary should-never-exist value
			m_mlscrBook.RequestedEncodings = array;

			Assert.AreEqual("GEN", m_mlscrBook.GetBookAbbrev(1));
			Assert.AreEqual("MAT", m_mlscrBook.GetBookAbbrev(40));
			Assert.AreEqual("REV", m_mlscrBook.GetBookAbbrev(66));
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
		[Test]
		public void ParseRefString()
		{
			Assert.AreEqual(65001001, m_mlscrBook.ParseRefString("jud").BBCCCVVV);
			Assert.AreEqual(7001001, m_mlscrBook.ParseRefString("ju").BBCCCVVV);
			Assert.AreEqual(65001001, m_mlscrBook.ParseRefString("JUD").BBCCCVVV);
			Assert.AreEqual(7001001, m_mlscrBook.ParseRefString("jU").BBCCCVVV);
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
		[Test]
		public void ParseRefString_EdgeCases()
		{
			Assert.AreEqual(42005015, m_mlscrBook.ParseRefString("Luk 5,15"));
			Assert.AreEqual(42005015, m_mlscrBook.ParseRefString("luk 5.15"));
			Assert.AreEqual(42005015, m_mlscrBook.ParseRefString("LUK5:15"));
			Assert.AreEqual(55001005, m_mlscrBook.ParseRefString("4T1:5"));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test parsing invalid reference strings. We expect to get an invalid BCVRef,
		/// but not an exception.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ParseRefString_Invalid()
		{
			Assert.IsFalse(m_mlscrBook.ParseRefString(string.Empty).Valid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that passing in a null value as string to parse throws an NullReferenceException.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		[ExpectedException(typeof(NullReferenceException))]
		public void ParseRefString_NullArgument()
		{
			m_mlscrBook.ParseRefString(null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the ParseRefString method when called with a verse bridge
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ParseRefStringWithVerseBridge()
		{
			BCVRef scrRef = m_mlscrBook.ParseRefString("LUK 23:50-51");

			Assert.IsTrue(scrRef.Valid);
			Assert.AreEqual(42023050, scrRef.BBCCCVVV);
		}
	}
}
