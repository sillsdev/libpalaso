namespace SIL.Scripture
{
	public interface IScrVerseRef
	{
		/// <summary>
		/// Get or set Book based on book number.
		/// </summary>
		/// <exception cref="VerseRefException">If BookNum is set to an invalid value</exception>
		int BookNum { get; set; }
		/// <summary>
		/// Gets chapter number. -1 if not valid
		/// </summary>
		/// <exception cref="VerseRefException">If ChapterNum is negative</exception>
		int ChapterNum { get; set; }
		/// <summary>
		/// Gets verse start number. -1 if not valid
		/// </summary>
		/// <exception cref="VerseRefException">If VerseNum is negative</exception>
		int VerseNum { get; set; }

		/// <summary>
		/// Gets the book of the reference. This is the 
		/// three-letter abbreviation in capital letters. e.g. "MAT"
		/// </summary>
		string Book { get; }
		/// <summary>
		/// Gets the chapter of the reference. e.g. "3"
		/// </summary>
		string Chapter { get; }
		/// <summary>
		///  Gets the verse of the reference e.g. "11"
		/// </summary>
		/// <remarks>The USX standard only expects support for Latin numerals {0-9}* in verse numbers.</remarks>
		string Verse { get; }

		int LastChapter { get; }
		int LastVerse { get; }

		/// <summary>
		/// Gets whether the versification of this reference has verse segments
		/// </summary>
		bool VersificationHasVerseSegments { get; }

		/// <summary>
		/// Returns verse ref with no bridging, but maintaining segments like "1a".
		/// </summary>
		IScrVerseRef UnBridge();

		/// <summary>
		/// Simplifies this verse ref so that it has no bridging of verses or 
		/// verse segments like "1a".
		/// </summary>
		void Simplify();

		/// <summary>
		/// Gets a new object having the specified book, chapter and verse values (with the
		/// same versification).
		/// </summary>
		IScrVerseRef Create(string book, string chapter, string verse);

		/// <summary>
		/// Makes a clone of the reference
		/// </summary>
		IScrVerseRef Clone();

		/// <summary>
		/// Tries to move to the next book among a set of books present.
		/// </summary>
		/// <param name="present">Set of books present or selected.</param>
		/// <returns>true if successful</returns>
		bool NextBook(BookSet present);

		/// <summary>
		/// Tries to move to the next book in the entire canon superset.
		/// </summary>
		/// <returns>True if successful.</returns>
		bool NextBook();

		/// <summary>
		/// Tries to move to the previous book among a set of books present.
		/// </summary>
		/// <param name="present">Set of books present or selected.</param>
		/// <returns>true if successful</returns>
		bool PreviousBook(BookSet present);

		/// <summary>
		/// Tries to move to the previous book in the entire canon superset.
		/// </summary>
		/// <returns>true if successful</returns>
		bool PreviousBook();

		/// <summary>
		/// Tries to move to the next chapter.
		/// </summary>
		/// <param name="present">Set of books present or selected.</param>
		/// <returns>true if successful</returns>
		bool NextChapter(BookSet present);

		/// <summary>
		/// Tries to move to the next chapter.
		/// </summary>
		/// <returns>true if successful</returns>
		bool NextChapter();

		/// <summary>
		/// Tries to move to the previous chapter.
		/// </summary>
		/// <param name="present">Set of books present or selected.</param>
		/// <returns>true if successful</returns>
		bool PreviousChapter(BookSet present);

		/// <summary>
		/// Tries to move to the previous chapter.
		/// </summary>
		/// <returns>true if successful</returns>
		bool PreviousChapter();

		/// <summary>
		/// Tries to move to the next verse (or verse segment, if available in the current versification).
		/// </summary>
		/// <param name="present">Set of books present or selected.</param>
		/// <returns>true if successful, false if at end of scripture</returns>
		bool NextVerse(BookSet present);

		/// <summary>
		/// Tries to move to the next verse (or verse segment, if available in the current versification).
		/// </summary>
		/// <returns>true if successful, false if at end of scripture</returns>
		bool NextVerse();

		/// <summary>
		/// Tries to move to the previous verse (or verse segment, if available in the current versification).
		/// </summary>
		/// <param name="present">Set of books present or selected.</param>
		/// <returns>true if successful, false if at beginning of scripture</returns>
		bool PreviousVerse(BookSet present);

		/// <summary>
		/// Tries to move to the previous verse (or verse segment, if available in the current versification).
		/// </summary>
		/// <returns>true if successful, false if at beginning of scripture</returns>
		bool PreviousVerse();

		/// <summary>
		/// Advances to the last segment associated with this verse, if any.
		/// </summary>
		void AdvanceToLastSegment();
	}
}
