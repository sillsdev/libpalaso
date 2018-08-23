using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SIL.Reflection;

namespace SIL.Scripture.Tests
{
	#region ScrVers test helper class
	/// <summary>
	/// Provides methods for accessing private data on Versification and Versification.Table
	/// for testing purposes
	/// </summary>
	public static class ScrVersReflectionHelper
	{
		#region Extension methods for easy access to private data
		/// <summary>
		/// Provides access to the ParseChapterVerseLine method
		/// </summary>
		public static void ParseChapterVerseLine(this ScrVers scrVers, string chapVerseLine)
		{
			Versification internalVers = (Versification)ReflectionHelper.GetProperty(scrVers, "VersInfo");
			ReflectionHelper.CallMethodWithThrow(Versification.Table.Implementation, "ProcessVersLine",
				chapVerseLine, "testfile", ScrVersType.Unknown, "", internalVers);
		}

		/// <summary>
		/// Provides access to the ParseExcludedVerseLine method
		/// </summary>
		public static void ParseExcludedVerseLine(this ScrVers scrVers, string excludeLine)
		{
			Versification internalVers = (Versification)ReflectionHelper.GetProperty(scrVers, "VersInfo");
			ReflectionHelper.CallMethodWithThrow(Versification.Table.Implementation, "ProcessVersLine",
				"#! " + excludeLine, "testfile", ScrVersType.Unknown, "", internalVers);
		}

		/// <summary>
		/// Provides access to the ParseMappingLine method
		/// </summary>
		public static void ParseMappingLine(this ScrVers scrVers, string mappingLine)
		{
			Versification internalVers = scrVers.VersInfo;
			ReflectionHelper.CallMethodWithThrow(Versification.Table.Implementation, "ProcessVersLine",
				mappingLine, "testfile", ScrVersType.Unknown, "", internalVers);
		}

		/// <summary>
		/// Provides access to the ParseRangeToOneMappingLine method
		/// </summary>
		public static void ParseRangeToOneMappingLine(this ScrVers scrVers, string mappingLine)
		{
			Versification internalVers = (Versification)ReflectionHelper.GetProperty(scrVers, "VersInfo");
			ReflectionHelper.CallMethodWithThrow(Versification.Table.Implementation, "ProcessVersLine",
				"#! " + mappingLine, "testfile", ScrVersType.Unknown, "", internalVers);
		}

		/// <summary>
		/// Provides access to the ParseVerseSegmentsLine method
		/// </summary>
		public static void ParseVerseSegmentsLine(this ScrVers scrVers, string segmentsLine, string fileName = "testfile")
		{
			Versification internalVers = (Versification)ReflectionHelper.GetProperty(scrVers, "VersInfo");
			ReflectionHelper.CallMethodWithThrow(Versification.Table.Implementation, "ProcessVersLine",
				"#! " + segmentsLine, fileName, ScrVersType.Unknown, "", internalVers);
		}

		/// <summary>
		/// Provides access to the ProcessVersLine method
		/// </summary>
		public static void ProcessVersLine(this ScrVers scrVers, string versLine)
		{
			Versification internalVers = (Versification)ReflectionHelper.GetProperty(scrVers, "VersInfo");
			ReflectionHelper.CallMethodWithThrow(Versification.Table.Implementation, "ProcessVersLine",
				versLine, "testfile", ScrVersType.Unknown, "", internalVers);
		}

		/// <summary>
		/// Provides access to the bookList field
		/// </summary>
		public static List<int[]> bookList(this ScrVers scrVers)
		{
			Versification internalVers = (Versification)ReflectionHelper.GetProperty(scrVers, "VersInfo");
			return (List<int[]>)ReflectionHelper.GetField(internalVers, "bookList");
		}

		/// <summary>
		/// Provides access to the excludedVerses field
		/// </summary>
		public static HashSet<int> excludedVerses(this ScrVers scrVers)
		{
			Versification internalVers = (Versification)ReflectionHelper.GetProperty(scrVers, "VersInfo");
			return (HashSet<int>)ReflectionHelper.GetField(internalVers, "excludedVerses");
		}

		/// <summary>
		/// Provides access to the verseSegments field
		/// </summary>
		public static Dictionary<int, string[]> verseSegments(this ScrVers scrVers)
		{
			Versification internalVers = (Versification)ReflectionHelper.GetProperty(scrVers, "VersInfo");
			return (Dictionary<int, string[]>)ReflectionHelper.GetField(internalVers, "verseSegments");
		}

		/// <summary>
		/// Clears all information from the internal versification for testing on a clean slate
		/// </summary>
		public static void ClearAllInfo(this ScrVers scrVers)
		{
			Versification internalVers = (Versification)ReflectionHelper.GetProperty(scrVers, "VersInfo");
			((List<int[]>)ReflectionHelper.GetField(internalVers, "bookList")).Clear();
			scrVers.ClearExcludedVerses();
			((Dictionary<int, string[]>)ReflectionHelper.GetField(internalVers, "verseSegments")).Clear();

			object mappings = ReflectionHelper.GetField(internalVers, "mappings");
			ReflectionHelper.CallMethodWithThrow(mappings, "Clear");
		}

		/// <summary>
		/// Clears all excluded verse information from the internal versification for testing 
		/// on a clean slate
		/// </summary>
		public static void ClearExcludedVerses(this ScrVers scrVers)
		{
			Versification internalVers = (Versification)ReflectionHelper.GetProperty(scrVers, "VersInfo");
			((HashSet<int>)ReflectionHelper.GetField(internalVers, "excludedVerses")).Clear();
		}

		/// <summary>
		/// Clears all verse segments information from the internal versification for testing on a clean slate
		/// </summary>
		public static void ClearVerseSegments(this ScrVers scrVers)
		{
			Versification internalVers = (Versification)ReflectionHelper.GetProperty(scrVers, "VersInfo");
			((Dictionary<int, string[]>)ReflectionHelper.GetField(internalVers, "verseSegments")).Clear();
		}

		/// <summary>
		/// Provides access to the WriteToStream method
		/// </summary>
		public static void WriteToStream(this ScrVers scrVers, StringWriter stream)
		{
			ReflectionHelper.CallMethodWithThrow(scrVers.VersInfo, "WriteToStream", stream);
		}
		#endregion
		
		#region Other test helper methods
		/// <summary>
		/// Runs the specified action when the Versification table is in "merge mode"
		/// </summary>
		public static void Merge(Action action)
		{
			ReflectionHelper.SetField(typeof(Versification.Table), "mergeMode", true);
			try
			{
				action();
			}
			finally
			{
				ReflectionHelper.SetField(typeof(Versification.Table), "mergeMode", false);
			}
		}

		public static Versification CreateClonedVers(Versification orig, string newName)
		{
			return (Versification)ReflectionHelper.CreateClassInstance(Assembly.Load("SIL.Scripture"), 
				"SIL.Scripture.Versification", new object[] { orig, newName, "" });
		}
		#endregion
	}
	#endregion

	/// <summary>
	/// Tests methods in the ScrVers/Versification class.
	/// </summary>
	[TestFixture]
	public class ScrVersTests
	{
		private ScrVers versification;

		#region Setup/Teardown
		[SetUp]
		public void Setup()
		{
			versification = new ScrVers("DummyScrVers"); // Defaults to the eng.vrs file but without a common name
			versification.ClearAllInfo(); // Clear all mappings, etc. from the versification so we can test with a clean slate
		}

		[TearDown]
		public void TearDown()
		{
			Versification.Table.Implementation.RemoveAllUnknownVersifications();
		}
		#endregion

		#region ParseChapterVerseLine
		/// <summary>
		/// Tests the method ParseChapterVerseLine.
		/// </summary>
		[Test]
		public void ParseChapterVerseLine()
		{
			versification.ParseChapterVerseLine("HAB 1:17 2:20 3:19");

			int bookId = Canon.BookIdToNumber("HAB");
			List<int[]> bookList = versification.bookList();
			Assert.AreEqual(bookList.Count, bookId);
			Assert.AreEqual(bookList[bookId - 1], new [] { 17, 20, 19 });

			Assert.AreEqual(bookId, versification.GetLastBook(),
				"HAB should be the last book in the versification");
			Assert.AreEqual(3, versification.GetLastChapter(bookId), "HAB has three chapters");
			Assert.AreEqual(17, versification.GetLastVerse(bookId, 1), "HAB 1 has 17 verses");
			Assert.AreEqual(20, versification.GetLastVerse(bookId, 2), "HAB 2 has 20 verses");
			Assert.AreEqual(19, versification.GetLastVerse(bookId, 3), "HAB 3 has 19 verses");
		}

		/// <summary>
		/// Tests the method ParseChapterVerseLine with an invalid book name.
		/// </summary>
		[Test]
		public void ParseChapterVerseLine_InvalidBook()
		{
			VerifyThrows(() => versification.ParseChapterVerseLine("BADBOOK 1:17 2:20 3:19"),
				VersificationLoadErrorType.InvalidSyntax, "BADBOOK 1:17 2:20 3:19", "testfile");
		}

		/// <summary>
		/// Tests the method ParseChapterVerseLine with an invalid verse.
		/// </summary>
		[Test]
		public void ParseChapterVerseLine_InvalidVerse()
		{
			VerifyThrows(() => versification.ParseChapterVerseLine("HAB 1:BADVERSE"),
				VersificationLoadErrorType.InvalidSyntax, "HAB 1:BADVERSE", "testfile");
		}
		#endregion

		#region ParseMappingLine tests
		/// <summary>
		/// Tests the method ParseMappingLine when the mapping is for a single verse.
		/// </summary>
		[Test]
		public void ParseMappingLine_SingleVerse()
		{
			versification.ParseMappingLine("NUM 17:1 = NUM 17:16");

			// Get mapping from "NUM 17:1 = NUM 17:16" in the versification
			VerseRef vref = new VerseRef(4, 17, 1, versification);
			ScrVers.Original.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef(4, 17, 16, ScrVers.Original), vref);

			vref = new VerseRef(4, 17, 16, ScrVers.Original);
			versification.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef(4, 17, 1, versification), vref);
		}
		
		/// <summary>
		/// Tests the method ParseMappingLine when mapping a range to another range of verses.
		/// </summary>
		[Test]
		public void ParseMappingLine_Range()
		{
			versification.ParseMappingLine("NUM 17:1-13 = NUM 17:16-28");

			// Get mapping from "NUM 17:1-13 = NUM 17:16-28" in the versification
			VerseRef vref;
			for (int i = 1; i <= 13; i++)
			{
				vref = new VerseRef(4, 17, i, versification);
				ScrVers.Original.ChangeVersification(ref vref);
				Assert.AreEqual(new VerseRef(4, 17, i + 15, ScrVers.Original), vref);
			}

			for (int i = 16; i <= 28; i++)
			{
				vref = new VerseRef(4, 17, i, ScrVers.Original);
				versification.ChangeVersification(ref vref);
				Assert.AreEqual(new VerseRef(4, 17, i - 15, versification), vref);
			}
		}

		/// <summary>
		/// Tests the method ParseMappingLine when the equal sign ('=') is missing from the mapping.
		/// </summary>
		[Test]
		public void ParseMappingLine_NoEqualSign()
		{
			VerifyThrows(() => versification.ParseMappingLine("NUM 17:1 NUM 17:16"),
				VersificationLoadErrorType.InvalidSyntax, "NUM 17:1 NUM 17:16", "testfile");
		}
		#endregion

		#region ParseRangeToOneMappingLine tests
		/// <summary>
		/// Tests the method ParseMappingLine when the mapping creates a many-to-one verse mapping
		/// (i.e. the verse in the original was split into three verses in the versification).
		/// See FB-17661
		/// </summary>
		[Test]
		public void ParseRangeToOneMappingLine_ManyToOneMapping()
		{
			versification.ParseRangeToOneMappingLine("&ACT 19:39-41 = ACT 19:40");

			VerseRef vref = new VerseRef("ACT 19:39", versification);
			ScrVers.Original.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:40", ScrVers.Original), vref);

			vref = new VerseRef("ACT 19:40", versification);
			ScrVers.Original.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:40", ScrVers.Original), vref);

			vref = new VerseRef("ACT 19:41", versification);
			ScrVers.Original.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:40", ScrVers.Original), vref);


			vref = new VerseRef("ACT 19:39", ScrVers.Original);
			versification.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:39", versification), vref);

			vref = new VerseRef("ACT 19:40", ScrVers.Original);
			versification.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:39", versification), vref);

			vref = new VerseRef("ACT 19:41", ScrVers.Original);
			versification.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:41", versification), vref);
		}

		/// <summary>
		/// Tests the method ParseMappingLine when the mapping creates a one-to-many verse mapping
		/// (i.e. three verses in the original were merged into a single verse in the versification).
		/// See FB-17661
		/// </summary>
		[Test]
		public void ParseRangeToOneMappingLine_OneToManyMapping()
		{
			versification.ParseRangeToOneMappingLine("&ACT 19:39 = ACT 19:38-40");
			VerseRef vref = new VerseRef("ACT 19:38", versification);
			ScrVers.Original.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:38", ScrVers.Original), vref);

			vref = new VerseRef("ACT 19:39", versification);
			ScrVers.Original.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:38", ScrVers.Original), vref);

			vref = new VerseRef("ACT 19:40", versification);
			ScrVers.Original.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:40", ScrVers.Original), vref);


			vref = new VerseRef("ACT 19:38", ScrVers.Original);
			versification.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:39", versification), vref);

			vref = new VerseRef("ACT 19:39", ScrVers.Original);
			versification.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:39", versification), vref);

			vref = new VerseRef("ACT 19:40", ScrVers.Original);
			versification.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:39", versification), vref);
		}

		/// <summary>
		/// Tests the method ParseMappingLine when the mapping creates a many-to-many verse mapping.
		/// See FB-17661
		/// </summary>
		[Test]
		public void ParseRangeToOneMappingLine_InvalidManyToManyMapping()
		{
			VerifyThrows(() => versification.ParseRangeToOneMappingLine("&ACT 19:39-40 = ACT 19:38-40"),
				VersificationLoadErrorType.InvalidManyToOneMap, "ACT 19:39-40 = ACT 19:38-40", "testfile");
		}

		/// <summary>
		/// Tests the method ParseMappingLine when the mapping contains an invalid reference.
		/// See FB-17661
		/// </summary>
		[Test]
		public void ParseRangeToOneMappingLine_InvalidReference()
		{
			VerifyThrows(() => versification.ParseRangeToOneMappingLine("&BADBOOK1 19:39-40 = ACT 19:38-40"),
				VersificationLoadErrorType.InvalidSyntax, "BADBOOK1 19:39-40 = ACT 19:38-40", "testfile");
			VerifyThrows(() => versification.ParseRangeToOneMappingLine("&ACT 19:39-40 = BADBOOK2 19:38-40"),
				VersificationLoadErrorType.InvalidSyntax, "ACT 19:39-40 = BADBOOK2 19:38-40", "testfile");
			VerifyThrows(() => versification.ParseRangeToOneMappingLine("&ACT BADCHAPTER:39-40 = ACT 19:38-40"),
				VersificationLoadErrorType.InvalidSyntax, "ACT BADCHAPTER:39-40 = ACT 19:38-40", "testfile");
			VerifyThrows(() => versification.ParseRangeToOneMappingLine("&ACT 19:BADSTARTVERSE-40 = ACT 19:38-40"),
				VersificationLoadErrorType.InvalidSyntax, "ACT 19:BADSTARTVERSE-40 = ACT 19:38-40", "testfile");
			VerifyThrows(() => versification.ParseRangeToOneMappingLine("&ACT 19:BADSTARTVERSE-40 = ACT 19:38-40"),
				VersificationLoadErrorType.InvalidSyntax, "ACT 19:BADSTARTVERSE-40 = ACT 19:38-40", "testfile");
			VerifyThrows(() => versification.ParseRangeToOneMappingLine("&ACT 19:39-BADENDVERSE = ACT 19:38-40"),
				VersificationLoadErrorType.InvalidSyntax, "ACT 19:39-BADENDVERSE = ACT 19:38-40", "testfile");
		}
		#endregion

		#region ParseExcludedVerseLine tests
		/// <summary>
		/// Tests the method ParseExcludedVerseLine with a valid verse.
		/// </summary>
		[Test]
		public void ParseExcludedVerseLine()
		{
			versification.ParseExcludedVerseLine("-GEN 1:31");

			// Confirm that the expected verse is added to the hash table.
			HashSet<int> excludedVerses = versification.excludedVerses();
			Assert.AreEqual(1, excludedVerses.Count);
			Assert.IsTrue(excludedVerses.Contains(new VerseRef("GEN 1:31").BBBCCCVVV));
			Assert.IsTrue(versification.IsExcluded(new VerseRef("GEN 1:31").BBBCCCVVV));
		}

		/// <summary>
		/// Tests the method ParseExcludedVerseLine with an invalid book.
		/// </summary>
		[Test]
		public void ParseExcludedVerseLine_InvalidBook()
		{
			VerifyThrows(() => versification.ParseExcludedVerseLine("-BADBOOK 1:31"),
				VersificationLoadErrorType.InvalidSyntax, "-BADBOOK 1:31", "testfile");
		}

		/// <summary>
		/// Tests the method ParseExcludedVerseLine with an invalid verse.
		/// </summary>
		[Test]
		public void ParseExcludedVerseLine_InvalidVerse()
		{
			VerifyThrows(() => versification.ParseExcludedVerseLine("-GEN 1:BADVERSE"),
				VersificationLoadErrorType.InvalidSyntax, "-GEN 1:BADVERSE", "testfile");
		}

		/// <summary>
		/// Tests the method ParseExcludedVerseLine with a duplicate verse.
		/// </summary>
		[Test]
		public void ParseExcludedVerseLine_Duplicate()
		{
			versification.ParseExcludedVerseLine("-GEN 1:31");
			// Add duplicate excluded verse
			VerifyThrows(() => versification.ParseExcludedVerseLine("-GEN 1:31"),
				VersificationLoadErrorType.DuplicateExcludedVerse, "-GEN 1:31", "testfile");
		}
		#endregion

		#region ParseVerseSegmentsLine tests
		/// <summary>
		/// Tests the method ParseVerseSegmentsLine with a valid verse. ParseExcludedVerseLine uses
		/// the same code to parse the reference so tests with an invalid reference are not necessary.
		/// </summary>
		[Test]
		public void ParseVerseSegmentsLine()
		{
			versification.ParseVerseSegmentsLine("*GEN 1:5,-,a,b,c,d,e,f");

			// Confirm that the expected segments are added to the specified verse.
			CheckSegments(versification, 1, new VerseRef("GEN 1:5"),
				new[] { "", "a", "b", "c", "d", "e", "f" });

			Assert.AreEqual(versification.VerseSegments(001001005),
				new[] { "", "a", "b", "c", "d", "e", "f" });
		}

		/// <summary>
		/// Tests the method ParseVerseSegmentsLine with a duplicate verse.
		/// </summary>
		[Test]
		public void ParseVerseSegmentsLine_Duplicate()
		{
			versification.ParseVerseSegmentsLine("*GEN 1:5,-,a,b,c,d,e,f", null);
			// Add duplicate verse
			VerifyThrows(() => versification.ParseVerseSegmentsLine("*GEN 1:5,-,a", null),
				VersificationLoadErrorType.DuplicateSegment, "*GEN 1:5,-,a", null);
		}

		/// <summary>
		/// Tests the method ParseVerseSegmentsLine with no segments defined.
		/// </summary>
		[Test]
		public void ParseVerseSegmentsLine_Empty()
		{
			// Add segment line that is invalid because no segments are defined
			VerifyThrows(() => versification.ParseVerseSegmentsLine("*GEN 1:5"),
				VersificationLoadErrorType.InvalidSyntax, "*GEN 1:5", "testfile");

			// Add segment line that is invalid because no segments are defined
			// (one empty segment is equivalent to no verse segments)
			VerifyThrows(() => versification.ParseVerseSegmentsLine("*GEN 1:5,-"),
				VersificationLoadErrorType.NoSegmentsDefined, "*GEN 1:5,-", "testfile");
		}

		/// <summary>
		/// Tests the method ParseVerseSegmentsLine when there are extra spaces.
		/// </summary>
		[Test]
		public void ParseVerseSegmentsLine_WithSpaces()
		{
			// Leading and trailing spaces
			versification.ParseVerseSegmentsLine(" *GEN 1:5,a,b,c ");

			// Confirm that the expected segments are added to the specified verse.
			CheckSegments(versification, 1, new VerseRef("GEN 1:5"),
				new[] { "a", "b", "c" });

			// Extra spaces between book and chapter/verse
			versification.ParseVerseSegmentsLine("*GEN   1:6,a,b,c");

			// Confirm that the expected segments are added to the specified verse.
			CheckSegments(versification, 2, new VerseRef("GEN 1:6"),
				new[] { "a", "b", "c" });

			// Spaces between segments
			versification.ParseVerseSegmentsLine("*GEN 1:7, a, b, c");

			// Confirm that the expected segments are added to the specified verse.
			CheckSegments(versification, 3, new VerseRef("GEN 1:7"),
				new[] { "a", "b", "c" });

			// Spaces between asterisk and book. Exception will not crash Paratext but be reported as a 
			// versification file error to the user.
			VerifyThrows(() => versification.ParseVerseSegmentsLine("* GEN 1:5"), 
				VersificationLoadErrorType.InvalidSyntax, "* GEN 1:5", "testfile");
		}
		#endregion

		#region ProcessVersLine (with custom modifications) tests
		/// <summary>
		/// Customize a versification scheme when the chapters/verses for a book is updated.
		/// </summary>
		[Test]
		public void ProcessVersLine_Custom_ChapterVerseUpdated()
		{
			versification.ProcessVersLine("HAB 1:17 2:20 3:19");

			// Add a custom line that updates the original chapter-verse versification for Habakkuk
			versification.ProcessVersLine("HAB 1:15 2:13 3:22 4:21");

			// Confirm that the custom versification overrides the original
			int bookId = Canon.BookIdToNumber("HAB");
			List<int[]> bookList = versification.bookList();
			Assert.AreEqual(bookList.Count, bookId);
			Assert.AreEqual(bookList[bookId - 1], new int[] { 15, 13, 22, 21 });

			Assert.AreEqual(4, versification.GetLastChapter(bookId),
				"The customized version of HAB should have four chapters");
			Assert.AreEqual(15, versification.GetLastVerse(bookId, 1), "Custom HAB 1 has 15 verses");
			Assert.AreEqual(13, versification.GetLastVerse(bookId, 2), "Custom HAB 2 has 13 verses");
			Assert.AreEqual(22, versification.GetLastVerse(bookId, 3), "Custom HAB 3 has 22 verses");
			Assert.AreEqual(21, versification.GetLastVerse(bookId, 4), "Custom HAB 4 has 21 verses");
		}

		/// <summary>
		/// Customize a versification scheme when verses for a single chapter of a book are updated.
		/// </summary>
		[Test]
		public void ProcessVersLine_Custom_SingleChapter()
		{
			versification.ProcessVersLine("HAB 1:17 2:20 3:19");

			// Add a custom line that updates a single chapter in Habakkuk
			versification.ProcessVersLine("HAB 2:55");

			// Confirm that the custom versification overrides the original
			int bookId = Canon.BookIdToNumber("HAB");
			List<int[]> bookList = versification.bookList();
			Assert.AreEqual(bookList.Count, bookId);
			Assert.AreEqual(bookList[bookId - 1], new int[] { 17, 55, 19 });

			Assert.AreEqual(3, versification.GetLastChapter(bookId),
				"The customized version of HAB should have three chapters");
			Assert.AreEqual(17, versification.GetLastVerse(bookId, 1), "Custom HAB 1 has 17 verses");
			Assert.AreEqual(55, versification.GetLastVerse(bookId, 2), "Custom HAB 2 has 55 verses");
			Assert.AreEqual(19, versification.GetLastVerse(bookId, 3), "Custom HAB 3 has 19 verses");
		}

		/// <summary>
		/// Customize a versification scheme when verses for a single chapter of a book are updated.
		/// However, the chapter is not in the original versification for the book.
		/// </summary>
		[Test]
		public void ProcessVersLine_Custom_ChapterNotInOriginal()
		{
			versification.ProcessVersLine("HAB 1:17 2:20 3:19");

			// Add a custom line that updates the original chapter-verse versification for Habakkuk
			versification.ProcessVersLine("HAB 7:55");

			// Confirm that the custom versification overrides the original
			int bookId = Canon.BookIdToNumber("HAB");
			List<int[]> bookList = versification.bookList();
			Assert.AreEqual(bookList.Count, bookId);
			Assert.AreEqual(bookList[bookId - 1], new int[] { 17, 20, 19, 1, 1, 1, 55 });

			Assert.AreEqual(7, versification.GetLastChapter(bookId));
			Assert.AreEqual(17, versification.GetLastVerse(bookId, 1));
			Assert.AreEqual(20, versification.GetLastVerse(bookId, 2));
			Assert.AreEqual(19, versification.GetLastVerse(bookId, 3));
			Assert.AreEqual(1, versification.GetLastVerse(bookId, 4));
			Assert.AreEqual(1, versification.GetLastVerse(bookId, 5));
			Assert.AreEqual(1, versification.GetLastVerse(bookId, 6));
			Assert.AreEqual(55, versification.GetLastVerse(bookId, 7));
		}

		/// <summary>
		/// Customize a versification scheme when the chapters/verses for a book is updated.
		/// </summary>
		[Test]
		public void ProcessVersLine_Custom_ChapterRemoved()
		{
			versification.ProcessVersLine("HAB 1:17 2:20 3:19");

			// Add a custom line that updates the original chapter-verse versification for Habakkuk
			versification.ProcessVersLine("HAB 2:0");

			// Confirm that the custom versification overrides the original
			int bookId = Canon.BookIdToNumber("HAB");
			List<int[]> bookList = versification.bookList();
			Assert.AreEqual(bookList.Count, bookId);
			Assert.AreEqual(bookList[bookId - 1], new int[] { 17, 0, 19 });

			Assert.AreEqual(3, versification.GetLastChapter(bookId),
				"The customized version of HAB should still have three chapters");
			Assert.AreEqual(17, versification.GetLastVerse(bookId, 1));
			Assert.AreEqual(0, versification.GetLastVerse(bookId, 2));
			Assert.AreEqual(19, versification.GetLastVerse(bookId, 3));
		}

		/// <summary>
		/// Customize a versification scheme when the verse mapping for a book is updated.
		/// </summary>
		[Test]
		public void ProcessVersLine_Custom_MappingUpdated1()
		{
			versification.ProcessVersLine("PSA 119:1-15 = PSA 119:3-18");

			// Add a custom line that updates the original verse mapping
			ScrVersReflectionHelper.Merge(() =>
			{
				versification.ProcessVersLine("PSA 119:1 = PSA 119:1");
				// no longer map PSA 119:1 to PSA 119:3
				versification.ProcessVersLine("PSA 119:2-16 = PSA 119:20-34");
			});

			// Get mapping from "PSA 119:2-16 = PSA 119:20-34" in the versification
			VerseRef vref;
			for (int i = 1; i <= 16; i++)
			{
				vref = new VerseRef(19, 119, i, versification);
				ScrVers.Original.ChangeVersification(ref vref);
				if (i == 1)
				{
					Assert.AreEqual(new VerseRef(19, 119, 1, ScrVers.Original), vref);
					continue;
				}
				Assert.AreEqual(new VerseRef(19, 119, i + 18, ScrVers.Original), vref);
			}

			vref = new VerseRef(19, 119, 1, ScrVers.Original);
			versification.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef(19, 119, 1, versification), vref);

			for (int i = 20; i <= 34; i++)
			{
				vref = new VerseRef(19, 119, i, ScrVers.Original);
				versification.ChangeVersification(ref vref);
				Assert.AreEqual(new VerseRef(19, 119, i - 18, versification), vref);
			}
		}

		/// <summary>
		/// Customize a versification scheme when the verse mapping for a book is updated.
		/// </summary>
		[Test]
		public void ProcessVersLine_Custom_MappingUpdated2()
		{
			versification.ProcessVersLine("PSA 78:3-18 = PSA 78:1-15");

			// Add a custom line that updates the original verse mapping
			ScrVersReflectionHelper.Merge(() =>
			{
				versification.ProcessVersLine("PSA 78:1 = PSA 78:1"); // no longer map PSA 78:1 to PSA 78:3
				versification.ProcessVersLine("PSA 78:20-34 = PSA 78:2-16 ");
			});

			// Get mapping from "PSA 78:20-34 = PSA 78:2-16" in the versification
			VerseRef vref = new VerseRef(19, 78, 1, versification);
			ScrVers.Original.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef(19, 78, 1, ScrVers.Original), vref);

			for (int i = 20; i <= 34; i++)
			{
				vref = new VerseRef(19, 78, i, versification);
				ScrVers.Original.ChangeVersification(ref vref);
				Assert.AreEqual(new VerseRef(19, 78, i - 18, ScrVers.Original), vref);
			}

			for (int i = 1; i <= 16; i++)
			{
				vref = new VerseRef(19, 78, i, ScrVers.Original);
				versification.ChangeVersification(ref vref);
				if (i == 1)
				{
					Assert.AreEqual(new VerseRef(19, 78, 1, versification), vref);
					continue;
				}
				Assert.AreEqual(new VerseRef(19, 78, i + 18, versification), vref);
			}
		}

		/// <summary>
		/// Customize a versification scheme when the segments on a verse are updated.
		/// </summary>
		[Test]
		public void ProcessVersLine_Custom_VerseSegments()
		{
			versification.ProcessVersLine("#! *HAG 1:5,-,a,b,c");

			// Add a custom line that updates the original verse segments
			ScrVersReflectionHelper.Merge(() => versification.ProcessVersLine("#! *HAG 1:5,d,e,f"));

			// Confirm that the expected segments are added to the specified verse.
			CheckSegments(versification, 1, new VerseRef("HAG 1:5"),
				new[] { "d", "e", "f" });

			Assert.AreEqual(versification.VerseSegments(new VerseRef("HAG 1:5").BBBCCCVVV),
				new[] { "d", "e", "f" });

		}

		/// <summary>
		/// Verify that invalid verse segment is rejected
		/// </summary>
		[Test]
		public void ProcessVersLine_Custom_VerseSegments_Invalid()
		{
			VerifyThrows(() => versification.ParseVerseSegmentsLine("*HAG 1:5,a,b,c,-"),
				VersificationLoadErrorType.UnspecifiedSegmentLocation, "*HAG 1:5,a,b,c,-", "testfile");

			try
			{
				versification.ProcessVersLine("#! *HAG 1:5,a,b,c,-");
			}
			catch (Exception e)
			{
				Assert.IsTrue(e.InnerException is InvalidVersificationLineException, "Didn't get the expected exception");
			}
		}
		#endregion

		#region ParseMappingLine tests
		/// <summary>
		/// Tests the method ChangeVersification when the versifications only differ by customization 
		/// (one is the base versification of another customized versification).
		/// See FB-17661
		/// </summary>
		[Test]
		public void ParseMappingLine_CompareBaseToCustomized()
		{
			Versification vers2 = ScrVersReflectionHelper.CreateClonedVers(versification.VersInfo,
				versification.Name + "-monkey");
			ScrVers versification2 = new ScrVers(vers2);

			versification.ParseChapterVerseLine(
				"ACT 1:26 2:47 3:26 4:37 5:42 6:15 7:60 8:40 9:43 10:48 11:30 12:25 13:52 14:28 15:41 16:40 17:34 18:28 19:41 20:38 21:40 22:30 23:35 24:27 25:27 26:32 27:44 28:31");
			versification2.ParseChapterVerseLine(
				"ACT 1:26 2:47 3:26 4:37 5:42 6:15 7:60 8:40 9:43 10:48 11:30 12:25 13:52 14:28 15:41 16:40 17:34 18:28 19:41 20:38 21:40 22:30 23:35 24:27 25:27 26:32 27:44 28:31");

			versification2.ParseMappingLine("ACT 19:41 = ACT 19:40");
			versification.ParseMappingLine("ACT 19:41 = ACT 19:40");

			// Even tho we have both vers 40 and 41 mapped to the same verse, doing a conversion between the
			// two versification should not cause the original distinction to be lost if both versifications are
			// based on the same original versification.

			VerseRef vref = new VerseRef("ACT 19:40", versification);
			versification2.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:40", versification2), vref);

			vref = new VerseRef("ACT 19:41", versification);
			versification2.ChangeVersification(ref vref);
			Assert.AreEqual(new VerseRef("ACT 19:41", versification2), vref);
		}
		#endregion

		#region RemoveAllUnknownVersifications tests
		[Test]
		public void RemoveAllUnknownVersifications()
		{
			Assert.IsFalse(Versification.Table.Implementation.Exists("ModifiedRuth"));

			using (TextReader reader = new StringReader("RUT 5:91"))
				Versification.Table.Implementation.Load(reader, "", ScrVers.English, "ModifiedRuth");

			Assert.IsTrue(Versification.Table.Implementation.Exists("ModifiedRuth"));

			ScrVers modRuth = new ScrVers("ModifiedRuth");
			Assert.AreEqual(5, modRuth.GetLastChapter(Canon.BookIdToNumber("RUT")));
			Assert.AreEqual(91, modRuth.GetLastVerse(Canon.BookIdToNumber("RUT"), 5));

			Versification.Table.Implementation.RemoveAllUnknownVersifications();

			Assert.IsFalse(Versification.Table.Implementation.Exists("ModifiedRuth"));

			modRuth = new ScrVers("ModifiedRuth");
			Assert.AreEqual(4, modRuth.GetLastChapter(Canon.BookIdToNumber("RUT")));
			Assert.AreEqual(22, modRuth.GetLastVerse(Canon.BookIdToNumber("RUT"), 4));
		}
		#endregion

		#region WriteToStream test
		/// <summary>
		/// Tests the WriteVersification method
		/// </summary>
		[Test]
		public void WriteVersification()
		{
			versification.ParseChapterVerseLine("GEN 1:20 2:70 3:3 4:5");
			versification.ParseChapterVerseLine("LEV 1:2 2:4 3:2");
			versification.ParseChapterVerseLine("DEU 1:16 2:22 3:1 4:18 5:22");

			versification.ParseMappingLine("GEN 1:5 = GEN 1:4");
			versification.ParseMappingLine("GEN 2:69-70 = GEN 4:1-2");
			versification.ParseMappingLine("GEN 3:1 = GEN 4:3");
			versification.ParseMappingLine("LEV 1:1-2 = LEV 1:3-4");

			versification.ParseExcludedVerseLine("-GEN 1:15");

			versification.ParseVerseSegmentsLine("*GEN 1:19,-,a,b,c,d");
			versification.ParseVerseSegmentsLine("*DEU 2:12,a,b,c,d,e,f");

			StringBuilder builder = new StringBuilder();
			using (StringWriter stream = new StringWriter(builder))
				versification.WriteToStream(stream);

			Assert.AreEqual("# List of books, chapters, verses" + Environment.NewLine +
				"# One line per book." + Environment.NewLine +
				"# One entry for each chapter." + Environment.NewLine +
				"# Verse number is the maximum verse number for that chapter." + Environment.NewLine +
				"GEN 1:20 2:70 3:3 4:5" + Environment.NewLine +
				"EXO 1:1" + Environment.NewLine + // EXO gets created automatically
				"LEV 1:2 2:4 3:2" + Environment.NewLine +
				"NUM 1:1" + Environment.NewLine + // NUM gets created automatically
				"DEU 1:16 2:22 3:1 4:18 5:22" + Environment.NewLine +
				"#" + Environment.NewLine +
				"# Mappings from this versification to standard versification" + Environment.NewLine +
				"GEN 1:5 = GEN 1:4" + Environment.NewLine +
				"GEN 2:69-70 = GEN 4:1-2" + Environment.NewLine +
				"GEN 3:1 = GEN 4:3" + Environment.NewLine +
				"LEV 1:1-2 = LEV 1:3-4" + Environment.NewLine +
				"#" + Environment.NewLine +
				"# Excluded verses" + Environment.NewLine +
				"#! -GEN 1:15" + Environment.NewLine +
				"#" + Environment.NewLine +
				"# Verse segment information" + Environment.NewLine +
				"#! *GEN 1:19,-,a,b,c,d" + Environment.NewLine +
				"#! *DEU 2:12,a,b,c,d,e,f" + Environment.NewLine
				, builder.ToString());
		}
		#endregion

		#region Other tests
		/// <summary>
		/// Tests the method GetVersificationType.
		/// </summary>
		[Test]
		public void GetVersificationType()
		{
			// Confirm that 0 is returned for unknown versifications.
			Assert.AreEqual(ScrVersType.Unknown, Versification.Table.GetVersificationType("Unknown"));
			Assert.AreEqual(ScrVersType.Unknown, Versification.Table.GetVersificationType("Esperanto"));
			Assert.AreEqual(ScrVersType.Unknown, Versification.Table.GetVersificationType("Other25"));

			// Confirm that the correct code is returned for valid versifications
			Assert.AreEqual(ScrVersType.Original, Versification.Table.GetVersificationType("Original"));
			Assert.AreEqual(ScrVersType.Septuagint, Versification.Table.GetVersificationType("Septuagint"));
			Assert.AreEqual(ScrVersType.Vulgate, Versification.Table.GetVersificationType("Vulgate"));
			Assert.AreEqual(ScrVersType.English, Versification.Table.GetVersificationType("English"));
		}

		[Test]
		public void TypeProperty_ReturnsCorrectType()
		{
			Assert.AreEqual(ScrVersType.Original, ScrVers.Original.Type);
			Assert.AreEqual(ScrVersType.English, ScrVers.English.Type);
			Assert.AreEqual(ScrVersType.Septuagint, ScrVers.Septuagint.Type);
			Assert.AreEqual(ScrVersType.Vulgate, ScrVers.Vulgate.Type);
			Assert.AreEqual(ScrVersType.RussianOrthodox, ScrVers.RussianOrthodox.Type);
			Assert.AreEqual(ScrVersType.RussianProtestant, ScrVers.RussianProtestant.Type);
			Assert.AreEqual(ScrVersType.Unknown, new ScrVers("random").Type);
		}

		/// <summary>
		/// Test that we correctly skip over a bunch of 0 verse chapters and a lone non-zero verse chapter
		/// </summary>
		[Test]
		public void FirstIncludedVerse()
		{
			versification.ParseChapterVerseLine("GEN 51:0 52:0 53:10");
			Assert.AreEqual(53, ((VerseRef)versification.FirstIncludedVerse(1, 51)).ChapterNum);
		}

		[Test]
		public void UnknownVersification_IsNotPresent_HasCorrectName()
		{
			ScrVers vers = new ScrVers("Blah");
			Assert.AreEqual("Blah", vers.Name);
			Assert.IsFalse(vers.IsPresent);
		}

		[Test]
		public void Get_BuiltInVersificationsCached()
		{
			ScrVers english1 = ScrVers.English;
			ScrVers english2 = new ScrVers(ScrVersType.English);
			ScrVers english3 = new ScrVers("English");
			Assert.IsTrue(ReferenceEquals(english1.VersInfo, english2.VersInfo));
			Assert.IsTrue(ReferenceEquals(english2.VersInfo, english3.VersInfo));
		}

		[Test]
		public void Get_UnknownVersificationsCached()
		{
			ScrVers monkey1 = new ScrVers("Monkey");
			ScrVers monkey2 = new ScrVers("Monkey");
			Assert.IsTrue(ReferenceEquals(monkey1.VersInfo, monkey2.VersInfo));
		}

		[Test]
		public void Get_NumberValueAllowedAsName()
		{
			ScrVers number2 = new ScrVers("2");
			Assert.AreEqual(ScrVersType.Unknown, number2.Type);
			Assert.AreEqual("2", number2.Name);

			ScrVers number27 = new ScrVers("27");
			Assert.AreEqual(ScrVersType.Unknown, number27.Type);
			Assert.AreEqual("27", number27.Name);
		}

		[Test]
		public void VersificationTables_AlwaysIncludesAllBuiltInVersifications()
		{
			List<ScrVers> foundVersifications = Versification.Table.Implementation.VersificationTables().ToList();
			Assert.AreEqual(7, foundVersifications.Count);
			Assert.IsTrue(foundVersifications.Contains(versification));
			Assert.IsTrue(foundVersifications.Contains(ScrVers.English), "missing English");
			Assert.IsTrue(foundVersifications.Contains(ScrVers.Original), "missing Original");
			Assert.IsTrue(foundVersifications.Contains(ScrVers.RussianOrthodox), "missing RussianOrthodox");
			Assert.IsTrue(foundVersifications.Contains(ScrVers.Septuagint), "missing Septuagint");
			Assert.IsTrue(foundVersifications.Contains(ScrVers.Vulgate), "missing Vulgate");
			Assert.IsTrue(foundVersifications.Contains(ScrVers.RussianProtestant), "missing RussianProtestant");
		}
		#endregion

		#region Private helper methods
		/// <summary>
		/// Get verse segment information.
		/// </summary>
		private static void CheckSegments(ScrVers scrVers, int numSegs, VerseRef verseRef, string[] expectedSegs)
		{
			Dictionary<int, string[]> verseSegments = scrVers.verseSegments();
			Assert.IsNotNull(verseSegments);
			Assert.AreEqual(numSegs, verseSegments.Count);
			Assert.IsTrue(verseSegments.ContainsKey(verseRef.BBBCCCVVV));

			Assert.AreEqual(expectedSegs.Length, verseSegments[verseRef.BBBCCCVVV].Length, "Unexpected number of segments");
			for (int iSeg = 0; iSeg < expectedSegs.Length; iSeg++)
			{
				Assert.AreEqual(expectedSegs[iSeg], verseSegments[verseRef.BBBCCCVVV][iSeg],
					"Segment " + iSeg + 1 + " should have been " + expectedSegs[iSeg] + " but was " +
					verseSegments[verseRef.BBBCCCVVV][iSeg]);
			}
		}

		/// <summary>
		/// Verify that an exception is thrown with the correct exception type and message.
		/// </summary>
		private static void VerifyThrows(Action action, VersificationLoadErrorType type, string lineText, string fileName)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				InvalidVersificationLineException ex = e.InnerException as InvalidVersificationLineException;
				Assert.IsNotNull(ex);
				Assert.AreEqual(type, ex.Type);
				Assert.AreEqual(lineText, ex.LineText);
				Assert.AreEqual(fileName, ex.FileName);
				return;
			}
			Assert.Fail("No exception when expected");
		}
		#endregion
	}
}
