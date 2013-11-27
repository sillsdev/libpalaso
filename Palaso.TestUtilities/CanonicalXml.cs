using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Palaso.TestUtilities
{
	///<summary>
	/// Assorted static methods for working with canonical xml
	///</summary>
	public class CanonicalXml
	{
		public static string ToCanonicalString(string inputXml)
		{
			var builder = new StringBuilder();
			using (var reader = XmlReader.Create(new StringReader(inputXml)))
			{
				using (var writer = XmlWriter.Create(builder, Palaso.Xml.CanonicalXmlSettings.CreateXmlWriterSettings()))
				{
					writer.WriteNode(reader, false);
				}
			}
			return builder.ToString();
		}

		public static string ToCanonicalStringFragment(string inputXml)
		{
			var builder = new StringBuilder();
			XmlReaderSettings readerSettings = new XmlReaderSettings();
			readerSettings.ConformanceLevel = ConformanceLevel.Fragment;
			using (var reader = XmlReader.Create(new StringReader(inputXml), readerSettings))
			{
				using (var writer = XmlWriter.Create(builder, Palaso.Xml.CanonicalXmlSettings.CreateXmlWriterSettings(ConformanceLevel.Fragment)))
				{
					writer.WriteNode(reader, false);
				}
			}
			return builder.ToString();
		}
	}
}
