using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;
using SIL.Xml;

namespace SIL.Scripture.Tests
{
	/// <summary>
	/// Text fixture for testing the VerseRef class
	/// </summary>
	[TestFixture]
	public class VerseRefTests
	{
		private const char rtlMarker = '\u200F';

		private ScrVers versification;

		#region Test setup/teardown
		[SetUp]
		public void Setup()
		{
			versification = new ScrVers("Dummy"); // Defaults to the eng.vrs file but without a common name
		}

		[TearDown]
		public void TearDown()
		{
			
			versification.ClearExcludedVerses();
			versification.ClearVerseSegments();
		}
		#endregion

		#region Construction/Initialization tests
		/// <summary>
		/// Test the constructors of VerseRef
		/// </summary>
		[Test]
		public void TestConstructors()
		{
			VerseRef vref = new VerseRef(1, 2, 3, ScrVers.Septuagint);
			Assert.IsTrue(vref.Valid);
			Assert.AreEqual(001002003, vref.BBBCCCVVV);
			Assert.AreEqual("001002003", vref.BBBCCCVVVS);
			Assert.AreEqual(1, vref.BookNum);
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual(2, vref.ChapterNum);
			Assert.AreEqual("2", vref.Chapter);
			Assert.AreEqual(3, vref.VerseNum);
			Assert.AreEqual(ScrVers.Septuagint, vref.Versification);

			vref = new VerseRef(4, 5, 6);
			Assert.AreEqual(004005006, vref.BBBCCCVVV);
			Assert.AreEqual("004005006", vref.BBBCCCVVVS);
			Assert.AreEqual(4, vref.BookNum);
			Assert.AreEqual("NUM", vref.Book);
			Assert.AreEqual(5, vref.ChapterNum);
			Assert.AreEqual(6, vref.VerseNum);
			Assert.AreEqual(VerseRef.defaultVersification, vref.Versification);

			vref = new VerseRef();
			Assert.IsTrue(vref.IsDefault);
			Assert.IsFalse(vref.Valid);
			Assert.AreEqual(000000000, vref.BBBCCCVVV);
			Assert.AreEqual("000000000", vref.BBBCCCVVVS);
			Assert.AreEqual(0, vref.BookNum);
			Assert.AreEqual(string.Empty, vref.Book);
			Assert.AreEqual(0, vref.ChapterNum);
			Assert.AreEqual(string.Empty, vref.Chapter);
			Assert.AreEqual(0, vref.VerseNum);
			Assert.AreEqual(string.Empty, vref.Verse);
			Assert.AreEqual(null, vref.Versification);

			vref = new VerseRef("LUK", "3", "4b-5a", ScrVers.Vulgate);
			Assert.IsTrue(vref.Valid);
			Assert.AreEqual(042003004, vref.BBBCCCVVV);
			Assert.AreEqual("042003004b", vref.BBBCCCVVVS);
			Assert.AreEqual(42, vref.BookNum);
			Assert.AreEqual(3, vref.ChapterNum);
			Assert.AreEqual(4, vref.VerseNum);
			Assert.AreEqual("4b-5a", vref.Verse);
			Assert.AreEqual("b", vref.Segment(null));
			Assert.AreEqual(2, vref.AllVerses().Count());
			Assert.AreEqual(ScrVers.Vulgate, vref.Versification);

			// Confirm RTL marker is removed
			vref = new VerseRef("LUK", "3", "4b" + rtlMarker + "-5a", ScrVers.Vulgate);
			Assert.IsTrue(vref.Valid);
			Assert.AreEqual(042003004, vref.BBBCCCVVV);
			Assert.AreEqual("042003004b", vref.BBBCCCVVVS);
			Assert.AreEqual(42, vref.BookNum);
			Assert.AreEqual(3, vref.ChapterNum);
			Assert.AreEqual(4, vref.VerseNum);
			Assert.AreEqual("4b-5a", vref.Verse);
			Assert.AreEqual("b", vref.Segment(null));
			Assert.AreEqual(2, vref.AllVerses().Count());
			Assert.AreEqual(ScrVers.Vulgate, vref.Versification);

			vref = new VerseRef(new VerseRef("LUK 3:4b-5a", ScrVers.Vulgate));
			Assert.IsTrue(vref.Valid);
			Assert.AreEqual(042003004, vref.BBBCCCVVV);
			Assert.AreEqual("042003004b", vref.BBBCCCVVVS);
			Assert.AreEqual(42, vref.BookNum);
			Assert.AreEqual(3, vref.ChapterNum);
			Assert.AreEqual(4, vref.VerseNum);
			Assert.AreEqual("4b-5a", vref.Verse);
			Assert.AreEqual("b", vref.Segment(null));
			Assert.AreEqual(2, vref.AllVerses().Count());
			Assert.AreEqual(ScrVers.Vulgate, vref.Versification);

			// Confirm RTL marker is removed
			vref = new VerseRef(new VerseRef("LUK 3" + rtlMarker + ":4" + rtlMarker + "-5", ScrVers.Vulgate));
			Assert.IsTrue(vref.Valid);
			Assert.AreEqual(042003004, vref.BBBCCCVVV);
			Assert.AreEqual("042003004", vref.BBBCCCVVVS);
			Assert.AreEqual(42, vref.BookNum);
			Assert.AreEqual(3, vref.ChapterNum);
			Assert.AreEqual(4, vref.VerseNum);
			Assert.AreEqual("4-5", vref.Verse);
			Assert.AreEqual("", vref.Segment(null));
			Assert.AreEqual(2, vref.AllVerses().Count());
			Assert.AreEqual(ScrVers.Vulgate, vref.Versification);

			vref = new VerseRef(012015013);
			Assert.AreEqual(012015013, vref.BBBCCCVVV);
			Assert.AreEqual("012015013", vref.BBBCCCVVVS);
			Assert.AreEqual("2KI", vref.Book);
			Assert.AreEqual(12, vref.BookNum);
			Assert.AreEqual(15, vref.ChapterNum);
			Assert.AreEqual(13, vref.VerseNum);
			Assert.AreEqual("13", vref.Verse);
			Assert.AreEqual(VerseRef.defaultVersification, vref.Versification);
		}

		/// <summary>
		/// Tests creating an invalid reference by passing empty strings for the chapter/verse values
		/// and by setting the values explicitly.
		/// </summary>
		[Test]
		public void ChapterAndVerseAsEmptyStrings()
		{
			VerseRef vref = new VerseRef("LUK", "", "", ScrVers.Septuagint);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, vref.ValidStatus);
			Assert.AreEqual("LUK", vref.Book);
			Assert.AreEqual("", vref.Chapter);
			Assert.AreEqual("", vref.Verse);
			Assert.AreEqual(42, vref.BookNum);
			Assert.AreEqual(-1, vref.ChapterNum);
			Assert.AreEqual(-1, vref.VerseNum);

			vref = new VerseRef("LUK", "5", "3", ScrVers.Septuagint);
			vref.Verse = "";
			vref.Chapter = "";
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, vref.ValidStatus);
			Assert.AreEqual("", vref.Chapter);
			Assert.AreEqual("", vref.Verse);
			Assert.AreEqual(-1, vref.ChapterNum);
			Assert.AreEqual(-1, vref.VerseNum);
		}

		/// <summary>
		/// Tests creating references where the strings contain RTL markers.
		/// </summary>
		[Test]
		public void VerseWithRtlMarkers()
		{
			VerseRef vref = new VerseRef("LUK", "5", "1" + rtlMarker + "-2", ScrVers.Septuagint);
			Assert.AreEqual(VerseRef.ValidStatusType.Valid, vref.ValidStatus);
			Assert.AreEqual("LUK", vref.Book);
			Assert.AreEqual("5", vref.Chapter);
			Assert.AreEqual("1-2", vref.Verse);
			Assert.AreEqual(42, vref.BookNum);
			Assert.AreEqual(5, vref.ChapterNum);
			Assert.AreEqual(1, vref.VerseNum);
		}

		/// <summary>
		/// Test building a VerseRef by setting individual properties.
		/// </summary>
		[Test]
		public void BuildVerseRefByProps()
		{
			VerseRef vref = new VerseRef();
			vref.Versification = ScrVers.English;
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, vref.ValidStatus); // 0 not allowed for chapter
			Assert.AreEqual(000000000, vref.BBBCCCVVV);

			vref.BookNum = 13;
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, vref.ValidStatus); // 0 not allowed for chapter
			Assert.AreEqual(013000000, vref.BBBCCCVVV);
			Assert.AreEqual(13, vref.BookNum);
			Assert.AreEqual(0, vref.ChapterNum);
			Assert.AreEqual(0, vref.VerseNum);

			vref.ChapterNum = 1;
			vref.VerseNum = 0;
			// a zero verse is considered valid for introduction, etc, but only for chapter 1
			Assert.IsTrue(vref.Valid);
			Assert.AreEqual(013001000, vref.BBBCCCVVV);
			Assert.AreEqual(13, vref.BookNum);
			Assert.AreEqual(1, vref.ChapterNum);
			Assert.AreEqual(0, vref.VerseNum);

			vref.ChapterNum = 14;
			vref.VerseNum = 15;
			Assert.IsTrue(vref.Valid);
			Assert.AreEqual(013014015, vref.BBBCCCVVV);
			Assert.AreEqual(13, vref.BookNum);
			Assert.AreEqual(14, vref.ChapterNum);
			Assert.AreEqual(15, vref.VerseNum);

			vref = new VerseRef();
			vref.Versification = ScrVers.English;
			vref.ChapterNum = 16;
			// Invalid because 0 is not valid for the book number
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, vref.ValidStatus);
			Assert.AreEqual(000016000, vref.BBBCCCVVV);
			Assert.AreEqual(0, vref.BookNum);
			Assert.AreEqual(16, vref.ChapterNum);
			Assert.AreEqual(0, vref.VerseNum);

			vref = new VerseRef();
			vref.Versification = ScrVers.English;
			vref.VerseNum = 17;
			// Invalid because 0 is not valid for the book and chapter numbers
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, vref.ValidStatus);
			Assert.AreEqual(000000017, vref.BBBCCCVVV);
			Assert.AreEqual(0, vref.BookNum);
			Assert.AreEqual(0, vref.ChapterNum);
			Assert.AreEqual(17, vref.VerseNum);
		}

		/// <summary>
		/// Tests that passing in invalid book, chapter, and/or verse numbers create
		/// a nice invalid reference.
		/// </summary>
		[Test]
		public void Invalid()
		{
			Assert.Throws<VerseRefException>(() => new VerseRef(-1, 1, 1));
			Assert.Throws<VerseRefException>(() => new VerseRef(0, 1, 1));
			Assert.Throws<VerseRefException>(() => new VerseRef(Canon.LastBook + 1, 1, 1));
			Assert.Throws<VerseRefException>(() => new VerseRef(2, -42, 1));
			Assert.Throws<VerseRefException>(() => new VerseRef(2, 1, -4));
			Assert.Throws<VerseRefException>(() => new VerseRef("MAT 1:"));
			Assert.Throws<VerseRefException>(() => new VerseRef("MAT 1:2-"));
			Assert.Throws<VerseRefException>(() => new VerseRef("MAT 1:2,"));

			VerseRef scrRef = new VerseRef(1, 1023, 5051, ScrVers.English);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, scrRef.ValidStatus);
			Assert.AreEqual(1, scrRef.BookNum);
			Assert.AreEqual(1023, scrRef.ChapterNum);
			Assert.AreEqual(5051, scrRef.VerseNum);

			scrRef = new VerseRef("GEN", "F", "@", ScrVers.English);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, scrRef.ValidStatus);
			Assert.AreEqual(1, scrRef.BookNum);
			Assert.AreEqual(-1, scrRef.ChapterNum);
			Assert.AreEqual(-1, scrRef.VerseNum);
		}

		/// <summary>
		/// Basic coverage test of segment parsing.
		/// </summary>
		[Test]
		public void TestSegments()
		{
			StringAssert.AreEqualIgnoringCase("040003013", new VerseRef("MAT 3:13").BBBCCCVVVS);
			StringAssert.AreEqualIgnoringCase("040003012a", new VerseRef("MAT 3:12a").BBBCCCVVVS);
			StringAssert.AreEqualIgnoringCase("011002035a", new VerseRef("1KI 2:35a-35h").BBBCCCVVVS);
			StringAssert.AreEqualIgnoringCase("069008008a", new VerseRef("ESG 8:8a").BBBCCCVVVS);
			StringAssert.AreEqualIgnoringCase("040012001", new VerseRef("MAT 12:1-3,5a,6c-9").BBBCCCVVVS);
			StringAssert.AreEqualIgnoringCase("040003013b", new VerseRef("MAT 3:13b-12a").BBBCCCVVVS);
		}
		#endregion

		#region Valid/ValidStatus tests
		/// <summary>
		/// Tests for valid references.
		/// </summary>
		[Test]
		public void Valid()
		{
			Assert.IsTrue(new VerseRef("GEN 1:1", ScrVers.English).Valid);
			Assert.IsTrue(new VerseRef("GEN 1:1-2", ScrVers.English).Valid);
			Assert.IsTrue(new VerseRef("GEN 1:1,3", ScrVers.English).Valid);
			Assert.IsTrue(new VerseRef("GEN 1:1,3,7", ScrVers.English).Valid);
			Assert.IsTrue(new VerseRef("GEN 1:1,3-6", ScrVers.English).Valid);
			Assert.IsTrue(new VerseRef("PSA 119:1,3-6", ScrVers.English).Valid);
		}

		/// <summary>
		/// Tests for valid references that contain segments.
		/// </summary>
		[Test]
		public void Valid_Segments()
		{
			Assert.IsTrue(new VerseRef("GEN 1:1b", ScrVers.English).Valid);
			Assert.IsTrue(new VerseRef("GEN 1:1c-2a", ScrVers.English).Valid);
			Assert.IsTrue(new VerseRef("GEN 1:1a,3b", ScrVers.English).Valid);
			Assert.IsTrue(new VerseRef("GEN 1:1a,3c,7b", ScrVers.English).Valid);
			Assert.IsTrue(new VerseRef("GEN 1:1a,3c-6a", ScrVers.English).Valid);
		}

		/// <summary>
		/// Tests for references that contain out-of-order verses.
		/// </summary>
		[Test]
		public void ValidStatus_InvalidOrder()
		{
			Assert.AreEqual(VerseRef.ValidStatusType.VerseOutOfOrder, new VerseRef("GEN 1:2-1", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.VerseOutOfOrder, new VerseRef("GEN 1:2,1", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.VerseOutOfOrder, new VerseRef("GEN 1:2-3,1", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.VerseOutOfOrder, new VerseRef("GEN 1:5,2-3", ScrVers.English).ValidStatus);
		}

		/// <summary>
		/// Tests for invalid references.
		/// </summary>
		[Test]
		public void ValidStatus_InvalidInVersification()
		{
			// Invalid chapters
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 100:1", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("PHM 2:1", ScrVers.English).ValidStatus);

			// Invalid verses
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:100", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:100-2", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:1-200", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:100,3", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:1,300", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:100,3,7", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:1,300,7", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:1,3,700", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:100,3-6", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:1,300-6", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:1,3-600", ScrVers.English).ValidStatus);
		}

		/// <summary>
		/// Tests for valid references when an excluded verse is specified.
		/// </summary>
		[Test]
		public void Valid_InvalidExcludedInVersification()
		{
			versification.ParseExcludedVerseLine("-GEN 1:30");

			// Valid verses (surrounding excluded verse)
			Assert.IsTrue(new VerseRef("GEN 1:29", versification).Valid);
			Assert.IsTrue(new VerseRef("GEN 1:31", versification).Valid);

			// Invalid (excluded) verse
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:30", versification).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:30,31", versification).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:29,30", versification).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:29-30", versification).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:30-31", versification).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:30b", versification).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:30b-31a", versification).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:29b-30a", versification).ValidStatus);
		}

		/// <summary>
		/// Tests for an excluded verse within a range (which is valid) or explicitly specified
		/// in the verse (which is invalid).
		/// </summary>
		[Test]
		public void Valid_ExcludedVerse()
		{
			versification.ParseExcludedVerseLine("-GEN 2:2");

			// If an excluded verse is within a verse range, it is valid.
			Assert.IsTrue(new VerseRef("GEN 2:1-3", versification).Valid);

			// If an excluded verse is explicitly included in the reference, it is invalid.
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 2:2", versification).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 2:2-3", versification).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 2:1-2", versification).ValidStatus);
		}

		/// <summary>
		/// Tests for invalid references that contain segments.
		/// </summary>
		[Test]
		public void Valid_InvalidVersificationOnSegments()
		{
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:100b", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:1c-200a", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:1a,300b", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:1a,3c,700b", ScrVers.English).ValidStatus);
			Assert.AreEqual(VerseRef.ValidStatusType.OutOfRange, new VerseRef("GEN 1:1a,3c-600a", ScrVers.English).ValidStatus);
		}
		#endregion

		#region Parsing reference string tests
		/// <summary>
		/// Tests the overload of the constructor that takes a reference string
		/// </summary>
		[Test]
		public void ParseRefString_Valid()
		{
			VerseRef reference = new VerseRef("Gen 1:1", ScrVers.English);
			Assert.IsTrue(reference.Valid);
			Assert.AreEqual(001001001, reference.BBBCCCVVV);
		}

		/// <summary>
		/// Tests the overload of the constructor that takes a reference string
		/// </summary>
		[Test]
		public void ParseRefString_Bridge()
		{
			VerseRef reference = new VerseRef("NUM 5:1-5", ScrVers.English);
			Assert.IsTrue(reference.Valid);
			Assert.AreEqual(004005001, reference.BBBCCCVVV);
			Assert.AreEqual("004005001", reference.BBBCCCVVVS);
			Assert.AreEqual("NUM 5:1-5", reference.ToString());
			Assert.AreEqual("NUM 5:1-5/4", reference.ToStringWithVersification());
			Assert.AreEqual(4, reference.BookNum);
			Assert.AreEqual(5, reference.ChapterNum);
			Assert.AreEqual(1, reference.VerseNum);
			Assert.AreEqual("1-5", reference.Verse);
			Assert.AreEqual(ScrVers.English, reference.Versification);
		}

		/// <summary>
		/// Tests the overload of the constructor that takes a reference string
		/// </summary>
		[Test]
		public void ParseRefString_BridgeWithVersification()
		{
			VerseRef reference = new VerseRef("NUM 5:1-5/2", ScrVers.Septuagint);
			Assert.IsTrue(reference.Valid);
			Assert.AreEqual(004005001, reference.BBBCCCVVV);
			Assert.AreEqual("004005001", reference.BBBCCCVVVS);
			Assert.AreEqual("NUM 5:1-5", reference.ToString());
			Assert.AreEqual("NUM 5:1-5/2", reference.ToStringWithVersification());
			Assert.AreEqual(4, reference.BookNum);
			Assert.AreEqual(5, reference.ChapterNum);
			Assert.AreEqual(1, reference.VerseNum);
			Assert.AreEqual("1-5", reference.Verse);
			Assert.AreEqual(ScrVers.Septuagint, reference.Versification);
		}

		/// <summary>
		/// Tests the overload of the constructor that takes a reference string
		/// </summary>
		[Test]
		public void ParseRefString_BookIntro()
		{
			VerseRef reference = new VerseRef("JOS 1:0", ScrVers.English);
			Assert.IsTrue(reference.Valid);
			Assert.AreEqual(006001000, reference.BBBCCCVVV);
		}

		/// <summary>
		/// Tests the overload of the constructor that takes a reference string
		/// </summary>
		[Test]
		public void ParseRefString_ChapterIntro()
		{
			VerseRef reference = new VerseRef("JOS 2:0", ScrVers.English);
			Assert.AreEqual(006002000, reference.BBBCCCVVV);
			Assert.IsTrue(reference.Valid);
		}

		[Test]
		public void ParseRefString_Weird()
		{
			VerseRef vref = new VerseRef("EXO 0:18", ScrVers.English);
			Assert.IsFalse(vref.Valid, "Invalid because 0 is not valid for the chapter number");
			Assert.AreEqual(002000018, vref.BBBCCCVVV);
			Assert.AreEqual(2, vref.BookNum);
			Assert.AreEqual(0, vref.ChapterNum);
			Assert.AreEqual(18, vref.VerseNum);
		}

		/// <summary>
		/// Tests the overload of the constructor that takes a reference string
		/// </summary>
		[Test]
		public void ParseRefString_InvalidBook()
		{
			Assert.Throws<VerseRefException>(() => new VerseRef("BLA 1:1"));
			Assert.Throws<VerseRefException>(() => new VerseRef("BLA", "1", "1", ScrVers.English));
		}

		/// <summary>
		/// Tests the overload of the constructor that takes a reference string
		/// </summary>
		[Test]
		public void ParseRefString_InvalidNumbers()
		{
			Assert.Throws<VerseRefException>(() => new VerseRef("EXO 6:-18"));
			Assert.Throws<VerseRefException>(() => new VerseRef("EXO -1:18"));
		}

		/// <summary>
		/// Tests the overload of the constructor that takes a reference string
		/// </summary>
		[Test]
		public void ParseRefString_Letters()
		{
			Assert.Throws<VerseRefException>(() => new VerseRef("EXO F:18"));
			Assert.Throws<VerseRefException>(() => new VerseRef("EXO 1:F"));
		}
		#endregion

		#region CopyFrom and CopyVerseFrom tests
		/// <summary>
		/// Test the CopyFrom method
		/// </summary>
		[Test]
		public void CopyFrom()
		{
			VerseRef vrefSource = new VerseRef("LUK", "3", "4b-6a", ScrVers.Vulgate);
			VerseRef vrefDest = new VerseRef();
			vrefDest.CopyFrom(vrefSource);
			// Now change the source to ensure that we didn't just make it referentially equal.
			vrefSource.BookNum = 2;
			vrefSource.ChapterNum = 6;
			vrefSource.VerseNum = 9;
			vrefSource.Versification = ScrVers.English;

			Assert.AreEqual("LUK", vrefDest.Book);
			Assert.AreEqual(3, vrefDest.ChapterNum);
			Assert.AreEqual("4b-6a", vrefDest.Verse);
			Assert.AreEqual(4, vrefDest.VerseNum);
			Assert.AreEqual(ScrVers.Vulgate, vrefDest.Versification);
		}

		/// <summary>
		/// Test the CopyVerseFrom method
		/// </summary>
		[Test]
		public void CopyVerseFrom()
		{
			VerseRef vrefSource = new VerseRef("LUK", "3", "4b-6a", ScrVers.Vulgate);
			VerseRef vrefDest = new VerseRef(1, 3, 5, ScrVers.RussianOrthodox);
			vrefDest.CopyVerseFrom(vrefSource);
			// Now change the source to ensure that we didn't just make it referentially equal.
			vrefSource.BookNum = 2;
			vrefSource.ChapterNum = 6;
			vrefSource.VerseNum = 9;
			vrefSource.Versification = ScrVers.English;

			Assert.AreEqual("GEN", vrefDest.Book);
			Assert.AreEqual(3, vrefDest.ChapterNum);
			Assert.AreEqual("4b-6a", vrefDest.Verse);
			Assert.AreEqual(4, vrefDest.VerseNum);
			Assert.AreEqual(ScrVers.RussianOrthodox, vrefDest.Versification);

			// Now test when the source just has a plain verse number (no bridges or segments)
			vrefDest.CopyVerseFrom(vrefSource);
			Assert.AreEqual("GEN", vrefDest.Book);
			Assert.AreEqual(3, vrefDest.ChapterNum);
			Assert.AreEqual("9", vrefDest.Verse);
			Assert.AreEqual(9, vrefDest.VerseNum);
			Assert.AreEqual(ScrVers.RussianOrthodox, vrefDest.Versification);

		}
		#endregion

		#region AllVerses tests
		/// <summary>
		/// Test VerseRef.AllVerses for a verse bridge with segment letters
		/// </summary>
		[Test]
		public void AllVerses_Bridge()
		{
			VerseRef vref = new VerseRef("LUK", "3", "4b-6a", ScrVers.Vulgate);
			Assert.AreEqual(3, vref.AllVerses().Count());
			Assert.AreEqual(new VerseRef("LUK", "3", "4b", ScrVers.Vulgate), vref.AllVerses().ElementAt(0));
			Assert.AreEqual(new VerseRef("LUK", "3", "5", ScrVers.Vulgate), vref.AllVerses().ElementAt(1));
			Assert.AreEqual(new VerseRef("LUK", "3", "6a", ScrVers.Vulgate), vref.AllVerses().ElementAt(2));
		}

		/// <summary>
		/// Test VerseRef.AllVerses for a simple verse
		/// </summary>
		[Test]
		public void AllVerses_SimpleVerse()
		{
			VerseRef vref = new VerseRef("LUK", "3", "12", ScrVers.Vulgate);
			Assert.AreEqual(1, vref.AllVerses().Count());
			Assert.AreEqual(vref, vref.AllVerses().ElementAt(0));
		}

		/// <summary>
		/// Test VerseRef.AllVerses for a verse with a segment letter
		/// </summary>
		[Test]
		public void AllVerses_VerseWithSegment()
		{
			VerseRef vref = new VerseRef("LUK", "3", "12v", ScrVers.Vulgate);
			Assert.AreEqual(1, vref.AllVerses().Count());
			Assert.AreEqual(vref, vref.AllVerses().ElementAt(0));
		}
		#endregion

		#region GetRanges
		[Test]
		public void GetRanges_SingleVerse()
		{
			VerseRef vref = new VerseRef("LUK 3:12", ScrVers.Original);
			List<VerseRef> ranges = vref.GetRanges().ToList();
			Assert.AreEqual(1, ranges.Count);
			Assert.AreEqual(new VerseRef("LUK 3:12", ScrVers.Original), ranges[0]);
		}

		[Test]
		public void GetRanges_SingleRange()
		{
			VerseRef vref = new VerseRef("LUK 3:12-14", ScrVers.Original);
			List<VerseRef> ranges = vref.GetRanges().ToList();
			Assert.AreEqual(1, ranges.Count);
			Assert.AreEqual(new VerseRef("LUK 3:12-14", ScrVers.Original), ranges[0]);
		}

		[Test]
		public void GetRanges_MultipleRanges()
		{
			VerseRef vref = new VerseRef("LUK 3:12-14,16-17", ScrVers.Original);
			List<VerseRef> ranges = vref.GetRanges().ToList();
			Assert.AreEqual(2, ranges.Count);
			Assert.AreEqual(new VerseRef("LUK 3:12-14", ScrVers.Original), ranges[0]);
			Assert.AreEqual(new VerseRef("LUK 3:16-17", ScrVers.Original), ranges[1]);
		}

		[Test]
		public void GetRanges_ComplicatedRanges()
		{
			VerseRef vref = new VerseRef("LUK 3:12-14,16b-17a,18a,19,20", ScrVers.Original);
			List<VerseRef> ranges = vref.GetRanges().ToList();
			Assert.AreEqual(5, ranges.Count);
			Assert.AreEqual(new VerseRef("LUK 3:12-14", ScrVers.Original), ranges[0]);
			Assert.AreEqual(new VerseRef("LUK 3:16b-17a", ScrVers.Original), ranges[1]);
			Assert.AreEqual(new VerseRef("LUK 3:18a", ScrVers.Original), ranges[2]);
			Assert.AreEqual(new VerseRef("LUK 3:19", ScrVers.Original), ranges[3]);
			Assert.AreEqual(new VerseRef("LUK 3:20", ScrVers.Original), ranges[4]);
		}
		#endregion

		#region Equality tests
		/// <summary>
		/// Tests the &lt; operator
		/// </summary>
		[Test]
		public void Less()
		{
			Assert.IsTrue(new VerseRef(1, 1, 1, ScrVers.English) < new VerseRef(2, 1, 1, ScrVers.English));
			Assert.IsFalse(new VerseRef(10, 1, 1, ScrVers.English) < new VerseRef(1, 1, 1, ScrVers.English));
			Assert.IsTrue(new VerseRef("GEN", "1", "1a", ScrVers.English) < new VerseRef("GEN", "1", "1b", ScrVers.English));
			Assert.IsTrue(new VerseRef(1, 1, 1, ScrVers.English) < new VerseRef("GEN", "1", "1a", ScrVers.English));
			Assert.IsFalse(new VerseRef("GEN", "1", "1a", ScrVers.English) < new VerseRef(1, 1, 1, ScrVers.English));
		}

		/// <summary>
		/// Tests the &lt;= operator
		/// </summary>
		[Test]
		public void LessOrEqual()
		{
			Assert.IsTrue(new VerseRef(1, 1, 1, ScrVers.English) <= new VerseRef(2, 1, 1, ScrVers.English));
			Assert.IsFalse(new VerseRef(10, 1, 1, ScrVers.English) <= new VerseRef(1, 1, 1, ScrVers.English));
			Assert.IsTrue(new VerseRef(1, 1, 1, ScrVers.English) <= new VerseRef(1, 1, 1, ScrVers.English));
			Assert.IsTrue(new VerseRef("GEN", "1", "1a", ScrVers.English) <= new VerseRef("GEN", "1", "1b", ScrVers.English));
			Assert.IsTrue(new VerseRef("GEN", "1", "1a", ScrVers.English) <= new VerseRef("GEN", "1", "1a", ScrVers.English));
			Assert.IsTrue(new VerseRef(1, 1, 1, ScrVers.English) <= new VerseRef("GEN", "1", "1a", ScrVers.English));
			Assert.IsFalse(new VerseRef("GEN", "1", "1a", ScrVers.English) <= new VerseRef(1, 1, 1, ScrVers.English));
		}

		/// <summary>
		/// Tests the &gt; operator
		/// </summary>
		[Test]
		public void Greater()
		{
			Assert.IsTrue(new VerseRef(2, 1, 1, ScrVers.English) > new VerseRef(1, 1, 1, ScrVers.English));
			Assert.IsFalse(new VerseRef(1, 1, 1, ScrVers.English) > new VerseRef(10, 1, 1, ScrVers.English));
			Assert.IsTrue(new VerseRef("GEN", "1", "1b", ScrVers.English) > new VerseRef("GEN", "1", "1a", ScrVers.English));
			Assert.IsTrue(new VerseRef("GEN", "1", "1a", ScrVers.English) > new VerseRef(1, 1, 1, ScrVers.English));
			Assert.IsFalse(new VerseRef(1, 1, 1, ScrVers.English) > new VerseRef("GEN", "1", "1a", ScrVers.English));
		}

		/// <summary>
		/// Tests the &gt;= operator
		/// </summary>
		[Test]
		public void GreaterOrEqual()
		{
			Assert.IsTrue(new VerseRef(2, 1, 1, ScrVers.English) >= new VerseRef(1, 1, 1, ScrVers.English));
			Assert.IsFalse(new VerseRef(1, 1, 1, ScrVers.English) >= new VerseRef(10, 1, 1, ScrVers.English));
			Assert.IsTrue(new VerseRef(1, 1, 1, ScrVers.English) >= new VerseRef(1, 1, 1, ScrVers.English));
			Assert.IsTrue(new VerseRef("GEN", "1", "1b", ScrVers.English) >= new VerseRef("GEN", "1", "1a", ScrVers.English));
			Assert.IsTrue(new VerseRef("GEN", "1", "1a", ScrVers.English) >= new VerseRef("GEN", "1", "1a", ScrVers.English));
			Assert.IsTrue(new VerseRef("GEN", "1", "1a", ScrVers.English) >= new VerseRef(1, 1, 1, ScrVers.English));
			Assert.IsFalse(new VerseRef(1, 1, 1, ScrVers.English) >= new VerseRef("GEN", "1", "1a", ScrVers.English));
		}

		/// <summary>
		/// Tests the Equals method operator
		/// </summary>
		[Test]
		public void Equal()
		{
			Assert.IsTrue(new VerseRef(1, 1, 1, ScrVers.English).Equals(new VerseRef(1, 1, 1, ScrVers.English)));
			Assert.IsTrue(new VerseRef("GEN", "1", "1a", ScrVers.English).Equals(new VerseRef("GEN", "1", "1a", ScrVers.English)));
			Assert.IsFalse(new VerseRef("GEN", "1", "1a", ScrVers.English).Equals(new VerseRef("GEN", "1", "1b", ScrVers.English)));
			Assert.IsFalse(new VerseRef("GEN", "1", "1a", ScrVers.English).Equals(new VerseRef(1, 1, 1, ScrVers.English)));
			Assert.IsFalse(new VerseRef("GEN", "1", "1a", ScrVers.English).Equals(01001001));
		}
		#endregion

		#region Versification tests
		/// <summary>
		/// Tests the ChangeVersification method
		/// </summary>
		[Test]
		public void ChangeVersification()
		{
			VerseRef vref = new VerseRef("EXO 6:0", ScrVers.English);
			vref.ChangeVersification(ScrVers.Original);
			Assert.AreEqual(new VerseRef("EXO 6:0", ScrVers.Original), vref);

			vref = new VerseRef("GEN 31:55", ScrVers.English);
			vref.ChangeVersification(ScrVers.Original);
			Assert.AreEqual(new VerseRef("GEN 32:1", ScrVers.Original), vref);

			vref = new VerseRef("ESG 1:2", ScrVers.English);
			vref.ChangeVersification(ScrVers.Septuagint);
			Assert.AreEqual(new VerseRef("ESG 1:1b", ScrVers.Septuagint), vref);

			vref = new VerseRef("ESG 1:1b", ScrVers.Septuagint);
			vref.ChangeVersification(ScrVers.English);
			Assert.AreEqual(new VerseRef("ESG 1:2", ScrVers.English), vref);

			vref = new VerseRef("ESG 1:3", ScrVers.RussianOrthodox);
			vref.ChangeVersification(ScrVers.Septuagint);
			Assert.AreEqual(new VerseRef("ESG 1:1c", ScrVers.Septuagint), vref);

			vref = new VerseRef("ESG 1:1c", ScrVers.Septuagint);
			vref.ChangeVersification(ScrVers.RussianOrthodox);
			Assert.AreEqual(new VerseRef("ESG 1:3", ScrVers.RussianOrthodox), vref);
		}

		/// <summary>
		/// Tests the ChangeVersificationWithRanges method
		/// </summary>
		[Test]
		public void ChangeVersificationWithRanges()
		{
			VerseRef vref = new VerseRef("EXO 6:0", ScrVers.English);
			Assert.IsTrue(vref.ChangeVersificationWithRanges(ScrVers.Original));
			Assert.AreEqual(new VerseRef("EXO 6:0", ScrVers.Original), vref);

			vref = new VerseRef("GEN 31:55", ScrVers.English);
			Assert.IsTrue(vref.ChangeVersificationWithRanges(ScrVers.Original));
			Assert.AreEqual(new VerseRef("GEN 32:1", ScrVers.Original), vref);

			vref = new VerseRef("GEN 32:3-4", ScrVers.English);
			Assert.IsTrue(vref.ChangeVersificationWithRanges(ScrVers.Original));
			Assert.AreEqual(new VerseRef("GEN 32:4-5", ScrVers.Original), vref);

			// This is the case where this can't really work properly:
			vref = new VerseRef("GEN 31:54-55", ScrVers.English);
			Assert.IsFalse(vref.ChangeVersificationWithRanges(ScrVers.Original));
			Assert.AreEqual(new VerseRef("GEN 31:54-1", ScrVers.Original), vref);
		}

		/// <summary>
		/// Tests the LastChapter property.
		/// </summary>
		[Test]
		public void LastChapter()
		{
			Assert.AreEqual(50, new VerseRef(1, 0, 0, ScrVers.English).LastChapter);
			Assert.AreEqual(40, new VerseRef(2, 0, 0, ScrVers.Vulgate).LastChapter);
			Assert.AreEqual(27, new VerseRef(3, 0, 0, ScrVers.Septuagint).LastChapter);
			Assert.AreEqual(36, new VerseRef(4, 0, 0, ScrVers.RussianProtestant).LastChapter);
		}

		/// <summary>
		/// Tests the LastVerse property.
		/// </summary>
		[Test]
		public void LastVerse()
		{
			Assert.AreEqual(31, new VerseRef(1, 1, 1, ScrVers.English).LastVerse);
			Assert.AreEqual(1, new VerseRef(2, 0, 0, ScrVers.Vulgate).LastVerse);
			Assert.AreEqual(27, new VerseRef(66, 21, 20, ScrVers.Septuagint).LastVerse);
			Assert.AreEqual(55, new VerseRef(1, 31, 1, ScrVers.English).LastVerse);
			Assert.AreEqual(54, new VerseRef(1, 31, 1, ScrVers.Original).LastVerse);
		}
		#endregion

		#region Next/Previous Book tests
		/// <summary>
		/// Tests the NextBook method with the default of All Books
		/// </summary>
		[Test]
		public void NextBook()
		{
			VerseRef vref = new VerseRef("EXO 6:0");
			Assert.IsTrue(vref.NextBook());
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);

			vref = new VerseRef("GEN 5:4-7");
			Assert.IsTrue(vref.NextBook());
			Assert.AreEqual("EXO", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);

			vref = new VerseRef("LAO 1:0");
			Assert.IsFalse(vref.NextBook());
			Assert.AreEqual("LAO", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);
		}

		/// <summary>
		/// Tests the NextBook method with a set of books (contrained to the odd-numbered
		/// books in the Torah)
		/// </summary>
		[Test]
		public void NextBook_WithBookSet()
		{
			VerseRef vref = new VerseRef("GEN 39:4");
			Assert.IsTrue(vref.NextBook(new BookSet("10101")));
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);

			vref = new VerseRef("DEU 10:2");
			Assert.IsFalse(vref.NextBook(new BookSet("10101")));
			Assert.AreEqual("DEU", vref.Book);
			Assert.AreEqual("10", vref.Chapter);
			Assert.AreEqual("2", vref.Verse);

			vref = new VerseRef("EXO 2:4");
			Assert.IsTrue(vref.NextBook(new BookSet("10101")));
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);
		}

		/// <summary>
		/// Tests the PreviousBook method with the default of All Books
		/// </summary>
		[Test]
		public void PreviousBook()
		{
			VerseRef vref = new VerseRef("REV 6:5");
			Assert.IsTrue(vref.PreviousBook());
			Assert.AreEqual("JUD", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("EXO 1:4-6");
			Assert.IsTrue(vref.PreviousBook());
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("GEN 1:0");
			Assert.IsFalse(vref.PreviousBook());
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);
		}

		/// <summary>
		/// Tests the PreviousBook method with a set of books (contrained to selected
		/// books in the Torah)
		/// </summary>
		[Test]
		public void PreviousBook_WithBookSet()
		{
			VerseRef vref = new VerseRef("DEU 3:2");
			Assert.IsTrue(vref.PreviousBook(new BookSet("10101")));
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("NUM 1:6");
			Assert.IsFalse(vref.PreviousBook(new BookSet("00011")));
			Assert.AreEqual("NUM", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("6", vref.Verse);

			vref = new VerseRef("EXO 1:6");
			Assert.IsFalse(vref.PreviousBook(new BookSet("00011")));
			Assert.AreEqual("EXO", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("6", vref.Verse);
		}
		#endregion

		#region Next/Previous Chapter tests
		/// <summary>
		/// Tests the NextChapter method with the default of All Books
		/// </summary>
		[Test]
		public void NextChapter()
		{
			VerseRef vref = new VerseRef("EXO 6:0", ScrVers.English);
			Assert.IsTrue(vref.NextChapter());
			Assert.AreEqual("EXO", vref.Book);
			Assert.AreEqual("7", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("JUD 1:3-4", ScrVers.English);
			Assert.IsTrue(vref.NextChapter());
			Assert.AreEqual("REV", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);

			// Have no idea what LAO is, but it's not in the English vrs file,
			// so Paratext considers it to have 1 chapter, and since it's the
			// last book in the Deutoerocanon, we can't advance.
			vref = new VerseRef("LAO 1:0", ScrVers.English);
			Assert.IsFalse(vref.NextChapter());
			Assert.AreEqual("LAO", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);
		}

		/// <summary>
		/// Tests the NextChapter method with a set of books (contrained to the odd-numbered
		/// books in the Torah)
		/// </summary>
		[Test]
		public void NextChapter_WithBookSet()
		{
			VerseRef vref = new VerseRef("GEN 49:4", ScrVers.English);
			Assert.IsTrue(vref.NextChapter(new BookSet("10101")));
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("50", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("GEN 50:14", ScrVers.English);
			Assert.IsTrue(vref.NextChapter(new BookSet("10101")));
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);

			vref = new VerseRef("NUM 3:3", ScrVers.English);
			Assert.IsTrue(vref.NextChapter(new BookSet("10101")));
			Assert.AreEqual("DEU", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);

			vref = new VerseRef("DEU 34:11", ScrVers.English);
			Assert.IsFalse(vref.NextChapter(new BookSet("10101")));
			Assert.AreEqual("DEU", vref.Book);
			Assert.AreEqual("34", vref.Chapter);
			Assert.AreEqual("11", vref.Verse);
		}

		/// <summary>
		/// Tests NextChapter when excluded verses are skipped. 
		/// </summary>
		[Test]
		public void NextChapter_SkipExcluded()
		{
			VerseRef vref = new VerseRef("GEN 1:1", versification);
			versification.ParseExcludedVerseLine("-GEN 2:1");
			Assert.IsTrue(vref.NextChapter(new BookSet("10101"), true));
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("2", vref.Chapter);
			Assert.AreEqual("2", vref.Verse);
		}

		/// <summary>
		/// Tests the PreviousChapter method with the default of All Books
		/// </summary>
		[Test]
		public void PreviousChapter()
		{
			VerseRef vref = new VerseRef("REV 3:2", ScrVers.English);
			Assert.IsTrue(vref.PreviousChapter());
			Assert.AreEqual("REV", vref.Book);
			Assert.AreEqual("2", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("REV 1:1-5", ScrVers.English);
			Assert.IsTrue(vref.PreviousChapter());
			Assert.AreEqual("JUD", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("GEN 1:17", ScrVers.English);
			Assert.IsFalse(vref.PreviousChapter());
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("17", vref.Verse);
		}

		/// <summary>
		/// Tests the PreviousChapter method with a set of books (contrained to selected
		/// books in the Torah)
		/// </summary>
		[Test]
		public void PreviousChapter_WithBookSet()
		{
			VerseRef vref = new VerseRef("DEU 3:2", ScrVers.English);
			Assert.IsTrue(vref.PreviousChapter(new BookSet("10101")));
			Assert.AreEqual("DEU", vref.Book);
			Assert.AreEqual("2", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("DEU 1:5", ScrVers.English);
			Assert.IsTrue(vref.PreviousChapter(new BookSet("10101")));
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("27", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("NUM 10:5", ScrVers.English);
			Assert.IsTrue(vref.PreviousChapter(new BookSet("10101")));
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("27", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("NUM 1:6", ScrVers.English);
			Assert.IsFalse(vref.PreviousChapter(new BookSet("00011")));
			Assert.AreEqual("NUM", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("6", vref.Verse);

			vref = new VerseRef("EXO 1:6", ScrVers.English);
			Assert.IsFalse(vref.PreviousChapter(new BookSet("00011")));
			Assert.AreEqual("EXO", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("6", vref.Verse);
		}
		#endregion

		#region NextVerse tests
		/// <summary>
		/// Tests the NextVerse method with the default of All Books
		/// </summary>
		[Test]
		public void NextVerse()
		{
			VerseRef vref = new VerseRef("EXO 6:0", ScrVers.English);
			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("EXO", vref.Book);
			Assert.AreEqual("6", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("GEN 1:5-7", ScrVers.English);
			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("6", vref.Verse);

			vref = new VerseRef("GEN 31:54", ScrVers.English);
			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("31", vref.Chapter);
			Assert.AreEqual("55", vref.Verse);

			vref = new VerseRef("GEN 31:54", ScrVers.Original);
			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("32", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("NAM 2:13", ScrVers.English);
			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("NAM", vref.Book);
			Assert.AreEqual("3", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("JUD 1:25", ScrVers.English);
			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("REV", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);

			// LAO has been added in the Paratext source ("Paratext/My Paratext Projects") to
			// include LAO having 20 verses. If this test fails here, make sure you update
			// your My Paratext Projects directory with the updated eng.vrs file.
			vref = new VerseRef("LAO 1:20", ScrVers.English);
			Assert.IsFalse(vref.NextVerse());
			Assert.AreEqual("LAO", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("20", vref.Verse);
		}

		/// <summary>
		/// Tests the NextVerse method when segments are defined in the versification for
		/// the next verse.
		/// </summary>
		[Test]
		public void NextVerse_WithSegments()
		{
			// Septuagint has verse segments defined. In this test, 1Kings 5:13 has no segments defined.
			// 1Kings 5:14 has three segments: unmarked, and segments with labels 'a' and 'b'
			VerseRef vref = new VerseRef("1KI 5:13", ScrVers.Septuagint);

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("1KI", vref.Book);
			Assert.AreEqual("5", vref.Chapter);
			Assert.AreEqual("14", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("1KI", vref.Book);
			Assert.AreEqual("5", vref.Chapter);
			Assert.AreEqual("14a", vref.Verse);
			Assert.AreEqual("a", vref.Segment(null));

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("1KI", vref.Book);
			Assert.AreEqual("5", vref.Chapter);
			Assert.AreEqual("14b", vref.Verse);
			Assert.AreEqual("b", vref.Segment(null));

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("1KI", vref.Book);
			Assert.AreEqual("5", vref.Chapter);
			Assert.AreEqual("15", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));
		}

		/// <summary>
		/// Tests the NextVerse method when segments are defined in the versification for
		/// the next verse and the first segment is a letter (rather than empty).
		/// </summary>
		[Test]
		public void NextVerse_WithSegmentsStartWithLetter()
		{
			versification.ParseVerseSegmentsLine("*RUT 1:3,a,b");

			VerseRef vref = new VerseRef("RUT 1:2", versification);

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("RUT", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("3a", vref.Verse);
			Assert.AreEqual("a", vref.Segment(null));

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("RUT", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("3b", vref.Verse);
			Assert.AreEqual("b", vref.Segment(null));

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("RUT", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("4", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));
		}

		/// <summary>
		/// FB33273: Handle case when versification line ends with a comma.
		/// </summary>
		[Test]
		public void NextVerse_SegmentLineEndsWithComma()
		{
			versification.ParseVerseSegmentsLine("*RUT 1:3,a,");

			VerseRef vref = new VerseRef("RUT 1:2", versification);

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("RUT", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("3a", vref.Verse);
			Assert.AreEqual("a", vref.Segment(null));

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("RUT", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("4", vref.Verse);
		}

		/// <summary>
		/// Tests the NextVerse method when identical unmarked segments (i.e. two occurrences of the same verse)
		/// are in a verse.
		/// </summary>
		[Test]
		public void NextVerse_IdenticalSegments()
		{
			// In the Septuagint, LAM 1 has two verse 1 markers.
			VerseRef vref = new VerseRef("LAM 1:1", ScrVers.Septuagint);

			Assert.IsTrue(vref.NextVerse());
			// We just want to go to the next verse because the navigation will be stalled.
			Assert.AreEqual("LAM", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("2", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));
		}

		/// <summary>
		/// Tests the NextVerse method when segments are defined in the versification for
		/// two verses in sequential order.
		/// </summary>
		[Test]
		public void NextVerse_SequentialVersesWithSegments()
		{
			// Add verse segment information to 1Kings 5:16 so that it has two segments with labels 'a' and 'b'
			versification.ParseVerseSegmentsLine("*1KI 5:16,a,b");
			// Add verse segment information to 1Kings 5:17 so that it has two segments with labels 'a' and 'b'
			versification.ParseVerseSegmentsLine("*1KI 5:17,a,b");

			// No verses in the Septuagint have this issue, but this tests the first segment when it has a label.
			// Test in this scenario as well: going from the last labeled segment of one verse to the first
			// labeled segment of the next.
			VerseRef vref = new VerseRef("1KI 5:16b", versification);

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("1KI", vref.Book);
			Assert.AreEqual("5", vref.Chapter);
			Assert.AreEqual("17a", vref.Verse);
			Assert.AreEqual("a", vref.Segment(null));
		}

		/// <summary>
		/// Tests the NextVerse method at a chapter boundary.
		/// </summary>
		[Test]
		public void NextVerse_AtChapterBoundary()
		{
			VerseRef vref = new VerseRef("MAL 1:14", ScrVers.Septuagint);

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("MAL", vref.Book);
			Assert.AreEqual("2", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));
		}

		/// <summary>
		/// Tests the NextVerse method at a chapter boundary when the first verse of the next
		/// chapter has segments.
		/// </summary>
		[Test]
		public void NextVerse_AtChapterBoundarySegmentsAfter()
		{
			// Add verse segment information to the verse after the chapter boundary (Zephaniah 2:1)
			versification.ParseVerseSegmentsLine("*ZEP 2:1,a,b");

			VerseRef vref = new VerseRef("ZEP 1:18", versification);

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("ZEP", vref.Book);
			Assert.AreEqual("2", vref.Chapter);
			Assert.AreEqual("1a", vref.Verse);
			Assert.AreEqual("a", vref.Segment(null));
		}

		/// <summary>
		/// Tests the NextVerse method at a chapter boundary when surrounding verses have verse segments.
		/// </summary>
		[Test]
		public void NextVerse_SegmentedVersesAtChapterBoundary()
		{
			// Add verse segment information to 1Kings 1:53 (last verse of chapter 1)
			versification.ParseVerseSegmentsLine("*1KI 1:53,a,b");
			// Add verse segment information to the following verse 1Kings 2:1
			versification.ParseVerseSegmentsLine("*1KI 2:1,a,b");

			VerseRef vref = new VerseRef("1KI 1:53b", versification);

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("1KI", vref.Book);
			Assert.AreEqual("2", vref.Chapter);
			Assert.AreEqual("1a", vref.Verse);
			Assert.AreEqual("a", vref.Segment(null));
		}

		/// <summary>
		/// Tests the NextVerse method when segments are defined in the versification for
		/// two verses in sequential order at a book boundary.
		/// </summary>
		[Test]
		public void NextVerse_SegmentedVersesAtBookBoundary()
		{
			// Add verse segment information to Ruth 4:2 (last verse of the book of Ruth)
			versification.ParseVerseSegmentsLine("*RUT 4:22,a,b");
			// Add verse segment information to the following verse 1Samuel 1:1
			versification.ParseVerseSegmentsLine("*1SA 1:1,a,b");

			VerseRef vref = new VerseRef("RUT 4:22b", versification);
			
			// By design, the first verse at a book boundary is 0 to include introductory material
			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("1SA", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("1SA", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("1a", vref.Verse);
			Assert.AreEqual("a", vref.Segment(null));
		}

		/// <summary>
		/// Tests the NextVerse method when segments are defined in the versification for
		/// the last verse in a book but not for the starting verse in the next book.
		/// </summary>
		[Test]
		public void NextVerse_AtBookBoundaryNoSegsInVerse1()
		{
			// Add verse segment information to Judges 21:25 (last verse of the book of Judges)
			versification.ParseVerseSegmentsLine("*JDG 21:25,a,b");
			// Do not add any verse segment information to the following verse (Ruth 1:1)

			VerseRef vref = new VerseRef("JDG 21:25b", versification);

			// By design, the first verse at a book boundary is 0 to include introductory material
			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("RUT", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));

			Assert.IsTrue(vref.NextVerse());
			Assert.AreEqual("RUT", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));
		}

		/// <summary>
		/// Tests the NextVerse method with a set of books (contrained to the odd-numbered
		/// books in the Torah)
		/// </summary>
		[Test]
		public void NextVerse_WithBookSet()
		{
			VerseRef vref = new VerseRef("GEN 50:26", ScrVers.English);
			Assert.IsTrue(vref.NextVerse(new BookSet("10101")));
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);

			vref = new VerseRef("EXO 39:42", ScrVers.English);
			Assert.IsTrue(vref.NextVerse(new BookSet("10101")));
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);

			vref = new VerseRef("DEU 34:12", ScrVers.English);
			Assert.IsFalse(vref.NextVerse(new BookSet("10101")));
			Assert.AreEqual("DEU", vref.Book);
			Assert.AreEqual("34", vref.Chapter);
			Assert.AreEqual("12", vref.Verse);
		}
		#endregion

		#region PreviousVerse tests
		/// <summary>
		/// Tests the PreviousVerse method with the default of All Books
		/// </summary>
		[Test]
		public void PreviousVerse()
		{
			VerseRef vref = new VerseRef("REV 3:2", ScrVers.English);
			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("REV", vref.Book);
			Assert.AreEqual("3", vref.Chapter);
			Assert.AreEqual("1", vref.Verse);

			vref = new VerseRef("GEN 1:5-7", ScrVers.English);
			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("4", vref.Verse);

			vref = new VerseRef("GEN 32:1", ScrVers.English);
			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("31", vref.Chapter);
			Assert.AreEqual("55", vref.Verse);

			vref = new VerseRef("GEN 32:1", ScrVers.Original);
			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("31", vref.Chapter);
			Assert.AreEqual("54", vref.Verse);

			vref = new VerseRef("REV 1:0", ScrVers.English);
			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("JUD", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("25", vref.Verse);

			vref = new VerseRef("GEN 1:1", ScrVers.English);
			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);

			vref = new VerseRef("GEN 1:0", ScrVers.English);
			Assert.IsFalse(vref.PreviousVerse());
			Assert.AreEqual("GEN", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);
		}

		/// <summary>
		/// Tests the PreviousVerse method with a set of books (contrained to selected
		/// books in the Torah)
		/// </summary>
		[Test]
		public void PreviousVerse_WithBookSet()
		{
			VerseRef vref = new VerseRef("DEU 3:1-2", ScrVers.English);
			Assert.IsTrue(vref.PreviousVerse(new BookSet("10101")));
			Assert.AreEqual("DEU", vref.Book);
			Assert.AreEqual("2", vref.Chapter);
			Assert.AreEqual("37", vref.Verse);

			vref = new VerseRef("DEU 1:0", ScrVers.English);
			Assert.IsTrue(vref.PreviousVerse(new BookSet("10101")));
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("27", vref.Chapter);
			Assert.AreEqual("34", vref.Verse);

			vref = new VerseRef("NUM 1:4-7", ScrVers.English);
			Assert.IsTrue(vref.PreviousVerse(new BookSet("10101")));
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("27", vref.Chapter);
			Assert.AreEqual("34", vref.Verse);

			vref = new VerseRef("NUM 1:0", ScrVers.English);
			Assert.IsFalse(vref.PreviousVerse(new BookSet("00011")));
			Assert.AreEqual("NUM", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);

			vref = new VerseRef("EXO 1:6", ScrVers.English);
			Assert.IsFalse(vref.PreviousVerse(new BookSet("00011")));
			Assert.AreEqual("EXO", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("6", vref.Verse);
		}

		/// <summary>
		/// Tests the PreviousVerse method when segments are defined in the versification for
		/// a verse.
		/// </summary>
		[Test]
		public void PreviousVerse_WithSegments()
		{
			// Septuagint has verse segments defined. In this test, 1Kings 5:13 and 5:15 have no segments defined.
			// 1Kings 5:14 has three segments: unmarked, and segments with labels 'a' and 'b'
			VerseRef vref = new VerseRef("1KI 5:15", ScrVers.Septuagint);

			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("1KI", vref.Book);
			Assert.AreEqual("5", vref.Chapter);
			Assert.AreEqual("14b", vref.Verse);
			Assert.AreEqual("b", vref.Segment(null));

			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("1KI", vref.Book);
			Assert.AreEqual("5", vref.Chapter);
			Assert.AreEqual("14a", vref.Verse);
			Assert.AreEqual("a", vref.Segment(null));

			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("1KI", vref.Book);
			Assert.AreEqual("5", vref.Chapter);
			Assert.AreEqual("14", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));

			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("1KI", vref.Book);
			Assert.AreEqual("5", vref.Chapter);
			Assert.AreEqual("13", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));
		}

		/// <summary>
		/// Tests the PreviousVerse method when segments are defined in the versification for
		/// two verses in sequential order.
		/// </summary>
		[Test]
		public void PreviousVerse_SequentialVersesWithSegments()
		{
			// Add verse segment information to Habakkuk  so that it has two segments with labels 'a' and 'b'
			versification.ParseVerseSegmentsLine("*HAB 3:17,a,b");
			// Add verse segment information to 1Kings 5:17 so that it has two segments with labels 'a' and 'b'
			versification.ParseVerseSegmentsLine("*HAB 3:18,a,b");

			// No verses in the Septuagint have this issue, but this tests the first segment when it has a label.
			// Test in this scenario as well: going from the first labeled segment of one verse to the last
			// labeled segment of the previous verse.
			VerseRef vref = new VerseRef("HAB 3:18a", versification);

			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("HAB", vref.Book);
			Assert.AreEqual("3", vref.Chapter);
			Assert.AreEqual("17b", vref.Verse);
			Assert.AreEqual("b", vref.Segment(null));

			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("HAB", vref.Book);
			Assert.AreEqual("3", vref.Chapter);
			Assert.AreEqual("17a", vref.Verse);
			Assert.AreEqual("a", vref.Segment(null));

			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("HAB", vref.Book);
			Assert.AreEqual("3", vref.Chapter);
			Assert.AreEqual("16", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));
		}

		/// <summary>
		/// Tests the PreviousVerse method when segments are defined in the versification for
		/// two verses in sequential order at a chapter boundary.
		/// </summary>
		[Test]
		public void PreviousVerse_SegmentedVersesAtChapterBoundary()
		{
			// Add verse segment information to Amos 1:15 (last verse of chapter 1)
			versification.ParseVerseSegmentsLine("*AMO 1:15,a,b");
			// Add verse segment information to the following verse, Amos 2:1
			versification.ParseVerseSegmentsLine("*AMO 2:1,a,b");

			VerseRef vref = new VerseRef("AMO 2:1a", versification);

			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("AMO", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("15b", vref.Verse);
			Assert.AreEqual("b", vref.Segment(null));
		}

		/// <summary>
		/// Tests the PreviousVerse method when segments are defined in the versification for
		/// two segmented verses at a book boundary.
		/// </summary>
		[Test]
		public void PreviousVerse_SegmentedVersesAtBookBoundary()
		{
			// Add verse segment information to Jonah 1:1 
			versification.ParseVerseSegmentsLine("*JON 1:1,a,b");
			// Add verse segment information to the previous verse, Obadiah 1:21
			versification.ParseVerseSegmentsLine("*OBA 1:21,a,b");

			VerseRef vref = new VerseRef("JON 1:1a", versification);

			// By design, the first verse at a book boundary is 0 to include introductory material
			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("JON", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));

			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("OBA", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("21b", vref.Verse);
			Assert.AreEqual("b", vref.Segment(null));
		}

		/// <summary>
		/// Tests the PreviousVerse method when segments are defined in the versification for
		/// the first verse in a book but not for the last verse in the previous book.
		/// </summary>
		[Test]
		public void PreviousVerse_AtBookBoundaryNoSegsInLastVerse()
		{
			// Add verse segment information to Judges 21:25 (last verse of the book of Judges)
			versification.ParseVerseSegmentsLine("*NAM 1:1,a,b");
			// Do not add any verse segment information to the previous verse (Micah 7:20)

			VerseRef vref = new VerseRef("NAM 1:1a", versification);

			// By design, the first verse at a book boundary is 0 to include introductory material
			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("NAM", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("0", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));

			Assert.IsTrue(vref.PreviousVerse());
			Assert.AreEqual("MIC", vref.Book);
			Assert.AreEqual("7", vref.Chapter);
			Assert.AreEqual("20", vref.Verse);
			Assert.AreEqual(string.Empty, vref.Segment(null));
		}
		#endregion

		#region CompareTo tests
		/// <summary>
		/// Tests the CompareTo method when verse bridges are included in one verse but not in another.
		/// </summary>
		[Test]
		public void CompareTo_WithWithoutVerseBridges()
		{
			VerseRef vRefWithOutBridge = new VerseRef(1, 1, 2, versification);
			VerseRef vRefWithBridge = new VerseRef("GEN 1:2-3", versification);

			Assert.Greater(vRefWithBridge.CompareTo(vRefWithOutBridge, true), 0);
			Assert.Less(vRefWithOutBridge.CompareTo(vRefWithBridge, true), 0);
		}

		/// <summary>
		/// Tests the CompareTo method when verse bridges are the same.
		/// </summary>
		[Test]
		public void CompareTo_SameVerseBridge()
		{
			VerseRef vRefWithBridge1 = new VerseRef("GEN 1:1-2", versification);
			VerseRef vRefWithBridge2 = new VerseRef("GEN 1:1-2", versification);

			Assert.AreEqual(0, vRefWithBridge2.CompareTo(vRefWithBridge1, true));
		}

		/// <summary>
		/// Tests the CompareTo method when verse bridges overlap each other but are not the same.
		/// </summary>
		[Test]
		public void CompareTo_OverlappingVerseBridges()
		{
			VerseRef vRefWithBridge1 = new VerseRef("GEN 1:1-2", versification);
			VerseRef vRefWithBridge2 = new VerseRef("GEN 1:2-3", versification);

			Assert.Greater(vRefWithBridge2.CompareTo(vRefWithBridge1, true), 0);
			Assert.Less(vRefWithBridge1.CompareTo(vRefWithBridge2, true), 0);
		}

		/// <summary>
		/// Tests the CompareTo method on verse lists.
		/// </summary>
		[Test]
		public void CompareTo_VerseLists()
		{
			VerseRef vRefWithBridge1 = new VerseRef("GEN 1:2,3,21", versification);
			VerseRef vRefWithBridge2 = new VerseRef("GEN 1:2,21", versification);

			Assert.Greater(vRefWithBridge2.CompareTo(vRefWithBridge1, true), 0);
			Assert.Less(vRefWithBridge1.CompareTo(vRefWithBridge2, true), 0);

			vRefWithBridge1 = new VerseRef("GEN 1:2,3,21", versification);
			vRefWithBridge2 = new VerseRef("GEN 1:2,3", versification);

			Assert.Less(vRefWithBridge2.CompareTo(vRefWithBridge1, true), 0);
			Assert.Greater(vRefWithBridge1.CompareTo(vRefWithBridge2, true), 0);
		}

		/// <summary>
		/// Tests the CompareTo method when one verse bridge is included within another one.
		/// </summary>
		[Test]
		public void CompareTo_VerseBridgeIncludesAnother()
		{
			VerseRef vRefWithBridge1 = new VerseRef("GEN 1:1-2", versification);
			VerseRef vRefWithBridge2 = new VerseRef("GEN 1:1-5", versification);

			Assert.Greater(vRefWithBridge2.CompareTo(vRefWithBridge1, true), 0);
			Assert.Less(vRefWithBridge1.CompareTo(vRefWithBridge2, true), 0);
		}

		/// <summary>
		/// Tests the CompareTo method when the versification mapping makes a different verse the same.
		/// </summary>
		[Test]
		public void CompareTo_VersificationMakesDifferentVerseSame()
		{
			VerseRef vRef1 = new VerseRef("EXO 8:1", ScrVers.English);

			// Set up another VerseRef that has a different verse that is defined
			// to be same as EXO 8:1 in the Septuagint
			// (The Septuagint is the same as original versification for these verses).
			VerseRef vRef2 = new VerseRef("EXO 7:26", ScrVers.Septuagint);

			Assert.AreEqual(0, vRef2.CompareTo(vRef1, true));
			Assert.AreEqual(0, vRef1.CompareTo(vRef2, true));
		}

		/// <summary>
		/// Tests the CompareTo method when the versification mapping makes a different verse range the same.
		/// </summary>
		[Test]
		public void CompareTo_VersificationMakesDifferentVerseRangeSame()
		{
			VerseRef vRef1 = new VerseRef("EXO 8:2-3", ScrVers.English);

			// Set up another VerseRef that has a different verse range that is defined
			// to be same as EXO 8:2-3 in original versification.
			VerseRef vRef2 = new VerseRef("EXO 7:27-28", ScrVers.Original);

			Assert.AreEqual(0, vRef2.CompareTo(vRef1, true));
			Assert.AreEqual(0, vRef1.CompareTo(vRef2, true));
		}

		/// <summary>
		/// Tests the CompareTo method when the versification mapping makes a same verse different.
		/// </summary>
		[Test]
		public void CompareTo_VersificationMakesSameVerseDifferent()
		{
			VerseRef vRef1 = new VerseRef("EXO 8:1", ScrVers.English);

			// Set up another VerseRef that has a different verse that is different from original.
			VerseRef vRef2 = new VerseRef("EXO 8:1", ScrVers.Original);

			// Changing English ref to standard versification (EXO 8:1 => EXO 7:26)
			// so difference (1) is found in chapter number that is evaluated first.
			Assert.Greater(vRef2.CompareTo(vRef1, true), 0);
			// Changing Septuagint ref to English versification EXO 8:1 => EXO 8:5 
			// so difference (-4) is found in verse number since book and chapter numbers are the same.
			Assert.Less(vRef1.CompareTo(vRef2, true), 0); // diff found in verse number 
		}

		/// <summary>
		/// Tests the CompareTo method when the versification mapping makes the same verse range different.
		/// </summary>
		[Test]
		public void CompareTo_VersificationMakesSameVerseRangeDifferent()
		{
			VerseRef vRef1 = new VerseRef("EXO 8:2-3", ScrVers.English);

			// Set up another VerseRef that has a different verse range that is defined
			// to be different in original.
			VerseRef vRef2 = new VerseRef("EXO 8:2-3", ScrVers.Septuagint);

			// Changing English ref to standard versification (EXO 8:2-3 => EXO 7:27-28)
			// so difference (1) is found in chapter number that is evaluated first.
			Assert.Greater(vRef2.CompareTo(vRef1, true), 0);
			// Changing Septuagint ref to English versification (EXO 8:2-3 => EXO 8:6-7)
			// so difference (-4) is found in verse number since book and chapter numbers are the same.
			Assert.Less(vRef1.CompareTo(vRef2, true), 0);
		}

		[Test]
		public void CompareTo_Segments()
		{
			Assert.Greater(new VerseRef("GEN 1:1a").CompareTo(new VerseRef("GEN 1:1")), 0);
			Assert.Less(new VerseRef("GEN 1:1").CompareTo(new VerseRef("GEN 1:1a")), 0);
			Assert.Less(new VerseRef("GEN 1:1a").CompareTo(new VerseRef("GEN 1:1b")), 0);
			Assert.Greater(new VerseRef("GEN 1:1b").CompareTo(new VerseRef("GEN 1:1a")), 0);
			Assert.AreEqual(new VerseRef("GEN 1:1a").CompareTo(new VerseRef("GEN 1:1a")), 0);
			Assert.AreEqual(new VerseRef("GEN 1:1b").CompareTo(new VerseRef("GEN 1:1b")), 0);
		}

		[Test]
		public void CompareTo_CustomSegments()
		{
			string[] segments = new string[] { "p", "a", "#", "@", "!", "b" };

			Assert.Less(new VerseRef("GEN 1:1p").CompareTo(new VerseRef("GEN 1:1b"), segments, false, true), 0);
			Assert.Less(new VerseRef("GEN 1:1#").CompareTo(new VerseRef("GEN 1:1@"), segments, false, true), 0);
			Assert.Less(new VerseRef("GEN 1:1!").CompareTo(new VerseRef("GEN 1:1b"), segments, false, true), 0);
			Assert.Greater(new VerseRef("GEN 1:1b").CompareTo(new VerseRef("GEN 1:1p"), segments, false, true), 0);
			Assert.Greater(new VerseRef("GEN 1:1@").CompareTo(new VerseRef("GEN 1:1#"), segments, false, true), 0);
			Assert.Greater(new VerseRef("GEN 1:1b").CompareTo(new VerseRef("GEN 1:1!"), segments, false, true), 0);

			Assert.AreEqual(new VerseRef("GEN 1:1#").CompareTo(new VerseRef("GEN 1:1#"), segments, false, true), 0);
			Assert.AreEqual(new VerseRef("GEN 1:1@").CompareTo(new VerseRef("GEN 1:1@"), segments, false, true), 0);
		}
		#endregion

		#region Serialization/Deserialization
		/// <summary>
		/// Tests serialization and deserialization
		/// </summary>
		[Test]
		public void SerializeDeserialize()
		{
			string serialized = XmlSerializationHelper.SerializeToString(new VerseRef("LEV", "12", "6-7a", ScrVers.Vulgate));
			Console.WriteLine("Serialized reference string: ");
			Console.WriteLine("***" + serialized + "***");
			Console.WriteLine();

			VerseRef restored = XmlSerializationHelper.DeserializeFromString<VerseRef>(serialized);
			Assert.IsFalse(restored.IsDefault);
			Assert.AreEqual("LEV", restored.Book);
			Assert.AreEqual("12", restored.Chapter);
			Assert.AreEqual("6-7a", restored.Verse);
			Assert.AreEqual(ScrVers.Vulgate, restored.Versification);
		}

		/// <summary>
		/// Tests serialization and deserialization when the VerseRef is default
		/// </summary>
		[Test]
		public void SerializeDeserialize_DefaultVerseRef()
		{
			string serialized = XmlSerializationHelper.SerializeToString(new VerseRef());
			Console.WriteLine("Serialized reference string: ");
			Console.WriteLine("***" + serialized + "***");
			Console.WriteLine();

			VerseRef restored = XmlSerializationHelper.DeserializeFromString<VerseRef>(serialized);
			Assert.IsTrue(restored.IsDefault);
			Assert.AreEqual("", restored.Book);
			Assert.AreEqual(0, restored.ChapterNum);
			Assert.AreEqual(0, restored.VerseNum);
			Assert.AreEqual(null, restored.Versification);
		}

		[Test]
		public void Deserialize_DefaultVerification()
		{
			string serialized = "<VerseRef>LEV 12:6-7a</VerseRef>";

			VerseRef restored = XmlSerializationHelper.DeserializeFromString<VerseRef>(serialized);
			Assert.IsFalse(restored.IsDefault);
			Assert.AreEqual("LEV", restored.Book);
			Assert.AreEqual("12", restored.Chapter);
			Assert.AreEqual("6-7a", restored.Verse);
			Assert.AreEqual(VerseRef.defaultVersification, restored.Versification);
		}
		#endregion

		#region HashCode test
		[Test, Explicit] // Not sure how useful this is...
		public void HashCode_ConfirmUniqueCodes()
		{
			HashSet<long> scrRefCodes = new HashSet<long>();
			VerseRef verseRef = new VerseRef("GEN 1:1", ScrVers.Septuagint); // use Septuagint to test with segments
			do
			{
				long hashCode = verseRef.LongHashCode;
				if (scrRefCodes.Contains(hashCode))
					Assert.Fail("Duplicate hash code for verse " + verseRef);
				else
					scrRefCodes.Add(hashCode);
			} while (verseRef.NextVerse() && Canon.IsCanonical(verseRef.BookNum));
		}

		[Test]
		public void HashCode_CloseReferences()
		{
			HashSet<long> scrRefCodes = new HashSet<long>();
			versification.ParseVerseSegmentsLine("*GEN 1:2,a,b,c,d");
			versification.ParseVerseSegmentsLine("*GEN 1:3,a,b,c,d");
			versification.ParseVerseSegmentsLine("*GEN 1:4,a,b,c,d");
			VerseRef verseRef = new VerseRef("GEN 1:1", versification);
			do
			{
				long hashCode = verseRef.LongHashCode;
				if (scrRefCodes.Contains(hashCode))
					Assert.Fail("Duplicate hash code for verse " + verseRef);
				else
					scrRefCodes.Add(hashCode);
			} while (verseRef.NextVerse() && verseRef.BookNum == 1);
		}
		#endregion

		#region Segment tests
		/// <summary>
		/// Tests the Segment method
		/// </summary>
		[Test]
		public void Segment()
		{
			Assert.AreEqual("", new VerseRef("GEN 1:1").Segment(null));
			Assert.AreEqual("a", new VerseRef("GEN 1:1a").Segment(null));
			Assert.AreEqual("@", new VerseRef("GEN 1:1@").Segment(null));
			Assert.AreEqual("a", new VerseRef("GEN 1:1a-5c").Segment(null));
			Assert.AreEqual("", new VerseRef("GEN 1:1-5c").Segment(null));
			Assert.AreEqual("b", new VerseRef("GEN 1:1b-3c").Segment(null));
			Assert.AreEqual("a", new VerseRef("GEN 1:1a,3,5").Segment(null));
			Assert.AreEqual("", new VerseRef("GEN 1:1,3b,5").Segment(null));
			Assert.AreEqual("abc", new VerseRef("GEN 1:1abc").Segment(null));
			Assert.AreEqual("a\u0301", new VerseRef("GEN 1:1a\u0301").Segment(null));
		}

		/// <summary>
		/// Tests the Segment method when segments are provided from the versification for a particular verse.
		/// </summary>
		[Test]
		public void Segment_WithVersificationInfo()
		{
			versification.ParseVerseSegmentsLine("*GEN 1:1,-,@,$,%,abc,a\u0301");

			Assert.AreEqual("", new VerseRef("GEN 1:1", versification).Segment(null));
			Assert.AreEqual("", new VerseRef("GEN 1:1a", versification).Segment(null));
			Assert.AreEqual("@", new VerseRef("GEN 1:1@", versification).Segment(null));
			Assert.AreEqual("", new VerseRef("GEN 1:1!", versification).Segment(null));
			Assert.AreEqual("", new VerseRef("GEN 1:1def", versification).Segment(null));
			Assert.AreEqual("a", new VerseRef("GEN 1:2a", versification).Segment(null));
			Assert.AreEqual("b", new VerseRef("GEN 1:2b", versification).Segment(null));
			Assert.AreEqual("abc", new VerseRef("GEN 1:1abc", versification).Segment(null));
			Assert.AreEqual("", new VerseRef("GEN 1:1abcdef", versification).Segment(null));
			Assert.AreEqual("a\u0301", new VerseRef("GEN 1:1a\u0301", versification).Segment(null));
		}

		/// <summary>
		/// Tests the Segment method when custom default segments are specified.
		/// </summary>
		[Test]
		public void Segment_WithDefinedDefaultSegments()
		{
			string[] definedSegments = new[] { "@", "$", "%", "abc", "a\u0301" };

			Assert.AreEqual("", new VerseRef("GEN 1:1").Segment(definedSegments));
			Assert.AreEqual("", new VerseRef("GEN 1:1a").Segment(definedSegments));
			Assert.AreEqual("@", new VerseRef("GEN 1:1@").Segment(definedSegments));
			Assert.AreEqual("$", new VerseRef("GEN 1:1$").Segment(definedSegments));
			Assert.AreEqual("", new VerseRef("GEN 1:1!").Segment(definedSegments));
			Assert.AreEqual("abc", new VerseRef("GEN 1:1abc").Segment(definedSegments));
			Assert.AreEqual("", new VerseRef("GEN 1:1def").Segment(definedSegments));
			Assert.AreEqual("a\u0301", new VerseRef("GEN 1:1a\u0301").Segment(definedSegments));
		}

		/// <summary>
		/// Tests the Segment method when versification and custom default segments are specified.
		/// The versification trumps the custom default segments.
		/// </summary>
		[Test]
		public void Segment_WithVersificationAndDefinedDefaultSegments()
		{
			versification.ParseVerseSegmentsLine("*GEN 1:1,^,&,*,a\u0301");
			string[] definedSegments = new[] { "@", "$", "%", "o\u0301" };

			Assert.AreEqual("*", new VerseRef("GEN 1:1*", versification).Segment(definedSegments));
			Assert.AreEqual("a\u0301", new VerseRef("GEN 1:1a\u0301", versification).Segment(definedSegments));
			Assert.AreEqual("", new VerseRef("GEN 1:2a\u0301", versification).Segment(definedSegments));
			Assert.AreEqual("", new VerseRef("GEN 1:2*", versification).Segment(definedSegments));
			Assert.AreEqual("", new VerseRef("GEN 1:1@", versification).Segment(definedSegments));
			Assert.AreEqual("", new VerseRef("GEN 1:1o\u0301", versification).Segment(definedSegments));
			Assert.AreEqual("@", new VerseRef("GEN 1:2@", versification).Segment(definedSegments));
			Assert.AreEqual("o\u0301", new VerseRef("GEN 1:2o\u0301", versification).Segment(definedSegments));
		}
		#endregion

		#region AreOverlappingVersesRanges tests
		/// <summary>
		/// Test VerseRef.AreOverlappingVersesRanges
		/// </summary>
		[Test]
		public void AreOverlappingVersesRanges()
		{
			Assert.IsFalse(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:1"), new VerseRef("GEN 2:2")));
			Assert.IsFalse(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 1:3"), new VerseRef("GEN 2:3")));
			Assert.IsFalse(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:3"), new VerseRef("EXO 2:3")));
			Assert.IsFalse(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:3a"), new VerseRef("GEN 2:3b")));
			Assert.IsFalse(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:2,5"), new VerseRef("GEN 2:4")));
			Assert.IsFalse(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:2,4-5"), new VerseRef("GEN 2:3")));

			Assert.IsTrue(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:3"), new VerseRef("GEN 2:3")));
			Assert.IsTrue(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:3b"), new VerseRef("GEN 2:2-3c")));
			Assert.IsTrue(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:1-3"), new VerseRef("GEN 2:2-4")));
			Assert.IsTrue(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:3b"), new VerseRef("GEN 2:2-3c")));
			Assert.IsTrue(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:2,4"), new VerseRef("GEN 2:4")));
			Assert.IsTrue(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:2,4-6"), new VerseRef("GEN 2:2")));
			Assert.IsTrue(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:2,4-6"), new VerseRef("GEN 2:3-4")));
			Assert.IsTrue(VerseRef.AreOverlappingVersesRanges(new VerseRef("GEN 2:2,4-6"), new VerseRef("GEN 2:1a,3a-4c")));
		}
		#endregion

		#region Other Methods tests
		/// <summary>
		/// Make sure IsDefault is true for default constructor and false when any value is set
		/// </summary>
		[Test]
		public void IsDefault()
		{
			VerseRef testRef = new VerseRef();
			Assert.IsTrue(testRef.IsDefault, "IsDefault not true for default VerseRef");
			testRef.BookNum = 1;
			Assert.IsFalse(testRef.IsDefault, "IsDefault not false for VerseRef with value set");
			testRef = new VerseRef();
			testRef.ChapterNum = 1;
			Assert.IsFalse(testRef.IsDefault, "IsDefault not false for VerseRef with value set");
			testRef = new VerseRef();
			testRef.VerseNum = 1;
			Assert.IsFalse(testRef.IsDefault, "IsDefault not false for VerseRef with value set");
			testRef = new VerseRef();
			testRef.Versification = ScrVers.Vulgate;
			Assert.IsFalse(testRef.IsDefault, "IsDefault not false for VerseRef with value set");
		}

		/// <summary>
		/// Tests the ToString method
		/// </summary>
		[Test]
		public new void ToString()
		{
			Assert.AreEqual("GEN 0:0", new VerseRef(1, 0, 0).ToString());
			Assert.AreEqual("GEN 1:0", new VerseRef(1, 1, 0).ToString());
			Assert.AreEqual("GEN 2:0", new VerseRef(1, 2, 0).ToString());
			Assert.AreEqual("EXO 4:6", new VerseRef(2, 4, 6).ToString());
			Assert.AreEqual("LEV 4:6b-7a", new VerseRef("LEV", "4", "6b-7a", ScrVers.English).ToString());
		}

		/// <summary>
		/// Tests the Text property
		/// </summary>
		[Test]
		public void Text()
		{
			VerseRef vref = new VerseRef();
			Assert.AreEqual("", vref.Text);

			vref.Text = " :"; // Old invalid references had this format when serialized
			Assert.AreEqual("", vref.Text);

			vref.Text = "GEN 5:18";
			Assert.AreEqual("GEN 5:18", vref.Text);

			vref.Text = "LEV 2:15-16a";
			Assert.AreEqual("LEV 2:15-16a", vref.Text);
			Assert.AreEqual("LEV", vref.Book);
			Assert.AreEqual("2", vref.Chapter);
			Assert.AreEqual("15-16a", vref.Verse);

			vref.Text = "EXO 1:3";
			Assert.AreEqual("EXO 1:3", vref.Text);
			Assert.AreEqual("EXO", vref.Book);
			Assert.AreEqual("1", vref.Chapter);
			Assert.AreEqual("3", vref.Verse);
		}

		/// <summary>
		/// Tests the Simplify method
		/// </summary>
		[Test]
		public void Simplify()
		{
			VerseRef vref = new VerseRef();
			vref.Simplify();
			Assert.AreEqual(new VerseRef(), vref);

			vref = new VerseRef("EXO 6:0");
			vref.Simplify();
			Assert.AreEqual(new VerseRef("EXO 6:0"), vref);

			vref = new VerseRef("EXO 6:5b-18a,19");
			vref.Simplify();
			Assert.AreEqual(new VerseRef("EXO 6:5"), vref);

			vref = new VerseRef("EXO 6:9a,9b");
			vref.Simplify();
			Assert.AreEqual(new VerseRef("EXO 6:9"), vref);

			vref = new VerseRef("EXO 6:4-10");
			vref.Simplify();
			Assert.AreEqual(new VerseRef("EXO 6:4"), vref);

			vref = new VerseRef("EXO 6:150monkeys");
			vref.Simplify();
			Assert.AreEqual(new VerseRef("EXO 6:150"), vref);
		}

		/// <summary>
		/// Tests the UnBridge method
		/// </summary>
		[Test]
		public void UnBridge()
		{
			Assert.AreEqual(new VerseRef(), new VerseRef().UnBridge());
			Assert.AreEqual(new VerseRef("EXO 6:0"), new VerseRef("EXO 6:0").UnBridge());
			Assert.AreEqual(new VerseRef("EXO 6:5b"), new VerseRef("EXO 6:5b-18a,19").UnBridge());
			Assert.AreEqual(new VerseRef("EXO 6:9a"), new VerseRef("EXO 6:9a,9b").UnBridge());
			Assert.AreEqual(new VerseRef("EXO 6:4"), new VerseRef("EXO 6:4-10").UnBridge());
			Assert.AreEqual(new VerseRef("EXO 6:150monkeys"), new VerseRef("EXO 6:150monkeys").UnBridge());
		}

		/// <summary>
		/// Tests the TrySetVerseUnicode method with numerals from various Unicode-supported scripts
		/// </summary>
		[TestCase("५", ExpectedResult = 5, TestName = "Devanagari numeral")]
		[TestCase("૧૬", ExpectedResult = 16, TestName = "Gujarati numeral")]
		[TestCase("5", ExpectedResult = 5, TestName = "Latin numeral")]
		[TestCase("᠔", ExpectedResult = 4, TestName = "Mongolian numeral")]
		[TestCase("A", ExpectedResult = -1, TestName = "Latin non-numeral")]
		[TestCase("ะ", ExpectedResult = -1, TestName = "Thai non-numeral")]
		[TestCase("᠔-᠔", ExpectedResult = 4, TestName = "Mongolian complex verse")]
		[TestCase("᠔ᠠ", ExpectedResult = 4, TestName = "Mongolian complex verse - lettered")]
		[TestCase("二十", ExpectedResult = 20, TestName = "Japanese numeral", IgnoreReason = "Non-decimal numeral systems not yet implemented. (See issue #1000.)")]
		[TestCase("יא", ExpectedResult = 11, TestName = "Hebrew numeral", IgnoreReason = "Non-decimal numeral systems not yet implemented. (See issue #1000.)")]
		[TestCase("\U0001113A\U00011138", ExpectedResult = 42, TestName = "Chakma numeral", IgnoreReason = "Surrogate pair handling not yet implemented. (See issue #1000.)")]
		public int TrySetVerseUnicode_InterpretNumerals(string verseStr)
		{
			VerseRef vref = new VerseRef("EXO 6:1");

			bool success = vref.TrySetVerseUnicode(verseStr);
			Assert.AreEqual(success, vref.VerseNum != -1);

			return vref.VerseNum;
		}

		/// <summary>
		/// Tests the Verse property's set method with various input strings
		/// </summary>
		[TestCase("5", ExpectedResult = 5, TestName = "Latin numeral")]
		[TestCase("524", ExpectedResult = 524, TestName = "Large Latin numeral")]
		[TestCase("A", ExpectedResult = -1, TestName = "Latin non-numeral")]
		[TestCase("૧૬", ExpectedResult = -1, TestName = "Non-Latin numeral")]
		[TestCase("1-", ExpectedResult = 1, TestName = "Complex verse - incomplete")]
		[TestCase("2.3", ExpectedResult = 2, TestName = "Complex verse - decimal")]
		[TestCase("5-7", ExpectedResult = 5, TestName = "Complex verse - complete")]
		[TestCase("7a", ExpectedResult = 7, TestName = "Complex verse - lettered")]
		public int SetVerse_InterpretNumerals(string verseStr)
		{
			VerseRef vref = new VerseRef("EXO 6:1");

			vref.Verse = verseStr;

			return vref.VerseNum;
		}

		#endregion

		#region InRange

		[TestCase("LEV 6:2-3", "LEV 6:2-3", true)]
		[TestCase("LEV 6:2-3", "LEV 24:2-3", false)]
		public void InRange_Exact(string thisRange, string startAndEndRange, bool expectedResult)
		{
			var levC6V2to3 = new VerseRef(thisRange);
			var levC24V2to3 = new VerseRef(startAndEndRange);

			Assert.AreEqual(expectedResult, levC6V2to3.InRange(levC24V2to3, levC24V2to3, true));
		}

		#endregion
	}
}
