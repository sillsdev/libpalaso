using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Palaso.Migration;

namespace Palaso.WritingSystems.Migration
{
	public class WritingSystemLdmlVersionGetter : IFileVersion
	{
		public int GetFileVersion(string pathToFile)
		{
			int version;
			using (XmlReader reader = XmlReader.Create(pathToFile))
			{
				reader.ReadToDescendant("ldml");
				reader.ReadToDescendant("special");
				reader.ReadToDescendant("palaso:version");
				string versionAsString = reader.GetAttribute("value");
				version = String.IsNullOrEmpty(versionAsString) ? 0 : Convert.ToInt32(versionAsString);
			}
			return version;
		}

		public int StrategyGoodToVersion
		{
			get { return 1; }
		}
	}
}
