using System.Xml;

namespace SIL.DblBundle.Usx
{
	/// <summary>
	/// Class to support processing of a USX document (Scripture book in XML format).
	/// </summary>
	public class UsxDocument
	{
		private readonly XmlDocument m_document;

		/// <summary>
		/// Create a USX document from an XmlDocument.
		/// </summary>
		public UsxDocument(XmlDocument document)
		{
			m_document = document;
		}

		/// <summary>
		/// Create a USX document from the specified file.
		/// </summary>
		public UsxDocument(string path)
		{
			m_document = new XmlDocument { PreserveWhitespace = true };
			m_document.Load(path);
		}

		/// <summary>
		/// Gets the three-letter book ID for the book.
		/// </summary>
		public string BookId
		{
			get
			{
				var book = m_document.SelectSingleNode("//book");
				return book.Attributes.GetNamedItem("code").Value;
			}
		}

		/// <summary>
		/// Gets a list of paragraph and chapter nodes in the book.
		/// </summary>
		/// <returns></returns>
		public XmlNodeList GetChaptersAndParas()
		{
			return m_document.SelectNodes("//para | //chapter");
		}

		/// <summary>
		/// Gets the XmlDocument for the book.
		/// </summary>
		public XmlDocument XmlDocument { get { return m_document; } }
	}
}
