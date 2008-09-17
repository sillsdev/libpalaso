using System;
using System.Globalization;

namespace Palaso.WritingSystems.Collation
{
	public class IcuRulesCollator: ICollator {
		private Icu.Collation.RuleBasedCollator _collator;

		public IcuRulesCollator(string rules)
		{
			try
			{
				this._collator = new Icu.Collation.RuleBasedCollator(rules);
			}
			catch (DllNotFoundException e)
			{
				throw new DllNotFoundException("IcuRulesCollator requires the ICU dlls to be present", e);
			}
		}

		public static bool ValidateSortRules(string rules, out string message)
		{
			IcuRulesParser parser = new IcuRulesParser();
			if (!parser.ValidateIcuRules(rules, out message))
			{
				return false;
			}
			try
			{
				new IcuRulesCollator(rules);
			}
			catch (Exception e)
			{
				message = String.Format("Invalid ICU sort rules: {0}", e.Message);
				return false;
			}
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
	}
}
