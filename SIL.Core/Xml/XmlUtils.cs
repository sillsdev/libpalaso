using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using JetBrains.Annotations;
using SIL.Linq;

namespace SIL.Xml
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
		[PublicAPI]
		public static bool GetBooleanAttributeValue(XmlNode node, string attrName)
		{
			return GetBooleanAttributeValue(GetOptionalAttributeValue(node, attrName));
		}

		/// <summary>
		/// Returns true if value of attrName is 'true' or 'yes' (case ignored)
		/// </summary>
		/// <param name="element">The XElement to look in.</param>
		/// <param name="attrName">The optional attribute to find.</param>
		public static bool GetBooleanAttributeValue(XElement element, string attrName)
		{
			return GetBooleanAttributeValue(GetOptionalAttributeValue(element, attrName));
		}

		/// <summary>
		/// Given bytes that represent an XML element, return the values of requested attributes (if they exist).
		/// </summary>
		/// <param name="data">Data that is expected to an XML element.</param>
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
		/// Given a string that represents an XML element, return the values of requested attributes (if they exist).
		/// </summary>
		/// <param name="data">Data that is expected to an XML element.</param>
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
		[PublicAPI]
		public static bool GetBooleanAttributeValue(XPathNavigator node, string attrName)
		{
			return GetBooleanAttributeValue(GetOptionalAttributeValue(node, attrName));
		}

		/// <summary>
		/// Returns true if sValue is 'true' or 'yes' (case ignored)
		/// </summary>
		public static bool GetBooleanAttributeValue(string sValue)
		{
			return sValue != null &&
			       (sValue.ToLower().Equals("true") || sValue.ToLower().Equals("yes"));
		}

		/// <summary>
		/// Returns an integer obtained from the (mandatory) attribute named.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The mandatory attribute to find.</param>
		/// <returns>The value, or 0 if attr is missing.</returns>
		[PublicAPI]
		public static int GetMandatoryIntegerAttributeValue(XmlNode node, string attrName)
		{
			return int.Parse(GetMandatoryAttributeValue(node, attrName));
		}

		/// <summary>
		/// Returns an integer obtained from the (mandatory) attribute named.
		/// </summary>
		/// <param name="element">The XmlNode to look in.</param>
		/// <param name="attrName">The mandatory attribute to find.</param>
		/// <returns>The value, or 0 if attr is missing.</returns>
		/// <exception cref="ApplicationException">Thrown if <paramref name="attrName"/> is not in <paramref name="element"/>.</exception>
		/// <exception cref="FormatException">Thrown, if <paramref name="attrName"/> value is not an integer (int).</exception>
		public static int GetMandatoryIntegerAttributeValue(XElement element, string attrName)
		{
			return int.Parse(GetMandatoryAttributeValue(element, attrName), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Return an optional integer attribute value, or if not found, the default value.
		/// </summary>
		/// <param name="node">The XML node representing the element that potentially contains the
		/// attribute</param>
		/// <param name="attrName">The name of the attribute</param>
		/// <param name="defaultVal">The default value to return if the attribute is not present
		/// </param>
		/// <exception cref="T:System.FormatException">The value contained in the attribute
		/// identified by <paramref name="attrName"/> is not in a format that can be parsed as an
		/// integer.</exception>
		/// <exception cref="T:System.OverflowException">The value contained in the attribute
		/// identified by <paramref name="attrName"/> represents a number less than
		/// <see cref="F:System.Int32.MinValue" /> or greater than
		/// <see cref="F:System.Int32.MaxValue" />.</exception>
		[PublicAPI]
		public static int GetOptionalIntegerValue(XmlNode node, string attrName, int defaultVal)
		{
			string val = GetOptionalAttributeValue(node, attrName);
			return val == null ? defaultVal : int.Parse(val);
		}

		/// <summary>
		/// Return an optional integer attribute value, or if not found, the default value.
		/// </summary>
		/// <param name="element">The XML element that potentially contains the attribute</param>
		/// <param name="attrName">The name of the attribute</param>
		/// <param name="defaultVal">The default value to return if the attribute is not present
		/// </param>
		/// <exception cref="T:System.FormatException">The value contained in the attribute
		/// identified by <paramref name="attrName"/> is not in a format that can be parsed as an
		/// integer.</exception>
		/// <exception cref="T:System.OverflowException">The value contained in the attribute
		/// identified by <paramref name="attrName"/> represents a number less than
		/// <see cref="F:System.Int32.MinValue" /> or greater than
		/// <see cref="F:System.Int32.MaxValue" />.</exception>
		public static int GetOptionalIntegerValue(XElement element, string attrName, int defaultVal)
		{
			var val = GetOptionalAttributeValue(element, attrName);
			return string.IsNullOrEmpty(val) ? defaultVal : int.Parse(val, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Retrieve an array of integers, given an attribute whose value consists of a
		///  comma-separated list of integers.
		/// </summary>
		/// <param name="node">The XML node representing the element that contains the attribute
		/// </param>
		/// <param name="attrName">The name of the attribute</param>
		/// <exception cref="T:System.FormatException">An item in the list obtained from the
		/// attribute identified by <paramref name="attrName"/> is not in a format that can be
		/// parsed as an integer.</exception>
		/// <exception cref="T:System.OverflowException">An item in the list obtained from the
		/// attribute identified by <paramref name="attrName"/> represents a number less than
		/// <see cref="F:System.Int32.MinValue" /> or greater than
		/// <see cref="F:System.Int32.MaxValue" />.</exception>
		[PublicAPI]
		public static int[] GetMandatoryIntegerListAttributeValue(XmlNode node, string attrName)
		{
			string input = GetMandatoryAttributeValue(node, attrName);
			string[] vals = input.Split(',');
			int[] result = new int[vals.Length];
			for (int i = 0;i < vals.Length;i++)
				result[i] = int.Parse(vals[i]);
			return result;
		}

		/// <summary>
		/// Retrieve an array, given an attribute whose value consists of a comma-separated list of
		/// integers.
		/// </summary>
		/// <param name="element">The XML element that contains the attribute</param>
		/// <param name="attrName">The name of the attribute</param>
		/// <exception cref="T:System.FormatException">An item in the list obtained from the
		/// attribute identified by <paramref name="attrName"/> is not in a format that can be
		/// parsed as an integer.</exception>
		/// <exception cref="T:System.OverflowException">An item in the list obtained from the
		/// attribute identified by <paramref name="attrName"/> represents a number less than
		/// <see cref="F:System.Int32.MinValue" /> or greater than
		/// <see cref="F:System.Int32.MaxValue" />.</exception>
		public static int[] GetMandatoryIntegerListAttributeValue(XElement element, string attrName)
		{
			var input = GetMandatoryAttributeValue(element, attrName);
			var vals = input.Split(',');
			var result = new int[vals.Length];
			for (var i = 0; i < vals.Length; i++)
				result[i] = int.Parse(vals[i], CultureInfo.InvariantCulture);
			return result;
		}

		/// <summary>
		/// Retrieve an array, given an attribute consisting of a comma-separated list of integers
		/// </summary>
		/// <param name="node">The XML node representing the element that contains the attribute
		/// </param>
		/// <param name="attrName">The name of the attribute</param>
		/// <exception cref="T:System.FormatException">An item in the list obtained from the
		/// attribute identified by <paramref name="attrName"/> is not in a format that can be
		/// parsed as an unsigned integer.</exception>
		/// <exception cref="T:System.OverflowException">An item in the list obtained from the
		/// attribute identified by <paramref name="attrName"/> represents a number less than
		/// <see cref="F:System.UInt32.MinValue" /> or greater than
		/// <see cref="F:System.UInt32.MaxValue" />.</exception>
		[CLSCompliant(false)]
		[PublicAPI]
		public static uint[] GetMandatoryUIntegerListAttributeValue(XmlNode node, string attrName)
		{
			var input = GetMandatoryAttributeValue(node, attrName);
			var vals = input.Split(',');
			var result = new uint[vals.Length];
			for (var i = 0; i < vals.Length; i++)
				result[i] = uint.Parse(vals[i]);
			return result;
		}

		/// <summary>
		/// Retrieve an array, given an attribute consisting of a comma-separated list of integers
		/// </summary>
		[CLSCompliant(false)]
		public static uint[] GetMandatoryUIntegerListAttributeValue(XElement element, string attrName)
		{
			var input = GetMandatoryAttributeValue(element, attrName);
			var vals = input.Split(',');
			var result = new uint[vals.Length];
			for (var i = 0; i < vals.Length; i++)
				result[i] = uint.Parse(vals[i]);
			return result;
		}

		private static string MakeStringFromEnum<T>(IEnumerable<T> vals, int count)
		{
			var builder = new StringBuilder(count * 7); // enough unless VERY big numbers

			foreach (var val in vals)
			{
				if (builder.Length > 0)
					builder.Append(",");
				builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", val);
			}
			return builder.ToString();
		}

		/// <summary>
		/// Make a value suitable for
		/// <see cref="GetMandatoryIntegerListAttributeValue(System.Xml.XmlNode,string)"/> to
		/// parse.
		/// </summary>
		/// <returns>A comma separated list of integers formatted as strings using
		/// <see cref="CultureInfo.InvariantCulture"/></returns>
		[PublicAPI]
		public static string MakeIntegerListValue(int[] vals)
		{
			return MakeStringFromEnum(vals, vals.Length);
		}

		/// <summary>
		/// Make a value suitable for
		/// <see cref="GetMandatoryIntegerListAttributeValue(System.Xml.XmlNode,string)"/> to
		/// parse.
		/// </summary>
		/// <returns>A comma separated list of integers formatted as strings using
		/// <see cref="CultureInfo.InvariantCulture"/></returns>
		public static string MakeStringFromList(List<int> vals)
		{
			return MakeStringFromEnum(vals, vals.Count);
		}

		/// <summary>
		/// Make a value suitable for
		/// <see cref="GetMandatoryUIntegerListAttributeValue(System.Xml.XmlNode,string)"/> to
		/// parse.
		/// </summary>
		/// <returns>A comma separated list of integers formatted as strings using
		/// <see cref="CultureInfo.InvariantCulture"/></returns>
		[CLSCompliant(false)]
		public static string MakeStringFromList(List<uint> vals)
		{
			return MakeStringFromEnum(vals, vals.Count);
		}

		/// <summary>
		/// Get an optional attribute value from an XmlNode.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <param name="defaultValue">The value to return if the attribute is not found.</param>
		/// <returns>The value of the attribute, or the default value, if the attribute is missing.
		/// </returns>
		[PublicAPI]
		public static bool GetOptionalBooleanAttributeValue(XmlNode node, string attrName,
			bool defaultValue)
		{
			return GetBooleanAttributeValue(GetOptionalAttributeValue(node, attrName,
				defaultValue ? "true" : "false"));
		}

		/// <summary>
		/// Get an optional attribute value from an XElement.
		/// </summary>
		/// <param name="element">The XElement to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <param name="defaultValue">The value to return if the attribute is not found.</param>
		/// <returns>The value of the attribute, or the default value, if the attribute is missing.</returns>
		public static bool GetOptionalBooleanAttributeValue(XElement element, string attrName, bool defaultValue)
		{
			return GetBooleanAttributeValue(GetOptionalAttributeValue(element, attrName,
				defaultValue ? "true" : "false"));
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
		/// Get an optional attribute value from an XElement.
		/// </summary>
		/// <param name="element">The XElement to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <returns>The value of the attribute, or null, if not found.</returns>
		public static string GetOptionalAttributeValue(XElement element, string attrName)
		{
			return GetOptionalAttributeValue(element, attrName, null);
		}

		/// <summary>
		/// Get an optional attribute value from an XmlNode.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <returns>The value of the attribute, or <c>null</c>, if not found.</returns>
		public static string GetOptionalAttributeValue(XPathNavigator node, string attrName)
		{
			return GetOptionalAttributeValue(node, attrName, null);
		}

		/// <summary>
		/// Get an optional attribute value from an XmlNode.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <param name="defaultString">The value to return if the attribute is not found</param>
		/// <returns>The value of the attribute, or <see cref="defaultString"/>, if not found.
		/// </returns>
		public static string GetOptionalAttributeValue(XmlNode node, string attrName,
			string defaultString)
		{
			return node?.Attributes?[attrName]?.Value ?? defaultString;
		}

		/// <summary>
		/// Get an optional attribute value from an XElement.
		/// </summary>
		/// <param name="element">The XElement to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <param name="defaultString">The value to return if the attribute is not found.</param>
		/// <returns>The value of the attribute, or null, if not found.</returns>
		public static string GetOptionalAttributeValue(XElement element, string attrName,
			string defaultString)
		{
			if (element == null || !element.Attributes().Any())
				return defaultString;
			return element.Attribute(attrName)?.Value ?? defaultString;
		}

		/// <summary>
		/// Get an optional attribute value from an XmlNode.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="attrName">The attribute to find.</param>
		/// <returns>The value of the attribute, or null, if not found.</returns>
		/// <param name="defaultString">The value to return if the attribute is not found.</param>
		public static string GetOptionalAttributeValue(XPathNavigator node, string attrName,
			string defaultString)
		{
			if (node?.HasAttributes == true)
			{
				var s = node.GetAttribute(attrName, string.Empty);
				if (!string.IsNullOrEmpty(s))
					return s;
			}
			return defaultString;
		}

		/// <summary>
		/// Return the node that has the desired 'name', either the input node or a descendant.
		/// </summary>
		/// <param name="node">The XmlNode to look in.</param>
		/// <param name="name">The XmlNode name to find.</param>
		public static XmlNode FindNode(XmlNode node, string name)
		{
			if (node.Name == name)
				return node;

			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name == name)
					return childNode;

				XmlNode n = FindNode(childNode, name);
				if (n != null)
					return n;
			}
			return null;
		}

		/// <summary>
		/// Return the element that has the desired 'name', either the input element or a descendant.
		/// </summary>
		/// <param name="element">The XElement to look in.</param>
		/// <param name="name">The XElement name to find.</param>
		public static XElement FindElement(XElement element, string name)
		{
			if (element.Name == name)
				return element;
			foreach (var childElement in element.Elements())
			{
				if (childElement.Name == name)
					return childElement;
				var grandchildElement = FindElement(childElement, name);
				if (grandchildElement != null)
					return grandchildElement;
			}
			return null;
		}

		/// <summary>
		/// Find the index of the <paramref name="target"/> in <paramref name="elements"/> that
		/// 'matches' the <paramref name="target"/> element.
		/// </summary>
		/// <param name="elements">The elements to look in.</param>
		/// <param name="target">The target to look for a match of.</param>
		/// <returns>Return -1 if not found.</returns>
		public static int FindIndexOfMatchingNode(IEnumerable<XElement> elements, XElement target)
		{
			int index = 0;
			foreach (var element in elements)
			{
				if (NodesMatch(element, target))
					return index;
				index++;
			}
			return -1;
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
		public static string GetMandatoryAttributeValue(XmlNode node, string attrName)
		{
			string retval = GetOptionalAttributeValue(node, attrName, null);
			if (retval == null)
				throw new ApplicationException($"The attribute'{attrName}' is mandatory, but was missing. {node.OuterXml}");

			return retval;
		}

		/// <summary>
		/// Get an obligatory attribute value.
		/// </summary>
		/// <param name="element">The XElement to look in.</param>
		/// <param name="attrName">The required attribute to find.</param>
		/// <returns>The value of the attribute.</returns>
		/// <exception cref="ApplicationException">
		/// Thrown when the value is not found in the node.
		/// </exception>
		public static string GetMandatoryAttributeValue(XElement element, string attrName)
		{
			var retval = GetOptionalAttributeValue(element, attrName, null);
			if (retval == null)
				throw new ApplicationException($"The attribute'{attrName}' is mandatory, but was missing. {element}");

			return retval;
		}

		[PublicAPI]
		public static string GetMandatoryAttributeValue(XPathNavigator node, string attrName)
		{
			string retval = GetOptionalAttributeValue(node, attrName, null);
			if (retval == null)
				throw new ApplicationException($"The attribute'{attrName}' is mandatory, but was missing. {node.OuterXml}");

			return retval;
		}

		/// <summary>
		/// Append a child node with the specified name and value to <paramref name="node"/>.
		/// </summary>
		/// <param name="node">The node to which the child node is to be added</param>
		/// <param name="elementName">The name of the child element to add</param>
		public static XmlElement AppendElement(XmlNode node, string elementName)
		{
			XmlElement xe = node.OwnerDocument.CreateElement(elementName);
			node.AppendChild(xe);
			return xe;
		}

		/// <summary>
		/// Change the value of the specified attribute, appending it if not already present.
		/// </summary>
		public static void SetAttribute(XmlNode node, string attrName, string attrVal)
		{
			var attr = node.Attributes[attrName];
			if (attr != null)
			{
				attr.Value = attrVal;
			}
			else
			{
				attr = node.OwnerDocument.CreateAttribute(attrName);
				attr.Value = attrVal;
				node.Attributes.Append(attr);
			}
		}

		/// <summary>
		/// Change the value of the specified attribute, appending it if not already present.
		/// </summary>
		public static void SetAttribute(XElement element, string attrName, string attrVal)
		{
			var attr = element.Attribute(attrName);
			if (attr != null)
			{
				attr.Value = attrVal;
			}
			else
			{
				element.Add(new XAttribute(attrName, attrVal));
			}
		}

		/// <summary>
		/// Return true if the two nodes match. Corresponding children should match, as well as
		/// corresponding attributes (though not necessarily in the same order).
		/// The nodes are expected to be actually XmlElements; not tested for other cases.
		/// Comments do not affect equality.
		/// </summary>
		public static bool NodesMatch(XmlNode node1, XmlNode node2)
		{
			if (node1 == null && node2 == null)
				return true;

			if (node1 == null || node2 == null)
				return false;

			if (node1.Name != node2.Name)
				return false;

			if (node1.InnerText != node2.InnerText)
				return false;

			if (node1.Attributes == null && node2.Attributes != null)
				return false;

			if (node1.Attributes != null)
			{
				if (node2.Attributes == null)
					return false;

				if (node1.Attributes.Count != node2.Attributes.Count)
					return false;

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

			int iChild1 = 0; // index node1.ChildNodes
			int iChild2 = 0; // index node2.ChildNodes
			while (iChild1 < node1.ChildNodes.Count && iChild2 < node1.ChildNodes.Count)
			{
				XmlNode child1 = node1.ChildNodes[iChild1];

				// Note that we must defer doing the 'continue' until after we have checked to see if both children are comments
				// If we continue immediately and the last node of both elements is a comment, the second node will not have
				// iChild2 incremented and the final test will fail.
				bool foundComment = false;

				if (child1 is XmlComment)
				{
					iChild1++;
					foundComment = true;
				}
				XmlNode child2 = node2.ChildNodes[iChild2];
				if (child2 is XmlComment)
				{
					iChild2++;
					foundComment = true;
				}

				if (foundComment)
					continue;

				if (!NodesMatch(child1, child2))
					return false;

				iChild1++;
				iChild2++;
				
				// If we finished both lists we got a match.
				return iChild1 == node1.ChildNodes.Count && iChild2 == node2.ChildNodes.Count;
			}
			// both lists are null
			return true;
		}

		/// <summary>
		/// Return true if the two nodes match. Corresponding children should match, and
		/// corresponding attributes (though not necessarily in the same order).
		/// Comments do not affect equality.
		/// </summary>
		public static bool NodesMatch(XElement element1, XElement element2)
		{
			if (element1 == null && element2 == null)
				return true;
			if (element1 == null || element2 == null)
				return false;
			if (element1.Name != element2.Name)
				return false;
			if (element1.GetInnerText() != element2.GetInnerText())
				return false;
			if (!element1.Attributes().Any() && element2.Attributes().Any())
				return false;
			if (element1.Attributes().Any() && !element2.Attributes().Any())
				return false;
			if (element1.Attributes().Any())
			{
				if (element1.Attributes().Count() != element2.Attributes().Count())
					return false;
				foreach (var xa1 in element1.Attributes())
				{
					var xa2 = element2.Attribute(xa1.Name);
					if (xa2 == null || xa1.Value != xa2.Value)
						return false;
				}
			}
			if (!element1.HasElements && element2.HasElements)
				return false;
			if (element1.HasElements && !element2.HasElements)
				return false;
			if (element1.HasElements)
			{
				int iChild1 = 0; // index node1.Elements()
				int iChild2 = 0; // index node2.Elements()
				while (iChild1 < element1.Elements().Count() && iChild2 < element1.Elements().Count())
				{
					var child1 = element1.Elements().ToList()[iChild1];
					var child2 = element2.Elements().ToList()[iChild2];

					if (!NodesMatch(child1, child2))
						return false;
					iChild1++;
					iChild2++;
				}
				// If we finished both lists we got a match.
				return iChild1 == element1.Elements().Count() && iChild2 == element2.Elements().Count();
			}

			// both lists are null
			return true;
		}

		/// <summary>
		/// Return the first child of <paramref name="node"/> that is not a comment (or null).
		/// </summary>
		[PublicAPI]
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
		/// Return the first child of <paramref name="element"/> that is not a comment (or null).
		/// </summary>
		[PublicAPI]
		public static XElement GetFirstNonCommentChild(XElement element)
		{
			return element?.Elements().FirstOrDefault();
		}

		/// <summary>
		/// Fix the string to be safe in a text region of XML.
		/// </summary>
		/// <param name="sInput">The string within which to escape invalid characters.</param>
		/// <returns>The input string with invalid characters replaced.</returns>
		/// <remarks>Since multi-paragraph content is not expected, carriage return and newline
		/// characters will not be escaped. As a result, they will be treated as whitespace,
		/// though their  exact handling during serialization may depend on XML writer settings.
		/// </remarks>
		/// <seealso cref="ConvertMultiParagraphToSafeXml"/>
		/// <seealso cref="MakeSafeXmlAttribute"/>
		public static string MakeSafeXml(string sInput) => MakeSafeXml(sInput, false);

		/// <summary>
		/// Efficient single-pass implementation for <see cref="MakeSafeXml"/> and
		/// <see cref="ConvertMultiParagraphToSafeXml"/>.
		/// </summary>
		private static string MakeSafeXml(string sInput, bool multiPara)
		{
			if (sInput == null)
				return null;
			StringBuilder stringBuilder = null;

			for (int i = 0; i < sInput.Length; i++)
			{
				void AddToBuilder(string escSequence)
				{
					if (stringBuilder == null)
					{
						var capacity = sInput.Length + escSequence.Length;
						stringBuilder = i > 1
							? new StringBuilder(sInput.Substring(0, i), capacity)
							: new StringBuilder(capacity);
					}

					stringBuilder.Append(escSequence);
				}

				switch (sInput[i])
				{
					// Ideally, we should be able to make use of System.Security rather than having
					// to hardcode these replacements. However, it doesn't expose exactly what we need.
					case '<': AddToBuilder("&lt;"); break;
					case '>': AddToBuilder("&gt;"); break;
					case '&': AddToBuilder("&amp;"); break;
					case '\r':
						if (!multiPara)
							goto default;
						AddToBuilder("\u2028");
						break;
					case '\n':
						if (!multiPara)
							goto default;
						if (i > 1 && Environment.NewLine.Length > 1 &&
						    Environment.NewLine[0] == sInput[i - 1])
							break;
						AddToBuilder("\u2028");
						break;

					default:
						stringBuilder?.Append(sInput[i]);
						break;
				}
			}

			return stringBuilder == null ? sInput : stringBuilder.ToString();
		}

		/// <summary>
		/// Convert a possibly multi-paragraph string to a form that is safe to store both in an
		/// XML file.
		/// </summary>
		/// <seealso cref="MakeSafeXml"/>
		[PublicAPI]
		public static string ConvertMultiParagraphToSafeXml(string sInput) =>
			MakeSafeXml(sInput, true);

		/// <summary>
		/// Convert a possibly multi-paragraph string to a form that is safe to store both in an
		/// XML file.
		/// </summary>
		[Obsolete("Use ConvertMultiParagraphToSafeXml")]
		public static string ConvertMultiparagraphToSafeXml(string sInput) =>
			ConvertMultiParagraphToSafeXml(sInput);

		/// <summary>
		/// Fix the string to be safe in an attribute value of XML.
		/// </summary>
		/// <param name="sInput">The string within which to escape invalid characters.</param>
		/// <returns>The input string with invalid characters replaced.</returns>
		/// <seealso cref="MakeSafeXml"/>
		[PublicAPI]
		public static string MakeSafeXmlAttribute(string sInput)=> 
			SecurityElement.Escape(sInput);

		/// <summary>
		/// Convert an encoded attribute string into plain text.
		/// </summary>
		/// <param name="sInput">The string within which to unescape characters.</param>
		/// <returns>The plain text representation, with all the XML codes replaced by their normal
		/// (single-character) textual representations</returns>
		public static string DecodeXmlAttribute(string sInput)
		{
			// ENHANCE: This could be made more efficient by using a StringBuilder and a single
			// pass through the data.
			string sOutput = sInput;
			if (!string.IsNullOrEmpty(sOutput) && sOutput.Contains("&"))
			{
				sOutput = sOutput.Replace("&gt;", ">");
				sOutput = sOutput.Replace("&lt;", "<");
				sOutput = sOutput.Replace("&apos;", "'");
				sOutput = sOutput.Replace("&quot;", "\"");
				sOutput = sOutput.Replace("&amp;", "&");
			}

			for (int idx = sOutput.IndexOf("&#"); idx >= 0; idx = sOutput.IndexOf("&#"))
			{
				int idxEnd = sOutput.IndexOf(';', idx);
				if (idxEnd < 0)
					break;
				string sOrig = sOutput.Substring(idx, (idxEnd - idx) + 1);
				string sNum = sOutput.Substring(idx + 2, idxEnd - (idx + 2));
				string sReplace = null;
				if (sNum[0] == 'x' || sNum[0] == 'X')
				{
					if (int.TryParse(sNum.Substring(1), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out var chNum))
						sReplace = char.ConvertFromUtf32(chNum);
				}
				else
				{
					if (int.TryParse(sNum, out var chNum))
						sReplace = char.ConvertFromUtf32(chNum);
				}
				sReplace ??= sNum;
				sOutput = sOutput.Replace(sOrig, sReplace);
			}
			return sOutput;
		}

		/// <summary>
		/// lifted from http://www.knowdotnet.com/articles/indentxml.html
		/// </summary>
		/// <param name="xml">The unformatted XML string</param>
		public static string GetIndentedXml(string xml)
		{
			using (MemoryStream ms = new MemoryStream())
			// Create a XMLTextWriter that will send its output to a memory stream (file)
			using (XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.Unicode))
			{
				XmlDocument doc = new XmlDocument();

//             try
//             {
				 // Load the unformatted XML text string into an instance
				 // of the XML Document Object Model (DOM)
				 doc.LoadXml(xml);

				 // Set the formatting property of the XML Text Writer to be indented
				 // the text writer is where the indenting will be performed
				 xtw.Formatting = Formatting.Indented;

				// write DOM XML to the XmlTextWriter
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

		[PublicAPI]
		public static XmlElement GetOrCreateElementPredicate(XmlDocument dom, XmlElement parent, string predicate, string name)
		{
			XmlElement element = (XmlElement)parent.SelectSingleNodeHonoringDefaultNS("/" + predicate);
			if (element == null)
			{
				element = parent.OwnerDocument.CreateElement(name, parent.NamespaceURI);
				parent.AppendChild(element);
			}
			return element;
		}

		public static XmlElement GetOrCreateElement(XmlDocument dom, string parentPath, string name)
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
				throw new XmlFormatException($"Expected a {attr} attribute on {form.OuterXml}.");
			}
		}

		public static string GetOptionalAttributeString(XmlNode xmlNode, string attributeName)
		{
			return xmlNode.Attributes[attributeName]?.Value;
		}

		[PublicAPI]
		public static XmlNode GetDocumentNodeFromRawXml(string outerXml, XmlNode nodeMaker)
		{
			if (string.IsNullOrEmpty(outerXml))
				throw new ArgumentException();

			if (!(nodeMaker is XmlDocument doc))
			{
				doc = nodeMaker.OwnerDocument;
				Debug.Assert(doc != null);
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

		[PublicAPI]
		public static string GetXmlForShowingInHtml(string xml)
		{
			var s = GetIndentedXml(xml).Replace("<", "&lt;");
			s = s.Replace("\r\n", "<br/>");
			s = s.Replace("  ", "&nbsp;&nbsp;");
			return s;
		}

		[PublicAPI]
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
		/// <param name="writer">The XML writer to be used for writing the node</param>
		/// <param name="dataToWrite">The XML (as a string) to be written</param>
		/// <param name="suppressIndentingChildren">Names of child nodes that should not be indented</param>
		/// <param name="preserveNamespaces">a set of namespaces to preserve when writing out elements</param>
		public static void WriteNode(XmlWriter writer, string dataToWrite, HashSet<string> suppressIndentingChildren, HashSet<string> preserveNamespaces = null)
		{
			XElement element = XDocument.Parse(dataToWrite).Root;
			WriteNode(writer, element, suppressIndentingChildren, preserveNamespaces);
		}

		/// <summary>
		/// Write a node out containing the XML in dataToWrite, pretty-printed according to the rules of writer, except
		/// that we suppress indentation for children of nodes whose names are listed in suppressIndentingChildren,
		/// and also for "mixed" nodes (where some children are text).
		/// </summary>
		/// <param name="writer">The XML writer to be used for writing the node</param>
		/// <param name="dataToWrite">The XML element to be written</param>
		/// <param name="suppressIndentingChildren">Names of child nodes that should not be indented</param>
		/// <param name="preserveNamespaces">a set of namespaces to preserve when writing out elements</param>
		public static void WriteNode(XmlWriter writer, XElement dataToWrite, HashSet<string> suppressIndentingChildren, HashSet<string> preserveNamespaces = null)
		{
			if (dataToWrite == null)
				return;
			WriteElementTo(writer, dataToWrite, suppressIndentingChildren, preserveNamespaces);
		}

		/// <summary>
		/// Recursively write an element to the writer, suppressing indentation of children when required.
		/// </summary>
		/// <param name="writer">The XML writer to be used</param>
		/// <param name="element">The XML element to be written</param>
		/// <param name="suppressIndentingChildren">Names of child nodes that should not be indented</param>
		/// <param name="preserveNamespaces">a set of namespaces to preserve when writing out elements</param>
		private static void WriteElementTo(XmlWriter writer, XElement element, HashSet<string> suppressIndentingChildren, HashSet<string> preserveNamespaces = null)
		{
			writer.WriteStartElement(element.Name.LocalName);
			foreach (var attr in element.Attributes())
			{
				// if we are preserving namespaces, we may need to write the attribute with the prefix
				if (preserveNamespaces != null && !string.IsNullOrEmpty(attr.Name.NamespaceName))
				{
					var attrPrefix = element.GetPrefixOfNamespace(attr.Name.NamespaceName);
					if (preserveNamespaces.Contains(attrPrefix))
					{
						if (attrPrefix == "xmlns")
						{
							// If you need to write out the xmlns attribute consistently between platforms custom code will need to be written
							// I'm leaving it unimplemented until needed
							throw new ArgumentException("The 'xmlns' local namespace declarations are handled differently in framework. Using it could cause thrashing.");
						}
						writer.WriteAttributeString(attrPrefix, attr.Name.LocalName, attr.Name.NamespaceName, attr.Value);
					}
					else
						writer.WriteAttributeString(attr.Name.LocalName, attr.Value);
				}
				else
					writer.WriteAttributeString(attr.Name.LocalName, attr.Value);
			}

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
				if (child is XElement xElement)
					WriteElementTo(writer, xElement, suppressIndentingChildren, preserveNamespaces);
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
					code = char.ConvertToUtf32(s, i);
				}
				catch (ArgumentException)
				{
					continue;
				}
				if (IsLegalXmlChar(code))
					buffer.Append(char.ConvertFromUtf32(code));
				if (char.IsSurrogatePair(s, i))
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
		/// Removes namespaces from the Xml document (makes querying easier)
		/// </summary>
		public static XDocument RemoveNamespaces(this XDocument document)
		{
			document.Root?.RemoveNamespaces();
			return document;
		}

		/// <summary>
		/// Removes namespaces from the Xml element and its children (makes querying easier)
		/// </summary>
		public static XElement RemoveNamespaces(this XElement element)
		{
			RemoveNamespaces(element.DescendantsAndSelf());
			return element;
		}

		private static void RemoveNamespaces(IEnumerable<XElement> elements)
		{
			foreach (var element in elements)
			{
				element.Attributes().Where(attribute => attribute.IsNamespaceDeclaration).ForEach(attribute => attribute.Remove());
				element.Name = element.Name.LocalName;
			}
		}

		/// <summary>
		/// Allow the visitor to 'visit' each attribute in the input XmlNode.
		/// </summary>
		/// <param name="input">XML node to be "visited"</param>
		/// <param name="visitor">Implementation of <see cref="IAttributeVisitor"/></param>
		/// <returns><c>true</c> if any Visit call returns <c>true</c></returns>
		public static bool VisitAttributes(XmlNode input, IAttributeVisitor visitor)
		{
			bool fSuccessfulVisit = false;
			if (input.Attributes != null) // can be, e.g, if Input is a XmlTextNode
			{
				foreach (XmlAttribute xa in input.Attributes)
				{
					if (visitor.Visit(new XAttribute(xa.Name, xa.Value)))
						fSuccessfulVisit = true;
				}
			}

			foreach (XmlNode child in input.ChildNodes)
			{
				if (VisitAttributes(child, visitor))
					fSuccessfulVisit = true;
			}
			return fSuccessfulVisit;
		}

		/// <summary>
		/// Allow the visitor to 'visit' each attribute in the input element.
		/// </summary>
		/// <param name="input">XML element to be "visited"</param>
		/// <param name="visitor">Implementation of <see cref="IAttributeVisitor"/></param>
		/// <returns><c>true</c> if any Visit call returns <c>true</c></returns>
		public static bool VisitAttributes(XElement input, IAttributeVisitor visitor)
		{
			bool fSuccessfulVisit = false;
			if (input.HasAttributes) // can be, e.g, if Input is a XmlTextNode
			{
				foreach (XAttribute xa in input.Attributes())
				{
					if (visitor.Visit(xa))
						fSuccessfulVisit = true;
				}
			}
			if (input.Elements().Any()) // not sure whether this can happen.
			{
				foreach (var child in input.Elements())
				{
					if (VisitAttributes(child, visitor))
						fSuccessfulVisit = true;
				}
			}
			return fSuccessfulVisit;
		}
	}

	/// <summary>
	/// Interface for operations we can apply to attributes.
	/// </summary>
	public interface IAttributeVisitor
	{
		bool Visit(XAttribute xa);
	}

	/// <summary>
	/// "Visitor" that replaces a substring in the value of an attribute.
	/// </summary>
	public class ReplaceSubstringInAttr : IAttributeVisitor
	{
		readonly string _pattern;
		readonly string _replacement;

		/// <summary>
		/// Sets up an object that can be used to replace a particular substring in the value of an
		/// attribute.
		/// </summary>
		/// <param name="pattern">The text of the substring to search for (using case-sensitive,
		/// ordinal comparison)</param>
		/// <param name="replacement">The replacement text</param>
		public ReplaceSubstringInAttr(string pattern, string replacement)
		{
			_pattern = pattern;
			_replacement = replacement;
		}

		/// <summary>
		/// Actually performs the replacement in the given attribute.
		/// </summary>
		/// <returns>Whether the pattern was found and the replacement made</returns>
		public virtual bool Visit(XAttribute xa)
		{
			string old = xa.Value;
			int index = old.IndexOf(_pattern, StringComparison.Ordinal);
			if (index < 0)
				return false;
			xa.Value = old.Replace(_pattern, _replacement);
			return true;
		}
	}
}