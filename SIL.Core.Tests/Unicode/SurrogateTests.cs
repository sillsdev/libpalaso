// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using NUnit.Framework;
using SIL.Unicode;

namespace SIL.Tests.Unicode
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Tests for the the Surrogates util class.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class SurrogateTests
	{
		[TestCase("ab\xD800\xDC00c", 0, ExpectedResult = 1)]
		[TestCase("ab\xD800\xDC00c", 1, ExpectedResult = 2)]
		[TestCase("ab\xD800\xDC00c", 2, ExpectedResult = 4)]
		[TestCase("ab\xD800\xDC00c", 4, ExpectedResult = 5)]
		[TestCase("ab\xD800\xDC00", 2, ExpectedResult = 4)]
		public int NextChar(string text, int index)
		{
			return Surrogates.NextChar(text, index);
		}

		[TestCase("ab\xD800\xDC00c", 1, ExpectedResult = 0)]
		[TestCase("ab\xD800\xDC00c", 2, ExpectedResult = 1)]
		[TestCase("ab\xD800\xDC00c", 3, ExpectedResult = 2, TestName = "initial ich at a bad position, move back normally to sync")]
		[TestCase("ab\xD800\xDC00c", 4, ExpectedResult = 2, TestName = "double move succeeds")]
		[TestCase("ab\xD800\xDC00c", 5, ExpectedResult = 4)]
		[TestCase("\xD800\xDC00c", 2, ExpectedResult = 0, TestName = "double move succeeds at start (and end)")]
		[TestCase("\xDC00c", 1, ExpectedResult = 0, TestName = "no double move on bad trailer at start")]
		[TestCase("\xD800c", 1, ExpectedResult = 0, TestName = "no double move on bad leader at start")]
		public int PrevChar(string text, int index)
		{
			return Surrogates.PrevChar(text, index);
		}

		// the following bad data cases we can't put in a testcase above because the compiler or
		// nunit detect that it's bad data and convert a null string to the test case. Doing it
		// this way works.
		[Test]
		public void NextChar_BadData()
		{
			Assert.AreEqual(3, Surrogates.NextChar("ab\xD800", 2), "Badly formed pair at end...don't go too far");
			Assert.AreEqual(3, Surrogates.NextChar("ab\xD800c", 2), "Badly formed pair in middle...don't go too far");
		}

		[Test]
		public void PrevChar_BadData()
		{
			Assert.AreEqual(3, Surrogates.PrevChar("ab\xD800c", 4), "no double move on bad pair");
		}
	}
}
