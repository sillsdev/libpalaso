using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;

namespace SIL.Scripture
{
	/// <summary>
	/// Stores a reference to a specific verse in Scripture.
	/// </summary>
	public struct VerseRef : IComparable<VerseRef>, IComparable, IScrVerseRef
	{
		#region Constants
		[PublicAPI]
#if DEBUG
		public static readonly ScrVers defaultVersification = null;
#else
		public static readonly ScrVers defaultVersification = ScrVers.English;
#endif
		[PublicAPI]
		public const char verseRangeSeparator = '-';
		[PublicAPI]
		public const char verseSequenceIndicator = ',';
		[PublicAPI]
		public static readonly string[] verseRangeSeparators = new[] { verseRangeSeparator.ToString() };
		[PublicAPI]
		public static readonly string[] verseSequenceIndicators = new[] { verseSequenceIndicator.ToString() };

		private const int chapterDigitShifter = 1000;
		private const int bookDigitShifter = chapterDigitShifter * chapterDigitShifter;
		private const int bcvMaxValue = chapterDigitShifter - 1;
		private const string rtlMark = "\u200f";
		#endregion

		#region Enumerated types

		/// <summary>
		/// The valid status of the VerseRef
		/// </summary>
		public enum ValidStatusType
		{
			Valid,
			UnknownVersification,
			OutOfRange,
			VerseOutOfOrder,
			VerseRepeated
		}

		#endregion

		#region Member variables

		private short bookNum;
		private short chapterNum;
		private short verseNum;
		private string verse;
		private ScrVers versification;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an empty reference with the specified versification
		/// </summary>
		public VerseRef(ScrVers versification)
		{
			bookNum = 0;
			chapterNum = verseNum = -1;
			verse = null;
			this.versification = versification;
		}
		
		public VerseRef(string book, string chapter, string verse, ScrVers versification) : this(versification)
		{
			UpdateInternal(book, chapter, verse);
		}

		/// <summary>
		/// Creates a new reference
		/// <para>WARNING: This constructor creates a VerseRef that has no versification. Use with caution!</para>
		/// </summary>
		public VerseRef(int bbbcccvvv)
			: this(bbbcccvvv / 1000000, bbbcccvvv % 1000000 / 1000, bbbcccvvv % 1000)
		{
		}
		
		/// <summary>
		/// Creates a new reference
		/// </summary>
		public VerseRef(int bbbcccvvv, ScrVers versification) : this(bbbcccvvv)
		{
			this.versification = versification;
		}

		/// <summary>
		/// Creates a new reference
		/// <para>WARNING: This constructor creates a VerseRef that has no versification. Use with caution!</para>
		/// </summary>
		public VerseRef(int bookNum, int chapterNum, int verseNum) : this(defaultVersification)
		{
			BookNum = bookNum;
			ChapterNum = chapterNum;
			VerseNum = verseNum;
		}
		
		public VerseRef(int bookNum, int chapterNum, int verseNum, ScrVers versification) :
			this(bookNum, chapterNum, verseNum)
		{
			this.versification = versification;
		}

		public VerseRef(VerseRef vref)
		{
			bookNum = vref.bookNum;
			chapterNum = vref.chapterNum;
			verseNum = vref.verseNum;
			verse = vref.verse;
			versification = vref.versification;
		}

		/// <summary>
		/// Creates a reference by parsing the specified string
		/// <para>WARNING: This constructor creates a VerseRef that has no versification. Use with caution!</para>
		/// </summary>
		/// <param name="verseStr">verse string to parse (e.g. "MAT 3:11")</param>
		/// <exception cref="VerseRefException"></exception>
		public VerseRef(string verseStr) : this(defaultVersification)
		{
			Parse(verseStr);
		}
		
		/// <summary>
		/// Creates a reference by parsing the specified string
		/// </summary>
		/// <param name="verseStr">verse string to parse (e.g. "MAT 3:11")</param>
		/// <param name="versification"></param>
		/// <exception cref="VerseRefException"></exception>
		public VerseRef(string verseStr, ScrVers versification) : this(verseStr)
		{
			this.versification = versification;
		}

		void UpdateInternal(string bookStr, string chapterStr, string verseStr)
		{
			BookNum = Canon.BookIdToNumber(bookStr);
			Chapter = chapterStr;
			Verse = verseStr;
		}

		#endregion

		#region Attribute Properties (access information about the reference)
		/// <summary>
		/// Checks to see if a VerseRef hasn't been set - all values are the default.
		/// </summary>
		public bool IsDefault => bookNum == 0 && chapterNum == 0 && verseNum == 0 && versification == null;

		/// <summary>
		/// Number of first chapter.
		/// TODO bro Do we need to make this 0 for intro material?
		/// </summary>
		[XmlIgnore]
		[PublicAPI]
		public int FirstChapter
		{
			get { return 1; }
		}

		[XmlIgnore]
		public int LastChapter
		{
			get { return versification.GetLastChapter(BookNum); }
		}

		[XmlIgnore]
		public int LastVerse
		{
			get { return versification.GetLastVerse(BookNum, ChapterNum); }
		}

		/// <summary>
		/// Gets whether the verse is defined as an excluded verse in the versification. 
		/// </summary>
		/// <remarks>Does not handle verse ranges</remarks>
		[XmlIgnore]
		[PublicAPI]
		public bool IsExcluded
		{
			get { return versification.IsExcluded(BBBCCCVVV); }
		}

		/// <summary>
		/// Gets whether the verse has explicit segments defined in the versification. 
		/// </summary>
		/// <remarks>Does not handle verse ranges</remarks>
		[XmlIgnore]
		[PublicAPI]
		public bool HasSegmentsDefined
		{
			get { return versification != null && versification.VerseSegments(BBBCCCVVV) != null; }
		}

		/// <summary>
		/// Gets whether the verse contains multiple verses. 
		/// </summary>
		[XmlIgnore]
		public bool HasMultiple
		{
			get
			{
				return verse != null &&
					   (verse.IndexOf(verseRangeSeparator) != -1 || verse.IndexOf(verseSequenceIndicator) != -1);
			}
		}

		/// <summary>
		/// Gets or sets the book of the reference. Book is the 
		/// three letter abbreviation in capital letters. e.g. "MAT"
		/// </summary>
		[XmlIgnore]
		public string Book
		{
			get { return Canon.BookNumberToId(BookNum, string.Empty); }
			set { BookNum = Canon.BookIdToNumber(value); }
		}
		
		/// <summary>
		/// Gets or sets the chapter of the reference. e.g. "3"
		/// </summary>
		[XmlIgnore]
		public string Chapter
		{
			get { return IsDefault || chapterNum < 0 ? string.Empty : chapterNum.ToString(); }
			set
			{
				short chapter;
				chapterNum = short.TryParse(value, out chapter) ? chapter : (short)-1;
				if (chapterNum < 0)
					Trace.TraceWarning("Just failed to parse a chapter number: " + value);
			}
		}

		/// <summary>
		///  Gets or sets the verse of the reference e.g. "11"
		/// </summary>
		[XmlIgnore]
		public string Verse
		{
			get => verse ?? (IsDefault || verseNum < 0 ? string.Empty : verseNum.ToString()); 
			set => TrySetVerse(value, true); // The USX standard only expects support for Latin numerals {0-9}* in verse numbers.
		}

		/// <summary>
		/// Value as "BBB C:V".
		/// This is used for XML serialization.
		/// </summary>
		[XmlText]
		public string Text
		{
			get { return ToString(); }
			set
			{
				bookNum = 0;
				chapterNum = -1;
				verseNum = -1;
				verse = null;
				if (versification == null)
					versification = defaultVersification;
				try
				{
					Parse(value);
				}
				catch (VerseRefException e)
				{
					// Allow parse to fail during deserialization. VerseRef will just be invalid.
					Console.WriteLine("Invalid deserialized reference: " + e.InvalidVerseRef);
				}
			}
		}

		/// <summary>
		/// Tries to set verse and verseNum by parsing the `value` string.
		/// This is used by Verse.set and TrySetVerseUnicode
		/// </summary>
		/// <returns><c>true</c> if the verse was set successfully </returns>
		bool TrySetVerse(string value, bool romanOnly)
		{
			verse = !TryGetVerseNum(value, romanOnly, out verseNum) ? value.Replace(rtlMark, "") : null;
			if (verseNum >= 0)
				return true;

			Trace.TraceWarning("Just failed to parse a verse number: " + value);
			TryGetVerseNum(verse, romanOnly, out verseNum);
			return false;
		}

		/// <summary>
		/// Parses a verse string and gets the leading numeric portion as a number.
		/// </summary>
		/// <returns><c>true</c> if the entire string could be parsed as a single,
		/// simple verse number (1-999); <c>false</c> if the verse string represented
		/// a verse bridge, contained segment letters, or was invalid</returns>
		static bool TryGetVerseNum(string verseStr, bool romanOnly , out short vNum)
		{
			if (string.IsNullOrEmpty(verseStr))
			{
				vNum = -1;
				return true;
			}

			vNum = 0;
			for (int i = 0; i < verseStr.Length; i++)
			{
				char ch = verseStr[i];
				if (romanOnly ? (ch < '0' || ch > '9') : !char.IsDigit(ch))
				{
					if (i == 0)
						vNum = -1;
					return false;
				}

				vNum = (short)(vNum * 10 + (romanOnly ? ch - '0' : char.GetNumericValue(ch)));
				if (vNum > bcvMaxValue)
				{
					// whoops, we got too big!
					vNum = -1;
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Gets the reference as a comparable integer where the book,
		/// chapter, and verse each occupy three digits and the verse is 0.
		/// </summary>
		public int BBBCCC
		{
			get { return GetBBBCCCVVV(bookNum, chapterNum, 0); }
		}

		/// <summary>
		/// Gets the reference as a comparable integer where the book,
		/// chapter, and verse each occupy three digits. If verse is not null
		/// (i.e., this reference represents a complex reference with verse
		/// segments or bridge) this cannot be used for an exact comparison.
		/// </summary>
		public int BBBCCCVVV
		{
			get { return GetBBBCCCVVV(bookNum, chapterNum, verseNum); }
		}

		/// <summary>
		/// Returns comparable string in the format BBBCCCVVV with the segment
		/// letter, if any, tacked on the end.
		/// </summary>
		public string BBBCCCVVVS
		{
			get { return BBBCCCVVV.ToString().PadLeft(9, '0') + Segment(); }
		}

		/// <summary>
		/// Returns a long hash code for the verse reference which is guaranteed to be unique
		/// provided that there are no more than 999 books, chapters and/or verses.
		/// </summary>
		public long LongHashCode
		{
			get { return ((long)BBBCCCVVV << 32) + (string.IsNullOrEmpty(verse) ? 0L : verse.GetHashCode()); }
		}

		/// <summary>
		/// Get segments associated with this verse, if any. Otherwise, get default segments.
		/// </summary>
		/// <param name="defaultSegments">verse segments defined for the current language</param>
		/// <returns></returns>
		[PublicAPI]
		public string[] GetSegments(string[] defaultSegments)
		{
			if (versification == null)
				return defaultSegments;

			string[] segsForThisVerse = versification.VerseSegments(BBBCCCVVV);
			return segsForThisVerse ?? defaultSegments;
		}

		public void AdvanceToLastSegment()
		{
			string[] segments = GetSegments(null);
			if (segments?.Length > 0)
				Verse += segments[segments.Length - 1];
		}

		/// <summary>
		/// Get the segment from the verse.
		/// </summary>
		/// <param name="validSegments">valid segments defined for the language or null if not defined or available</param>
		/// <returns>validated segment (according to default segments or versification, if available for verse);
		/// empty string if no segment or if segment did not validate</returns>
		[PublicAPI]
		public string Segment(string[] validSegments)
		{
			string seg = Segment();
			if (seg.Length == 0)
				return "";

			validSegments = GetSegments(validSegments);
			if (validSegments != null && validSegments.Length > 0)
				return validSegments.Contains(seg) ? seg : "";

			return seg;
		}

		/// <summary>
		/// Get the segment number for the verse.
		/// </summary>
		/// <param name="validSegments">valid segments defined for the language or null if not defined or available</param>
		/// <returns>returns the index of the segment (according to default segments or versification, if available for verse) or
		/// -1 if no segment or an unknown segment</returns>
		public int SegmentNumber(string[] validSegments)
		{
			string seg = Segment();
			if (seg.Length == 0)
				return -1;

			validSegments = GetSegments(validSegments);
			if (validSegments != null)
				return Array.IndexOf(validSegments, seg);

			return -1;
		}

		/// <summary>
		/// Get segment from verse string.
		/// </summary>
		/// <returns>non-validated segment, or empty string if no segment found</returns>
		[PublicAPI]
		public string Segment()
		{
			if (string.IsNullOrEmpty(verse) || !char.IsDigit(verse[0]))
				return "";

			bool foundSegStart = false;

			StringBuilder strBldr = new StringBuilder();
			for (int i = 0; i < verse.Length; i++)
			{
				char c = verse[i];
				if (c == verseRangeSeparator || c == verseSequenceIndicator)
					break;

				if (!char.IsDigit(c))
				{
					foundSegStart = true;
					strBldr.Append(c);
				}
				else if (foundSegStart)
					break;
			}

			return strBldr.ToString();
		}

		/// <summary>
		/// Simplifies this verse ref so that it has no bridging of verses or 
		/// verse segments like "1a".
		/// </summary>
		public void Simplify()
		{
			verse = null;
		}

		/// <summary>
		/// Returns verse ref with no bridging, but maintaining segments like "1a".
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public VerseRef UnBridge()
		{
			return AllVerses().FirstOrDefault();
		}

		/// <summary>
		/// String representing the versification (should ONLY be used for serialization/deserialization)
		/// </summary>
		/// <remarks>This is for backwards compatibility when ScrVers was an enumeration.</remarks>
		[XmlAttribute("Versification")]
		public string VersificationStr
		{
			get { return versification?.Name; }
			set { versification = value != null ? new ScrVers(value) : null; }
		}

		/// <summary>
		/// Gets or sets the versification of the reference.
		/// Setting this value does not attempt to convert between
		/// versifications. To do so, use one of the ChangeVersification methods
		/// </summary>
		[XmlIgnore]
		public ScrVers Versification
		{
			get { return versification; }
			set { versification = value; }
		}

		/// <summary>
		/// Determines if the reference is valid
		/// </summary>
		public bool Valid
		{
			get { return ValidStatus == ValidStatusType.Valid; }
		}

		/// <summary>
		/// Get the valid status for this reference.
		/// </summary>
		[PublicAPI]
		public ValidStatusType ValidStatus
		{
			get { return ValidateVerse(verseRangeSeparators, verseSequenceIndicators); }
		}

		/// <summary>
		/// Gets whether a single verse reference is valid.
		/// </summary>
		ValidStatusType InternalValid
		{
			get
			{
				// Unknown versification is always invalid
				if (versification == null)
					return ValidStatusType.UnknownVersification;

				// If invalid book, reference is invalid
				if (bookNum <= 0 || bookNum > Canon.LastBook)
					return ValidStatusType.OutOfRange;

				// If non-biblical book, any chapter/verse is valid
				if (!Canon.IsCanonical(bookNum))
					return ValidStatusType.Valid;

				if (bookNum > versification.GetLastBook() || chapterNum <= 0 ||
					chapterNum > versification.GetLastChapter(bookNum) || VerseNum < 0 ||
					VerseNum > versification.GetLastVerse(bookNum, chapterNum))
				{
					return ValidStatusType.OutOfRange;
				}

				return versification.IsExcluded(BBBCCCVVV) ? ValidStatusType.OutOfRange : ValidStatusType.Valid;
			}
		}
		
		/// <summary>
		/// Attempts to change the versification to the specified versification
		/// </summary>
		/// <param name="newVersification">new versification to use</param>
		[PublicAPI]
		public void ChangeVersification(ScrVers newVersification)
		{
			if (!HasMultiple)
				newVersification.ChangeVersification(ref this);
			else
			{
				VerseRef result;
				newVersification.ChangeVersificationWithRanges(this, out result);
				CopyFrom(result);
			}
		}
		
		/// <summary>
		/// Change the versification of an entry with Verse like 1-3 or 1,3a.
		/// Can't really work in the most general case because the verse parts could become separate chapters.
		/// </summary>
		[PublicAPI]
		public bool ChangeVersificationWithRanges(ScrVers newVersification)
		{
			VerseRef temp;
			bool result = newVersification.ChangeVersificationWithRanges(this, out temp);
			CopyFrom(temp);
			return result;
		}

		#endregion

		#region Convert, Copy

		/// <summary>
		/// Makes a clone of the reference
		/// </summary>
		/// <returns></returns>
		public VerseRef Clone()
		{
			// Leaving this for now to reduce code changes. Isn't really nessary when VerseRef is a struct since a = b is a copy.
			return new VerseRef(this);
		}

		/// <summary>
		/// Copy contents from vref
		/// </summary>
		/// <param name="vref">VerseRef to be copied from</param>
		public void CopyFrom(VerseRef vref)
		{
			bookNum = vref.bookNum;
			chapterNum = vref.chapterNum;
			verseNum = vref.verseNum;
			verse = vref.verse;
			versification = vref.versification;
		}

		/// <summary>
		/// Copies the verse information to this object from vref.
		/// </summary>
		/// <param name="vref">VerseRef to be copied from</param>
		public void CopyVerseFrom(VerseRef vref)
		{
			verseNum = vref.verseNum;
			verse = vref.verse;
		}

		/// <summary>
		/// Parses the reference in the specified string.
		/// Optionally versification can follow reference as in GEN 3:11/4
		/// Throw an exception if 
		/// - invalid book name
		/// - chapter number is missing or not a number
		/// - verse number is missing or does not start with a number
		/// - versifcation is invalid
		/// </summary>
		/// <param name="verseStr">string to parse e.g. "MAT 3:11"</param>
		/// <exception cref="VerseRefException"></exception>
		[PublicAPI]
		public void Parse(string verseStr)
		{
			verseStr = verseStr.Replace(rtlMark, "");
			if (verseStr.IndexOf('/') >= 0)
			{
				string[] parts = verseStr.Split('/');
				verseStr = parts[0];
				if (parts.Length > 1)
				{
					try
					{
						int scrVerseCode = int.Parse(parts[1].Trim());
						versification = new ScrVers((ScrVersType)scrVerseCode);
					}
					catch (Exception)
					{
						throw new VerseRefException("Invalid reference : " + verseStr) { InvalidVerseRef = verseStr };
					}
				}
			}
			
			string[] b_cv = verseStr.Trim().Split(' ');
			if (b_cv.Length != 2)
				throw new VerseRefException("Invalid reference : " + verseStr) { InvalidVerseRef = verseStr };

			string[] c_v = b_cv[1].Split(':');

			int cnum;
			if (c_v.Length != 2 || Canon.BookIdToNumber(b_cv[0]) == 0 || !int.TryParse(c_v[0], out cnum) ||
				cnum < 0 || !IsVerseParseable(c_v[1]))
			{
				throw new VerseRefException("Invalid reference : " + verseStr) { InvalidVerseRef = verseStr };
			}

			UpdateInternal(b_cv[0], c_v[0], c_v[1]);
		}

		public override string ToString()
		{
			StringBuilder toStringBuilder = new StringBuilder(20); // length of 20 should get 99.99% of references
			string book = Book;
			if (book.Length == 0)
				return ""; // Handle empty book by just returning empty string - works around a bug in Mono 3.

			toStringBuilder.Append(book).Append(' ').Append(Chapter).Append(':').Append(Verse);
			return toStringBuilder.ToString();
		}
		
		/// <summary>
		/// .e.g GEN 3:11/4. Parse understands this format.
		/// </summary>
		public string ToStringWithVersification()
		{
			return ToString() + "/" + (int)Versification.Type;
		}

		public override int GetHashCode()
		{
			// ENHANCE: Currently the hashcode can change when the values of the VerseRef change.
			// This will create problems if/when VerseRef is ever used for a key in a hashtable.
			// The best thing we could do would be to make VerseRef immutable, but that seems unlikely
			// to go well.
			return verse != null ? BBBCCCVVV ^ verse.GetHashCode() : BBBCCCVVV;
		}

		#endregion

		#region Navigation: Book ( e.g. Next/Previous)

		/// <summary>
		/// Get or set Book based on book number. We still do quite a bit with book numbers
		/// so lets leave this public.
		/// </summary>
		/// <exception cref="VerseRefException">If BookNum is set to an invalid value</exception>
		[XmlIgnore]
		public int BookNum
		{
			get { return bookNum; }
			set
			{
				if (value <= 0 || value > Canon.LastBook)
					throw new VerseRefException("BookNum must be greater than zero and less than or equal to last book");
				bookNum = (short)value;
			}
		}

		/// <summary>
		/// Gets chapter number. -1 if not valid
		/// </summary>
		/// <exception cref="VerseRefException">If ChapterNum is negative</exception>
		[XmlIgnore]
		public int ChapterNum
		{
			get { return chapterNum; }
			set
			{
				if (value < 0)
					throw new VerseRefException("ChapterNum can not be negative");
				chapterNum = (short)value;
			}
		}

		/// <summary>
		/// Gets verse start number. -1 if not valid
		/// </summary>
		/// <exception cref="VerseRefException">If VerseNum is negative</exception>
		[XmlIgnore]
		public int VerseNum
		{
			get { return verseNum; }
			set
			{
				if (value < 0)
					throw new VerseRefException("VerseNum can not be negative");
				verseNum = (short)value;
				verse = null;
			}
		}

		// ---------- BOOK NEXT AND PREVIOUS ----------

		// NOTES ABOUT ALL NAVIGATION FUNCTIONS:
		//All the navigiation funcitons have two forms. One takes a BookSet
		//and constrains Next/Previous within that set. The other takes no
		//no arguments and constrains Next/Previous only to the entire canon
		//as Canon presents it to BookSet.

		/// <summary>
		/// Tries to move to the next book among a set of books present.
		/// </summary>
		/// <param name="present">Set of books present or selected.</param>
		/// <returns>true if successful</returns>
		public bool NextBook(BookSet present)
		{
			int curBook = bookNum;
			int newBook = present.NextSelected(curBook);
			if (newBook == curBook)
				return false;
			bookNum = (short)newBook;
			chapterNum = 1;
			VerseNum = 0; // Use property to reset verse string
			return true;
		}

		/// <summary>
		/// Tries to move to the next book in the entire canon superset.
		/// </summary>
		/// <returns>True if successful.</returns>
		public bool NextBook()
		{
			return NextBook(BookSet.AllBooks);
		}

		public bool PreviousBook(BookSet present)
		{
			int curBook = bookNum;
			int newBook = present.PreviousSelected(curBook);
			if (newBook == curBook)
				return false; //no previous selected book
			bookNum = (short)newBook;
			chapterNum = 1;
			VerseNum = 1; // Use property to reset verse string
			return true;
		}

		public bool PreviousBook()
		{
			return PreviousBook(BookSet.AllBooks);
		}

		// ---------- CHAPTER NEXT AND PREVIOUS ----------
		[PublicAPI]
		public bool NextChapter(BookSet present, bool skipExcluded)
		{
			// If current book doesn't exist, try jump to next.
			if (!present.IsSelected(bookNum))
				return NextBook(present);
			int newPosition = chapterNum + 1;
			if (newPosition > LastChapter)
				return NextBook(present);

			if (skipExcluded)
			{
				var nextRef = versification.FirstIncludedVerse(bookNum, newPosition);
				if (nextRef != null)
					CopyFrom(nextRef.Value);
				else
					return NextBook(present);
			}
			else
			{
				chapterNum = (short)newPosition;
				VerseNum = 1;
			}

			return true;
		}

		public bool NextChapter(BookSet present)
		{
			return NextChapter(present, false);
		}

		public bool NextChapter()
		{
			return NextChapter(BookSet.AllBooks);
		}

		bool PreviousBookLastChapter(BookSet present)
		{
			bool result = PreviousBook(present);
			if (result)
				chapterNum = (short)(LastChapter != Scripture.Versification.NonCanonicalLastChapterOrVerse ? LastChapter : 1);
			return result;
		}

		public bool PreviousChapter(BookSet present)
		{
			// current ref doesn't exist? try find an existing one prior
			if (!present.IsSelected(bookNum))
				return PreviousBookLastChapter(present);
			int newPosition = chapterNum - 1;
			if (newPosition < FirstChapter)
				return PreviousBookLastChapter(present);
			VerseNum = 1; // Use property to reset verse string
			chapterNum = (short)newPosition;
			return true;
		}

		public bool PreviousChapter()
		{
			return PreviousChapter(BookSet.AllBooks);
		}

		// ---------- VERSE NEXT AND PREVIOUS ----------

		/// <summary>
		/// Moves to the next verse (or verse segment, if available in the current versification).
		/// </summary>
		/// <returns>true if successful, false if at end of scripture</returns>
		public bool NextVerse(BookSet present)
		{
			return NextVerse(present, false);
		}

		/// <summary>
		/// Moves to the next verse (or verse segment, if available in the current versification).
		/// </summary>
		/// <returns>true if successful, false if at end of scripture</returns>
		[PublicAPI]
		public bool NextVerse(BookSet present, bool skipExcluded)
		{
			// avoid incrementing through a blank book
			if (!present.IsSelected(bookNum))
				return NextBook(present);
			
			string[] verseSegments = Versification.VerseSegments(BBBCCCVVV);
			if (verseSegments != null)
			{
				int segIndex = FindSegment(verseSegments);
				Debug.Assert(segIndex != -1, "No valid segment found.");
				if (segIndex != -1)
				{
					if (segIndex < verseSegments.Length - 1 &&
						!verseSegments[segIndex].Equals(verseSegments[segIndex + 1]))
					{
						// There is another segment and it isn't identical. 
						// (Navigation for an identical segment gets stalled so we just want to go to the next verse.)
						Verse = verseNum + verseSegments[segIndex + 1];
						return true;
					}
				}
			}

			do
			{
				if (verseNum >= versification.GetLastVerse(bookNum, chapterNum))
				{
					if (NextChapter(present, skipExcluded))
					{
						SetVerseWithSegmentInfo(true);
						return true;
					}
					return false;
				}

				VerseNum++; // Use property to reset verse string
				SetVerseWithSegmentInfo(true);
			} while (skipExcluded && versification.IsExcluded(BBBCCCVVV)); // search for next included verse if needed

			return true;
		}

		public bool NextVerse()
		{
			return NextVerse(BookSet.AllBooks);
		}

		bool PreviousChapterLastVerse(BookSet present)
		{
			bool result;
			// This current logic prevents simple nav to chapter 0:
			// Book doesn't exist or we just asked for the chapter before #1
			if (!present.IsSelected(bookNum) || chapterNum <= 1)
				result = PreviousBookLastChapter(present);
			else
			{
				result = true;
				chapterNum--;
			}
			if (result)
				VerseNum = LastVerse; // Use property to reset verse string
			return result;
		}


		/// <summary>
		/// Moves to the previous verse (or verse segment, if available in the current versification).
		/// </summary>
		/// <returns>true if successful, false if at beginning of scripture</returns>
		public bool PreviousVerse(BookSet present)
		{
			// avoid moving through nonexistent books.
			if (!present.IsSelected(bookNum))
			{
				if (PreviousChapterLastVerse(present))
				{
					SetVerseWithSegmentInfo(false);
					return true;
				}
				return false;
			}
			
			string[] verseSegments = Versification.VerseSegments(GetBBBCCCVVV(bookNum, chapterNum, verseNum));
			if (verseSegments != null)
			{
				int segIndex = FindSegment(verseSegments);
				Debug.Assert(segIndex != -1, "No valid segment found");
				if (segIndex != -1)
				{
					if (segIndex > 0)
					{
						Verse = verseNum + verseSegments[segIndex - 1];
						return true;
					}
				}
			}

			// No segment information available for current verse
			if (verseNum == 1 && chapterNum == 1)
				VerseNum = 0; // Use property to reset verse string
			else if (verseNum <= 1)
			{
				if (PreviousChapterLastVerse(present))
				{
					SetVerseWithSegmentInfo(false);
					return true;
				}
				return false;
			}
			else
			{
				VerseNum--; // Use property to reset verse string
				SetVerseWithSegmentInfo(false);
			}
			return true;
		}

		public bool PreviousVerse()
		{
			return PreviousVerse(BookSet.AllBooks);
		}

		/// <summary>
		/// Set the contents of the verse with any applicable segment information after changing the reference
		/// given whether the verse reference is moving forward or backward.
		/// </summary>
		void SetVerseWithSegmentInfo(bool movingForward)
		{
			string[] verseSegments = Versification.VerseSegments(GetBBBCCCVVV(bookNum, chapterNum, verseNum));
			if (verseSegments != null)
			{
				string segment = (movingForward) ? verseSegments[0] : verseSegments[verseSegments.Length - 1];
				Verse = verseNum + segment;
			}
			else
				verse = null;
		}

		/// <summary>
		/// Gets the index of the current segment for this verse given a list of segments for the verse.
		/// </summary>
		int FindSegment(string[] segments)
		{

			for (int iSeg = 0; iSeg < segments.Length; iSeg++)
			{
				if (segments[iSeg].Equals(Segment()))
					return iSeg;
			}

			return -1;
		}

		#endregion

		#region Compare: Equals and IComparable<VerseRef> Members

		public override bool Equals(object obj)
		{
			if (obj is VerseRef)
			{
				VerseRef v = (VerseRef)obj;
				return (v.bookNum == bookNum)
					   && (v.chapterNum == chapterNum)
					   && (v.verseNum == verseNum)
					   && (v.verse == verse)
					   && (v.versification == versification);
			}
			return false;
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			if (!(obj is VerseRef))
				throw new ArgumentException("Object must be of type VerseRef.");

			return CompareTo((VerseRef) obj);
		}

		/// <summary>
		/// Compares two verse refs using the default canon. 
		/// </summary>
		public int CompareTo(VerseRef other)
		{
			return CompareTo(other, false); // by default, compare only initial verses in a verse bridge
		}

		/// <summary>
		/// Compares two verse refs using the default canon. 
		/// </summary>
		/// <param name="other">the other VerseRef to compare to this one</param>
		/// <param name="compareAllVerses">if <c>true</c>, compare both the starting and, if it exists, 
		/// the ending verse in a verse bridge; if <c>false</c> compare only the first verse number in a bridge</param>
		[PublicAPI]
		public int CompareTo(VerseRef other, bool compareAllVerses)
		{
			return CompareTo(other, null, compareAllVerses, true);
		}

		/// <summary>
		/// Compares two verse refs using the default canon.
		/// </summary>
		/// <param name="other">the other VerseRef to compare to this one</param>
		/// <param name="segmentOrder">ordered array of verse segments or null to use default segment
		/// comparison (Unicode value)</param>
		/// <param name="compareAllVerses">if <c>true</c>, compare both the starting and, if it exists,
		/// the ending verse in a verse bridge; if <c>false</c> compare only the first verse number in a bridge</param>
		/// <param name="compareSegments">if set to <c>true</c> compare segments; <c>false</c> to ignore 
		/// segment differences.</param>
		/// <returns></returns>
		[PublicAPI]
		public int CompareTo(VerseRef other, string[] segmentOrder, bool compareAllVerses, bool compareSegments)
		{
			if (other.Versification != Versification)
			{
				if (!string.IsNullOrEmpty(other.verse) &&
					(other.verse.IndexOf(verseRangeSeparator) != -1 || other.verse.IndexOf(verseSequenceIndicator) != -1))
				{
					other.ChangeVersificationWithRanges(Versification);
				}
				else
					other.ChangeVersification(Versification);
			}

			if (bookNum != other.bookNum)
				return bookNum - other.bookNum;
			if (chapterNum != other.chapterNum)
				return chapterNum - other.chapterNum;
			if (compareAllVerses)
			{
				// compare all available verses (whether a single verse or a verse bridge)
				return CompareVerses(other);
			}
			// REVIEW: Using this method of comparison, two verse references where
			// one is a verse bridge and the other is the first part of that bridge
			// (e.g. MAT 1:2-3 and MAT 1:2) will be considered equal. However, the
			// Equals method will not consider those two references equal. Is this
			// the behavior we want?)

			// compare only the first verse bridge
			if (verseNum != other.verseNum)
				return verseNum - other.verseNum;

			if (!compareSegments)
				return 0;

			// Comparing same verse so get segment order for this verse
			string thisSegment = Segment(segmentOrder);
			string otherSegment = other.Segment(segmentOrder);
			if (string.IsNullOrEmpty(thisSegment) && string.IsNullOrEmpty(otherSegment))
				return 0;
			if (string.IsNullOrEmpty(thisSegment) && !string.IsNullOrEmpty(otherSegment))
				return -1;
			if (!string.IsNullOrEmpty(thisSegment) && string.IsNullOrEmpty(otherSegment))
				return 1;

			if (segmentOrder != null)
			{
				// Both verses have segments. Compare according to custom order.
				string[] verseSegOrder = GetSegments(segmentOrder);
				int thisVerseSegIndex = Array.IndexOf(verseSegOrder, thisSegment);
				int otherVerseSegIndex = Array.IndexOf(verseSegOrder, otherSegment);
				return thisVerseSegIndex - otherVerseSegIndex;
			}
			// REVIEW: is this comparison adequate for a customized verse segment definition?
			return string.Compare(thisSegment, otherSegment, StringComparison.Ordinal);
		}

		/// <summary>
		/// Compare the verses with verses in otherVerse (verses can be a single verse or a verse bridge).
		/// </summary>
		int CompareVerses(VerseRef otherVerse)
		{
			List<int> verseList = GetVerses();
			List<int> otherVerseList = otherVerse.GetVerses();

			for (int i = 0; i < verseList.Count && i < otherVerseList.Count; i++)
			{
				if (verseList[i] != otherVerseList[i])
					return verseList[i] - otherVerseList[i];
			}

			return verseList.Count - otherVerseList.Count;
		}

		/// <summary>
		/// GetVerses gets a list of verses from the verses specified in this VerseRef.
		/// </summary>
		private List<int> GetVerses()
		{
			// Get verses from the verse strings
			List<int> verseList = new List<int>();
			if (string.IsNullOrEmpty(verse))
			{
				verseList.Add(verseNum); // no bridge or segment info included in verse
				return verseList;
			}

			StringBuilder verseStr = new StringBuilder();
			for (int ich = 0; ich < verse.Length; ich++)
			{
				if (char.IsDigit(verse[ich]))
					verseStr.Append(verse[ich]);
				else if (verseStr.Length > 0)
				{
					verseList.Add(int.Parse(verseStr.ToString()));
					verseStr.Remove(0, verseStr.Length); // clear verse string for next use
				}
			}

			if (verseStr.Length > 0)
				verseList.Add(int.Parse(verseStr.ToString())); // add any accumulated digits
			return verseList;
		}

		public static bool operator <(VerseRef a, VerseRef b)
		{
			return a.CompareTo(b) < 0;
		}

		public static bool operator >(VerseRef a, VerseRef b)
		{
			return a.CompareTo(b) > 0;
		}

		public static bool operator >=(VerseRef a, VerseRef b)
		{
			return a.CompareTo(b) >= 0;
		}

		public static bool operator <=(VerseRef a, VerseRef b)
		{
			return a.CompareTo(b) <= 0;
		}
		#endregion

		#region Methods for parsing and comparing verse numbers

		/// <summary>
		/// True if there is any overlap between these two verse numbers.
		/// Verse numbers may be ranges with the form \d+[^-\s]*(-\d+\S*)?
		/// This is used when we have a list of verses and we wish to extract
		/// the ones that match a given reference.
		/// Examples:
		///	 1, 1: true
		///	 1b, 1-2c: true
		///	 1-3, 2-4: true
		///		1a, 1b: false
		/// </summary>
		/// <param name="verse1">first verse, e.g. 1, 1a, 1-2c</param>
		/// <param name="verse2">second verse</param>
		/// <returns></returns>
		public static bool AreOverlappingVersesRanges(string verse1, string verse2)
		{
			string[] verse1Parts = verse1.Split(verseSequenceIndicator);
			string[] verse2Parts = verse2.Split(verseSequenceIndicator);

			foreach (string verse1Part in verse1Parts)
			{
				foreach (string verse2Part in verse2Parts)
				{
					int verse1Num, verse1EndNum, verse2Num, verse2EndNum;
					string verse1Seg, verse1EndSeg, verse2Seg, verse2EndSeg;

					ParseVerseNumberRange(verse1Part, out verse1Num, out verse1Seg, out verse1EndNum, out verse1EndSeg);
					ParseVerseNumberRange(verse2Part, out verse2Num, out verse2Seg, out verse2EndNum, out verse2EndSeg);

					if (verse1Num == verse1EndNum && verse2Num == verse2EndNum
						&& verse1Seg == verse1EndSeg && verse2Seg == verse2EndSeg)
					{
						// no ranges, this is easy
						if (verse1Num == verse2Num && (verse1Seg == "" || verse2Seg == "" || verse1Seg == verse2Seg))
							return true;
					}
					else
					{
						if (InVerseRange(verse1Num, verse1Seg, verse2Num, verse2Seg, verse2EndNum, verse2EndSeg))
							return true;

						if (InVerseRange(verse1EndNum, verse1EndSeg, verse2Num, verse2Seg, verse2EndNum, verse2EndSeg))
							return true;

						if (InVerseRange(verse2Num, verse2Seg, verse1Num, verse1Seg, verse1EndNum, verse1EndSeg))
							return true;

						if (InVerseRange(verse2EndNum, verse2EndSeg, verse1Num, verse1Seg, verse1EndNum, verse1EndSeg))
							return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// True if there is any overlap between these two verse references
		/// </summary>
		public static bool AreOverlappingVersesRanges(VerseRef verseRef1, VerseRef verseRef2)
		{
			if (verseRef1.IsDefault || verseRef1.IsDefault)
				return false;

			Debug.Assert(verseRef1.Versification == verseRef2.Versification,
						 "Versification of verse references does not match");

			// Check simple cases first
			if (verseRef1.BookNum != verseRef2.BookNum || verseRef1.ChapterNum != verseRef2.ChapterNum)
				return false;

			// If both verse references are not complex, then we just need to check for verse number equality
			if (string.IsNullOrEmpty(verseRef1.verse) && string.IsNullOrEmpty(verseRef2.verse))
				return verseRef1.VerseNum == verseRef2.VerseNum;

			return AreOverlappingVersesRanges(verseRef1.Verse, verseRef2.Verse);
		}

		// true if the verse1 (a number and a (possibly empty) verse seg)
		// lies between verse2 and verse2 end inclusive.
		// Examples:
		//	1a, 1b, 1c: false
		//	1, 1, 1: true
		//	1b, 1a, 2: true
		static bool InVerseRange(int verse1, string verse1Seg,
								 int verse2, string verse2Seg,
								 int verse2End, string verse2EndSeg)
		{
			if (verse1 < verse2)
				return false;

			if (verse1 == verse2 && verse1Seg != "" && verse2Seg != "")
			{
				if (string.CompareOrdinal(verse1Seg, verse2Seg) < 0)
					return false;
			}

			if (verse1 > verse2End)
				return false;

			if (verse1 == verse2End && verse1Seg != "" && verse2EndSeg != "")
			{
				if (string.CompareOrdinal(verse1Seg, verse2EndSeg) > 0)
					return false;
			}

			return true;
		}

		// break verse number with an optional range into two parts
		// Examples:
		//   1 -> 1, "", 1, ""
		//   1a-2  ->  1, "a", 2, ""
		static void ParseVerseNumberRange(string vNum,
										  out int number1, out string segment1,
										  out int number2, out string segment2)
		{
			string[] parts = vNum.Split(verseRangeSeparator, '\u2013', '\u2014');
			if (parts.Length == 1)
			{
				ParseVerseNumber(parts[0], out number1, out segment1);
				number2 = number1;
				segment2 = segment1;
				return;
			}

			ParseVerseNumber(parts[0], out number1, out segment1);
			ParseVerseNumber(parts[1], out number2, out segment2);
		}

		// Break a verse number into a number and an optional segment
		// Examples:
		//	1  ->  1, ""
		//	1a ->  1, "a"
		static void ParseVerseNumber(string vNum, out int number, out string segment)
		{
			int j;
			for (j = 0; j < vNum.Length && char.IsDigit(vNum[j]); ++j)
			{
			}

			number = 0;
			if (j > 0)
			{
				string num = vNum.Substring(0, j);
				int.TryParse(num, out number); // Can't fail, we have already validated digits
			}

			segment = vNum.Substring(j);
		}

		/// <summary>
		/// Parses a verse string and gets the leading numeric portion as a number.
		/// Functionally identical to Verse.Set for Roman numbers, made distinct to preserve USX standard.
		/// </summary>
		/// <returns><c>true</c> if the entire string could be parsed as a single,
		/// simple verse number in any supported script; <c>false</c> if the verse string represented
		/// a verse bridge, contained segment letters, or was invalid</returns>
		public bool TrySetVerseUnicode(string value)
		{
			return TrySetVerse(value, false);
		}

		/// <summary>
		/// Returns whether any of the specified references overlap with this one
		/// </summary>
		/// <see cref="AreOverlappingVersesRanges(VerseRef,VerseRef)"/>
		public bool OverlapsAny(params VerseRef[] compareTo)
		{
			VerseRef temp = this;
			return compareTo.Any(vref => AreOverlappingVersesRanges(temp, vref));
		}

		/// <summary>
		/// Determines if this reference falls in the specified range of verses.
		/// </summary>
		/// <param name="rangeStart">The start of the range (inclusive)</param>
		/// <param name="rangeEnd">The end of the range (inclusive)</param>
		/// <param name="exact">True to require an exact match of the range start or 
		/// range end if it does not fall strictly between the range start and end; 
		/// False to accept partial overlaps at the start or end of the range</param>
		public bool InRange(VerseRef rangeStart, VerseRef rangeEnd, bool exact)
		{
			if (rangeStart < this && this < rangeEnd)
				return true;

			if (exact)
				return rangeStart.Equals(this) || rangeEnd.Equals(this);

			return BBBCCCVVV == rangeStart.BBBCCCVVV || BBBCCCVVV == rangeEnd.BBBCCCVVV ||
				   OverlapsAny(rangeStart, rangeEnd);
		}

		#endregion

		#region Other public methods

		/// <summary>
		/// Gets the reference as a comparable integer where the book,
		/// chapter, and verse each occupy three digits.
		/// </summary>
		public static int GetBBBCCCVVV(int bookNum, int chapterNum, int verseNum)
		{
			return (bookNum % bcvMaxValue) * bookDigitShifter +
				   (chapterNum >= 0 ? (chapterNum % bcvMaxValue) * chapterDigitShifter : 0) +
				   (verseNum >= 0 ? (verseNum % bcvMaxValue) : 0);
		}

		/// <summary>
		/// Enumerate all individual verses contained in a VerseRef.
		/// Verse ranges are indicated by "-" and consecutive verses by ","s.
		/// Examples:
		/// GEN 1:2 returns GEN 1:2
		/// GEN 1:1a-3b,5 returns GEN 1:1a, GEN 1:2, GEN 1:3b, GEN 1:5
		/// GEN 1:2a-2c returns //! ??????
		/// </summary>
		/// <param name="specifiedVersesOnly">if set to <c>true</c> return only verses that are explicitly specified only,
		/// not verses within a range.</param>
		/// <returns></returns>
		public IEnumerable<VerseRef> AllVerses(bool specifiedVersesOnly = false)
		{
			return AllVerses(specifiedVersesOnly, verseRangeSeparators, verseSequenceIndicators);
		}

		public IEnumerable<VerseRef> AllVerses(bool specifiedVersesOnly, string[] verseRangeSeparators,
											   string[] verseSequenceSeparators)
		{
			if (verse == null || ChapterNum <= 0)
				yield return Clone();
			else
			{
				VerseRef vref;
				int book = BookNum;
				int chapter = ChapterNum;

				string[] parts = verse.Split(verseSequenceSeparators, StringSplitOptions.None);
				foreach (
					string[] pieces in parts.Select(part => part.Split(verseRangeSeparators, StringSplitOptions.None)))
				{
					vref = Clone();
					vref.Verse = pieces[0];
					int startVerse = vref.VerseNum;
					yield return vref;

					if (pieces.Length > 1)
					{
						VerseRef vlast = Clone();
						vlast.Verse = pieces[1];

						if (!specifiedVersesOnly)
						{
							// get all verses within a range
							for (int verseNum = startVerse + 1; verseNum < vlast.VerseNum; verseNum++)
							{
								VerseRef verseInRange = new VerseRef(book, chapter, verseNum, versification);
								if (!verseInRange.IsExcluded)
									yield return new VerseRef(book, chapter, verseNum, versification);
							}
						}
						yield return vlast;
					}
				}
			}
		}

		/// <summary>
		/// Gets the single verses or verse ranges that are represented in this verse.
		/// </summary>
		public IEnumerable<VerseRef> GetRanges()
		{
			if (verse == null || ChapterNum <= 0)
				yield return Clone();
			else
			{
				string[] ranges = verse.Split(',');
				foreach (string range in ranges)
				{
					VerseRef vRef = Clone();
					vRef.Verse = range;
					yield return vRef;
				}
			}
		}

		/// <summary>
		/// Tests if the string can be parsed into a verse reference 
		/// </summary>
		/// <param name="str"></param>
		/// <returns>true if parsable</returns>
		public static bool IsParseable(string str)
		{
			if (string.IsNullOrEmpty(str))
				return false;

			VerseRef dummy;
			return TryParse(str, out dummy);
		}

		/// <summary>
		/// Determines if the verse string is in a valid format (does not consider versification).
		/// </summary>
		[PublicAPI]
		public static bool IsVerseParseable(string verse)
		{
			return verse.Length != 0 && char.IsDigit(verse[0]) && verse[verse.Length - 1] != verseRangeSeparator &&
				   verse[verse.Length - 1] != verseSequenceIndicator;
		}

		/// <summary>
		/// Tries to parse the specified string into a verse reference
		/// </summary>
		/// <param name="str">The string to attempt to parse</param>
		/// <param name="vref">The result of the parse if successful, or null if it failed</param>
		/// <returns>True if the specified string was successfully parsed, false otherwise</returns>
		[PublicAPI]
		public static bool TryParse(string str, out VerseRef vref)
		{
			try
			{
				vref = new VerseRef(str);
				return true;
			}
			catch (VerseRefException)
			{
				vref = new VerseRef();
				return false;
			}
		}

		/// <summary>
		/// Validates a verse number using the supplied separators rather than the defaults.
		/// </summary>
		[PublicAPI]
		public ValidStatusType ValidateVerse(string[] verseRangeSeparators, string[] verseSequenceSeparators)
		{
			if (string.IsNullOrEmpty(verse))
				return InternalValid;

			int prevVerse = 0;
			foreach (VerseRef vRef in AllVerses(true, verseRangeSeparators, verseSequenceSeparators))
			{
				ValidStatusType validStatus = vRef.InternalValid;
				if (validStatus != ValidStatusType.Valid)
					return validStatus;

				int bbbcccvvv = vRef.BBBCCCVVV;
				if (prevVerse > bbbcccvvv)
					return ValidStatusType.VerseOutOfOrder;
				if (prevVerse == bbbcccvvv)
					return ValidStatusType.VerseRepeated;
				prevVerse = bbbcccvvv;
			}
			return ValidStatusType.Valid; // TODO: make Valid tests Valid Status tests
		}

		#endregion

		#region IScrVerseRef-specific implementation
		public IScrVerseRef Create(string book, string chapter, string verse)
		{
			return new VerseRef(book, chapter, verse, Versification);
		}

		IScrVerseRef IScrVerseRef.Clone() => Clone();

		IScrVerseRef IScrVerseRef.UnBridge() => UnBridge();

		public bool VersificationHasVerseSegments => Versification.HasVerseSegments;
		#endregion
	}

	#region VerseRefException class

	/// <summary>
	/// Indicates a problem in the verse reference.
	/// </summary>
	[Serializable]
	public class VerseRefException : ApplicationException
	{
		public VerseRefException(string message) : base(message)
		{
		}

		public string InvalidVerseRef;
	}

	#endregion
}
