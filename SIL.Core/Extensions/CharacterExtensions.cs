using System.Globalization;
using static System.Char;

namespace SIL.Core.Extensions
{
	public static class CharacterExtensions
	{
		public static bool IsLikelyWordForming(this char ch)
		{
			switch (GetUnicodeCategory(ch))
			{
				// REVIEW: Enclosing marks are seldom (if ever) used in normal (e.g., Scripture)
				// text. Probably best not to treat them as word-forming here.
				//case UnicodeCategory.EnclosingMark:
				case UnicodeCategory.Format: // Most characters in this category (ZWJ, ZWNJ, bi-di marks, soft hyphen) that are likely to appear in text are word-forming.
				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.ModifierLetter:
				case UnicodeCategory.NonSpacingMark:
				case UnicodeCategory.OtherLetter:
				case UnicodeCategory.PrivateUse: // Most likely added PUA characters will be letters
				case UnicodeCategory.SpacingCombiningMark:
				case UnicodeCategory.Surrogate: // REVIEW: These will be rare, and the caller probably needs to deal with the pair in a more meaningful way
				case UnicodeCategory.TitlecaseLetter:
				case UnicodeCategory.UppercaseLetter:
					return true;
			}

			return false;
		}
	}
}
