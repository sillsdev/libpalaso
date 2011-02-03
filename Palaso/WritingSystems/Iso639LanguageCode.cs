using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems
{
	public class Iso639LanguageCode
	{
		public Iso639LanguageCode(string code, string name, string iso3Code)
		{
			Code = code;
			Name = name;
			ISO3Code = iso3Code;
		}


		public string Name { get; set; }

		public string Code { get; set; }

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
					return x.Name.CompareTo(y.Name);
				}
			}
		}

	}
}
