using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SIL.Extensions;
using SIL.Scripture.Properties;

namespace SIL.Scripture
{
	/// <summary>
	/// Manages internal information for a versification. You should use the <see cref="ScrVers"/> class to
	/// access the versification information.
	/// </summary>
	public sealed class Versification
	{
		#region Constants/Member variables
		public const int NonCanonicalLastChapterOrVerse = 998;

		private static readonly Regex versificationNameMatcher =
			new Regex("^#\\s*Versification\\s+\"(?<name>[^\"]+)\"\\s*$", RegexOptions.Compiled);

		private BookSet scriptureBookSet;
		#endregion

		#region Member variables
		private readonly string name;
		private readonly List<int[]> bookList;

		/// <summary>Mapping to and from standard versification</summary>
		private readonly VerseMappings mappings;
		/// <summary>Excluded verses are represented as BBBCCCVVV integers so lookup with segments will be handled correctly</summary>
		private readonly HashSet<int> excludedVerses;
		/// <summary>Verses with segments are represented as BBBCCCVVV integers so lookup with segments will be handled correctly</summary>
		private readonly Dictionary<int, string[]> verseSegments;

		private string description;
		#endregion

		#region Construct/Initialize
		/// <summary>
		/// Creates a new Versification with the specified name
		/// </summary>
		private Versification(string versName, string fullPath)
		{
			name = versName;
			Type = Table.GetVersificationType(versName);
			FullPath = fullPath;

			bookList = new List<int[]>();
			mappings = new VerseMappings();
			excludedVerses = new HashSet<int>();
			verseSegments = new Dictionary<int, string[]>();
		}

		/// <summary>
		/// Creates a copy of another Versification
		/// </summary>
		private Versification(Versification baseVersification, string newName, string fullPath)
		{
			if (baseVersification == null)
				throw new ArgumentNullException("baseVersification");

			name = newName;
			FullPath = fullPath;
			BaseVersification = baseVersification;
			Type = ScrVersType.Unknown;
			description = baseVersification.description;
			bookList = new List<int[]>(baseVersification.bookList);
			mappings = new VerseMappings(baseVersification.mappings);
			excludedVerses = new HashSet<int>(baseVersification.excludedVerses);
			verseSegments = new Dictionary<int, string[]>(baseVersification.verseSegments);
		}

		private void Clear()
		{
			bookList.Clear();
			mappings.Clear();
			excludedVerses.Clear();
			verseSegments.Clear();
		}
		#endregion

		#region Internal properties
		/// <summary>
		/// Gets the name of this versification
		/// </summary>
		internal string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Gets the base versification of this customized versification or null if this versification is
		/// not customized.
		/// </summary>
		internal Versification BaseVersification { get; private set; }

		/// <summary>
		/// Gets the full path for this versification file (e.g. \My Paratext Projects\eng.vrs).
		/// <para>Note that this will be null for built-in versifications since they are stored as embedded resources.</para>
		/// </summary>
		internal string FullPath { get; private set; }

		/// <summary>
		/// Is versification file for this versification present
		/// </summary>
		internal bool IsPresent
		{
			get { return Table.Implementation.VersificationFileExists(Name); }
		}

		/// <summary>
		/// Gets whether or not this versification is created from a custom VRS file that overrides
		/// a default base versification
		/// </summary>
		internal bool IsCustomized
		{
			get { return BaseVersification != null; }
		}
		
		/// <summary>
		/// Gets the type of versification.
		/// </summary>
		internal ScrVersType Type { get; private set; }

		/// <summary>
		/// Gets whether the current versification has verse segment information.
		/// </summary>
		internal bool HasVerseSegments
		{
			get { return verseSegments != null && verseSegments.Count > 0; }
		}

		/// <summary>
		/// All books which are valid in this scripture text.
		/// Valid means a) is a cannonical book, b) not obsolete, c) present in the versification for this text
		/// </summary>
		internal BookSet ScriptureBooks
		{
			get
			{
				if (scriptureBookSet == null)
				{
					scriptureBookSet = new BookSet();
					foreach (int bookNum in Canon.ScriptureBooks.SelectedBookNumbers
						.Where(bookNum => LastChapter(bookNum) != 1 || LastVerse(bookNum, 1) != 1))
					{
						scriptureBookSet.Add(bookNum);
					}
				}
				return scriptureBookSet;
			}
		}
		#endregion

		#region Internal methods
		/// <summary>
		/// Gets last book in this project
		/// </summary>
		internal int LastBook()
		{
			return (bookList != null) ? bookList.Count : 0;
		}

		/// <summary>
		/// Gets last chapter number in this book.
		/// </summary>
		internal int LastChapter(int bookNum)
		{
			// Non-scripture books have 998 chapters
			if (!Canon.IsCanonical(bookNum))
				return NonCanonicalLastChapterOrVerse; // Use 998 so the VerseRef.BBBCCCVVV value is computed properly

			// Anything else not in .vrs file has 1 chapter
			if (bookNum > bookList.Count)
				return 1;

			int[] chapters = bookList[bookNum - 1];
			return chapters.Length;
		}

		/// <summary>
		/// Gets last verse number in this book/chapter.
		/// </summary>
		internal int LastVerse(int bookNum, int chapterNum)
		{
			// Non-scripture books have 998 verses in each chapter
			if (!Canon.IsCanonical(bookNum))
				return NonCanonicalLastChapterOrVerse; // Use 998 so the VerseRef.BBBCCCVVV value is computed properly

			// Anything else not in .vrs file has 1 chapter
			if (bookNum > bookList.Count)
				return 1;

			int[] chapters = bookList[bookNum - 1];
			if (chapterNum > chapters.Length || chapterNum < 1)
				return 1;

			return chapters[chapterNum - 1];
		}

		/// <summary>
		/// Determines whether the specified verse is excluded in the versification.
		/// </summary>
		internal bool IsExcluded(int bbbcccvvv)
		{
			return excludedVerses.Contains(bbbcccvvv);
		}

		/// <summary>
		/// Gets a list of verse segments for the specified reference or null if the specified
		/// reference does not have segments defined in the versification.
		/// </summary>
		internal string[] VerseSegments(int bbbcccvvv)
		{
			string[] segments;
			if (verseSegments.TryGetValue(bbbcccvvv, out segments))
				return segments;

			return null;
		}

		/// <summary>
		/// Change the versification of an entry with Verse like 1-3 or 1,3a.
		/// Can't really work in the most general case because the verse parts could become separate chapters.
		/// </summary>
		/// <returns>true if successful (i.e. all verses were in the same the same chapter in the new versification),
		/// false if the changing resulted in the reference spanning chapters (which makes the results undefined)</returns>
		internal bool ChangeVersificationWithRanges(VerseRef vref, out VerseRef newRef)
		{
			VerseRef vref2 = vref;

			string[] parts = Regex.Split(vref.Verse, @"([,\-])");

			vref.Verse = parts[0];
			ChangeVersification(ref vref);
			bool allSameChapter = true;

			for (int i = 2; i < parts.Length; i += 2)
			{
				VerseRef vref3 = vref2;
				vref3.Verse = parts[i];
				ChangeVersification(ref vref3);
				allSameChapter &= vref.ChapterNum == vref3.ChapterNum;

				vref.Verse = vref.Verse + parts[i - 1] + vref3.Verse;
			}

			newRef = vref;
			return allSameChapter;
		}

		/// <summary>
		/// Change the passed VerseRef to be this versification.
		/// </summary>
		internal void ChangeVersification(ref VerseRef vref)
		{
			if (vref.IsDefault || vref.Versification == null || vref.Versification.VersInfo == this)
			{
				vref.Versification = new ScrVers(this);
				return;
			}

			Debug.Assert(!vref.HasMultiple, "Use ChangeVersificationWithRanges");

			Versification origVersification = vref.Versification.VersInfo;

			// Map from existing to standard versification

			VerseRef origVerse = vref;
			origVerse.Versification = null;
			VerseRef standardVerse;
			if (origVersification.mappings != null)
				standardVerse = origVersification.mappings.GetStandard(origVerse) ?? origVerse;
			else
				standardVerse = origVerse;

			// If both versifications contain this verse and
			// map this verse to the same location then no versification change is needed.
			// This test is present in order to prevent a verse being changed when you have a many to one mapping from
			// a versification to a standard versification (e.g. FB-17661)
			VerseRef standardVerseThisVersification;
			if (mappings != null)
				standardVerseThisVersification = mappings.GetStandard(origVerse) ?? origVerse;
			else
				standardVerseThisVersification = origVerse;

			// ESG is a specicial case since we have added mappings from verses to LXX segments in several versifications and
			// want this mapping to work both ways.
			if (vref.Book != "ESG" && standardVerse.Equals(standardVerseThisVersification) && BookChapterVerseExists(vref))
			{
				vref.Versification = new ScrVers(this);
				return;
			}

			// Map from standard versification to this versification
			VerseRef newVerse;
			if (mappings != null)
				newVerse = mappings.GetVers(standardVerse) ?? standardVerse;
			else
				newVerse = standardVerse;

			// If verse has changed, parse new value
			if (!origVerse.Equals(newVerse))
				vref.CopyFrom(newVerse);

			vref.Versification = new ScrVers(this);
		}

		private bool BookChapterVerseExists(VerseRef vref)
		{
			return vref.BookNum <= LastBook() &&
				   vref.ChapterNum <= LastChapter(vref.BookNum) &&
				   vref.VerseNum <= LastVerse(vref.BookNum, vref.ChapterNum);
		}

		/// <summary>
		/// Write out versification information to the specified stream.
		/// </summary>
		internal void WriteToStream(StringWriter stream)
		{
			// Write out the list of books, chapters, verses
			stream.WriteLine("# List of books, chapters, verses");
			stream.WriteLine("# One line per book.");
			stream.WriteLine("# One entry for each chapter.");
			stream.WriteLine("# Verse number is the maximum verse number for that chapter.");

			for (int book = 0; book < bookList.Count; book++)
			{
				int[] versesInChapter = bookList[book];
				stream.Write(Canon.BookNumberToId(book + 1));
				for (int chap = 0; chap < versesInChapter.Length; chap++)
					stream.Write(" " + (chap + 1) + Table.chapVerseSep + versesInChapter[chap]);

				stream.WriteLine();
			}

			// Write out the mappings, if any
			stream.WriteLine("#");
			stream.WriteLine("# Mappings from this versification to standard versification");

			Dictionary<VerseRef, VerseRef> mappingRanges = mappings.GetMappingRanges();
			foreach (KeyValuePair<VerseRef, VerseRef> mappingRange in mappingRanges)
				stream.WriteLine(mappingRange.Key + " = " + mappingRange.Value);

			// Write out excluded verses, if any
			stream.WriteLine("#");
			stream.WriteLine("# Excluded verses");

			foreach (int bbbcccvvv in excludedVerses)
				stream.WriteLine("#! -" + new VerseRef(bbbcccvvv));

			// Write out verse segment information, if any
			stream.WriteLine("#");
			stream.WriteLine("# Verse segment information");

			foreach (KeyValuePair<int, string[]> verseSegPair in verseSegments)
			{
				stream.Write("#! *" + new VerseRef(verseSegPair.Key));
				foreach (string seg in verseSegPair.Value)
					stream.Write("," + (seg.Length == 0 ? "-" : seg));
				stream.WriteLine();
			}
		}

		/// <summary>
		/// Get the string description of the versification.
		/// </summary>
		public override string ToString()
		{
			if (description != null)
				return description;

			description = FullPath;  // set default;
			if (!string.IsNullOrEmpty(FullPath) && File.Exists(FullPath))
			{
				using (TextReader reader = new StreamReader(FullPath))
				{
					string text;
					while ((text = reader.ReadLine()) != null)
					{
						Match match = versificationNameMatcher.Match(text);
						if (match.Success)
						{
							description = match.Groups[1].Value;
							break;
						}
					}
				}
			}

			if (string.IsNullOrEmpty(description))
				description = Name;
			return description;
		}

		public override bool Equals(object obj)
		{
			var other = obj as Versification;
			if (other == null)
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			return name == other.name && description == other.description &&
					bookList.SequenceEqual(other.bookList, new IntArrayComparer()) &&
					excludedVerses.KeyedSetsEqual(other.excludedVerses) &&
					verseSegments.KeyedSetsEqual(other.verseSegments) &&
					mappings.Equals(other.mappings);
		}

		public override int GetHashCode()
		{
			return name.GetHashCode();
		}

		private sealed class IntArrayComparer : IEqualityComparer<int[]>
		{
			public bool Equals(int[] x, int[] y)
			{
				return x.Length == y.Length && x.SequenceEqual(y);
			}

			public int GetHashCode(int[] obj)
			{
				return obj.GetHashCode();
			}
		}
		#endregion

		#region Table class
		/// <summary>
		/// Provides public access to the list of versifications
		/// </summary>
		public class Table
		{
			#region Constants
			// Symbols used in parsing lines from a versification file
			private const char commentSymbol = '#';
			private const char excludedSymbol = '-';
			private const char segmentSymbol = '*';
			private const char unspecifiedSegSymbol = '-';
			private const char segmentSep = ',';
			private const char mappingSymbol = '=';
			private const char versExtensionSymbol = '!';
			internal const char chapVerseSep = ':';
			#endregion

			#region Member variables
			private static readonly Dictionary<string, ScrVersType> stringToTypeMap = new Dictionary<string, ScrVersType>();

			public static Table Implementation = new Table();

			private readonly Dictionary<VersificationKey, Versification> versifications = 
				new Dictionary<VersificationKey, Versification>();
			#endregion

			#region Static constructor
			static Table()
			{
				foreach (ScrVersType type in Enum.GetValues(typeof(ScrVersType)))
					stringToTypeMap[type.ToString()] = type;
			}
			#endregion

			#region Public methods
			/// <summary>
			/// True iff named versification exists
			/// </summary>
			public bool Exists(string versName)
			{
				ScrVersType versificationType = GetVersificationType(versName);
				if (versificationType != ScrVersType.Unknown)
					return true;

				lock (versifications)
					return versifications.ContainsKey(new VersificationKey(ScrVersType.Unknown, versName));
			}

			public virtual bool VersificationFileExists(string versName)
			{
				if (!Exists(versName))
					return false;

				Versification versification = Get(versName);
				if (versification.FullPath != null)
					return File.Exists(versification.FullPath);

				// If not a known type and it doesn't have a path, then assume it's an invalid versification.
				return typeof(ScrVersType).IsEnumDefined(versName);
			}

			/// <summary>
			/// Removes all versifications that have an unknown type (i.e. all versifications that are not built-in).
			/// Mostly used for testing purposes.
			/// </summary>
			public void RemoveAllUnknownVersifications()
			{
				lock (versifications)
				{
					foreach (ScrVers ver in VersificationTables())
					{
						if (ver.Type == ScrVersType.Unknown)
							versifications.Remove(new VersificationKey(ScrVersType.Unknown, ver.Name));
					}
				}
			}

			/// <summary>
			/// Gets all versification schemes.
			/// </summary>
			public IEnumerable<ScrVers> VersificationTables()
			{
				yield return ScrVers.English;
				yield return ScrVers.Original;
				yield return ScrVers.Septuagint;
				yield return ScrVers.Vulgate;
				yield return ScrVers.RussianOrthodox;
				yield return ScrVers.RussianProtestant;

				List<Versification> versificationList;
				lock (versifications)
					versificationList = versifications.Values.ToList();

				foreach (var versification in versificationList.Where(v => v.Type == ScrVersType.Unknown))
					yield return new ScrVers(versification);
			}

			/// <summary>
			/// Reload all non-standard, non-ad-hoc versifications used so far.
			/// This is necessary after a versification file has changed.
			/// </summary>
			public void ReloadVersifications()
			{
				lock (versifications)
				{
					foreach (Versification versificationReadonly in versifications.Values)
					{
						Versification versification = versificationReadonly;
						// REVIEW: This version doesn't seem to support customized versifications (i.e., it reloads them without the base).
						if (string.IsNullOrEmpty(versification.FullPath) ||
							!File.Exists(versification.FullPath))
							continue; // Don't reload versifications that don't have backing files

						versification.Clear();
						Load(versification.FullPath, versification.Type, ref versification);
					}
				}
			}

			/// <summary>
			/// Loads the specified versification file and returns the results. The
			/// versification is ad-hoc (not loaded into the versification map).
			/// </summary>
			public ScrVers Load(string fullPath, string fallbackName = null)
			{
				Versification versification = null;
				Load(fullPath, ScrVersType.Unknown, ref versification, fallbackName);
				return new ScrVers(versification);
			}

			/// <summary>
			/// Loads a versification from the specified stream (with no base versification). The
			/// versification is ad-hoc (not loaded into the versification map).
			/// </summary>
			public ScrVers Load(TextReader stream, string fullPath, string fallbackName = null)
			{
				Versification versification = null;
				Load(stream, fullPath, ScrVersType.Unknown, ref versification, fallbackName);
				return new ScrVers(versification);
			}

			/// <summary>
			/// Loads a versification from the specified stream while overriding a base versification. 
			/// The versification is loaded into the versification map so any calls to get a versification of that name 
			/// will return the same versification.
			/// </summary>
			public ScrVers Load(TextReader stream, string fullPath, ScrVers baseVers, string name)
			{
				if (string.IsNullOrEmpty(name))
					throw new ArgumentNullException("name");

				if (baseVers == null)
					throw new ArgumentNullException("baseVers");

				if (baseVers.IsCustomized)
					throw new InvalidOperationException("Can not create a custom versification from customized versification " + baseVers.Name);

				Versification versification;
				lock (versifications)
				{
					versification = new Versification(baseVers.VersInfo, name, fullPath);
					Load(stream, fullPath, ScrVersType.Unknown, ref versification, name);
					versifications.Add(new VersificationKey(ScrVersType.Unknown, name), versification);
				}
				return new ScrVers(versification);
			}

			public static ParsedVersificationLine ParseLine(string line)
			{
				line = line.Trim();
				bool isCommentLine = (line.Length > 0 && line[0] == commentSymbol);
				string[] parts = line.Split(new[] { commentSymbol }, 2);
				line = parts[0].Trim();
				string comment = parts.Length == 2 ? parts[1].Trim() : string.Empty;

				LineType lineType;
				if (line == string.Empty && comment.Length > 2 && comment[0] == versExtensionSymbol)
				{
					line = comment.Substring(1).Trim(); // found Paratext 7.3(+) versification line beginning with #!
					comment = "";
					isCommentLine = false;
				}

				if (line.Length == 0 || isCommentLine)
					lineType = LineType.comment;
				else if (line.Contains(mappingSymbol))
				{
					// mapping one verse to multiple
					lineType = line[0] == '&' ? LineType.oneToManyMapping : LineType.standardMapping;
				}
				else if (line[0] == excludedSymbol)
					lineType = LineType.excludedVerse;
				else if (line[0] == segmentSymbol)
					lineType = LineType.verseSegments;
				else
					lineType = LineType.chapterVerse;

				return new ParsedVersificationLine(lineType, line, comment);
			}
			#endregion

			/// <summary>
			/// Override this to handle a versification line error besides just throwing it
			/// </summary>
			/// <returns>True if the exception was handled, false otherwise (meaning it will be thrown)</returns>
			protected virtual bool HandleVersificationLineError(InvalidVersificationLineException ex)
			{
				return false;
			}

			#region Private/internal methods
			/// <summary>
			/// Get the versification table for this versification
			/// </summary>
			internal Versification Get(ScrVersType type)
			{
				lock (versifications)
				{
					VersificationKey key = CreateKey(type, "");
					Versification versification;
					if (versifications.TryGetValue(key, out versification))
						return versification;

					string resourceFileText;
					switch (type)
					{
						case ScrVersType.Original: resourceFileText = Resources.org_vrs; break;
						case ScrVersType.English: resourceFileText = Resources.eng_vrs; break;
						case ScrVersType.Septuagint: resourceFileText = Resources.lxx_vrs; break;
						case ScrVersType.Vulgate: resourceFileText = Resources.vul_vrs; break;
						case ScrVersType.RussianOrthodox: resourceFileText = Resources.rso_vrs; break;
						case ScrVersType.RussianProtestant: resourceFileText = Resources.rsc_vrs; break;
						default: throw new InvalidOperationException("Can not create a versification for an unknown type");
					}
					
					versification = new Versification(type.ToString(), null);
					using (TextReader fallbackVersificationStream = new StringReader(resourceFileText))
						ReadVersificationFile(fallbackVersificationStream, null, type, ref versification);
					versifications[key] = versification;
					return versification;
				}
			}

			/// <summary>
			/// Gets the versification with the specified name. This can be a built-in versification or a custom one.
			/// </summary>
			protected internal virtual Versification Get(string versName)
			{
				if (string.IsNullOrEmpty(versName))
					throw new ArgumentNullException("versName");

				ScrVersType type = GetVersificationType(versName);
				if (type != ScrVersType.Unknown)
					return Get(type);

				lock (versifications)
				{
					VersificationKey key = CreateKey(ScrVersType.Unknown, versName);
					Versification versification;
					if (versifications.TryGetValue(key, out versification))
						return versification;

					versification = new Versification(versName, null);
					using (TextReader fallbackVersificationStream = new StringReader(Resources.eng_vrs))
						ReadVersificationFile(fallbackVersificationStream, null, ScrVersType.Unknown, ref versification);
					versifications[key] = versification;
					return versification;
				}
			}

			/// <summary>
			/// Get the versification type given its name. If it is not a standard
			/// versification type, return Unknown.
			/// </summary>
			internal static ScrVersType GetVersificationType(string versName)
			{
				ScrVersType type;
				return stringToTypeMap.TryGetValue(versName, out type) ? type : ScrVersType.Unknown;
			}

			private void Load(string filePath, ScrVersType type, ref Versification versification, string fallbackName = null)
			{
				using (TextReader stream = new StreamReader(filePath))
					Load(stream, filePath, type, ref versification, fallbackName);
			}

			private void Load(TextReader stream, string filePath, ScrVersType type, ref Versification versification, string fallbackName)
			{
				ReadVersificationFile(stream, filePath, type, ref versification, fallbackName);
			}

			/// <summary>
			/// Read versification file and "add" its entries.
			/// At the moment we only do this once. Eventually we will call this twice.
			/// Once for the standard versification, once for custom entries in versification.vrs
			/// file for this project.
			/// </summary>
			private void ReadVersificationFile(TextReader stream, string filePath, ScrVersType type,
				ref Versification versification, string fallbackName = null)
			{
				// Parse the lines in the versification file
				foreach (string line in GetLines(stream))
				{
					try
					{
						ProcessVersLine(line, filePath, type, fallbackName, ref versification);
					}
					catch (InvalidVersificationLineException ex)
					{
						if (!HandleVersificationLineError(ex))
							throw;
					}
				}
			}

			/// <summary>
			/// Read lines from a file into a list of strings.
			/// </summary>
			private static IEnumerable<string> GetLines(TextReader reader)
			{
				List<string> lines = new List<string>();
				for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
					lines.Add(line);
				return lines;
			}

			/// <summary>
			/// Process a line from a versification file.
			/// </summary>
			/// <param name="line">line of text in the file</param>
			/// <param name="filePath">full path to the versification file (if loaded from a file)</param>
			/// <param name="type"></param>
			/// <param name="fallbackName">Optional name to use if no name is found inside the file.</param>
			/// <param name="versification">Existing versification (being reloaded) or null (loading a new one)</param>
			private void ProcessVersLine(string line, string filePath, ScrVersType type, string fallbackName, ref Versification versification)
			{
				if (versification == null)
				{
					string name = null;
					var match = versificationNameMatcher.Match(line);
					if (match.Success)
						name = match.Groups["name"].Value;

					if (!string.IsNullOrEmpty(name))
					{
						versification = new Versification(name, filePath);
						versifications[CreateKey(type, name)] = versification;
					}
				}

				ParsedVersificationLine parsedLine = ParseLine(line);
				if (parsedLine.LineType == LineType.comment)
					return;

				if (versification == null)
				{
					if (!string.IsNullOrEmpty(fallbackName))
					{
						versification = new Versification(fallbackName, filePath);
						versifications[CreateKey(type, fallbackName)] = versification;
					}
					else
						throw new InvalidVersificationLineException(VersificationLoadErrorType.MissingName, parsedLine.Line, filePath);
				}

				switch (parsedLine.LineType)
				{
					case LineType.comment:
						break;
					case LineType.chapterVerse:
						ParseChapterVerseLine(filePath, versification, parsedLine.Line);
						break;
					case LineType.standardMapping:
						ParseMappingLine(filePath, versification, parsedLine.Line);
						break;
					case LineType.oneToManyMapping:
						ParseRangeToOneMappingLine(filePath, versification, parsedLine.Line);
						break;
					case LineType.excludedVerse:
						ParseExcludedVerseLine(filePath, versification, parsedLine.Line);
						break;
					case LineType.verseSegments:
						if (parsedLine.Line.IndexOf(commentSymbol) != -1)
							throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidSyntax, parsedLine.Line, filePath);
						ParseVerseSegmentsLine(filePath, versification, parsedLine.Line);
						break;
				}
			}

			/// <summary>
			/// Parse lines mapping from this versification to standard versification. For example:
			///   GEN 1:10 = GEN 2:11
			///   GEN 1:10-13 = GEN 2:11-14
			/// </summary>
			private static void ParseChapterVerseLine(string fileName, Versification versification, string line)
			{
				string[] parts = line.Split(' ');
				int bookNum = Canon.BookIdToNumber(parts[0]);
				if (bookNum == 0)
					throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidSyntax, line, fileName);

				while (versification.bookList.Count < bookNum)
					versification.bookList.Add(new[] { 1 });

				// Initialize to previous list of verses, if any.
				List<int> versesInChapter = new List<int>(versification.bookList[bookNum - 1]);

				int chapter = 0;
				for (int i = 1; i < parts.Length; ++i)
				{
					// END is used if the number of chapters in custom is less than base
					if (parts[i] == "END")
					{
						if (versesInChapter.Count > chapter)
							versesInChapter.RemoveRange(chapter, versesInChapter.Count - chapter);
						break;
					}

					string[] pieces = parts[i].Split(chapVerseSep);

					if (!int.TryParse(pieces[0], out chapter) || chapter <= 0)
						throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidSyntax, line, fileName);

					int verseCount;
					if (pieces.Length != 2 || !int.TryParse(pieces[1], out verseCount) || verseCount < 0)
						throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidSyntax, line, fileName);

					if (versesInChapter.Count < chapter)
					{
						for (int iChapter = versesInChapter.Count; iChapter < chapter; iChapter++)
							versesInChapter.Add(1); // by default, chapters have one verse
					}
					versesInChapter[chapter - 1] = verseCount;
				}

				versification.bookList[bookNum - 1] = versesInChapter.ToArray();
			}

			/// <summary>
			/// Parse lines indicating excluded verse numbers, like:
			///  -GEN 1:5
			/// </summary>
			private static void ParseExcludedVerseLine(string fileName, Versification scrVers, string line)
			{
				line = line.Trim();
				if (line.Length < 8 || line[0] != excludedSymbol || !line.Contains(chapVerseSep) || !line.Contains(' '))
					throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidSyntax, line, fileName);

				string[] parts = line.Split(' ');
				try
				{
					// Get Scripture reference, throwing an exception if it is not valid.
					string bookName;
					int chapter, verse;
					GetVerseReference(parts, out bookName, out chapter, out verse);

					VerseRef verseRef = new VerseRef(bookName, chapter.ToString(), verse.ToString(), new ScrVers(scrVers));
					if (!scrVers.excludedVerses.Contains(verseRef.BBBCCCVVV))
						scrVers.excludedVerses.Add(verseRef.BBBCCCVVV);
					else
						throw new InvalidVersificationLineException(VersificationLoadErrorType.DuplicateExcludedVerse, line, fileName);
				}
				catch (InvalidVersificationLineException)
				{
					throw;
				}
				catch
				{
					throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidSyntax, line, fileName);
				}
			}

			/// <summary>
			/// Parse lines specifying segments for a specific verse, like:
			///  *GEN 1:5,-,a,b,c,d,e,f
			/// </summary>
			private static void ParseVerseSegmentsLine(string fileName, Versification scrVers, string line)
			{
				line = line.Trim();
				if (line.Length < 8 || line[0] != segmentSymbol || !line.Contains(chapVerseSep) ||
					!line.Contains(' ') || !line.Contains(segmentSep))
					throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidSyntax, line, fileName);

				int indexOfColon = line.IndexOf(':');
				line = RemoveSpaces(line, indexOfColon);

				string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				try
				{
					// Get segmenting information
					int segmentStart = parts[1].IndexOf(segmentSep);
					if (segmentStart == -1)
						throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidSyntax, line, fileName);

					string segments = parts[1].Substring(segmentStart + 1);

					// Get Scripture reference, throwing an exception if it is not valid.
					string bookName;
					int chapter, verse;
					parts[1] = parts[1].Substring(0, segmentStart);
					// Remove segment info from chapter:verse reference
					GetVerseReference(parts, out bookName, out chapter, out verse);

					List<string> segmentList = new List<string>();
					bool nonEmptySegmentFound = false;
					foreach (string seg in segments.Split(segmentSep))
					{
						if (string.IsNullOrEmpty(seg))
							continue;
						if (nonEmptySegmentFound && seg == unspecifiedSegSymbol.ToString())
							throw new InvalidVersificationLineException(VersificationLoadErrorType.UnspecifiedSegmentLocation, line, fileName);
						if (seg == unspecifiedSegSymbol.ToString())
						{
							// '-' indicates no marking for segment
							segmentList.Add(string.Empty);
						}
						else
						{
							segmentList.Add(seg);
							nonEmptySegmentFound = true;
						}
					}

					if (segmentList.Count == 1 && string.IsNullOrEmpty(segmentList[0]))
						throw new InvalidVersificationLineException(VersificationLoadErrorType.NoSegmentsDefined, line, fileName);

					int bbbcccvvv = VerseRef.GetBBBCCCVVV(Canon.BookIdToNumber(bookName), chapter, verse);
					// Don't allow overwrites for built-in versifications
					if (fileName == null && scrVers.verseSegments.ContainsKey(bbbcccvvv))
						throw new InvalidVersificationLineException(VersificationLoadErrorType.DuplicateSegment, line);

					scrVers.verseSegments[bbbcccvvv] = segmentList.ToArray();
				}
				catch (InvalidVersificationLineException)
				{
					throw;
				}
				catch
				{
					throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidSyntax, line, fileName);
				}
			}

			/// <summary>
			/// Remove spaces from the specified index.
			/// </summary>
			private static string RemoveSpaces(string line, int index)
			{
				if (index < 1)
					throw new ArgumentException("Invalid index " + index);
				if (string.IsNullOrEmpty(line) || line.Length < 2)
					throw new ArgumentException("Invalid line");

				StringBuilder strBldr = new StringBuilder();
				strBldr.Append(line.Substring(0, index));
				string[] parts = line.Substring(index).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				foreach (string part in parts)
					strBldr.Append(part);
				return strBldr.ToString();
			}

			/// <summary>
			/// Parse lines giving a mapping from this versification to standard versification:
			///	 NUM 17:1-13 = NUM 17:16-28
			/// </summary>
			private static void ParseMappingLine(string fileName, Versification versification, string line)
			{
				try
				{
					string[] parts = line.Split(mappingSymbol);
					string[] leftPieces = parts[0].Trim().Split('-');
					string[] rightPieces = parts[1].Trim().Split('-');

					VerseRef newVerseRef = new VerseRef(leftPieces[0]);
					int leftLimit = leftPieces.Length == 1 ? 0 : int.Parse(leftPieces[1]);

					VerseRef standardVerseRef = new VerseRef(rightPieces[0]);

					while (true)
					{
						versification.mappings.AddMapping(newVerseRef.Clone(), standardVerseRef.Clone());

						if (newVerseRef.VerseNum >= leftLimit)
							break;

						newVerseRef.VerseNum++;
						standardVerseRef.VerseNum++;
					}
				}
				catch
				{
					throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidSyntax, line, fileName);
				}
			}

			/// <summary>
			/// Parse lines giving a mapping from this versification to standard versification where
			/// a range of verses is mapped to a single verse. For example:
			///	 NUM 17:1 = NUM 17:1-3
			///	 NUM 17:1-3 = NUM 17:1
			/// </summary>
			private static void ParseRangeToOneMappingLine(string fileName, Versification versification, string line)
			{
				line = line.Substring(1); // remove initial '&'

				VerseRef[] versRefs;
				VerseRef[] standardRefs;
				try
				{
					string[] parts = line.Split(mappingSymbol);
					string[] leftPieces = parts[0].Trim().Split('-');
					string[] rightPieces = parts[1].Trim().Split('-');

					versRefs = GetReferences(leftPieces);
					standardRefs = GetReferences(rightPieces);
				}
				catch (Exception)
				{
					throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidSyntax, line, fileName);
				}

				if (versRefs.Length != 1 && standardRefs.Length != 1) // either versification or standard must have just one verse
					throw new InvalidVersificationLineException(VersificationLoadErrorType.InvalidManyToOneMap, line, fileName);

				versification.mappings.AddMapping(versRefs, standardRefs);
			}

			/// <summary>
			/// Gets the reference(s) in a string array.
			/// </summary>
			private static VerseRef[] GetReferences(string[] versePieces)
			{
				if (versePieces.Length == 1)
					return new[] { new VerseRef(versePieces[0]) };

				VerseRef newVerseRef = new VerseRef(versePieces[0]);
				int limit = int.Parse(versePieces[1]);

				List<VerseRef> verseRefs = new List<VerseRef>();
				while (true)
				{
					verseRefs.Add(newVerseRef.Clone());
					if (newVerseRef.VerseNum >= limit)
						break;

					newVerseRef.VerseNum = newVerseRef.VerseNum + 1;
				}

				return verseRefs.ToArray();
			}

			/// <summary>
			/// Get a verse reference from an a string array. If the reference is not valid, an exception will be thrown.
			/// </summary>
			/// <param name="parts">a string arry. The first string should contain a three-letter book name. The second
			/// string should contain chapter:verse</param>
			/// <param name="bookName">[out] three-letter abbreviation for book</param>
			/// <param name="chapter">[out] chapter number</param>
			/// <param name="verse">[out] verse number</param>
			private static void GetVerseReference(string[] parts, out string bookName, out int chapter, out int verse)
			{
				bookName = parts[0].Substring(1);
				if (Canon.BookIdToNumber(bookName) == 0)
					throw new Exception();

				// Confirm that chapter and verse are valid numbers.
				string[] pieces = parts[1].Split(chapVerseSep);
				chapter = int.Parse(pieces[0]);
				verse = int.Parse(pieces[1]);
			}

			private VersificationKey CreateKey(ScrVersType type, string scrVersName)
			{
				return new VersificationKey(type, type == ScrVersType.Unknown ? scrVersName : "");
			}
			#endregion

			#region VersificationKey class
			private sealed class VersificationKey
			{
				private readonly ScrVersType type;
				private readonly string name;

				public VersificationKey(ScrVersType type, string name)
				{
					this.type = type;
					this.name = name;
				}

				public override int GetHashCode()
				{
					return type.GetHashCode() ^ name.GetHashCode();
				}

				public override bool Equals(object obj)
				{
					VersificationKey other = obj as VersificationKey;
					return other != null && other.type == type && other.name == name;
				}
			}
			#endregion
		}
		#endregion

		#region VerseMapping class
		/// <summary>
		/// Provides a bidirectional mapping from both the standard versification and a 
		/// specific versification.
		/// Although this class isn't needed from a design standpoint, it does help the
		/// readability of the code that uses it.
		/// </summary>
		private sealed class VerseMappings
		{
			private readonly Dictionary<VerseRef, VerseRef> versToStandard;
			private readonly Dictionary<VerseRef, VerseRef> standardToVers;

			/// <summary>
			/// Default constructor.
			/// </summary>
			public VerseMappings()
			{
				versToStandard = new Dictionary<VerseRef, VerseRef>(100);
				standardToVers = new Dictionary<VerseRef, VerseRef>(100);
			}

			/// <summary>
			/// Creates a copy of an original mapping.
			/// </summary>
			public VerseMappings(VerseMappings origMapping)
			{
				versToStandard = new Dictionary<VerseRef, VerseRef>(origMapping.versToStandard);
				standardToVers = new Dictionary<VerseRef, VerseRef>(origMapping.standardToVers);
			}

			/// <summary>
			/// Adds a new verse mapping. Calling this for an existing mapping will replace it.
			/// </summary>
			/// <param name="vers">The verse mapping for the specific versification</param>
			/// <param name="standard">The verse mapping for the standard versification</param>
			public void AddMapping(VerseRef vers, VerseRef standard)
			{
				if (vers.AllVerses().Count() != 1 || standard.AllVerses().Count() != 1)
					throw new ArgumentException("Mappings must resolve into a single reference on both sides.");

				// Want to compare references while ignoring versification
				standard.Versification = null;
				vers.Versification = null;

				versToStandard[vers] = standard;
				standardToVers[standard] = vers;
			}

			/// <summary>
			/// Adds a new verse mapping. Calling this for an existing mapping will replace it.
			/// </summary>
			/// <param name="vers">The verse mapping for the specific versification</param>
			/// <param name="standard">The verse mapping for the standard versification</param>
			public void AddMapping(VerseRef[] vers, VerseRef[] standard)
			{
				for (int iVers = vers.Length - 1; iVers >= 0; iVers--)
				{
					for (int iStandard = standard.Length - 1; iStandard >= 0; iStandard--)
						AddMapping(vers[iVers], standard[iStandard]);
				}
			}

			/// <summary>
			/// Gets the specific verse mapping for the specified standard verse mapping
			/// </summary>
			/// <param name="standard">The verse mapping for the standard versification</param>
			/// <returns>The found verse mapping for the specific versification (null if not found)</returns>
			public VerseRef? GetVers(VerseRef standard)
			{
				VerseRef vers;
				return standardToVers.TryGetValue(standard, out vers) ? (VerseRef?)vers : null;
			}

			/// <summary>
			/// Gets the standard verse mapping for the specified specific verse mapping
			/// </summary>
			/// <param name="vers">The verse mapping for the specific versification</param>
			/// <returns>The found verse mapping for the standard versification (null if not found)</returns>
			public VerseRef? GetStandard(VerseRef vers)
			{
				VerseRef standard;
				return versToStandard.TryGetValue(vers, out standard) ? (VerseRef?)standard : null;
			}

			/// <summary>
			/// Get the verse mappings as verse ranges rather than individual verses.
			/// </summary>
			/// <returns>a dictionary of mappings where the key is the verse or verse range in this versifcation; 
			/// value is a verse or verse range in the standard versification that it maps to</returns>
			public Dictionary<VerseRef, VerseRef> GetMappingRanges()
			{
				// Create a sorted list of all individual verse mappings 
				SortedDictionary<VerseRef, VerseRef> mappings = new SortedDictionary<VerseRef, VerseRef>(versToStandard);

				// For some strange reason, some versifications have multiple verses mapping to a single verse, so
				// we need to add any unaccounted for mappings from the other direction.
				foreach (KeyValuePair<VerseRef, VerseRef> data in standardToVers)
					mappings[data.Value] = data.Key;

				// Create a dictionary of mappings where we merge any contiguous verse mappings into a single mapping range
				Dictionary<VerseRef, VerseRef> mergedMappings = new Dictionary<VerseRef, VerseRef>();
				while (mappings.Count > 0)
				{
					KeyValuePair<VerseRef, VerseRef> versePair = mappings.First();
					VerseRef nextLeftVerse = versePair.Key.Clone();
					VerseRef nextRightVerse = versePair.Value.Clone();
					int lastLeftVerse, lastRightVerse;
					VerseRef mappedVerse;
					// Look for any contiguous verse mappings, keeping track of the last one.
					do
					{
						mappings.Remove(nextLeftVerse); // dealt with this mapping

						lastLeftVerse = nextLeftVerse.VerseNum;
						lastRightVerse = nextRightVerse.VerseNum;
						nextLeftVerse.VerseNum++;
						nextRightVerse.VerseNum++;
					}
					while (mappings.TryGetValue(nextLeftVerse, out mappedVerse) && mappedVerse.Equals(nextRightVerse));

					VerseRef leftMergedVerse = versePair.Key;
					VerseRef rightMergedVerse = versePair.Value;
					if (leftMergedVerse.VerseNum != lastLeftVerse)
					{
						// We found contigous verse mappings, so create a mapping range.
						Debug.Assert(rightMergedVerse.VerseNum != lastRightVerse);
						leftMergedVerse.Verse = leftMergedVerse.VerseNum + "-" + lastLeftVerse;
						rightMergedVerse.Verse = rightMergedVerse.VerseNum + "-" + lastRightVerse;
					}
					mergedMappings.Add(leftMergedVerse, rightMergedVerse);
				}

				return mergedMappings;
			}

			/// <summary>
			/// Called from tests with reflection to clear the mappings
			/// </summary>
			public void Clear()
			{
				versToStandard.Clear();
				standardToVers.Clear();
			}

			public override bool Equals(object obj)
			{
				var other = obj as VerseMappings;

				if (other == null)
					return false;

				return versToStandard.KeyedSetsEqual(other.versToStandard) &&
					standardToVers.KeyedSetsEqual(other.standardToVers);
			}

			public override int GetHashCode()
			{
				return standardToVers.GetHashCode() ^ versToStandard.GetHashCode();
			}
		}
		#endregion
	}

	#region VersificationLoadErrorType enum
	public enum VersificationLoadErrorType
	{
		MissingName,
		InvalidSyntax,
		DuplicateExcludedVerse,
		UnspecifiedSegmentLocation,
		NoSegmentsDefined,
		DuplicateSegment,
		InvalidManyToOneMap
	}
	#endregion

	#region InvalidVersificationLineException class
	[Serializable]
	public class InvalidVersificationLineException : Exception
	{
		public readonly VersificationLoadErrorType Type;
		public readonly string LineText;
		public readonly string FileName;

		public InvalidVersificationLineException(VersificationLoadErrorType type, string lineText = null, string fileName = null)
		{
			Type = type;
			LineText = lineText;
			FileName = fileName;
		}
	}
	#endregion

	#region ScrVersType enum
	/// <summary>
	/// List of versification types. Used mostly for backwards compatibility where just a 
	/// versification integer code was stored.
	/// <para>WARNING: The order of these items are very important as they correspond to the old, legacy codes.</para>
	/// </summary>
	public enum ScrVersType
	{
		/// <summary>
		/// This means the versification was loaded from a file or it is a custom versification based on a 
		/// built-in versification
		/// </summary>
		Unknown,
		Original,
		Septuagint,
		Vulgate,
		English,
		RussianProtestant,
		RussianOrthodox
	}
	#endregion

	#region LineType enumeration
	public enum LineType { comment, chapterVerse, standardMapping, oneToManyMapping, excludedVerse, verseSegments };
	#endregion

	#region ParsedVersificationLine class
	public sealed class ParsedVersificationLine
	{
		public ParsedVersificationLine(LineType lineType, string line, string comment)
		{
			LineType = lineType;
			Line = line.Trim();
			Comment = comment.Trim();
		}

		public LineType LineType { get; private set; }

		public string Line { get; private set; }

		public string Comment { get; private set; }

		public override string ToString()
		{
			switch (LineType)
			{
				case LineType.chapterVerse: return Line;
				case LineType.oneToManyMapping: return "#! " + Line;
				case LineType.comment:
					if (Comment != "")
						return "# " + Comment;
					return "";
				default: return Line + (Comment != "" ? " # " + Comment : "");
			}
		}
	}
	#endregion
}
