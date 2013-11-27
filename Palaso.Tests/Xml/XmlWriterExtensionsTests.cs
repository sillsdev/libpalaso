using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.Xml;

namespace Palaso.Tests.Xml
{
	[TestFixture]
	public class XmlWriterExtensionsTests
	{
		[Test]
		public void WriteNodeShallow_WithMultipleChildren_VisitsThemAll()
		{
			const string xml = @"<?xml version='1.0' encoding='utf-8'?>
<root attrib='1'>
  <child>some child</child>
  <child>some child</child>
</root>";

			var output = new StringBuilder();
			using (var reader = new StringReader(xml))
			using (var writer = new StringWriter(output))
			{
				var xmlReader = XmlReader.Create(reader);
				var xmlWriter = XmlWriter.Create(writer, CanonicalXmlSettings.CreateXmlWriterSettings());

				int count = 0;
				while (xmlReader.Read())
				{
					if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "child")
					{
						count++;
					}
					xmlWriter.WriteNodeShallow(xmlReader);
				}
				Assert.That(count, Is.EqualTo(2));

			}

		}


	}
}
