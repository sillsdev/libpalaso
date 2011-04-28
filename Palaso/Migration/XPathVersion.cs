using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Palaso.Migration
{
	public class XPathVersion : IFileVersion
	{
		public XPathVersion(int goodToVersion, string xPath)
		{
			StrategyGoodToVersion = goodToVersion;
			XPath = xPath;
			NamespaceManager = new XmlNamespaceManager(new NameTable());
		}

		public XPathVersion(int goodToVersion, string xPath, XmlNamespaceManager namespaceManager) :
			this(goodToVersion, xPath)
		{
			NamespaceManager = namespaceManager;
		}

		public XmlNamespaceManager NamespaceManager { get; set; }

		private string XPath { get; set; }

		public int GetFileVersion(string filePath)
		{
			int result = -1;
			using (var sourceStream = new StreamReader(filePath))
			{
				var xPathDocument = new XPathDocument(sourceStream);
				var navigator = xPathDocument.CreateNavigator();
				var versionNode = navigator.SelectSingleNode(XPath, NamespaceManager);
				if (versionNode != null && !String.IsNullOrEmpty(versionNode.Value))
				{
					result = int.Parse(versionNode.Value);
				}
			}
			return result;
		}

		public int StrategyGoodToVersion { get; private set; }
	}
}
