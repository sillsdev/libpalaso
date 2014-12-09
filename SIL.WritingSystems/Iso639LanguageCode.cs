using System.Text.RegularExpressions;

namespace SIL.WritingSystems
{
	public class Iso639LanguageCode
	{
		//previously, we were spending huge amounts of time during sorting, just calculating this, using a regex
		private string SortingName;
		private string _code;
		public string InvariantLowerCaseCode;

		public Iso639LanguageCode(string code, string name, string iso3Code)
		{
			Code = code;
			Name = name;
			ISO3Code = iso3Code;
		}

		private string _name;
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				SortingName = Regex.Replace(_name, @"[^\w]", "");
			}
		}

		public string Code
		{
			get { return _code; }
			set
			{
				_code = value;
				//profiling showed ToLowerInvariant() was taking up to 6% of Bloom startup time, so now we cache it
				InvariantLowerCaseCode = _code.ToLowerInvariant();
			}
		}

		//Iso 639-1 code or Iso 639-3 code if former is not available

		public string ISO3Code { get; set; }

		public static int CompareByName(Iso639LanguageCode x, Iso639LanguageCode y)
		{
			if (x == null)
			{
				if (y == null)
				{
					return 0;
				}
				else
				{
					return -1;
				}
			}
			else
			{
				if (y == null)
				{
					return 1;
				}
				else
				{
					return x.SortingName.CompareTo(y.SortingName);
				}
			}
		}

	}
}
