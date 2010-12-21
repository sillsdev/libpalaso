using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Palaso.Xml
{
	/// <summary>
	/// Summary description for XmlUtils.
	/// </summary>
	public class XmlUtils
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

			if (sOutput != null && sOutput.Length != 0)
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
	}

}