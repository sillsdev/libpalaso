namespace SIL.DblBundle
{
	/// <summary>
	/// Interface for the stylesheet.
	/// </summary>
	public interface IStylesheet
	{
		/// <summary>Gets the style for specified ID</summary>
		IStyle GetStyle(string styleId);
		string FontFamily { get; }
		int FontSizeInPoints { get; }
	}

	/// <summary>
	/// Interface methods for a style
	/// </summary>
	public interface IStyle
	{
		/// <summary>Style ID</summary>
		string Id { get; }
		/// <summary>Gets whether style is for verse text</summary>
		bool IsVerseText { get; }
		/// <summary>Gets whether style is for publishable content</summary>
		bool IsPublishable { get; }
		/// <summary>Gets whether style is for a chapter label</summary>
		bool IsChapterLabel { get; }
		/// <summary>Gets whether the style is for a parallel passage reference</summary>
		bool IsParallelPassageReference { get; }
		/// <summary>Gets whether the style is for an inline quotation reference</summary>
		bool IsInlineQuotationReference { get; }
		/// <summary>Gets whether the style contains information about the name or abbreviation for the book</summary>
		bool HoldsBookNameOrAbbreviation { get; }
	}
}
