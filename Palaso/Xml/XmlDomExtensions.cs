using System;
using System.IO;
using System.Xml;

namespace Palaso.Xml
{
	public static class XmlDomExtensions
	{
		public static XmlDocument StripXHtmlNameSpace(this XmlDocument node)
		{
			XmlDocument x = new XmlDocument();
			x.LoadXml(node.OuterXml.Replace("xmlns", "xmlnsNeutered"));
			return x;
		}

		public static void AddStyleSheet(this XmlDocument dom, string cssFilePath)
		{
			var head = dom.SelectSingleNodeHonoringDefaultNS("//head");
			AddSheet(dom, head, cssFilePath);
		}

		private static void AddSheet(this XmlDocument dom, XmlNode head, string cssFilePath)
		{
			var link = dom.CreateElement("link", "http://www.w3.org/1999/xhtml");
			link.SetAttribute("rel", "stylesheet");

			if(cssFilePath.Contains(Path.PathSeparator.ToString())) // review: not sure about relative vs. complete paths
			{
				link.SetAttribute("href", "file://" + cssFilePath);
			}
			else //at least with gecko/firefox, something like "file://foo.css" is never found, so just give it raw
			{
				link.SetAttribute("href",cssFilePath);
			}
			link.SetAttribute("type", "text/css");
			head.AppendChild(link);
		}
	}
}