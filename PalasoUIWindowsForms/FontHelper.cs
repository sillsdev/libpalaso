using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Xml.Serialization;

namespace Palaso.UI.WindowsForms
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Encapsulates a font object that can be serialized.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[XmlType("Font")]
	public class SerializableFont
	{
		[XmlAttribute]
		public string Name ;
		[XmlAttribute]
		public float Size = 10;
		[XmlAttribute]
		public bool Bold;
		[XmlAttribute]
		public bool Italic;

		/// ------------------------------------------------------------------------------------
		public SerializableFont()
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Intializes a new Serializable font object from the specified font.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public SerializableFont(Font fnt)
		{
			Font = fnt;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a font object based on the SerializableFont's settings or sets the
		/// SerializableFont's settings.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public Font Font
		{
			get
			{
				if (Name == null)
					return null;

				FontStyle style = FontStyle.Regular;

				if (Bold)
					style = FontStyle.Bold;

				if (Italic)
					style |= FontStyle.Italic;

				return FontHelper.MakeFont(Name, (int)Size, style);
			}
			set
			{
				if (value == null)
					Name = null;
				else
				{
					Name = value.Name;
					Size = value.SizeInPoints;
					Bold = value.Bold;
					Italic = value.Italic;
				}
			}
		}
	}

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Misc. font helper methods.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public static class FontHelper
	{
		/// --------------------------------------------------------------------------------
		static FontHelper()
		{
			ResetFonts();
		}

		/// ------------------------------------------------------------------------------------
		public static void ResetFonts()
		{
			UIFont = (Font)SystemFonts.MenuFont.Clone();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Determines if the specified font is installed on the computer.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool FontInstalled(string fontName)
		{
			fontName = fontName.ToLower();

			using (var installedFonts = new InstalledFontCollection())
			{
				if (installedFonts.Families.Any(f => f.Name.ToLower() == fontName))
					return true;
			}

			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a new font object that is a regualar style derivative of the specified
		/// font, having the specified size.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Font MakeRegularFontDerivative(Font fnt, float size)
		{
			return MakeFont((fnt ?? UIFont).FontFamily.Name, size, FontStyle.Regular);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a string containing three pieces of information about the specified font:
		/// the name, size and style.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string FontToString(Font fnt)
		{
			if (fnt == null)
				return null;

			return fnt.Name + ", " + fnt.SizeInPoints + ", " + fnt.Style;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a font object with the specified properties. If an error occurs while
		/// making the font (e.g. because the font doesn't support a particular style) a
		/// fallback scheme is used.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Font MakeFont(string fontString)
		{
			if (fontString == null)
				return SystemFonts.DefaultFont;

			var name = SystemFonts.DefaultFont.FontFamily.Name;
			var size = SystemFonts.DefaultFont.SizeInPoints;
			var style = FontStyle.Regular;

			var parts = fontString.Split(',');
			if (parts.Length > 0)
				name = parts[0];

			if (parts.Length > 1)
				float.TryParse(parts[1], out size);

			if (parts.Length > 2)
			{
				try
				{
					style = (FontStyle)Enum.Parse(typeof(FontStyle), parts[2]);
				}
				catch { }
			}

			return MakeFont(name, size, style);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a font object with the specified properties. If an error occurs while
		/// making the font (e.g. because the font doesn't support a particular style) a
		/// fallback scheme is used.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Font MakeFont(Font fnt, float size, FontStyle style)
		{
			return MakeFont(fnt.FontFamily.Name, size, style);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a font object with the specified properties. If an error occurs while
		/// making the font (e.g. because the font doesn't support a particular style) a
		/// fallback scheme is used.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Font MakeFont(Font fnt, float size)
		{
			return MakeFont(fnt.FontFamily.Name, size, fnt.Style);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a font object with the specified properties. If an error occurs while
		/// making the font (e.g. because the font doesn't support a particular style) a
		/// fallback scheme is used.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Font MakeFont(Font fnt, FontStyle style)
		{
			return MakeFont(fnt.FontFamily.Name, fnt.SizeInPoints, style);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a font object with the specified properties. If an error occurs while
		/// making the font (e.g. because the font doesn't support a particular style) a
		/// fallback scheme is used.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Font MakeFont(string fontName, float size, FontStyle style)
		{
			// I'm not sure why this try catch block is here, but on TeamCity
			// it fails with exit code 139 if I remove it.
			try
			{
				// search for the font on this system that supports the requested style
				//foreach (var family in FontFamily.Families.Where(f => f.Name == fontName).ToList())
				foreach (var family in FontFamily.Families)
				{
					if (family.Name != fontName) continue;

					if (family.IsStyleAvailable(style))
						return new Font(family, size, style, GraphicsUnit.Point);
					else
						return new Font(family, size, GraphicsUnit.Point);
				}
			}
			catch
			{
				throw;
			}

			// if the requested font was not found, use the default font
			var defaultFont = SystemFonts.IconTitleFont.FontFamily;

			if (defaultFont.IsStyleAvailable(style))
				return new Font(defaultFont, size, style, GraphicsUnit.Point);
			else
				return new Font(defaultFont, size, GraphicsUnit.Point);
		}

		/// --------------------------------------------------------------------------------
		public static bool GetSupportsRegular(string fontName)
		{
			return GetSupportsStyle(fontName, FontStyle.Regular);
		}

		/// --------------------------------------------------------------------------------
		public static bool GetSupportsBold(string fontName)
		{
			return GetSupportsStyle(fontName, FontStyle.Bold);
		}

		/// --------------------------------------------------------------------------------
		public static bool GetSupportsItalic(string fontName)
		{
			return GetSupportsStyle(fontName, FontStyle.Italic);
		}

		/// --------------------------------------------------------------------------------
		public static bool GetSupportsStyle(string fontName, FontStyle style)
		{
			var family = FontFamily.Families.SingleOrDefault(f => f.Name == fontName);
			return (family != null && family.IsStyleAvailable(style));
		}

		/// --------------------------------------------------------------------------------
		/// <summary>
		/// Compares the font face name, size and style of two fonts.
		/// </summary>
		/// --------------------------------------------------------------------------------
		public static bool AreFontsSame(Font x, Font y)
		{
			if (x == null || y == null)
				return false;

			return (x.Name == y.Name && x.Size.Equals(y.Size) && x.Style == y.Style);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the desired font for most UI elements.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Font UIFont { get; set; }
	}
}
