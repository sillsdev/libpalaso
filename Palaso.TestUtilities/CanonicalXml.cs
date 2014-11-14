using System.IO;
using System.Text;
using System.Xml;

namespace Palaso.TestUtilities
{
	///<summary>
	/// Assorted static methods for working with Canonical XML
	/// 
	/// Be careful, after some research, we determined this is technically not Canonical XML.
	/// See http://en.wikipedia.org/wiki/Canonical_XML
	/// Notable differences are 
	///  * output is utf-16 (rather than utf-8)
	///  * end of line is \r\n (rather than \r)
	///  * attribute order maintained (rather than having a normative order)
	/// 
	/// It does, however, work if simply attempting to ignore arbitrary whitespace.
	///</summary>
	public class CanonicalXml
	{
		public static string ToCanonicalString(string inputXml)
		{
			var builder = new StringBuilder();
			var readerSettings = new XmlReaderSettings { IgnoreWhitespace = true };
			using (var reader = XmlReader.Create(new StringReader(inputXml), readerSettings))
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
			var readerSettings = new XmlReaderSettings { IgnoreWhitespace = true, ConformanceLevel = ConformanceLevel.Fragment };
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
