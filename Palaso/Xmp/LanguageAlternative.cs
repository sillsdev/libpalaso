using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.Xmp
{
	public class LanguageAlternative
	{
		public string Iso;

		public string Region;

		public string XmlAttributeValue_xmlLang
		{
			get
			{
				if (string.IsNullOrEmpty(Iso) || string.IsNullOrEmpty(Region))
				{
					return "x-default";
				}
				else
				{
					return string.Format("{0}-{1}", Iso, Region);
				}
			}
		}

		public string Text;
	}
}
