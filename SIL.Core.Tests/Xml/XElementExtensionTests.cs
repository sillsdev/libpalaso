using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.Xml;

namespace SIL.Tests.Xml
{
	[TestFixture]
	public class XElementExtensionTests
	{
		private static XNamespace Sil = "urn://www.sil.org/ldml/0.1";

		private static readonly string Contents =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity version='5.43' alt='draft'>Alt Identity description</identity>
	<identity version='3.14' date='Jan 1, 2015'>Identity description</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1' status='released'>
		<sil:identity version='33' alt='draft'>Alternate special description</sil:identity>
		<sil:identity version='10'>Special description</sil:identity>
	</special>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1' alt='draft'>
		<sil:identity version='44' alt='draft'>Will never parse this</sil:identity>
	</special>
</ldml>".Replace("'", "\"");

		[Test]
		public void NonAltElement()
		{
			XElement root = XElement.Parse(Contents);
			XElement identityElem = root.NonAltElement("identity");
			Assert.That((string)identityElem.Attribute("version"), Is.EqualTo("3.14"));
			Assert.That((string)identityElem, Is.EqualTo("Identity description"));
		}

		[Test]
		public void NonAltElements()
		{
			XElement root = XElement.Parse(Contents);
			foreach (var elem in root.NonAltElements("identity"))
			{
				Assert.That((string)elem.Attribute("version"), Is.EqualTo("3.14"));
				Assert.That((string)elem, Is.EqualTo("Identity description"));
			}
			foreach (var elem in root.NonAltElements())
			{
				if (elem.Name == "identity")
				{
					Assert.That((string) elem.Attribute("version"), Is.EqualTo("3.14"));
					Assert.That((string)elem, Is.EqualTo("Identity description"));
				}
				else if (elem.Name == "special")
				{
					Assert.That((string) elem.Attribute(XNamespace.Xmlns + "sil"), Is.EqualTo(Sil.ToString()));
					Assert.That((string)elem.Attribute("status"), Is.EqualTo("released"));
				}
			}
		}

		[Test]
		public void GetChildNullAttribute()
		{
			XElement root = XElement.Parse(Contents);
			Assert.That(root.GetAttributeValue("identity", "year"), Is.Null);
		}

		[Test]
		public void GetOrSetElement()
		{
			XElement root = XElement.Parse(Contents);
			// Test child previously doesn't exist and will be created
			Assert.That(root.Element("child"), Is.Null);
			XElement childElem = root.GetOrCreateElement("child");
			Assert.That(root.Element("child"), Is.EqualTo(childElem));
			// Test Sil:child previously doesn't exist and will be created
			Assert.That(root.Element(Sil + "child"), Is.Null);
			XElement silChildElem = root.GetOrCreateElement(Sil + "child");
			Assert.That(root.Element(Sil + "child"), Is.EqualTo(silChildElem));
		}

		[Test]
		public void SetNewChildAttribute()
		{
			XElement root = XElement.Parse(Contents);
			// Assert child element exists, but attribute does not
			Assert.That(root.Elements("identity").Any(), Is.True);
			Assert.That(root.GetAttributeValue("identity", "color"), Is.Null);
			// Set the new attribute
			root.SetAttributeValue("identity", "color", "blue");
			Assert.That(root.GetAttributeValue("identity", "color"), Is.EqualTo("blue"));
		}

		[Test]
		public void SetNewChildNamespaceAttribute()
		{
			XElement root = XElement.Parse(Contents);
			XElement specialElem = root.NonAltElement("special");
			// Assert child element exists, but attribute does not
			Assert.That(specialElem.NonAltElement(Sil + "identity"), Is.Not.EqualTo(null));
			Assert.That(specialElem.GetAttributeValue(Sil + "identity", "color"), Is.Null);
			// Set the attribute
			specialElem.SetAttributeValue(Sil + "identity", "color", "blue");
			Assert.That(specialElem.GetAttributeValue(Sil + "identity", "color").Equals("blue"));
		}

		[Test]
		public void GetAndSetChildAttribute()
		{
			XElement root = XElement.Parse(Contents);
			string attribute = root.GetAttributeValue("identity", "version");
			Assert.That(attribute.Equals("3.14"));
			// Modify and verify version attribute
			root.SetAttributeValue("identity", "version", "4.0");
			Assert.That(root.GetAttributeValue("identity", "version"), Is.EqualTo("4.0"));
			// Set color attribute and remove with null
			root.SetAttributeValue("identity", "color", "blue");
			Assert.That(root.GetAttributeValue("identity", "color"), Is.EqualTo("blue"));
			root.SetAttributeValue("identity", "color", null);
			Assert.That(string.IsNullOrEmpty(root.GetAttributeValue("identity", "color")));
			// Remove the version attribute with empty string
			root.SetAttributeValue("identity", "version", "");
			XAttribute attr = root.NonAltElement("identity").Attribute("version");
			Assert.That(attr, Is.Null);
			// Attempt to remove the version attribute again
			root.SetAttributeValue("identity", "version", null);
			attr = root.NonAltElement("identity").Attribute("version");
			Assert.That(attr, Is.Null);
		}

		[Test]
		public void GetChildNamespaceNullAttribute()
		{
			XElement root = XElement.Parse(Contents);
			string attribute = root.GetAttributeValue(Sil + "special", "date");
			Assert.That(attribute, Is.Null);
			
		}

		[Test]
		public void GetAndSetChildNamespaceAttribute()
		{
			XElement root = XElement.Parse(Contents);
			XElement specialElem = root.NonAltElement("special");
			string attribute = specialElem.GetAttributeValue(Sil + "identity", "version");
			Assert.That(attribute.Equals("10"));
			// Modify and verify version attribute
			specialElem.SetAttributeValue(Sil + "identity", "version", "4.0");
			Assert.That(specialElem.GetAttributeValue(Sil + "identity", "version"), Is.EqualTo("4.0"));
			// Set color attribute and remove with null
			specialElem.SetAttributeValue(Sil + "identity", "color", "blue");
			Assert.That(specialElem.GetAttributeValue(Sil + "identity", "color"), Is.EqualTo("blue"));
			specialElem.SetAttributeValue(Sil + "identity", "color", null);
			Assert.That(string.IsNullOrEmpty(specialElem.GetAttributeValue(Sil + "identity", "color")));
			// Remove the version attribute with empty string
			specialElem.SetAttributeValue(Sil + "identity", "version", "");
			XAttribute attr = specialElem.NonAltElement(Sil + "identity").Attribute("version");
			Assert.That(attr, Is.Null);
			// Attempt to remove the version attribute again
			specialElem.SetAttributeValue(Sil + "identity", "version", null);
			attr = specialElem.NonAltElement(Sil + "identity").Attribute("version");
			Assert.That(attr, Is.Null);
		}

		[Test]
		public void SetOptionalAttribute()
		{
			XElement root = XElement.Parse(Contents);
			XElement identityElem = root.NonAltElement("identity");
			identityElem.SetOptionalAttributeValue("color", "blue");
			Assert.That(root.GetAttributeValue("identity", "color"), Is.EqualTo("blue"));
			// Remove the color attribute with empty string
			identityElem.SetOptionalAttributeValue("color", string.Empty);
			Assert.That(root.GetAttributeValue("identity", "color"), Is.Null);
			// Recreate the color attribute
			identityElem.SetOptionalAttributeValue("color", "blue");
			Assert.That(root.GetAttributeValue("identity", "color"), Is.EqualTo("blue"));
			// Remove the color attribute with null
			identityElem.SetOptionalAttributeValue("color", null);
			Assert.That(root.GetAttributeValue("identity", "color"), Is.Null);
		}

		[TestCase("<stuff />", "stuff")]
		[TestCase("<stuff attr='myAttrValue' />", "myAttrValue")]
		[TestCase("<stuff attr='myAttrValue' ><child attr='myChildAttrValue' /></stuff>", "myChildAttrValue")]
		[TestCase("<stuff attr='myAttrValue' ><!-- Some comment --><child attr='myChildAttrValue' /></stuff>", "<!-- Some comment -->")]
		public void CloneIsSameAsSource(string sourceData, string testData)
		{
			StringAssert.Contains(testData, sourceData);
			var sourceElement = XElement.Parse(sourceData);
			var clone = sourceElement.Clone();
			Assert.AreNotSame(sourceElement, clone);
			Assert.AreEqual(sourceElement.ToString(), clone.ToString());
		}

		/// <summary>
		/// Make sure XElement "GetOuterXml" (aka: 'OuterXml') is the same as the ToString() contents.
		/// </summary>
		[Test]
		public void GetOuterXml_IsSameAsToStringText()
		{
			const string outerXmlData = "<stuff attr='myAttrValue' ><!-- Some comment --><child attr='myChildAttrValue' /></stuff>";
			var element = XElement.Parse(outerXmlData);
			Assert.AreEqual(element.ToString(), element.GetOuterXml());
		}

		/// <summary>
		/// Make sure XElement "GetInnerXml" (aka: 'InnerXml') is the same as combined child node contents,
		/// but without the comment.
		/// </summary>
		[Test]
		public void GetInnerXml_IsCombinedChildElementXml_CommentIsExcluded()
		{
			const string outerTextData = "<stuff attr='myAttrValue' ><!-- Some comment --><child attr=\"myChildAttrValue\" /><child2 attr=\"myChild2AttrValue\" /></stuff>";
			const string expectedResult = "<child attr=\"myChildAttrValue\" /><child2 attr=\"myChild2AttrValue\" />";
			var element = XElement.Parse(outerTextData);
			Assert.AreEqual(expectedResult, element.GetInnerXml());
		}

		/// <summary>
		/// Make sure XElement "GetInnerXml" (aka: 'InnerXml') is the same as combined child node contents.
		/// </summary>
		[Test]
		public void GetInnerXml_IsCombinedChildElementXml()
		{
			const string outerTextData = "<stuff attr='myAttrValue' ><child attr=\"myChildAttrValue\" /><child2 attr=\"myChild2AttrValue\" /></stuff>";
			const string expectedResult = "<child attr=\"myChildAttrValue\" /><child2 attr=\"myChild2AttrValue\" />";
			var element = XElement.Parse(outerTextData);
			Assert.AreEqual(expectedResult, element.GetInnerXml());
		}

		/// <summary>
		/// Make sure XElement "GetInnerText" (aka: 'InnerText') is the same as self's Value concatenated child node Value properties,
		/// but without the comment.
		/// </summary>
		[Test]
		public void GetInnerText_IsConcatenatedElementValues_CommentIsExcluded()
		{
			const string outerTextData = "<stuff attr='myAttrValue' ><!-- Some comment -->My Value<child attr=\"myChildAttrValue\" >Child Value</child><child2 attr=\"myChild2AttrValue\" >Child2 Value</child2></stuff>";
			const string myValueExpected = "My Value";
			const string childExpected = "Child Value";
			const string child2Expected = "Child2 Value";
			var element = XElement.Parse(outerTextData);
			Assert.AreEqual(string.Concat(myValueExpected, childExpected, child2Expected), element.GetInnerText());
		}

		/// <summary>
		/// Make sure XElement "GetInnerText" (aka: 'InnerText') is the same as self's Value concatenated child node Value properties.
		/// </summary>
		[Test]
		public void GetInnerText_IsConcatenatedElementValues()
		{
			const string outerTextData = "<stuff attr='myAttrValue' >My Value<child attr=\"myChildAttrValue\" >Child Value</child><child2 attr=\"myChild2AttrValue\" >Child2 Value</child2></stuff>";
			const string myValueExpected = "My Value";
			const string childExpected = "Child Value";
			const string child2Expected = "Child2 Value";
			var element = XElement.Parse(outerTextData);
			Assert.AreEqual(string.Concat(myValueExpected, childExpected, child2Expected), element.GetInnerText());
		}
	}
}
