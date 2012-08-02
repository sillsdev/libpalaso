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
			dom.AddStyleSheet(cssFilePath, null);
		}

		public static void AddStyleSheet(this XmlDocument dom, string cssFilePath, string nameSpaceIfDesired)
		{
			RemoveStyleSheetIfFound(dom, cssFilePath);//prevent duplicates
			var head = XmlUtils.GetOrCreateElement(dom, "//html", "head"); //dom.SelectSingleNodeHonoringDefaultNS("//head");
			AddSheet(dom, head, cssFilePath, nameSpaceIfDesired);
		}

		public static void RemoveStyleSheetIfFound(XmlDocument dom, string cssFilePath)
		{
			foreach (XmlElement linkNode in dom.SafeSelectNodes("/html/head/link"))
			{
				var href = linkNode.GetAttribute("href");
				if (href == null)
				{
					continue;
				}
				//strip it down to just the name+extension, so other forms (e.g., via slightly different urls) will be removed.
				var path = href.ToLower().Replace("file://", "");
				if(Path.GetFileName(path)==Path.GetFileName(cssFilePath.ToLower()))
				{
					linkNode.ParentNode.RemoveChild(linkNode);
				}
			}
		}


		private static void AddSheet(this XmlDocument dom, XmlNode head, string cssFilePath, string namespaceIfDesired)
		{
			var link = string.IsNullOrEmpty(namespaceIfDesired) ? dom.CreateElement("link") : dom.CreateElement("link", namespaceIfDesired);
			link.SetAttribute("rel", "stylesheet");

			if(cssFilePath.Contains(Path.DirectorySeparatorChar.ToString())) // review: not sure about relative vs. complete paths
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