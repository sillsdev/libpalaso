using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using SIL.Base32;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class AddSortKeysToXmlTests
	{
		private XPathNavigator _document;
		private string _xpathSortKeySource;
		private string _xpathElementToPutSortKeyAttributeIn;
		private string _attribute;
		private string _prefix;
		private string _uri;

		private AddSortKeysToXml.SortKeyGenerator _sortKeyGenerator;

		[SetUp]
		public void Setup()
		{
			XmlDocument document = new XmlDocument();
			document.LoadXml(
@"<test>
<node>z</node>
<node>a</node>
<node>b</node>
<node>q</node>
</test>");

			this._document = document.CreateNavigator();
			this._xpathSortKeySource = "//node";
			this._xpathElementToPutSortKeyAttributeIn = "self::*";
			this._attribute = "sort-key";
			this._prefix = "sil";
			this._uri = "http://sil.org/sort-key";

			_sortKeyGenerator = CultureInfo.InvariantCulture.CompareInfo.GetSortKey;
		}

		[Test]
		public void VerifySortKeyGenerator()
		{
			var sortKey = _sortKeyGenerator("z");
			var sortKeyBase32 = Base32Convert.ToBase32HexString(sortKey.KeyData, Base32FormattingOptions.None);
			//this may be over specified, but all the tests expect this to pass
			Assert.AreEqual("1QKG20810400", sortKeyBase32);
			Assert.AreEqual("z", sortKey.OriginalString);
		}

		[Test]
		public void AddSortKeys()
		{
			AddSortKeysToXml.AddSortKeys(_document,
						_xpathSortKeySource,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						_attribute);

			string expectedXml = "<test>\r\n  <node sort-key=\"1QKG20810400\">z</node>\r\n  <node sort-key=\"1O1020810400\">a</node>\r\n  <node sort-key=\"1O4G20810400\">b</node>\r\n  <node sort-key=\"1Q4G20810400\">q</node>\r\n</test>";
			Assert.AreEqual(NormalizeLineBreaks(expectedXml),
							NormalizeLineBreaks(_document.OuterXml));
		}

		[Test]
		public void AddSortKeys_EmptyDocument()
		{
			XmlDocument document = new XmlDocument();

			XPathNavigator navigator = document.CreateNavigator();
			AddSortKeysToXml.AddSortKeys(navigator,
						_xpathSortKeySource,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						_attribute);

			Assert.AreEqual(string.Empty, navigator.OuterXml);
		}

		[Test]
		public void AddSortKeys_DocumentNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => AddSortKeysToXml.AddSortKeys(null,
						_xpathSortKeySource,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						_attribute));

		}

		[Test]
		public void AddSortKeys_XPathSourceNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => AddSortKeysToXml.AddSortKeys(_document,
						null,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						_attribute));
		}

		[Test]
		public void AddSortKeys_XPathSourceEmpty_Throws()
		{
			Assert.Throws<XPathException>(
				() => AddSortKeysToXml.AddSortKeys(_document,
						string.Empty,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						_attribute));

		}

		[Test]
		public void AddSortKeys_XPathDestinationNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => AddSortKeysToXml.AddSortKeys(_document,
						_xpathSortKeySource,
						_sortKeyGenerator,
						null,
						_attribute));
		}

		[Test]
		public void AddSortKeys_XPathDestinationEmpty_Throws()
		{
			Assert.Throws<XPathException>(
				() => AddSortKeysToXml.AddSortKeys(_document,
						_xpathSortKeySource,
						_sortKeyGenerator,
						string.Empty,
						_attribute));
		}

		[Test]
		public void AddSortKeys_AttributeNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => AddSortKeysToXml.AddSortKeys(_document,
						_xpathSortKeySource,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						null));
		}

		[Test]
		public void AddSortKeys_AttributeEmpty_Throws()
		{
			Assert.Throws<ArgumentException>(
				() => AddSortKeysToXml.AddSortKeys(_document,
						_xpathSortKeySource,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						string.Empty));

		}


		[Test]
		public void AddSortKeys_PrefixNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => AddSortKeysToXml.AddSortKeys(_document,
						_xpathSortKeySource,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						null,
						_attribute,
						_uri));
		}

		[Test]
		public void AddSortKeys_PrefixEmpty_Throws()
		{
			Assert.Throws<ArgumentException>(
				() => AddSortKeysToXml.AddSortKeys(_document,
						_xpathSortKeySource,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						string.Empty,
						_attribute,
						_uri));
		}

		[Test]
		public void AddSortKeys_PrefixInvalidCharacter_Throws()
		{
			Assert.Throws<ArgumentException>(
				() => AddSortKeysToXml.AddSortKeys(_document,
						_xpathSortKeySource,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						"1",
						_attribute,
						_uri));
		}

		[Test]
		public void AddSortKeys_UriNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => AddSortKeysToXml.AddSortKeys(_document,
						_xpathSortKeySource,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						_prefix,
						_attribute,
						null));
		}

		[Test]
		public void AddSortKeys_UriEmpty_Throws()
		{
			Assert.Throws<ArgumentException>(
				() => AddSortKeysToXml.AddSortKeys(_document,
						_xpathSortKeySource,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						_prefix,
						_attribute,
						string.Empty));
		}

		[Test]
		public void AddSortKeysWithNamespace()
		{
			AddSortKeysToXml.AddSortKeys(_document,
						_xpathSortKeySource,
						_sortKeyGenerator,
						_xpathElementToPutSortKeyAttributeIn,
						_prefix,
						_attribute,
						_uri);

			string expectedXml = "<test>\r\n  <node sil:sort-key=\"1QKG20810400\" xmlns:sil=\""+ _uri +
			"\">z</node>\r\n  <node sil:sort-key=\"1O1020810400\" xmlns:sil=\""+ _uri +
			"\">a</node>\r\n  <node sil:sort-key=\"1O4G20810400\" xmlns:sil=\""+ _uri +
			"\">b</node>\r\n  <node sil:sort-key=\"1Q4G20810400\" xmlns:sil=\""+ _uri +
			"\">q</node>\r\n</test>";
			Assert.AreEqual(NormalizeLineBreaks(expectedXml),
							NormalizeLineBreaks(_document.OuterXml));
		}

	  [Test]
	  public void AddSortKeys_AlreadyHasSortKeys()
	  {
		AddSortKeysToXml.AddSortKeys(_document,
				   _xpathSortKeySource,
				   _sortKeyGenerator,
				   _xpathElementToPutSortKeyAttributeIn,
				   _attribute);

		AddSortKeysToXml.AddSortKeys(_document,
				   _xpathSortKeySource,
				   _sortKeyGenerator,
				   _xpathElementToPutSortKeyAttributeIn,
				   _attribute);

		string expectedXml = "<test>\r\n  <node sort-key=\"1QKG20810400\">z</node>\r\n  <node sort-key=\"1O1020810400\">a</node>\r\n  <node sort-key=\"1O4G20810400\">b</node>\r\n  <node sort-key=\"1Q4G20810400\">q</node>\r\n</test>";
		Assert.AreEqual(NormalizeLineBreaks(expectedXml),
						NormalizeLineBreaks(_document.OuterXml));
	  }

	  private string NormalizeLineBreaks(string s)
	  {
		  return s.Replace("\r\n", "\n");
	  }
	}

}