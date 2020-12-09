using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SIL.DblBundle.Tests.Properties;
using SIL.IO;
using SIL.Xml;

namespace SIL.DblBundle.Tests
{
	[TestFixture]
	class StylesheetTests
	{
		private Stylesheet m_stylesheet;
		private Stylesheet m_stylesheetFromXml;

		private const string kXml = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<stylesheet>
  <property name=""font-family"">Charis SIL</property>
  <property name=""font-size"" unit=""pt"">12</property>
  <style id=""h"" publishable=""true"" versetext=""false"">
    <name>h - File - Header</name>
    <description>Running header text for a book (basic)</description>
    <property name=""text-align"">left</property>
  </style>
  <style id=""p"" publishable=""true"" versetext=""true"">
    <name>p - Paragraph - Normal - First Line Indent</name>
    <description>Paragraph text, with first line indent (basic)</description>
    <property name=""text-indent"" unit=""in"">0.125</property>
    <property name=""text-align"">left</property>
  </style>
</stylesheet>";

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			using (var stylesheetFile = TempFile.WithExtension(".xml"))
			{
				File.WriteAllText(stylesheetFile.Path, Resources.styles_xml);
				Exception e;
				m_stylesheet = Stylesheet.Load(stylesheetFile.Path, out e);
				Assert.IsNull(e);
			}

			m_stylesheetFromXml = XmlSerializationHelper.DeserializeFromString<Stylesheet>(kXml);
		}

		[Test]
		public void Deserialize_FontFamilyAndFontSize()
		{
			Assert.AreEqual("Charis SIL", m_stylesheetFromXml.FontFamily);
			Assert.AreEqual(12, m_stylesheetFromXml.FontSizeInPoints);
		}

		[Test]
		public void Deserialize_Styles()
		{
			Assert.AreEqual(2, m_stylesheetFromXml.Styles.Count);
			Assert.AreEqual("h", m_stylesheetFromXml.Styles[0].Id);
			Assert.AreEqual("p", m_stylesheetFromXml.Styles[1].Id);
		}

		[Test]
		public void Deserialize_GetStyle()
		{
			IStyle headerStyle = m_stylesheetFromXml.GetStyle("h");
			Assert.NotNull(headerStyle);
			Assert.AreEqual("h", headerStyle.Id);
			Assert.IsTrue(headerStyle.IsPublishable);
			Assert.IsFalse(headerStyle.IsVerseText);
		}

		[Test]
		public void Load_FontFamilyAndFontSize()
		{
			Assert.AreEqual("Cambria", m_stylesheet.FontFamily);
			Assert.AreEqual(14, m_stylesheet.FontSizeInPoints);
		}

		[Test]
		public void Load_Styles()
		{
			Assert.AreEqual(10, m_stylesheet.Styles.Count);
			Assert.Contains("h", m_stylesheet.Styles.Select(s => s.Id).ToList());
			Assert.Contains("p", m_stylesheet.Styles.Select(s => s.Id).ToList());
		}

		[Test]
		public void Load_GetStyle()
		{
			IStyle style = m_stylesheetFromXml.GetStyle("p");
			Assert.NotNull(style);
			Assert.AreEqual("p", style.Id);
			Assert.IsTrue(style.IsPublishable);
			Assert.IsTrue(style.IsVerseText);
		}
	}
}
