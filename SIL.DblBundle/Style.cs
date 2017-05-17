using System.Xml.Serialization;

namespace SIL.DblBundle
{
	/// <summary>
	/// A specific style defined in a stylesheet
	/// </summary>
	public class Style : IStyle
	{
		/// <summary>Style ID</summary>
		[XmlAttribute("id")]
		public string Id { get; set; }

		/// <summary>Gets whether style is for verse text</summary>
		[XmlAttribute("versetext")]
		public bool IsVerseText { get; set; }

		/// <summary>Gets whether style is for publishable content</summary>
		[XmlAttribute("publishable")]
		public bool IsPublishable { get; set; }

		/// <summary>Gets whether style is for a chapter label</summary>
		public bool IsChapterLabel
		{
			get { return Id == "cl"; }
		}

		/// <summary>Gets whether the style is for a parallel passage reference</summary>
		public bool IsParallelPassageReference
		{
			get { return Id == "r"; }
		}

		/// <summary>Gets whether the style is for an inline quotation reference</summary>
		public bool IsInlineQuotationReference
		{
			get { return Id == "rq"; }
		}

		/// <summary>
		/// True if the style contains a book name or book abbreviation
		/// </summary>
		public bool HoldsBookNameOrAbbreviation
		{
			get
			{
				if (!IsPublishable || IsVerseText)
					return false;
				if (Id.StartsWith("h") || Id.StartsWith("toc") || Id.StartsWith("mt")) 
					return true;
				return false;
			}
		}
	}
}
