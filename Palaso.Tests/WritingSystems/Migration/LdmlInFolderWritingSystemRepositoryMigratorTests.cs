using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;
using Palaso.WritingSystems.Migration;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.Tests.WritingSystems.Migration
{
	[TestFixture]
	public class LdmlInFolderWritingSystemRepositoryMigratorTests
	{
		private class TestEnvironment : IDisposable
		{
			public TestEnvironment()
			{
				FolderContainingLdml = new TemporaryFolder("LdmlInFolderMigratorTests");
				NamespaceManager = new XmlNamespaceManager(new NameTable());
				NamespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			}

			public XmlNamespaceManager NamespaceManager { get; private set; }

			private TemporaryFolder FolderContainingLdml { get; set; }

			public void WriteLdmlFile(string fileName, string contentToWrite)
			{
				string filePath = Path.Combine(FolderContainingLdml.Path, fileName);
				File.WriteAllText(filePath, contentToWrite);
			}

			public void OnMigrateCallback(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo)
			{
				MigrationInfo = new List<LdmlVersion0MigrationStrategy.MigrationInfo>(migrationInfo);
			}

			public IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> MigrationInfo { get; private set; }

			public string LdmlPath
			{
				get { return FolderContainingLdml.Path; }
			}

			public void Dispose()
			{
				FolderContainingLdml.Dispose();
			}

			public int GetFileVersion(string fileName)
			{
				var versionReader = new WritingSystemLdmlVersionGetter();
				string filePath = Path.Combine(FolderContainingLdml.Path, fileName);
				return versionReader.GetFileVersion(filePath);
			}

			public string MappedFilePath(string sourceFileName)
			{
				var migrationInfo = MigrationInfo.First(info => info.FileName == sourceFileName);
				return Path.Combine(FolderContainingLdml.Path, migrationInfo.RfcTagAfterMigration + ".ldml");
			}

			public string FilePath(string fileName)
			{
				return Path.Combine(FolderContainingLdml.Path, fileName);
			}
		}

		#region InterFileTests

		[Test]
		public void Migrate_LdmlContainsWritingSystemThatIsVersion0_MigratedToLatest()
		{
			using (var environment = new TestEnvironment())
			{
				const string fileName = "en-Zxxx-x-audio.ldml";
				environment.WriteLdmlFile(fileName, LdmlContentForTests.Version0("en", "Zxxx", "", "x-audio"));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				Assert.AreEqual(1, environment.GetFileVersion(fileName));
			}
		}

		[Test]
		public void Migrate_WritingSystemRepositoryContainsWsThatWouldBeMigratedToDuplicateOfExistingWs_DuplicateWsAreDisambiguated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("en-Zxxx-x-audio.ldml", LdmlContentForTests.Version0("en", "Zxxx", "", "x-audio"));
				environment.WriteLdmlFile("en-x-audio.ldml", LdmlContentForTests.Version0("en", "", "", "x-audio"));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				Assert.True(File.Exists(environment.MappedFilePath("en-Zxxx-x-audio.ldml")));
				Assert.True(File.Exists(environment.MappedFilePath("en-x-audio.ldml")));
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/variant[@type='x-audio']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-x-audio.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-x-audio.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-x-audio.ldml"), "/ldml/identity/variant[@type='x-audio-dupl1']");
			}
		}

		[Test]
		public void Migrate_WritingSystemRepositoryContainsWsThatWouldBeMigratedToCaseInsensitiveDuplicateOfExistingWs_DuplicateWsAreDisambiguated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("en-Zxxx-x-audio.ldml", LdmlContentForTests.Version0("en", "Zxxx", "", "x-audio"));
				environment.WriteLdmlFile("en-x-audio.ldml", LdmlContentForTests.Version0("eN", "", "", "x-AuDio"));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				Assert.True(File.Exists(environment.MappedFilePath("en-Zxxx-x-audio.ldml")));
				Assert.True(File.Exists(environment.MappedFilePath("en-x-audio.ldml")));
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/variant[@type='x-audio']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-x-audio.ldml"), "/ldml/identity/language[@type='eN']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-x-audio.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-x-audio.ldml"), "/ldml/identity/variant[@type='x-AuDio-dupl1']");
			}
		}

		[Test]
		public void Migrate_RepoContainsWsThatWouldBeMigratedToCaseInsensitiveDuplicateOfExistingWswithBogusFileName_WritingSystemWithValidRfcTagBeforeMigrationIsNotTheDuplicate()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("bogus.ldml", LdmlContentForTests.Version0("eN", "ZxXx", "", "x-AuDio"));
				environment.WriteLdmlFile("en-Zxxx-x-audio.ldml", LdmlContentForTests.Version0("en", "", "", "x-audio"));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				Assert.True(File.Exists(environment.MappedFilePath("bogus.ldml")));
				Assert.True(File.Exists(environment.MappedFilePath("en-Zxxx-x-audio.ldml")));
				AssertLdmlHasXpath(environment.MappedFilePath("bogus.ldml"), "/ldml/identity/language[@type='eN']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus.ldml"), "/ldml/identity/script[@type='ZxXx']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus.ldml"), "/ldml/identity/variant[@type='x-AuDio']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/variant[@type='x-audio-dupl1']");
			}
		}

		[Test]
		public void Migrate_RepoContainsMultipleDuplicates_DuplicatesAreMarkedCorrectly()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("bogus.ldml", LdmlContentForTests.Version0("en", "Zxxx", "", "x-audio"));
				environment.WriteLdmlFile("bogus1.ldml", LdmlContentForTests.Version0("en", "", "", "x-audio"));
				environment.WriteLdmlFile("bogus2.ldml", LdmlContentForTests.Version0("en-x-audio", "", "", ""));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				Assert.True(File.Exists(environment.MappedFilePath("bogus.ldml")));
				Assert.True(File.Exists(environment.MappedFilePath("bogus1.ldml")));
				Assert.True(File.Exists(environment.MappedFilePath("bogus2.ldml")));
				AssertLdmlHasXpath(environment.MappedFilePath("bogus.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus.ldml"), "/ldml/identity/variant[@type='x-audio']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus1.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus1.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus1.ldml"), "/ldml/identity/variant[@type='x-audio-dupl1']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus2.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus2.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus2.ldml"), "/ldml/identity/variant[@type='x-audio-dupl2']");
			}
		}

		private static void AssertMigrationInfoContains(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo, string tagBefore, string tagAfter)
		{
			var info = migrationInfo.First(tag => tag.RfcTagBeforeMigration == tagBefore);
			Assert.IsNotNull(info, String.Format("'{0}' not found", tagBefore));
			Assert.AreEqual(tagAfter, info.RfcTagAfterMigration);
		}

		[Test]
		public void Migrate_RepoContainsRfcTagsThatWillBeMigrated_DelegateIsCalledAndHasCorrectMap()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("bogus.ldml", LdmlContentForTests.Version0("en-x-audio", "", "", ""));
				environment.WriteLdmlFile("bogus1.ldml", LdmlContentForTests.Version0("de", "bogus", "stuff", ""));
				environment.WriteLdmlFile("bogus2.ldml", LdmlContentForTests.Version0("en", "Zxxx", "", "x-audio"));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				AssertMigrationInfoContains(environment.MigrationInfo, "de-bogus-stuff", "de-x-bogus-stuff");
				AssertMigrationInfoContains(environment.MigrationInfo, "en-Zxxx-x-audio", "en-Zxxx-x-audio");
				AssertMigrationInfoContains(environment.MigrationInfo, "en-x-audio", "en-Zxxx-x-audio-dupl1");

			}
		}

		#endregion

		#region Intra File Tests

		[Test]
		public void Migrate_LanguageSubtagContainsFonipa_VariantContainsIpaVariantSubtag()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-fonipa", "","",""));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='fonipa']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsxDashEtic_xDashEticIsMovedToPrivateUseSubtag()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-fonipa-x-etic", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='fonipa-x-etic']");
			}
		}

		[Test]
		public void Migrate_VariantContainsMultipleValidVariants_ValidVariantsAreLeftUntouched()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "", "biske-bogus-bauddha"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='biske-bauddha-x-bogus']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsxDashEmic_xDashEmicIsMovedToPrivateUseSubtag()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-fonipa-x-emic", "", "", ""));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='fonipa-x-emic']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagInLdmlContainsxDashaudio_VariantSubtagInLdmlContainsxDashaudio()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-x-audio",String.Empty,String.Empty,String.Empty));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-audio']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsxDashaudioAndScriptSubtagIsNotEmpty_ScriptSubtagPartsAreMovedToPrivateUseAndScriptIsOverwrittenToBecomeZxxx()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-x-audio", "Latn", String.Empty, String.Empty));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-audio-Latn']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsxDashaudioAndScriptSubtagcontainsZxxx_ZxxxIsNotAppendedToPrivateUseSubtag()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-x-audio", "Zxxx", String.Empty, String.Empty));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-audio']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsValidScript_ScriptIsMovedToScript()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-Latn", String.Empty, String.Empty, String.Empty));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Latn']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsValidRegion_RegionIsMovedToRegion()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-US", String.Empty, String.Empty, String.Empty));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='US']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsValidVariant_VariantIsMovedToVariant()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-1901", String.Empty, String.Empty, String.Empty));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='1901']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsx_xIsNotDuplicated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-x-audio", String.Empty, String.Empty, "x-test"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-audio-test']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsDataThatIsNotValidLanguageScriptRegionOrVariant_DataIsMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-bogus-stuff", String.Empty, String.Empty, String.Empty));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsMultipleValidLanguageSubtagsAsWellAsDataThatIsNotValidLanguageScriptRegionOrVariant_LanguageIsSetToFirstValidLanguageSubtag()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-bogus-en-audio-de-bogus2-x-", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsMultipleValidLanguageSubtagsAsWellAsDataThatIsNotValidLanguageScriptRegionOrVariant_AllSubtagsButFirstValidLanguageSubtagAreMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-bogus-en-audio-tpi-bogus2-x-", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-audio-bogus2-tpi']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsInvalidData_InvalidDataIsMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "bogus-stuff", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsValidScript_ScriptIsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "Latn", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Latn']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsMultipleValidScriptSubtagsAsWellAsDataThatIsNotValidScript_NonValidDataIsMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "x-bogus-Latn-test-Zxxx-bogus2-x-", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Latn']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-test-bogus2-Zxxx']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsMultipleValidScriptSubtagsAsWellAsDataThatIsNotValidScriptAsWellAsAudio_NonValidDataisMovedToPrivateUseAsScriptIsZxxx()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "x-bogus-Latn-audio-Afak-bogus2-x-", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-audio-bogus2-Afak-Latn']");
			}
		}

		[Test]
		public void Migrate_LanguageAndScriptSubtagsContainInvalidData_AllInvalidDataIsMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("bogus-stuff", "more-bogus", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='']");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-stuff-more']");
			}
		}

		[Test]
		public void Migrate_RegionSubtagContainsAnythingButValidRegion_InvalidContentIsMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("qaa", "", "bogus-stuff", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_RegionContainsTagThatIsValidRegion_IsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "DE", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='DE']");
			}
		}

		[Test]
		public void Migrate_VariantSubtagContainsAnythingButValidVariantOrPrivateUse_InvalidContentIsMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "", "", "bogus-stuff"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsDatathatIsNotValidLanguageScriptRegionOrVariantAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-bogus", "", "", "x-BoGuS-stuff"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_ScriptContainsDuplicate_DuplicateIsmovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "Zxxx-zxXX", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-zxXX']");
			}
		}

		[Test]
		public void Migrate_RegionContainsDuplicate_DuplicateIsmovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "Us-US", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='Us']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-US']");
			}
		}

		[Test, Ignore("Currently duplicate variants are removed, which is ok")]
		public void Migrate_VariantContainsDuplicate_DuplicateIsmovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "", "biske-BiSKe"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='biske-x-BiSKe']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagAndRegionSubtagContainValidDuplicates_DataIsNotDuplicatedInPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("de", "", "DE", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='de']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='DE']");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsDuplicateValidlanguage_DuplicateIsMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-En", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-En']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsOnlyInvalidData_DataIsMovedToPrivateUseAndLanguageSubtagIsSetToQaa()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("bogus-stuff", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsSubtagThatIsToLongForPrivateUse_SubtagIsTruncated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "", "x-bogusstuffistolong"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogusstu']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsDuplicatePrivateUse_DuplicateIsRemoved()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("bogus", "", "", "x-bogus"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsAnythingButValidScriptAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "bogus-stuff", "", "x-bogus"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_RegionSubtagContainsAnythingButValidRegionAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "", "bogus-stuff", "x-bogus"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_VariantSubtagContainsAnythingButValidVariantOrprivateUseAndIsDuplicateOfDataInPrivateUse_DataIsNotDuplicatedInPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "", "", "bogus-stuff-x-bogus"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		private void AssertLdmlHasXpath(string filePath, string xPath)
		{
			AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath(xPath);
		}

		private void AssertLdmlHasNoXpath(string filePath, string xPath)
		{
			AssertThatXmlIn.File(filePath).HasNoMatchForXpath(xPath);
		}

		[Test]
		public void Migrate_OriginalFileContainsLdmlDataWeDontCareAbout_DataIsCopied()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0WithLdmlInfoWeDontCareAbout("","","",""));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/fallback/testing[text()='fallback']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/layout/testing[text()='layout']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/characters/testing[text()='characters']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/delimiters/testing[text()='delimiters']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/measurement/testing[text()='measurement']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/dates/testing[text()='dates']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/numbers/testing[text()='numbers']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/units/testing[text()='units']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/listPatterns/testing[text()='listPatterns']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/posix/testing[text()='posix']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/segmentations/testing[text()='segmentations']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/rbnf/testing[text()='rbnf']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/references/testing[text()='references']");
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsAllSortsOfDataThatShouldJustBeCopiedOver_DataIsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile(
					"test.ldml",
					LdmlContentForTests.Version0WithAllSortsOfDatathatdoesNotNeedSpecialAttention("", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertThatXmlIn.File(environment.MappedFilePath("test.ldml")).
					HasAtLeastOneMatchForXpath("/ldml/special/palaso:defaultFontFamily[@value='Arial']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.MappedFilePath("test.ldml")).
					HasAtLeastOneMatchForXpath("/ldml/special/palaso:defaultFontSize[@value='12']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.MappedFilePath("test.ldml")).
					HasAtLeastOneMatchForXpath("/ldml/special/palaso:abbreviation[@value='la']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.MappedFilePath("test.ldml")).
					HasAtLeastOneMatchForXpath("/ldml/special/palaso:isLegacyEncoded[@value='True']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.MappedFilePath("test.ldml")).
					HasAtLeastOneMatchForXpath("/ldml/special/palaso:defaultKeyboard[@value='bogusKeyboard']", environment.NamespaceManager);
				AssertThatXmlIn.File(environment.MappedFilePath("test.ldml")).
					HasAtLeastOneMatchForXpath("/ldml/special/palaso:spellCheckingId[@value='ol']", environment.NamespaceManager);
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsNoCollationInfo_CollationInfoIsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile(
					"test.ldml",
					LdmlContentForTests.Version0WithCollationInfo(WritingSystemDefinitionV0.SortRulesType.DefaultOrdering));
				var wsV0 = new WritingSystemDefinitionV0();
				new LdmlAdaptorV0().Read(environment.FilePath("test.ldml"), wsV0);

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				var wsV1 = new WritingSystemDefinitionV1();
				new LdmlAdaptorV1().Read(environment.MappedFilePath("test.ldml"), wsV1);
				Assert.AreEqual(String.Empty, wsV1.SortRules);
				Assert.AreEqual(Enum.GetName(typeof(WritingSystemDefinitionV0.SortRulesType), wsV0.SortUsing), Enum.GetName(typeof(WritingSystemDefinitionV1.SortRulesType), wsV0.SortUsing));
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsOtherLanguageCollationInfo_CollationInfoIsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile(
					"test.ldml",
					LdmlContentForTests.Version0WithCollationInfo(WritingSystemDefinitionV0.SortRulesType.OtherLanguage));
				var wsV0 = new WritingSystemDefinitionV0();
				new LdmlAdaptorV0().Read(environment.FilePath("test.ldml"), wsV0);

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				var wsV1 = new WritingSystemDefinitionV1();
				new LdmlAdaptorV1().Read(environment.MappedFilePath("test.ldml"), wsV1);
				Assert.AreEqual(wsV0.SortRules, wsV1.SortRules);
				Assert.AreEqual(Enum.GetName(typeof(WritingSystemDefinitionV0.SortRulesType), wsV0.SortUsing), Enum.GetName(typeof(WritingSystemDefinitionV1.SortRulesType), wsV0.SortUsing));
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsCustomSimpleCollationInfo_CollationInfoIsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile(
					"test.ldml",
					LdmlContentForTests.Version0WithCollationInfo(WritingSystemDefinitionV0.SortRulesType.CustomSimple));
				var wsV0 = new WritingSystemDefinitionV0();
				new LdmlAdaptorV0().Read(environment.FilePath("test.ldml"), wsV0);

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				var wsV1 = new WritingSystemDefinitionV1();
				new LdmlAdaptorV1().Read(environment.MappedFilePath("test.ldml"), wsV1);
				Assert.AreEqual(wsV0.SortRules, wsV1.SortRules);
				Assert.AreEqual(Enum.GetName(typeof(WritingSystemDefinitionV0.SortRulesType), wsV0.SortUsing), Enum.GetName(typeof(WritingSystemDefinitionV1.SortRulesType), wsV0.SortUsing));
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsCustomIcuCollationInfo_CollationInfoIsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile(
					"test.ldml",
					LdmlContentForTests.Version0WithCollationInfo(WritingSystemDefinitionV0.SortRulesType.CustomICU));
				var wsV0 = new WritingSystemDefinitionV0();
				new LdmlAdaptorV0().Read(environment.FilePath("test.ldml"), wsV0);

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				var wsV1 = new WritingSystemDefinitionV1();
				new LdmlAdaptorV1().Read(environment.MappedFilePath("test.ldml"), wsV1);
				Assert.AreEqual(wsV0.SortRules, wsV1.SortRules);
				Assert.AreEqual(Enum.GetName(typeof(WritingSystemDefinitionV0.SortRulesType), wsV0.SortUsing), Enum.GetName(typeof(WritingSystemDefinitionV1.SortRulesType), wsV0.SortUsing));
			}
		}

		[Test]
		public void Migrate_DateModified_IsLaterThanBeforeMigration()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile(
					"test.ldml",
					LdmlContentForTests.Version0WithAllSortsOfDatathatdoesNotNeedSpecialAttention("", "", "", ""));

				var wsV0 = new WritingSystemDefinitionV0();
				new LdmlAdaptorV0().Read(environment.FilePath("test.ldml"), wsV0);
				DateTime dateBeforeMigration = wsV0.DateModified;

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				var wsV1= new WritingSystemDefinitionV1();
				new LdmlAdaptorV1().Read(environment.MappedFilePath("test.ldml"), wsV1);
				DateTime dateAfterMigration = wsV1.DateModified;
				Assert.IsTrue(dateAfterMigration > dateBeforeMigration);
			}
		}

		[Test]
		public void Migrate_OriginalFileIsNotVersionThatWeCanMigrate_Throws()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version99Default());
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				Assert.Throws<InvalidOperationException>(migrator.Migrate);
			}
		}

		[Test]
		public void Migrate_LanguageNameIsSetTootherThanWhatIanaSubtagRegistrySays_LanguageNameIsMaintained()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0WithLanguageSubtagAndName("en", "German"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertThatXmlIn.File(environment.MappedFilePath("test.ldml")).
					HasAtLeastOneMatchForXpath("/ldml/special/palaso:languageName[@value='German']", environment.NamespaceManager);
			}
		}

		[Test]
		public void Migrate_LanguageNameIsNotSet_LanguageNameIsSetToWhatIanaSubtagRegistrySays()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0WithLanguageSubtagAndName("en", String.Empty));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertThatXmlIn.File(environment.MappedFilePath("test.ldml")).
					HasAtLeastOneMatchForXpath("/ldml/special/palaso:languageName[@value = 'English']", environment.NamespaceManager);
			}
		}

		[Test]
		public void Migrate_LdmlIsVersion0_IsLatestVersion()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0English());
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				var versionGetter = new WritingSystemLdmlVersionGetter();
				Assert.AreEqual(1, versionGetter.GetFileVersion(environment.MappedFilePath("test.ldml")));
			}
		}

		//[Test]
		//public void NeedsMigration_LdmlIsVersion0_IsTrue()
		//{
		//    using (var environment = new TestEnvironment())
		//    {
		//        environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version1English());
		//        var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.Path, environment.OnMigrateCallback);
		//        Assert.IsFalse(migrator.NeedsMigration());
		//    }
		//}

		//[Test]
		//public void NeedsMigration_LdmlIsAlreadyLatestVersion_IsFalse()
		//{
		//    using (var environment = new TestEnvironment())
		//    {
		//        environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version1English());
		//        var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.Path, environment.OnMigrateCallback);
		//        Assert.IsFalse(migrator.NeedsMigration());
		//    }
		//}

		//[Test]
		//public void NeedsMigration_FileIsVersion1_IsFalse()
		//{
		//    using (var environment = new TestEnvironment())
		//    {
		//        environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version1English());
		//        var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.Path, environment.OnMigrateCallback);
		//        migrator.NeedsMigration();
		//        Assert.IsFalse(migrator.NeedsMigration());
		//    }
		//}

		[Test]
		public void Migrate_LanguageSubtagContainsExcessiveXs_XsAreIgnoredAndNonIanaConformDataIsWrittenToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-x-a-x-b", "","",""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-a-b']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsValidSubtagWithPrecedingX_LanguageSubtagIsSetToValidSubtag()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsExcessiveXs_XsAreIgnoredAndNonIanaConformDataIsWrittenToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "x-Latn-x-test", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Latn']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-test']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsValidSubtagWithPrecedingX_ScriptSubtagIsSetToValidSubtag()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "x-latn", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='latn']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagInLdmlIsEmpty_LanguageSubtagIsSetToQaa()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-x-", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "Latn-x-", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Latn']");
			}

		}

		[Test]
		public void Migrate_RegionSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "", "US-x-", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='US']");
			}

		}

		[Test]
		public void Migrate_VariantSubtagEndsInDashXDash_DashXDashIsRemoved()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "", "", "x-bogus-x-"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemoved()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("de-e...n", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='de']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-en']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemovedAndResultMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("", "Z._x!x%x-Latn", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Latn']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-Zxxx']");
			}
		}

		[Test]
		public void Migrate_RegionSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemovedAndResultMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "U.!$%^S-gb", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='gb']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-US']");
			}
		}

		[Test]
		public void Migrate_VariantSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemovedAndResultMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "", "b*is^k_e-1901"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='1901-x-biske']");
			}
		}

		[Test]
		public void Migrate_PrivateUseSubtagContainsNonAlphaNumericCharacters_NonAlphaNumericCharactersAreRemoved()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "", "x-t@#$e_st-hi"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-test-hi']");
			}
		}

		[Test]
		public void Migrate_RfcTagContainsOnlyNonAlphaNumericCharactersAndEndsInDashXDash_WritingsystemIsSetToqaa()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("!@$#-x-", "(*^%$-x-@#%-x", "x-@^**__", "x-@#$-x-_-x-x-"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsAmbiguousLanguageOrRegionAndNoPrecedingLanguageSubtagExists_LeaveAmbiguousTagInLanguageSubtag()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("DE", "","",""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='DE']");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsAmbiguousLanguageOrRegionAndPrecedingLanguageSubtagExists_MoveAmbiguousTagToRegion()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("de-DE", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='de']");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='DE']");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsAmbiguousLanguageOrRegionAndidenticalPrecedingLanguageSubtagExists_MoveAmbiguousTagToRegion()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("de-de", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='de']");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='de']");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagContainsZxxx_VariantDoesNotContainxDashaudio()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-Zxxx", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant");
			}
		}

		[Test]
		public void Migrate_VariantSubtagContainsZxxx_RfctagisConvertedToAudio()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "", "Zxxx"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-audio']");
			}
		}

		[Test]
		public void Migrate_FileIsVersion0_IsMigratedToLatestVersion()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "", ""));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				Assert.AreEqual(1, environment.GetFileVersion(environment.MappedFilePath("test.ldml")));
			}
		}

		[Test]
		public void Migrate_NoFiles_DoesNotAddFilesOrDirectories()
		{
			using (var environment = new TestEnvironment())
			{
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				Assert.AreEqual(0, Directory.GetFiles(environment.LdmlPath).Length);
				Assert.AreEqual(0, Directory.GetDirectories(environment.LdmlPath).Length);
			}
		}

		[Test]
		public void Migrate_FolderContainsFiles_FolderContainsOnlyMigratedFiles()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				Assert.AreEqual(1, Directory.GetFiles(environment.LdmlPath).Length);
				Assert.AreEqual(0, Directory.GetDirectories(environment.LdmlPath).Length);
			}
		}

		#endregion

	}
}
