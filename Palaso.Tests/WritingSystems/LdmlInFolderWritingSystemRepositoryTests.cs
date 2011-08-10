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
			LdmlInFolderWritingSystemRepository repository = LdmlInFolderWritingSystemRepository.Initialize(testPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
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
				Collection = LdmlInFolderWritingSystemRepository.Initialize(TestPath, DummyWritingSystemHandler.onMigration, onLoadProblem);
			}

			public IEnumerable<WritingSystemRepositoryProblem> LoadProblems { get; private set; }

			private void onLoadProblem(IEnumerable<WritingSystemRepositoryProblem> problems)
			{
				LoadProblems = problems;
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
				var newStore = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);

				Assert.AreEqual(2, newStore.Count);
			}
		}

		[Test]
		public void SavesWhenNotPreexisting()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SaveDefinition(environment.WritingSystem);
				environment.AssertWritingSystemFileExists(environment.WritingSystem.Bcp47Tag);
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
				environment.AssertWritingSystemFileExists(environment.WritingSystem.Bcp47Tag);
			}
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			using (var environment = new TestEnvironment())
			{
				var newRepoPath = Path.Combine(environment.TestPath, "newguy");
				var newRepository = LdmlInFolderWritingSystemRepository.Initialize(newRepoPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
				newRepository.SaveDefinition(environment.WritingSystem);
				Assert.That(File.Exists(Path.Combine(newRepoPath, environment.WritingSystem.Bcp47Tag + ".ldml")));
			}
		}

		[Test]
		public void Save_WritingSystemIdChanged_ChangeLogUpdated()
		{
			using (var e = new TestEnvironment())
			{
				var repo = LdmlInFolderWritingSystemRepository.Initialize(Path.Combine(e.TestPath, "idchangedtest1"), DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
				var ws = WritingSystemDefinition.Parse("en");
				repo.Set(ws);
				repo.Save();

				ws.Script = "Latn";
				repo.Set(ws);
				ws.Script = "Thai";
				repo.Set(ws);

				var ws2 = WritingSystemDefinition.Parse("de");
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
				var newStore = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
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
				WritingSystemDefinition ws2 = environment.Collection.Get(environment.WritingSystem.Id);
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
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.Bcp47Tag)).HasAtLeastOneMatchForXpath("ldml/identity/language[@type='en']");
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
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.Bcp47Tag)).HasAtLeastOneMatchForXpath("ldml/identity/variant[@type='1901']");
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

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
				WritingSystemDefinition ws2 = newCollection.Get(environment.WritingSystem.StoreID);
				ws2.Variant = "x-piglatin";
				environment.Collection.SaveDefinition(ws2);
				string path = Path.Combine(environment.Collection.PathToWritingSystems,
										   environment.GetPathForWsId(ws2.Bcp47Tag));
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

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
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

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
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

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
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

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
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

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
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

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
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
				string path = environment.GetPathForWsId(environment.WritingSystem.Bcp47Tag);

				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/identity/variant");
				environment.WritingSystem.Variant = string.Empty;
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.Bcp47Tag)).HasNoMatchForXpath("ldml/identity/variant");
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
				string path = environment.GetPathForWsId(environment.WritingSystem.Bcp47Tag);
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
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.Bcp47Tag)).HasAtLeastOneMatchForXpath(
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
										   environment.GetPathForWsId(environment.WritingSystem.Bcp47Tag));
				Assert.IsTrue(File.Exists(path));
				environment.Collection.Remove(environment.WritingSystem.Language);
				Assert.IsFalse(File.Exists(path));
				AssertFileIsInTrash(environment);
			}
		}

		private static void AssertFileIsInTrash(TestEnvironment environment)
		{
			string path = Path.Combine(environment.Collection.PathToWritingSystems, "trash");
			path = Path.Combine(path,environment.WritingSystem.Bcp47Tag + ".ldml");
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
										   environment.GetPathForWsId(environment.WritingSystem.Bcp47Tag));
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
				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
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

				var repository = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
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
								  LdmlContentForTests.CurrentVersion("de", WellKnownSubTags.Audio.Script, "",
													 WellKnownSubTags.Audio.PrivateUseSubtag));
				File.WriteAllText(Path.Combine(environment.TestPath, "inconsistent-filename.ldml"),
								  LdmlContentForTests.CurrentVersion("de", WellKnownSubTags.Audio.Script, "",
													 WellKnownSubTags.Audio.PrivateUseSubtag));

				var repository = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				var problems = repository.LoadProblems;

				Assert.That(problems.Count, Is.EqualTo(2));
				Assert.That(
					problems[0].Exception,
					Is.TypeOf<ApplicationException>().With.Property("Message").
					ContainsSubstring(@"The writing system file C:\Users\PaxErit\AppData\Local\Temp\LdmlInFolderWritingSystemRepositoryTests\inconsistent-filename.ldml seems to be named inconsistently. It conatins the Rfc5646 tag: 'de-Zxxx-x-audio'. The name should have been made consistent with its content upon migration of the writing systems.")
				);
				Assert.That(
					problems[1].Exception,
					Is.TypeOf<ArgumentException>().With.Property("Message").
					ContainsSubstring("Unable to set writing system 'de-Zxxx-x-audio' because this id already exists. Please change this writing system id before storing setting it.")
				);

			}
		}

		[Test]
		//This is not really a problem, but it would be nice if the file were made consistant. So make we will make them run it through the migrator, which they should be using anyway.
		public void Constructor_LdmlFolderStoreContainsInconsistentlyNamedFile_HasExpectedProblem()
		{
			using (var environment = new TestEnvironment())
			{
				File.WriteAllText(Path.Combine(environment.TestPath, "tpi-Zxxx-x-audio.ldml"),
								  LdmlContentForTests.CurrentVersion("de", "latn", "ch", "1901"));

				var repository = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				var problems = repository.LoadProblems;

				Assert.That(problems.Count, Is.EqualTo(1));
				Assert.That(
					problems[0].Exception,
					Is.TypeOf<ApplicationException>().With.Property("Message").
					ContainsSubstring(@"The writing system file C:\Users\PaxErit\AppData\Local\Temp\LdmlInFolderWritingSystemRepositoryTests\tpi-Zxxx-x-audio.ldml seems to be named inconsistently. It conatins the Rfc5646 tag: 'de-latn-ch-1901'. The name should have been made consistent with its content upon migration of the writing systems.")
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
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
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
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				File.Move(Path.Combine(environment.TestPath, "en.ldml"), Path.Combine(environment.TestPath, "de.ldml"));

				// Now try to load up.
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
				Assert.That(environment.Collection.Contains("en"));
			}
		}

		[Test]
		public void Get_WritingSystemContainedInFileWithfilenameThatDoesNotMatchRfc5646Tag_ReturnsWritingSystem()
		{
			using (var environment = new TestEnvironment())
			{
				//Make the filepath inconsistant
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				File.Move(Path.Combine(environment.TestPath, "en.ldml"), Path.Combine(environment.TestPath, "de.ldml"));

				// Now try to load up.
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
				var ws = environment.Collection.Get("en");
				Assert.That(ws.Bcp47Tag, Is.EqualTo("en"));
			}
		}

		[Test]
		public void LoadAllDefinitions_FilenameIsFlexConformPrivateUseAndDoesNotMatchRfc5646Tag_DoesNotThrow()
		{
			using (var environment = new TestEnvironment())
			{
				var ldmlPath = Path.Combine(environment.TestPath, "x-en-Zxxx.ldml");
				File.WriteAllText(ldmlPath, LdmlContentForTests.Version0("x-en", "Zxxx", "", ""));
				var repo = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);

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
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
				environment.Collection.Set(ws);
				Assert.That(environment.Collection.Get("en").Bcp47Tag, Is.EqualTo("en"));
			}
		}

		[Test]
		public void SaveDefinition_WritingSystemCameFromFlexPrivateUseLdml_FileNameIsRetained()
		{
			using (var environment = new TestEnvironment())
			{
				var pathToFlexprivateUseLdml = Path.Combine(environment.TestPath, "x-Zxxx-x-audio.ldml");
				File.WriteAllText(pathToFlexprivateUseLdml, LdmlContentForTests.Version0("x", "Zxxx", "", "x-audio"));
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.onMigration, DummyWritingSystemHandler.onLoadProblem);
				var ws = environment.Collection.Get("x-Zxxx-x-audio");
				environment.Collection.SaveDefinition(ws);
				Assert.That(File.Exists(pathToFlexprivateUseLdml));
			}
		}

		[Test]
		public void WritingSystemIdHasBeenChanged_IdNeverExisted_ReturnsFalse()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				Assert.That(environment.Collection.WritingSystemIdHasChanged("en"), Is.False);
			}
		}

		[Test]
		public void WritingSystemIdHasBeenChanged_IdChanged_ReturnsTrue()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var ws = new WritingSystemDefinition("en");
				environment.Collection.Set(ws);
				environment.Collection.Save();
				//Now change the Id
				ws.Variant = "x-bogus";
				environment.Collection.Save();
				Assert.That(environment.Collection.WritingSystemIdHasChanged("en"), Is.True);
			}
		}

		[Test]
		public void WritingSystemIdHasBeenChanged_IdChangedToMultipleDifferentNewIds_ReturnsTrue()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var wsEn = new WritingSystemDefinition("en");
				environment.Collection.Set(wsEn);
				environment.Collection.Save();
				//Now change the Id and create a duplicate of the original Id
				wsEn.Variant = "x-bogus";
				environment.Collection.Set(wsEn);
				var wsEnDup = new WritingSystemDefinition("en");
				environment.Collection.Set(wsEnDup);
				environment.Collection.Save();
				//Now change the duplicate's Id as well
				wsEnDup.Variant = "x-bogus2";
				environment.Collection.Set(wsEnDup);
				environment.Collection.Save();
				Assert.That(environment.Collection.WritingSystemIdHasChanged("en"), Is.True);
			}
		}

		[Test]
		public void WritingSystemIdHasBeenChanged_IdExistsAndHasNeverChanged_ReturnsFalse()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var ws = new WritingSystemDefinition("en");
				environment.Collection.Set(ws);
				environment.Collection.Save();
				Assert.That(environment.Collection.WritingSystemIdHasChanged("en"), Is.False);
			}
		}

		[Test]
		public void WritingSystemIdHasChangedTo_IdNeverExisted_ReturnsNull()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				Assert.That(environment.Collection.WritingSystemIdHasChangedTo("en"), Is.Null);
			}
		}

		[Test]
		public void WritingSystemIdHasChangedTo_IdChanged_ReturnsNewId()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var ws = new WritingSystemDefinition("en");
				environment.Collection.Set(ws);
				environment.Collection.Save();
				//Now change the Id
				ws.Variant = "x-bogus";
				environment.Collection.Save();
				Assert.That(environment.Collection.WritingSystemIdHasChangedTo("en"), Is.EqualTo("en-x-bogus"));
			}
		}

		[Test]
		public void WritingSystemIdHasChangedTo_IdChangedToMultipleDifferentNewIds_ReturnsNull()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var wsEn = new WritingSystemDefinition("en");
				environment.Collection.Set(wsEn);
				environment.Collection.Save();
				//Now change the Id and create a duplicate of the original Id
				wsEn.Variant = "x-bogus";
				environment.Collection.Set(wsEn);
				var wsEnDup = new WritingSystemDefinition("en");
				environment.Collection.Set(wsEnDup);
				environment.Collection.Save();
				//Now change the duplicate's Id as well
				wsEnDup.Variant = "x-bogus2";
				environment.Collection.Set(wsEnDup);
				environment.Collection.Save();
				Assert.That(environment.Collection.WritingSystemIdHasChangedTo("en"), Is.Null);
			}
		}

		[Test]
		public void WritingSystemIdHasChangedTo_IdExistsAndHasNeverChanged_ReturnsId()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var ws = new WritingSystemDefinition("en");
				environment.Collection.Set(ws);
				environment.Collection.Save();
				Assert.That(environment.Collection.WritingSystemIdHasChangedTo("en"), Is.EqualTo("en"));
			}
		}

		[Test]
		//This test checks that "old" ldml files are not overwritten on Save before they can be used to roundtrip unknown ldml (i.e. from flex)
		public void Save_IdOfWsIsSameAsOldIdOfOtherWs_LdmlUnknownToRepoIsMaintained()
		{
			using (var environment = new TestEnvironment())
			{
				string germanFromFlex =
#region fileContent
 @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='' />
		<generation date='2010-12-02T23:05:33' />
		<language type='de' />
	</identity>
	<collations />
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation value='de' />
		<palaso:defaultFontFamily value='FieldWorks Test SIL' />
		<palaso:defaultKeyboard value='FwTest' />
		<palaso:languageName value='German' />
		<palaso:spellCheckingId value='de' />
		<palaso:version value='1'/>
	</special>
	<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1'>
		<fw:graphiteEnabled value='False' />
		<fw:windowsLCID value='1058' />
	</special>
</ldml>".Replace("'", "\"");
#endregion
				var pathForFlexGerman = Path.Combine(environment.TestPath, "de.ldml");
				environment.Collection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				var ws1 = new WritingSystemDefinition("en");
				var ws2 = new WritingSystemDefinition("de");
				//Create repo with english and flex german
				environment.Collection.Set(ws1);
				environment.Collection.Set(ws2);
				environment.Collection.Save();
				//The content of the file is switched out here as opposed to loading from this content in the first place
				//because order is extremely important for this test and if we loaded from this ldml "de" would be the
				//first writing system in the repo rather than the second.
				File.WriteAllText(pathForFlexGerman, germanFromFlex);
				//rename the ws
				ws1.Language = "de";
				ws2.Language = "fr";
				environment.Collection.Set(ws2);
				environment.Collection.Set(ws1);
				environment.Collection.Save();

				pathForFlexGerman = Path.Combine(environment.TestPath, "fr.ldml");
				var manager = new XmlNamespaceManager(new NameTable());
				manager.AddNamespace("fw", "urn://fieldworks.sil.org/ldmlExtensions/v1");
				AssertThatXmlIn.File(pathForFlexGerman).HasAtLeastOneMatchForXpath("/ldml/special/fw:graphiteEnabled", manager);
			}
		}

		[Test]
		public void Save_IdOfWsIsSameAsOldIdOfOtherWs_RepoHasCorrectNumberOfWritingSystemsOnLoad()
		{
			using (var environment = new TestEnvironment())
			{
				var pathForFlexGerman = Path.Combine(environment.TestPath, "de.ldml");
				environment.Collection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				var ws1 = new WritingSystemDefinition("en");
				var ws2 = new WritingSystemDefinition("de");
				environment.Collection.Set(ws1);
				environment.Collection.Set(ws2);
				environment.Collection.Save();
				//rename the ws
				ws1.Language = "de";
				ws2.Language = "fr";
				environment.Collection.Set(ws2);
				environment.Collection.Set(ws1);
				environment.Collection.Save();
				environment.Collection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				Assert.That(environment.Collection.Count, Is.EqualTo(2));
			}
		}

		[Test]
		public void LoadAllDefinitions_LDMLV0_HasExpectedProblem()
		{
			using (var environment = new TestEnvironment())
			{
				var ldmlPath = Path.Combine(environment.TestPath, "en.ldml");
				File.WriteAllText(ldmlPath, LdmlContentForTests.Version0("en", "", "", ""));

				var repository = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				var problems = repository.LoadProblems;

				Assert.That(problems.Count, Is.EqualTo(1));
				Assert.That(
					problems[0].Exception,
					Is.TypeOf<ApplicationException>().With.Property("Message").
					ContainsSubstring("The LDML tag 'en' is version 0.  Version 1 was expected.")
				);
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

	internal class DummyWritingSystemHandler
	{
		public static void onMigration(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo)
		{
		}

		public static void onLoadProblem(IEnumerable<WritingSystemRepositoryProblem> problems)
		{
		}

	}

}