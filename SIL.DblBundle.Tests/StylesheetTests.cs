using NUnit.Framework;
using SIL.Xml;

namespace SIL.DblBundle.Tests
{
	[TestFixture]
	class StylesheetTests
	{
		private Stylesheet m_stylesheet;

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

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			m_stylesheet = XmlSerializationHelper.DeserializeFromString<Stylesheet>(kXml);
		}

		[Test]
		public void Deserialize_FontFamilyAndFontSize()
		{
			Assert.AreEqual("Charis SIL", m_stylesheet.FontFamily);
			Assert.AreEqual(12, m_stylesheet.FontSizeInPoints);
		}

		[Test]
		public void Deserialize_Styles()
		{
			Assert.AreEqual(2, m_stylesheet.Styles.Count);
			Assert.AreEqual("h", m_stylesheet.Styles[0].Id);
			Assert.AreEqual("p", m_stylesheet.Styles[1].Id);
		}

		[Test]
		public void GetStyle()
		{
			IStyle headerStyle = m_stylesheet.GetStyle("h");
			Assert.NotNull(headerStyle);
			Assert.AreEqual("h", headerStyle.Id);
		}
	}
}
