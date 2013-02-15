using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Palaso.Xml
{
	/// <summary>
	/// Summary description for XmlUtils.
	/// </summary>
	public static class XmlUtils
	{
		/// <summary>
		/// Returns true if value of attrName is 'true' or 'yes' (case ignored)
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The optional attribute to find.</param>
		/// <returns></returns>
		public static bool GetBooleanAttributeValue(XmlNode node, string attrName)
		{
			return GetBooleanAttributeValue(GetOptionalAttributeValue(node, attrName));
		}

		/// <summary>
		/// Given bytes that represent an xml element, return the values of requested attributes (if they exist).
		/// </summary>
		/// <param name="data">Data that is expected to an xml element.</param>
		/// <param name="attributes">A set of attributes, the values of which are to be returned.</param>
		/// <returns>A dictionary </returns>
		public static Dictionary<string, string> GetAttributes(byte[] data, HashSet<string> attributes)
		{
			var results = new Dictionary<string, string>(attributes.Count);
			using (var reader = XmlReader.Create(new MemoryStream(data), CanonicalXmlSettings.CreateXmlReaderSettings(ConformanceLevel.Fragment)))
			{
				reader.MoveToContent();
				foreach (var attr in attributes)
				{
					results.Add(attr, null);
					if (reader.MoveToAttribute(attr))
					{
						results[attr] = reader.Value;
					}
				}
			}
			return results;
		}

		/// <summary>
		/// Given a string that represents an xml element, return the values of requested attributes (if they exist).
		/// </summary>
		/// <param name="data">Data that is expected to an xml element.</param>
		/// <param name="attributes">A set of attributes, the values of which are to be returned.</param>
		/// <returns>A dictionary </returns>
		public static Dictionary<string, string> GetAttributes(string data, HashSet<string> attributes)
		{
			var results = new Dictionary<string, string>(attributes.Count);
			using (var reader = XmlReader.Create(new StringReader(data), CanonicalXmlSettings.CreateXmlReaderSettings(ConformanceLevel.Fragment)))
			{
				reader.MoveToContent();
				foreach (var attr in attributes)
				{
					results.Add(attr, null);
					if (reader.MoveToAttribute(attr))
					{
						results[attr] = reader.Value;
					}
				}
			}
			return results;
		}

		/// <summary>
		/// Returns true if value of attrName is 'true' or 'yes' (case ignored)
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The optional attribute to find.</param>
		/// <returns></returns>
		public static bool GetBooleanAttributeValue(XPathNavigator node, string attrName)
		{
			return GetBooleanAttributeValue(GetOptionalAttributeValue(node, attrName));
		}

		/// <summary>
		/// Returns true if sValue is 'true' or 'yes' (case ignored)
		/// </summary>
		public static bool GetBooleanAttributeValue(string sValue)
		{
			return (sValue != null &&
					(sValue.ToLower().Equals("true") || sValue.ToLower().Equals("yes")));
		}

		/// <summary>
		/// Returns a integer obtained from the (mandatory) attribute named.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The mandatory attribute to find.</param>
		/// <returns>The value, or 0 if attr is missing.</returns>
		public static int GetMandatoryIntegerAttributeValue(XmlNode node, string attrName)
		{
			return Int32.Parse(GetManditoryAttributeValue(node, attrName));
		}

		/// <summary>
		/// Return an optional integer attribute value, or if not found, the default value.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="attrName"></param>
		/// <param name="defaultVal"></param>
		/// <returns></returns>
		public static int GetOptionalIntegerValue(XmlNode node, string attrName, int defaultVal)
		{
			string val = GetOptionalAttributeValue(node, attrName);
			if (val == null)
			{
				return defaultVal;
			}
			return Int32.Parse(val);
		}

		/// <summary>
		/// Retrieve an array, given an attribute consisting of a comma-separated list of integers
		/// </summary>
		/// <param name="node"></param>
		/// <param name="attrName"></param>
		/// <returns></returns>
		public static int[] GetMandatoryIntegerListAttributeValue(XmlNode node, string attrName)
		{
			string input = GetManditoryAttributeValue(node, attrName);
			string[] vals = input.Split(',');
			int[] result = new int[vals.Length];
			for (int i = 0;i < vals.Length;i++)
			{
				result[i] = Int32.Parse(vals[i]);
			}
			return result;
		}

		/// <summary>
		/// Make a value suitable for GetMandatoryIntegerListAttributeValue to parse.
		/// </summary>
		/// <param name="vals"></param>
		/// <returns></returns>
		public static string MakeIntegerListValue(int[] vals)
		{
			StringBuilder builder = new StringBuilder(vals.Length * 7);
			// enough unless VERY big numbers
			for (int i = 0;i < vals.Length;i++)
			{
				if (i != 0)
				{
					builder.Append(",");
				}
				builder.Append(vals[i].ToString());
			}
			return builder.ToString();
		}

		/// <summary>
		/// Make a comma-separated list of the ToStrings of the values in the list.
		/// </summary>
		/// <param name="vals"></param>
		/// <returns></returns>
		public static string MakeListValue(List<int> vals)
		{
			StringBuilder builder = new StringBuilder(vals.Count * 7);
			// enough unless VERY big numbers
			for (int i = 0;i < vals.Count;i++)
			{
				if (i != 0)
				{
					builder.Append(",");
				}
				builder.Append(vals[i].ToString());
			}
			return builder.ToString();
		}

		/// <summary>
		/// Get an optional attribute value from an XmlNode.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <param name="defaultValue"></param>
		/// <returns>The value of the attribute, or the default value, if the attribute dismissing</returns>
		public static bool GetOptionalBooleanAttributeValue(XmlNode node,
															string attrName,
															bool defaultValue)
		{
			return
				GetBooleanAttributeValue(GetOptionalAttributeValue(node,
																   attrName,
																   defaultValue
																	   ? "true"
																	   : "false"));
		}

		/// <summary>
		/// Deprecated: use GetOptionalAttributeValue instead.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="attrName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static string GetAttributeValue(XmlNode node, string attrName, string defaultValue)
		{
			return GetOptionalAttributeValue(node, attrName, defaultValue);
		}

		/// <summary>
		/// Get an optional attribute value from an XmlNode.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <returns>The value of the attribute, or null, if not found.</returns>
		public static string GetAttributeValue(XmlNode node, string attrName)
		{
			return GetOptionalAttributeValue(node, attrName);
		}

		/// <summary>
		/// Get an optional attribute value from an XmlNode.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <returns>The value of the attribute, or null, if not found.</returns>
		public static string GetOptionalAttributeValue(XmlNode node, string attrName)
		{
			return GetOptionalAttributeValue(node, attrName, null);
		}

		/// <summary>
		/// Get an optional attribute value from an XmlNode.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <returns>The value of the attribute, or null, if not found.</returns>
		public static string GetOptionalAttributeValue(XPathNavigator node, string attrName)
		{
			return GetOptionalAttributeValue(node, attrName, null);
		}

		/// <summary>
		/// Get an optional attribute value from an XmlNode.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <returns>The value of the attribute, or null, if not found.</returns>
		/// <param name="defaultString"></param>
		public static string GetOptionalAttributeValue(XmlNode node,
													   string attrName,
													   string defaultString)
		{
			if (node != null && node.Attributes != null)
			{
				XmlAttribute xa = node.Attributes[attrName];
				if (xa != null)
				{
					return xa.Value;
				}
			}
			return defaultString;
		}

		/// <summary>
		/// Get an optional attribute value from an XmlNode.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <returns>The value of the attribute, or null, if not found.</returns>
		/// <param name="defaultString"></param>
		public static string GetOptionalAttributeValue(XPathNavigator node,
													   string attrName,
													   string defaultString)
		{
			if (node != null && node.HasAttributes)
			{
				string s = node.GetAttribute(attrName, string.Empty);
				if (!string.IsNullOrEmpty(s))
				{
					return s;
				}
			}
			return defaultString;
		}

		/// <summary>
		/// Return the node that has the desired 'name', either the input node or a decendent.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="name">The XmlNode name to find.</param>
		/// <returns></returns>
		public static XmlNode FindNode(XmlNode node, string name)
		{
			if (node.Name == name)
			{
				return node;
			}
			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name == name)
				{
					return childNode;
				}
				XmlNode n = FindNode(childNode, name);
				if (n != null)
				{
					return n;
				}
			}
			return null;
		}

		/// <summary>
		/// Get an obligatory attribute value.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The required attribute to find.</param>
		/// <returns>The value of the attribute.</returns>
		/// <exception cref="ApplicationException">
		/// Thrown when the value is not found in the node.
		/// </exception>
		public static string GetManditoryAttributeValue(XmlNode node, string attrName)
		{
			string retval = GetOptionalAttributeValue(node, attrName, null);
			if (retval == null)
			{
				throw new ApplicationException("The attribute'" + attrName +
											   "' is mandatory, but was missing. " + node.OuterXml);
			}
			return retval;
		}

		public static string GetManditoryAttributeValue(XPathNavigator node, string attrName)
		{
			string retval = GetOptionalAttributeValue(node, attrName, null);
			if (retval == null)
			{
				throw new ApplicationException("The attribute'" + attrName +
											   "' is mandatory, but was missing. " + node.OuterXml);
			}
			return retval;
		}

		/// <summary>
		/// Append an attribute with the specified name and value to parent.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="attrName"></param>
		/// <param name="attrVal"></param>
		public static void AppendAttribute(XmlNode parent, string attrName, string attrVal)
		{
			XmlAttribute xa = parent.OwnerDocument.CreateAttribute(attrName);
			xa.Value = attrVal;
			parent.Attributes.Append(xa);
		}

		/// <summary>
		/// Return true if the two nodes match. Corresponding children should match, and
		/// corresponding attributes (though not necessarily in the same order).
		/// The nodes are expected to be actually XmlElements; not tested for other cases.
		/// Comments do not affect equality.
		/// </summary>
		/// <param name="node1"></param>
		/// <param name="node2"></param>
		/// <returns></returns>
		public static bool NodesMatch(XmlNode node1, XmlNode node2)
		{
			if (node1 == null && node2 == null)
			{
				return true;
			}
			if (node1 == null || node2 == null)
			{
				return false;
			}
			if (node1.Name != node2.Name)
			{
				return false;
			}
			if (node1.InnerText != node2.InnerText)
			{
				return false;
			}
			if (node1.Attributes == null && node2.Attributes != null)
			{
				return false;
			}
			if (node1.Attributes != null && node2.Attributes == null)
			{
				return false;
			}
			if (node1.Attributes != null)
			{
				if (node1.Attributes.Count != node2.Attributes.Count)
				{
					return false;
				}
				for (int i = 0;i < node1.Attributes.Count;i++)
				{
					XmlAttribute xa1 = node1.Attributes[i];
					XmlAttribute xa2 = node2.Attributes[xa1.Name];
					if (xa2 == null || xa1.Value != xa2.Value)
					{
						return false;
					}
				}
			}
			if (node1.ChildNodes == null && node2.ChildNodes != null)
			{
				return false;
			}
			if (node1.ChildNodes != null && node2.ChildNodes == null)
			{
				return false;
			}
			if (node1.ChildNodes != null)
			{
				int ichild1 = 0; // index node1.ChildNodes
				int ichild2 = 0; // index node2.ChildNodes
				while (ichild1 < node1.ChildNodes.Count && ichild2 < node1.ChildNodes.Count)
				{
					XmlNode child1 = node1.ChildNodes[ichild1];

					// Note that we must defer doing the 'continue' until after we have checked to see if both children are comments
					// If we continue immediately and the last node of both elements is a comment, the second node will not have
					// ichild2 incremented and the final test will fail.
					bool foundComment = false;

					if (child1 is XmlComment)
					{
						ichild1++;
						foundComment = true;
					}
					XmlNode child2 = node2.ChildNodes[ichild2];
					if (child2 is XmlComment)
					{
						ichild2++;
						foundComment = true;
					}

					if (foundComment)
					{
						continue;
					}

					if (!NodesMatch(child1, child2))
					{
						return false;
					}
					ichild1++;
					ichild2++;
				}
				// If we finished both lists we got a match.
				return ichild1 == node1.ChildNodes.Count && ichild2 == node2.ChildNodes.Count;
			}
			else
			{
				// both lists are null
				return true;
			}
		}

		/// <summary>
		/// Return the first child of the node that is not a comment (or null).
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static XmlNode GetFirstNonCommentChild(XmlNode node)
		{
			if (node == null)
			{
				return null;
			}
			foreach (XmlNode child in node.ChildNodes)
			{
				if (!(child is XmlComment))
				{
					return child;
				}
			}
			return null;
		}

		/// <summary>
		/// Fix the string to be safe in a text region of XML.
		/// </summary>
		/// <param name="sInput"></param>
		/// <returns></returns>
		public static string MakeSafeXml(string sInput)
		{
			string sOutput = sInput;

			if (!string.IsNullOrEmpty(sOutput))
			{
				sOutput = sOutput.Replace("&", "&amp;");
				sOutput = sOutput.Replace("<", "&lt;");
				sOutput = sOutput.Replace(">", "&gt;");
			}
			return sOutput;
		}

		/// <summary>
		/// Fix the string to be safe in an attribute value of XML.
		/// </summary>
		/// <param name="sInput"></param>
		/// <returns></returns>
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
		/// lifted from http://www.knowdotnet.com/articles/indentxml.html
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static string GetIndendentedXml(string xml)
	  {
		 string outXml = string.Empty;
		 using(MemoryStream ms = new MemoryStream())
		 // Create a XMLTextWriter that will send its output to a memory stream (file)
		 using (XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.Unicode))
		 {
			 XmlDocument doc = new XmlDocument();

//             try
//             {
				 // Load the unformatted XML text string into an instance
				 // of the XML Document Object Model (DOM)
				 doc.LoadXml(xml);

				 // Set the formatting property of the XML Text Writer to indented
				 // the text writer is where the indenting will be performed
				 xtw.Formatting = Formatting.Indented;

				 // write dom xml to the xmltextwriter
				 doc.WriteContentTo(xtw);
				 // Flush the contents of the text writer
				 // to the memory stream, which is simply a memory file
				 xtw.Flush();

				 // set to start of the memory stream (file)
				 ms.Seek(0, SeekOrigin.Begin);
				 // create a reader to read the contents of
				 // the memory stream (file)
				 StreamReader sr = new StreamReader(ms);
				 // return the formatted string to caller
				 return sr.ReadToEnd();
//             }
//             catch (Exception ex)
//             {
//                 return ex.Message;
//             }
		 }
	  }

		//todo: what's the diff between this one and the next?
		static public XmlElement GetOrCreateElementPredicate(XmlDocument dom, XmlElement parent, string predicate, string name)
		{
			XmlElement element = (XmlElement)parent.SelectSingleNodeHonoringDefaultNS("/" + predicate);
			if (element == null)
			{
				element = parent.OwnerDocument.CreateElement(name, parent.NamespaceURI);
				parent.AppendChild(element);
			}
			return element;
		}

		static public XmlElement GetOrCreateElement(XmlDocument dom, string parentPath, string name)
		{
			XmlElement element = (XmlElement)dom.SelectSingleNodeHonoringDefaultNS(parentPath + "/" + name);
			if (element == null)
			{
				XmlElement parent = (XmlElement)dom.SelectSingleNodeHonoringDefaultNS(parentPath);
				if (parent == null)
					return null;
				element = parent.OwnerDocument.CreateElement(name, parent.NamespaceURI);
				parent.AppendChild(element);
			}
			return element;
		}

		public static string GetStringAttribute(XmlNode form, string attr)
		{
			try
			{
				return form.Attributes[attr].Value;
			}
			catch (NullReferenceException)
			{
				throw new XmlFormatException(string.Format("Expected a {0} attribute on {1}.", attr, form.OuterXml));
			}
		}

		public static string GetOptionalAttributeString(XmlNode xmlNode, string attributeName)
		{
			XmlAttribute attr = xmlNode.Attributes[attributeName];
			if (attr == null)
				return null;
			return attr.Value;
		}

		public static XmlNode GetDocumentNodeFromRawXml(string outerXml, XmlNode nodeMaker)
		{
			if (string.IsNullOrEmpty(outerXml))
			{
				throw new ArgumentException();
			}
			XmlDocument doc = nodeMaker as XmlDocument;
			if (doc == null)
			{
				doc = nodeMaker.OwnerDocument;
			}
			using (StringReader sr = new StringReader(outerXml))
			{
				using (XmlReader r = XmlReader.Create(sr))
				{
					r.Read();
					return doc.ReadNode(r);
				}
			}
		}

		public static string GetXmlForShowingInHtml(string xml)
		{
			var s = Palaso.Xml.XmlUtils.GetIndendentedXml(xml).Replace("<", "&lt;");
			s = s.Replace("\r\n", "<br/>");
			s = s.Replace("  ", "&nbsp;&nbsp;");
			return s;
		}


		public static string GetTitleOfHtml(XmlDocument dom, string defaultIfMissing)
		{
			var title = dom.SelectSingleNode("//head/title");
			if (title != null && !string.IsNullOrEmpty(title.InnerText) && !string.IsNullOrEmpty(title.InnerText.Trim()))
			{
				return title.InnerText.Trim();
			}
			return defaultIfMissing;
		}

		/// <summary>
		/// Write a node out containing the XML in dataToWrite, pretty-printed according to the rules of writer, except
		/// that we suppress indentation for children of nodes whose names are listed in suppressIndentingChildren,
		/// and also for "mixed" nodes (where some children are text).
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="dataToWrite"></param>
		/// <param name="suppressIndentingChildren"></param>
		public static void WriteNode(XmlWriter writer, string dataToWrite, HashSet<string> suppressIndentingChildren)
		{
			XElement element = XDocument.Parse(dataToWrite).Root;
			WriteNode(writer, element, suppressIndentingChildren);
		}

		/// <summary>
		/// Write a node out containing the XML in dataToWrite, pretty-printed according to the rules of writer, except
		/// that we suppress indentation for children of nodes whose names are listed in suppressIndentingChildren,
		/// and also for "mixed" nodes (where some children are text).
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="dataToWrite"></param>
		/// <param name="suppressIndentingChildren"></param>
		public static void WriteNode(XmlWriter writer, XElement dataToWrite, HashSet<string> suppressIndentingChildren)
		{
			if (dataToWrite == null)
				return;
			WriteElementTo(writer, dataToWrite, suppressIndentingChildren);
		}

		/// <summary>
		/// Recursively write an element to the writer, suppressing indentation of children when required.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="element"></param>
		/// <param name="suppressIndentingChildren"></param>
		private static void WriteElementTo(XmlWriter writer, XElement element, HashSet<string> suppressIndentingChildren)
		{
			writer.WriteStartElement(element.Name.LocalName);
			foreach (var attr in element.Attributes())
				writer.WriteAttributeString(attr.Name.LocalName, attr.Value);
			// The writer automatically suppresses indenting children for any element that it detects has text children.
			// However, it won't do this for the first child if that is an element, even if it later encounters text children.
			// Also, there may be a parent where text including white space is significant, yet it is possible for the
			// WHOLE content to be an element. For example, a <text> or <AStr> element may consist entirely of a <span>.
			// In such cases there is NO way to tell from the content that it should not be indented, so all we can do
			// is pass a list of such elements.
			bool suppressIndenting = suppressIndentingChildren.Contains(element.Name.LocalName) || element.Nodes().Any(x => x is XText);
			// In either case, we implement the suppression by making the first child a fake text element.
			// Calling this method, even with an empty string, has proved to be enough to make the writer treat the parent
			// as "mixed" which prevents indenting its children.
			if (suppressIndenting)
				writer.WriteString("");
			foreach (var child in element.Nodes())
			{
				var xElement = child as XElement;
				if (xElement != null)
					WriteElementTo(writer, xElement, suppressIndentingChildren);
				else
					child.WriteTo(writer); // defaults are fine for everything else.
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Remove illegal XML characters from a string.
		/// </summary>
		public static string SanitizeString(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return s;
			}

			StringBuilder buffer = new StringBuilder(s.Length);

			for (int i = 0; i < s.Length; i++)
			{
				int code;
				try
				{
					code = Char.ConvertToUtf32(s, i);
				}
				catch (ArgumentException)
				{
					continue;
				}
				if (IsLegalXmlChar(code))
					buffer.Append(Char.ConvertFromUtf32(code));
				if (Char.IsSurrogatePair(s, i))
					i++;
			}

			return buffer.ToString();
		}

		/// <summary>
		/// Whether a given character is allowed by XML 1.0.
		/// </summary>
		private static bool IsLegalXmlChar(int codePoint)
		{
			return (codePoint == 0x9 ||
				codePoint == 0xA ||
				codePoint == 0xD ||
				(codePoint >= 0x20 && codePoint <= 0xD7FF) ||
				(codePoint >= 0xE000 && codePoint <= 0xFFFD) ||
				(codePoint >= 0x10000/* && character <= 0x10FFFF*/) //it's impossible to get a code point bigger than 0x10FFFF because Char.ConvertToUtf32 would have thrown an exception
			);
		}
		/// <summary>
		/// Write a node out containing the XML in dataToWrite, pretty-printed according to the rules of writer, except
		/// that we suppress indentation for children of nodes whose names are listed in suppressIndentingChildren,
		/// and also for "mixed" nodes (where some children are text).
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="dataToWrite"></param>
		/// <param name="suppressIndentingChildren"></param>
		public static void WriteNode(XmlWriter writer, string dataToWrite, HashSet<string> suppressIndentingChildren)
		{
			XElement element = XDocument.Parse(dataToWrite).Root;
			WriteNode(writer, element, suppressIndentingChildren);
		}
		/// <summary>
		/// Write a node out containing the XML in dataToWrite, pretty-printed according to the rules of writer, except
		/// that we suppress indentation for children of nodes whose names are listed in suppressIndentingChildren,
		/// and also for "mixed" nodes (where some children are text).
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="dataToWrite"></param>
		/// <param name="suppressIndentingChildren"></param>
		public static void WriteNode(XmlWriter writer, XElement dataToWrite, HashSet<string> suppressIndentingChildren)
		{
			if (dataToWrite == null)
				return;
			WriteElementTo(writer, dataToWrite, suppressIndentingChildren);
		}

		/// <summary>
		/// Recursively write an element to the writer, suppressing indentation of children when required.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="element"></param>
		/// <param name="suppressIndentingChildren"></param>
		private static void WriteElementTo(XmlWriter writer, XElement element, HashSet<string> suppressIndentingChildren)
		{
			writer.WriteStartElement(element.Name.LocalName);
			foreach (var attr in element.Attributes())
				writer.WriteAttributeString(attr.Name.LocalName, attr.Value);
			// The writer automatically suppresses indenting children for any element that it detects has text children.
			// However, it won't do this for the first child if that is an element, even if it later encounters text children.
			// Also, there may be a parent where text including white space is significant, yet it is possible for the
			// WHOLE content to be an element. For example, a <text> or <AStr> element may consist entirely of a <span>.
			// In such cases there is NO way to tell from the content that it should not be indented, so all we can do
			// is pass a list of such elements.
			bool suppressIndenting = suppressIndentingChildren.Contains(element.Name.LocalName) || element.Nodes().Any(x => x is XText);
			// In either case, we implement the suppression by making the first child a fake text element.
			// Calling this method, even with an empty string, has proved to be enough to make the writer treat the parent
			// as "mixed" which prevents indenting its children.
			if (suppressIndenting)
				writer.WriteString("");
			foreach (var child in element.Nodes())
			{
				var xElement = child as XElement;
				if (xElement != null)
					WriteElementTo(writer, xElement, suppressIndentingChildren);
				else
					child.WriteTo(writer); // defaults are fine for everything else.
			}
			writer.WriteEndElement();
		}
	}
}