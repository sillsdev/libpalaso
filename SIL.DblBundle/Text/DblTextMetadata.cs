using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using L10NSharp;
using SIL.ObjectModel;
using SIL.Scripture;
using SIL.WritingSystems;
using SIL.Xml;

namespace SIL.DblBundle.Text
{
	/// <summary>
	/// Information about a Digitial Bible Library bundle
	/// </summary>
	[Serializable]
	[XmlRoot("DBLMetadata")]
	public class DblTextMetadata<TL> : DblMetadataBase<TL> where TL : DblMetadataLanguage, new()
	{
		public const String Version2BookIdPrefix = "book-";
		private ObservableList<DblMetadataCanon> _canons;
		private ObservableList<DblMetadataPublication> _publications;
		private List<Book> _availableBooks;

		[XmlElement("identification")]
		public DblMetadataIdentification Identification { get; set; }

		[XmlElement("copyright")]
		public DblMetadataCopyright Copyright { get; set; }

		[XmlElement("promotion")]
		public DblMetadataPromotion Promotion { get; set; }

		[XmlElement("archiveStatus")]
		public DblMetadataArchiveStatus ArchiveStatus { get; set; }

		/// <summary>
		/// The books were stored in this xml structure in metadata version 1.
		/// </summary>
		[XmlArray("bookNames")]
		[XmlArrayItem("book")]
		public List<Book> AvailableBooks_XmlDeprecated
		{
			get { return _availableBooks; }
			set { _availableBooks = value; }
		}

		/// <summary>
		/// Prevents AvailableBooks_XmlDeprecated from being serialized
		/// (while allowing it to be deserialized)
		/// </summary>
		public bool ShouldSerializeAvailableBooks_XmlDeprecated() { return false; }

		/// <summary>
		/// The books are stored in this xml structure starting with metadata version 2.
		/// </summary>
		[XmlArray("names")]
		[XmlArrayItem("name")]
		public List<Book> AvailableBooks
		{
			get { return _availableBooks; }
			set { _availableBooks = value; }
		}

		/// <summary>
		/// The AvailableBooks which are also part of the 66 books of the Bible
		/// </summary>
		public IReadOnlyList<Book> AvailableBibleBooks
		{
			get
			{
				return new ReadOnlyList<Book>(AvailableBooks.Where(b =>
				{
					if (b.Id == null)
						return false;
					int bookNum = BCVRef.BookToNumber(b.Id);
					if (b.Id.StartsWith(Version2BookIdPrefix))
						bookNum = BCVRef.BookToNumber(b.Id.Substring(Version2BookIdPrefix.Length));
					return bookNum >= 1 && bookNum <= BCVRef.LastBook;
				}).ToList());
			}
		}

		/// <summary>
		/// Prevents Canons_DeprecatedXml from being serialized
		/// (while allowing it to be deserialized)
		/// </summary>
		public bool ShouldSerializeCanons_DeprecatedXml() { return false; }

		/// <summary>
		/// List of canons (publications) contained in the bundle.
		/// Deprecated starting with metadata version 2.
		/// </summary>
		[XmlArray("contents")]
		[XmlArrayItem("bookList")]
		public ObservableList<DblMetadataCanon> Canons_DeprecatedXml
		{
			get { return _canons; }
			set
			{
				if (value != null)
				{
					if (_publications == null)
					{
						_publications = new ObservableList<DblMetadataPublication>(ToPublications(value));
						_publications.CollectionChanged += PublicationsChanged;
					}
					value.CollectionChanged += CanonsChanged;
				}
				_canons = value;
			}
		}

		/// <summary>
		/// This is required to prevent a breaking change to the API after deprecating the contents element above.
		/// Technically, it does break the API since the original was a List. But changing this to IList
		/// was deemed better than having to create and return a copy every time we call get here.
		/// </summary>
		[XmlIgnore]
		public IList<DblMetadataCanon> Canons
		{
			get { return _canons; }
			set { Canons_DeprecatedXml = new ObservableList<DblMetadataCanon>(value); }
		}

		/// <summary>
		/// List of publications (canons) contained in the bundle.
		/// Replaced the contents element starting with metadata version 2.
		/// </summary>
		[XmlArray("publications")]
		[XmlArrayItem("publication")]
		public ObservableList<DblMetadataPublication> Publications
		{
			get { return _publications; }
			set
			{
				if (value != null)
				{
					if (_canons == null)
					{
						_canons = new ObservableList<DblMetadataCanon>(ToCanons(value));
						_canons.CollectionChanged += CanonsChanged;
					}
					value.CollectionChanged += PublicationsChanged;
				}
				_publications = value;
			}
		}

		private void CanonsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_publications.CollectionChanged -= PublicationsChanged; // prevent circular logic

			if (e.NewItems != null)
			{
				foreach (var newCanon in e.NewItems.Cast<DblMetadataCanon>())
					_publications.Add(new DblMetadataPublication(newCanon));
			}

			if (e.OldItems != null)
			{
				foreach (var deletedCanon in e.OldItems.Cast<DblMetadataCanon>())
					_publications.Remove(_publications.Single(p => p.CanonId == deletedCanon.CanonId));
			}

			_publications.CollectionChanged += PublicationsChanged;
		}

		private void PublicationsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_canons.CollectionChanged -= CanonsChanged; // prevent circular logic

			if (e.NewItems != null)
			{
				foreach (var newPublication in e.NewItems.Cast<DblMetadataPublication>())
					_canons.Add(new DblMetadataCanon(newPublication));
			}

			if (e.OldItems != null)
			{
				foreach (var deletedPublication in e.OldItems.Cast<DblMetadataPublication>())
					_canons.Remove(_canons.Single(c => c.CanonId == deletedPublication.CanonId));
			}

			_canons.CollectionChanged += CanonsChanged;
		}

		private ObservableList<DblMetadataPublication> ToPublications(IList<DblMetadataCanon> canons)
		{
			if (canons == null)
				return null;
			var publications = new ObservableList<DblMetadataPublication>();
			foreach (var canon in canons)
				publications.Add(new DblMetadataPublication(canon));
			return publications;
		}

		private ObservableList<DblMetadataCanon> ToCanons(IList<DblMetadataPublication> publications)
		{
			if (publications == null)
				return null;
			var canons = new ObservableList<DblMetadataCanon>();
			foreach (var publication in publications)
				canons.Add(new DblMetadataCanon(publication));
			return canons;
		}

		public string GetAsXml()
		{
			return XmlSerializationHelper.SerializeToString(this);
		}

		public override string Name { get { return Identification.Name; } }

		public override string ToString()
		{
			if (Language.Iso == "sample")
				return Id;

			string identificationPart = Identification == null ? Id : Identification.ToString();

			return String.Format("{0} - {1}", Language, identificationPart);
		}
	}

	public class DblMetadataIdentification
	{
		[XmlElement("name")]
		public string Name { get; set; }

		[XmlElement("nameLocal")]
		public string NameLocal { get; set; }

		[XmlElement("systemId")]
		public HashSet<DblMetadataSystemId> SystemIds { get; set; }

		public override string ToString()
		{
			return NameLocal == Name ? NameLocal : String.Format("{0} ({1})", NameLocal, Name);
		}
	}

	public class DblMetadataLanguage
	{
		[XmlElement("iso")]
		public string Iso { get; set; }

		[XmlElement("name")]
		public string Name { get; set; }

		[XmlElement("ldml")]
		public string Ldml { get; set; }

		[XmlElement("rod")]
		public string Rod { get; set; }

		[XmlElement("script")]
		public string Script { get; set; }

		[XmlElement("scriptDirection")]
		[DefaultValue("LTR")]
		public string ScriptDirection { get; set; }

		[XmlElement("numerals")]
		public string Numerals { get; set; }

		[XmlIgnore]
		public string DisplayName
		{
			get
			{
				if (string.IsNullOrEmpty(Name))
					return Iso == WellKnownSubtags.UnlistedLanguage ? LocalizationManager.GetString("DblBundle.UnknownLanguageName", "Unknown") : Iso;
				return string.IsNullOrEmpty(Iso) ? Name : string.Format("{0} ({1})", Name, Iso);
			}
		}

		public override string ToString()
		{
			return DisplayName;
		}
	}

	public class DblMetadataCopyright
	{
		[XmlElement("statement")]
		public DblMetadataXhtmlContentNode Statement { get; set; }
	}

	public class DblMetadataPromotion
	{
		[XmlElement("promoVersionInfo")]
		public DblMetadataXhtmlContentNode PromoVersionInfo { get; set; }

		[XmlElement("promoEmail")]
		public DblMetadataXhtmlContentNode PromoEmail { get; set; }
	}

	public class DblMetadataXhtmlContentNode
	{
		private string m_value;

		public DblMetadataXhtmlContentNode()
		{
			ContentType = "xhtml";
		}

		[XmlAttribute("contentType")]
		public string ContentType { get; set; }

		[XmlAnyElement]
		public XmlNode[] InternalNodes { get; set; }

		[XmlText]
		public string Text { get; set; }

		[XmlIgnore]
		public string Xhtml
		{
			get
			{
				if (m_value == null)
				{
					var sb = new StringBuilder();
					if (InternalNodes == null)
						m_value = Text;
					else
					{
						foreach (var node in InternalNodes)
							sb.Append(node.OuterXml);
						m_value = sb.ToString();
					}
				}
				return m_value;
			}
			set
			{
				m_value = value;
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

	public class DblMetadataSystemId
	{
		[XmlAttribute("type")]
		public string Type { get; set; }

		// This attribute is probably only relevant to ad-hoc bundles created by Paratext.
		[XmlAttribute("csetid")]
		public string ChangeSetId { get; set; }

		[XmlText]
		public string Id { get; set; }
	}

	public class DblMetadataArchiveStatus
	{
		[XmlElement("dateArchived")]
		public string DateArchived { get; set; }

		[XmlElement("dateUpdated")]
		public string DateUpdated { get; set; }
	}

	public class DblMetadataCanon
	{
		public DblMetadataCanon()
		{
		}

		public DblMetadataCanon(DblMetadataPublication publication)
		{
			Default = publication.Default;
			CanonId = publication.CanonId;
			Description = publication.Description;
			DescriptionLocal = publication.DescriptionLocal;
			CanonBooks = publication.CanonBooks;
		}

		[XmlAttribute("default")]
		public bool Default { get; set; }

		[XmlAttribute("id")]
		public string CanonId { get; set; }

		[XmlElement("name")]
		public string Name { get; set; }

		[XmlElement("nameLocal")]
		public string NameLocal { get; set; }

		[XmlElement("abbreviation")]
		public string Abbreviation { get; set; }

		[XmlElement("abbreviationLocal")]
		public string AbbreviationLocal { get; set; }

		[XmlElement("description")]
		public string Description { get; set; }

		[XmlElement("descriptionLocal")]
		public string DescriptionLocal { get; set; }

		[XmlArray("books")]
		[XmlArrayItem("book")]
		public List<DblMetadataCanonBook> CanonBooks { get; set; }
	}

	public class DblMetadataPublication
	{
		public DblMetadataPublication()
		{
		}

		public DblMetadataPublication(DblMetadataCanon canon)
		{
			Default = canon.Default;
			CanonId = canon.CanonId;
			Description = canon.Description;
			DescriptionLocal = canon.DescriptionLocal;
			CanonBooks = canon.CanonBooks;
		}

		[XmlAttribute("default")]
		public bool Default { get; set; }

		[XmlAttribute("id")]
		public string CanonId { get; set; }

		[XmlElement("description")]
		public string Description { get; set; }

		[XmlElement("descriptionLocal")]
		public string DescriptionLocal { get; set; }

		[XmlArray("canonicalContent")]
		[XmlArrayItem("book")]
		public List<DblMetadataCanonBook> CanonBooks { get; set; }
	}

	public class DblMetadataCanonBook
	{
		[XmlAttribute("code")]
		public string Code { get; set; }
	}

	[Serializable]
	public class Book
	{
		private string _id;

		public Book()
		{
			IncludeInScript = true;
		}

		/// <summary>
		/// Prevents Code_DeprecatedXml from being serialized
		/// (while allowing it to be deserialized)
		/// </summary>
		public bool ShouldSerializeCode() { return false; }

		/// <summary>
		/// The book code (in form MAT).
		/// Used in metadata version 1.
		/// </summary>
		[XmlAttribute("code")]
		public string Code
		{
			get { return GetCodeFromId(Id); }
			set { Id = GetIdFromCode(value); }
		}

		/// <summary>
		/// The book ID (in form book-mat).
		/// Starting with metadata version 2, this replaces the code attribute.
		/// </summary>
		[XmlAttribute("id")]
		public string Id
		{
			get { return _id; }
			set { _id = value; }
		}

		[XmlAttribute("include")]
		[DefaultValue(true)]
		public bool IncludeInScript { get; set; }

		[XmlElement("long")]
		public string LongName { get; set; }

		[XmlElement("short")]
		public string ShortName { get; set; }

		[XmlElement("abbr")]
		public string Abbreviation { get; set; }

		private string GetCodeFromId(string id)
		{
			if (id == null)
				return null;
			if (id.StartsWith(DblTextMetadata<DblMetadataLanguage>.Version2BookIdPrefix))
				return id.Substring(DblTextMetadata<DblMetadataLanguage>.Version2BookIdPrefix.Length).ToUpper();
			return id;
		}

		private string GetIdFromCode(string code)
		{
			if (code == null)
				return null;
			if (!code.StartsWith(DblTextMetadata<DblMetadataLanguage>.Version2BookIdPrefix))
				return DblTextMetadata<DblMetadataLanguage>.Version2BookIdPrefix + code.ToLower();
			return code;
		}
	}
}
