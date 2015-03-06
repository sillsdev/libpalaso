using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	[Category("SkipOnTeamCity")]
	public class SldrTests
	{
		private class TestEnvironment : IDisposable
		{
			public TestEnvironment()
			{
				FolderContainingLdml = new TemporaryFolder("SldrTests");
			}

			private TemporaryFolder FolderContainingLdml { get; set; }

			public bool GetLdmlFile(string fileName, string bcp47Tag)
			{
				string filePath = Path.Combine(FolderContainingLdml.Path, fileName);
				if (File.Exists(filePath))
					File.Delete(filePath);

				return Sldr.GetLdmlFile(filePath, bcp47Tag);
			}

			public XElement ReadLdmlFile(string fileName)
			{
				string filePath = Path.Combine(FolderContainingLdml.Path, fileName);
				return XElement.Load(filePath);
			}

			public void Dispose()
			{
				FolderContainingLdml.Dispose();
			}
		}

		[Test]
		public void Get_EmptyFileName_Throws()
		{
			string filename = string.Empty;
			const string bcp47Tag = "en";

			Assert.Throws<ArgumentException>(
				() => Sldr.GetLdmlFile(filename, bcp47Tag)
			);
		}

		[Test]
		public void Get_BadFileName_Throws()
		{
			const string filename = "/dev/null/foo";
			const string bcp47Tag = "en";

			Assert.Throws<DirectoryNotFoundException>(
				() => Sldr.GetLdmlFile(filename, bcp47Tag)
			);
		}

		[Test]
		public void Get_BadBcp47Tag_Fails()
		{
			using (var environment = new TestEnvironment())
			{
				const string filename = "en.ldml";
				const string bcp47Tag = "!@#";

				Assert.That(environment.GetLdmlFile(filename, bcp47Tag), Is.False);
			}
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void Get_Validate()
		{
			using (var environment = new TestEnvironment())
			{
				const string filename = "en_GB.ldml";
				const string bcp47Tag = "en-GB";

				const string expectedLanguage = "en";
				const string expectedScript = "Latn";
				const string expectedTerritory = "GB";

				Assert.That(environment.GetLdmlFile(filename, bcp47Tag), Is.True);

				// Parse the LDML file
				XElement element = environment.ReadLdmlFile(filename);
				XElement identity = element.Descendants("identity").First();
				Assert.AreEqual((string) identity.Element("language").Attribute("type"), expectedLanguage);
				Assert.AreEqual((string) identity.Element("script").Attribute("type"), expectedScript);
				Assert.AreEqual((string) identity.Element("territory").Attribute("type"), expectedTerritory);

				// Verify user ID is generated
				XElement silIdentityElem = identity.Descendants(Sldr.Sil + "identity").First();
				Assert.AreEqual(((string) silIdentityElem.Attribute("uid")).Length, 8);
			}
		}
	}
}
