using System;
using System.Text;
using System.Xml.Linq;
using NUnit.Framework;
using Palaso.Extensions;

namespace Palaso.Tests.Extensions
{
	[TestFixture]
	public class XElementExtensionTests
	{
		private static XNamespace Sil = "urn://www.sil.org/ldml/0.1";

		private static string contents =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity version='3.14' sil:date='Jan 1, 2015'>Identity description</identity>
	<sil:special version='10'>Special description</sil:special>
</ldml>".Replace("'", "\"");

		[Test]
		public void GetNullAttribute()
		{

			XElement root = XElement.Parse(contents);
			string attribute = root.GetAttributeValue("date");
			Assert.That(attribute.Equals(string.Empty));
		}

		[Test]
		public void GetAttribute()
		{
			XElement root = XElement.Parse(contents);
			XElement identityElem = root.Element("identity");
			string attribute = identityElem.GetAttributeValue("version");
			Assert.That(attribute.Equals("3.14"));
		}

		[Test]
		public void GetNamespaceAttribute()
		{
			XElement root = XElement.Parse(contents);
			XElement identityElem = root.Element("identity");
			string attribute = identityElem.GetAttributeValue(Sil + "date");
			Assert.That(attribute.Equals("Jan 1, 2015"));
		}

		[Test]
		public void GetChildNullAttribute()
		{
			XElement root = XElement.Parse(contents);
			string attribute = root.GetAttributeValue("identity", "date");
			Assert.That(attribute.Equals(string.Empty));
		}

		[Test]
		public void GetChildAttribute()
		{
			XElement root = XElement.Parse(contents);
			string attribute = root.GetAttributeValue("identity", "version");
			Assert.That(attribute.Equals("3.14"));
		}

		[Test]
		public void GetChildNamespaceNullAttribute()
		{
			XElement root = XElement.Parse(contents);
			string attribute = root.GetAttributeValue(Sil + "special", "date");
			Assert.That(attribute.Equals(string.Empty));
			
		}

		[Test]
		public void GetChildNamespaceAttribute()
		{
			XElement root = XElement.Parse(contents);
			string attribute = root.GetAttributeValue(Sil + "special", "version");
			Assert.That(attribute.Equals("10"));
		}

	}
}
