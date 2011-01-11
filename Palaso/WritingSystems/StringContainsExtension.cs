using System;
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
}
