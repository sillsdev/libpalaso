using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Palaso
{
	public class XmlHelpers
	{

//        /// <summary>
//        /// Append an attribute with the specified name and value to parent.
//        /// </summary>
//        /// <param name="parent"></param>
//        /// <param name="attrName"></param>
//        /// <param name="attrVal"></param>
//        public static void AppendAttribute(XmlNode parent, string attrName, string attrVal)
//        {
//            XmlAttribute xa = parent.OwnerDocument.CreateAttribute(attrName);
//            xa.Value = attrVal;
//            parent.Attributes.Append(xa);
//        }

		public static void AddOrUpdateAttribute(XmlNode node, string attributeName, string value)
		{
			XmlNode attr = GetDocument(node).CreateAttribute(attributeName);
			attr.Value = value;
			node.Attributes.SetNamedItem(attr);
//
//            XmlAttribute attribute = node.Attributes[attributeName];
//            if (attribute == null)
//            {
//                attribute = node.OwnerDocument.CreateAttribute(attributeName);
//                node.Attributes.Append(attribute);
//            }
//
//            attribute.Value = value;
		}

		public static XmlNode GetOrCreateElement(XmlNode node, string xpathNotIncludingElement, string elementName)
		{
			//enhance: if the parent path isn't found, strip of the last piece and recurse,
			//so that the path will always be created if needed.

			XmlNode parentNode = node.SelectSingleNode(xpathNotIncludingElement);
			if (parentNode == null)
			{
				throw new ApplicationException(string.Format("The path {0} could not be found", xpathNotIncludingElement));
			}
			XmlNode n = parentNode.SelectSingleNode(elementName);
			if (n == null)
			{
				n = GetDocument(node).CreateElement(elementName);
				parentNode.AppendChild(n);
			}
			return n;
		}

		private static XmlDocument GetDocument(XmlNode nodeOrDoc)
		{
				if (nodeOrDoc is XmlDocument)
				{
					return (XmlDocument)nodeOrDoc;
				}
				else
				{
					return nodeOrDoc.OwnerDocument;
				}
		}
	}
}
