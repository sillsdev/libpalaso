using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.Xml;

namespace Palaso.Tests.Xml
{
	[TestFixture]
	public class XmlUtilsTests
	{
		[Test]
		public void DoubleQuotedAttribute_HasValue()
		{
			const string data = @"<element attr='data' />";
			var tmp = data.Replace("'", "\"");
			var attrValues = XmlUtils.GetAttributes(Encoding.UTF8.GetBytes(tmp), new HashSet<string> { "attr" });
			Assert.IsTrue(attrValues["attr"] == "data");

			attrValues = XmlUtils.GetAttributes(tmp, new HashSet<string> { "attr" });
			Assert.IsTrue(attrValues["attr"] == "data");
		}

		[Test]
		public void SingleQuotedAttribute_HasValue()
		{
			const string data = @"<element attr='data' />";
			var attrValues = XmlUtils.GetAttributes(Encoding.UTF8.GetBytes(data), new HashSet<string> { "attr" });
			Assert.IsTrue(attrValues["attr"] == "data");

			attrValues = XmlUtils.GetAttributes(data, new HashSet<string> { "attr" });
			Assert.IsTrue(attrValues["attr"] == "data");
		}

		[Test]
		public void NonExistantAttribute_IsNull()
		{
			const string data = @"<element />";
			var attrValues = XmlUtils.GetAttributes(Encoding.UTF8.GetBytes(data), new HashSet<string> { "attr" });
			Assert.IsNull(attrValues["attr"]);

			attrValues = XmlUtils.GetAttributes(data, new HashSet<string> { "attr" });
			Assert.IsNull(attrValues["attr"]);
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