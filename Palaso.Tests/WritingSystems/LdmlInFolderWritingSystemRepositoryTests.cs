using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Palaso.WritingSystems;
using Palaso.TestUtilities;

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
			LdmlInFolderWritingSystemRepository repository = new LdmlInFolderWritingSystemRepository(testPath);
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
			private WritingSystemDefinition _writingSystem;

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
				Collection = new LdmlInFolderWritingSystemRepository(TestPath);
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
		}

		private void AssertWritingSystemFileExists(TestEnvironment environment)
		{
			AssertWritingSystemFileExists(environment.WritingSystem, environment.Collection);
		}

		private void AssertWritingSystemFileExists(WritingSystemDefinition writingSystem, LdmlInFolderWritingSystemRepository collection)
		{
			string path = collection.FilePathToWritingSystem(writingSystem);
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void LatestVersion_IsOne()
		{
			Assert.AreEqual(1, LdmlInFolderWritingSystemRepository.LatestVersion);
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
				environment.WritingSystem.ISO639 = "one";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				WritingSystemDefinition ws2 = new WritingSystemDefinition();
				ws2.ISO639 = "two";
				environment.Collection.SaveDefinition(ws2);
				var newStore = new LdmlInFolderWritingSystemRepository(environment.TestPath);

				Assert.AreEqual(2, newStore.Count);
			}
		}

		[Test]
		public void SavesWhenNotPreexisting()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertWritingSystemFileExists(environment);
			}
		}

		[Test]
		public void FileNameWhenNothingKnown()
		{
			using (var environment = new TestEnvironment())
			{
				Assert.AreEqual("qaa.ldml", environment.Collection.GetFileName(environment.WritingSystem));
			}
		}

		[Test]
		public void FileNameWhenOnlyHaveIso()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				Assert.AreEqual("en.ldml", environment.Collection.GetFileName(environment.WritingSystem));
			}
		}
		[Test]
		public void FileNameWhenHaveIsoAndRegion()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				environment.WritingSystem.Region = "us";
				Assert.AreEqual("en-us.ldml", environment.Collection.GetFileName(environment.WritingSystem));
			}
		}

		[Test]
		public void SavesWhenPreexisting()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				environment.Collection.SaveDefinition(environment.WritingSystem);

				WritingSystemDefinition ws2 = new WritingSystemDefinition();
				environment.WritingSystem.ISO639 = "en";
				environment.Collection.SaveDefinition(ws2);
			}
		}

		[Test]
		public void RegressionWhereUnchangedDefDeleted()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "qaa";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				WritingSystemDefinition ws2 = environment.Collection.Get(environment.WritingSystem.StoreID);
				environment.Collection.SaveDefinition(ws2);
				AssertWritingSystemFileExists(environment);
			}
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			using (var environment = new TestEnvironment())
			{
				LdmlInFolderWritingSystemRepository newRepository =
					new LdmlInFolderWritingSystemRepository(Path.Combine(environment.TestPath, "newguy"));
				newRepository.SaveDefinition(environment.WritingSystem);
				AssertWritingSystemFileExists(environment.WritingSystem, newRepository);
			}
		}

		[Test]
		public void StoreIDAfterSave_SameAsFileNameWithoutExtension()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				Assert.AreEqual("en", environment.WritingSystem.StoreID);
			}
		}

		[Test]
		public void StoreIDAfterLoad_SameAsFileNameWithoutExtension()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				Assert.AreNotEqual(0, Directory.GetFiles(environment.TestPath, "*.ldml"));
				environment.Collection.SaveDefinition(environment.WritingSystem);
				var newStore = new LdmlInFolderWritingSystemRepository(environment.TestPath);
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
				environment.WritingSystem.ISO639 = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = Path.Combine(environment.Collection.PathToWritingSystems, "en.ldml");
				Assert.IsTrue(File.Exists(path));
				WritingSystemDefinition ws2 = environment.Collection.Get(environment.WritingSystem.StoreID);
				ws2.ISO639 = "de";
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

				environment.WritingSystem.ISO639 = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(PathToWS(environment)).HasAtLeastOneMatchForXpath("ldml/identity/language[@type='en']");
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
				AssertThatXmlIn.File(PathToWS(environment)).HasAtLeastOneMatchForXpath("ldml/identity/variant[@type='1901']");
			}
		}

		[Test]
		public void CanAddVariantToExistingLDML()
		{
			using (var environment = new TestEnvironment())
			{

				environment.WritingSystem.ISO639 = "en";
				environment.WritingSystem.Abbreviation = "bl";
					//crucially, abbreviation isn't part of the name of the file
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get(environment.WritingSystem.StoreID);
				ws2.Variant = "x-piglatin";
				environment.Collection.SaveDefinition(ws2);
				string path = Path.Combine(environment.Collection.PathToWritingSystems,
										   environment.Collection.GetFileName(ws2));
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
				environment.WritingSystem.ISO639 = "en";
				environment.WritingSystem.Variant = "x-piglatin";
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get(environment.WritingSystem.StoreID);
				Assert.AreEqual("x-piglatin", ws2.Variant);
			}
		}

		[Test]
		public void CanSaveAndReadDefaultFont()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				environment.WritingSystem.DefaultFontName = "Courier";
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get("en");
				Assert.AreEqual("Courier", ws2.DefaultFontName);
			}
		}

		[Test]
		public void CanSaveAndReadKeyboardName()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				environment.WritingSystem.Keyboard = "Thai";
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get("en");
				Assert.AreEqual("Thai", ws2.Keyboard);
			}
		}

		[Test]
		public void CanSaveAndReadRightToLeft()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				Assert.IsFalse(environment.WritingSystem.RightToLeftScript);
				environment.WritingSystem.RightToLeftScript = true;
				Assert.IsTrue(environment.WritingSystem.RightToLeftScript);
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get("en");
				Assert.IsTrue(ws2.RightToLeftScript);
			}
		}

		[Test]
		public void CanSaveAndReadIsUnicode()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				Assert.IsFalse(environment.WritingSystem.IsLegacyEncoded);
				environment.WritingSystem.IsLegacyEncoded = true;
				Assert.IsTrue(environment.WritingSystem.IsLegacyEncoded);
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get("en");
				Assert.IsTrue(ws2.IsLegacyEncoded);
			}
		}

		[Test]
		public void IsLegacyEncoded_FalseByDefault()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);

				var newCollection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get("en");
				Assert.IsFalse(ws2.IsLegacyEncoded);
			}
		}

		[Test]
		public void CanRemoveVariant()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				environment.WritingSystem.Variant = "x-piglatin";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = environment.Collection.FilePathToWritingSystem(environment.WritingSystem);

				AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/identity/variant");
				environment.WritingSystem.Variant = string.Empty;
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(PathToWS(environment)).HasNoMatchForXpath("ldml/identity/variant");
			}
		}


		[Test]
		public void CanRemoveAbbreviation()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				environment.WritingSystem.Abbreviation = "abbrev";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = environment.Collection.FilePathToWritingSystem(environment.WritingSystem);
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
				environment.WritingSystem.ISO639 = "en";
				environment.WritingSystem.Abbreviation = "bl";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				AssertThatXmlIn.File(PathToWS(environment)).HasAtLeastOneMatchForXpath(
					"ldml/special/palaso:abbreviation[@value='bl']", environment.NamespaceManager);
			}
		}

		private string PathToWS(TestEnvironment environment)
		{
			return environment.Collection.FilePathToWritingSystem(environment.WritingSystem);
		}

		[Test]
		public void CanDeleteFileThatIsNotInTrash()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				string path = Path.Combine(environment.Collection.PathToWritingSystems,
										   environment.Collection.GetFileName(environment.WritingSystem));
				Assert.IsTrue(File.Exists(path));
				environment.Collection.Remove(environment.WritingSystem.ISO639);
				Assert.IsFalse(File.Exists(path));
				AssertFileIsInTrash(environment);
			}
		}

		private void AssertFileIsInTrash(TestEnvironment environment)
		{
			string path = Path.Combine(environment.Collection.PathToWritingSystems, "trash");
			path = Path.Combine(path,environment.Collection.GetFileName(environment.WritingSystem));
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void CanDeleteFileMatchingOneThatWasPreviouslyTrashed()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				environment.Collection.Remove(environment.WritingSystem.StoreID);
				AssertFileIsInTrash(environment);
				WritingSystemDefinition ws2 = new WritingSystemDefinition {ISO639 = "en"};
				environment.Collection.SaveDefinition(ws2);
				environment.Collection.Remove(ws2.StoreID);
				string path = Path.Combine(environment.Collection.PathToWritingSystems,
										   environment.Collection.GetFileName(environment.WritingSystem));
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
				environment.WritingSystem.ISO639 = "en";
				Assert.IsTrue(environment.WritingSystem.Modified);
			}
		}

		[Test]
		public void MarkedAsNotModifiedWhenLoaded()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				var newCollection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				WritingSystemDefinition ws2 = newCollection.Get(environment.WritingSystem.StoreID);
				Assert.IsFalse(ws2.Modified);
			}
		}

		[Test]
		public void MarkedAsNotModifiedWhenSaved()
		{
			using (var environment = new TestEnvironment())
			{
				environment.WritingSystem.ISO639 = "en";
				Assert.IsTrue(environment.WritingSystem.Modified);
				environment.Collection.SaveDefinition(environment.WritingSystem);
				Assert.IsFalse(environment.WritingSystem.Modified);
				environment.WritingSystem.ISO639 = "de";
				Assert.IsTrue(environment.WritingSystem.Modified);
			}
		}

		[Test]
		public void SystemWritingSystemProvider_Set_WritingSystemsAreIncludedInStore()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SystemWritingSystemProvider = new DummyWritingSystemProvider();
				IEnumerable<WritingSystemDefinition> list = environment.Collection.WritingSystemDefinitions;
				Assert.IsTrue(ContainsLanguageWithName(list, "English"));
			}
		}

		[Test]
		public void DefaultLanguageNotAddedIfInTrash()
		{
			using (var environment = new TestEnvironment())
			{
				environment.Collection.SystemWritingSystemProvider = new DummyWritingSystemProvider();
				IEnumerable<WritingSystemDefinition> list = environment.Collection.WritingSystemDefinitions;
				Assert.IsTrue(ContainsLanguageWithName(list, "English"));
				IList<WritingSystemDefinition> list2 =
					new List<WritingSystemDefinition>(environment.Collection.WritingSystemDefinitions);
				WritingSystemDefinition ws2 = list2[0];
				environment.Collection.Remove(ws2.ISO639);

				var repository = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				//  repository.DontAddDefaultDefinitions = false;
				repository.SystemWritingSystemProvider = new DummyWritingSystemProvider();
				Assert.IsFalse(ContainsLanguageWithName(repository.WritingSystemDefinitions, "English"));
			}

		}

		[Test]
		public void Constructor_LdmlFolderStoreContainsMultipleFilesThatOnLoadDescribeWritingSystemsWithIdenticalRFC5646Tags_Throws()
		{
			using (var environment = new TestEnvironment())
			{
				File.WriteAllText(Path.Combine(environment.TestPath, "de-Zxxx-x-audio.ldml"),
								  GetLdmlV0FileContent("de-Zxxx-x-audio", "", "", ""));
				File.WriteAllText(Path.Combine(environment.TestPath, "inconsistent-filename.ldml"),
								  GetLdmlV0FileContent("de", WellKnownSubTags.Audio.Script, "",
													 WellKnownSubTags.Audio.PrivateUseSubtag));

				Assert.Throws<ApplicationException>(
					() => environment.Collection = new LdmlInFolderWritingSystemRepository(environment.TestPath));
			}
		}

		[Test]
		//This is not really a problem, but it would be nice if the file were made consistant. So make we will make them run it through the migrator, which they should be using anyway.
		public void Constructor_LdmlFolderStoreContainsInconsistentlyNamedFile_Throws()
		{
			using (var environment = new TestEnvironment())
			{
				File.WriteAllText(Path.Combine(environment.TestPath, "tpi-Zxxx-x-audio.ldml"),
								  GetLdmlV0FileContent("de", "latn", "ch", "1901"));
				Assert.Throws<ApplicationException>(() => new LdmlInFolderWritingSystemRepository(environment.TestPath));
			}
		}

		[Test]
		public void Set_WritingSystemWasLoadedFromFlexPrivateUseLdmlAndRearranged_DoesNotChangeFileName()
		{
			using (var environment = new TestEnvironment())
			{
				var pathToFlexprivateUseLdml = Path.Combine(environment.TestPath, "x-en-Zxxx-x-audio.ldml");
				File.WriteAllText(pathToFlexprivateUseLdml,
								  GetLdmlV0FileContent("x-en", "Zxxx", "", "x-audio"));
				environment.Collection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				var ws = environment.Collection.Get("x-en-Zxxx-x-audio");
				environment.Collection.Set(ws);
				Assert.That(File.Exists(pathToFlexprivateUseLdml), Is.True);
			}
		}

		[Test]
		public void LoadAllDefinitions_FilenameDoesNotMatchRfc5646Tag_Throws()
		{
			using (var environment = new TestEnvironment())
			{
				//Make the filepath inconsistant
				environment.Collection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				environment.WritingSystem.ISO639 = "en";
				environment.Collection.SaveDefinition(environment.WritingSystem);
				File.Move(Path.Combine(environment.TestPath, "en.ldml"), Path.Combine(environment.TestPath, "de.ldml"));

				// Now try to load up.
				Assert.Throws<ApplicationException>(() => new LdmlInFolderWritingSystemRepository(environment.TestPath));
			}
		}

		[Test]
		public void LoadAllDefinitions_FilenameIsFlexConformPrivateUseAndDoesNotMatchRfc5646Tag_DoesNotThrow()
		{
			using (var environment = new TestEnvironment())
			{
				var ldmlPath = Path.Combine(environment.TestPath, "x-en-Zxxx.ldml");
				File.WriteAllText(ldmlPath, GetLdmlV0FileContent("x-en", "Zxxx", "", ""));
				var repo = new LdmlInFolderWritingSystemRepository(environment.TestPath);

				// Now try to load up.
				Assert.That(repo.Get("x-en-Zxxx").ISO639, Is.EqualTo("qaa"));
			}
		}

		[Test]
		public void Set_NewWritingSystem_StoreContainsWritingSystem()
		{
			using (var environment = new TestEnvironment())
			{
				var ws = new WritingSystemDefinition("en");
				environment.Collection = new LdmlInFolderWritingSystemRepository(environment.TestPath);
				environment.Collection.Set(ws);
				Assert.That(environment.Collection.Get("en").RFC5646, Is.EqualTo("en"));
			}
		}

		private string GetLdmlV0FileContent(string language, string script, string region, string variant)
		{
			string ldml = "<ldml><!--Comment--><identity><version number='' /><generation date='0001-01-01T00:00:00' />".Replace("'","\"");
			ldml += String.Format("<language type='{0}' />",language).Replace("'","\"");
			if(script!=String.Empty)
			{
			   ldml += String.Format("<script type='{0}' />", script).Replace("'","\"");
			}
			if(region!=String.Empty)
			{
				ldml += String.Format("<territory type='{0}' />",region).Replace("'", "\"");
			}
			if(variant!=String.Empty)
			{
				ldml += String.Format("<variant type='{0}' />", variant).Replace("'", "\"");
			}
			ldml += "</identity><dates /><collations /><special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1' /><special></special></ldml>".Replace("'","\"");
			return ldml;
		}

		private bool ContainsLanguageWithName(IEnumerable<WritingSystemDefinition> list, string name)
		{
			foreach (WritingSystemDefinition definition in list)
			{
				if(definition.LanguageName == name)
					return true;
			}
			return false;
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
}