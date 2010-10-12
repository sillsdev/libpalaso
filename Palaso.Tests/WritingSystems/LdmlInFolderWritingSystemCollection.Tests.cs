using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Palaso.Keyboarding;
using Palaso.WritingSystems;
using Palaso.TestUtilities;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class LdmlInFolderWritingSystemStoreInterfaceTests : IWritingSystemStoreTests
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

		public override IWritingSystemStore CreateNewStore()
		{
			string testPath = Path.GetTempPath() + "PalasoTest" + _testPaths.Count;
			if (Directory.Exists(testPath))
			{
				Directory.Delete(testPath, true);
			}
			_testPaths.Add(testPath);
			LdmlInFolderWritingSystemStore store = new LdmlInFolderWritingSystemStore(testPath);
			//store.DontAddDefaultDefinitions = true;
			return store;
		}
	}

	[TestFixture]
	public class LdmlInFolderWritingSystemCollectionTests
	{
		private string _testPath;
		private LdmlInFolderWritingSystemStore _collection;
		private WritingSystemDefinition _writingSystem;
		private XmlNamespaceManager _namespaceManager;

		[SetUp]
		public void Setup()
		{
			_writingSystem = new WritingSystemDefinition();
			_testPath = Path.GetTempPath() + "PalasoTest";
			if (Directory.Exists(_testPath))
			{
				Directory.Delete(_testPath, true);
			}
			_collection = new LdmlInFolderWritingSystemStore(_testPath);
//            _collection.DontAddDefaultDefinitions = true;
			Console.WriteLine("Writing System Path: {0}", _testPath);
			_namespaceManager = new XmlNamespaceManager(new NameTable());
			_namespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
		}

		[TearDown]
		public void TearDown()
		{
			//Directory.Delete(_testPath, true);
		}

		private void AssertWritingSystemFileExists(WritingSystemDefinition writingSystem)
		{
			AssertWritingSystemFileExists(writingSystem, _collection);
		}

		private void AssertWritingSystemFileExists(WritingSystemDefinition writingSystem, LdmlInFolderWritingSystemStore collection)
		{
			string path = collection.FilePathToWritingSystem(writingSystem);
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void PathToCollection_SameAsGiven()
		{
			Assert.AreEqual(_testPath, _collection.PathToWritingSystems);
		}

		[Test]
		public void SaveDefinitionsThenLoad_CountEquals2()
		{
			_writingSystem.ISO = "one";
			_collection.SaveDefinition(_writingSystem);
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2.ISO = "two";
			_collection.SaveDefinition(ws2);
			_collection.LoadAllDefinitions();

			Assert.AreEqual(2, _collection.Count);
		}

		[Test]
		public void SavesWhenNotPreexisting()
		{
			_collection.SaveDefinition(_writingSystem);
			AssertWritingSystemFileExists(_writingSystem);
		}

		[Test]
		public void FileNameWhenNothingKnown()
		{
			Assert.AreEqual("unknown.ldml", _collection.GetFileName(_writingSystem));
		}

		[Test]
		public void FileNameWhenOnlyHaveIso()
		{

			_writingSystem.ISO = "en";
			Assert.AreEqual("en.ldml", _collection.GetFileName(_writingSystem));
		}
		[Test]
		public void FileNameWhenHaveIsoAndRegion()
		{

			_writingSystem.ISO = "en";
			_writingSystem.Region = "us";
			Assert.AreEqual("en-us.ldml", _collection.GetFileName(_writingSystem));
		}

		[Test]
		public void SavesWhenPreexisting()
		{
			_writingSystem.ISO = "en";
			_collection.SaveDefinition(_writingSystem);
			_collection.SaveDefinition(_writingSystem);

			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			_writingSystem.ISO = "en";
			_collection.SaveDefinition(ws2);
		}

		[Test]
		public void RegressionWhereUnchangedDefDeleted()
		{
			_writingSystem.ISO = "blah";
			_collection.SaveDefinition(_writingSystem);
			WritingSystemDefinition ws2 = _collection.LoadDefinition("blah");
			_collection.SaveDefinition(ws2);
			AssertWritingSystemFileExists(_writingSystem);
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			LdmlInFolderWritingSystemStore newRepository = new LdmlInFolderWritingSystemStore(Path.Combine(_testPath, "newguy"));
			newRepository.SaveDefinition(_writingSystem);
			AssertWritingSystemFileExists(_writingSystem,newRepository);
		}

		[Test]
		public void StoreIDAfterSave_SameAsFileNameWithoutExtension()
		{
			_writingSystem.ISO = "1";
			_collection.SaveDefinition(_writingSystem);
			Assert.AreEqual("1", _writingSystem.StoreID);
		}

		[Test]
		public void StoreIDAfterLoad_SameAsFileNameWithoutExtension()
		{
			_writingSystem.ISO = "1";
			_collection.SaveDefinition(_writingSystem);

			WritingSystemDefinition ws2 = _collection.LoadDefinition("1");
			Assert.AreEqual("1", ws2.StoreID);
		}

		[Test]
		public void UpdatesFileNameWhenISOChanges()
		{
			_writingSystem.ISO = "1";
			_collection.SaveDefinition(_writingSystem);
			string path = Path.Combine(_collection.PathToWritingSystems, "1.ldml");
			Assert.IsTrue(File.Exists(path));
			WritingSystemDefinition ws2 = _collection.LoadDefinition("1");
			ws2.ISO = "2";
			Assert.AreEqual("1", ws2.StoreID);
			_collection.SaveDefinition(ws2);
			Assert.AreEqual("2", ws2.StoreID);
			Assert.IsFalse(File.Exists(path));
			path = Path.Combine(_collection.PathToWritingSystems, "2.ldml");
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void MakesNewFileIfNeeded()
		{

			_writingSystem.ISO = "blah";
			_collection.SaveDefinition(_writingSystem);
			AssertThatXmlIn.File(PathToWS).HasAtLeastOneMatchForXpath("ldml/identity/language[@type='blah']");
		}

		[Test]
		public void CanAddVariantToLDMLUsingSameWS()
		{

			_collection.SaveDefinition(_writingSystem);
			_writingSystem.Variant = "piglatin";
			_collection.SaveDefinition(_writingSystem);
			AssertThatXmlIn.File(PathToWS ).HasAtLeastOneMatchForXpath( "ldml/identity/variant[@type='piglatin']");
		}

		[Test]
		public void CanAddVariantToExistingLDML()
		{

			_writingSystem.ISO = "blah";
			_writingSystem.Abbreviation = "bl";//crucially, abbreviation isn't part of the name of the file
			_collection.SaveDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _collection.LoadDefinition("blah");
			ws2.Variant = "piglatin";
			_collection.SaveDefinition(ws2);
			string path = Path.Combine(_collection.PathToWritingSystems, _collection.GetFileName(ws2));
			AssertThatXmlIn.File(path ).HasAtLeastOneMatchForXpath( "ldml/identity/variant[@type='piglatin']");
			AssertThatXmlIn.File(path ).HasAtLeastOneMatchForXpath( "ldml/special/palaso:abbreviation[@value='bl']", _namespaceManager);
		}

		[Test]
		public void CanReadVariant()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Variant = "piglatin";
			_collection.SaveDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 =_collection.LoadDefinition("en-piglatin");
			Assert.AreEqual("piglatin", ws2.Variant);
		}

		[Test]
		public void CanSaveAndReadDefaultFont()
		{
			_writingSystem.ISO = "en";
			_writingSystem.DefaultFontName = "Courier";
			_collection.SaveDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _collection.LoadDefinition("en");
			Assert.AreEqual("Courier", ws2.DefaultFontName);
		}

		[Test]
		public void CanSaveAndReadKeyboard()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Keyboard = new KeyboardDescriptor("Thai", Engines.None, "ThaiId");
			_collection.SaveDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _collection.LoadDefinition("en");
			Assert.AreEqual("Thai", ws2.Keyboard.KeyboardName);
			Assert.AreEqual(Engines.None, ws2.Keyboard.KeyboardingEngine);
			Assert.AreEqual("ThaiId", ws2.Keyboard.Id);
		}

		[Test]
		public void CanSaveAndReadRightToLeft()
		{
			_writingSystem.ISO = "en";
			Assert.IsFalse(_writingSystem.RightToLeftScript);
			_writingSystem.RightToLeftScript = true;
			Assert.IsTrue(_writingSystem.RightToLeftScript);
			_collection.SaveDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _collection.LoadDefinition("en");
			Assert.IsTrue(ws2.RightToLeftScript);
		}

		[Test]
		public void CanSaveAndReadIsUnicode()
		{
			_writingSystem.ISO = "en";
			Assert.IsFalse(_writingSystem.IsLegacyEncoded);
			_writingSystem.IsLegacyEncoded = true;
			Assert.IsTrue(_writingSystem.IsLegacyEncoded);
			_collection.SaveDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _collection.LoadDefinition("en");
			Assert.IsTrue(ws2.IsLegacyEncoded);
		}

		[Test]
		public void IsUnicode_TrueByDefault()
		{
			_writingSystem.ISO = "en";
			_collection.SaveDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _collection.LoadDefinition("en");
			Assert.IsFalse(ws2.IsLegacyEncoded);
		}

		[Test]
		public void CanRemoveVariant()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Variant = "piglatin";
			_collection.SaveDefinition(_writingSystem);
			string path = _collection.FilePathToWritingSystem(_writingSystem);

			AssertThatXmlIn.File(path ).HasAtLeastOneMatchForXpath( "ldml/identity/variant");
			_writingSystem.Variant = string.Empty;
			_collection.SaveDefinition(_writingSystem);
			AssertThatXmlIn.File(PathToWS).HasNoMatchForXpath("ldml/identity/variant");
		}


		[Test]
		public void CanRemoveAbbreviation()
		{

			_writingSystem.ISO = "en";
			_writingSystem.Abbreviation = "abbrev";
			_collection.SaveDefinition(_writingSystem);
			string path = _collection.FilePathToWritingSystem(_writingSystem);
			AssertThatXmlIn.File(path).HasAtLeastOneMatchForXpath("ldml/special/palaso:abbreviation", _namespaceManager);
			_writingSystem.Abbreviation = string.Empty;
			_collection.SaveDefinition(_writingSystem);
			AssertThatXmlIn.File(PathToWS).HasNoMatchForXpath("ldml/special/palaso:abbreviation", _namespaceManager);
		}

		[Test]
		public void WritesAbbreviationToLDML()
		{

			_writingSystem.ISO = "blah";
			_writingSystem.Abbreviation = "bl";
			_collection.SaveDefinition(_writingSystem);
			AssertThatXmlIn.File(PathToWS).HasAtLeastOneMatchForXpath("ldml/special/palaso:abbreviation[@value='bl']", _namespaceManager);
		}

		private string PathToWS
		{
			get
			{
				return _collection.FilePathToWritingSystem(_writingSystem);
			}
		}

		[Test]
		public void CanDeleteFileThatIsNotInTrash()
		{
			_writingSystem.ISO = "blah";
			_collection.SaveDefinition(_writingSystem);
			string path = Path.Combine(_collection.PathToWritingSystems, _collection.GetFileName(_writingSystem));
			Assert.IsTrue(File.Exists(path));
			_collection.Remove(_writingSystem.ISO);
			Assert.IsFalse(File.Exists(path));
			AssertFileIsInTrash(_writingSystem);
		}

		private void AssertFileIsInTrash(WritingSystemDefinition definition)
		{
			string path = Path.Combine(_collection.PathToWritingSystems, "trash");
			path = Path.Combine(path,_collection.GetFileName(definition));
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void CanDeleteFileMatchingOneThatWasPreviouslyTrashed()
		{
			_writingSystem.ISO = "blah";
			_collection.SaveDefinition(_writingSystem);
			_collection.Remove(_writingSystem.ISO);
			AssertFileIsInTrash(_writingSystem);
			WritingSystemDefinition ws2 = new WritingSystemDefinition {ISO = "blah"};
			_collection.SaveDefinition(ws2);
			_collection.Remove(ws2.ISO);
			string path = Path.Combine(_collection.PathToWritingSystems, _collection.GetFileName(_writingSystem));
			Assert.IsFalse(File.Exists(path));
			AssertFileIsInTrash(_writingSystem);
		}

		[Test]
		public void MarkedNotModifiedWhenNew()
		{
			//not worth saving until has some data
			Assert.IsFalse(_writingSystem.Modified);
		}

		[Test]
		public void MarkedAsModifiedWhenISOChanges()
		{
			_writingSystem.ISO = "foo";
			Assert.IsTrue(_writingSystem.Modified);
		}

		[Test]
		public void MarkedAsNotModifiedWhenLoaded()
		{
			_writingSystem.ISO = "blah";
			_collection.SaveDefinition(_writingSystem);
			WritingSystemDefinition ws2 = _collection.LoadDefinition("blah");
			Assert.IsFalse(ws2.Modified);
		}

		[Test]
		public void MarkedAsNotModifiedWhenSaved()
		{
			_writingSystem.ISO = "bla";
			Assert.IsTrue(_writingSystem.Modified);
			_collection.SaveDefinition(_writingSystem);
			Assert.IsFalse(_writingSystem.Modified);
			_writingSystem.ISO = "foo";
			Assert.IsTrue(_writingSystem.Modified);
		}

		[Test]
		public void LoadDefinitionCanFabricateEnglish()
		{
			//_collection.DontAddDefaultDefinitions = false;
			Assert.AreEqual("English",_collection.LoadDefinition("en-Latn").LanguageName);
		}

		[Test]
		public void DefaultDefinitionListIncludesActiveOSLanguages()
		{
		  //  _collection.DontAddDefaultDefinitions = false;
			_collection.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			_collection.LoadAllDefinitions();
			IEnumerable<WritingSystemDefinition> list = _collection.WritingSystemDefinitions;
			Assert.IsTrue(ContainsLanguageWithName(list, "test"));
		}

		[Test]
		public void DefaultLanguageNotAddedIfInTrash()
		{
		   // _collection.DontAddDefaultDefinitions = false;
			_collection.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			_collection.LoadAllDefinitions();
			IEnumerable<WritingSystemDefinition> list = _collection.WritingSystemDefinitions;
			Assert.IsTrue(ContainsLanguageWithName(list, "test"));
			IList<WritingSystemDefinition> list2 = new List<WritingSystemDefinition>(_collection.WritingSystemDefinitions);
			WritingSystemDefinition ws2 = list2[0];
			_collection.Remove(ws2.ISO);

			Palaso.WritingSystems.LdmlInFolderWritingSystemStore repository = new LdmlInFolderWritingSystemStore(_testPath);
		  //  repository.DontAddDefaultDefinitions = false;
			repository.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			Assert.IsFalse(ContainsLanguageWithName(repository.WritingSystemDefinitions, "test"));

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
					yield return new WritingSystemDefinition("tst", "", "", "", "test", "", false);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

		}
	}
}