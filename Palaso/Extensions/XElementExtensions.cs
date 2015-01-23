using System.Xml.Linq;

namespace Palaso.Extensions
{
	public static class XElementExtensions
	{
		/// <summary>
		/// Get the attribute value of the element as a string.
		/// <param name="element">base XElement</param>
		/// <param name="attribute">attribute to return</param>
		/// <returns>attribute value.  string.Empty if the attribute doesn't exist</returns>
		/// </summary>
		public static string GetAttributeValue(this XElement element, string attribute)
		{
			string value = (string) element.Attribute(attribute) ?? string.Empty;
			return value;
		}

		/// <summary>
		/// Get the attribute value of the element as a string
		/// </summary>
		/// <param name="element">base XElement</param>
		/// <param name="attribute">XName of the attribute to return</param>
		/// <returns>attribute value.  string.Empty if the attribute doesn't exist</returns>
		public static string GetAttributeValue(this XElement element, XName attribute)
		{
			string value = (string) element.Attribute(attribute) ?? string.Empty;
			return value;
		}

		/// <summary>
		/// Get the attribute value of a child element as a string.
		/// <param name="element">parent XElement</param>
		/// <param name="child">child element name</param>
		/// <param name="attribute">attribute to return</param>
		/// <returns>attribute value.  string.Empty if the attribute doesn't exist</returns>
		/// </summary>
		public static string GetAttributeValue(this XElement element, string child, string attribute)
		{
			string value = string.Empty;
			XElement childElem = element.Element(child);
			if (childElem != null)
			{
				value = childElem.GetAttributeValue(attribute);
			}
			return value;
		}

		/// <summary>
		/// Get the attribute value of a child element as a string.
		/// <param name="element">parent XElement</param>
		/// <param name="child">XName of the child element</param>
		/// <param name="attribute">attribute to return</param>
		/// <returns>attribute value.  string.Empty if the attribute doesn't exist</returns>
		/// </summary>
		public static string GetAttributeValue(this XElement element, XName child, string attribute)
		{
			string value = string.Empty;
			XElement childElem = element.Element(child);
			if (childElem != null)
			{
				value = childElem.GetAttributeValue(attribute);
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
		/// Set the attribute value of a child element as a string.  If the child element doesn't exist, it is created.
		/// The attribute is removed if the value is null.
		/// </summary>
		/// <param name="element">parent XElement</param>
		/// <param name="child">child element name</param>
		/// <param name="attribute">attribute to set</param>
		/// <param name="value">attribute value</param>
		public static void SetAttributeValue(this XElement element, string child, string attribute, string value)
		{
			XElement childElem = element.GetOrCreateElement(child);
			XAttribute attr = childElem.Attribute(attribute);
			if (!string.IsNullOrEmpty(value))
				childElem.SetAttributeValue(attribute, value);
			else if (attr != null)
			{
				attr.Remove();
			}
		}

		/// <summary>
		/// Set the attribute value of a child element as a string.  If the child element doesn't exist, it is created.
		/// The attribute is removed if the value is null.
		/// </summary>
		/// <param name="element">parent XElement</param>
		/// <param name="child">XName of the child element</param>
		/// <param name="attribute">attribute to set</param>
		/// <param name="value">attribute value</param>
		public static void SetAttributeValue(this XElement element, XName child, string attribute, string value)
		{
			XElement childElem = element.GetOrCreateElement(child);
			XAttribute attr = childElem.Attribute(attribute);
			if (!string.IsNullOrEmpty(value))
				childElem.SetAttributeValue(attribute, value);
			else if (attr != null)
			{
				attr.Remove();
			}
		}
	}
}
