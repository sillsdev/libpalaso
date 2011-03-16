using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace Palaso.Migration
{
	public class XPathVersion : IFileVersion
	{
		public XPathVersion(int goodFromVersion, int goodToVersion, string xPath)
		{
			StrategyGoodFromVersion = goodFromVersion;
			StrategyGoodToVersion = goodToVersion;
			XPath = xPath;
		}

		private string XPath { get; set; }

		public int GetFileVersion(string source)
		{
			int result = -1;
			using (var sourceStream = new StreamReader(source))
			{
				var xPathDocument = new XPathDocument(sourceStream);
				var navigator = xPathDocument.CreateNavigator();
				var versionString = navigator.SelectSingleNode(XPath);
				result = int.Parse(versionString.Value);
			}
			return result;
		}

		public int StrategyGoodToVersion { get; private set; }
		public int StrategyGoodFromVersion { get; private set; }
	}
}
