using System;
using System.Collections.Generic;
using System.IO;
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

		/// <summary>
		/// normal string.format will throw if it can't do the format; this is dangerous if you're, for example
		/// just logging stuff that might contain messed up strings (myWorkSafe paths)
		/// </summary>
		public static string FormatWithErrorStringInsteadOfException(this string format, params object[] args)
		{
			try
			{
				return string.Format(format, args);
			}
			catch (Exception e)
			{
				string argList = "";
				foreach (var arg in args)
				{
					argList = argList + arg + ",";
				}
				argList = argList.Trim(new char[] {','});
				return "FormatWithErrorStringInsteadOfException(" + format + "," + argList + ") Exception: " + e.Message;
			}
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

		/// <summary>
		/// Similar to Path.Combine, but it combines as may parts as you have into a single, platform-appropriate path.
		/// </summary>
		/// <example> string path = "my".Combine("stuff", "toys", "ball.txt")</example>
		public static string CombineForPath(this string rootPath, params string[] partsOfThePath)
		{
			string result = rootPath.ToString();
			foreach (var s in partsOfThePath)
			{
				result = Path.Combine(result, s);
			}
			return result;
		}

	}
}
