using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class SldrTests
	{
		[SetUp]
		public void SetUp()
		{

		}

		[Test, Ignore("Run by hand")]
		public void Get_EmptyFileName_Throws()
		{
			string filename = string.Empty;
			const string bcp47Tag = "en";

			if (File.Exists(filename))
				File.Delete(filename);

			Assert.Throws<ArgumentException>(
				() => Sldr.GetLdmlFile(filename, bcp47Tag)
			);
		}

		[Test, Ignore("Run by hand")]
		public void Get_BadFileName_Throws()
		{
			const string filename = "c:\badpath";
			const string bcp47Tag = "en";

			if (File.Exists(filename))
				File.Delete(filename);

			Assert.Throws<System.Net.WebException>(
				() => Sldr.GetLdmlFile(filename, bcp47Tag)
			);
		}

		[Test, Ignore("Run by hand")]
		public void Get_BadBcp47Tag_Throws()
		{
			const string filename = "c:\\temp\\en.ldml";
			const string bcp47Tag = "!@#";

			if (File.Exists(filename))
				File.Delete(filename);

			Assert.Throws<System.Net.WebException>(
				() => Sldr.GetLdmlFile(filename, bcp47Tag)
			);
		}

		// This test should be run on TC.  But ignore for now until LDML service correctly writes the SIL namespace
		[Test, Ignore("Run by hand")]
		public void Get_Validate()
		{
			const string filename = "c:\\temp\\en_GB.ldml";
			const string bcp47Tag = "en-GB";

			const string expectedLanguage = "en";
			const string expectedScript = "Latn";
			const string expectedTerritory = "GB";

			if (File.Exists(filename))
				File.Delete(filename);

			Sldr.GetLdmlFile(filename, bcp47Tag);

			// Parse the LDML file
			XElement element = XElement.Load(filename);
			XElement identity = element.Descendants("identity").First();
			Assert.AreEqual((string)identity.Element("language").Attribute("type"), expectedLanguage);
			Assert.AreEqual((string)identity.Element("script").Attribute("type"), expectedScript);
			Assert.AreEqual((string)identity.Element("territory").Attribute("type"), expectedTerritory);

			// Verify user ID is generated
			XElement silIdentityElem = identity.Descendants(Sldr.Sil + "identity").First();
			Assert.AreEqual(((string) silIdentityElem.Attribute("uid")).Length, 8);
		}
	}
}
