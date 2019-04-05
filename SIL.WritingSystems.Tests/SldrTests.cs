using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SIL.ObjectModel;
using SIL.TestUtilities;
using Is = SIL.TestUtilities.NUnitExtensions.Is;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class SldrTests
	{
		private class TestEnvironment : IDisposable
		{
			public TestEnvironment(bool sldrOffline = true, DateTime? embeddedAllTagsTime = null)
			{
				string sldrCachePath = Sldr.SldrCachePath;
				Sldr.Cleanup();
				if (embeddedAllTagsTime == null)
					Sldr.Initialize(sldrOffline, sldrCachePath);
				else
					Sldr.Initialize(sldrOffline, sldrCachePath, embeddedAllTagsTime.Value);
				FolderContainingLdml = new TemporaryFolder("SldrTests");
				NamespaceManager = new XmlNamespaceManager(new NameTable());
				NamespaceManager.AddNamespace("sil", "urn://www.sil.org/ldml/0.1");
			}

			public XmlNamespaceManager NamespaceManager { get; private set; }

			private TemporaryFolder FolderContainingLdml { get; set; }

			public SldrStatus GetLdmlFile(string ietfLanguageTag, out string filename)
			{
				return Sldr.GetLdmlFile(FolderContainingLdml.Path, ietfLanguageTag, new List<string> { "characters" }, out filename);
			}

			public string FilePath
			{
				get { return FolderContainingLdml.Path; }
			}

			public void Dispose()
			{
				FolderContainingLdml.Dispose();
				string sldrCachePath = Sldr.SldrCachePath;
				Sldr.Cleanup();
				// clear out SLDR cache
				DirectoryInfo di = new DirectoryInfo(sldrCachePath);
				foreach (FileInfo fi in di.GetFiles())
					fi.Delete();
				// The OfflineSldrAttribute has been assigned to the entire test assembly, so we reinitialize
				// the SLDR back to what it was
				Sldr.Initialize(true, sldrCachePath);
			}
		}

		[Test]
		public void GetLdmlFile_EmptyPath_Throws()
		{
			string path = string.Empty;
			const string ietfLanguageTag = "en";
			string filename;
			Assert.Throws<ArgumentException>(
				() => Sldr.GetLdmlFile(path, ietfLanguageTag, out filename)
			);
		}

		[Test]
		public void GetLdmlFile_BadDirName_Throws()
		{
			const string path = "/dev/null/";
			const string ietfLanguageTag = "en";
			string filename;
			Assert.Throws<DirectoryNotFoundException>(
				() => Sldr.GetLdmlFile(path, ietfLanguageTag, out filename)
			);
		}

		[Test]
		public void GetLdmlFile_BadIetfLanguageTag_Throws()
		{
			using (var environment = new TestEnvironment())
			{
				const string ietfLanguageTag = "!@#";
				string filename;
				Assert.Throws<ArgumentException>(
					() => environment.GetLdmlFile(ietfLanguageTag, out filename));
			}
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetLdmlFile_DefaultDownloadsAllTopLevelElements()
		{
			using (var environment = new TestEnvironment(false))
			{
				string filename;
				const string ietfLanguageTag = "en-US";
				var sldrStatus = Sldr.GetLdmlFile(environment.FilePath, ietfLanguageTag, out filename);
				if(sldrStatus == SldrStatus.UnableToConnectToSldr)
					Assert.Ignore("Ignored becuase SLDR is offline.");
				Assert.That(sldrStatus, Is.EqualTo(SldrStatus.FromSldr));
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[identity]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[localeDisplayNames]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[layout]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[contextTransforms]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[characters]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[delimiters]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[dates]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[numbers]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[units]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[listPatterns]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[collations]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[posix]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[segmentations]", environment.NamespaceManager);
				AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename)).HasAtLeastOneMatchForXpath("/ldml[metadata]", environment.NamespaceManager);
			}
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetLdmlFile_UnknownLanguage_StatusFileNotFound()
		{
			using (var environment = new TestEnvironment(false))
			{
				string filename;
				const string ietfLanguageTag = "qaa";

				var sldrStatus = environment.GetLdmlFile(ietfLanguageTag, out filename);
				if (sldrStatus == SldrStatus.UnableToConnectToSldr)
					Assert.Ignore("Ignored becuase SLDR is offline.");
				Assert.That(sldrStatus, Is.EqualTo(SldrStatus.NotFound));
			}
		}

		[Test]
		public void GetLdmlFile_Fonipa_StatusFileNotFound()
		{
			using (var environment = new TestEnvironment())
			{
				string filename;
				const string ietfLanguageTag = "lo-LA-fonipa";

				Assert.That(environment.GetLdmlFile(ietfLanguageTag, out filename), Is.EqualTo(SldrStatus.NotFound));
			}
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetLdmlFile_LanguageTagWithSuppressedScript_DownloadsFile()
		{
			using (var environment = new TestEnvironment(false))
			{
				string filename;
				const string ietfLanguageTag = "oro";
				var sldrStatus = environment.GetLdmlFile(ietfLanguageTag, out filename);
				if (sldrStatus == SldrStatus.UnableToConnectToSldr)
					Assert.Ignore("Ignored becuase SLDR is offline.");

				Assert.That(sldrStatus, Is.EqualTo(SldrStatus.FromSldr));
				Assert.That(File.Exists(Path.Combine(environment.FilePath, filename)), Is.True);
			}
		}

		#region SLDR cache
		[Test]
		public void GetLdmlFile_CacheFileWithUid_StatusFileFromSldrCache()
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
				File.WriteAllText(Path.Combine(environment.FilePath, ietfLanguageTag + ".ldml"), content);
				string filename = Path.Combine(Sldr.SldrCachePath, ietfLanguageTag + "-e2ccb575.ldml");
				File.WriteAllText(filename, content);

				Assert.That(environment.GetLdmlFile(ietfLanguageTag, out filename), Is.EqualTo(SldrStatus.FromCache));
				Assert.That(filename, Is.EqualTo(ietfLanguageTag + ".ldml"));
			}
		}

		[Test]
		public void GetLdmlFile_CacheFileWithUidUnknown_StatusFileFromSldrCache()
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
				string filename = Path.Combine(Sldr.SldrCachePath, ietfLanguageTag + ".ldml");
				// File only exists in cache so uid unknown
				File.WriteAllText(filename, content);

				Assert.That(environment.GetLdmlFile(ietfLanguageTag, out filename), Is.EqualTo(SldrStatus.FromCache));
				string filePath = Path.Combine(environment.FilePath, filename);
				Assert.That(filename, Is.EqualTo(ietfLanguageTag + ".ldml"));
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@draft='proposed']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[not(@uid)]", environment.NamespaceManager);
			}
		}

		#endregion

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetLdmlFile_NewFile_StatusFileFromSldr()
		{
			using (var environment = new TestEnvironment(false))
			{
				string filename;
				const string ietfLanguageTag = "en-GB";
				var sldrStatus = environment.GetLdmlFile(ietfLanguageTag, out filename);
				if (sldrStatus == SldrStatus.UnableToConnectToSldr)
					Assert.Ignore("Ignored becuase SLDR is offline.");

				Assert.That(sldrStatus, Is.EqualTo(SldrStatus.FromSldr));

				string filePath = Path.Combine(environment.FilePath, filename);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasNoMatchForXpath("/ldml/identity/script", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='GB']", environment.NamespaceManager);

				// Verify draft is approved and uid doesn't exist
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@draft='approved']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[not(@uid)]", environment.NamespaceManager);
			}
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetLdmlFile_SldrCacheDestinationPath_ReturnsCacheFile()
		{
			using (var environment = new TestEnvironment(false))
			{
				string filename;
				const string ietfLanguageTag = "en-GB";

				var sldrStatus = Sldr.GetLdmlFile(Sldr.SldrCachePath, ietfLanguageTag, new[] { "characters" }, out filename);
				if (sldrStatus == SldrStatus.UnableToConnectToSldr)
					Assert.Ignore("Ignored becuase SLDR is offline.");

				Assert.That(sldrStatus, Is.EqualTo(SldrStatus.FromSldr));

				string filePath = Path.Combine(Sldr.SldrCachePath, filename);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasNoMatchForXpath("/ldml/identity/script", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='GB']", environment.NamespaceManager);

				// Verify draft is approved and uid doesn't exist
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@draft='approved']", environment.NamespaceManager);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[not(@uid)]", environment.NamespaceManager);
			}
		}

		[Test]
		[Category("SkipOnTeamCity")]
		// This depends on when SLDR updates "en-US.ldml" with the revid 
		public void GetLdmlFile_NotModified_DoesntDownloadNewFile()
		{
			using (var environment = new TestEnvironment(false))
			{
				// Write 
				string content = LdmlContentForTests.Version3Identity("en", "", "US", "", "", "12345", "variantName", "", "d9fabd0fa2c615cfdfb3d2b48f403f55145ff16a");
				const string ietfLanguageTag = "en-US";

				// Write content to destination and cache
				File.WriteAllText(Path.Combine(environment.FilePath, ietfLanguageTag + ".ldml"), content);
				File.WriteAllText(Path.Combine(Sldr.SldrCachePath, ietfLanguageTag + ".ldml"), content);

				string filename;
				var sldrStatus = environment.GetLdmlFile(ietfLanguageTag, out filename);
				if (sldrStatus == SldrStatus.UnableToConnectToSldr)
					Assert.Ignore("Ignored becuase SLDR is offline.");
				Assert.That(sldrStatus, Is.EqualTo(SldrStatus.FromCache));
				string filePath = Path.Combine(environment.FilePath, filename);
				AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@windowsLCID='12345']", environment.NamespaceManager);
			}
		}

		#region internal methods

		[Test]
		public void ReadSilIdentity_GetsRevidAndUid()
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
				string filename = Path.Combine(environment.FilePath, ietfLanguageTag + ".ldml");
				File.WriteAllText(filename, content);

				string revid, uid;
				Assert.True(Sldr.ReadSilIdentity(filename, out revid, out uid));
				Assert.That(revid, Is.EqualTo("53d542ba498f40f437f7723e69dcf64dab6c9794"));
				Assert.That(uid, Is.EqualTo("e2ccb575"));
			}
		}

		[Test]
		public void MoveTmpToCache_DraftApproved_RemovesUid()
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
				string filename = Path.Combine(environment.FilePath, ietfLanguageTag + ".ldml");
				// LDML in destination to get uid.
				File.WriteAllText(filename, content);
				string cacheFilename = Path.Combine(Sldr.SldrCachePath, ietfLanguageTag + "-e2ccb575.ldml");
				// Tmp and "filename + uid" in cache
				File.WriteAllText(cacheFilename, content);
				string tempFilename = Path.Combine(Sldr.SldrCachePath, ietfLanguageTag + ".ldml.tmp");
				File.WriteAllText(tempFilename, content);
				Assert.True(File.Exists(cacheFilename));

				Sldr.MoveTmpToCache(tempFilename, "e2ccb575");
				// file + original uid no longer exists in SLDR cache
				Assert.True(!File.Exists(cacheFilename));
				filename = Path.Combine(environment.FilePath, ietfLanguageTag + ".ldml");
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']", environment.NamespaceManager);
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@revid='53d542ba498f40f437f7723e69dcf64dab6c9794']", environment.NamespaceManager);
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[not(uid)]", environment.NamespaceManager);
			}
		}

		[Test]
		public void MoveTmpToCache_DraftProposed_KeepsUid()
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
				string filename = Path.Combine(environment.FilePath, ietfLanguageTag + ".ldml.tmp");
				File.WriteAllText(filename, content);

				Sldr.MoveTmpToCache(filename, "e2ccb575");
				// uid appended to filename and uid attribute preserved
				filename = Path.Combine(environment.FilePath, ietfLanguageTag + "-e2ccb575.ldml");
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']", environment.NamespaceManager);
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@revid='53d542ba498f40f437f7723e69dcf64dab6c9794']", environment.NamespaceManager);
				AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@uid='e2ccb575']", environment.NamespaceManager);
			}
		}

		#endregion

		/// <summary>
		/// This test is only valid when run by its self. If other tests are running they can affect the file that
		/// this is trying to verify.
		/// </summary>
		[Test]
		[Category("SkipOnTeamCity")]
		[Category("ByHand")]
		public void LanguageTags_OlderEmbeddedLangTags_DownloadsNewLangTags()
		{
			using (var testEnv = new TestEnvironment(false, new DateTime(2000, 1, 1, 12, 0, 0)))
			{
				string enLdml;
				if (testEnv.GetLdmlFile("en", out enLdml) == SldrStatus.UnableToConnectToSldr)
				{
					Assert.Ignore();
				}
				string langTagsPath = Path.Combine(Sldr.SldrCachePath, "langtags.json");
				Assert.That(File.Exists(langTagsPath), Is.False);
				Assert.That(Sldr.LanguageTags, Is.Not.Empty);
				Assert.That(File.Exists(langTagsPath), Is.True);
			}
		}

		[Test]
		public void LanguageTags_OlderCachedLangTagsSldrOffline_UseEmbeddedLangTags()
		{
			using (new TestEnvironment())
			{
				string langTagsPath = Path.Combine(Sldr.SldrCachePath, "langtags.json");

				File.WriteAllText(langTagsPath, "[ { \"full\": \"en-Latn-GB\", \"iso639_3\": \"eng\", \"name\": \"English\", \"region\": \"GB\", \"sldr\": true, \"tag\": \"en-GB\" } ]");
				File.SetLastWriteTime(langTagsPath, new DateTime(2000, 1, 1, 12, 0, 0));

				Assert.That(Sldr.LanguageTags.Count, Is.GreaterThan(1));
				Assert.That(File.Exists(langTagsPath), Is.False);
			}
		}

		[Test]
		public void LanguageTags_NewerCachedLangTagsSldrOffline_UseCachedLangTags()
		{
			using (new TestEnvironment())
			{
				string langTagsPath = Path.Combine(Sldr.SldrCachePath, "langtags.json");

				File.WriteAllText(langTagsPath, "[ { \"full\": \"en-Latn-GB\", \"iso639_3\": \"eng\", \"name\": \"English\", \"region\": \"GB\", \"sldr\": true, \"tag\": \"en-GB\" } ]");
				DateTime time = DateTime.Parse(LanguageRegistryResources.AllTagsTime, CultureInfo.InvariantCulture);
				time += TimeSpan.FromSeconds(1);
				File.SetLastWriteTime(langTagsPath, time);

				Assert.That(Sldr.LanguageTags.Count, Is.EqualTo(1));
				Assert.That(File.Exists(langTagsPath), Is.True);
			}
		}

		[Test]
		public void LanguageTags_NoCachedLangTagsSldrOffline_UseEmbeddedLangTags()
		{
			using (new TestEnvironment())
			{
				string langTagsPath = Path.Combine(Sldr.SldrCachePath, "langtags.json");
				Assert.That(File.Exists(langTagsPath), Is.False);

				Assert.That(Sldr.LanguageTags, Is.Not.Empty);
				Assert.That(File.Exists(langTagsPath), Is.False);
			}
		}
	}
}
