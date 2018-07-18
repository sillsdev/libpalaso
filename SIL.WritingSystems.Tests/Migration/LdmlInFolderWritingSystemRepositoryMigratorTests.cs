using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.WritingSystems.Migration;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace SIL.WritingSystems.Tests.Migration
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
				NamespaceManager.AddNamespace("sil", "urn://www.sil.org/ldml/0.1");
				NamespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
				NamespaceManager.AddNamespace("palaso2", "urn://palaso.org/ldmlExtensions/v2");
			}

			private XmlNamespaceManager NamespaceManager { get; set; }

			private TemporaryFolder FolderContainingLdml { get; set; }

			public void WriteLdmlFile(string fileName, string contentToWrite)
			{
				string filePath = Path.Combine(FolderContainingLdml.Path, fileName);
				File.WriteAllText(filePath, contentToWrite);
			}

			public void ReadLdmlFile(string filePath, WritingSystemDefinition ws)
			{
				var adaptor = new LdmlDataMapper(new TestWritingSystemFactory());
				adaptor.Read(filePath, ws);
			}

			/// <summary>
			/// Dictionary to map original filename the final post-migrated IETF language tag.
			/// </summary>
			private readonly Dictionary<string, string> _oldNameToLanguageTag = new Dictionary<string, string>(); 

			public void OnMigrateCallback(int toVersion, IEnumerable<LdmlMigrationInfo> migrationInfo)
			{			
				foreach (LdmlMigrationInfo info in migrationInfo)
				{
					KeyValuePair<string, string> entry = _oldNameToLanguageTag.FirstOrDefault(e => e.Value == info.LanguageTagBeforeMigration);
					_oldNameToLanguageTag[entry.Key ?? info.FileName] = info.LanguageTagAfterMigration;
				}
			}
			
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
				string filename = _oldNameToLanguageTag[sourceFileName] + ".ldml";
				return Path.Combine(FolderContainingLdml.Path, filename);
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

				Assert.AreEqual(LdmlDataMapper.CurrentLdmlLibraryVersion, environment.GetFileVersion(fileName));
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
				AssertLdmlHasXpath(environment.MappedFilePath("en-x-audio.ldml"), "/ldml/identity/variant[@type='x-audio-dupl0']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("en-x-audio.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-x-audio.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-x-audio.ldml"), "/ldml/identity/variant[@type='x-audio-dupl0']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("bogus.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus.ldml"), "/ldml/identity/variant[@type='x-audio']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("en-Zxxx-x-audio.ldml"), "/ldml/identity/variant[@type='x-audio-dupl0']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("bogus1.ldml"), "/ldml/identity/variant[@type='x-audio-dupl0']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus2.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus2.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus2.ldml"), "/ldml/identity/variant[@type='x-audio-dupl1']");
			}
		}

		[Test]
		public void Migrate_RepoContainsMultipleDuplicatesV1_DuplicatesAreRenamedAndMarkedCorrectly()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("bogus.ldml", LdmlContentForTests.Version1("en", "Zxxx", "", "x-audio"));
				environment.WriteLdmlFile("bogus1.ldml", LdmlContentForTests.Version1("en", "", "", "x-audio"));
				environment.WriteLdmlFile("bogus2.ldml", LdmlContentForTests.Version1("en-x-audio", "", "", ""));

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
				AssertLdmlHasXpath(environment.MappedFilePath("bogus1.ldml"), "/ldml/identity/variant[@type='x-audio-dupl0']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus2.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus2.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus2.ldml"), "/ldml/identity/variant[@type='x-audio-dupl1']");
			}
		}

		[Test]
		public void Migrate_RepoContainsMultipleDuplicatesEn_DuplicatesAreRenamedAndMarkedCorrectly()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("bogus.ldml", LdmlContentForTests.Version1("en", "", "", ""));
				environment.WriteLdmlFile("bogus1.ldml", LdmlContentForTests.Version1("en", "Latn", "", ""));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				Assert.True(File.Exists(environment.MappedFilePath("bogus.ldml")));
				Assert.True(File.Exists(environment.MappedFilePath("bogus1.ldml")));
				AssertLdmlHasXpath(environment.MappedFilePath("bogus.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus1.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasNoXpath(environment.MappedFilePath("bogus1.ldml"), "/ldml/identity/script");
				AssertLdmlHasXpath(environment.MappedFilePath("bogus1.ldml"), "/ldml/identity/variant[@type='x-dupl0']");
			}
			
		}

		private static void AssertMigrationInfoContains(IEnumerable<LdmlMigrationInfo> migrationInfo, string tagBefore, string tagAfter)
		{
			var info = migrationInfo.First(tag => tag.LanguageTagBeforeMigration == tagBefore);
			Assert.IsNotNull(info, String.Format("'{0}' not found", tagBefore));
			Assert.AreEqual(tagAfter, info.LanguageTagAfterMigration);
		}

		[Test]
		public void Migrate_RepoContainsRfcTagsThatWillBeMigrated_DelegateIsCalledAndHasCorrectMap()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("bogus.ldml", LdmlContentForTests.Version0("en-x-audio", "", "", ""));
				environment.WriteLdmlFile("bogus1.ldml", LdmlContentForTests.Version0("de", "bogus", "stuff", ""));
				environment.WriteLdmlFile("bogus2.ldml", LdmlContentForTests.Version0("en", "Zxxx", "", "x-audio"));
				environment.WriteLdmlFile("bogus3.ldml", LdmlContentForTests.Version0("zh", "", "CN", ""));
				environment.WriteLdmlFile("bogus4.ldml", LdmlContentForTests.Version0("cmn", "", "", ""));

				var migrationInfo = new Dictionary<int, LdmlMigrationInfo[]>();
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath,
					(toVersion, infos) => migrationInfo[toVersion] = infos.ToArray());
				migrator.Migrate();

				AssertMigrationInfoContains(migrationInfo[2], "de-bogus-stuff", "de-Qaaa-QM-x-bogus-stuff");
				AssertMigrationInfoContains(migrationInfo[2], "en-Zxxx-x-audio", "en-Zxxx-x-audio");
				AssertMigrationInfoContains(migrationInfo[2], "en-x-audio", "en-Zxxx-x-audio-dupl0");
				AssertMigrationInfoContains(migrationInfo[2], "zh-CN", "zh-CN");
				AssertMigrationInfoContains(migrationInfo[2], "cmn", "zh-CN-x-dupl0");

				AssertMigrationInfoContains(migrationInfo[3], "de-Qaaa-QM-x-bogus-stuff", "de-Qaaa-QM-x-bogus-stuff");
				AssertMigrationInfoContains(migrationInfo[3], "en-Zxxx-x-audio", "en-Zxxx-x-audio");
				AssertMigrationInfoContains(migrationInfo[3], "en-Zxxx-x-audio-dupl0", "en-Zxxx-x-audio-dupl0");
				AssertMigrationInfoContains(migrationInfo[3], "zh-CN", "zh-CN");
				AssertMigrationInfoContains(migrationInfo[3], "zh-CN-x-dupl0", "zh-CN-x-dupl0");
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
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en-Thai", String.Empty, String.Empty, String.Empty));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Thai']");
			}
		}

		[Test]
		public void Migrate_LanguageSubtagIsIso3Code_UseIso1Code()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("eng", String.Empty, String.Empty, String.Empty));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='en']");
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
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("bogus-en-audio-de-bogus2-x-", "", "", ""));
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
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("bogus-en-audio-tpi-bogus2-x-", "", "", ""));
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Qaaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-stuff']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsValidScript_ScriptIsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "Cyrl", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Cyrl']");
			}
		}

		[Test]
		public void Migrate_ScriptSubtagContainsImplicitScript_ScriptIsRemoved()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "Latn", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasNoXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script");
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

		/// <summary>
		/// JohnT: the outcome here is perhaps not completely obvious. The input is an invalid LDML where language and script
		/// are both specified with two part codes that are not valid. The first part of each comes to be treated as the
		/// private-use language and script: thus, "bogus", from the language code, becomes the first private-use part,
		/// and qaa is inserted as the LDML language code, and "more" from the invalid script code becomes the second
		/// private-use part, corresponding to script = "Qaaa".
		/// The remaining invalid components are moved to the end of the private-use, except that the duplicate 'bogus' is removed.
		/// </summary>
		[Test]
		public void Migrate_LanguageAndScriptSubtagsContainInvalidData_AllInvalidDataIsMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("bogus-stuff", "more-bogus", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Qaaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-bogus-more-stuff']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='QM']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='US']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Qaaa']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='QM']");
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

		#region FlexPrivateUseWritingSystem
		[Test]
		public void Migrate_OriginalWasFlexPrivateUseWritingSystemButNowChangedLanguage_IdentityElementChangedToPalasoWay()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-en']");
				var ws = new WritingSystemDefinition();
				environment.ReadLdmlFile(environment.MappedFilePath("test.ldml"), ws);
				ws.Language = "de";
				environment.WriteLdmlFile(environment.MappedFilePath("test.ldml"), LdmlContentForTests.CurrentVersion(ws.LanguageTag));
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='de']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='']");
			}
		}

		[Test]
		public void Migrate_OriginalWasFlexPrivateUseWritingSystemButNowChangedScript_IdentityElementChangedToPalasoWay()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "Zxxx", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-en']");
				var ws = new WritingSystemDefinition();
				environment.ReadLdmlFile(environment.MappedFilePath("test.ldml"), ws);
				ws.Script = "Latn";
				environment.WriteLdmlFile(environment.MappedFilePath("test.ldml"), LdmlContentForTests.CurrentVersion(ws.LanguageTag));
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Latn']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-en']");
			}
		}

		[Test]
		public void Migrate_OriginalWasFlexPrivateUseWritingSystemButNowChangedRegion_IdentityElementChangedToPalasoWay()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "", "US", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='US']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-en']");
				var ws = new WritingSystemDefinition();
				environment.ReadLdmlFile(environment.MappedFilePath("test.ldml"), ws);
				ws.Region = "GB";
				environment.WriteLdmlFile(environment.MappedFilePath("test.ldml"), LdmlContentForTests.CurrentVersion(ws.LanguageTag));
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='GB']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-en']");
			}
		}

		[Test]
		public void Migrate_OriginalWasFlexPrivateUseWritingSystemButNowChangedVariant_IdentityElementChangedToPalasoWay()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "", "", "fonipa"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='fonipa-x-en']");
				var ws = new WritingSystemDefinition();
				environment.ReadLdmlFile(environment.MappedFilePath("test.ldml"), ws);
				ws.Variants.Clear();
				ws.Variants.Add("1901");
				environment.WriteLdmlFile(environment.MappedFilePath("test.ldml"), LdmlContentForTests.CurrentVersion(ws.LanguageTag));
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='1901-x-en']");
			}
		}

		[Test]
		public void Migrate_OriginalWasFlexPrivateUseWritingSystemButNowChangedPrivateUse_IdentityElementChangedToPalasoWay()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "", "", "x-private"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-en-private']");
				var ws = new WritingSystemDefinition();
				environment.ReadLdmlFile(environment.MappedFilePath("test.ldml"), ws);
				ws.Variants.Clear();
				ws.Variants.Add("changed");
				environment.WriteLdmlFile(environment.MappedFilePath("test.ldml"), LdmlContentForTests.CurrentVersion(ws.LanguageTag));
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-en-changed']");
			}
		}

		[Test]
		public void Migrate_LdmlIsFlexPrivateUseFormatLanguageAndVariantArePopulated_PrivateUseLanguageMovedToPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "", "", "fonipa"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='fonipa-x-en']");
			}
		}

		[Test]
		public void Migrate_LdmlIsFlexPrivateUseFormatLanguageAndPrivateUseIsPopulated_LanguageTagIsMovedAndIsFirstInPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "", "", "x-private"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-en-private']");
			}
		}

		[Test]
		public void Migrate_FlexEntirelyPrivateUseLdmlContainingLanguageScriptRegionVariant_WritingSystemVariantIsConcatOfPrivateUseVariant()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "Zxxx", "US", "1901-x-audio"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='US']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='1901-x-en-audio']");
			}
		}

		[Test]
		public void Migrate_FlexEntirelyPrivateUseLdmlContainingLanguage_WritingSystemVariantIsLanguage()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/variant[@type='x-en']");
			}
		}

		[Test]
		public void Migrate_FlexEntirelyPrivateUseLdmlContainingLanguageScript_WritingSystemVariantIsLanguage()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "Zxxx", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "ldml/identity/script[@type='Zxxx']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "ldml/identity/variant[@type='x-en']");
			}
		}

		[Test]
		public void Migrate_FlexEntirelyPrivateUseLdmlContainingLanguageRegion_WritingSystemVariantIdIsLanguage()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("x-en", "", "US", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "ldml/identity/territory[@type='US']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "ldml/identity/variant[@type='x-en']");
			}
		}
#endregion

		private static void AssertLdmlHasXpath(string filePath, string xPath)
		{
			AssertThatXmlIn.File(filePath).HasAtLeastOneMatchForXpath(xPath);
		}

		private static void AssertLdmlHasNoXpath(string filePath, string xPath)
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/measurement/testing[text()='measurement']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/dates/testing[text()='dates']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/units/testing[text()='units']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/listPatterns/testing[text()='listPatterns']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/posix/testing[text()='posix']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/segmentations/testing[text()='segmentations']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/rbnf/testing[text()='rbnf']");
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/references/testing[text()='references']");
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsNoCollationInfo_StandardCollationInfoIsMigrated()
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

				var wsV3 = new WritingSystemDefinition();
				new LdmlDataMapper(null).Read(environment.MappedFilePath("test.ldml"), wsV3);
				var cdV3 = (IcuRulesCollationDefinition) wsV3.Collations.First();
				Assert.IsNullOrEmpty(wsV0.SortRules);
				Assert.IsNullOrEmpty(cdV3.CollationRules);
				Assert.That(cdV3.Type, Is.EqualTo("standard"));
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsSystemCollationInfo_CollationInfoIsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile(
					"test.ldml",
					LdmlContentForTests.Version0WithSystemCollationInfo());
				var wsV0 = new WritingSystemDefinitionV0();
				new LdmlAdaptorV0().Read(environment.FilePath("test.ldml"), wsV0);

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				var repo = new TestLdmlInFolderWritingSystemRepository(environment.LdmlPath);
				migrator.ResetRemovedProperties(repo);

				WritingSystemDefinition ws = repo.Get("de");
				var scd = new SystemCollationDefinition {LanguageTag = "de"};
				Assert.That(ws.DefaultCollation.ValueEquals(scd), Is.True);

				var fromFile = new WritingSystemDefinition();
				new LdmlDataMapper(new TestWritingSystemFactory()).Read(environment.MappedFilePath("test.ldml"), fromFile);
				Assert.NotNull(fromFile.DefaultCollation);
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

				var wsV3 = new WritingSystemDefinition();
				new LdmlDataMapper(null).Read(environment.MappedFilePath("test.ldml"), wsV3);
				var cdV3 = (SimpleRulesCollationDefinition) wsV3.Collations.First();
				Assert.AreEqual(wsV0.SortRules, cdV3.SimpleRules);
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

				var wsV3 = new WritingSystemDefinition();
				new LdmlDataMapper(null).Read(environment.MappedFilePath("test.ldml"), wsV3);
				var cdV3 = (IcuRulesCollationDefinition) wsV3.Collations.First();
				Assert.AreEqual(wsV0.SortRules, cdV3.CollationRules);
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsFwNamespace_InfoIsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile(
					"test.ldml",
					LdmlContentForTests.Version0WithFw("en", "x-Kala", "x-AP", "1996-x-myOwnVariant"));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				var repo = new TestLdmlInFolderWritingSystemRepository(environment.LdmlPath);
				migrator.ResetRemovedProperties(repo);

				var other = new FontDefinition("Arial") {Features = "order=3 children=2 color=red createDate=1996", Roles = FontRoles.Default};

				var main = new CharacterSetDefinition("main")
				{
					Characters = {
						"α", "Α", "ά", "ὰ", "ᾷ", "ἀ", "Ἀ", "ἁ", "Ἁ", "ἄ", "Ἄ", "ἂ", "ἅ", "Ἅ", "ἃ", "Ἃ", "ᾶ", "ᾳ", "ᾴ", "ἆ", "Ἆ", "ᾄ", "ᾅ", "β", "Β", "γ",
						"Γ", "δ", "Δ", "ε", "Ε", "έ", "ὲ", "ἐ", "Ἐ", "ἑ", "Ἑ", "ἔ", "Ἔ", "ἕ", "Ἕ", "ἓ", "Ἓ", "ζ", "Ζ", "η", "Η", "ή", "ὴ", "ῇ", "ἠ", "Ἠ",
						"ἡ", "Ἡ", "ἤ", "Ἤ", "ἢ", "ἥ", "Ἥ", "Ἢ", "ἣ", "ᾗ", "ῆ", "ῃ", "ῄ", "ἦ", "Ἦ", "ᾖ", "ἧ", "ᾐ", "ᾑ", "ᾔ", "θ", "Θ", "ι", "ί", "ὶ", "ϊ",
						"ΐ", "ῒ", "ἰ", "Ἰ", "ἱ", "Ἱ", "ἴ", "Ἴ", "ἵ", "Ἵ", "ἳ", "ῖ", "ἶ", "ἷ", "κ", "Κ", "λ", "Λ", "μ", "Μ", "ν", "Ν", "ξ", "Ξ", "ο", "Ο",
						"ό", "ὸ", "ὀ", "Ὀ", "ὁ", "Ὁ", "ὄ", "Ὄ", "ὅ", "ὂ", "Ὅ", "ὃ", "Ὃ", "π", "Π", "ρ", "Ρ", "ῥ", "Ῥ", "σ", "ς", "Σ", "τ", "Τ", "υ", "Υ",
						"ύ", "ὺ", "ϋ", "ΰ", "ῢ", "ὐ", "ὑ", "Ὑ", "ὔ", "ὕ", "ὒ", "Ὕ", "ὓ", "ῦ", "ὖ", "ὗ", "Ὗ", "φ", "Φ", "χ", "Χ", "ψ", "Ψ", "ω", "ώ", "ὼ",
						"ῷ", "ὠ", "ὡ", "Ὡ", "ὤ", "Ὤ", "ὢ", "ὥ", "Ὥ", "ᾧ", "ῶ", "ῳ", "ῴ", "ὦ", "Ὦ", "ὧ", "Ὧ", "ᾠ"
					}
				};

				var punctuation = new CharacterSetDefinition("punctuation") {Characters = {" ", "-", ",", ".", "’", "«", "»", "(", ")", "[", "]"}};

				WritingSystemDefinition ws = repo.Get("en-Qaaa-QM-1996-x-Kala-AP-myOwnVar");

				Assert.That(ws.DefaultFont.ValueEquals(other));
				Assert.That(ws.WindowsLcid, Is.EqualTo("4321"));
				Assert.That(ws.CharacterSets["main"].ValueEquals(main));
				Assert.That(ws.NumberingSystem.ValueEquals(new NumberingSystemDefinition("thai")));
				Assert.That(ws.CharacterSets["punctuation"].ValueEquals(punctuation));

				// ScriptName, RegionName, VariantName, LegacyMapping, IsGraphiteEnabled
				Assert.That(ws.LegacyMapping, Is.EqualTo("SomeMapper"));
				Assert.That(ws.IsGraphiteEnabled, Is.True);
				Assert.That(ws.Script.Name, Is.EqualTo("scriptName"));
				Assert.That(ws.Region.Name, Is.EqualTo("regionName"));
				Assert.That(ws.Variants[1].Name, Is.EqualTo("aVarName"));
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsFwNamespaceWithOldValidChars_ValidCharsIsMigratedFromLegacyOverridesFile()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile(
					"test.ldml",
					LdmlContentForTests.Version0WithOldFwValidChars("en", "", "US", ""));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				var repo = new TestLdmlInFolderWritingSystemRepository(environment.LdmlPath);
				migrator.ResetRemovedProperties(repo);

				var main = new CharacterSetDefinition("main") {Characters = {"x", "y", "z"}};

				WritingSystemDefinition ws = repo.Get("en-US");

				Assert.That(ws.CharacterSets["main"].ValueEquals(main));
				Assert.That(ws.CharacterSets.Contains("numeric"), Is.False);
				Assert.That(ws.CharacterSets.Contains("punctuation"), Is.False);
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsPalasoNamespace_InfoIsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile(
					"test.ldml",
					LdmlContentForTests.Version0WithAllSortsOfDatathatdoesNotNeedSpecialAttention("x-kal", "", "", ""));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				var repo = new TestLdmlInFolderWritingSystemRepository(environment.LdmlPath);
				migrator.ResetRemovedProperties(repo);

				WritingSystemDefinition ws = repo.Get("qaa-x-kal");
				Assert.That(ws.SpellCheckingId, Is.EqualTo("ol"));
				Assert.That(ws.Abbreviation, Is.EqualTo("la"));
				Assert.That(ws.DefaultFontSize, Is.EqualTo(12));
				Assert.That(ws.Language.Name, Is.EqualTo("language"));
				Assert.That(ws.DefaultFont.Name, Is.EqualTo("Arial"));
				Assert.That(ws.Keyboard, Is.EqualTo("bogusKeyboard"));
			}
		}

		[Test]
		public void Migrate_OriginalFileContainsPalaso2Namespace_InfoIsMigrated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version2("qaa", "", "", "x-kal"));

				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				var repo = new TestLdmlInFolderWritingSystemRepository(environment.LdmlPath);
				migrator.ResetRemovedProperties(repo);

				WritingSystemDefinition ws = repo.Get("qaa-x-kal");
				Assert.That(ws.DefaultFontSize, Is.EqualTo(12));
				Assert.That(ws.DefaultFont.Name, Is.EqualTo("Arial"));
				Assert.That(ws.KnownKeyboards.Select(kd => kd.Id), Is.EqualTo(new[] {"en-US_English"}));
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

				var wsV3 = new WritingSystemDefinition();
				new LdmlDataMapper(new TestWritingSystemFactory()).Read(environment.MappedFilePath("test.ldml"), wsV3);
				DateTime dateAfterMigration = wsV3.DateModified;
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

				var repo = new TestLdmlInFolderWritingSystemRepository(environment.LdmlPath);
				migrator.ResetRemovedProperties(repo);

				WritingSystemDefinition ws = repo.Get("en");
				Assert.That(ws.Language.Name, Is.EqualTo("German"));
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

				var repo = new TestLdmlInFolderWritingSystemRepository(environment.LdmlPath);
				migrator.ResetRemovedProperties(repo);

				WritingSystemDefinition ws = repo.Get("en");
				Assert.That(ws.Language.Name, Is.EqualTo("English"));
			}
		}

		[Test]
		public void Migrate_LdmlIsVersion0_IsLatestVersion()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("test.ldml", LdmlContentForTests.Version0("en", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				var versionGetter = new WritingSystemLdmlVersionGetter();
				Assert.AreEqual(LdmlDataMapper.CurrentLdmlLibraryVersion, versionGetter.GetFileVersion(environment.MappedFilePath("test.ldml")));
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
		public void Migrate_LanguageSubtagContainsValidSubtagWithPrecedingX_IsChangedToNonPrivateUse()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("x-en.ldml", LdmlContentForTests.Version0("x-en", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				AssertLdmlHasXpath(environment.MappedFilePath("x-en.ldml"), "/ldml/identity/language[@type='qaa']");
				AssertLdmlHasXpath(environment.MappedFilePath("x-en.ldml"), "/ldml/identity/variant[@type='x-en']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/script[@type='Latn']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='GB']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/language[@type='de']");
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
				AssertLdmlHasXpath(environment.MappedFilePath("test.ldml"), "/ldml/identity/territory[@type='DE']");
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

				Assert.AreEqual(LdmlDataMapper.CurrentLdmlLibraryVersion, environment.GetFileVersion(environment.MappedFilePath("test.ldml")));
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

		#region ChangeLog Tests

		[Test]
		public void Migrate_WritingSystemRepositoryNeedsMigrating_WSChangeLogUpdated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("en-bogus.ldml",
										  LdmlContentForTests.Version0("en-bogus", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				string idChangeLogFilePath = Path.Combine(environment.LdmlPath, "idchangelog.xml");
				AssertThatXmlIn.File(idChangeLogFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Change[From/text()='en-bogus' and To/text()='en-x-bogus']");
			}
		}

		[Test]
		public void Migrate_WritingSystemRepositoryNoNeedForMigration_WSChangeLogDoesntExist()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("en.ldml",
										  LdmlContentForTests.Version0("en", "", "", ""));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();
				string idChangeLogFilePath = Path.Combine(environment.LdmlPath, "idchangelog.xml");
				Assert.IsFalse(File.Exists(idChangeLogFilePath));
			}
		}

		[Test]
		public void Migrate_Layout_RightToLeftIsRetained()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WriteLdmlFile("hbo.ldml",
					LdmlContentForTests.Version2WithRightToLeftLayout("hbo", "Hebrew, Ancient"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.LdmlPath, environment.OnMigrateCallback);
				migrator.Migrate();

				var wsV3 = new WritingSystemDefinition();
				new LdmlDataMapper(new TestWritingSystemFactory()).Read(environment.MappedFilePath("hbo.ldml"), wsV3);
				Assert.True(wsV3.RightToLeftScript);
			}
		}

		#endregion

	}
}
