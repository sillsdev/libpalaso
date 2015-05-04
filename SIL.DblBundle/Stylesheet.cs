using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using SIL.Xml;

namespace SIL.DblBundle
{
	/// <summary>
	/// Stylesheet shipped with a DBL Bundle
	/// </summary>
	[XmlRoot("stylesheet")]
	public class Stylesheet : IStylesheet
	{
		private Dictionary<string, Style> m_styleLookup;
		private static int s_defaultFontSize = 10;

		/// <summary>
		/// Load the stylesheet at the given path
		/// </summary>
		public static Stylesheet Load(string filename, out Exception exception)
		{
			return XmlSerializationHelper.DeserializeFromFile<Stylesheet>(filename, out exception);
		}

		/// <summary>
		/// The default font size if not found in the style sheet. Default can be overridden by the caller. Set it before calling Load.
		/// </summary>
		public static int DefaultFontSize
		{
			get { return s_defaultFontSize; }
			set { s_defaultFontSize = value; }
		}

		/// <summary>
		/// property elements in the stylesheet
		/// </summary>
		[XmlElement(ElementName = "property")]
		public List<StylesheetProperty> Properties { get; set; }

		/// <summary>
		/// Get the style from the stylesheet based on the styleId
		/// </summary>
		/// <param name="styleId"></param>
		/// <returns>An IStyle object with the given styleId</returns>
		public IStyle GetStyle(string styleId)
		{
			if (m_styleLookup == null)
				m_styleLookup = Styles.ToDictionary(s => s.Id, s => s);

			Style style;
			if (m_styleLookup.TryGetValue(styleId, out style))
				return style;

			Debug.Fail("Should never get here. Either we encountered an unknown style or dictionary got created prematurely.");

			return Styles.FirstOrDefault(s => s.Id == styleId);
		}

		/// <summary>
		/// The font family as set in the stylesheet
		/// </summary>
		public string FontFamily
		{
			get
			{
				var fontFamilyProperty = Properties.FirstOrDefault(p => p.Name == "font-family");
				return fontFamilyProperty != null ? fontFamilyProperty.Value : null;
			}
		}

		/// <summary>
		/// The font size (in pt) as set in the stylesheet
		/// </summary>
		public int FontSizeInPoints
		{
			get
			{
				var fontSizeProperty = Properties.FirstOrDefault(p => p.Name == "font-size");
				int val;
				if (fontSizeProperty == null || !Int32.TryParse(fontSizeProperty.Value, out val))
					return DefaultFontSize;

				if (fontSizeProperty.Unit == "pt")
					return val;

				// REVIEW: Are any other units possible?
				return DefaultFontSize;
			}
		}

		/// <summary>
		/// All the styles defined in the stylesheet
		/// </summary>
		[XmlElement(ElementName = "style")]
		public List<Style> Styles { get; set; }
	}

	/// <summary>
	/// A stylesheet property element
	/// </summary>
	public class StylesheetProperty
	{
		/// <summary>
		/// Name of the property
		/// </summary>
		[XmlAttribute("name")]
		public string Name { get; set; }

		/// <summary>
		/// Unit of the property (optional)
		/// </summary>
		[XmlAttribute("unit")]
		public string Unit { get; set; }

		/// <summary>
		/// Value of the property
		/// </summary>
		[XmlText]
		public string Value { get; set; }
	}
}
