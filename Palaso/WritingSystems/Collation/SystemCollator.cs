using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Palaso.WritingSystems.Collation
{
	public class SystemCollator : ICollator
	{
		private readonly CultureInfo _cultureInfo;

		public SystemCollator(string cultureId)
		{
			_cultureInfo = null;
			if (!String.IsNullOrEmpty(cultureId))
			{
				_cultureInfo = GetCultureInfoFromWritingSystemId(cultureId);
			}
			if (_cultureInfo == null)
			{
				_cultureInfo = CultureInfo.InvariantCulture;
			}
		}

		public SortKey GetSortKey(string source)
		{
			return _cultureInfo.CompareInfo.GetSortKey(source);
		}

		public int Compare(string x, string y)
		{
			return _cultureInfo.CompareInfo.Compare(x, y);
		}

		public int Compare(object x, object y)
		{
			return Compare((string)x, (string)y);
		}

		private static CultureInfo GetCultureInfoFromWritingSystemId(string cultureId)
		{
			CultureInfo ci;
			try
			{
				ci = CultureInfo.GetCultureInfo(cultureId);
			}
			catch (ArgumentException e)
			{
				if (e is ArgumentNullException || e is ArgumentOutOfRangeException)
				{
					throw;
				}
				ci = TryGetCultureInfoByIetfLanguageTag(cultureId);
			}
			return ci;
		}

		private static CultureInfo TryGetCultureInfoByIetfLanguageTag(string ietfLanguageTag)
		{
			CultureInfo ci = null;
			try
			{
				ci = CultureInfo.GetCultureInfoByIetfLanguageTag(ietfLanguageTag);
			}
			catch (ArgumentException ex)
			{
				if (ex is ArgumentNullException || ex is ArgumentOutOfRangeException)
				{
					throw;
				}
			}
			return ci;
		}
	}
}
