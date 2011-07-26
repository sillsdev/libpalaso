using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Palaso.WritingSystems;
using Palaso.TestUtilities;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class LdmlInFolderWritingSystemRepositoryInterfaceTests : IWritingSystemRepositoryTests
	{
		private List<string> _testPaths;

		[SetUp]
		public override void SetUp()
		{
			_testPaths = new List<string>();
			base.SetUp();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			foreach (string testPath in _testPaths)
			{
				if (Directory.Exists(testPath))
				{
					Directory.Delete(testPath, true);
				}
			}
		}

		public override IWritingSystemRepository CreateNewStore()
		{
			string testPath = Path.GetTempPath() + "PalasoTest" + _testPaths.Count;
			if (Directory.Exists(testPath))
			{
				Directory.Delete(testPath, true);
			}
			_testPaths.Add(testPath);
			LdmlInFolderWritingSystemRepository repository = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, testPath);
			//repository.DontAddDefaultDefinitions = true;
			return repository;
		}
	}

	[TestFixture]
	public class LdmlInFolderWritingSystemRepositoryTests
	{

		private class TestEnvironment : IDisposable
		{
			private readonly TemporaryFolder _tempFolder;
			private readonly WritingSystemDefinition _writingSystem;

			public TestEnvironment()
			{
				_tempFolder = new TemporaryFolder("LdmlInFolderWritingSystemRepositoryTests");
				NamespaceManager = new XmlNamespaceManager(new NameTable());
				NamespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
				if (Directory.Exists(TestPath))
				{
					Directory.Delete(TestPath, true);
				}
				_writingSystem = new WritingSystemDefinition();
				Collection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, TestPath);
			}

			public void Dispose()
			{
				_tempFolder.Dispose();
			}

			public LdmlInFolderWritingSystemRepository Collection { get; set; }

			public string TestPath
			{
				get { return _tempFolder.Path; }
			}

			public WritingSystemDefinition WritingSystem
			{
				get { return _writingSystem; }
			}

			public XmlNamespaceManager NamespaceManager { get; private set; }

			public string GetPathForWsId(string id)
			{
				var path = Path.Combine(TestPath, id + ".ldml");
				return path;
			}

			public void AssertWritingSystemFileExists(string id)
			{
				Assert.IsTrue(File.Exists(GetPathForWsId(id)));
			}
		}

		[Test]
		public void LatestVersion_IsOne()
		{
			Assert.AreEqual(1, WritingSystemDefinition.LatestWritingSystemDefinitionVersion);
		}

		[Test]
		public void PathToCollection_SameAsGiven()
		{
			using (var environment = new TestEnvironment())
			{
				Assert.AreEqual(environment.TestPath, environment.Collection.PathToWritingSystems);
			}
		}

		[Test]
		public void SaveDefinitionsThenLoad_CountEquals2()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "one";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				var ws2 = new WritingSystemDefinition();
				ws2.Language = "two";
				environment.Collection.SaveDefinition(ws2);
				var newStore = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);

				Assert.AreEqual(2, newStore.Count);
			}
		}

		[Test]
		public void SavesWhenNotPreexisting()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SaveDefinition(environment.WritingSystem);
				environment.AssertWritingSystemFileExists(environment.WritingSystem.RFC5646);
			}
		}

		[Test]
		public void SavesWhenPreexisting()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var ws2 = new WritingSystemDefinition();
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(ws2);
			}
		}

		[Test]
		public void RegressionWhereUnchangedDefDeleted()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "qaa";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				WritingSystemDefinition ws2 = environment.Collection.Get(environment.WritingSystem.StoreID);
				environment.Collection.SaveDefinition(ws2);
				environment.AssertWritingSystemFileExists(environment.WritingSystem.RFC5646);
			}
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			using (var environment = new TestEnvironment())
			{
				var newRepoPath = Path.Combine(environment.TestPath, "newguy");
				var newRepository = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, newRepoPath);
				newRepository.SaveDefinition(environment.WritingSystem);
				Assert.That(File.Exists(Path.Combine(newRepoPath, environment.WritingSystem.RFC5646 + ".ldml")));
			}
		}

		[Test]
		public void Save_WritingSystemIdChanged_ChangeLogUpdated()
		{
			using (var e = new TestEnvironment())
			{
				var repo = LdmlInFolderWritingSystemRepository.Initialize(
					DummyMigratorCallback.onMigration,
					Path.Combine(e.TestPath, "idchangedtest1")
				);
				var ws = WritingSystemDefinition.FromLanguage("en");
				repo.Set(ws);
				repo.Save();

				ws.Script = "Latn";
				repo.Set(ws);
				ws.Script = "Thai";
				repo.Set(ws);

				var ws2 = WritingSystemDefinition.FromLanguage("de");
				repo.Set(ws2);
				ws2.Script = "Latn";
				repo.Save();

				string logFilePath = Path.Combine(repo.PathToWritingSystems, "idchangelog.xml");
				AssertThatXmlIn.File(logFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Change/From[text()='en']");
				AssertThatXmlIn.File(logFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Change/To[text()='en-Thai']");

				// writing systems added for the first time shouldn't be in the log as a change
				AssertThatXmlIn.File(logFilePath).HasNoMatchForXpath("/WritingSystemChangeLog/Changes/Change/From[text()='de']");
			}
		}

		[Test]
		public void StoreIDAfterSave_SameAsFileNameWithoutExtension()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				Assert.AreEqual("en", environment.WritingSystem.StoreID);
			}
		}

		[Test]
		public void StoreIDAfterLoad_SameAsFileNameWithoutExtension()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				Assert.AreNotEqual(0, Directory.GetFiles(environment.TestPath, "*.ldml"));
				environment.Collection.SaveDefinition(environment.WritingSystem);
				var newStore = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				WritingSystemDefinition ws2 = newStore.Get("en");
				Assert.AreEqual(
					Path.GetFileNameWithoutExtension(Directory.GetFiles(environment.TestPath, "*.ldml")[0]), ws2.StoreID);
			}
		}

		[Test]
		public void UpdatesFileNameWhenISOChanges()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = Path.Combine(environment.Collection.PathToWritingSystems, "en.ldml");
				Assert.IsTrue(File.Exists(path));
				WritingSystemDefinition ws2 = environment.Collection.Get(environment.WritingSystem.StoreID);
				ws2.Language = "de";
				Assert.AreEqual("en", ws2.StoreID);
				environment.Collection.SaveDefinition(ws2);
				Assert.AreEqual("de", ws2.StoreID);
				Assert.IsFalse(File.Exists(path));
				path = Path.Combine(environment.Collection.PathToWritingSystems, "de.ldml");
				Assert.IsTrue(File.Exists(path));
			}
		}

		[Test]
		public void MakesNewFileIfNeeded()
		{
			using (var environment = new TestEnvironment())
			{

				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.RFC5646)).HasAtLeastOneMatchForXpath("ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void CanAddVariantToLDMLUsingSameWS()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SaveDefinition(environment.WritingSystem);
				environment.WritingSystem.Variant = "1901";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.RFC5646)).HasAtLeastOneMatchForXpath("ldml/identity/variant[@type='1901']");
			}
		}

		[Test]
		public void CanAddVariantToExistingLDML()
		{
			using (var environment = new TestEnvironment())
			{

				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Abbreviation = "bl";
					//crucially, abbreviation isn't part of the name of the file
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get(environment.WritingSystem.StoreID);
				ws2.Variant = "x-piglatin";
				environment.Collection.SaveDefinition(ws2);
				string path = Path.Combine(environment.Collection.PathToWritingSystems,
										   environment.GetPathForWsId(ws2.RFC5646));
				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/identity/variant[@type='x-piglatin']");
				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/special/palaso:abbreviation[@value='bl']",
																	  environment.NamespaceManager);
			}
		}

		[Test]
		public void CanReadVariant()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Variant = "x-piglatin";
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get(environment.WritingSystem.StoreID);
				Assert.AreEqual("x-piglatin", ws2.Variant);
			}
		}

		[Test]
		public void CanSaveAndReadDefaultFont()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.DefaultFontName = "Courier";
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get("en");
				Assert.AreEqual("Courier", ws2.DefaultFontName);
			}
		}

		[Test]
		public void CanSaveAndReadKeyboardName()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Keyboard = "Thai";
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get("en");
				Assert.AreEqual("Thai", ws2.Keyboard);
			}
		}

		[Test]
		public void CanSaveAndReadRightToLeft()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				Assert.IsFalse(environment.WritingSystem.RightToLeftScript);
				environment.WritingSystem.RightToLeftScript = true;
				Assert.IsTrue(environment.WritingSystem.RightToLeftScript);
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get("en");
				Assert.IsTrue(ws2.RightToLeftScript);
			}
		}

		[Test]
		public void CanSaveAndReadIsUnicode()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				Assert.IsTrue(environment.WritingSystem.IsUnicodeEncoded);
				environment.WritingSystem.IsUnicodeEncoded = false;
				Assert.IsFalse(environment.WritingSystem.IsUnicodeEncoded);
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get("en");
				Assert.IsFalse(ws2.IsUnicodeEncoded);
			}
		}

		[Test]
		public void IsUnicodeEncoded_TrueByDefault()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get("en");
				Assert.IsTrue(ws2.IsUnicodeEncoded);
			}
		}

		[Test]
		public void CanRemoveVariant()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Variant = "x-piglatin";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = environment.GetPathForWsId(environment.WritingSystem.RFC5646);

				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/identity/variant");
				environment.WritingSystem.Variant = string.Empty;
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.RFC5646)).HasNoMatchForXpath("ldml/identity/variant");
			}
		}


		[Test]
		public void CanRemoveAbbreviation()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Abbreviation = "abbrev";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = environment.GetPathForWsId(environment.WritingSystem.RFC5646);
				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath(
					"ldml/special/palaso:abbreviation[@value='abbrev']",
					environment.NamespaceManager
				);
				environment.WritingSystem.Abbreviation = string.Empty;
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath(
					"ldml/special/palaso:abbreviation[@value='en']",
					environment.NamespaceManager
				);
			}
		}

		[Test]
		public void WritesAbbreviationToLDML()
		{

			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Abbreviation = "bl";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.RFC5646)).HasAtLeastOneMatchForXpath(
					"ldml/special/palaso:abbreviation[@value='bl']", environment.NamespaceManager);
			}
		}

		[Test]
		public void CanDeleteFileThatIsNotInTrash()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = Path.Combine(environment.Collection.PathToWritingSystems,
										   environment.GetPathForWsId(environment.WritingSystem.RFC5646));
				Assert.IsTrue(File.Exists(path));
				environment.Collection.Remove(environment.WritingSystem.Language);
				Assert.IsFalse(File.Exists(path));
				AssertFileIsInTrash(environment);
			}
		}

		private static void AssertFileIsInTrash(TestEnvironment environment)
		{
			string path = Path.Combine(environment.Collection.PathToWritingSystems, "trash");
			path = Path.Combine(path,environment.WritingSystem.RFC5646 + ".ldml");
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void CanDeleteFileMatchingOneThatWasPreviouslyTrashed()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				environment.Collection.Remove(environment.WritingSystem.StoreID);
				AssertFileIsInTrash(environment);
				var ws2 = new WritingSystemDefinition {Language = "en"};
				environment.Collection.SaveDefinition(ws2);
				environment.Collection.Remove(ws2.StoreID);
				string path = Path.Combine(environment.Collection.PathToWritingSystems,
										   environment.GetPathForWsId(environment.WritingSystem.RFC5646));
				Assert.IsFalse(File.Exists(path));
				AssertFileIsInTrash(environment);
			}
		}

		[Test]
		public void MarkedNotModifiedWhenNew()
		{
			using (var environment = new TestEnvironment())
			{
				//not worth saving until has some data
				Assert.IsFalse(environment.WritingSystem.Modified);
			}
		}

		[Test]
		public void MarkedAsModifiedWhenISOChanges()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				Assert.IsTrue(environment.WritingSystem.Modified);
			}
		}

		[Test]
		public void MarkedAsNotModifiedWhenLoaded()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get(environment.WritingSystem.StoreID);
				Assert.IsFalse(ws2.Modified);
			}
		}

		[Test]
		public void MarkedAsNotModifiedWhenSaved()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				Assert.IsTrue(environment.WritingSystem.Modified);
				environment.Collection.SaveDefinition(environment.WritingSystem);
				Assert.IsFalse(environment.WritingSystem.Modified);
				environment.WritingSystem.Language = "de";
				Assert.IsTrue(environment.WritingSystem.Modified);
			}
		}

		[Test]
		public void SystemWritingSystemProvider_Set_WritingSystemsAreIncludedInStore()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SystemWritingSystemProvider = new DummyWritingSystemProvider();
				IEnumerable<WritingSystemDefinition> list = environment.Collection.AllWritingSystems;
				Assert.IsTrue(ContainsLanguageWithName(list, "English"));
			}
		}

		[Test]
		public void DefaultLanguageNotAddedIfInTrash()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SystemWritingSystemProvider = new DummyWritingSystemProvider();
				IEnumerable<WritingSystemDefinition> list = environment.Collection.AllWritingSystems;
				Assert.IsTrue(ContainsLanguageWithName(list, "English"));
				IList<WritingSystemDefinition> list2 =
					new List<WritingSystemDefinition>(environment.Collection.AllWritingSystems);
				WritingSystemDefinition ws2 = list2[0];
				environment.Collection.Remove(ws2.Language);

				var repository = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				//  repository.DontAddDefaultDefinitions = false;
				repository.SystemWritingSystemProvider = new DummyWritingSystemProvider();
				Assert.IsFalse(ContainsLanguageWithName(repository.AllWritingSystems, "English"));
			}

		}

		[Test]
		public void Constructor_LdmlFolderStoreContainsMultipleFilesThatOnLoadDescribeWritingSystemsWithIdenticalRFC5646Tags_Throws()
		{
			using (var environment = new TestEnvironment())
			{
				File.WriteAllText(Path.Combine(environment.TestPath, "de-Zxxx-x-audio.ldml"),
								  LdmlContentForTests.Version0("de-Zxxx-x-audio", "", "", ""));
				File.WriteAllText(Path.Combine(environment.TestPath, "inconsistent-filename.ldml"),
								  LdmlContentForTests.Version0("de", WellKnownSubTags.Audio.Script, "",
													 WellKnownSubTags.Audio.PrivateUseSubtag));

				Assert.Throws<ApplicationException>(
					() => environment.Collection = new LdmlInFolderWritingSystemRepository(environment.TestPath)
				);

			}
		}

		[Test]
		//This is not really a problem, but it would be nice if the file were made consistant. So make we will make them run it through the migrator, which they should be using anyway.
		public void Constructor_LdmlFolderStoreContainsInconsistentlyNamedFile_Throws()
		{
			using (var environment = new TestEnvironment())
			{
				File.WriteAllText(Path.Combine(environment.TestPath, "tpi-Zxxx-x-audio.ldml"),
								  LdmlContentForTests.Version0("de", "latn", "ch", "1901"));
				Assert.Throws<ApplicationException>(
					() => new LdmlInFolderWritingSystemRepository(environment.TestPath)
				);
			}
		}

		[Test]
		public void Set_WritingSystemWasLoadedFromFlexPrivateUseLdmlAndRearranged_DoesNotChangeFileName()
		{
			using (var environment = new TestEnvironment())
			{
				var pathToFlexprivateUseLdml = Path.Combine(environment.TestPath, "x-en-Zxxx-x-audio.ldml");
				File.WriteAllText(pathToFlexprivateUseLdml,
								  LdmlContentForTests.Version0("x-en", "Zxxx", "", "x-audio"));
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				var ws = environment.Collection.Get("x-en-Zxxx-x-audio");
				environment.Collection.Set(ws);
				Assert.That(File.Exists(pathToFlexprivateUseLdml), Is.True);
			}
		}

		[Test]
		//this used to throw
		public void LoadAllDefinitions_FilenameDoesNotMatchRfc5646Tag_NoProblem()
		{
			using (var environment = new TestEnvironment())
			{
				//Make the filepath inconsistant
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				File.Move(Path.Combine(environment.TestPath, "en.ldml"), Path.Combine(environment.TestPath, "de.ldml"));

				// Now try to load up.
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				Assert.That(environment.Collection.Contains("en"));
			}
		}

		[Test]
		public void Get_WritingSystemContainedInFileWithfilenameThatDoesNotMatchRfc5646Tag_ReturnsWritingSystem()
		{
			using (var environment = new TestEnvironment())
			{
				//Make the filepath inconsistant
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				File.Move(Path.Combine(environment.TestPath, "en.ldml"), Path.Combine(environment.TestPath, "de.ldml"));

				// Now try to load up.
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				var ws = environment.Collection.Get("en");
				Assert.That(ws.RFC5646, Is.EqualTo("en"));
			}
		}

		[Test]
		public void LoadAllDefinitions_FilenameIsFlexConformPrivateUseAndDoesNotMatchRfc5646Tag_DoesNotThrow()
		{
			using (var environment = new TestEnvironment())
			{
				var ldmlPath = Path.Combine(environment.TestPath, "x-en-Zxxx.ldml");
				File.WriteAllText(ldmlPath, LdmlContentForTests.Version0("x-en", "Zxxx", "", ""));
				var repo = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);

				// Now try to load up.
				Assert.That(repo.Get("x-en-Zxxx").Language, Is.EqualTo("qaa"));
			}
		}

		[Test]
		public void Set_NewWritingSystem_StoreContainsWritingSystem()
		{
			using (var environment = new TestEnvironment())
			{
				var ws = new WritingSystemDefinition("en");
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				environment.Collection.Set(ws);
				Assert.That(environment.Collection.Get("en").RFC5646, Is.EqualTo("en"));
			}
		}

		[Test]
		public void SaveDefinition_WritingSystemCameFromFlexPrivateUseLdml_FileNameIsRetained()
		{
			using (var environment = new TestEnvironment())
			{
				var pathToFlexprivateUseLdml = Path.Combine(environment.TestPath, "x-Zxxx-x-audio.ldml");
				File.WriteAllText(pathToFlexprivateUseLdml, LdmlContentForTests.Version0("x", "Zxxx", "", "x-audio"));
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(DummyMigratorCallback.onMigration, environment.TestPath);
				var ws = environment.Collection.Get("x-Zxxx-x-audio");
				environment.Collection.SaveDefinition(ws);
				Assert.That(File.Exists(pathToFlexprivateUseLdml));
			}
		}

		private static bool ContainsLanguageWithName(IEnumerable<WritingSystemDefinition> list, string name)
		{
			return list.Any(definition => definition.LanguageName == name);
		}

		class DummyWritingSystemProvider : IEnumerable<WritingSystemDefinition>
		{

			public IEnumerator<WritingSystemDefinition> GetEnumerator()
			{
					yield return new WritingSystemDefinition("en", "", "", "", "", false);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

		}

	}

	internal class DummyMigratorCallback
	{
		public static void onMigration(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo)
		{
		}
	}

}