using System.Xml.Serialization;

namespace SIL.DblBundle
{
	/// <summary>
	/// A specific style defined in a stylesheet
	/// </summary>
	public class Style : IStyle
	{
		[XmlAttribute("id")]
		public string Id { get; set; }

		[XmlAttribute("versetext")]
		public bool IsVerseText { get; set; }

		[XmlAttribute("publishable")]
		public bool IsPublishable { get; set; }

		public bool IsChapterLabel
		{
			get { return Id == "cl"; }
		}

		public bool IsParallelPassageReference
		{
			get { return Id == "r"; }
		}

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
