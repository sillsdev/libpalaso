using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SIL.Xml;

namespace SIL.DblBundle.Text
{
	[XmlRoot("DBLMetadata")]
	public class DblTextMetadata<TL> : DblMetadataBase<TL> where TL : DblMetadataLanguage, new()
	{
		[XmlElement("identification")]
		public DblMetadataIdentification Identification { get; set; }

		[XmlElement("promotion")]
		public DblMetadataPromotion Promotion { get; set; }

		[XmlElement("archiveStatus")]
		public DblMetadataArchiveStatus ArchiveStatus { get; set; }

		[XmlArray("bookNames")]
		[XmlArrayItem("book")]
		public List<Book> AvailableBooks { get; set; }

		[XmlArray("contents")]
		[XmlArrayItem("bookList")]
		public List<DblMetadataCanon> Canons { get; set; }

		public string GetAsXml()
		{
			return XmlSerializationHelper.SerializeToString(this);
		}

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

		public override string ToString()
		{
			return string.IsNullOrEmpty(Name) ? Iso : string.Format("{0} ({1})", Name, Iso);
		}
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
		public XmlElement[] InternalNodes { get; set; }

		[XmlIgnore]
		public string Xhtml
		{
			get
			{
				if (m_value == null)
				{
					var sb = new StringBuilder();
					foreach (var node in InternalNodes)
						sb.Append(node.OuterXml);
					m_value = sb.ToString();
				}
				return m_value;
			}
			set
			{
				m_value = value;
				var doc = new XmlDocument();
				doc.LoadXml(value);
				InternalNodes = new[] { doc.DocumentElement };
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

	public class DblMetadataCanonBook
	{
		[XmlAttribute("code")]
		public string Code { get; set; }
	}

	public class Book
	{
		public Book()
		{
			IncludeInScript = true;
		}

		[XmlAttribute("code")]
		public string Code { get; set; }

		[XmlAttribute("include")]
		[DefaultValue(true)]
		public bool IncludeInScript { get; set; }

		[XmlElement("long")]
		public string LongName { get; set; }

		[XmlElement("short")]
		public string ShortName { get; set; }

		[XmlElement("abbr")]
		public string Abbreviation { get; set; }
	}
}
