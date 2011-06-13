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
		readonly List<IFileVersion> _versionGetters = new List<IFileVersion>();

		public WritingSystemLdmlVersionGetter()
		{
			var versionNodeVersion = new XPathVersion(1, "/ldml/special/palaso:version/@value");
			versionNodeVersion.NamespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");

			var flexPrivateUseVersionGetter = new XPathVersion(1, "/ldml/identity/language/@type");
			flexPrivateUseVersionGetter.VersionParser = str => { return str.StartsWith("x", StringComparison.OrdinalIgnoreCase) ? 1 : -1; };

			_versionGetters.Add(versionNodeVersion);
			_versionGetters.Add(flexPrivateUseVersionGetter);
		}

		public int GetFileVersion(string ldmlFilePath)
		{
			return _versionGetters.Max(get => get.GetFileVersion(ldmlFilePath));
		}

		public int StrategyGoodToVersion
		{
			get { return _versionGetters.Max(get => get.StrategyGoodToVersion); }
		}
	}
}
