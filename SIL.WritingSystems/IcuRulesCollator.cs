using System;
using System.Globalization;
using Icu.Collation;

namespace SIL.WritingSystems
{
	public class IcuRulesCollator : ICollator
	{
		private readonly RuleBasedCollator _collator;

		public IcuRulesCollator(string rules)
		{
			try
			{
				_collator = new RuleBasedCollator(LdmlCollationParser.ReplaceUnicodeEscapesForIcu(rules));
			}
			catch (DllNotFoundException e)
			{
				throw new DllNotFoundException("IcuRulesCollator requires the ICU dlls to be present", e);
			}
		}

		public static bool ValidateSortRules(string rules, out string message)
		{
			Collator.CollationRuleErrorInfo errorInfo = Collator.CheckRules(LdmlCollationParser.ReplaceUnicodeEscapesForIcu(rules));
			if (errorInfo != null)
			{
				message = string.Format("Invalid ICU rules (Line: {0}, Column: {1}).", errorInfo.Line, errorInfo.Offset);
				return false;
			}
			message = null;
			return true;
		}

		public SortKey GetSortKey(string source)
		{
			return _collator.GetSortKey(source);
		}

		///<summary>Compares two strings and returns a value indicating whether one is less than,
		/// equal to, or greater than the other.</summary>
		///
		///<returns>Less than zero when string1 is less than string2.
		///  Zero when string1 equals string2.
		///  Greater than zero when string1 is greater than string2.
		///</returns>
		///
		///<param name="string1">The first string to compare.</param>
		///<param name="string2">The second object to compare.</param>
		public int Compare(string string1, string string2)
		{
			return _collator.Compare(string1, string2);
		}

		public int Compare(object x, object y)
		{
			return Compare((string) x, (string) y);
		}
	}
}
