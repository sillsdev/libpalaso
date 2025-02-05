// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace SIL.Extensions
{
	public static class XAttributeExtension
	{
		private static string GetPrefixOfNamespace(XNamespace ns) {
			string namespaceName = ns.NamespaceName;
			if (namespaceName.Length == 0)
				return string.Empty;
			if (namespaceName == XNamespace.Xml.NamespaceName)
				return "xml";
			if (namespaceName == XNamespace.Xmlns.NamespaceName)
				return "xmlns";
			return null;
		}

		// Current Mono versions have a bug so that the attribute
		// xmlns:flex="http://fieldworks.sil.org"
		// ends up as
		// {http://www.w3.org/2000/xmlns/}flex="http://fieldworks.sil.org"
		// This extension method works around this problem by copying some code from
		// the reference source implementation.
		public static string ToStringMonoWorkaround(this XAttribute attr)
		{
			using (var sw = new StringWriter(CultureInfo.InvariantCulture)) {
				var ws = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment };
				using (var w = XmlWriter.Create(sw, ws)) {
					w.WriteAttributeString(GetPrefixOfNamespace(attr.Name.Namespace), attr.Name.LocalName,
						attr.Name.NamespaceName, attr.Value);
				}
				return sw.ToString().Trim();
			}
		}
	}
}

