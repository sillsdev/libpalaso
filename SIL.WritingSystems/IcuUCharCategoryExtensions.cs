using System;
using System.Globalization;
using Icu;

namespace SIL.WritingSystems
{
	public static class IcuUCharCategoryExtensions
	{
		[CLSCompliant(false)]
		public static UnicodeCategory ToUnicodeCategory(this Character.UCharCategory category)
		{
			switch (category)
			{
				case Character.UCharCategory.UNASSIGNED:
					return UnicodeCategory.OtherNotAssigned;
				case Character.UCharCategory.UPPERCASE_LETTER:
					return UnicodeCategory.UppercaseLetter;
				case Character.UCharCategory.LOWERCASE_LETTER:
					return UnicodeCategory.LowercaseLetter;
				case Character.UCharCategory.TITLECASE_LETTER:
					return UnicodeCategory.TitlecaseLetter;
				case Character.UCharCategory.MODIFIER_LETTER:
					return UnicodeCategory.ModifierLetter;
				case Character.UCharCategory.OTHER_LETTER:
					return UnicodeCategory.OtherLetter;
				case Character.UCharCategory.NON_SPACING_MARK:
					return UnicodeCategory.NonSpacingMark;
				case Character.UCharCategory.ENCLOSING_MARK:
					return UnicodeCategory.EnclosingMark;
				case Character.UCharCategory.COMBINING_SPACING_MARK:
					return UnicodeCategory.SpacingCombiningMark;
				case Character.UCharCategory.DECIMAL_DIGIT_NUMBER:
					return UnicodeCategory.DecimalDigitNumber;
				case Character.UCharCategory.LETTER_NUMBER:
					return UnicodeCategory.LetterNumber;
				case Character.UCharCategory.OTHER_NUMBER:
					return UnicodeCategory.OtherNumber;
				case Character.UCharCategory.SPACE_SEPARATOR:
					return UnicodeCategory.SpaceSeparator;
				case Character.UCharCategory.LINE_SEPARATOR:
					return UnicodeCategory.LineSeparator;
				case Character.UCharCategory.PARAGRAPH_SEPARATOR:
					return UnicodeCategory.ParagraphSeparator;
				case Character.UCharCategory.CONTROL_CHAR:
					return UnicodeCategory.Control;
				case Character.UCharCategory.FORMAT_CHAR:
					return UnicodeCategory.Format;
				case Character.UCharCategory.PRIVATE_USE_CHAR:
					return UnicodeCategory.PrivateUse;
				case Character.UCharCategory.SURROGATE:
					return UnicodeCategory.Surrogate;
				case Character.UCharCategory.DASH_PUNCTUATION:
					return UnicodeCategory.DashPunctuation;
				case Character.UCharCategory.START_PUNCTUATION:
					return UnicodeCategory.OpenPunctuation;
				case Character.UCharCategory.END_PUNCTUATION:
					return UnicodeCategory.ClosePunctuation;
				case Character.UCharCategory.CONNECTOR_PUNCTUATION:
					return UnicodeCategory.ConnectorPunctuation;
				case Character.UCharCategory.OTHER_PUNCTUATION:
					return UnicodeCategory.OtherPunctuation;
				case Character.UCharCategory.MATH_SYMBOL:
					return UnicodeCategory.MathSymbol;
				case Character.UCharCategory.CURRENCY_SYMBOL:
					return UnicodeCategory.CurrencySymbol;
				case Character.UCharCategory.MODIFIER_SYMBOL:
					return UnicodeCategory.ModifierSymbol;
				case Character.UCharCategory.OTHER_SYMBOL:
					return UnicodeCategory.OtherSymbol;
				case Character.UCharCategory.INITIAL_PUNCTUATION:
					return UnicodeCategory.InitialQuotePunctuation;
				case Character.UCharCategory.FINAL_PUNCTUATION:
					return UnicodeCategory.FinalQuotePunctuation;
				default:
					return UnicodeCategory.OtherNotAssigned;
			}
		}
	}
}
