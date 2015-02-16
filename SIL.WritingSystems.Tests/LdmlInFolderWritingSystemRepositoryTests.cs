using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;
using SIL.Keyboarding;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class LdmlInFolderWritingSystemRepositoryInterfaceTests : WritingSystemRepositoryTests
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
			LdmlInFolderWritingSystemRepository repository = LdmlInFolderWritingSystemRepository.Initialize(testPath, Enumerable.Empty<ICustomDataMapper>(), DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
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
				Collection = LdmlInFolderWritingSystemRepository.Initialize(TestPath, Enumerable.Empty<ICustomDataMapper>(), DummyWritingSystemHandler.OnMigration, onLoadProblem);
			}

			private void onLoadProblem(IEnumerable<WritingSystemRepositoryProblem> problems)
			{
				throw new ApplicationException("Unexpected Writing System load problem in test.");
				// Currently there are no tests that expect load problems. If there are then we can
				// make the TestEnvironment suppress the exception above and make the problems available
				// to the test.
				//LoadProblems = problems;
			}

			//public IEnumerable<WritingSystemRepositoryProblem> LoadProblems { get; private set; }

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
		public void LatestVersion_IsThree()
		{
			Assert.AreEqual(3, WritingSystemDefinition.LatestWritingSystemDefinitionVersion);
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
				var ws2 = new WritingSystemDefinition
					{
						Language = "two"
					};
				environment.Collection.SaveDefinition(ws2);
				LdmlInFolderWritingSystemRepository newStore = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);

				Assert.AreEqual(2, newStore.Count);
			}
		}

		[Test]
		public void SavesWhenNotPreexisting()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SaveDefinition(environment.WritingSystem);
				environment.AssertWritingSystemFileExists(environment.WritingSystem.LanguageTag);
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
				var ws2 = environment.Collection.Get(environment.WritingSystem.StoreID);
				environment.Collection.SaveDefinition(ws2);
				environment.AssertWritingSystemFileExists(environment.WritingSystem.LanguageTag);
			}
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			using (var environment = new TestEnvironment())
			{
				string newRepoPath = Path.Combine(environment.TestPath, "newguy");
				LdmlInFolderWritingSystemRepository newRepository = LdmlInFolderWritingSystemRepository.Initialize(newRepoPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				newRepository.SaveDefinition(environment.WritingSystem);
				Assert.That(File.Exists(Path.Combine(newRepoPath, environment.WritingSystem.LanguageTag + ".ldml")));
			}
		}

		[Test]
		public void Save_WritingSystemIdChanged_ChangeLogUpdated()
		{
			using (var e = new TestEnvironment())
			{
				LdmlInFolderWritingSystemRepository repo = LdmlInFolderWritingSystemRepository.Initialize(Path.Combine(e.TestPath, "idchangedtest1"), Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				var ws = new WritingSystemDefinition("en");
				repo.Set(ws);
				repo.Save();

				ws.Script = "Latn";
				repo.Set(ws);
				ws.Script = "Thai";
				repo.Set(ws);

				var ws2 = new WritingSystemDefinition("de");
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
		public void Save_WritingSystemIdConflated_ChangeLogUpdatedAndDoesNotContainDelete()
		{
			using (var e = new TestEnvironment())
			{
				LdmlInFolderWritingSystemRepository repo = LdmlInFolderWritingSystemRepository.Initialize(Path.Combine(e.TestPath, "idchangedtest1"), Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				var ws = new WritingSystemDefinition("en");
				repo.Set(ws);
				repo.Save();

				var ws2 = new WritingSystemDefinition("de");
				repo.Set(ws2);
				repo.Save();

				repo.Conflate(ws.Id, ws2.Id);
				repo.Save();

				string logFilePath = Path.Combine(repo.PathToWritingSystems, "idchangelog.xml");
				AssertThatXmlIn.File(logFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Merge/From[text()='en']");
				AssertThatXmlIn.File(logFilePath).HasAtLeastOneMatchForXpath("/WritingSystemChangeLog/Changes/Merge/To[text()='de']");
				AssertThatXmlIn.File(logFilePath).HasNoMatchForXpath("/WritingSystemChangeLog/Changes/Delete/Id[text()='en']");
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
				LdmlInFolderWritingSystemRepository newStore = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				WritingSystemDefinition ws2 = newStore.Get("en");
				Assert.AreEqual(
					Path.GetFileNameWithoutExtension(Directory.GetFiles(environment.TestPath, "*.ldml")[0]), ws2.StoreID);
			}
		}

		[Test]
		public void UpdatesFileNameWhenIsoChanges()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = Path.Combine(environment.Collection.PathToWritingSystems, "en.ldml");
				Assert.IsTrue(File.Exists(path));
				var ws2 = environment.Collection.Get(environment.WritingSystem.Id);
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
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.LanguageTag)).HasAtLeastOneMatchForXpath("ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void CanAddVariantToLdmlUsingSameWS()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SaveDefinition(environment.WritingSystem);
				environment.WritingSystem.Variants.Add("1901");
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.LanguageTag)).HasAtLeastOneMatchForXpath("ldml/identity/variant[@type='1901']");
			}
		}

		[Test]
		public void CanAddVariantToExistingLdml()
		{
			using (var environment = new TestEnvironment())
			{

				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Abbreviation = "bl";
					//crucially, abbreviation isn't part of the name of the file
				environment.Collection.SaveDefinition(environment.WritingSystem);

				LdmlInFolderWritingSystemRepository newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				WritingSystemDefinition ws2 = newCollection.Get(environment.WritingSystem.StoreID);
				ws2.Variants.Add("piglatin");
				environment.Collection.SaveDefinition(ws2);
				string path = Path.Combine(environment.Collection.PathToWritingSystems,
										   environment.GetPathForWsId(ws2.LanguageTag));
				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/identity/variant[@type='x-piglatin']");

				// TODO: Add this back when Abbreviation is written to application-specific namespace
#if WS_FIX
				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/special/palaso:abbreviation[@value='bl']",
																	  environment.NamespaceManager);
#endif
			}
		}

		[Test]
		public void CanReadVariant()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Variants.Add("piglatin");
				environment.Collection.SaveDefinition(environment.WritingSystem);

				LdmlInFolderWritingSystemRepository newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				WritingSystemDefinition ws2 = newCollection.Get(environment.WritingSystem.StoreID);
				Assert.That(ws2.Variants, Is.EqualTo(new VariantSubtag[] {"piglatin"}));
			}
		}

		// TODO: Add this when DefaultFontName is written to application-specific
#if WS_FIX
		[Test]
		public void CanSaveAndReadDefaultFont()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.DefaultFontName = "Courier";
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				var ws2 = newCollection.Get("en");
				Assert.AreEqual("Courier", ws2.DefaultFontName);
			}
		}
#endif

		[Test]
		public void CanSaveAndReadKeyboardId()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				var kbd1 = new DefaultKeyboardDefinition("Thai", "Thai");
				kbd1.Format = KeyboardFormat.Msklc;
				environment.WritingSystem.KnownKeyboards.Add(kbd1);
				environment.Collection.SaveDefinition(environment.WritingSystem);

				LdmlInFolderWritingSystemRepository newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				WritingSystemDefinition ws2 = newCollection.Get("en");
				Assert.AreEqual("Thai", ws2.KnownKeyboards[0].Id);
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

				LdmlInFolderWritingSystemRepository newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				var ws2 = newCollection.Get("en");
				Assert.IsTrue(ws2.RightToLeftScript);
			}
		}

		// TODO: Does IsUnicodeEncoded go away or get put in application-specific?
#if WS_FIX
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

				var newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				var ws2 = newCollection.Get("en");
				Assert.IsFalse(ws2.IsUnicodeEncoded);
			}
		}
#endif

		[Test]
		public void CanRemoveVariant()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Variants.Add("piglatin");
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = environment.GetPathForWsId(environment.WritingSystem.LanguageTag);

				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/identity/variant");
				environment.WritingSystem.Variants.Clear();
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.LanguageTag)).HasNoMatchForXpath("ldml/identity/variant");
			}
		}

		// TODO: Abbreviation to go in application-specific
#if WS_FIX
		[Test]
		public void CanRemoveAbbreviation()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Abbreviation = "abbrev";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = environment.GetPathForWsId(environment.WritingSystem.LanguageTag);
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
		public void WritesAbbreviationToLdml()
		{

			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Abbreviation = "bl";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(environment.GetPathForWsId(environment.WritingSystem.LanguageTag)).HasAtLeastOneMatchForXpath(
					"ldml/special/palaso:abbreviation[@value='bl']", environment.NamespaceManager);
			}
		}
#endif

		[Test]
		public void CanDeleteFileThatIsNotInTrash()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = Path.Combine(environment.Collection.PathToWritingSystems,
										   environment.GetPathForWsId(environment.WritingSystem.LanguageTag));
				Assert.IsTrue(File.Exists(path));
				environment.Collection.Remove(environment.WritingSystem.Language);
				Assert.IsFalse(File.Exists(path));
				AssertFileIsInTrash(environment);
			}
		}

		private static void AssertFileIsInTrash(TestEnvironment environment)
		{
			string path = Path.Combine(environment.Collection.PathToWritingSystems, "trash");
			path = Path.Combine(path,environment.WritingSystem.LanguageTag + ".ldml");
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
										   environment.GetPathForWsId(environment.WritingSystem.LanguageTag));
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
				Assert.IsFalse(environment.WritingSystem.IsChanged);
			}
		}

		[Test]
		public void MarkedAsModifiedWhenIsoChanges()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				Assert.IsTrue(environment.WritingSystem.IsChanged);
			}
		}

		[Test]
		public void MarkedAsNotModifiedWhenLoaded()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				LdmlInFolderWritingSystemRepository newCollection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				WritingSystemDefinition ws2 = newCollection.Get(environment.WritingSystem.StoreID);
				Assert.IsFalse(ws2.IsChanged);
			}
		}

		[Test]
		public void MarkedAsNotModifiedWhenSaved()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				Assert.IsTrue(environment.WritingSystem.IsChanged);
				environment.Collection.SaveDefinition(environment.WritingSystem);
				Assert.IsFalse(environment.WritingSystem.IsChanged);
				environment.WritingSystem.Language = "de";
				Assert.IsTrue(environment.WritingSystem.IsChanged);
			}
		}

		[Test]
		public void SystemWritingSystemProvider_Set_WritingSystemsAreIncludedInStore()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SystemWritingSystemProvider = new DummyWritingSystemProvider();
				var list = environment.Collection.AllWritingSystems;
				Assert.IsTrue(ContainsLanguageWithName(list, "English"));
			}
		}

		[Test]
		public void DefaultLanguageNotAddedIfInTrash()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SystemWritingSystemProvider = new DummyWritingSystemProvider();
				var list = environment.Collection.AllWritingSystems;
				Assert.IsTrue(ContainsLanguageWithName(list, "English"));
				var list2 = new List<WritingSystemDefinition>(environment.Collection.AllWritingSystems);
				WritingSystemDefinition ws2 = list2[0];
				environment.Collection.Remove(ws2.Language);

				LdmlInFolderWritingSystemRepository repository = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
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
								  LdmlContentForTests.CurrentVersion("de", WellKnownSubtags.UnwrittenScript, "", "x-audio"));
				File.WriteAllText(Path.Combine(environment.TestPath, "inconsistent-filename.ldml"),
								  LdmlContentForTests.CurrentVersion("de", WellKnownSubtags.UnwrittenScript, "", "x-audio"));

				var repository = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				IList<WritingSystemRepositoryProblem> problems = repository.LoadProblems;

				Assert.That(problems.Count, Is.EqualTo(2));
				Assert.That(
					problems[0].Exception,
					Is.TypeOf<ApplicationException>().With.Property("Message").
					ContainsSubstring(String.Format(
						@"The writing system file {0} seems to be named inconsistently. It contains the Rfc5646 tag: 'de-Zxxx-x-audio'. The name should have been made consistent with its content upon migration of the writing systems.",
						Path.Combine(environment.TestPath, "inconsistent-filename.ldml")
					))
				);
				Assert.That(
					problems[1].Exception,
					Is.TypeOf<ArgumentException>().With.Property("Message").
					ContainsSubstring("Unable to set writing system 'de-Zxxx-x-audio' because this id already exists. Please change this writing system id before setting it.")
				);

			}
		}

		[Test]
		public void Conflate_ChangelogRecordsChange()
		{
			using(var e = new TestEnvironment())
			{
				e.Collection.Set(new WritingSystemDefinition("de"));
				e.Collection.Set(new WritingSystemDefinition("en"));
				e.Collection.Conflate("de", "en");
				Assert.That(e.Collection.WritingSystemIdHasChangedTo("de"), Is.EqualTo("en"));
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
					ContainsSubstring(String.Format(
						@"The writing system file {0} seems to be named inconsistently. It contains the Rfc5646 tag: 'de-latn-ch-1901'. The name should have been made consistent with its content upon migration of the writing systems.",
						Path.Combine(environment.TestPath, "tpi-Zxxx-x-audio.ldml")
					))
				);
			}
		}

		[Test]
		public void Constructor_LdmlFolderStoreContainsInconsistentlyNamedFileDifferingInCaseOnly_HasNoProblem()
		{
			using (var environment = new TestEnvironment())
			{
				File.WriteAllText(Path.Combine(environment.TestPath, "tpi-latn.ldml"),
								  LdmlContentForTests.CurrentVersion("tpi", "Latn", "", ""));

				var repository = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				var problems = repository.LoadProblems;
				Assert.That(problems.Count, Is.EqualTo(0));
			}
		}

		// TODO: Add when migrating FlexPrivateUse
#if WS_FIX
		[Test]
		public void Set_WritingSystemWasLoadedFromFlexPrivateUseLdmlAndRearranged_DoesNotChangeFileName()
		{
			using (var environment = new TestEnvironment())
			{
				var pathToFlexprivateUseLdml = Path.Combine(environment.TestPath, "x-en-Zxxx-x-audio.ldml");
				File.WriteAllText(pathToFlexprivateUseLdml,
								  LdmlContentForTests.Version0("x-en", "Zxxx", "", "x-audio"));
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem, WritingSystemCompatibility.Flex7V0Compatible);
				var ws = environment.Collection.Get("x-en-Zxxx-x-audio");
				environment.Collection.Set(ws);
				Assert.That(File.Exists(pathToFlexprivateUseLdml), Is.True);
			}
		}
#endif

		[Test]
		//this used to throw
		public void LoadAllDefinitions_FilenameDoesNotMatchRfc5646Tag_NoProblem()
		{
			using (var environment = new TestEnvironment())
			{
				//Make the filepath inconsistant
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				File.Move(Path.Combine(environment.TestPath, "en.ldml"), Path.Combine(environment.TestPath, "de.ldml"));

				// Now try to load up.
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				Assert.That(environment.Collection.Contains("en"));
			}
		}

		[Test]
		public void Get_WritingSystemContainedInFileWithfilenameThatDoesNotMatchRfc5646Tag_ReturnsWritingSystem()
		{
			using (var environment = new TestEnvironment())
			{
				//Make the filepath inconsistant
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				environment.WritingSystem.Language = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				File.Move(Path.Combine(environment.TestPath, "en.ldml"), Path.Combine(environment.TestPath, "de.ldml"));

				// Now try to load up.
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				var ws = environment.Collection.Get("en");
				Assert.That(ws.LanguageTag, Is.EqualTo("en"));
			}
		}

#if WS_FIX
		[Test]
		public void LoadAllDefinitions_FilenameIsFlexConformPrivateUseAndDoesNotMatchRfc5646TagWithLegacySupport_DoesNotThrow()
		{
			using (var environment = new TestEnvironment())
			{
				var ldmlPath = Path.Combine(environment.TestPath, "x-en-Zxxx.ldml");
				File.WriteAllText(ldmlPath, LdmlContentForTests.Version0("x-en", "Zxxx", "", ""));
				var repo = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem, WritingSystemCompatibility.Flex7V0Compatible);

				// Now try to load up.
				Assert.That(repo.Get("x-en-Zxxx").Language, Is.EqualTo(new LanguageSubtag("en", true)));
			}
		}
#endif

		[Test]
		public void LoadAllDefinitions_FilenameIsFlexConformPrivateUseAndDoesNotMatchRfc5646Tag_Migrates()
		{
			using (var environment = new TestEnvironment())
			{
				var ldmlPath = Path.Combine(environment.TestPath, "x-en-Zxxx.ldml");
				File.WriteAllText(ldmlPath, LdmlContentForTests.Version0("x-en", "Zxxx", "", ""));
				LdmlInFolderWritingSystemRepository repo = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem, WritingSystemCompatibility.Strict);

				// Now try to load up.
				Assert.That(repo.Get("qaa-Zxxx-x-en").Language, Is.EqualTo(new LanguageSubtag("en", true)));
			}
		}

		[Test]
		public void Set_NewWritingSystem_StoreContainsWritingSystem()
		{
			using (var environment = new TestEnvironment())
			{
				var ws = new WritingSystemDefinition("en");
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, Enumerable.Empty<ICustomDataMapper>(),
					DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem);
				environment.Collection.Set(ws);
				Assert.That(environment.Collection.Get("en").LanguageTag, Is.EqualTo("en"));
			}
		}

#if WS_FIX
		[Test]
		public void SaveDefinition_WritingSystemCameFromFlexPrivateUseLdml_FileNameIsRetained()
		{
			using (var environment = new TestEnvironment())
			{
				var pathToFlexprivateUseLdml = Path.Combine(environment.TestPath, "x-Zxxx-x-audio.ldml");
				File.WriteAllText(pathToFlexprivateUseLdml, LdmlContentForTests.Version0("x", "Zxxx", "", "x-audio"));
				environment.Collection = LdmlInFolderWritingSystemRepository.Initialize(environment.TestPath, DummyWritingSystemHandler.OnMigration, DummyWritingSystemHandler.OnLoadProblem, WritingSystemCompatibility.Flex7V0Compatible);
				var ws = environment.Collection.Get("x-Zxxx-x-audio");
				environment.Collection.SaveDefinition(ws);
				Assert.That(File.Exists(pathToFlexprivateUseLdml));
			}
		}
#endif

		[Test]
		public void SaveDefinition_WritingSystemCameFromValidRfc5646WritingSystemStartingWithX_FileNameIsChanged()
		{
			using (var environment = new TestEnvironment())
			{
				var pathToFlexprivateUseLdml = Path.Combine(environment.TestPath, "x-Zxxx-x-audio.ldml");
				File.WriteAllText(pathToFlexprivateUseLdml, LdmlContentForTests.CurrentVersion("xh", "", "", ""));
				environment.Collection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				Assert.That(File.Exists(Path.Combine(environment.TestPath, "xh.ldml")));
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
				ws.Variants.Add("bogus");
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
				wsEn.Variants.Add("bogus");
				environment.Collection.Set(wsEn);
				var wsEnDup = new WritingSystemDefinition("en");
				environment.Collection.Set(wsEnDup);
				environment.Collection.Save();
				//Now change the duplicate's Id as well
				wsEnDup.Variants.Add("bogus2");
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
				ws.Variants.Add("bogus");
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
				wsEn.Variants.Add("bogus");
				environment.Collection.Set(wsEn);
				var wsEnDup = new WritingSystemDefinition("en");
				environment.Collection.Set(wsEnDup);
				environment.Collection.Save();
				//Now change the duplicate's Id as well
				wsEnDup.Variants.Add("bogus2");
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
					ContainsSubstring(String.Format(
						"The LDML tag 'en' is version 0.  Version {0} was expected.",
						WritingSystemDefinition.LatestWritingSystemDefinitionVersion
					))
				);
			}
		}

		[Test]
		public void LoadDefinitions_ValidLanguageTagStartingWithXButV0_Throws()
		{
			using (var environment = new TestEnvironment())
			{
				var pathToFlexprivateUseLdml = Path.Combine(environment.TestPath, "xh.ldml");
				File.WriteAllText(pathToFlexprivateUseLdml, LdmlContentForTests.Version0("xh", "", "", ""));
				var repository = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				var problems = repository.LoadProblems;

				Assert.That(problems.Count, Is.EqualTo(1));
				Assert.That(
					problems[0].Exception,
					Is.TypeOf<ApplicationException>().With.Property("Message").
					ContainsSubstring(String.Format(
						"The LDML tag 'xh' is version 0.  Version {0} was expected.",
						WritingSystemDefinition.LatestWritingSystemDefinitionVersion
					))
				);
			}
		}

		private static bool ContainsLanguageWithName(IEnumerable<WritingSystemDefinition> list, string name)
		{
			return list.Any(definition => definition.Language.Name == name);
		}

		class DummyWritingSystemProvider : IEnumerable<WritingSystemDefinition>
		{

			public IEnumerator<WritingSystemDefinition> GetEnumerator()
			{
				yield return new WritingSystemDefinition("en", "", "", "");
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

		}

	}

	internal class DummyWritingSystemHandler
	{
		public static void OnMigration(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo)
		{
		}

		public static void OnLoadProblem(IEnumerable<WritingSystemRepositoryProblem> problems)
		{
		}

	}

}