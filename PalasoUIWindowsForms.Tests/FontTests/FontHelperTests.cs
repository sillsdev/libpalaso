using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Palaso.UI.WindowsForms;

namespace PalasoUIWindowsForms.Tests.FontTests
{
	[TestFixture]
	public class FontHelperTests
	{

		[SetUp]
		public void SetUp()
		{
			// setup code goes here
		}

		[TearDown]
		public void TearDown()
		{
			// tear down code goes here
		}

		[Test]
		public void MakeFont_FontName_ValidFont()
		{
			Font sourceFont = SystemFonts.DefaultFont;
			Font returnFont = FontHelper.MakeFont(sourceFont.FontFamily.Name);
			Assert.AreEqual(sourceFont.FontFamily.Name, returnFont.FontFamily.Name);
		}

		[Test]
		public void MakeFont_FontNameAndStyle_ValidFont()
		{
			// use Times New Roman
			foreach (var family in FontFamily.Families.Where(family => family.Name == "Times New Roman"))
			{
				Font sourceFont = new Font(family, 10f, FontStyle.Regular);
				Font returnFont = FontHelper.MakeFont(sourceFont, FontStyle.Bold);

				Assert.AreEqual(sourceFont.FontFamily.Name, returnFont.FontFamily.Name);
				Assert.AreEqual(FontStyle.Bold, returnFont.Style & FontStyle.Bold);

				break;
			}

			Assert.IsTrue(true);
		}
	}
}
