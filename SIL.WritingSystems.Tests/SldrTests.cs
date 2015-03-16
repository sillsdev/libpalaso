using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
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
				FolderContainingSldrCache = new TemporaryFolder("SldrCache");
				NamespaceManager = new XmlNamespaceManager(new NameTable());
				NamespaceManager.AddNamespace("sil", "urn://www.sil.org/ldml/0.1");
			}

			public XmlNamespaceManager NamespaceManager { get; private set; }

			private TemporaryFolder FolderContainingLdml { get; set; }
			private TemporaryFolder FolderContainingSldrCache { get; set; }

			public SldrStatus GetLdmlFile(string ietfLanguageTag, out string filename)
			{
				return Sldr.GetLdmlFile(FolderContainingLdml.Path, ietfLanguageTag, new List<string> { "characters" }, out filename);
			}

			public string FilePath()
			{
				return FolderContainingLdml.Path;
			}

			public string SldrCacheFilePath()
			{
				return FolderContainingSldrCache.Path;
			}

			public void Dispose()
			{
				FolderContainingLdml.Dispose();
				FolderContainingSldrCache.Dispose();
			}
		}

		[Test]
		public void Get_EmptyPath_Throws()
		{
			string path = string.Empty;
			const string ietfLanguageTag = "en";
			string filename;
			Assert.Throws<ArgumentException>(
				() => Sldr.GetLdmlFile(path, ietfLanguageTag, out filename)
			);
		}

		[Test]
		public void Get_BadDirName_Throws()
		{
			const string path = "/dev/null/";
			const string ietfLanguageTag = "en";
			string filename;
			Assert.Throws<DirectoryNotFoundException>(
				() => Sldr.GetLdmlFile(path, ietfLanguageTag, out filename)
			);
		}

		[Test]
		public void Get_BadIetfLanguageTag_Throws()
		{
			using (var environment = new TestEnvironment())
			{
				const string ietfLanguageTag = "!@#";
				string filename;
				Assert.Throws<ArgumentException>(
					() => Sldr.GetLdmlFile(environment.FilePath(), ietfLanguageTag, out filename));
			}
		}

		[Test]
		public void ReadSilIdentity()
		{
			using (var environment = new TestEnvironment())
			{
				string content =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $'/>
		<generation date='$Date: 2015-01-30 22:33 +0000 $'/>
		<language type='en'/>
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity source='cldr' draft='proposed' revid='53d542ba498f40f437f7723e69dcf64dab6c9794' uid='e2ccb575'/>
		</special>
		<script type='Latn'/>
	</identity>
</ldml>".Replace("\'", "\"");
				const string ietfLanguageTag = "en";
				string filename = Path.Combine(environment.FilePath(), ietfLanguageTag + ".ldml");
				File.WriteAllText(filename, content);

				string revid, uid;
				Assert.True(Sldr.ReadSilIdentity(filename, out revid, out uid));
				Assert.That(revid, Is.EqualTo("53d542ba498f40f437f7723e69dcf64dab6c9794"));
				Assert.That(uid, Is.EqualTo("e2ccb575"));
			}
		}

		[Test]
		public void MoveTmpToCache_DraftApprovedRemovesUid()
		{
			using (var environment = new TestEnvironment())
			{
				string content =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $'/>
		<generation date='$Date: 2015-01-30 22:33 +0000 $'/>
		<language type='en'/>
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity source='cldr' draft='approved' revid='53d542ba498f40f437f7723e69dcf64dab6c9794' uid='e2ccb575'/>
		</special>
		<script type='Latn'/>
	</identity>
</ldml>".Replace("\'", "\"");
				const string ietfLanguageTag = "en";
				string filename = Path.Combine(environment.FilePath(), ietfLanguageTag + ".ldml");
				// LDML in destination to get uid.
				File.WriteAllText(filename, content);
				string cacheFilename = Path.Combine(environment.SldrCacheFilePath(), ietfLanguageTag + "-e2ccb575.ldml");
				// Tmp and "filename + uid" in cache
				File.WriteAllText(cacheFilename, content);
				string tempFilename = Path.Combine(environment.SldrCacheFilePath(), ietfLanguageTag + ".ldml.tmp");
				File.WriteAllText(tempFilename, content);
				Assert.True(File.Exists(cacheFilename));

				Sldr.MoveTmpToCache(tempFilename, "e2ccb575");
				// file + original uid no longer exists in SLDR cache
				Assert.True(!File.Exists(cacheFilename));
				filename = Path.Combine(environment.FilePath(), ietfLanguageTag + ".ldml");
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']", environment.NamespaceManager);
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@revid='53d542ba498f40f437f7723e69dcf64dab6c9794']", environment.NamespaceManager);
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[not(uid)]", environment.NamespaceManager);
			}
		}

		[Test]
		public void MoveTmpToCache_DraftProposedKeepsUid()
		{
			using (var environment = new TestEnvironment())
			{
				string content =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $'/>
		<generation date='$Date: 2015-01-30 22:33 +0000 $'/>
		<language type='en'/>
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity source='cldr' draft='proposed' revid='53d542ba498f40f437f7723e69dcf64dab6c9794' uid='e2ccb575'/>
		</special>
		<script type='Latn'/>
	</identity>
</ldml>".Replace("\'", "\"");
				const string ietfLanguageTag = "en";
				string filename = Path.Combine(environment.FilePath(), ietfLanguageTag + ".ldml.tmp");
				File.WriteAllText(filename, content);

				Sldr.MoveTmpToCache(filename, "e2ccb575");
				// uid appended to filename and uid attribute preserved
				filename = Path.Combine(environment.FilePath(), ietfLanguageTag + "-e2ccb575.ldml");
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']", environment.NamespaceManager);
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@revid='53d542ba498f40f437f7723e69dcf64dab6c9794']", environment.NamespaceManager);
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@uid='e2ccb575']", environment.NamespaceManager);
			}
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void Get_FileNotFound()
		{
			using (var environment = new TestEnvironment())
			{
				string filename;
				const string ietfLanguageTag = "qaa";

				Assert.That(environment.GetLdmlFile(ietfLanguageTag, out filename), Is.EqualTo(SldrStatus.FileNotFound));
			}
		}

		[Test, Ignore("Run by hand")]
		// This needs to be run offline to pass
		public void Get_FileFromSldrCacheWithUid()
		{
			using (var environment = new TestEnvironment())
			{
				string content =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $'/>
		<generation date='$Date: 2015-01-30 22:33 +0000 $'/>
		<language type='qaa'/>
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity source='cldr' draft='proposed' revid='53d542ba498f40f437f7723e69dcf64dab6c9794' uid='e2ccb575'/>
		</special>
		<script type='Latn'/>
	</identity>
</ldml>".Replace("\'", "\"");
				const string ietfLanguageTag = "qaa";
				// File exists in destination and cache, so uid will be checked
				File.WriteAllText(Path.Combine(environment.FilePath(), ietfLanguageTag + ".ldml"), content);
				string filename = Path.Combine(environment.SldrCacheFilePath(), ietfLanguageTag + "-e2ccb575.ldml");
				File.WriteAllText(filename, content);

				Assert.That(environment.GetLdmlFile(ietfLanguageTag, out filename), Is.EqualTo(SldrStatus.FileFromSldrCache));
				Assert.That(filename, Is.EqualTo(ietfLanguageTag + ".ldml"));
			}
		}

		[Test, Ignore("Run by hand")]
		// This needs to be run offline to pass
		public void Get_FileFromSldrCacheUidUnknown()
		{
			using (var environment = new TestEnvironment())
			{
				string content =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $'/>
		<generation date='$Date: 2015-01-30 22:33 +0000 $'/>
		<language type='qaa'/>
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity source='cldr' draft='proposed' revid='53d542ba498f40f437f7723e69dcf64dab6c9794'/>
		</special>
		<script type='Latn'/>
	</identity>
</ldml>".Replace("\'", "\"");
				const string ietfLanguageTag = "qaa";
				string filename = Path.Combine(environment.SldrCacheFilePath(), ietfLanguageTag + ".ldml");
				// File only exists in cache so uid unknown
				File.WriteAllText(filename, content);

				Assert.That(environment.GetLdmlFile(ietfLanguageTag, out filename), Is.EqualTo(SldrStatus.FileFromSldrCache));
				string filePath = Path.Combine(environment.FilePath(), filename);
				Assert.That(filename, Is.EqualTo(ietfLanguageTag + ".ldml"));
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@draft='proposed']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[not(@uid)]", environment.NamespaceManager);

			}
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void Get_FileFromSldr()
		{
			using (var environment = new TestEnvironment())
			{
				string filename;
				const string ietfLanguageTag = "en-GB";

				Assert.That(environment.GetLdmlFile(ietfLanguageTag, out filename), Is.EqualTo(SldrStatus.FileFromSldr));

				string filePath = Path.Combine(environment.FilePath(), filename);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='GB']", environment.NamespaceManager);

				// Verify draft is approved and uid doesn't exist
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@draft='approved']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[not(@uid)]", environment.NamespaceManager);
			}
		}
	}
}
