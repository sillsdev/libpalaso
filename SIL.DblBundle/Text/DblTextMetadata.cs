using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SIL.ObjectModel;
using SIL.Scripture;
using SIL.WritingSystems;
using SIL.Xml;

namespace SIL.DblBundle.Text
{
	/// <summary>
	/// Information about a Digital Bible Library bundle
	/// </summary>
	[XmlRoot("DBLMetadata")]
	public class DblTextMetadata<TL> : DblMetadataBase<TL> where TL : DblMetadataLanguage, new()
	{
		/// <summary>Name and id for DBL bundle</summary>
		[XmlElement("identification")]
		public DblMetadataIdentification Identification { get; set; }

		/// <summary>The short written copyright statement which should accompany any publication of the scripture text.</summary>
		[XmlElement("copyright")]
		public DblMetadataCopyright Copyright { get; set; }

		/// <summary>Contains child elements for supplying background and promotion information related to the scripture text.</summary>
		[XmlElement("promotion")]
		public DblMetadataPromotion Promotion { get; set; }

		/// <summary>Date when DBL bundle was updated and originally archived</summary>
		[XmlElement("archiveStatus")]
		public DblMetadataArchiveStatus ArchiveStatus { get; set; }

		/// <summary>List of books in the DBL bundle</summary>
		[XmlArray("bookNames")]
		[XmlArrayItem("book")]
		public List<Book> AvailableBooks { get; set; }

		/// <summary>
		/// Gets the books that are available in this DBL bundle.
		/// </summary>
		public IReadOnlyList<Book> AvailableBibleBooks
		{
			get
			{
				return new ReadOnlyList<Book>(AvailableBooks.Where(b =>
				{
					var bookNum = BCVRef.BookToNumber(b.Code);
					return bookNum >= 1 && bookNum <= BCVRef.LastBook;
				}).ToList());
			}
		}

		/// <summary>
		/// Gets the canons used with this DBL bundle.
		/// </summary>
		[XmlArray("contents")]
		[XmlArrayItem("bookList")]
		public List<DblMetadataCanon> Canons { get; set; }

		/// <summary>
		/// Gets information about the DBL bundle in XML format.
		/// </summary>
		/// <returns></returns>
		public string GetAsXml()
		{
			return XmlSerializationHelper.SerializeToString(this);
		}

		/// <summary>
		/// Gets the name of the DBL bundle.
		/// </summary>
		public override string Name => Identification.Name;

		/// <summary>
		/// Gets the text representation for this DBL bundle as its name.
		/// </summary>
		public override string ToString()
		{
			if (Language.Iso == "sample")
				return Id;

			string identificationPart = Identification?.ToString() ?? Id;

			return $"{Language} - {identificationPart}";
		}
	}

	/// <summary>
	/// Identification information for a DBL bundle.
	/// </summary>
	public class DblMetadataIdentification
	{
		/// <summary>Preferred/common name used to refer to the scripture text (English)</summary>
		[XmlElement("name")]
		public string Name { get; set; }

		/// <summary>Preferred/common name used to refer to the scripture text (vernacular).</summary>
		[XmlElement("nameLocal")]
		public string NameLocal { get; set; }

		/// <summary>Identification for a DBL bundle</summary>
		[XmlElement("systemId")]
		public HashSet<DblMetadataSystemId> SystemIds { get; set; }

		/// <summary>Text representation for the DBL bundle</summary>
		public override string ToString()
		{
			return NameLocal == Name ? NameLocal : $"{NameLocal} ({Name})";
		}
	}

	/// <summary>
	/// Information about the language used in a Digital Bible Library bundle.
	/// </summary>
	public class DblMetadataLanguage
	{
		/// <summary>ISO 639-2 code for the language</summary>
		[XmlElement("iso")]
		public string Iso { get; set; }

		/// <summary>Language name</summary>
		[XmlElement("name")]
		public string Name { get; set; }

		/// <summary>
		/// Unicode language identifier. Not required, but may be needed in order to distinguish the language of the
		/// scripture text from a text in the same language,but from another region, or using another writing system
		/// (e.g. en-US vs en-GB). Paratext generated DBL text bundles are supplied this value from the ‘Language Identifier’
		/// field of the selected project’s properties.
		/// </summary>
		[XmlElement("ldml")]
		public string Ldml { get; set; }

		/// <summary>
		/// Dialect code taken from the Harvest Information Sysyems (HIS) Registry of Dialects (ROD).
		/// A ROD code may be needed(in addition to language/ldml) in order to distinguish the specific dialect of the
		/// language of the scripture text from others within the DBL.
		/// References: http://globalrecordings.net/research/rod
		/// </summary>
		[XmlElement("rod")]
		public string Rod { get; set; }

		/// <summary>Script used for the writing system</summary>
		[XmlElement("script")]
		public string Script { get; set; }

		/// <summary>Direction in which script is written (LTR or RTL)</summary>
		[XmlElement("scriptDirection")]
		[DefaultValue("LTR")]
		public string ScriptDirection { get; set; }

		/// <summary>
		/// Preferred numbering system to use for rendering digits in the scripture text.
		/// Publishers should use this information for providing the correct presentation of chapter and verse numbers,
		/// which are always provided in USX &lt;chapter&gt; and &lt;verse&gt; ‘num’ attributes in Arabic script.
		/// </summary>
		[XmlElement("numerals")]
		public string Numerals { get; set; }

		/// <summary>Name of language displayed in the UI</summary>
		[XmlIgnore]
		public string DisplayName
		{
			get
			{
				if (string.IsNullOrEmpty(Name))
					return Iso == WellKnownSubtags.UnlistedLanguage ? Localizer.GetString("DblBundle.UnknownLanguageName", "Unknown") : Iso;
				return string.IsNullOrEmpty(Iso) ? Name : $"{Name} ({Iso})";
			}
		}

		/// <summary>Text representation of bundle language</summary>
		public override string ToString()
		{
			return DisplayName;
		}
	}

	/// <summary>Information about the project copyright for the DBL bundle</summary>
	public class DblMetadataCopyright
	{
		/// <summary>Copyright statement in text and XML format</summary>
		[XmlElement("statement")]
		public DblMetadataXhtmlContentNode Statement { get; set; }
	}

	/// <summary>Introductory (promotional/background) information about the scripture text translation.</summary>
	public class DblMetadataPromotion
	{
		/// <summary>Indicates the specific content format which this element contains.</summary>
		[XmlElement("promoVersionInfo")]
		public DblMetadataXhtmlContentNode PromoVersionInfo { get; set; }

		/// <summary>
		/// A promotional email message text which can be used to send to a new user of the text
		/// (for example - when the text is downloaded within a publishing application for offline use).
		/// </summary>
		[XmlElement("promoEmail")]
		public DblMetadataXhtmlContentNode PromoEmail { get; set; }
	}

	/// <summary>
	/// Contains child elements for supplying one or more bookList elements in which divisions and book codes are supplied
	/// in the correct order for known publications of the scripture text.
	/// </summary>
	public class DblMetadataXhtmlContentNode
	{
		private string _value;

		/// <summary>
		/// Constructs a DBL content node
		/// </summary>
		public DblMetadataXhtmlContentNode()
		{
			ContentType = "xhtml";
		}

		/// <summary>Type of content, currently xhtml is supported</summary>
		[XmlAttribute("contentType")]
		public string ContentType { get; set; }

		/// <summary>Descendant nodes within this content node</summary>
		[XmlAnyElement]
		public XmlNode[] InternalNodes { get; set; }

		/// <summary></summary>
		[XmlText]
		public string Text { get; set; }

		/// <summary>XHTML for all nodes contained within this content node</summary>
		[XmlIgnore]
		public string Xhtml
		{
			get
			{
				if (_value == null)
				{
					var sb = new StringBuilder();
					if (InternalNodes == null)
						_value = Text;
					else
					{
						foreach (var node in InternalNodes)
							sb.Append(node.OuterXml);
						_value = sb.ToString();
					}
				}
				return _value;
			}
			set
			{
				_value = value;
				var doc = new XmlDocument();
				string dummyXml = "<dummy>" + value + "</dummy>";
				doc.LoadXml(dummyXml);
				if (doc.DocumentElement != null && doc.DocumentElement.ChildNodes.Count > 0)
				{
					var childNodes = doc.DocumentElement.ChildNodes;
					InternalNodes = new XmlNode[childNodes.Count];
					int i = 0;
					foreach (var childNode in childNodes)
						InternalNodes[i++] = (XmlNode)childNode;
				}
				else
				{
					InternalNodes = new XmlNode[0];
				}
			}
		}
	}

	/// <summary>
	/// A unique ID used to refer to the scripture text resource within a specific organizational database or software system.
	/// </summary>
	public class DblMetadataSystemId
	{
		/// <summary>Valid types include: tms, paratext, reap, biblica</summary>
		[XmlAttribute("type")]
		public string Type { get; set; }

		/// <summary>
		/// This attribute is probably only relevant to ad-hoc bundles created by Paratext.
		/// </summary>
		[XmlAttribute("csetid")]
		public string ChangeSetId { get; set; }

		/// <summary>
		/// A unique ID used to refer to the scripture text resource within a specific organizational database or software system.
		/// </summary>
		[XmlText]
		public string Id { get; set; }
	}

	/// <summary>Information about when bundle was created or updated</summary>
	public class DblMetadataArchiveStatus
	{
		/// <summary>Creation date for archive</summary>
		[XmlElement("dateArchived")]
		public string DateArchived { get; set; }

		/// <summary>Date when archive was last updated</summary>
		[XmlElement("dateUpdated")]
		public string DateUpdated { get; set; }
	}

	/// <summary>Information about a Scripture canon</summary>
	public class DblMetadataCanon
	{
		/// <summary>Gets whether this canon is a default canon</summary>
		[XmlAttribute("default")]
		public bool Default { get; set; }

		/// <summary>Unique id for the canon</summary>
		[XmlAttribute("id")]
		public string CanonId { get; set; }

		/// <summary>Name of the canon</summary>
		[XmlElement("name")]
		public string Name { get; set; }

		/// <summary>Localized name of the canon</summary>
		[XmlElement("nameLocal")]
		public string NameLocal { get; set; }

		/// <summary>Abbreviation for the canon</summary>
		[XmlElement("abbreviation")]
		public string Abbreviation { get; set; }

		/// <summary>Localized abbreviation for the canon</summary>
		[XmlElement("abbreviationLocal")]
		public string AbbreviationLocal { get; set; }

		/// <summary>Description for the canon</summary>
		[XmlElement("description")]
		public string Description { get; set; }

		/// <summary>Localized description for the canon</summary>
		[XmlElement("descriptionLocal")]
		public string DescriptionLocal { get; set; }

		/// <summary>List of books included in the canon</summary>
		[XmlArray("books")]
		[XmlArrayItem("book")]
		public List<DblMetadataCanonBook> CanonBooks { get; set; }
	}

	/// <summary>Information about a book in the canon</summary>
	public class DblMetadataCanonBook
	{
		/// <summary>Three-letter code for a book in the canon</summary>
		[XmlAttribute("code")]
		public string Code { get; set; }
	}

	/// <summary>
	/// Contains child elements for supplying long, short, and
	/// abbr (abbreviation) vernacular names for Biblical books in the scripture text.
	/// </summary>
	public class Book
	{
		/// <summary>Constructor for a DBL bundle book</summary>
		public Book()
		{
			IncludeInScript = true;
		}

		/// <summary>Three-letter book code for book</summary>
		[XmlAttribute("code")]
		public string Code { get; set; }

		[XmlAttribute("include")]
		[DefaultValue(true)]
		public bool IncludeInScript { get; set; }

		/// <summary>Vernacular text for the long form of the name for the Biblical book.</summary>
		[XmlElement("long")]
		public string LongName { get; set; }

		/// <summary>
		/// Vernacular text for the shorter form of the name for the Biblical book.
		/// This is the text typically used for the running header reference text in a printed volume, or
		/// for the text to appear for the book name in a digital navigation (menu) system.
		/// </summary>
		[XmlElement("short")]
		public string ShortName { get; set; }

		/// <summary>
		/// Vernacular text for the abbreviated form of the name for the Biblical book.
		/// This is the text typically used within reference lists found in cross references, indexes, or concordances.
		/// </summary>
		[XmlElement("abbr")]
		public string Abbreviation { get; set; }
	}
}
