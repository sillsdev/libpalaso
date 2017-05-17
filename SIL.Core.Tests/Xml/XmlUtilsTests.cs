using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
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

		[Test]
		public void GetMandatoryIntegerAttributeValue_OfElement_ReturnsInteger()
		{
			var element = new XElement("element");
			var attr = new XAttribute("intAttr", 1);
			element.Add(attr);
			Assert.AreEqual(1, XmlUtils.GetMandatoryIntegerAttributeValue(element, "intAttr"));
		}

		[Test]
		public void GetMandatoryIntegerAttributeValue_OfElement_Throws_If_Attr_Not_Present()
		{
			Assert.Throws<ApplicationException>(() => XmlUtils.GetMandatoryIntegerAttributeValue(new XElement("element"), "intAttr"));
		}

		[Test]
		public void GetMandatoryIntegerAttributeValue_OfElement_Throws_If_Not_Integer()
		{
			Assert.Throws<FormatException>(() => XmlUtils.GetMandatoryIntegerAttributeValue(new XElement("element", new XAttribute("intAttr", "Not_Int")), "intAttr"));
		}

		[Test]
		public void GetOptionalIntegerValue_OfElement_Returns_Default_Value()
		{
			Assert. AreEqual(1, XmlUtils.GetOptionalIntegerValue(new XElement("element"), "intAttr", 1));
		}

		[Test]
		public void GetOptionalIntegerValue_OfElement_Ignores_Default_Value_When_Attr_Present()
		{
			Assert.AreEqual(2, XmlUtils.GetOptionalIntegerValue(new XElement("element", new XAttribute("intAttr", "2")), "intAttr", 1));
		}

		[Test]
		public void GetOptionalIntegerValue_OfElement_Throws_If_Not_Integer()
		{
			Assert.Throws<FormatException>(() => XmlUtils.GetOptionalIntegerValue(new XElement("element", new XAttribute("intAttr", "Not_Int")), "intAttr", 1));
		}

		[TestCase(null, "Null should return false")]
		[TestCase("", "Empty string should return false")]
		[TestCase(" ", "Whitespace only string should return false")]
		[TestCase("FALSE", "'FALSE' should return false")]
		[TestCase("False", "'False' should return false")]
		[TestCase("false", "'false' should return false")]
		[TestCase("NO", "'NO' should return false")]
		[TestCase("No", "'No' should return false")]
		[TestCase("no", "'no' should return false")]
		public void GetBooleanAttributeValue_Returns_False(string input, string errorMessage)
		{
			// Test overload that takes just the attribute's value
			Assert.IsFalse(XmlUtils.GetBooleanAttributeValue(input), errorMessage);

			// Test overload that takes the element and attr name.
			var element = new XElement("element");
			if (input != null)
			{
				var attr = new XAttribute("boolAttr", input);
				element.Add(attr);
			}
			Assert.IsFalse(XmlUtils.GetBooleanAttributeValue(element, "boolAttr"), errorMessage);
		}

		[TestCase("TRUE", "'TRUE' should return true")]
		[TestCase("True", "'True' should return true")]
		[TestCase("true", "'true' should return true")]
		[TestCase("YES", "'YES' should return true")]
		[TestCase("Yes", "'Yes' should return true")]
		[TestCase("yes", "'yes' should return true")]
		public void GetBooleanAttributeValue_Returns_True(string input, string errorMessage)
		{
			// Test overload that takes just the attribute's value
			Assert.IsTrue(XmlUtils.GetBooleanAttributeValue(input), errorMessage);

			// Test overload that takes the element and attr name.
			var element = new XElement("element", new XAttribute("boolAttr", input));
			Assert.IsTrue(XmlUtils.GetBooleanAttributeValue(element, "boolAttr"), errorMessage);
		}

		[TestCase(false, true /* not used */, true, "No attr should return default of true")]
		[TestCase(false, true /* not used */, false, "No attr should return default of false")]
		[TestCase(true, true, false, "Has attr should not return default of false")]
		[TestCase(true, false, true, "Has attr should not return default of true")]
		public void GetOptionalBooleanAttributeValue(bool hasAttr, bool actualValue, bool defaultValue, string errorMessage)
		{
			bool expectedValue;
			var element = new XElement("element");

			if (hasAttr)
			{
				element.Add(new XAttribute("boolAttr", actualValue));
				expectedValue = actualValue;
			}
			else
			{
				// Don't add attr, but expect the call to use the default value.
				expectedValue = defaultValue;
			}
			Assert.AreEqual(expectedValue, XmlUtils.GetOptionalBooleanAttributeValue(element, "boolAttr", defaultValue), errorMessage);
		}

		[TestCase(false, "NotUsedValue" /* not used */, "DefaultValue", "No attr should return default of 'DefaultValue'")]
		[TestCase(true, "RealValue", "DefaultValue", "Has attr should not return default of 'DefaultValue'")]
		public void GetOptionalAttributeValue_Using_Default(bool hasAttr, string actualValue, string defaultValue, string errorMessage)
		{
			string expectedValue;
			var element = new XElement("element");

			if (hasAttr)
			{
				element.Add(new XAttribute("attr", actualValue));
				expectedValue = actualValue;
			}
			else
			{
				// Don't add attr, but expect the call to use the default value.
				expectedValue = defaultValue;
			}
			Assert.AreEqual(expectedValue, XmlUtils.GetOptionalAttributeValue(element, "attr", expectedValue), errorMessage);
		}

		[TestCase(false, "NotUsedValue" /* not used */, "No attr should return default of 'DefaultValue'")]
		[TestCase(true, "RealValue", "Has attr should not return default of 'DefaultValue'")]
		public void GetOptionalAttributeValue_Without_Using_Default(bool hasAttr, string actualValue, string errorMessage)
		{
			string expectedValue;
			var element = new XElement("element");

			if (hasAttr)
			{
				element.Add(new XAttribute("attr", actualValue));
				expectedValue = actualValue;
			}
			else
			{
				// Don't add attr, but expect the call to use the default value.
				expectedValue = null;
			}
			Assert.AreEqual(expectedValue, XmlUtils.GetOptionalAttributeValue(element, "attr"), errorMessage);
		}

		[TestCase("-1,1,2", "'-1,1,2' should return three integers in the array", "Expected order in array is: '-1,1,2'")]
		[TestCase("1,2", "'1,2' should return two integers in the array", "Expected order in array is: '1,2'")]
		[TestCase("2,1", "'2,1' should return two integers in the array", "Expected order in array is: '2,1'")]
		public void GetMandatoryIntegerListAttributeValue_Has_Correct_List(string sourceAttrData, string errorMessageCount, string errorMessageOrder)
		{
			var element = new XElement("element", new XAttribute("intArray", sourceAttrData));
			var resultArray = XmlUtils.GetMandatoryIntegerListAttributeValue(element, "intArray");
			var sourceArray = sourceAttrData.Split(',');
			Assert.AreEqual(sourceArray.Length, resultArray.Length, errorMessageCount);

			for (var idx = 0; idx < sourceArray.Length; idx++)
			{
				Assert.AreEqual(int.Parse(sourceArray[idx], CultureInfo.InvariantCulture), resultArray[idx], errorMessageOrder);
			}
		}

		[TestCase("1,2", "'1,2' should return two integers in the array", "Expected order in array is: '1,2'")]
		[TestCase("2,1", "'2,1' should return two integers in the array", "Expected order in array is: '2,1'")]
		public void GetMandatoryUIntegerListAttributeValue_Has_Correct_List(string sourceAttrData, string errorMessageCount, string errorMessageOrder)
		{
			var element = new XElement("element", new XAttribute("intArray", sourceAttrData));
			var resultArray = XmlUtils.GetMandatoryUIntegerListAttributeValue(element, "intArray");
			var sourceArray = sourceAttrData.Split(',');
			Assert.AreEqual(sourceArray.Length, resultArray.Length, errorMessageCount);

			for (var idx = 0; idx < sourceArray.Length; idx++)
			{
				Assert.AreEqual(uint.Parse(sourceArray[idx], CultureInfo.InvariantCulture), resultArray[idx], errorMessageOrder);
			}
		}

		[Test]
		public void MakeStringFromList_For_int()
		{
			Assert.AreEqual("1,2,3", XmlUtils.MakeStringFromList(new List<int>() { 1, 2, 3 }));
		}

		[Test]
		public void MakeStringFromList_For_uint()
		{
			Assert.AreEqual("1,2,3", XmlUtils.MakeStringFromList(new List<uint>() { 1, 2, 3 }));
		}

		[Test]
		public void DecodeXmlAttributeTest()
		{
			string sFixed = XmlUtils.DecodeXmlAttribute("abc&amp;def&lt;ghi&gt;jkl&quot;mno&apos;pqr&amp;stu");
			Assert.AreEqual("abc&def<ghi>jkl\"mno'pqr&stu", sFixed, "First Test of DecodeXmlAttribute");

			sFixed = XmlUtils.DecodeXmlAttribute("abc&amp;def&#xD;&#xA;ghi&#x1F;jkl&#x7F;&#x9F;mno");
			Assert.AreEqual("abc&def\r\nghi\u001Fjkl\u007F\u009Fmno", sFixed, "Second Test of DecodeXmlAttribute");
		}

		[Test]
		public void FindElement_For_XElements()
		{
			var grandfather = new XElement("grandfather");
			var father = new XElement("father");
			grandfather.Add(father);
			var me = new XElement("me");
			father.Add(me);

			Assert.AreSame(grandfather, XmlUtils.FindElement(grandfather, "grandfather"));
			Assert.AreSame(father, XmlUtils.FindElement(grandfather, "father"));
			Assert.AreSame(me, XmlUtils.FindElement(grandfather, "me"));
		}

		[Test]
		public void FindNode_For_XmlNodes()
		{
			var dom = new XmlDocument();
			var root = dom.CreateElement("familyTree");
			dom.AppendChild(root);
			var grandfather = dom.CreateElement("grandfather");
			root.AppendChild(grandfather);
			var father = dom.CreateElement("father");
			grandfather.AppendChild(father);
			var me = dom.CreateElement("me");
			father.AppendChild(me);

			Assert.AreSame(grandfather, XmlUtils.FindNode(grandfather, "grandfather"));
			Assert.AreSame(father, XmlUtils.FindNode(grandfather, "father"));
			Assert.AreSame(me, XmlUtils.FindNode(grandfather, "me"));
		}

		[Test]
		public void FindIndexOfMatchingNode()
		{
			var threeStooges = new List<XElement>
			{
				new XElement("Larry"),
				new XElement("Moe"),
				new XElement("Curly")
			};
			var target = new XElement("Moe");
			// Moe is in the list.
			Assert.AreEqual(1, XmlUtils.FindIndexOfMatchingNode(threeStooges, target));

			// This one is not in the list.
			Assert.AreEqual(-1, XmlUtils.FindIndexOfMatchingNode(threeStooges, new XElement("Flopsie")));
		}

		[Test]
		public void SetAttribute_AddNewAttr()
		{
			var element = new XElement("element");
			XmlUtils.SetAttribute(element, "newAttr", "newAttrValue");
			Assert.IsNotNull(element.Attribute("newAttr"));
			Assert.AreEqual("newAttrValue", element.Attribute("newAttr").Value);
		}

		[Test]
		public void SetAttribute_ChangeAttrValue()
		{
			var element = new XElement("element", new XAttribute("oldAttr", "oldAttrValue"));
			XmlUtils.SetAttribute(element, "oldAttr", "newAttrValue");
			Assert.AreEqual("newAttrValue", element.Attribute("oldAttr").Value);
		}

		[Test]
		public void SetAttribute_SameAttrValue()
		{
			var element = new XElement("element", new XAttribute("oldAttr", "oldAttrValue"));
			XmlUtils.SetAttribute(element, "oldAttr", "oldAttrValue");
			Assert.AreEqual("oldAttrValue", element.Attribute("oldAttr").Value);
		}
	}
}