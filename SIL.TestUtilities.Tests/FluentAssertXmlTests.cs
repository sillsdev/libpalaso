using NUnit.Framework;

namespace SIL.TestUtilities
{
	[TestFixture]
	class FluentAssertXmlTests
	{
		[Test]
		public void EqualsIgnoreWhitespace()
		{
			const string xml1 = "<root><element attr1=\"1\" attr2=\"2\" /></root>";
			const string xml2 = "<root> <element\r\n\tattr1=\"1\"  attr2=\"2\"  />\n</root>";
			AssertThatXmlIn.String(xml1).EqualsIgnoreWhitespace(xml2);
		}

		[Test]
		public void NotEqualsIgnoreWhitespace()
		{
			const string xml1 = "<element attr1=\"1\" attr2=\"2\" />";
			const string xml2 = "<element attr1=\"1\" attr3=\"3\" />";
			AssertThatXmlIn.String(xml1).NotEqualsIgnoreWhitespace(xml2);
		}

		[Test]
		public void IsNodewiseEqual_IgnoresIrrelevantSpace()
		{
			const string xml1 = "<element attr1=\"1\" attr2=\"2\" />";
			const string xml2 = "<element attr1=\"1\"  attr2=\"2\"  />";
			AssertThatXmlIn.String(xml1).IsNodewiseEqualTo(xml2);
		}

		[Test]
		public void IsNodewiseEqual_IgnoresIrrelevantLineBreak()
		{
			const string xml1 = "<root><element1 />\r\n<element2 /></root>";
			const string xml2 = "<root><element1 />\r<element2 /></root>";
			const string xml3 = "<root><element1 />\n<element2 /></root>";
			AssertThatXmlIn.String(xml1).IsNodewiseEqualTo(xml2);
			AssertThatXmlIn.String(xml1).IsNodewiseEqualTo(xml3);
			AssertThatXmlIn.String(xml2).IsNodewiseEqualTo(xml3);
		}

		[Test]
		public void IsNodewiseEqual_IgnoresIrrelevantLineBreakWithinElement()
		{
			const string xml1 = "<element attr1=\"1\"\r\nattr2=\"2\" />";
			const string xml2 = "<element attr1=\"1\"\nattr2=\"2\" />";
			const string xml3 = "<element attr1=\"1\" attr2=\"2\" />";
			AssertThatXmlIn.String(xml1).IsNodewiseEqualTo(xml2);
			AssertThatXmlIn.String(xml1).IsNodewiseEqualTo(xml3);
			AssertThatXmlIn.String(xml2).IsNodewiseEqualTo(xml3);
		}

		[Test]
		public void IsNodewiseEqual_IgnoresIrrelevantIndentation()
		{
			const string xml1 = "<root>\n  <element1 />\n  <element2 />\n</root>";
			const string xml2 = "<root>\n\t<element1 />\n\t<element2 />\n</root>";
			AssertThatXmlIn.String(xml1).IsNodewiseEqualTo(xml2);
		}

		[Test]
		public void IsNodewiseEqual_DoesNotIgnoresAttributeOrder()
		{
			const string xml1 = "<element attr1=\"1\" attr2=\"2\" />";
			const string xml2 = "<element attr2=\"2\" attr1=\"1\" />";
			AssertThatXmlIn.String(xml1).IsNotNodewiseEqualTo(xml2);
		}

		[Test]
		public void IsNodewiseEqual_DoesNotIgnoreElementOrder()
		{
			const string xml1 = "<root><element1 /><element2 /></root>";
			const string xml2 = "<root><element2 /><element1 /></root>";
			AssertThatXmlIn.String(xml1).IsNotNodewiseEqualTo(xml2);
		}

		[Test]
		public void IsNodewiseEqual_IgnoresEncoding()
		{
			const string xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><root><element1 /><element2 /></root>";
			const string xml2 = "<root><element1 /><element2 /></root>";
			AssertThatXmlIn.String(xml1).IsNodewiseEqualTo(xml2);
		}

		[Test]
		public void IsNodewiseEqual_IgnoresDifferentEncoding()
		{
			const string xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><root><element1 /><element2 /></root>";
			const string xml2 = "<?xml version=\"1.0\" encoding=\"utf-8\"?><root><element1 /><element2 /></root>";
			AssertThatXmlIn.String(xml1).IsNodewiseEqualTo(xml2);
		}
	}
}
