using System;
using System.Globalization;

namespace SIL.WritingSystems
{
	public class SystemCollator : ICollator
	{
		private readonly CultureInfo _cultureInfo;

		public SystemCollator(string ietfLanguageTag)
		{
			_cultureInfo = null;
			if (!string.IsNullOrEmpty(ietfLanguageTag))
			{
				try
				{
					_cultureInfo = CultureInfo.GetCultureInfo(ietfLanguageTag);
				}
				catch (CultureNotFoundException)
				{
				}
			}
			if (_cultureInfo == null)
				_cultureInfo = CultureInfo.InvariantCulture;
		}

		public SortKey GetSortKey(string source)
		{
			return _cultureInfo.CompareInfo.GetSortKey(source);
		}

		public int Compare(string x, string y)
		{
			int order = _cultureInfo.CompareInfo.Compare(x, y);

			if (order != 0)
			{
				return order;
			}
			if (_cultureInfo == CultureInfo.InvariantCulture)
			{
				// bugfix WS-33997.  Khmer (invariant culture) strings when compared return "same",
				// when in fact they are different strings.  In this case, use an ordinal compare.
				if (x != null && x.GetHashCode() == y.GetHashCode())
				{
					return 0;
				}
				return String.CompareOrdinal(x, y);
			}
			return 0;
		}

		public int Compare(object x, object y)
		{
			return Compare((string)x, (string)y);
		}

		/// <summary>
		/// Validates the specified IETF language tag.
		/// </summary>
		public static bool ValidateIetfLanguageTag(string ietfLanguageTag, out string message)
		{
			try
			{
				if (!String.IsNullOrEmpty(ietfLanguageTag))
					CultureInfo.GetCultureInfo(ietfLanguageTag);
			}
			catch (CultureNotFoundException)
			{
				message = String.Format("The locale, {0}, is not available on this machine.", ietfLanguageTag);
				return false;
			}
			message = null;
			return true;
		}
	}
}
