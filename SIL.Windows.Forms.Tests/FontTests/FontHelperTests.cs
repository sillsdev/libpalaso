using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;

namespace SIL.Windows.Forms.Tests.FontTests
{
	[TestFixture]
	public class FontHelperTests
	{
		[Test]
		public void MakeFont_FontName_ValidFont()
		{
			using (var sourceFont = SystemFonts.DefaultFont)
			{
				using (var returnFont = FontHelper.MakeFont(sourceFont.FontFamily.Name))
				{
					Assert.AreEqual(sourceFont.FontFamily.Name, returnFont.FontFamily.Name);
				}
			}
		}

		[Test]
		public void MakeFont_FontNameAndStyle_ValidFont()
		{
			// find a bold font
			var family = FontFamily.Families.FirstOrDefault(f => (f.IsStyleAvailable(FontStyle.Regular) && f.IsStyleAvailable(FontStyle.Bold)));
			Assert.IsNotNull(family, "No font was found on this system that supports the Bold style");
			Console.Out.WriteLine("Using the " + family.Name + " font for this test.");

			using (var sourceFont = new Font(family, 10f, FontStyle.Regular))
			{
				using (var returnFont = FontHelper.MakeFont(sourceFont, FontStyle.Bold))
				{
					Assert.AreEqual(sourceFont.FontFamily.Name, returnFont.FontFamily.Name);
					Assert.AreEqual(FontStyle.Bold, returnFont.Style & FontStyle.Bold);
				}
			}
		}

		[Test]
		public void MakeFont_InvalidFontName_ValidFont()
		{
			const string invalidName = "SomeInvalidName";
			using (var returnFont = FontHelper.MakeFont(invalidName))
			{
				Assert.AreNotEqual(invalidName, returnFont.FontFamily.Name);
			}
		}
	}
}
