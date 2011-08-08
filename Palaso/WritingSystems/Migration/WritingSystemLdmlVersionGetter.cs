using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Palaso.Migration;

namespace Palaso.WritingSystems.Migration
{
	/// <summary>
	/// This identifies the version of an LDML file via /ldml/special/palaso:version/@value node
	/// Note that there is an exception for LDML files containing /ldml/identity/language/@type beginning with "x-".
	/// This is in order to accomodate Flex LDML files that store enirely private use language tags differently.
	/// There should also be checks for private use script and region, but we'll wait and see if this is really a problem.
	/// The counterpart to this exception is found in the LdmlDataMapper which treats these types of files specially.
	/// </summary>
	public class WritingSystemLdmlVersionGetter : IFileVersion
	{
		readonly List<IFileVersion> _versionGetters = new List<IFileVersion>();

		public WritingSystemLdmlVersionGetter():this(false){}

		public WritingSystemLdmlVersionGetter(bool roundtripFlex70PrivateUse)
		{
			var versionNodeVersion = new XPathVersion(1, "/ldml/special/palaso:version/@value");
			versionNodeVersion.NamespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");

			if(roundtripFlex70PrivateUse){
				var flexPrivateUseVersionGetter = new XPathVersion(1, "/ldml/identity/language/@type");
				flexPrivateUseVersionGetter.VersionParser = str => { return (str.StartsWith("x-", StringComparison.OrdinalIgnoreCase) || str.Equals("x")) ? 2 : -1; };
				_versionGetters.Add(flexPrivateUseVersionGetter);
			}

			_versionGetters.Add(versionNodeVersion);
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
