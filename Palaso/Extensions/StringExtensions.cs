using System.Collections.Generic;
using System.Xml;

namespace Palaso.Extensions
{
	public static class StringExtensions
	{
		public static List<string> SplitTrimmed(this string s, char seperator)
		{
			if(s.Trim() == string.Empty)
				return new List<string>();

			var x = s.Split(seperator);

			var r = new List<string>();

			foreach (var part in x)
			{
				var trim = part.Trim();
				if(trim!=string.Empty)
				{
					r.Add(trim);
				}
			}
			return r;
		}


		private static XmlNode _xmlNodeUsedForEscaping;
		public static string EscapeAnyUnicodeCharactersIllegalInXml(this string text)
		{
			//we actually want to preserve html markup, just escape the disallowed unicode characters
			text = text.Replace("<", "_lt;");
			text = text.Replace(">", "_gt;");
			text = text.Replace("&", "_amp;");
			text = text.Replace("\"", "_quot;");
			text = text.Replace("'", "_apos;");

			text = EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml(text);
			//put it back, now
			text = text.Replace("_lt;", "<");
			text = text.Replace("_gt;", ">");
			text = text.Replace("_amp;", "&");
			text = text.Replace("_quot;", "\"");
			text = text.Replace("_apos;", "'");
			return text;
		}
		public static string EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml(this string text)
		{
			if (_xmlNodeUsedForEscaping == null)//notice, this is only done once per run
			{
				XmlDocument doc = new XmlDocument(); // review: There are other, cheaper ways of doing this.  System.Security has a good escape mechanism IIRC CP 2011-01
				_xmlNodeUsedForEscaping = doc.CreateElement("text", "x", "");
			}

			_xmlNodeUsedForEscaping.InnerText = text;
			text = _xmlNodeUsedForEscaping.InnerXml;
			return text;
		}

	}
}
