using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SIL.Xml
{
	public static class XElementExtensions
	{
		/// <summary>
		/// Get the first (in document order) child element with the specified XName that doesn't have an "alt" attribute
		/// </summary>
		/// <param name="element"></param>
		/// <param name="name">The XName to match</param>
		/// <returns>A XElement that matches the specified XName, or null</returns>
		public static XElement NonAltElement(this XElement element, XName name)
		{
			return element.Elements(name).FirstOrDefault(e => e.Attribute("alt") == null);
		}

		/// <summary>
		/// Returns a collection of the child elements of this element. Only elements that have
		/// a matching XName and don't have an "alt" attribute are included in the collection
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="name">The name.</param>
		public static IEnumerable<XElement> NonAltElements(this XElement element, XName name)
		{
			return element.Elements(name).Where(e => e.Attribute("alt") == null);
		}

		/// <summary>
		/// Returns a collection of the child elements of this element. Only elements that
		/// don't have an "alt" attribute are included in the collection
		/// </summary>
		/// <param name="element">The element.</param>
		public static IEnumerable<XElement> NonAltElements(this XElement element)
		{
			return element.Elements().Where(e => e.Attribute("alt") == null);
		}

		/// <summary>
		/// Returns a collection of the child elements of this element. Only elements that have a matching XName
		/// and don't have an "alt" attribute or a "draft" attribute are included in the collection
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="name">The name.</param>
		public static IEnumerable<XElement> NonAltNonDraftElements(this XElement element, XName name)
		{
			return element.Elements(name).Where(e => e.Attribute("alt") == null && e.Attribute("draft") == null);
		}

		/// <summary>
		/// Get the attribute value of a child element as a string.
		/// <param name="element">parent XElement</param>
		/// <param name="child">child element name</param>
		/// <param name="attribute">attribute to return</param>
		/// <returns>attribute value; null if there is no attribute with the specified name</returns>
		/// </summary>
		public static string GetAttributeValue(this XElement element, string child, string attribute)
		{
			string value = null;
			XElement childElem = element.NonAltElement(child);
			if (childElem != null)
			{
				value = (string) childElem.Attribute(attribute);
			}
			return value;
		}

		/// <summary>
		/// Get the attribute value of a child element as a string.
		/// <param name="element">parent XElement</param>
		/// <param name="child">XName of the child element</param>
		/// <param name="attribute">attribute to return</param>
		/// <returns>attribute value; null if there is no attribute with the specified name</returns>
		/// </summary>
		public static string GetAttributeValue(this XElement element, XName child, string attribute)
		{
			string value = null;
			XElement childElem = element.NonAltElement(child);
			if (childElem != null)
			{
				value = (string) childElem.Attribute(attribute);
			}
			return value;
		}

		/// <summary>
		/// Get the child element that doesn't have the alt attribute. If it doesn't exist, create
		/// one.
		/// </summary>
		/// <param name="element">parent element</param>
		/// <param name="child">string name of the child element</param>
		/// <returns>child element</returns>
		public static XElement GetOrCreateElement(this XElement element, string child)
		{
			XElement childElem = element.NonAltElement(child);
			if (childElem == null)
			{
				childElem = new XElement(child);
				element.Add(childElem);
			}
			return childElem;
		}

		/// <summary>
		/// Get the child element that doesn't have the alt attribute. If it doesn't exist, create
		/// one.
		/// </summary>
		/// <param name="element">parent element</param>
		/// <param name="child">XName of the child element</param>
		/// <returns>child element</returns>
		public static XElement GetOrCreateElement(this XElement element, XName child)
		{
			XElement childElem = element.NonAltElement(child);
			if (childElem == null)
			{
				childElem = new XElement(child);
				element.Add(childElem);
			}
			return childElem;
		}

		/// <summary>
		/// Set the attribute value of a child element as a string.
		/// If value is not null or empty, the child element is created if it doesn't exist.
		/// If value is null or empty, the attribute is removed.
		/// </summary>
		/// <param name="element">parent XElement</param>
		/// <param name="child">child element name</param>
		/// <param name="attribute">attribute to set</param>
		/// <param name="value">attribute value</param>
		public static void SetAttributeValue(this XElement element, string child, string attribute, string value)
		{
			XElement childElem = element.NonAltElement(child);
			if (!string.IsNullOrEmpty(value))
			{
				if (childElem == null)
					childElem = element.GetOrCreateElement(child);
				childElem.SetAttributeValue(attribute, value);
			}
			else if (childElem != null)
			{
				// Child element exists, so remove the blank attribute
				XAttribute attr = childElem.Attribute(attribute);
				if (attr != null)
				{
					attr.Remove();
				}
			}
		}

		/// <summary>
		/// Set the attribute value of a child element as a string.
		/// If value is not null or empty, the child element is created if it doesn't exist.
		/// If value is null or empty, the attribute is removed.
		/// </summary>
		/// <param name="element">parent XElement</param>
		/// <param name="child">XName of the child element</param>
		/// <param name="attribute">attribute to set</param>
		/// <param name="value">attribute value</param>
		public static void SetAttributeValue(this XElement element, XName child, string attribute, string value)
		{
			XElement childElem = element.NonAltElement(child);
			if (!string.IsNullOrEmpty(value))
			{
				if (childElem == null)
					childElem = element.GetOrCreateElement(child);
				childElem.SetAttributeValue(attribute, value);
			}
			else if (childElem != null)
			{
				// Child element exists, so remove the blank attribute
				XAttribute attr = childElem.Attribute(attribute);
				if (attr != null)
				{
					attr.Remove();
				}
			}
		}

		/// <summary>
		/// Set the attribute value of an element as a string.
		/// If the value is null or empty, the attribute is removed
		/// </summary>
		/// <remarks>
		/// This extension was created because XElement.SetAttributeValue() still creates attributes if the value is empty.
		/// </remarks>
		/// <param name="element">base XElement</param>
		/// <param name="attribute">attribute to set</param>
		/// <param name="value">attribute value</param>
		public static void SetOptionalAttributeValue(this XElement element, string attribute, string value)
		{
			element.SetAttributeValue(attribute, string.IsNullOrEmpty(value) ? null : value);
		}

		/// <summary>
		/// Clone the XElement.
		/// </summary>
		/// <returns>A copy of the XElement</returns>
		public static XElement Clone(this XElement element)
		{
			return XElement.Parse(element.ToString());
		}

		/// <summary>
		/// Get an XML string for the given XElement.
		/// </summary>
		/// <returns>Equivalent of "OuterXml" for an XmlNode</returns>
		public static string GetOuterXml(this XElement element)
		{
			return element.ToString();
		}

		/// <summary>
		/// Get an XML string for all children.
		/// </summary>
		/// <returns>Equivalent of "InnerText" for an XmlNode</returns>
		public static string GetInnerText(this XElement element)
		{
			return element.Value;
		}

		/// <summary>
		/// Get an XML string for all children.
		/// </summary>
		/// <returns>Equivalent of "InnerText" for an XmlNode</returns>
		public static string GetInnerXml(this XElement element)
		{
			return string.Concat(element.Elements());
		}
	}
}
