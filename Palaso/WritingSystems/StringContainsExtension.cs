using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems
{
	public static class StringExtension
	{
		public static bool Contains(this String stringToSearch, String stringToFind, StringComparison comparison)
		{
			int ind = stringToSearch.IndexOf(stringToFind, comparison); //This comparer should be extended to be "-"/"_" insensitive as well.
			return ind == -1 ? false : true;
		}
	}

	public static class ListExtension
	{
		public static bool Contains(this IList listToSearch, string itemToFind, StringComparison comparison)
		{
			foreach (string s in listToSearch)
			{
				if (s.Equals(itemToFind, comparison))
				{
					return true;
				}
			}
			return false;
		}
	}
}
