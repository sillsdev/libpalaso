using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace SIL.Migration
{
	public class XPathVersion : IFileVersion
	{
		public delegate int VersionParserFn(string version);

		public const int BadVersion = -1;

		public XPathVersion(int goodToVersion, string xPath)
		{
			StrategyGoodToVersion = goodToVersion;
			XPath = xPath;
			NamespaceManager = new XmlNamespaceManager(new NameTable());
			VersionParser = ParseVersionString;
		}

		public XPathVersion(int goodToVersion, string xPath, XmlNamespaceManager namespaceManager) :
			this(goodToVersion, xPath)
		{
			NamespaceManager = namespaceManager;
		}

		public XmlNamespaceManager NamespaceManager { get; set; }

		public VersionParserFn VersionParser { get; set; }

		private string XPath { get; set; }

		public int GetFileVersion(string filePath)
		{
			int result = BadVersion;
			using (var sourceStream = new StreamReader(filePath))
			{
				var xPathDocument = new XPathDocument(sourceStream);
				var navigator = xPathDocument.CreateNavigator();
				var versionNode = navigator.SelectSingleNode(XPath, NamespaceManager);
				if (versionNode != null && !String.IsNullOrEmpty(versionNode.Value))
				{
					if (VersionParser == null)
					{
						throw new ApplicationException("VersionParser has not been set in XPathVersion");
					}
					result = VersionParser(versionNode.Value);
				}
			}
			return result;
		}

		protected int ParseVersionString(string version)
		{
			return int.Parse(version);
		}

		public int StrategyGoodToVersion { get; private set; }
	}
}
