using System.Xml;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class LdmlNodeComparerTests
	{
		private XmlDocument _dom;

		[SetUp]
		public void SetUp()
		{
			_dom = new XmlDocument();
		}

		[Test]
		public void Compare_BothNull_Zero()
		{
			Assert.AreEqual(0, LdmlNodeComparer.Singleton.Compare(null, null));
		}

		[Test]
		public void Compare_FirstNull_LessThanZero()
		{
			Assert.Less(LdmlNodeComparer.Singleton.Compare(null, _dom.CreateElement("test")), 0);
		}

		[Test]
		public void Compare_SecondNull_GreaterThanZero()
		{
			Assert.Greater(LdmlNodeComparer.Singleton.Compare(_dom.CreateElement("test"), null), 0);
		}

		[Test]
		public void Compare_AttributeAndElement_AttributeFirst()
		{
			Assert.Less(LdmlNodeComparer.Singleton.Compare(_dom.CreateAttribute("attr"), _dom.CreateElement("test")), 0);
		}

		[Test]
		public void Compare_SpecialElement_SpecialIsLast()
		{
			Assert.Greater(LdmlNodeComparer.Singleton.Compare(_dom.CreateElement("special"), _dom.CreateElement("other")), 0);
		}

		[Test]
		public void Compare_ElementsWithSameName_Equal()
		{
			Assert.AreEqual(0, LdmlNodeComparer.Singleton.Compare(_dom.CreateElement("foo"), _dom.CreateElement("foo")));
		}

		[Test]
		public void Compare_AttributesWithSameNameAndValue_Equal()
		{
			Assert.AreEqual(0, LdmlNodeComparer.Singleton.Compare(_dom.CreateAttribute("foo"), _dom.CreateAttribute("foo")));
		}

		[Test]
		public void Compare_TwoDifferentElements_CorrectOrder()
		{
			Assert.Less(LdmlNodeComparer.Singleton.Compare(_dom.CreateElement("ldml"), _dom.CreateElement("collation")), 0);
		}

		[Test]
		public void Compare_AttributesWithDifferentNames_CorrectOrder()
		{
			Assert.Less(LdmlNodeComparer.Singleton.Compare(_dom.CreateAttribute("key"), _dom.CreateAttribute("number")), 0);
		}

		[Test]
		public void Compare_AttributesWithSameNameAndDifferentValues_CorrectOrder()
		{
			// I'm picking element/attribute/value pairs where the alphabetic order is different than
			// the correct order specified in the LDML value order table.
			XmlNode parent = _dom.CreateElement("weekendStart");
			XmlNode x = _dom.CreateAttribute("day");
			x.Value = "fri";
			parent.Attributes.SetNamedItem(x);
			parent = _dom.CreateElement("weekendStart");
			XmlNode y = _dom.CreateAttribute("day");
			y.Value = "wed";
			parent.Attributes.SetNamedItem(y);
			Assert.Greater(LdmlNodeComparer.Singleton.Compare(x, y), 0);
		}

		[Test]
		public void Compare_ElementsDifferOnlyByAttributeValue_CorrectOrder()
		{
			XmlNode x = _dom.CreateElement("foo");
			XmlNode xattr = _dom.CreateAttribute("bar");
			xattr.Value = "1";
			x.Attributes.SetNamedItem(xattr);
			XmlNode y = _dom.CreateElement("foo");
			XmlNode yattr = _dom.CreateAttribute("bar");
			yattr.Value = "2";
			y.Attributes.SetNamedItem(yattr);
			Assert.Less(LdmlNodeComparer.Singleton.Compare(x, y), 0);
		}
	}
}
