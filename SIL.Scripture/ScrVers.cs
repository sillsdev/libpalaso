using System;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace SIL.Scripture
{
	/// <summary>
	/// Accessor for getting information about a versification. This class has a small memory 
	/// footprint so multiple ScrVers objects can be created that point to the same versification;
	/// useful for deserialization of versification information.
	/// </summary>
	[Serializable]
	public sealed class ScrVers : IScrVers
	{
		#region Public static member variables
		/// <summary>Original versification</summary>
		public static readonly ScrVers Original = new ScrVers(ScrVersType.Original);

		/// <summary>Septuagint versification.</summary>
		public static readonly ScrVers Septuagint = new ScrVers(ScrVersType.Septuagint);

		/// <summary>Vulgate versification.</summary>
		public static readonly ScrVers Vulgate = new ScrVers(ScrVersType.Vulgate);

		/// <summary>English versification</summary>
		public static readonly ScrVers English = new ScrVers(ScrVersType.English);

		/// <summary>RussianProtestant versification</summary>
		public static readonly ScrVers RussianProtestant = new ScrVers(ScrVersType.RussianProtestant);

		/// <summary>RussianOrthodox versification</summary>
		public static readonly ScrVers RussianOrthodox = new ScrVers(ScrVersType.RussianOrthodox);
		#endregion

		#region Member variables
		private ScrVersType type;
		private Versification versInfo;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="ScrVers"/> class
		/// </summary>
		private ScrVers()
		{
			// Here for deserialization
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScrVers"/> class for the specified known type.
		/// </summary>
		public ScrVers(ScrVersType type)
		{
			this.type = type;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScrVers"/> class with the specified name. 
		/// Versification data will be loaded from the <see cref="Versification.Table"/> using the name to look it up.
		/// </summary>
		public ScrVers(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			Name = name;
		}
		
		/// <summary>
		/// Internal constructor for creating a ScrVers directly with a versification
		/// </summary>
		internal ScrVers(Versification versInfo)
		{
			this.versInfo = versInfo;
			type = versInfo.Type;
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Gets last book in this project
		/// </summary>
		public int GetLastBook()
		{
			return VersInfo.LastBook();
		}

		/// <summary>
		/// Gets last chapter number in the given book.
		/// </summary>
		public int GetLastChapter(int bookNum)
		{
			return VersInfo.LastChapter(bookNum);
		}

		/// <summary>
		/// Gets last verse number in the given book/chapter.
		/// </summary>
		public int GetLastVerse(int bookNum, int chapterNum)
		{
			return VersInfo.LastVerse(bookNum, chapterNum);
		}

		/// <summary>
		/// Gets first verse number in this book/chapter considering excluded verses.
		/// </summary>
		/// <returns>first verse in the specified book and chapter that is not excluded or
		/// returns <c>null</c> if no included verse left in book</returns>
		public VerseRef? FirstIncludedVerse(int bookNum, int chapterNum)
		{
			do
			{
				int lastVerse = GetLastVerse(bookNum, chapterNum);
				for (int verseNum = 1; verseNum <= lastVerse; verseNum++)
				{
					if (!IsExcluded(VerseRef.GetBBBCCCVVV(bookNum, chapterNum, verseNum)))
						return new VerseRef(bookNum, chapterNum, verseNum, this);
				}

				// Non-excluded verse not found in this chapter, so try next chapter
				chapterNum++;
			} while (chapterNum <= GetLastChapter(bookNum));

			return null;
		}

		/// <summary>
		/// Determines whether the specified verse is excluded in the versification.
		/// </summary>
		public bool IsExcluded(int bbbcccvvv)
		{
			return VersInfo != null && VersInfo.IsExcluded(bbbcccvvv);
		}

		/// <summary>
		/// Gets a list of verse segments for the specified reference or null if the specified
		/// reference does not have segments defined in the versification.
		/// </summary>
		public string[] VerseSegments(int bbbcccvvv)
		{
			return VersInfo.VerseSegments(bbbcccvvv);
		}

		/// <summary>
		/// Change the versification of an entry with Verse like 1-3 or 1,3a.
		/// Can't really work in the most general case because the verse parts could become separate chapters.
		/// </summary>
		/// <returns>true if successful (i.e. all verses were in the same the same chapter in the new versification),
		/// false if the changing resulted in the reference spanning chapters (which makes the results undefined)</returns>
		public bool ChangeVersificationWithRanges(VerseRef vref, out VerseRef newRef)
		{
			return VersInfo.ChangeVersificationWithRanges(vref, out newRef);
		}

		public int ChangeVersification(int bbbcccvvv, IScrVers otherVersification)
		{
			VerseRef vRef = new VerseRef(bbbcccvvv, (ScrVers)otherVersification);
			ChangeVersification(ref vRef);
			return vRef.BBBCCCVVV;
		}

		/// <summary>
		/// Change the passed VerseRef to be this versification.
		/// </summary>
		public void ChangeVersification(ref VerseRef vref)
		{
			VersInfo.ChangeVersification(ref vref);
		}

		/// <summary>
		/// Writes this versification to the specified fileName.
		/// </summary>
		/// <param name="fileName">full path where versification file should be written</param>
		/// <exception cref="IOException">if the fileName cannot be written</exception>
		public void Save(string fileName)
		{
			using (var writer = new StreamWriter(fileName, false))
				Save(writer);
		}

		/// <summary>
		/// Writes this versification to the specified fileName.
		/// </summary>
		/// <param name="writer">string writer to which the Versification contents should be written</param>
		public void Save(TextWriter writer)
		{
			VersInfo.WriteToStream(writer);
		}

		/// <summary>
		/// Get the string description of the versification.
		/// </summary>
		public override string ToString()
		{
			return VersInfo.ToString();
		}
		#endregion

		#region Public properties
		/// <summary>
		/// Gets the name of this ScrVers.
		/// <para>WARNING: This property should never be set except when deserialized.</para> 
		/// </summary>
		[XmlText]
		public string Name
		{
			get { return VersInfo.Name; }
			set
			{
				ScrVersType knownType = Versification.Table.GetVersificationType(value);
				if (knownType != ScrVersType.Unknown)
				{
					type = knownType;
					versInfo = null;
				}
				else
				{
					versInfo = Versification.Table.Implementation.Get(value);
					type = versInfo.Type;
				}
			}
		}

		/// <summary>
		/// Gets the full path for this versification file (e.g. \My Paratext Projects\eng.vrs)
		/// </summary>
		[XmlIgnore]
		public string FullPath
		{
			get { return VersInfo.FullPath; }
		}

		/// <summary>
		/// Is versification file for this versification present
		/// </summary>
		[XmlIgnore]
		public bool IsPresent
		{
			get { return VersInfo.IsPresent; }
		}

		/// <summary>
		/// Gets the type of versification.
		/// </summary>
		[XmlIgnore]
		public ScrVersType Type
		{
			get { return VersInfo.Type; }
		}

		/// <summary>
		/// Gets whether the current versification has verse segment information.
		/// </summary>
		[XmlIgnore]
		public bool HasVerseSegments
		{
			get { return VersInfo.HasVerseSegments; }
		}

		/// <summary>
		/// Gets whether or not this versification is created from a custom VRS file that overrides
		/// a default base versification
		/// </summary>
		[XmlIgnore]
		public bool IsCustomized
		{
			get { return VersInfo.IsCustomized; }
		}

		/// <summary>
		/// Gets the base versification of this customized versification or null if this versification is
		/// not customized.
		/// </summary>
		[XmlIgnore]
		public ScrVers BaseVersification
		{
			get
			{
				Versification versification = VersInfo.BaseVersification;
				return versification != null ? new ScrVers(versification) : null;
			}
		}


		/// <summary>
		/// All books which are valid in this scripture text.
		/// Valid means a) is a cannonical book, b) not obsolete, c) present in the versification for this text
		/// </summary>
		[XmlIgnore]
		public BookSet ScriptureBooks
		{
			get { return VersInfo.ScriptureBooks; }
		}
		#endregion

		#region Internal properties
		/// <summary>
		/// Gets the internal versification mapping (Should only be called from Versification)
		/// </summary>
		internal Versification VersInfo
		{
			get
			{
				if (versInfo == null)
					versInfo = Versification.Table.Implementation.Get(type);
				return versInfo;
			}
		}
		#endregion

		#region Operator overloads
		/// <summary>
		/// Determines if the specified ScrVers objects are equal to each other.
		/// They are considered equal if any of the following is true:
		/// <para>* They are both the same object</para>
		/// <para>* They are both null</para>
		/// <para>* The internal versification for both of them is the same object</para>
		/// </summary>
		public static bool operator ==(ScrVers scrVers1, ScrVers scrVers2)
		{
			// If both are null, or both are same instance, return true.
			if (ReferenceEquals(scrVers1, scrVers2))
				return true;

			// If one is null, check if the value of the other one is Unknown
			if ((object)scrVers1 == null || (object)scrVers2 == null)
				return false;
			
			// Using a reference comparison for the versInfo used to be safe because the
			// Versification.Table class guaranteed that only one instance of a
			// versification with the same name will exist.
			// However now it seems that the Versification table can be reloaded, 
			// at least for unittests, so I changed it to do value equals.
			// I doubt this will cause any noticable performance regressions.
			return scrVers1.VersInfo.Equals(scrVers2.VersInfo);
		}

		public static bool operator !=(ScrVers scrVers1, ScrVers scrVers2)
		{
			return !(scrVers1 == scrVers2);
		}

		public override bool Equals(object obj)
		{
			// Using a reference comparison for the versInfo used to be safe because the
			// Versification.Table class guaranteed that only one instance of a
			// versification with the same name will exist.
			// However now it seems that the Versification table can be reloaded, 
			// at least for unittests, so I changed to the do value equals.
			// I doubt this will cause any noticable performance regressions.

			ScrVers scrVers = obj as ScrVers;
			if (scrVers == null)
				return false;

			return scrVers.VersInfo != null && scrVers.VersInfo.Equals(VersInfo);
		}

		public override int GetHashCode()
		{
			return VersInfo == null ? 0 : versInfo.GetHashCode();
		}
		#endregion
	}
}
