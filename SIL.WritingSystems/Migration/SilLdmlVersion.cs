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
			{"urn://www.sil.org/ldml/0.1", 3},
		};

		public int GetFileVersion(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			int result = BadVersion;

			XElement ldmlElem = XElement.Load(filePath);
			if (ldmlElem.Name != "ldml")
			{
				return result;
			}

			string uri = (string)ldmlElem.Attribute(XNamespace.Xmlns + "sil");
			if (!string.IsNullOrEmpty(uri))
				result = UriToVersion[uri];
			return result;
		}

		public int StrategyGoodToVersion
		{
			get { return UriToVersion.Max(kvp => kvp.Value); }
		}

	}
}