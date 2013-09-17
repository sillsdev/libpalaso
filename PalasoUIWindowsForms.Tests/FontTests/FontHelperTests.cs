using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.UI.WindowsForms;

namespace PalasoUIWindowsForms.Tests.FontTests
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
			// use Times New Roman
			foreach (var family in FontFamily.Families.Where(family => family.Name == "Times New Roman"))
			{
				using (var sourceFont = new Font(family, 10f, FontStyle.Regular))
				{
					using (var returnFont = FontHelper.MakeFont(sourceFont, FontStyle.Bold))
					{
						Assert.AreEqual(sourceFont.FontFamily.Name, returnFont.FontFamily.Name);
						Assert.AreEqual(FontStyle.Bold, returnFont.Style & FontStyle.Bold);
					}
				}
				break;
			}
		}
	}
}
