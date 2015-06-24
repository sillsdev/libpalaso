using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SIL.Text;

namespace SIL.Tests.Text
{
	[TestFixture]
	public class ApproximateMatcherTests
	{
		private IList<string> _forms;

		[SetUp]
		public void Setup()
		{
			_forms = new List<string>();
		}

		private void AddEntry(string form)
		{
			_forms.Add(form);
		}

		[Test]
		public void Equal()
		{
			AddEntry("distance");
			AddEntry("distances");
			AddEntry("distane");
			AddEntry("destance");
			AddEntry("distence");
			IList closest = (IList) ApproximateMatcher.FindClosestForms(_forms, "distance");
			Assert.AreEqual(1, closest.Count);
			Assert.Contains("distance", closest);
		}

		[Test]
		public void Prefixes()
		{
			AddEntry("distance");
			AddEntry("distances");
			AddEntry("distane");
			AddEntry("destance");
			AddEntry("distence");
			IList closest =
				(IList)
				ApproximateMatcher.FindClosestForms(_forms,
													"dist",
													ApproximateMatcherOptions.
														IncludePrefixedForms);
			Assert.AreEqual(4, closest.Count);
			Assert.Contains("distance", closest);
			Assert.Contains("distances", closest);
			Assert.Contains("distane", closest);
			Assert.Contains("distence", closest);
		}

		[Test]
		public void FindClosestAndNextClosestAndPrefixedForms()
		{
			AddEntry("bad"); //2
			AddEntry("goad"); //1
			AddEntry("godo"); //1
			AddEntry("good"); //0
			AddEntry("good-bye"); //0
			IList closest =
				(IList)
				ApproximateMatcher.FindClosestForms(_forms,
													"good",
													ApproximateMatcherOptions.
														IncludePrefixedAndNextClosestForms);
			Assert.AreEqual(4, closest.Count);
			Assert.Contains("goad", closest);
			Assert.Contains("godo", closest);
			Assert.Contains("good", closest);
			Assert.Contains("good-bye", closest);
		}

		[Test]
		public void FindClosestAndNextClosestAndPrefixedForms_TwoBestOneSecondBest()
		{
			// This test is here because we found that we had made it so
			// that when the best edit distance and the second best
			// edit distance are one apart and the edit distance for an
			// entry that is the same as the best is found,
			// it clears the second best and makes the second best edit
			// distance the same as the best edit distance! Oooops!
			AddEntry("past"); //2
			AddEntry("dest"); //1
			AddEntry("dits"); //1
			AddEntry("noise"); //3
			IList closest =
				(IList)
				ApproximateMatcher.FindClosestForms(_forms,
													"dist",
													ApproximateMatcherOptions.
														IncludePrefixedAndNextClosestForms);
			Assert.AreEqual(3, closest.Count);
			Assert.Contains("past", closest);
			Assert.Contains("dest", closest);
			Assert.Contains("dits", closest);
		}

		[Test]
		public void Closest_EditDistance1()
		{
			AddEntry("a1234567890"); // insertion at beginning
			AddEntry("1a234567890"); // insertion in middle
			AddEntry("1234567890a"); // insertion at end
			AddEntry("234567890"); // deletion at beginning
			AddEntry("123457890"); // deletion in middle
			AddEntry("123456789"); // deletion at end
			AddEntry("a234567890"); //substitution at beginning
			AddEntry("1234a67890"); //substitution in middle
			AddEntry("123456789a"); //substitution at end
			AddEntry("2134567890"); //transposition at beginning
			AddEntry("1234657890"); //transposition in middle
			AddEntry("1234567809"); //transposition at end

			AddEntry("aa1234567890"); // noise
			AddEntry("1a23456789a0");
			AddEntry("1a2a34567890");
			AddEntry("1a23a4567890");
			AddEntry("1a234a567890");
			AddEntry("1a2345a67890");
			AddEntry("ab34567890");
			AddEntry("1234ab7890");
			AddEntry("12345678ab");
			AddEntry("2134567809");
			AddEntry("1235467980");

			IList closest = (IList) ApproximateMatcher.FindClosestForms(_forms, "1234567890");
			Assert.AreEqual(12, closest.Count);
			Assert.Contains("a1234567890", closest);
			Assert.Contains("1a234567890", closest);
			Assert.Contains("1234567890a", closest);
			Assert.Contains("234567890", closest);
			Assert.Contains("123457890", closest);
			Assert.Contains("123456789", closest);
			Assert.Contains("a234567890", closest);
			Assert.Contains("1234a67890", closest);
			Assert.Contains("123456789a", closest);
			Assert.Contains("2134567890", closest);
			Assert.Contains("1234657890", closest);
			Assert.Contains("1234567809", closest);
		}

		[Test]
		public void ClosestAndNextClosest_EditDistance0and1()
		{
			AddEntry("a1234567890"); // insertion at beginning
			AddEntry("1a234567890"); // insertion in middle
			AddEntry("1234567890a"); // insertion at end
			AddEntry("234567890"); // deletion at beginning
			AddEntry("123457890"); // deletion in middle
			AddEntry("123456789"); // deletion at end
			AddEntry("a234567890"); //substitution at beginning
			AddEntry("1234a67890"); //substitution in middle
			AddEntry("123456789a"); //substitution at end
			AddEntry("2134567890"); //transposition at beginning
			AddEntry("1234657890"); //transposition in middle
			AddEntry("1234567809"); //transposition at end
			AddEntry("1234567890"); // identity

			AddEntry("aa1234567890"); // noise
			AddEntry("1a23456789a0");
			AddEntry("1a2a34567890");
			AddEntry("1a23a4567890");
			AddEntry("1a234a567890");
			AddEntry("1a2345a67890");
			AddEntry("ab34567890");
			AddEntry("1234ab7890");
			AddEntry("12345678ab");
			AddEntry("2134567809");
			AddEntry("1235467980");

			IList closest =
				(IList)
				ApproximateMatcher.FindClosestForms(_forms,
													"1234567890",
													ApproximateMatcherOptions.
														IncludeNextClosestForms);
			Assert.AreEqual(13, closest.Count);
			Assert.Contains("1234567890", closest);
			Assert.Contains("a1234567890", closest);
			Assert.Contains("1a234567890", closest);
			Assert.Contains("1234567890a", closest);
			Assert.Contains("234567890", closest);
			Assert.Contains("123457890", closest);
			Assert.Contains("123456789", closest);
			Assert.Contains("a234567890", closest);
			Assert.Contains("1234a67890", closest);
			Assert.Contains("123456789a", closest);
			Assert.Contains("2134567890", closest);
			Assert.Contains("1234657890", closest);
			Assert.Contains("1234567809", closest);
		}

		[Test]
		public void ClosestAndNextClosest_EditDistance1and3()
		{
			AddEntry("a1234567890");
			AddEntry("aaa1234567890");

			AddEntry("aaa1234567890a"); // noise

			IList closest =
				(IList)
				ApproximateMatcher.FindClosestForms(_forms,
													"1234567890",
													ApproximateMatcherOptions.
														IncludeNextClosestForms);
			Assert.AreEqual(2, closest.Count);
			Assert.Contains("a1234567890", closest);
			Assert.Contains("aaa1234567890", closest);
		}

		[Test]
		public void EditDistance_Same_0()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "abo", 0, false);
			Assert.AreEqual(0, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtStart_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "habo", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtStart_0Max_EditDistanceLargerThanMax()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "habo", 0, false);
			Assert.AreEqual(ApproximateMatcher.EditDistanceLargerThanMax, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtMiddle_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "abho", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtMiddle_0Max_EditDistanceLargerThanMax()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "abho", 0, false);
			Assert.AreEqual(ApproximateMatcher.EditDistanceLargerThanMax, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtEnd_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "aboh", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtStartAndMiddle_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "habho", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtStartAndMiddle_1Max_EditDistanceLargerThanMax()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "habho", 1, false);
			Assert.AreEqual(ApproximateMatcher.EditDistanceLargerThanMax, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtMiddleAndEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "abhoh", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtMiddleAndEnd_1Max_EditDistanceLargerThanMax()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "abhoh", 1, false);
			Assert.AreEqual(ApproximateMatcher.EditDistanceLargerThanMax, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtStartAndEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "haboh", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtStartAndEnd_1Max_EditDistanceLargerThanMax()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "haboh", 1, false);
			Assert.AreEqual(ApproximateMatcher.EditDistanceLargerThanMax, editDistance);
		}

		[Test]
		public void EditDistance_SingleDeletionAtStart_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "bo", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleDeletionAtMiddle_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "ao", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleDeletionAtEnd_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "ab", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleDeletionAtStartAndMiddle_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("aboh", "bh", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleDeletionAtMiddleAndEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("aboh", "ao", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleDeletionAtStartAndEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("aboh", "bo", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleSubstitutionAtStart_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "sbo", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleSubstitutionAtMiddle_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "aso", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleSubstitutionAtEnd_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "abs", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleSubstitutionAtStartAndMiddle_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("aboh", "sbsh", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleSubstitutionAtMiddleAndEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("aboh", "asos", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleSubstitutionAtStartAndEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("aboh", "sbos", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleTranspositionAtStart_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "bao", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleTranspositionAtMiddle_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("aboh", "aobh", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleTranspositionAtEnd_1()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "aob", 1, false);
			Assert.AreEqual(1, editDistance);
		}

		[Test]
		public void EditDistance_SingleTranspositionAtStartAndMiddle_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("abohor", "baoohr", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleTranspositionAtMiddleAndEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("abohor", "aobhro", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleTranspositionAtStartAndEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("abohor", "baohro", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleDeletionAtStartAndSingleInsertionAtEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "boh", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void
			EditDistance_SingleDeletionAtStartAndSingleInsertionAtEnd_1Max_EditDistanceLargerThanMax
			()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "boh", 1, false);
			Assert.AreEqual(ApproximateMatcher.EditDistanceLargerThanMax, editDistance);
		}

		[Test]
		public void EditDistance_SingleDeletionAtStartAndDoubleInsertionAtEnd_3()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "boha", 3, false);
			Assert.AreEqual(3, editDistance);
		}

		[Test]
		public void
			EditDistance_SingleDeletionAtStartAndDoubleInsertionAtEnd_2Max_EditDistanceLargerThanMax
			()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "boha", 2, false);
			Assert.AreEqual(ApproximateMatcher.EditDistanceLargerThanMax, editDistance);
		}

		[Test]
		public void EditDistance_SingleDeletionAtStartAndSingleTranspositionAtEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("aboh", "bho", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void
			EditDistance_SingleDeletionAtStartAndSingleTranspositionAtEnd_1Max_EditDistanceLargerThanMax
			()
		{
			int editDistance = ApproximateMatcher.EditDistance("aboh", "bho", 1, false);
			Assert.AreEqual(ApproximateMatcher.EditDistanceLargerThanMax, editDistance);
		}

		[Test]
		public void EditDistance_SingleDeletionAtStartAndSingleSubstitutionAtEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "bi", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void
			EditDistance_SingleDeletionAtStartAndSingleSubstitutionAtEnd_1Max_EditDistanceLargerThanMax
			()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "bi", 1, false);
			Assert.AreEqual(ApproximateMatcher.EditDistanceLargerThanMax, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtStartAndSingleDeletionAtEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "rab", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtStartAndSingleTranspositionAtEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "raob", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtStartAndSingleSubstitutionAtEnd_2()
		{
			int editDistance = ApproximateMatcher.EditDistance("abo", "rabi", 2, false);
			Assert.AreEqual(2, editDistance);
		}

		[Test]
		public void EditDistance_SingleInsertionAtEndWithSuffixTreatedAsZeroDistance_1()
		{
			Assert.AreEqual(1, ApproximateMatcher.EditDistance("cased", "case", 1, true));
		}

		[Test]
		public void EditDistance_SingleInsertionAtEndWithSuffixTreatedAsZeroDistance_0()
		{
			Assert.AreEqual(0, ApproximateMatcher.EditDistance("case", "cased", 1, true));
		}

		[Test]
		public void EditDistance_SingleInsertionAtBeginningWithSuffixTreatedAsZeroDistance_1()
		{
			// has suffix to ignore on the second word
			Assert.AreEqual(1, ApproximateMatcher.EditDistance("case", "acaseinfact", 1, true));
		}

		[Test]
		public void
			EditDistance_SingleDeletionAtBeginningWithSixInsertionsAtEndAndSuffixTreatedAsZeroDistance_7
			()
		{
			// no suffix to ignore on the second word
			Assert.AreEqual(7,
							ApproximateMatcher.EditDistance("acaseinfact",
															"case",
															int.MaxValue,
															true));
		}

		[Test]
		public void
			EditDistance_SingleInsertionSingleSubstitutionAtBeginningSingleSubstitutionAtEndWithSuffixTreatedAsZeroDistance_3
			()
		{
			// has suffix to ignore on the second word
			Assert.AreEqual(3, ApproximateMatcher.EditDistance("dist", "noise", 4, true));
		}

		[Test]
		public void
			EditDistance_SingleDeletionSingleSubstitutionAtBeginningSingleSubstitutionAtEndWithSuffixTreatedAsZeroDistance_3
			()
		{
			// no suffix to ignore on the second word
			Assert.AreEqual(3, ApproximateMatcher.EditDistance("noise", "dist", 4, true));
		}

		[Test]
		public void
			EditDistance_SingleInsertionSingleSubstitutionAtBeginningSingleSubstitutionAtEndWithSuffixTreatedAsZeroDistance_2Cutoff_EditDistanceLargerThanMax
			()
		{
			Assert.AreEqual(ApproximateMatcher.EditDistanceLargerThanMax,
							ApproximateMatcher.EditDistance("noise", "dist", 2, true));
			Assert.AreEqual(ApproximateMatcher.EditDistanceLargerThanMax,
							ApproximateMatcher.EditDistance("dist", "noise", 2, true));
		}

		[Test]
		public void EditDistance_MajorDifference()
		{
			Assert.AreEqual(2,
							ApproximateMatcher.EditDistance("ab",
															"\"look busy do nothing\"",
															int.MaxValue,
															true));
		}

		/// <summary>
		/// This test was created after we found that LexEntries did not
		/// cascade on their delete and so lexical forms could be found even when
		/// their entry was deleted, causing a crash.
		/// </summary>
		[Test]
		public void Find_AfterDeleted_NotFound() // move to sorted Cache Tests
		{
			//LexEntry test = AddEntry("test");
			//AddEntry("test1");
			//this._records.Remove(test);

			//IList<LexEntry> closest = ApproximateMatcher.FindEntriesWithClosestLexemeForms("test", _forms);
			//Assert.AreEqual(1, closest.Count);
			//Assert.Contains("test1", closest);
		}

		//[Test]
		//public void Time()
		//{
		//    Stopwatch stopwatch = new Stopwatch();
		//    stopwatch.Start();
		//    Random random = new Random();
		//    for (int i = 0; i < 5000; i++)
		//    {
		//        string LexicalForm = string.Empty;
		//        for (int j = 0; j < 10; j++) //average word length of 10 characters
		//        {
		//            LexicalForm += Convert.ToChar(random.Next(Convert.ToInt16('a'), Convert.ToInt16('z')));
		//        }
		//        AddEntry(LexicalForm);
		//    }
		//
		//    stopwatch.Stop();
		//    Console.WriteLine("Time to initialize " + stopwatch.Elapsed.ToString());
		//
		//    stopwatch.Reset();
		//    stopwatch.Start();
		//    Db4oLexQueryHelper.FindClosest("something");
		//    stopwatch.Stop();
		//    Console.WriteLine("Time to find " + stopwatch.Elapsed.ToString());
		//}
	}
}