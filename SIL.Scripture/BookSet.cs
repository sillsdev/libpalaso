using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIL.Scripture
{
	/// <summary>
	/// Traditionally Paratext stored selected sets of books as ascii strings
	/// of 1's and 0's. This class wraps this representation to allow manipulating
	/// and iterating through this structure.
	/// </summary>
	public sealed class BookSet
	{
		#region Fields and Xtors
		private readonly bool[] selected = new bool[Canon.AllBookIds.Length];

		// constant to simplify filtered operations code
		public static readonly BookSet AllBooks = new BookSet(true);

		// The empty BookSet.
		public static readonly BookSet Empty = new BookSet();

		public BookSet()
		{
		}

		public BookSet(params string[] books)
		{
			if (books != null && books.Length == 1 && books[0].Length != 3)
				Books = books[0];
			else
				Add(books);
		}

		public BookSet(IEnumerable<int> bookNums)
		{
			foreach (int bookNum in bookNums)
				Add(bookNum);
		}

		public BookSet(BookSet bset)
		{
			Books = bset.Books;
		}

		public BookSet(int singleBook)
		{
			selected[singleBook - 1] = true;
		}

		public BookSet(int firstBook, int lastBook)
		{
			for (int book = firstBook; book <= lastBook; book++)
				selected[book - 1] = true;
		}

		/// <summary>
		/// Used to make filter sets for all books or no books.
		/// </summary>
		/// <param name="all">true means make a set of all books, false means make a set of no books</param>
		public BookSet(bool all)
		{
			if (all)
				SelectAll();
		}
		#endregion

		#region Conversion, Query, Summary
		/// <summary>
		/// Convert to/from a string of ascii 1's and 0's.
		/// </summary>
		public string Books
		{
			get
			{
				StringBuilder strBldr = new StringBuilder(selected.Length);
				foreach (bool sel in selected)
					strBldr.Append(sel ? "1" : "0");
				return strBldr.ToString();
			}
			set
			{
				Clear();
				for (int i = 0; value != null && i < value.Length; i++)
					if (value[i] == '1')
						Add(i + 1);
			}
		}
		
		/// <summary>
		/// Return a string giving all Ids of selected books,
		/// e.g "MAT, MRK, REV"
		/// </summary>
		/// <returns>list of books</returns>
		public string AllIds()
		{
			var result = new StringBuilder();

			for (int j = 0; j < selected.Length; ++j)
			{
				if (selected[j])
				{
					string name = Canon.BookNumberToId(j + 1);
					result.Append(name).Append(", ");
				}
			}

			if (result.Length != 0)
				result.Remove(result.Length - 2, 2);

			return result.ToString();
		}

		public bool IsSelected(int bookNum)
		{
			return bookNum >= 1 && bookNum <= selected.Length && selected[bookNum - 1];
		}

		/// <summary>
		/// Return first selected book number, 0 if none
		/// </summary>
		public int FirstSelectedBookNum
		{
			get
			{
				for (int i = 0; i < selected.Length; ++i)
					if (selected[i])
						return i + 1;

				return 0;
			}
		}

		/// <summary>
		/// Return last selected book number, 0 if none
		/// </summary>
		/// <returns></returns>
		public int LastSelectedBookNum
		{
			get
			{
				int bookNum = 0;

				for (int i = 0; i < selected.Length; ++i)
					if (selected[i])
						bookNum = i + 1;

				return bookNum;
			}
		}

		/// <summary>
		/// Number of books selected.
		/// </summary>
		public int Count
		{
			get { return selected.Count(val => val); }
		}

		#endregion

		#region Set operations
		public void Add(int bookNum)
		{
			selected[bookNum - 1] = true;
		}

		public void Add(params string[] bookIds)
		{
			foreach (var bookId in bookIds)
				selected[Canon.BookIdToNumber(bookId) - 1] = true;
		}

		public void Add(BookSet other)
		{
			foreach (int bookNum in other.SelectedBookNumbers)
				Add(bookNum);
		}

		/// <summary>
		/// Remove a single book number from BookSet.
		/// </summary>
		/// <param name="bookNum">book number to remove</param>
		public void Remove(int bookNum)
		{
			selected[bookNum - 1] = false;
		}

		/// <summary>
		/// Remove a single book from BookSet.
		/// </summary>
		/// <param name="bookId">BookId (e.g. "GEN") to remove</param>
		public void Remove(string bookId)
		{
			selected[Canon.BookIdToNumber(bookId) - 1] = false;
		}

		/// <summary>
		/// Remove all the books in the "other" BookSet from this BookSet.
		/// </summary>
		/// <param name="other"></param>
		public void Remove(BookSet other)
		{
			foreach (int bookNum in other.SelectedBookNumbers)
				Remove(bookNum);
		}


		public void Clear()
		{
			for (int i = 0; i < selected.Length; ++i)
				selected[i] = false;
		}

		public void SelectAll()
		{
			for (int i = 0; i < selected.Length; ++i)
				selected[i] = true;
		}

		/// <summary>
		/// Intersects two book sets
		/// </summary>
		/// <param name="other"></param>
		/// <returns>books that are present in both</returns>
		public BookSet Intersect(BookSet other)
		{
			BookSet bookSet = new BookSet();
			foreach (int bookNum in other.SelectedBookNumbers)
				if (IsSelected(bookNum))
					bookSet.Add(bookNum);
			return bookSet;
		}

		#endregion

		#region Filtered Navigation
		/// <summary>
		/// Returns the number of the next book after bookNum that is selected. If there are none,
		/// returns bookNum (even if it is not itself selected...it's up to the programmer to check
		/// that before passing it).
		/// </summary>
		/// <param name="bookNum">Current book. Start the search from the book after this.</param>
		/// <returns>Number of next selected book in this set, or bookNum if none found.</returns>
		public int NextSelected(int bookNum)
		{
			if (bookNum >= Canon.LastBook)
				return Canon.LastBook; //constrain to range
			for (int i = bookNum + 1; i <= Canon.LastBook; i++)
			{
				if (IsSelected(i))
					return i;
			}
			return bookNum;
		}

		/// <summary>
		/// Returns the number of the first book before bookNum that is selected. If there are none,
		/// returns bookNum (even if it is not itself selected...it's up to the programmer to check
		/// that before passing it).
		/// </summary>
		/// <param name="bookNum">Current book. Start the search from the book before this.</param>
		/// <returns>Number of previous selected book in this set, or bookNum if none found.</returns>
		public int PreviousSelected(int bookNum)
		{
			if (bookNum <= Canon.FirstBook)
				return Canon.FirstBook; //constrain to range
			for (int i = bookNum - 1; i >= Canon.FirstBook; i--)
			{
				if (IsSelected(i))
					return i;
			}
			return bookNum;
		}

		#endregion

		#region Iteration
		public IEnumerable<int> SelectedBookNumbers
		{
			get
			{
				for (int i = 0; i < selected.Length; ++i)
					if (selected[i])
						yield return i + 1;
			}
		}

		public IEnumerable<string> SelectedBookIds
		{
			get
			{
				for (int i = 0; i < selected.Length; ++i)
					if (selected[i])
						yield return Canon.BookNumberToId(i + 1);
			}
		}
		#endregion

		#region Other public methods
		/// <summary>
		/// Creates a book set containing books in verses
		/// </summary>
		public static BookSet CreateBookSetFromRefs(IEnumerable<VerseRef> verseRefs)
		{
			BookSet books = new BookSet();

			foreach (var verseRef in verseRefs)
				books.Add(verseRef.BookNum);

			return books;
		}
		#endregion

		#region Overrides of Object
		public override int GetHashCode()
		{
			return Books.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			BookSet other = (obj as BookSet);
			if (other == null)
				return false;

			return selected.SequenceEqual(other.selected);
		}

		public override string ToString()
		{
			return string.Join(", ", SelectedBookIds) + ": " + Books;
		}
		#endregion
	}
}
