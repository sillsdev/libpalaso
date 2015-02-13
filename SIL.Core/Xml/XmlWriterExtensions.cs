using System;
using System.Xml;

namespace SIL.Xml
{
	public static class XmlWriterExtensions
	{
		/// <summary>
		/// Unlike WriteNodes, this method does not descend through the tree.
		/// </summary>
		// From Mark Fussell http://blogs.msdn.com/b/mfussell/archive/2005/02/12/371546.aspx
		public static void WriteNodeShallow(this XmlWriter writer, XmlReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}

			switch (reader.NodeType)
			{
				case XmlNodeType.Element:
					writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
					writer.WriteAttributes(reader, true);
					if (reader.IsEmptyElement)
					{
						writer.WriteEndElement();
					}
					break;
				case XmlNodeType.Text:
					writer.WriteString(reader.Value);
					break;
				case XmlNodeType.Whitespace:
				case XmlNodeType.SignificantWhitespace:
					writer.WriteWhitespace(reader.Value);
					break;
				case XmlNodeType.CDATA:
					writer.WriteCData(reader.Value);
					break;
				case XmlNodeType.EntityReference:
					writer.WriteEntityRef(reader.Name);
					break;
				case XmlNodeType.XmlDeclaration:
				case XmlNodeType.ProcessingInstruction:
					writer.WriteProcessingInstruction(reader.Name, reader.Value);
					break;
				case XmlNodeType.DocumentType:
					writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
					break;
				case XmlNodeType.Comment:
					writer.WriteComment(reader.Value);
					break;
				case XmlNodeType.EndElement:
					writer.WriteFullEndElement();
					break;
			}
		}

	}
}
