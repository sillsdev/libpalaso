using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	[Category("SkipOnTeamCity")]
	public class SldrTests
	{

		[Test]
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

		[Test]
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

		[Test]
		public void Get_BadBcp47Tag_Throws()
		{
			string filename = Path.Combine(Path.GetTempPath(), "en.ldml");
			const string bcp47Tag = "!@#";

			if (File.Exists(filename))
				File.Delete(filename);

			Assert.Throws<System.Net.WebException>(
				() => Sldr.GetLdmlFile(filename, bcp47Tag)
			);
		}

		// This test should be run on TC.  But ignore for now until LDML service correctly writes the SIL namespace
		[Test]
		public void Get_Validate()
		{
			string filename = Path.Combine(Path.GetTempPath(), "en_GB.ldml");
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
