using NUnit.Framework;

namespace SIL.TestUtilities
{
	[TestFixture]
	class CanonicalXmlTests
	{
		[Test]
		public void ToCanonicalString_IgnoresIrrelevantSpace()
		{
			const string xml1 = "<element attr1=\"1\" attr2=\"2\" />";
			const string xml2 = "<element attr1=\"1\"  attr2=\"2\"  />";
			Assert.AreEqual(CanonicalXml.ToCanonicalString(xml1), CanonicalXml.ToCanonicalString(xml2));
		}

		[Test]
		public void ToCanonicalString_IgnoresIrrelevantLineBreak()
		{
			const string xml1 = "<root><element1 />\r\n<element2 /></root>";
			const string xml2 = "<root><element1 />\r<element2 /></root>";
			const string xml3 = "<root><element1 />\n<element2 /></root>";
			Assert.AreEqual(CanonicalXml.ToCanonicalString(xml1), CanonicalXml.ToCanonicalString(xml2));
			Assert.AreEqual(CanonicalXml.ToCanonicalString(xml1), CanonicalXml.ToCanonicalString(xml3));
			Assert.AreEqual(CanonicalXml.ToCanonicalString(xml2), CanonicalXml.ToCanonicalString(xml3));
		}

		[Test]
		public void ToCanonicalString_IgnoresIrrelevantLineBreakWithinElement()
		{
			const string xml1 = "<element attr1=\"1\"\r\nattr2=\"2\" />";
			const string xml2 = "<element attr1=\"1\"\nattr2=\"2\" />";
			const string xml3 = "<element attr1=\"1\" attr2=\"2\" />";
			Assert.AreEqual(CanonicalXml.ToCanonicalString(xml1), CanonicalXml.ToCanonicalString(xml2));
			Assert.AreEqual(CanonicalXml.ToCanonicalString(xml1), CanonicalXml.ToCanonicalString(xml3));
			Assert.AreEqual(CanonicalXml.ToCanonicalString(xml2), CanonicalXml.ToCanonicalString(xml3));
		}

		[Test]
		public void ToCanonicalString_IgnoresIrrelevantIndentation()
		{
			const string xml1 = "<root>\n  <element1 />\n  <element2 />\n</root>";
			const string xml2 = "<root>\n\t<element1 />\n\t<element2 />\n</root>";
			Assert.AreEqual(CanonicalXml.ToCanonicalString(xml1), CanonicalXml.ToCanonicalString(xml2));
		}

		[Test, Ignore("Incorrect implementation")]
		// Ideally, Canonical XML should ignore attribute order (i.e., they should have the same order after transformation)
		public void ToCanonicalString_IgnoresAttributeOrder()
		{
			const string xml1 = "<element attr1=\"1\" attr2=\"2\" />";
			const string xml2 = "<element attr2=\"2\" attr1=\"1\" />";
			Assert.AreEqual(CanonicalXml.ToCanonicalString(xml1), CanonicalXml.ToCanonicalString(xml2));
		}

		[Test]
		public void ToCanonicalString_DoesNotIgnoreElementOrder()
		{
			const string xml1 = "<root><element1 /><element2 /></root>";
			const string xml2 = "<root><element2 /><element1 /></root>";
			Assert.AreNotEqual(CanonicalXml.ToCanonicalString(xml1), CanonicalXml.ToCanonicalString(xml2));
		}


		[Test]
		public void ToCanonicalStringFragment_IgnoresIrrelevantSpace()
		{
			const string xml1 = "<element attr1=\"1\" attr2=\"2\" />";
			const string xml2 = "<element attr1=\"1\"  attr2=\"2\"  />";
			Assert.AreEqual(CanonicalXml.ToCanonicalStringFragment(xml1), CanonicalXml.ToCanonicalStringFragment(xml2));
		}

		[Test]
		public void ToCanonicalStringFragment_IgnoresIrrelevantLineBreak()
		{
			const string xml1 = "<root><element1 />\r\n<element2 /></root>";
			const string xml2 = "<root><element1 />\r<element2 /></root>";
			const string xml3 = "<root><element1 />\n<element2 /></root>";
			Assert.AreEqual(CanonicalXml.ToCanonicalStringFragment(xml1), CanonicalXml.ToCanonicalStringFragment(xml2));
			Assert.AreEqual(CanonicalXml.ToCanonicalStringFragment(xml1), CanonicalXml.ToCanonicalStringFragment(xml3));
			Assert.AreEqual(CanonicalXml.ToCanonicalStringFragment(xml2), CanonicalXml.ToCanonicalStringFragment(xml3));
		}

		[Test]
		public void ToCanonicalStringFragment_IgnoresIrrelevantLineBreakWithinElement()
		{
			const string xml1 = "<element attr1=\"1\"\r\nattr2=\"2\" />";
			const string xml2 = "<element attr1=\"1\"\nattr2=\"2\" />";
			const string xml3 = "<element attr1=\"1\" attr2=\"2\" />";
			Assert.AreEqual(CanonicalXml.ToCanonicalStringFragment(xml1), CanonicalXml.ToCanonicalStringFragment(xml2));
			Assert.AreEqual(CanonicalXml.ToCanonicalStringFragment(xml1), CanonicalXml.ToCanonicalStringFragment(xml3));
			Assert.AreEqual(CanonicalXml.ToCanonicalStringFragment(xml2), CanonicalXml.ToCanonicalStringFragment(xml3));
		}

		[Test]
		public void ToCanonicalStringFragment_IgnoresIrrelevantIndentation()
		{
			const string xml1 = "<root>\n  <element1 />\n  <element2 />\n</root>";
			const string xml2 = "<root>\n\t<element1 />\n\t<element2 />\n</root>";
			Assert.AreEqual(CanonicalXml.ToCanonicalStringFragment(xml1), CanonicalXml.ToCanonicalStringFragment(xml2));
		}

		[Test, Ignore("Incorrect implementation")]
		// Ideally, Canonical XML should ignore attribute order (i.e., they should have the same order after transformation)
		public void ToCanonicalStringFragment_IgnoresAttributeOrder()
		{
			const string xml1 = "<element attr1=\"1\" attr2=\"2\" />";
			const string xml2 = "<element attr2=\"2\" attr1=\"1\" />";
			Assert.AreEqual(CanonicalXml.ToCanonicalStringFragment(xml1), CanonicalXml.ToCanonicalStringFragment(xml2));
		}

		[Test]
		public void ToCanonicalStringFragment_DoesNotIgnoreElementOrder()
		{
			const string xml1 = "<root><element1 /><element2 /></root>";
			const string xml2 = "<root><element2 /><element1 /></root>";
			Assert.AreNotEqual(CanonicalXml.ToCanonicalStringFragment(xml1), CanonicalXml.ToCanonicalStringFragment(xml2));
		}

		[Test]
		public void ToCanonicalStringFragment_FragmentsAreEquivalent()
		{
			const string xml1 = "<element1 /><element2 />";
			const string xml2 = "<element1/><element2/>";
			Assert.AreEqual(CanonicalXml.ToCanonicalStringFragment(xml1), CanonicalXml.ToCanonicalStringFragment(xml2));
		}
	}
}
