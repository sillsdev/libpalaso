using System;
using System.Linq;
using System.Xml.Linq;

using System.Text;

namespace Palaso.Extensions
{
	public static class XElementExtensions
	{
		/// <summary>
		/// Get the attribute value of the element as a string.
		/// <param name="element">parent XElement</param>
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
		/// <param name="element">parent XElement</param>
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
				value = (string) childElem.Attribute(attribute) ?? string.Empty;
			}
			return value;
		}

		/// <summary>
		/// Get the attribute value of a child element as a string.
		/// <param name="element">parent XElement</param>
		/// <param name="ns">namespace</param>
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
				value = (string ) childElem.Attribute(attribute) ?? string.Empty;
			}
			return value;
		}
	}
}
