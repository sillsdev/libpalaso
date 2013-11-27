using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;
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
	}

}