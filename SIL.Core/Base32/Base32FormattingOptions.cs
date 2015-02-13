using System;

namespace SIL.Base32
{
	[Flags]
	public enum Base32FormattingOptions
	{
		/// <summary>
		/// Does not insert '=' at end of the string to pad it so that the string length is a multiple
		/// of 8
		/// </summary>
		None = 0,
		/// <summary>
		/// Inserts '=' at the end of the string to pad it so that the string length is a multiple
		/// of 8
		/// </summary>
		InsertTrailingPadding = 1,
		/*
		/// <summary>
		/// Inserts line breaks every 76 characters in the string representation
		/// </summary>
		InsertLineBreaks = 2
		*/
	}
}