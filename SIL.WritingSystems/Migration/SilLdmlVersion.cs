using System;
using System.Xml.Linq;
using SIL.Migration;

namespace SIL.WritingSystems.Migration
{
	public class SilLdmlVersion : IFileVersion
	{
		public const int BadVersion = -1;

		public int GetFileVersion(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			XElement ldmlElem = XElement.Load(filePath);
			if (ldmlElem.Name == "ldml")
			{
				// The exisitence of any other special namespace means invalid version
				foreach (var elem in ldmlElem.Elements("special"))
				{
					if (!string.IsNullOrEmpty((string)elem.Attribute(XNamespace.Xmlns+"palaso")) || 
						!string.IsNullOrEmpty((string)elem.Attribute(XNamespace.Xmlns+"palaso2")) ||
						!string.IsNullOrEmpty((string)elem.Attribute(XNamespace.Xmlns+"fw")))
					{
						return BadVersion;
					}
				}
				// Otherwise assume good current version
				return LdmlDataMapper.CurrentLdmlLibraryVersion;
			}

			return BadVersion;
		}

		public int StrategyGoodToVersion
		{
			get { return LdmlDataMapper.CurrentLdmlLibraryVersion; }
		}

	}
}