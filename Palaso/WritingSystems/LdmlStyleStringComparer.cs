using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems
{
	public class LdmlStyleStringComparer:StringComparer
	{
		public override int Compare(string x, string y)
		{
			x = NormalizeString(x);
			y = NormalizeString(y);
			return String.Compare(x, y, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(string x, string y)
		{
			x = NormalizeString(x);
			y = NormalizeString(y);
			return String.Equals(x, y, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode(string obj)
		{
			obj = NormalizeString(obj);
			return obj.GetHashCode();
		}

		private string NormalizeString(string stringToNormalize)
		{
			string normalizedString;
			normalizedString = ReplaceUnderScoreWithDash(stringToNormalize);
			return normalizedString;
		}

		private string ReplaceUnderScoreWithDash(string stringToNormalize)
		{
			return stringToNormalize.Replace('_', '-');
		}
	}
}
