namespace SIL.DblBundle
{
	public interface IStylesheet
	{
		IStyle GetStyle(string styleId);
		string FontFamily { get; }
		int FontSizeInPoints { get; }
	}

	public interface IStyle
	{
		string Id { get; }
		bool IsVerseText { get; }
		bool IsPublishable { get; }
		bool IsChapterLabel { get; }
		bool IsParallelPassageReference { get; }
		bool IsInlineQuotationReference { get; }
		bool HoldsBookNameOrAbbreviation { get; }
	}
}
