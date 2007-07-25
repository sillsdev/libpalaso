using System;
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;

namespace Palaso.Tests
{
	[TestFixture]
	public class XmlHelpers
	{
		[SetUp]
		public void Setup()
		{

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
			XmlNode node= Palaso.XmlHelpers.GetOrCreateElement(doc, "world/thailand", "chiangmai");
			Assert.IsNotNull(node);
			Assert.AreEqual("chiangmai", node.Name);
			TestUtilities.AssertXPathNotNull(doc, "world/thailand/chiangmai");
		}

		[Test]
		public void GetOrCreateElement_Gets()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<world><thailand/></world>");
			XmlNode node = Palaso.XmlHelpers.GetOrCreateElement(doc, "world", "thailand");
			Assert.AreEqual("thailand",node.Name);
		}

		[Test]
		public void AddOrUpdateAttribute_Adds()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<world><thailand><chiangmai/></thailand></world>");
			XmlNode node = doc.SelectSingleNode("world/thailand/chiangmai");
			Palaso.XmlHelpers.AddOrUpdateAttribute(node, "temp", "24");
			TestUtilities.AssertXPathNotNull(doc, "world/thailand/chiangmai[@temp='24']");
		}

		[Test]
		public void AddOrUpdateAttribute_UpdatesExisting()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<world><thailand><chiangmai temp='12'/></thailand></world>");
			XmlNode node = doc.SelectSingleNode("world/thailand/chiangmai");
			Palaso.XmlHelpers.AddOrUpdateAttribute(node, "temp", "12");
			TestUtilities.AssertXPathIsNull(doc, "world/thailand/chiangmai[@temp='24']");
			TestUtilities.AssertXPathNotNull(doc, "world/thailand/chiangmai[@temp='12']");
		}
	}

}