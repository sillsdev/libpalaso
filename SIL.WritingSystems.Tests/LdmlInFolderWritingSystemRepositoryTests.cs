using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.Keyboarding;
using SIL.TestUtilities;

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
			LdmlInFolderWritingSystemRepository repository = LdmlInFolderWritingSystemRepository.Initialize(testPath);
			//repository.DontAddDefaultDefinitions = true;
			return repository;
		}
	}

	[TestFixture]
	public class LdmlInFolderWritingSystemRepositoryTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly TemporaryFolder _localRepoFolder;
			private readonly WritingSystemDefinition _writingSystem;
			private readonly TemporaryFolder _templateFolder;
			private readonly TemporaryFolder _globalRepoFolder;
			private readonly TestWritingSystemCustomDataMapper _writingSystemCustomDataMapper;

			public TestEnvironment()
			{
				_localRepoFolder = new TemporaryFolder("LdmlInFolderWritingSystemRepositoryTests");
				_templateFolder = new TemporaryFolder("Templates");
				_globalRepoFolder = new TemporaryFolder("GlobalWritingSystemRepository");
				_writingSystem = new WritingSystemDefinition();
				_writingSystemCustomDataMapper = new TestWritingSystemCustomDataMapper();
				ResetRepositories();
			}

			public void ResetRepositories()
			{
				if (GlobalRepository != null)
					GlobalRepository.Dispose();
				GlobalRepository = new GlobalWritingSystemRepository(_globalRepoFolder.Path);
				LocalRepository = new TestLdmlInFolderWritingSystemRepository(_localRepoFolder.Path, new[] {_writingSystemCustomDataMapper}, GlobalRepository);
				LocalRepository.WritingSystemFactory.TemplateFolder = _templateFolder.Path;
			}

			public void Dispose()
			{
				GlobalRepository.Dispose();
				_globalRepoFolder.Dispose();
				_templateFolder.Dispose();
				_localRepoFolder.Dispose();
			}

			public TestLdmlInFolderWritingSystemRepository LocalRepository { get; private set; }

			public GlobalWritingSystemRepository GlobalRepository { get; private set; }

			public string LocalRepositoryPath
			{
				get { return _localRepoFolder.Path; }
			}

			public WritingSystemDefinition WritingSystem
			{
				get { return _writingSystem; }
			}

			public string GetPathForLocalWSId(string id)
			{
				string path = Path.Combine(_localRepoFolder.Path, id + ".ldml");
				return path;
			}

			public string GetPathForGlobalWSId(string id)
			{
				string path = Path.Combine(GlobalWritingSystemRepository.CurrentVersionPath(_globalRepoFolder.Path), id + ".ldml");
				return path;
			}

			public void AssertWritingSystemFileExists(string id)
			{
				Assert.IsTrue(File.Exists(GetPathForLocalWSId(id)));
			}
		}

		[Test]
		public void LatestVersion_IsThree()
		{
			Assert.AreEqual(3, LdmlDataMapper.CurrentLdmlLibraryVersion);
		}

		[Test]
		public void PathToCollection_SameAsGiven()
		{
			using (var environment = new TestEnvironment())
			{
				Assert.AreEqual(environment.LocalRepositoryPath, environment.LocalRepository.PathToWritingSystems);
			}
		}

		[Test]
		public void SaveDefinitionsThenLoad_CountEquals2()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "one";
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				var ws2 = new WritingSystemDefinition
					{
						Language = "two"
					};
				environment.LocalRepository.SaveDefinition(ws2);
				environment.ResetRepositories();

				Assert.AreEqual(2, environment.LocalRepository.Count);
			}
		}

		[Test]
		public void SavesWhenNotPreexisting()
		{
			using (var environment = new TestEnvironment())
			{
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				environment.AssertWritingSystemFileExists(environment.WritingSystem.LanguageTag);
			}
		}

		[Test]
		public void SavesWhenPreexisting()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);

				var ws2 = new WritingSystemDefinition();
				environment.WritingSystem.Language = "en";
				environment.LocalRepository.SaveDefinition(ws2);
			}
		}

		[Test]
		public void RegressionWhereUnchangedDefDeleted()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "qaa";
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				var ws2 = environment.LocalRepository.Get(environment.WritingSystem.Id);
				environment.LocalRepository.SaveDefinition(ws2);
				environment.AssertWritingSystemFileExists(environment.WritingSystem.LanguageTag);
			}
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			using (var environment = new TestEnvironment())
			{
				string newRepoPath = Path.Combine(environment.LocalRepositoryPath, "newguy");
				var newRepository = new LdmlInFolderWritingSystemRepository(newRepoPath);
				newRepository.SaveDefinition(environment.WritingSystem);
				Assert.That(File.Exists(Path.Combine(newRepoPath, environment.WritingSystem.LanguageTag + ".ldml")));
			}
		}

		[Test]
		public void Save_WritingSystemIdChanged_ChangeLogUpdated()
		{
			using (var e = new TestEnvironment())
			{
				LdmlInFolderWritingSystemRepository repo = LdmlInFolderWritingSystemRepository.Initialize(Path.Combine(e.LocalRepositoryPath, "idchangedtest1"));
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
				LdmlInFolderWritingSystemRepository repo = LdmlInFolderWritingSystemRepository.Initialize(Path.Combine(e.LocalRepositoryPath, "idchangedtest1"));
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
		public void IdAfterSave_SameAsFileNameWithoutExtension()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				Assert.AreEqual("en", environment.WritingSystem.Id);
			}
		}

		[Test]
		public void IdAfterLoad_SameAsFileNameWithoutExtension()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				Assert.AreNotEqual(0, Directory.GetFiles(environment.LocalRepositoryPath, "*.ldml"));
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				environment.ResetRepositories();
				WritingSystemDefinition ws2 = environment.LocalRepository.Get("en");
				Assert.AreEqual(
					Path.GetFileNameWithoutExtension(Directory.GetFiles(environment.LocalRepositoryPath, "*.ldml")[0]), ws2.Id);
			}
		}

		[Test]
		public void UpdatesFileNameWhenIsoChanges()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				string path = Path.Combine(environment.LocalRepository.PathToWritingSystems, "en.ldml");
				Assert.IsTrue(File.Exists(path));
				var ws2 = environment.LocalRepository.Get(environment.WritingSystem.LanguageTag);
				ws2.Language = "de";
				Assert.AreEqual("en", ws2.Id);
				environment.LocalRepository.SaveDefinition(ws2);
				Assert.AreEqual("de", ws2.Id);
				Assert.IsFalse(File.Exists(path));
				path = Path.Combine(environment.LocalRepository.PathToWritingSystems, "de.ldml");
				Assert.IsTrue(File.Exists(path));
			}
		}

		[Test]
		public void MakesNewFileIfNeeded()
		{
			using (var environment = new TestEnvironment())
			{

				environment.WritingSystem.Language = "en";
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(environment.GetPathForLocalWSId(environment.WritingSystem.LanguageTag)).HasAtLeastOneMatchForXpath("ldml/identity/language[@type='en']");
			}
		}

		[Test]
		public void CanAddVariantToLdmlUsingSameWS()
		{
			using (var environment = new TestEnvironment())
			{
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				environment.WritingSystem.Variants.Add("1901");
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(environment.GetPathForLocalWSId(environment.WritingSystem.LanguageTag)).HasAtLeastOneMatchForXpath("ldml/identity/variant[@type='1901']");
			}
		}

		[Test]
		public void CanAddVariantToExistingLdml()
		{
			using (var environment = new TestEnvironment())
			{

				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Script = "Latn";
				environment.WritingSystem.WindowsLcid = "12345";
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);

				environment.ResetRepositories();
				WritingSystemDefinition ws2 = environment.LocalRepository.Get(environment.WritingSystem.Id);
				ws2.Variants.Add("piglatin");
				environment.LocalRepository.SaveDefinition(ws2);
				string path = Path.Combine(environment.LocalRepository.PathToWritingSystems,
										   environment.GetPathForLocalWSId(ws2.LanguageTag));
				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/identity/variant[@type='x-piglatin']");
				var manager = new XmlNamespaceManager(new NameTable());
				manager.AddNamespace("sil", "urn://www.sil.org/ldml/0.1");
				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/identity/special/sil:identity[@windowsLCID='12345']", manager);
			}
		}

		[Test]
		public void CanReadVariant()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Variants.Add("piglatin");
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);

				environment.ResetRepositories();
				WritingSystemDefinition ws2 = environment.LocalRepository.Get(environment.WritingSystem.Id);
				Assert.That(ws2.Variants, Is.EqualTo(new VariantSubtag[] {"piglatin"}));
			}
		}

		[Test]
		public void CanSaveAndReadKeyboardId()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				var kbd1 = new DefaultKeyboardDefinition("Thai", "Thai");
				kbd1.Format = KeyboardFormat.Msklc;
				environment.WritingSystem.KnownKeyboards.Add(kbd1);
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);

				environment.ResetRepositories();
				WritingSystemDefinition ws2 = environment.LocalRepository.Get("en");
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
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);

				environment.ResetRepositories();
				WritingSystemDefinition ws2 = environment.LocalRepository.Get("en");
				Assert.IsTrue(ws2.RightToLeftScript);
			}
		}

		[Test]
		public void CanRemoveVariant()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.WritingSystem.Variants.Add("piglatin");
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				string path = environment.GetPathForLocalWSId(environment.WritingSystem.LanguageTag);

				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/identity/variant");
				environment.WritingSystem.Variants.Clear();
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(environment.GetPathForLocalWSId(environment.WritingSystem.LanguageTag)).HasNoMatchForXpath("ldml/identity/variant");
			}
		}

		[Test]
		public void CanDeleteFileThatIsNotInTrash()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				string path = Path.Combine(environment.LocalRepository.PathToWritingSystems,
										   environment.GetPathForLocalWSId(environment.WritingSystem.LanguageTag));
				Assert.IsTrue(File.Exists(path));
				environment.LocalRepository.Remove(environment.WritingSystem.Language);
				Assert.IsFalse(File.Exists(path));
				AssertFileIsInTrash(environment);
			}
		}

		private static void AssertFileIsInTrash(TestEnvironment environment)
		{
			string path = Path.Combine(environment.LocalRepository.PathToWritingSystems, "trash");
			path = Path.Combine(path,environment.WritingSystem.LanguageTag + ".ldml");
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void CanDeleteFileMatchingOneThatWasPreviouslyTrashed()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.Language = "en";
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				environment.LocalRepository.Remove(environment.WritingSystem.Id);
				AssertFileIsInTrash(environment);
				var ws2 = new WritingSystemDefinition {Language = "en"};
				environment.LocalRepository.SaveDefinition(ws2);
				environment.LocalRepository.Remove(ws2.Id);
				string path = Path.Combine(environment.LocalRepository.PathToWritingSystems,
										   environment.GetPathForLocalWSId(environment.WritingSystem.LanguageTag));
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
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				environment.ResetRepositories();
				WritingSystemDefinition ws2 = environment.LocalRepository.Get(environment.WritingSystem.Id);
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
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
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
				environment.LocalRepository.SystemWritingSystemProvider = new DummyWritingSystemProvider();
				var list = environment.LocalRepository.AllWritingSystems;
				Assert.IsTrue(ContainsLanguageWithName(list, "English"));
			}
		}

		[Test]
		public void DefaultLanguageNotAddedIfInTrash()
		{
			using (var environment = new TestEnvironment())
			{
				environment.LocalRepository.SystemWritingSystemProvider = new DummyWritingSystemProvider();
				var list = environment.LocalRepository.AllWritingSystems;
				Assert.IsTrue(ContainsLanguageWithName(list, "English"));
				var list2 = new List<WritingSystemDefinition>(environment.LocalRepository.AllWritingSystems);
				WritingSystemDefinition ws2 = list2[0];
				environment.LocalRepository.Remove(ws2.Language);

				environment.ResetRepositories();
				//  repository.DontAddDefaultDefinitions = false;
				environment.LocalRepository.SystemWritingSystemProvider = new DummyWritingSystemProvider();
				Assert.IsFalse(ContainsLanguageWithName(environment.LocalRepository.AllWritingSystems, "English"));
			}

		}

		[Test]
		public void Constructor_LdmlFolderStoreContainsMultipleFilesThatOnLoadDescribeWritingSystemsWithIdenticalRFC5646Tags_Throws()
		{
			using (var environment = new TestEnvironment())
			{
				File.WriteAllText(Path.Combine(environment.LocalRepositoryPath, "de-Zxxx-x-audio.ldml"),
								  LdmlContentForTests.CurrentVersion("de", WellKnownSubtags.UnwrittenScript, "", "x-audio"));
				File.WriteAllText(Path.Combine(environment.LocalRepositoryPath, "inconsistent-filename.ldml"),
								  LdmlContentForTests.CurrentVersion("de", WellKnownSubtags.UnwrittenScript, "", "x-audio"));

				environment.ResetRepositories();
				IList<WritingSystemRepositoryProblem> problems = environment.LocalRepository.LoadProblems;

				Assert.That(problems.Count, Is.EqualTo(2));
				Assert.That(
					problems[0].Exception,
					Is.TypeOf<ApplicationException>().With.Property("Message").
					ContainsSubstring(String.Format(
						@"The writing system file {0} seems to be named inconsistently. It contains the IETF language tag: 'de-Zxxx-x-audio'. The name should have been made consistent with its content upon migration of the writing systems.",
						Path.Combine(environment.LocalRepositoryPath, "inconsistent-filename.ldml")
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
				e.LocalRepository.Set(new WritingSystemDefinition("de"));
				e.LocalRepository.Set(new WritingSystemDefinition("en"));
				e.LocalRepository.Conflate("de", "en");
				Assert.That(e.LocalRepository.WritingSystemIdHasChangedTo("de"), Is.EqualTo("en"));
			}
		}

		[Test]
		//This is not really a problem, but it would be nice if the file were made consistent. So make we will make them run it through the migrator, which they should be using anyway.
		public void Constructor_LdmlFolderStoreContainsInconsistentlyNamedFile_HasExpectedProblem()
		{
			using (var environment = new TestEnvironment())
			{
				File.WriteAllText(Path.Combine(environment.LocalRepositoryPath, "tpi-Zxxx-x-audio.ldml"),
								  LdmlContentForTests.CurrentVersion("de", "latn", "ch", "1901"));

				environment.ResetRepositories();
				IList<WritingSystemRepositoryProblem> problems = environment.LocalRepository.LoadProblems;
				Assert.That(problems.Count, Is.EqualTo(1));
				Assert.That(
					problems[0].Exception,
					Is.TypeOf<ApplicationException>().With.Property("Message").
					ContainsSubstring(String.Format(
						@"The writing system file {0} seems to be named inconsistently. It contains the IETF language tag: 'de-CH-1901'. The name should have been made consistent with its content upon migration of the writing systems.",
						Path.Combine(environment.LocalRepositoryPath, "tpi-Zxxx-x-audio.ldml")
					))
				);
			}
		}

		[Test]
		public void Constructor_LdmlFolderStoreContainsInconsistentlyNamedFileDifferingInCaseOnly_HasNoProblem()
		{
			using (var environment = new TestEnvironment())
			{
				File.WriteAllText(Path.Combine(environment.LocalRepositoryPath, "aa-cyrl.ldml"),
								  LdmlContentForTests.CurrentVersion("aa", "Cyrl", "", ""));

				environment.ResetRepositories();
				IList<WritingSystemRepositoryProblem> problems = environment.LocalRepository.LoadProblems;
				Assert.That(problems.Count, Is.EqualTo(0));
			}
		}

		[Test]
		//this used to throw
		public void LoadAllDefinitions_FilenameDoesNotMatchRfc5646Tag_NoProblem()
		{
			using (var environment = new TestEnvironment())
			{
				//Make the filepath inconsistant
				environment.WritingSystem.Language = "en";
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				File.Move(Path.Combine(environment.LocalRepositoryPath, "en.ldml"), Path.Combine(environment.LocalRepositoryPath, "de.ldml"));

				// Now try to load up.
				environment.ResetRepositories();
				Assert.That(environment.LocalRepository.Contains("en"));
			}
		}

		[Test]
		public void Get_WritingSystemContainedInFileWithfilenameThatDoesNotMatchRfc5646Tag_ReturnsWritingSystem()
		{
			using (var environment = new TestEnvironment())
			{
				//Make the filepath inconsistant
				environment.WritingSystem.Language = "en";
				environment.LocalRepository.SaveDefinition(environment.WritingSystem);
				File.Move(Path.Combine(environment.LocalRepositoryPath, "en.ldml"), Path.Combine(environment.LocalRepositoryPath, "de.ldml"));

				// Now try to load up.
				environment.ResetRepositories();
				var ws = environment.LocalRepository.Get("en");
				Assert.That(ws.LanguageTag, Is.EqualTo("en"));
			}
		}

		[Test]
		public void LoadAllDefinitions_FilenameIsFlexConformPrivateUseAndDoesNotMatchRfc5646Tag_Migrates()
		{
			using (var environment = new TestEnvironment())
			{
				var ldmlPath = Path.Combine(environment.LocalRepositoryPath, "x-kal-Zxxx.ldml");
				File.WriteAllText(ldmlPath, LdmlContentForTests.Version0("x-kal", "Zxxx", "", ""));
				LdmlInFolderWritingSystemRepository repo = LdmlInFolderWritingSystemRepository.Initialize(environment.LocalRepositoryPath);

				// Now try to load up.
				Assert.That(repo.Get("qaa-Zxxx-x-kal").Language, Is.EqualTo(new LanguageSubtag("kal")));
			}
		}

		[Test]
		public void Set_NewWritingSystem_StoreContainsWritingSystem()
		{
			using (var environment = new TestEnvironment())
			{
				var ws = new WritingSystemDefinition("en");
				environment.LocalRepository.Set(ws);
				Assert.That(environment.LocalRepository.Get("en").LanguageTag, Is.EqualTo("en"));
			}
		}

		[Test]
		public void SaveDefinition_WritingSystemCameFromValidRfc5646WritingSystemStartingWithX_FileNameIsChanged()
		{
			using (var environment = new TestEnvironment())
			{
				var pathToFlexprivateUseLdml = Path.Combine(environment.LocalRepositoryPath, "x-Zxxx-x-audio.ldml");
				File.WriteAllText(pathToFlexprivateUseLdml, LdmlContentForTests.CurrentVersion("xh", "", "", ""));
				environment.ResetRepositories();
				Assert.That(File.Exists(Path.Combine(environment.LocalRepositoryPath, "xh.ldml")));
			}
		}

		[Test]
		public void WritingSystemIdHasBeenChanged_IdNeverExisted_ReturnsFalse()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				Assert.That(environment.LocalRepository.WritingSystemIdHasChanged("en"), Is.False);
			}
		}

		[Test]
		public void WritingSystemIdHasBeenChanged_IdChanged_ReturnsTrue()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var ws = new WritingSystemDefinition("en");
				environment.LocalRepository.Set(ws);
				environment.LocalRepository.Save();
				//Now change the Id
				ws.Variants.Add("bogus");
				environment.LocalRepository.Save();
				Assert.That(environment.LocalRepository.WritingSystemIdHasChanged("en"), Is.True);
			}
		}

		[Test]
		public void WritingSystemIdHasBeenChanged_IdChangedToMultipleDifferentNewIds_ReturnsTrue()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var wsEn = new WritingSystemDefinition("en");
				environment.LocalRepository.Set(wsEn);
				environment.LocalRepository.Save();
				//Now change the Id and create a duplicate of the original Id
				wsEn.Variants.Add("bogus");
				environment.LocalRepository.Set(wsEn);
				var wsEnDup = new WritingSystemDefinition("en");
				environment.LocalRepository.Set(wsEnDup);
				environment.LocalRepository.Save();
				//Now change the duplicate's Id as well
				wsEnDup.Variants.Add("bogus2");
				environment.LocalRepository.Set(wsEnDup);
				environment.LocalRepository.Save();
				Assert.That(environment.LocalRepository.WritingSystemIdHasChanged("en"), Is.True);
			}
		}

		[Test]
		public void WritingSystemIdHasBeenChanged_IdExistsAndHasNeverChanged_ReturnsFalse()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var ws = new WritingSystemDefinition("en");
				environment.LocalRepository.Set(ws);
				environment.LocalRepository.Save();
				Assert.That(environment.LocalRepository.WritingSystemIdHasChanged("en"), Is.False);
			}
		}

		[Test]
		public void WritingSystemIdHasChangedTo_IdNeverExisted_ReturnsNull()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				Assert.That(environment.LocalRepository.WritingSystemIdHasChangedTo("en"), Is.Null);
			}
		}

		[Test]
		public void WritingSystemIdHasChangedTo_IdChanged_ReturnsNewId()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var ws = new WritingSystemDefinition("en");
				environment.LocalRepository.Set(ws);
				environment.LocalRepository.Save();
				//Now change the Id
				ws.Variants.Add("bogus");
				environment.LocalRepository.Save();
				Assert.That(environment.LocalRepository.WritingSystemIdHasChangedTo("en"), Is.EqualTo("en-x-bogus"));
			}
		}

		[Test]
		public void WritingSystemIdHasChangedTo_IdChangedToMultipleDifferentNewIds_ReturnsNull()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var wsEn = new WritingSystemDefinition("en");
				environment.LocalRepository.Set(wsEn);
				environment.LocalRepository.Save();
				//Now change the Id and create a duplicate of the original Id
				wsEn.Variants.Add("bogus");
				environment.LocalRepository.Set(wsEn);
				var wsEnDup = new WritingSystemDefinition("en");
				environment.LocalRepository.Set(wsEnDup);
				environment.LocalRepository.Save();
				//Now change the duplicate's Id as well
				wsEnDup.Variants.Add("bogus2");
				environment.LocalRepository.Set(wsEnDup);
				environment.LocalRepository.Save();
				Assert.That(environment.LocalRepository.WritingSystemIdHasChangedTo("en"), Is.Null);
			}
		}

		[Test]
		public void WritingSystemIdHasChangedTo_IdExistsAndHasNeverChanged_ReturnsId()
		{
			using (var environment = new TestEnvironment())
			{
				//Add a writing system to the repo
				var ws = new WritingSystemDefinition("en");
				environment.LocalRepository.Set(ws);
				environment.LocalRepository.Save();
				Assert.That(environment.LocalRepository.WritingSystemIdHasChangedTo("en"), Is.EqualTo("en"));
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
				var pathForFlexGerman = Path.Combine(environment.LocalRepositoryPath, "de.ldml");
				var ws1 = new WritingSystemDefinition("en");
				var ws2 = new WritingSystemDefinition("de");
				//Create repo with english and flex german
				environment.LocalRepository.Set(ws1);
				environment.LocalRepository.Set(ws2);
				environment.LocalRepository.Save();
				//The content of the file is switched out here as opposed to loading from this content in the first place
				//because order is extremely important for this test and if we loaded from this ldml "de" would be the
				//first writing system in the repo rather than the second.
				File.WriteAllText(pathForFlexGerman, germanFromFlex);
				//rename the ws
				ws1.Language = "de";
				ws2.Language = "fr";
				environment.LocalRepository.Set(ws2);
				environment.LocalRepository.Set(ws1);
				environment.LocalRepository.Save();

				pathForFlexGerman = Path.Combine(environment.LocalRepositoryPath, "fr.ldml");
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
				var ws1 = new WritingSystemDefinition("en");
				var ws2 = new WritingSystemDefinition("de");
				environment.LocalRepository.Set(ws1);
				environment.LocalRepository.Set(ws2);
				environment.LocalRepository.Save();
				//rename the ws
				ws1.Language = "de";
				ws2.Language = "fr";
				environment.LocalRepository.Set(ws2);
				environment.LocalRepository.Set(ws1);
				environment.LocalRepository.Save();
				environment.ResetRepositories();
				Assert.That(environment.LocalRepository.Count, Is.EqualTo(2));
			}
		}

		[Test]
		public void LoadAllDefinitions_LDMLV0_HasExpectedProblem()
		{
			using (var environment = new TestEnvironment())
			{
				var ldmlPath = Path.Combine(environment.LocalRepositoryPath, "en.ldml");
				File.WriteAllText(ldmlPath, LdmlContentForTests.Version0("en", "", "", ""));

				var repository = new LdmlInFolderWritingSystemRepository(environment.LocalRepositoryPath);
				var problems = repository.LoadProblems;

				Assert.That(problems.Count, Is.EqualTo(1));
				Assert.That(
					problems[0].Exception,
					Is.TypeOf<ApplicationException>().With.Property("Message").
					ContainsSubstring(String.Format(
						"The LDML tag 'en' is version 0.  Version {0} was expected.",
						LdmlDataMapper.CurrentLdmlLibraryVersion
					))
				);
			}
		}

		[Test]
		public void LoadDefinitions_ValidLanguageTagStartingWithXButV0_Throws()
		{
			using (var environment = new TestEnvironment())
			{
				var pathToFlexprivateUseLdml = Path.Combine(environment.LocalRepositoryPath, "xh.ldml");
				File.WriteAllText(pathToFlexprivateUseLdml, LdmlContentForTests.Version0("xh", "", "", ""));
				var repository = new LdmlInFolderWritingSystemRepository(environment.LocalRepositoryPath);
				var problems = repository.LoadProblems;

				Assert.That(problems.Count, Is.EqualTo(1));
				Assert.That(
					problems[0].Exception,
					Is.TypeOf<ApplicationException>().With.Property("Message").
					ContainsSubstring(String.Format(
						"The LDML tag 'xh' is version 0.  Version {0} was expected.",
						LdmlDataMapper.CurrentLdmlLibraryVersion
					))
				);
			}
		}

		[Test]
		public void FactoryCreate_TemplateAvailableInLocalRepo_UsedTemplateFromLocalRepo()
		{
			using (var environment = new TestEnvironment())
			{
				File.WriteAllText(environment.GetPathForLocalWSId("en"), @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='1.0'>From Repo</version>
		<generation date='0001-01-01T00:00:00' />
		<language type='en' />
		<script type='Latn' />
	</identity>
	<layout>
		<orientation>
			<characterOrder>left-to-right</characterOrder>
			<lineOrder>top-to-bottom</lineOrder>
		</orientation>
	</layout>
</ldml>
");
				environment.ResetRepositories();
				WritingSystemDefinition enWS;
				Assert.That(environment.LocalRepository.WritingSystemFactory.Create("en", out enWS), Is.True);
				Assert.That(enWS.Language, Is.EqualTo((LanguageSubtag) "en"));
				Assert.That(enWS.Script, Is.EqualTo((ScriptSubtag) "Latn"));
				Assert.That(enWS.VersionDescription, Is.EqualTo("From Repo"));
				Assert.That(enWS.Template, Is.EqualTo(environment.GetPathForLocalWSId("en")));

				// ensure that the template is used when the writing system is saved
				enWS.Region = "US";
				environment.LocalRepository.Set(enWS);
				environment.LocalRepository.Save();
				XElement ldmlElem = XElement.Load(environment.GetPathForLocalWSId("en-US"));
				Assert.That((string) ldmlElem.Elements("layout").Elements("orientation").Elements("lineOrder").First(), Is.EqualTo("top-to-bottom"));
			}
		}

		[Test]
		public void FactoryCreate_TemplateAvailableInSldr_UsedTemplateFromSldr()
		{
			using (var environment = new TestEnvironment())
			{
				environment.LocalRepository.WritingSystemFactory.SldrLdmls["en"] = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='1.0'>From SLDR</version>
		<generation date='0001-01-01T00:00:00' />
		<language type='en' />
		<script type='Latn' />
	</identity>
	<layout>
		<orientation>
			<characterOrder>left-to-right</characterOrder>
			<lineOrder>top-to-bottom</lineOrder>
		</orientation>
	</layout>
</ldml>
";

				WritingSystemDefinition enWS;
				Assert.That(environment.LocalRepository.WritingSystemFactory.Create("en", out enWS), Is.True);
				Assert.That(enWS.Language, Is.EqualTo((LanguageSubtag) "en"));
				Assert.That(enWS.Script, Is.EqualTo((ScriptSubtag) "Latn"));
				Assert.That(enWS.VersionDescription, Is.EqualTo("From SLDR"));
				Assert.That(enWS.Template, Is.EqualTo(Path.Combine(Sldr.SldrCachePath, "en.ldml")));

				// ensure that the template is used when the writing system is saved
				environment.LocalRepository.Set(enWS);
				environment.LocalRepository.Save();
				XElement ldmlElem = XElement.Load(environment.GetPathForLocalWSId("en"));
				Assert.That((string) ldmlElem.Elements("layout").Elements("orientation").Elements("lineOrder").First(), Is.EqualTo("top-to-bottom"));
			}
		}

		[Test]
		public void FactoryCreate_TemplateNotAvailableInSldr_UsedTemplateFromTemplateFolder()
		{
			using (var environment = new TestEnvironment())
			{
				File.WriteAllText(Path.Combine(environment.LocalRepository.WritingSystemFactory.TemplateFolder, "zh-CN.ldml"), @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='1.0'>From Templates</version>
		<generation date='0001-01-01T00:00:00' />
		<language type='zh' />
		<script type='Hans' />
		<territory type='CN' />
	</identity>
	<layout>
		<orientation>
			<characterOrder>left-to-right</characterOrder>
			<lineOrder>top-to-bottom</lineOrder>
		</orientation>
	</layout>
</ldml>
");

				WritingSystemDefinition chWS;
				Assert.That(environment.LocalRepository.WritingSystemFactory.Create("zh-CN", out chWS), Is.False);
				Assert.That(chWS.Language, Is.EqualTo((LanguageSubtag) "zh"));
				Assert.That(chWS.Script, Is.EqualTo((ScriptSubtag) "Hans"));
				Assert.That(chWS.Region, Is.EqualTo((RegionSubtag) "CN"));
				Assert.That(chWS.VersionDescription, Is.EqualTo("From Templates"));
				Assert.That(chWS.Template, Is.EqualTo(Path.Combine(environment.LocalRepository.WritingSystemFactory.TemplateFolder, "zh-CN.ldml")));

				// ensure that the template is used when the writing system is saved
				environment.LocalRepository.Set(chWS);
				environment.LocalRepository.Save();
				XElement ldmlElem = XElement.Load(environment.GetPathForLocalWSId("zh-CN"));
				Assert.That((string) ldmlElem.Elements("layout").Elements("orientation").Elements("lineOrder").First(), Is.EqualTo("top-to-bottom"));
			}
		}

		[Test]
		public void Save_UpdatesGlobalStore()
		{
			using (var environment = new TestEnvironment())
			using (var testFolder2 = new TemporaryFolder("LdmlInFolderWritingSystemRepositoryTests2"))
			{
				var enUsTag = "en-US";
				var ws = new WritingSystemDefinition(enUsTag);
				environment.LocalRepository.Set(ws);
				ws.RightToLeftScript = true;
				ws.DefaultCollation = new SystemCollationDefinition {LanguageTag = enUsTag};
				environment.LocalRepository.Save();
				Assert.IsTrue(File.Exists(environment.GetPathForLocalWSId(enUsTag)));
				Assert.IsTrue(File.Exists(environment.GetPathForGlobalWSId(enUsTag)));
				// Add some extra content to make sure we round trip it
				var document = XDocument.Load(environment.GetPathForGlobalWSId(enUsTag));
				var root = document.Root;
				root.Add(XElement.Parse("<mysteryTag><languages><language type='aa'>Ingles</language></languages></mysteryTag>"));
				document.Save(environment.GetPathForGlobalWSId(enUsTag));

				// ensure that the date modified actually changes
				Thread.Sleep(1000);

				DateTime lastModified = File.GetLastWriteTime(environment.GetPathForGlobalWSId(enUsTag));
				var localRepo2 = new LdmlInFolderWritingSystemRepository(testFolder2.Path, environment.GlobalRepository);
				ws = new WritingSystemDefinition(enUsTag);
				localRepo2.Set(ws);
				ws.RightToLeftScript = false;
				localRepo2.Save();
				Assert.Less(lastModified, File.GetLastWriteTime(environment.GetPathForGlobalWSId(enUsTag)));

				lastModified = File.GetLastWriteTime(environment.GetPathForLocalWSId(enUsTag));
				environment.ResetRepositories();
				ws = environment.LocalRepository.Get(enUsTag);
				Assert.That(ws.RightToLeftScript, Is.True);
				WritingSystemDefinition[] sharedWSs = environment.LocalRepository.CheckForNewerGlobalWritingSystems().ToArray();
				Assert.That(sharedWSs.Select(sharedWS => sharedWS.LanguageTag), Is.EqualTo(new[] {enUsTag}));
				environment.LocalRepository.Remove(sharedWSs[0].LanguageTag);
				environment.LocalRepository.Set(sharedWSs[0]);
				environment.LocalRepository.Save();

				ws = environment.LocalRepository.Get(enUsTag);
				Assert.That(ws.RightToLeftScript, Is.False);
				// ensure that application-specific settings are preserved
				Assert.That(ws.DefaultCollation.ValueEquals(new SystemCollationDefinition {LanguageTag = enUsTag}), Is.True);
				Assert.Less(lastModified, File.GetLastWriteTime(environment.GetPathForLocalWSId(enUsTag)));
				// verify that we round tripped the extra content
				AssertThatXmlIn.File(environment.GetPathForGlobalWSId(enUsTag)).HasSpecifiedNumberOfMatchesForXpath("/ldml/mysteryTag/languages/language[@type='aa']", 1);
			}
		}

		[Test]
		public void Save_UpdatesWritingSystemsToIgnore()
		{
			using (var environment = new TestEnvironment())
			using (var testFolder2 = new TemporaryFolder("LdmlInFolderWritingSystemRepositoryTests2"))
			{
				var ws = new WritingSystemDefinition("en-US");
				environment.LocalRepository.Set(ws);
				ws.RightToLeftScript = true;
				environment.LocalRepository.Save();

				// ensure that the date modified actually changes
				Thread.Sleep(1000);

				var localRepo2 = new LdmlInFolderWritingSystemRepository(testFolder2.Path, environment.GlobalRepository);
				ws = new WritingSystemDefinition("en-US");
				localRepo2.Set(ws);
				ws.RightToLeftScript = false;
				localRepo2.Save();

				environment.ResetRepositories();
				ws = environment.LocalRepository.Get("en-US");
				Assert.That(ws.RightToLeftScript, Is.True);
				WritingSystemDefinition[] sharedWSs = environment.LocalRepository.CheckForNewerGlobalWritingSystems().ToArray();
				Assert.That(sharedWSs.Select(sharedWS => sharedWS.LanguageTag), Is.EqualTo(new[] {"en-US"}));
				environment.LocalRepository.Save();

				environment.ResetRepositories();
				Assert.That(environment.LocalRepository.CheckForNewerGlobalWritingSystems(), Is.Empty);
				environment.LocalRepository.Save();

				// ensure that the date modified actually changes
				Thread.Sleep(1000);

				localRepo2 = new LdmlInFolderWritingSystemRepository(testFolder2.Path, environment.GlobalRepository);
				ws = localRepo2.Get("en-US");
				ws.CharacterSets.Add(new CharacterSetDefinition("main"));
				localRepo2.Save();

				environment.ResetRepositories();
				ws = environment.LocalRepository.Get("en-US");
				Assert.That(ws.CharacterSets, Is.Empty);
				sharedWSs = environment.LocalRepository.CheckForNewerGlobalWritingSystems().ToArray();
				Assert.That(sharedWSs.Select(sharedWS => sharedWS.LanguageTag), Is.EqualTo(new[] {"en-US"}));
				environment.LocalRepository.Remove(sharedWSs[0].LanguageTag);
				environment.LocalRepository.Set(sharedWSs[0]);
				environment.LocalRepository.Save();
				ws = environment.LocalRepository.Get("en-US");
				Assert.That(ws.CharacterSets.Count, Is.EqualTo(1));
				Assert.That(ws.CharacterSets[0].Type, Is.EqualTo("main"));
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
}