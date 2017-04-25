using System.Collections.Generic;
using System.Linq;

namespace SIL.Scripture
{
	/// <summary>
	/// Canon information. Also, contains
	/// static information on complete list of books and localization.
	/// </summary>
	public static class Canon
	{
		// Used for fast look up of book IDs to the book number
		private static readonly Dictionary<string, int> bookNumbers = new Dictionary<string, int>();
		private static BookSet scriptureBooks;
		private static BookSet allBooks;

		static Canon()
		{
			for (int i = 0; i < AllBookIds.Length; i++)
				bookNumbers[AllBookIds[i]] = i + 1;
		}

		/// <summary>
		/// Gets the 1-based number of the specified book
		/// </summary>
		/// <remarks>This is a fairly performance-critical method.</remarks>
		/// <returns>book number, or 0 if id doesn't exist</returns>
		public static int BookIdToNumber(string id, bool ignoreCase = true)
		{
			int result;
			bookNumbers.TryGetValue(ignoreCase ? id.ToUpperInvariant() : id, out result);
			//Debug.Assert(result != 0);
			return result; // result will be zero if TryGetValue fails
		}

		/// <summary>
		/// Check if a book id is valid
		/// </summary>
		/// <param name="id"> id to check</param>
		/// <returns> true if book id is valid</returns>
		public static bool IsBookIdValid(string id)
		{
			return BookIdToNumber(id) > 0;
		}

		/// <summary>
		/// Check if book id is in western NT
		/// </summary>
		public static bool IsBookNT(string id)
		{
			int num = BookIdToNumber(id);
			return IsBookNT(num);
		}

		public static bool IsBookNT(int num)
		{
			return (num >= 40) && (num <= 66);
		}

		/// <summary>
		/// Check if book id is in Protestant OT
		/// </summary>
		public static bool IsBookOT(string id)
		{
			int num = BookIdToNumber(id);
			return IsBookOT(num);
		}

		public static bool IsBookOT(int num)
		{
			return (num <= 39);
		}

		public static bool IsBookOTNT(int num)
		{
			return (num <= 66);
		}

		/// <summary>
		/// Check if book is in Deutero Canon
		/// </summary>
		public static bool IsBookDC(string id)
		{
			int num = BookIdToNumber(id);
			return IsBookDC(num);
		}

		public static bool IsBookDC(int num)
		{
			return IsCanonical(num) && !IsBookOTNT(num);
		}

		/// <summary>
		/// Enumerates all book numbers
		/// </summary>
		public static IEnumerable<int> AllBookNumbers
		{
			get
			{
				for (int i = 1; i <= AllBookIds.Length; i++)
					yield return i;
			}
		}

		/// <summary>
		/// Gets a bookset containing only scripture books, i.e. not XXA, etc.
		/// </summary>
		public static BookSet ScriptureBooks
		{
			get
			{
				if (scriptureBooks == null)
				{
					// This must be lazy as the BookSet constructor depends on Canon. If we try to initialize scriptureBooks
					// in the constructor, then we have a chicken-and-egg problem where the Canon can not initialize without a
					// BookSet and a BookSet can not initialize without a Canon.
					scriptureBooks = new BookSet();
					foreach (int bookNum in AllBookNumbers.Where(bookNum => IsCanonical(bookNum) && !IsObsolete(bookNum)))
						scriptureBooks.Add(bookNum);
				}
				return scriptureBooks;
			}
		}

		/// <summary>
		/// Gets a bookset containing all books that are not obsolete.
		/// </summary>
		public static BookSet AllBooks
		{
			get
			{
				if (allBooks == null)
				{
					allBooks = new BookSet();
					foreach (int bookNum in AllBookNumbers.Where(bookNum => !IsObsolete(bookNum)))
						allBooks.Add(bookNum);
				}
				return allBooks;
			}
		}

		/// <summary>
		/// Index of the first book. Abstracting this makes code less fragile.
		/// </summary>
		public static int FirstBook { get { return 1; } }

		/// <summary>
		/// Number of the last book (1-based).
		/// </summary>
		public static int LastBook { get { return AllBookIds.Length; } }

		public static IEnumerable<string> ExtraBooks
		{
			get { return new[] { "XXA", "XXB", "XXC", "XXD", "XXE", "XXF", "XXG" }; }
		}

		/// <summary>
		/// Gets the id if a book based on its 1-based number
		/// </summary>
		/// <param name="number">Book number (this is 1-based, not an index)</param>
		public static string BookNumberToId(int number)
		{
			return BookNumberToId(number, "***");
		}

		/// <summary>
		/// Gets the id if a book based on its 1-based number
		/// </summary>
		/// <param name="number">Book number (this is 1-based, not an index)</param>
		/// <param name="errorValue">The string to return if the book number does not
		/// correspond to a valid book</param>
		public static string BookNumberToId(int number, string errorValue)
		{
			int index = number - 1;

			if (index < 0 || index >= AllBookIds.Length)
				return errorValue;

			return AllBookIds[index];
		}
		
		public static string BookNumberToEnglishName(int number)
		{
			// FB 36586 restored check for bad data that was removed earlier in 7.5
			if (number <= 0 || number > LastBook)
				return "******";

			return allBookEnglishNames[number - 1];
		}

		public static string BookIdToEnglishName(string id)
		{
			return BookNumberToEnglishName(BookIdToNumber(id));
		}
		
		/// <summary>
		/// True if this is a canonical book id, as opposed to front matter etc.
		/// </summary>
		public static bool IsCanonical(string id)
		{
			return IsBookIdValid(id) && !NonCanonicalIds.Contains(id);
		}

		/// <summary>
		/// True if this is a canonical book number, as opposed to front matter etc.
		/// </summary>
		public static bool IsCanonical(int bookNum)
		{
			return IsCanonical(BookNumberToId(bookNum));
		}

		public static bool IsExtraMaterial(string id)
		{
			return IsBookIdValid(id) && NonCanonicalIds.Contains(id);
		}

		public static bool IsExtraMaterial(int bookNum)
		{
			return IsExtraMaterial(BookNumberToId(bookNum));
		}

		/// <summary>
		/// Array of all book ids.
		/// BE SURE TO UPDATE ISCANONICAL above whenever you change this array.
		/// </summary>
		public static readonly string[] AllBookIds = {
			"GEN",
			"EXO",
			"LEV",
			"NUM",
			"DEU",
			"JOS",
			"JDG",
			"RUT",
			"1SA",
			"2SA", // 10

			"1KI",
			"2KI",
			"1CH",
			"2CH",
			"EZR",
			"NEH",
			"EST",
			"JOB",
			"PSA",
			"PRO", // 20

			"ECC",
			"SNG",
			"ISA",
			"JER",
			"LAM",
			"EZK",
			"DAN",
			"HOS",
			"JOL",
			"AMO", // 30

			"OBA",
			"JON",
			"MIC",
			"NAM",
			"HAB",
			"ZEP",
			"HAG",
			"ZEC",
			"MAL",
			"MAT", // 40

			"MRK",
			"LUK",
			"JHN",
			"ACT",
			"ROM",
			"1CO",
			"2CO",
			"GAL",
			"EPH",
			"PHP", // 50

			"COL",
			"1TH",
			"2TH",
			"1TI",
			"2TI",
			"TIT",
			"PHM",
			"HEB",
			"JAS",
			"1PE", // 60

			"2PE",
			"1JN",
			"2JN",
			"3JN",
			"JUD",
			"REV",
			"TOB",
			"JDT",
			"ESG",
			"WIS", // 70

			"SIR",
			"BAR",
			"LJE",
			"S3Y",
			"SUS",
			"BEL",
			"1MA",
			"2MA",
			"3MA",
			"4MA", // 80

			"1ES",
			"2ES",
			"MAN",
			"PS2",
			"ODA",
			"PSS",
			"JSA",  // actual variant text for JOS, now in LXA text
			"JDB",  // actual variant text for JDG, now in LXA text
			"TBS",  // actual variant text for TOB, now in LXA text
			"SST",  // actual variant text for SUS, now in LXA text // 90

			"DNT",  // actual variant text for DAN, now in LXA text
			"BLT",  // actual variant text for BEL, now in LXA text
			"XXA",
			"XXB",
			"XXC",
			"XXD",
			"XXE",
			"XXF",
			"XXG",
			"FRT", // 100

			"BAK",
			"OTH",
			"3ES",   // Used previously but really should be 2ES 
			"EZA",   // Used to be called 4ES, but not actually in any known project
			"5EZ",   // Used to be called 5ES, but not actually in any known project  
			"6EZ",   // Used to be called 6ES, but not actually in any known project
			"INT",
			"CNC",
			"GLO",
			"TDX", // 110

			"NDX",
			"DAG",
			"PS3",
			"2BA",
			"LBA",
			"JUB",
			"ENO",
			"1MQ",
			"2MQ",
			"3MQ", // 120

			"REP",
			"4BA",
			"LAO"
		};

		public static readonly HashSet<string> NonCanonicalIds = new HashSet<string>(new[] {
			"XXA",
			"XXB",
			"XXC",
			"XXD",
			"XXE",
			"XXF",
			"XXG",
			"FRT",
			"BAK",
			"OTH",
			"INT",
			"CNC",
			"GLO",
			"TDX",
			"NDX"
		});

		/// <summary>
		/// Array of the English names of all books
		/// </summary>
		private static readonly string[] allBookEnglishNames = new[] {
			"Genesis",
			"Exodus",
			"Leviticus",
			"Numbers",
			"Deuteronomy",
			"Joshua",
			"Judges",
			"Ruth",
			"1 Samuel",
			"2 Samuel",

			"1 Kings",
			"2 Kings",
			"1 Chronicles",
			"2 Chronicles",
			"Ezra",
			"Nehemiah",
			"Esther (Hebrew)",
			"Job",
			"Psalms",
			"Proverbs",

			"Ecclesiastes",
			"Song of Songs",
			"Isaiah",
			"Jeremiah",
			"Lamentations",
			"Ezekiel",
			"Daniel (Hebrew)",
			"Hosea",
			"Joel",
			"Amos",

			"Obadiah",
			"Jonah",
			"Micah",
			"Nahum",
			"Habakkuk",
			"Zephaniah",
			"Haggai",
			"Zechariah",
			"Malachi",
			"Matthew",

			"Mark",
			"Luke",
			"John",
			"Acts",
			"Romans",
			"1 Corinthians",
			"2 Corinthians",
			"Galatians",
			"Ephesians",
			"Philippians",

			"Colossians",
			"1 Thessalonians",
			"2 Thessalonians",
			"1 Timothy",
			"2 Timothy",
			"Titus",
			"Philemon",
			"Hebrews",
			"James",
			"1 Peter",

			"2 Peter",
			"1 John",
			"2 John",
			"3 John",
			"Jude",
			"Revelation",
			"Tobit",
			"Judith",
			"Esther Greek",
			"Wisdom of Solomon",

			"Sirach (Ecclesiasticus)",
			"Baruch",
			"Letter of Jeremiah",
			"Song of 3 Young Men",
			"Susanna",
			"Bel and the Dragon",
			"1 Maccabees",
			"2 Maccabees",
			"3 Maccabees",
			"4 Maccabees",

			"1 Esdras (Greek)",
			"2 Esdras (Latin)",
			"Prayer of Manasseh",
			"Psalm 151",
			"Odes",
			"Psalms of Solomon",
			// WARNING, if you change the spelling of the *obsolete* tag be sure to update
			// IsObsolete routine
			"Joshua A. *obsolete*",
			"Judges B. *obsolete*",
			"Tobit S. *obsolete*",
			"Susanna Th. *obsolete*",

			"Daniel Th. *obsolete*",
			"Bel Th. *obsolete*",
			"Extra A",
			"Extra B",
			"Extra C",
			"Extra D",
			"Extra E",
			"Extra F",
			"Extra G",
			"Front Matter",

			"Back Matter",
			"Other Matter",
			"3 Ezra *obsolete*",
			"Apocalypse of Ezra",
			"5 Ezra (Latin Prologue)",
			"6 Ezra (Latin Epilogue)",
			"Introduction",
			"Concordance ",
			"Glossary ",
			"Topical Index",

			"Names Index",
			"Daniel Greek",
			"Psalms 152-155",
			"2 Baruch (Apocalypse)",
			"Letter of Baruch",
			"Jubilees",
			"Enoch",
			"1 Meqabyan",
			"2 Meqabyan",
			"3 Meqabyan",
			"Reproof (Proverbs 25-31)",

			"4 Baruch (Rest of Baruch)",
			"Laodiceans"
		};

		public static bool IsObsolete(int bookNum)
		{
			string name = allBookEnglishNames[bookNum - 1];
			return name.Contains("*obsolete*");
		}
	}
}
