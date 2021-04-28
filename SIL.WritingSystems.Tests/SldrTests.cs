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
				var sldrCachePath = Sldr.SldrCachePath;
				Sldr.Cleanup();
				if (embeddedAllTagsTime == null)
					Sldr.Initialize(sldrOffline, sldrCachePath);
				else
					Sldr.Initialize(sldrOffline, sldrCachePath, embeddedAllTagsTime.Value);
				FolderContainingLdml = new TemporaryFolder("SldrTests");
				NamespaceManager = new XmlNamespaceManager(new NameTable());
				NamespaceManager.AddNamespace("sil", "urn://www.sil.org/ldml/0.1");
			}

			public XmlNamespaceManager NamespaceManager { get; }

			private TemporaryFolder FolderContainingLdml { get; }

			public SldrStatus GetLdmlFile(string ietfLanguageTag, out string filename)
			{
				return Sldr.GetLdmlFile(FolderContainingLdml.Path, ietfLanguageTag, new List<string> { "characters" }, out filename);
			}

			public string FilePath => FolderContainingLdml.Path;

			public void Dispose()
			{
				FolderContainingLdml.Dispose();
				var sldrCachePath = Sldr.SldrCachePath;
				Sldr.Cleanup();
				// clear out SLDR cache
				var di = new DirectoryInfo(sldrCachePath);
				foreach (var fi in di.GetFiles())
					fi.Delete();
				// The OfflineSldrAttribute has been assigned to the entire test assembly, so we reinitialize
				// the SLDR back to what it was
				Sldr.Initialize(true, sldrCachePath);
			}
		}

		[Test]
		public void GetLdmlFile_EmptyPath_Throws()
		{
			var path = string.Empty;
			const string ietfLanguageTag = "en";
			Assert.That(() => Sldr.GetLdmlFile(path, ietfLanguageTag, out var filename),
				Throws.ArgumentException);
		}

		[Test]
		public void GetLdmlFile_BadDirName_Throws()
		{
			const string path = "/dev/null/";
			const string ietfLanguageTag = "en";
			Assert.That(() => Sldr.GetLdmlFile(path, ietfLanguageTag, out var filename),
				Throws.TypeOf<DirectoryNotFoundException>());
		}

		[Test]
		public void GetLdmlFile_BadIetfLanguageTag_Throws()
		{
			using var environment = new TestEnvironment();
			const string ietfLanguageTag = "!@#";
			Assert.That(() => environment.GetLdmlFile(ietfLanguageTag, out var filename),
				Throws.ArgumentException);
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetLdmlFile_DefaultDownloadsAllTopLevelElements()
		{
			using var environment = new TestEnvironment(false);
			const string ietfLanguageTag = "en-US";
			var sldrStatus = Sldr.GetLdmlFile(environment.FilePath, ietfLanguageTag, out var filename);
			if(sldrStatus == SldrStatus.UnableToConnectToSldr)
				Assert.Ignore("Ignored because SLDR is offline.");
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

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetLdmlFile_UnknownLanguage_StatusFileNotFound()
		{
			using var environment = new TestEnvironment(false);
			const string ietfLanguageTag = "qaa";

			var sldrStatus = environment.GetLdmlFile(ietfLanguageTag, out var filename);
			if (sldrStatus == SldrStatus.UnableToConnectToSldr)
				Assert.Ignore("Ignored because SLDR is offline.");
			Assert.That(sldrStatus, Is.EqualTo(SldrStatus.NotFound));
		}

		[Test]
		public void GetLdmlFile_Fonipa_StatusFileNotFound()
		{
			using var environment = new TestEnvironment();
			const string ietfLanguageTag = "lo-LA-fonipa";

			Assert.That(environment.GetLdmlFile(ietfLanguageTag, out var filename), Is.EqualTo(SldrStatus.NotFound));
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetLdmlFile_LanguageTagWithSuppressedScript_DownloadsFile()
		{
			using var environment = new TestEnvironment(false);
			const string ietfLanguageTag = "oro";
			var sldrStatus = environment.GetLdmlFile(ietfLanguageTag, out var filename);
			if (sldrStatus == SldrStatus.UnableToConnectToSldr)
				Assert.Ignore("Ignored because SLDR is offline.");

			Assert.That(sldrStatus, Is.EqualTo(SldrStatus.FromSldr));
			Assert.That(File.Exists(Path.Combine(environment.FilePath, filename)), Is.True);
		}

		#region SLDR cache
		[Test]
		public void GetLdmlFile_CacheFileWithUid_StatusFileFromSldrCache()
		{
			using var environment = new TestEnvironment();
			var content =
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
			var filename = Path.Combine(Sldr.SldrCachePath, ietfLanguageTag + "-e2ccb575.ldml");
			File.WriteAllText(filename, content);

			Assert.That(environment.GetLdmlFile(ietfLanguageTag, out filename), Is.EqualTo(SldrStatus.FromCache));
			Assert.That(filename, Is.EqualTo(ietfLanguageTag + ".ldml"));
		}

		[Test]
		public void GetLdmlFile_CacheFileWithUidUnknown_StatusFileFromSldrCache()
		{
			using var environment = new TestEnvironment();
			var content =
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
			var filename = Path.Combine(Sldr.SldrCachePath, ietfLanguageTag + ".ldml");
			// File only exists in cache so uid unknown
			File.WriteAllText(filename, content);

			Assert.That(environment.GetLdmlFile(ietfLanguageTag, out filename), Is.EqualTo(SldrStatus.FromCache));
			var filePath = Path.Combine(environment.FilePath, filename);
			Assert.That(filename, Is.EqualTo(ietfLanguageTag + ".ldml"));
			AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='qaa']", environment.NamespaceManager);
			AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Latn']", environment.NamespaceManager);
			AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@draft='proposed']", environment.NamespaceManager);
			AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[not(@uid)]", environment.NamespaceManager);
		}

		#endregion

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetLdmlFile_NewFile_StatusFileFromSldr()
		{
			using var environment = new TestEnvironment(false);
			const string ietfLanguageTag = "en-GB";
			var sldrStatus = environment.GetLdmlFile(ietfLanguageTag, out var filename);
			if (sldrStatus == SldrStatus.UnableToConnectToSldr)
				Assert.Ignore("Ignored because SLDR is offline.");

			Assert.That(sldrStatus, Is.EqualTo(SldrStatus.FromSldr));

			var filePath = Path.Combine(environment.FilePath, filename);
			AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']", environment.NamespaceManager);
			AssertThatXmlIn.File(filePath).HasNoMatchForXpath("/ldml/identity/script", environment.NamespaceManager);
			AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='GB']", environment.NamespaceManager);
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetLdmlFile_SldrCacheDestinationPath_ReturnsCacheFile()
		{
			using var environment = new TestEnvironment(false);
			const string ietfLanguageTag = "en-GB";

			var sldrStatus = Sldr.GetLdmlFile(Sldr.SldrCachePath, ietfLanguageTag, new[] { "characters" }, out var filename);
			if (sldrStatus == SldrStatus.UnableToConnectToSldr)
				Assert.Ignore("Ignored because SLDR is offline.");

			Assert.That(sldrStatus, Is.EqualTo(SldrStatus.FromSldr));

			var filePath = Path.Combine(Sldr.SldrCachePath, filename);
			AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']", environment.NamespaceManager);
			AssertThatXmlIn.File(filePath).HasNoMatchForXpath("/ldml/identity/script", environment.NamespaceManager);
			AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath("/ldml/identity/territory[@type='GB']", environment.NamespaceManager);
		}

		[Test]
		public void GetLdmlFile_SldrStagingEnvironmentVariable_UsesStagingUrl()
		{
			var originalStagingValue = Environment.GetEnvironmentVariable(Sldr.SldrStaging);
			Environment.SetEnvironmentVariable(Sldr.SldrStaging, "true");
			StringAssert.Contains("&staging=1", Sldr.BuildLdmlRequestUrl("en", "", "", ""));
			Environment.SetEnvironmentVariable(Sldr.SldrStaging, originalStagingValue);
		}

		[Test]
		[Category("SkipOnTeamCity")]
		// This depends on when SLDR updates "en-US.ldml" with the revid
		public void GetLdmlFile_NotModified_DoesntDownloadNewFile()
		{
			using var environment = new TestEnvironment(false);

			// Write
			var content = LdmlContentForTests.Version3Identity("en", "", "US", "", "", "12345", "variantName", "", "d9fabd0fa2c615cfdfb3d2b48f403f55145ff16a");
			const string ietfLanguageTag = "en-US";

			// Write content to destination and cache
			File.WriteAllText(Path.Combine(environment.FilePath, ietfLanguageTag + ".ldml"), content);
			File.WriteAllText(Path.Combine(Sldr.SldrCachePath, ietfLanguageTag + ".ldml"), content);

			var sldrStatus = environment.GetLdmlFile(ietfLanguageTag, out var filename);
			if (sldrStatus == SldrStatus.UnableToConnectToSldr)
				Assert.Ignore("Ignored because SLDR is offline.");
			// Call a second time, this should use the Cache now
			sldrStatus = environment.GetLdmlFile(ietfLanguageTag, out filename);
			Assert.That(sldrStatus, Is.EqualTo(SldrStatus.FromCache));
			AssertThatXmlIn.File(Path.Combine(environment.FilePath, filename))
				.HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@source='cldr']", environment.NamespaceManager);
		}

		#region internal methods

		[Test]
		public void ReadSilIdentity_GetsRevidAndUid()
		{
			using var environment = new TestEnvironment();
			var content =
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
			var filename = Path.Combine(environment.FilePath, ietfLanguageTag + ".ldml");
			File.WriteAllText(filename, content);

			Assert.True(Sldr.ReadSilIdentity(filename, out var revid, out var uid));
			Assert.That(revid, Is.EqualTo("53d542ba498f40f437f7723e69dcf64dab6c9794"));
			Assert.That(uid, Is.EqualTo("e2ccb575"));
		}

		[Test]
		public void MoveTmpToCache_DraftApproved_RemovesUid()
		{
			using var environment = new TestEnvironment();
			var content =
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
			var filename = Path.Combine(environment.FilePath, ietfLanguageTag + ".ldml");
			// LDML in destination to get uid.
			File.WriteAllText(filename, content);
			var cacheFilename = Path.Combine(Sldr.SldrCachePath, ietfLanguageTag + "-e2ccb575.ldml");
			// Tmp and "filename + uid" in cache
			File.WriteAllText(cacheFilename, content);
			var tempFilename = Path.Combine(Sldr.SldrCachePath, ietfLanguageTag + ".ldml.tmp");
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

		[Test]
		public void MoveTmpToCache_DraftProposed_KeepsUid()
		{
			using var environment = new TestEnvironment();
			var content =
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
			var filename = Path.Combine(environment.FilePath, ietfLanguageTag + ".ldml.tmp");
			File.WriteAllText(filename, content);

			Sldr.MoveTmpToCache(filename, "e2ccb575");
			// uid appended to filename and uid attribute preserved
			filename = Path.Combine(environment.FilePath, ietfLanguageTag + "-e2ccb575.ldml");
			AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']", environment.NamespaceManager);
			AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@revid='53d542ba498f40f437f7723e69dcf64dab6c9794']", environment.NamespaceManager);
			AssertThatXmlIn.File(filename).HasAtLeastOneMatchForXpath("/ldml/identity/special/sil:identity[@uid='e2ccb575']", environment.NamespaceManager);
		}

		#endregion

		/// <summary>
		/// This test is only valid when run by itself. If other tests are running they can affect the file that
		/// this is trying to verify.
		/// </summary>
		[Test]
		[Category("SkipOnTeamCity")]
		[Category("ByHand")]
		[Explicit]
		public void LanguageTags_OlderEmbeddedLangTags_DownloadsNewLangTags()
		{
			using var testEnv = new TestEnvironment(false, new DateTime(2000, 1, 1, 12, 0, 0));
			var langTagsPath = Path.Combine(Sldr.SldrCachePath, "langtags.json");
			if (File.Exists(langTagsPath))
			{
				Assert.Ignore($"Delete the langtags.json from {langTagsPath} and run this test solo");
			}
			if (testEnv.GetLdmlFile("en", out var enLdml) == SldrStatus.UnableToConnectToSldr)
			{
				Assert.Ignore("SLDR is offline");
			}
			Assert.That(Sldr.LanguageTags, Is.Not.Empty);
			Assert.That(File.Exists(langTagsPath), Is.True);
		}

		/// <remarks>
		/// REVIEW (Hasso) 2019.11: this test loads Private Use script subtags and doesn't clean them up, but no other tests seem to care
		/// </remarks>
		[Test]
		public void LanguageTags_OlderCachedLangTagsSldrOffline_UseEmbeddedLangTags()
		{
			using (new TestEnvironment())
			{
				var langTagsPath = Path.Combine(Sldr.SldrCachePath, "langtags.json");

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
				var langTagsPath = Path.Combine(Sldr.SldrCachePath, "langtags.json");

				File.WriteAllText(langTagsPath, "[ { \"full\": \"en-Latn-GB\", \"iso639_3\": \"eng\", \"name\": \"English\", \"region\": \"GB\", \"sldr\": true, \"tag\": \"en-GB\" } ]");
				var time = DateTime.Parse(LanguageRegistryResources.AllTagsTime, CultureInfo.InvariantCulture);
				time += TimeSpan.FromSeconds(1);
				File.SetLastWriteTime(langTagsPath, time);

				Assert.That(Sldr.LanguageTags.Count, Is.EqualTo(1));
				Assert.That(File.Exists(langTagsPath), Is.True);
			}
		}

		/// <remarks>
		/// REVIEW (Hasso) 2019.11: this test loads Private Use script subtags and doesn't clean them up, but no other tests seem to care
		/// </remarks>
		[Test]
		public void LanguageTags_NoCachedLangTagsSldrOffline_UseEmbeddedLangTags()
		{
			using (new TestEnvironment())
			{
				var langTagsPath = Path.Combine(Sldr.SldrCachePath, "langtags.json");
				Assert.That(File.Exists(langTagsPath), Is.False);

				Assert.That(Sldr.LanguageTags, Is.Not.Empty);
				Assert.That(File.Exists(langTagsPath), Is.False);
			}
		}
	}
}
