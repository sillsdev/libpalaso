using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit.Framework;
using SIL.Xml;

namespace SIL.Tests.Xml
{
	[TestFixture]
	public class XmlUtilsTests
	{
		[Test]
		public void GetAttributes_DoubleQuotedAttribute_HasValue()
		{
			const string data = @"<element attr='data' />";
			var tmp = data.Replace("'", "\"");
			var attrValues = XmlUtils.GetAttributes(Encoding.UTF8.GetBytes(tmp), new HashSet<string> { "attr" });
			Assert.IsTrue(attrValues["attr"] == "data");

			attrValues = XmlUtils.GetAttributes(tmp, new HashSet<string> { "attr" });
			Assert.IsTrue(attrValues["attr"] == "data");
		}

		[Test]
		public void GetAttributes_SingleQuotedAttribute_HasValue()
		{
			const string data = @"<element attr='data' />";
			var attrValues = XmlUtils.GetAttributes(Encoding.UTF8.GetBytes(data), new HashSet<string> { "attr" });
			Assert.IsTrue(attrValues["attr"] == "data");

			attrValues = XmlUtils.GetAttributes(data, new HashSet<string> { "attr" });
			Assert.IsTrue(attrValues["attr"] == "data");
		}

		[Test]
		public void GetAttributes_NonExistentAttribute_IsNull()
		{
			const string data = @"<element />";
			var attrValues = XmlUtils.GetAttributes(Encoding.UTF8.GetBytes(data), new HashSet<string> { "attr" });
			Assert.IsNull(attrValues["attr"]);

			attrValues = XmlUtils.GetAttributes(data, new HashSet<string> { "attr" });
			Assert.IsNull(attrValues["attr"]);
		}

		[Test]
		public void SanitizeString_NullString_ReturnsNull()
		{
			Assert.IsNull(XmlUtils.SanitizeString(null));
		}

		[Test]
		public void SanitizeString_EmptyString_ReturnsEmptyString()
		{
			Assert.AreEqual(string.Empty, XmlUtils.SanitizeString(string.Empty));
		}

		[Test]
		public void SanitizeString_ValidString_ReturnsSameString()
		{
			string s = "Abc\u0009 \u000A\u000D\uD7FF\uE000\uFFFD";
			s += char.ConvertFromUtf32(0x10000);
			s += char.ConvertFromUtf32(0x10FFFF);
			Assert.AreEqual(s, XmlUtils.SanitizeString(s));
		}

		[Test]
		public void SanitizeString_CompletelyInvalidString_ReturnsEmptyString()
		{
			string s = "\u0000\u0008\u000B\u000C\u000E\u001F\uD800\uD999\uFFFE\uFFFF";
			int utf32 = 0x20ffff - 0x10000;
			char[] surrogate = new char[2];
			surrogate[0] = (char)((utf32 / 0x400) + '\ud800');
			surrogate[1] = (char)((utf32 % 0x400) + '\udc00');
			s += new string(surrogate);
			Assert.AreEqual(string.Empty, XmlUtils.SanitizeString(s));
		}

		[Test]
		public void SanitizeString_StringWithInvalidChars_ReturnsStringWithInvalidCharsRemoved()
		{
			string s = "A\u0008B\u000B\u000C\u000E\u001F\uD800\uD999\uFFFE\uFFFFC";
			int utf32 = 0x20ffff - 0x10000;
			char[] surrogate = new char[2];
			surrogate[0] = (char)((utf32 / 0x400) + '\ud800');
			surrogate[1] = (char)((utf32 % 0x400) + '\udc00');
			s += new string(surrogate);
			Assert.AreEqual("ABC", XmlUtils.SanitizeString(s));
		}

		/// <summary>
		/// This is a regression test for (FLEx) LT-13962, a problem caused by importing white space introduced by pretty-printing.
		/// </summary>
		[Test]
		public void WriteNode_DoesNotIndentFirstChildOfMixedNode()
		{
			string input = @"<text><span class='bold'>bt</span> more text</text>";
			string expectedOutput =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n"
				+ "<root>\r\n"
				+ "	<text><span\r\n"
				+ "			class=\"bold\">bt</span> more text</text>\r\n"
				+ "</root>";
			var output = new StringBuilder();
			using (var writer = XmlWriter.Create(output, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("root");
				XmlUtils.WriteNode(writer, input, new HashSet<string>());
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
			Assert.That(output.ToString(), Is.EqualTo(expectedOutput));
		}

		/// <summary>
		/// This verifies the special case of (FLEx) LT-13962 where the ONLY child of an element that can contain significant text
		/// is an element.
		/// </summary>
		[Test]
		public void WriteNode_DoesNotIndentChildWhenSuppressed()
		{
			string input = @"<text><span class='bold'>bt</span></text>";
			string expectedOutput =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n"
				+ "<root>\r\n"
				+ "	<text><span\r\n"
				+ "			class=\"bold\">bt</span></text>\r\n"
				+ "</root>";
			var output = new StringBuilder();
			var suppressIndentingChildren = new HashSet<string>();
			suppressIndentingChildren.Add("text");
			using (var writer = XmlWriter.Create(output, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("root");
				XmlUtils.WriteNode(writer, input, suppressIndentingChildren);
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
			Assert.That(output.ToString(), Is.EqualTo(expectedOutput));
		}
		/// <summary>
		/// This verifies that suppressing pretty-printing of children works for spans nested in spans nested in text.
		/// </summary>
		[Test]
		public void WriteNode_DoesNotIndentChildWhenTwoLevelsSuppressed()
		{
			string input = @"<text><span class='bold'><span class='italic'>bit</span>bt</span></text>";
			string expectedOutput =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n"
				+ "<root>\r\n"
				+ "	<text><span\r\n"
				+ "			class=\"bold\"><span\r\n"
				+ "				class=\"italic\">bit</span>bt</span></text>\r\n"
				+ "</root>";
			var output = new StringBuilder();
			var suppressIndentingChildren = new HashSet<string>();
			suppressIndentingChildren.Add("text");
			using (var writer = XmlWriter.Create(output, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("root");
				XmlUtils.WriteNode(writer, input, suppressIndentingChildren);
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
			Assert.That(output.ToString(), Is.EqualTo(expectedOutput));
		}
	}
}