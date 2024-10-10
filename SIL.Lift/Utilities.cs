using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Xsl;
using SIL.Lift.Merging.xmldiff;
using SIL.Lift.Validation;

namespace SIL.Lift
{
	/// <summary>
	/// This contains various static utility methods related to Lift file processing.
	/// </summary>
	public class Utilities
	{

		/// <summary>
		/// Add guids
		/// </summary>
		/// <param name="inputPath"></param>
		/// <returns>path to a processed version</returns>
		static public string ProcessLiftForLaterMerging(string inputPath)
		{
			if (inputPath == null)
			{
				throw new ArgumentNullException("inputPath");
			}

			string outputOfPassOne = Path.GetTempFileName();
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;//ugly, but great for merging with revision control systems

			// nb:  don't use XmlTextWriter.Create, that's broken. Ignores the indent setting
			using (XmlWriter writer = XmlWriter.Create(outputOfPassOne /*Console.Out*/, settings))
			{
				using (XmlReader reader = XmlReader.Create(inputPath))
				{
					//bool elementWasReplaced = false;
					while (!reader.EOF)
					{
						ProcessNode(reader, writer);
					}
				}
			}

			XslCompiledTransform transform = new XslCompiledTransform();
			using (Stream canonicalizeXsltStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SIL.Lift.canonicalizeLift.xsl"))
			{
				if (canonicalizeXsltStream != null)
				{
					using (XmlReader xsltReader = XmlReader.Create(canonicalizeXsltStream))
					{
						transform.Load(xsltReader);
						xsltReader.Close();
					}
					canonicalizeXsltStream.Close();
				}
			}
			string outputOfPassTwo = Path.GetTempFileName();
			using (Stream output = File.Create(outputOfPassTwo))
			{
				transform.Transform(outputOfPassOne, new XsltArgumentList(), output);
			}
			#if NET462
			TempFileCollection tempfiles = transform.TemporaryFiles;
			if (tempfiles != null) // tempfiles will be null when debugging is not enabled
			{
				tempfiles.Delete();
			}
			#endif
			File.Delete(outputOfPassOne);

			return outputOfPassTwo;
		}



		private static void ProcessNode(XmlReader reader, XmlWriter writer)
		{
			switch (reader.NodeType)
			{
				case XmlNodeType.EndElement:
				case XmlNodeType.Element:
					ProcessElement(reader, writer);
					break;
				default:
					WriteShallowNode(reader, writer);
					break;
			}
		}


		private static void ProcessElement(XmlReader reader, XmlWriter writer)
		{
				if (reader.Name == "entry")
				{
					string guid = reader.GetAttribute("guid");
					if (String.IsNullOrEmpty(guid))
					{
						guid = Guid.NewGuid().ToString();
						writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
						writer.WriteAttributes(reader, true);
						writer.WriteAttributeString("guid", guid);
						string s = reader.ReadInnerXml();//this seems to be enough to get the reader to the next element
						writer.WriteRaw(s);
						writer.WriteEndElement();
					}
					else
					{
						writer.WriteNode(reader, true);
					}
				}
				else
				{
					WriteShallowNode(reader, writer);
				}
		}

		///<summary>
		/// Create an empty Lift file, unless a file of the given name exists already and
		/// doOverwriteIfExists is false.
		///</summary>
		static public void CreateEmptyLiftFile(string path, string producerString, bool doOverwriteIfExists)
		{
			if (File.Exists(path))
			{
				if (doOverwriteIfExists)
				{
					File.Delete(path);
				}
				else
				{
					return;
				}
			}

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;//ugly, but great for merging with revision control systems
			using (XmlWriter writer = XmlWriter.Create(path, settings))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("lift");
				writer.WriteAttributeString("version", Validator.LiftVersion);
				writer.WriteAttributeString("producer", producerString);
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
		}


		//came from a blog somewhere
		static internal void WriteShallowNode(XmlReader reader, XmlWriter writer)
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
					writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"),
										reader.Value);
					break;
				case XmlNodeType.Comment:
					writer.WriteComment(reader.Value);
					break;
				case XmlNodeType.EndElement:
					writer.WriteFullEndElement();
					break;
			}
			reader.Read();
		}

		/// <summary>
		/// Check wether the two XML elements (given as strings) are equal.
		/// </summary>
		public static bool AreXmlElementsEqual(string ours, string theirs)
		{
			StringReader osr = new StringReader(ours);
			XmlReader or = XmlReader.Create(osr);
			XmlDocument od = new XmlDocument();
			XmlNode on = od.ReadNode(or);
			if (on != null)
				on.Normalize();

			StringReader tsr = new StringReader(theirs);
			XmlReader tr = XmlReader.Create(tsr);
			XmlDocument td = new XmlDocument();
			XmlNode tn = td.ReadNode(tr);
			if (tn != null)
			{
				tn.Normalize();//doesn't do much

//            StringBuilder builder = new StringBuilder();
//            XmlWriter w = XmlWriter.Create(builder);


				return AreXmlElementsEqual(on, tn);
			}
			return false;
		}

		/// <summary>
		/// Check whether the two XML elements are equal.
		/// </summary>
		public static bool AreXmlElementsEqual(XmlNode ours, XmlNode theirs)
		{
			if (ours.NodeType == XmlNodeType.Text)
			{
				if (ours.NodeType != XmlNodeType.Text)
				{
					return false;
				}
				bool oursIsEmpty = (ours.InnerText.Trim() == string.Empty);
				bool theirsIsEmpty = (theirs.InnerText.Trim() == string.Empty);
				if(oursIsEmpty != theirsIsEmpty)
				{
					return false;
				}
				return ours.InnerText.Trim() == theirs.InnerText.Trim();
			}
		   // DiffConfiguration config = new DiffConfiguration(WhitespaceHandling.None);
			var diff = new XmlDiff(new XmlInput(ours.OuterXml), new XmlInput(theirs.OuterXml));//, config);
			DiffResult d = diff.Compare();
			return (d == null || d.Difference == null || !d.Difference.HasMajorDifference);
		}

		/// <summary>
		/// Get an attribute value from the XML element, or throw an exception if it isn't there.
		/// </summary>
		public static string GetStringAttribute(XmlNode form, string attr)
		{
			try
			{
				if (form.Attributes != null)
					return form.Attributes[attr].Value;
			}
			catch(NullReferenceException)
			{
			}
			throw new LiftFormatException(string.Format("Expected a {0} attribute on {1}.", attr, form.OuterXml));
		}

		/// <summary>
		/// Get an attribute value from the XML element, or return null if it isn't there.
		/// </summary>
		public static string GetOptionalAttributeString(XmlNode xmlNode, string attributeName)
		{
			if (xmlNode.Attributes == null)
				return null;
			XmlAttribute attr = xmlNode.Attributes[attributeName];
			if (attr == null)
				return null;
			return attr.Value;
		}

		/// <summary>
		/// Make the string safe for writing in an XML attribute value.
		/// </summary>
		public static string MakeSafeXmlAttribute(string sInput)
		{
			string sOutput = sInput;

			if (sOutput != null && sOutput.Length != 0)
			{
				sOutput = sOutput.Replace("&", "&amp;");
				sOutput = sOutput.Replace("\"", "&quot;");
				sOutput = sOutput.Replace("'", "&apos;");
				sOutput = sOutput.Replace("<", "&lt;");
				sOutput = sOutput.Replace(">", "&gt;");
			}
			return sOutput;
		}

		/// <summary>
		/// Parse a string into a new XmlNode that belongs to the same XmlDocument as
		/// nodeMaker (which may be either the XmlDocument or a child XmlNode).
		/// </summary>
		public static XmlNode GetDocumentNodeFromRawXml(string outerXml, XmlNode nodeMaker)
		{
			if(string.IsNullOrEmpty(outerXml))
				throw new ArgumentException("string outerXml is null or empty");
			XmlDocument doc = nodeMaker as XmlDocument;
			if(doc == null)
				doc = nodeMaker.OwnerDocument;
			if (doc == null)
				throw new ArgumentException("Could not get XmlDocument from XmlNode nodeMaker");
			using (StringReader sr = new StringReader(outerXml))
			{
				using (XmlReader r = XmlReader.Create(sr))
				{
					r.Read();
					return doc.ReadNode(r);
				}
			}
		}
	}
}
