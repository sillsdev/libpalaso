using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Palaso.Migration;

namespace SIL.WritingSystems.Migration
{
	public class SilLdmlVersion : IFileVersion
	{
		public const int BadVersion = -1;

		/// <summary>
		/// Mapping of Sil namespace URI to LDML version
		/// </summary>
		private static readonly Dictionary<string, int> UriToVersion = new Dictionary<string, int>
		{
			{"urn://www.sil.org/ldml/0.1", WritingSystemDefinition.LatestWritingSystemDefinitionVersion},
		};

		public int GetFileVersion(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			XElement ldmlElem = XElement.Load(filePath);
			if (ldmlElem.Name == "ldml")
			{
				int result;
				string uri = (string)ldmlElem.Attribute(XNamespace.Xmlns + "sil");
				if (!string.IsNullOrEmpty(uri) && UriToVersion.TryGetValue(uri, out result))
				{
					return result;
				}
			}
			return BadVersion;
		}

		public int StrategyGoodToVersion
		{
			get { return UriToVersion.Max(kvp => kvp.Value); }
		}

	}
}