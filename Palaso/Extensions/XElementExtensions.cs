using System.Xml.Linq;

namespace Palaso.Extensions
{
	public static class XElementExtensions
	{
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
			XElement childElem = element.Element(child);
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
			XElement childElem = element.Element(child);
			if (childElem != null)
			{
				value = (string) childElem.Attribute(attribute);
			}
			return value;
		}

		/// <summary>
		/// Get the child element.  If it doesn't exist, create one
		/// </summary>
		/// <param name="element">parent element</param>
		/// <param name="child">string name of the child element</param>
		/// <returns>child element</returns>
		public static XElement GetOrCreateElement(this XElement element, string child)
		{
			XElement childElem = element.Element(child);
			if (childElem == null)
			{
				childElem = new XElement(child);
				element.Add(childElem);
			}
			return childElem;
		}

		/// <summary>
		/// Get the child element.  If it doesn't exist, create one
		/// </summary>
		/// <param name="element">parent element</param>
		/// <param name="child">XName of the child element</param>
		/// <returns>child element</returns>
		public static XElement GetOrCreateElement(this XElement element, XName child)
		{
			XElement childElem = element.Element(child);
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
			XElement childElem = element.Element(child);
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
			XElement childElem = element.Element(child);
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
