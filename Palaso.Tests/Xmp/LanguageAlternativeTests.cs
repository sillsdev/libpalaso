using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Xml;
using Palaso.Xmp;

namespace Palaso.Tests.Xml
{
	[TestFixture]
	public class LanguageAlternativeTests

	{
		[Test]
		public void Get_XmlAttributeValue_xmlLang_Default()
		{
			var languageAlternative = new LanguageAlternative();

			const string expected = "x-default";

			Assert.AreEqual(languageAlternative.XmlAttributeValue_xmlLang, expected);
		}

		[Test]
		public void Get_XmlAttributeValue_xmlLang_English()
		{
			var languageAlternative = new LanguageAlternative();

			languageAlternative.Iso = "en";
			languageAlternative.Region = "us";

			const string expected = "en-us";

			Assert.AreEqual(languageAlternative.XmlAttributeValue_xmlLang, expected);
		}
	}
}
