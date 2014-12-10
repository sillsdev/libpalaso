using System.IO;
using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;
using SIL.WritingSystems;
using Palaso.Xml;

namespace Palaso.Tests.Xml
{
	[TestFixture]
	public class XmlHelpersTests
	{
		private XmlNamespaceManager _nameSpaceManager;

		[SetUp]
		public void Setup()
		{
			_nameSpaceManager = new XmlNamespaceManager(new NameTable());
			_nameSpaceManager.AddNamespace("palaso", "http://palaso.org");

		}

		[TearDown]
		public void TearDown()
		{

		}

		[Test]
		public void GetOrCreateElement_Creates()
		{
			var doc = new XmlDocument();
			doc.LoadXml("<world><thailand/></world>");
			XmlNode node= XmlHelpers.GetOrCreateElement(doc, "world/thailand", "chiangmai", null, _nameSpaceManager);
			Assert.IsNotNull(node);
			Assert.AreEqual("chiangmai", node.Name);
			AssertThatXmlIn.Dom(doc).HasAtLeastOneMatchForXpath("world/thailand/chiangmai");
		}

		[Test]
		public void GetOrCreateElement_Gets()
		{
			var doc = new XmlDocument();
			doc.LoadXml("<world><thailand/></world>");
			XmlNode node = XmlHelpers.GetOrCreateElement(doc, "world", "thailand", null, _nameSpaceManager);
			Assert.AreEqual("thailand",node.Name);
		}

		[Test]
		public void GetUsingNameSpace()
		{
			var doc = new XmlDocument();
			doc.LoadXml("<ldml><special><foo>one</foo></special><special><foo xmlns='http://palaso.org'>two</foo></special><special><foo>three</foo></special></ldml>");
			XmlNode node = XmlHelpers.GetOrCreateElement(doc, "ldml/special[palaso:foo]", "foo", "palaso", _nameSpaceManager);
			Assert.AreEqual("two", node.InnerText);
		}


		[Test]
		public void RemoveUsingNameSpace()
		{
			var doc = new XmlDocument();
			doc.LoadXml("<ldml><special><foo>one</foo></special><special><foo xmlns='http://palaso.org'>two</foo></special><special><foo>three</foo></special></ldml>");
			XmlHelpers.RemoveElement(doc, "ldml/special/palaso:foo",_nameSpaceManager);
			Assert.IsNull(doc.SelectSingleNode("ldml/special/palaso:foo", _nameSpaceManager));
		}

		[Test]
		public void AddOrUpdateAttribute_Adds()
		{
			var doc = new XmlDocument();
			doc.LoadXml("<world><thailand><chiangmai/></thailand></world>");
			XmlNode node = doc.SelectSingleNode("world/thailand/chiangmai");
			XmlHelpers.AddOrUpdateAttribute(node, "temp", "24");
			AssertThatXmlIn.Dom(doc).HasAtLeastOneMatchForXpath("world/thailand/chiangmai[@temp='24']");
		}

		[Test]
		public void AddOrUpdateAttribute_UpdatesExisting()
		{
			var doc = new XmlDocument();
			doc.LoadXml("<world><thailand><chiangmai temp='12'/></thailand></world>");
			XmlNode node = doc.SelectSingleNode("world/thailand/chiangmai");
			XmlHelpers.AddOrUpdateAttribute(node, "temp", "12");
			AssertThatXmlIn.Dom(doc).HasNoMatchForXpath("world/thailand/chiangmai[@temp='24']");
			AssertThatXmlIn.Dom(doc).HasAtLeastOneMatchForXpath("world/thailand/chiangmai[@temp='12']");
		}

		[Test]
		public void FindNextElementInSequence_FindsElement()
		{
			var reader = XmlReader.Create(new StringReader("<ldml><alternate><attributes><character/><context/></attributes></alternate></ldml>"));
			for (int i = 0; i <= 3; i++)
				reader.Read(); // seek to inside <attributes> element
			Assert.IsTrue(XmlHelpers.FindNextElementInSequence(reader, "character", LdmlNodeComparer.CompareElementNames),
				"Failed to find <character> element");
			Assert.AreEqual("character", reader.Name, "Failed to find <character> element");
			Assert.IsTrue(XmlHelpers.FindNextElementInSequence(reader, "context", LdmlNodeComparer.CompareElementNames),
				"Failed to find <context> element");
			Assert.AreEqual("context", reader.Name, "Failed to find <context> element");
		}

		[Test]
		public void FindNextElementInSequence_FalseIfNotFound()
		{
			var reader = XmlReader.Create(new StringReader(
				"<ldml><alternate><attributes><character/><context/></attributes><attributes><special/></attributes></alternate></ldml>"));
			for (int i = 0; i <= 3; i++)
				reader.Read(); // seek to inside first <attributes> element
			Assert.IsFalse(XmlHelpers.FindNextElementInSequence(reader, "comment", LdmlNodeComparer.CompareElementNames),
				"There is no <comment> element");
			Assert.AreEqual("context", reader.Name, "Should have stopped searching after we passed the expected location. Has order changed?");
			Assert.IsFalse(XmlHelpers.FindNextElementInSequence(reader, "special", LdmlNodeComparer.CompareElementNames),
				"Should have stopped looking when when we hit the first </attributes> end tag");
			Assert.AreEqual(XmlNodeType.EndElement, reader.NodeType, "Should have stopped looking when when we hit the first end tag");
			Assert.AreEqual("attributes", reader.Name, "Should have stopped looking when when we hit the first </attributes> end tag");
		}

		[Test]
		public void ReadEndElement()
		{
			var reader = XmlReader.Create(new StringReader("<ldml><alternate><attributes><character/><context/></attributes></alternate></ldml>"));
			XmlHelpers.ReadEndElement(reader, "ldml");
			Assert.IsTrue(reader.EOF, "Should have had to read to EOF to find </ldml>");
		}
	}
}