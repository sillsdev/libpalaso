using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SIL.WritingSystems
{
	internal static class XElementExtensions
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
		/// Returns a collection of the child elements of this element.  Only elements that have
		/// a matching XName and don't have an "alt" attribute are included in the collection
		/// </summary>
		/// <param name="element"></param>
		/// <param name="name">The XName to match</param>
		/// <returns>An IEnumerable<typeparam name=">of XElement"></typeparam></returns>
		public static IEnumerable<XElement> NonAltElements(this XElement element, XName name)
		{
			return element.Elements(name).Where(e => e.Attribute("alt") == null);
		}

		/// <summary>
		/// Returns a collection of the child elements of this element.  Only elements that 
		/// don't have an "alt" attribute are included in the collection
		/// </summary>
		/// <param name="element"></param>
		/// <returns>An IEnumerable<typeparam name=">of XElement"></typeparam></returns>
		public static IEnumerable<XElement> NonAltElements(this XElement element)
		{
			return element.Elements().Where(e => e.Attribute("alt") == null);
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
		/// Get the child element that doesn't have the alt attribute.  If it doesn't exist, create one
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
		/// Get the child element that doesn't have the alt attribute.  If it doesn't exist, create one
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
	}
}
