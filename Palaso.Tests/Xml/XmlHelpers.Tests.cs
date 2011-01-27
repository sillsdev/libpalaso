using System;
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;


namespace Palaso.Tests
{
	[TestFixture]
	public class XmlHelpers
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
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<world><thailand/></world>");
			XmlNode node= Palaso.XmlHelpers.GetOrCreateElement(doc, "world/thailand", "chiangmai", null, _nameSpaceManager);
			Assert.IsNotNull(node);
			Assert.AreEqual("chiangmai", node.Name);
			AssertThatXmlIn.Dom(doc).HasAtLeastOneMatchForXpath("world/thailand/chiangmai");
		}

		[Test]
		public void GetOrCreateElement_Gets()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<world><thailand/></world>");
			XmlNode node = Palaso.XmlHelpers.GetOrCreateElement(doc, "world", "thailand", null, _nameSpaceManager);
			Assert.AreEqual("thailand",node.Name);
		}

		[Test]
		public void GetUsingNameSpace()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<ldml><special><foo>one</foo></special><special><foo xmlns='http://palaso.org'>two</foo></special><special><foo>three</foo></special></ldml>");
			XmlNode node = Palaso.XmlHelpers.GetOrCreateElement(doc, "ldml/special[palaso:foo]", "foo", "palaso", _nameSpaceManager);
			Assert.AreEqual("two", node.InnerText);
		}


		[Test]
		public void RemoveUsingNameSpace()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<ldml><special><foo>one</foo></special><special><foo xmlns='http://palaso.org'>two</foo></special><special><foo>three</foo></special></ldml>");
			Palaso.XmlHelpers.RemoveElement(doc, "ldml/special/palaso:foo",_nameSpaceManager);
			Assert.IsNull(doc.SelectSingleNode("ldml/special/palaso:foo", _nameSpaceManager));
		}

		[Test]
		public void AddOrUpdateAttribute_Adds()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<world><thailand><chiangmai/></thailand></world>");
			XmlNode node = doc.SelectSingleNode("world/thailand/chiangmai");
			Palaso.XmlHelpers.AddOrUpdateAttribute(node, "temp", "24");
			AssertThatXmlIn.Dom(doc).HasAtLeastOneMatchForXpath("world/thailand/chiangmai[@temp='24']");
		}

		[Test]
		public void AddOrUpdateAttribute_UpdatesExisting()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<world><thailand><chiangmai temp='12'/></thailand></world>");
			XmlNode node = doc.SelectSingleNode("world/thailand/chiangmai");
			Palaso.XmlHelpers.AddOrUpdateAttribute(node, "temp", "12");
			AssertThatXmlIn.Dom(doc).HasNoMatchForXpath("world/thailand/chiangmai[@temp='24']");
			AssertThatXmlIn.Dom(doc).HasAtLeastOneMatchForXpath("world/thailand/chiangmai[@temp='12']");
		}
	}

}