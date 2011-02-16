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
		private string _testPath;
		private LdmlInFolderWritingSystemRepository _collection;
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
			_collection = new LdmlInFolderWritingSystemRepository(_testPath);
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
			Assert.AreEqual(_testPath, _collection.PathToWritingSystems);
		}

		[Test]
		public void SaveDefinitionsThenLoad_CountEquals2()
		{
			_writingSystem.ISO639 = "one";
			_collection.SaveDefinition(_writingSystem);
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2.ISO639 = "two";
			_collection.SaveDefinition(ws2);
			var newStore = new LdmlInFolderWritingSystemRepository(_testPath);

			Assert.AreEqual(2, newStore.Count);
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
			Assert.AreEqual("qaa.ldml", _collection.GetFileName(_writingSystem));
		}

		[Test]
		public void FileNameWhenOnlyHaveIso()
		{

			_writingSystem.ISO639 = "en";
			Assert.AreEqual("en.ldml", _collection.GetFileName(_writingSystem));
		}
		[Test]
		public void FileNameWhenHaveIsoAndRegion()
		{

			_writingSystem.ISO639 = "en";
			_writingSystem.Region = "us";
			Assert.AreEqual("en-us.ldml", _collection.GetFileName(_writingSystem));
		}

		[Test]
		public void SavesWhenPreexisting()
		{
			_writingSystem.ISO639 = "en";
			_collection.SaveDefinition(_writingSystem);
			_collection.SaveDefinition(_writingSystem);

			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			_writingSystem.ISO639 = "en";
			_collection.SaveDefinition(ws2);
		}

		[Test]
		public void RegressionWhereUnchangedDefDeleted()
		{
			_writingSystem.ISO639 = "qaa";
			_collection.SaveDefinition(_writingSystem);
			WritingSystemDefinition ws2 = _collection.Get(_writingSystem.StoreID);
			_collection.SaveDefinition(ws2);
			AssertWritingSystemFileExists(_writingSystem);
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			LdmlInFolderWritingSystemRepository newRepository = new LdmlInFolderWritingSystemRepository(Path.Combine(_testPath, "newguy"));
			newRepository.SaveDefinition(_writingSystem);
			AssertWritingSystemFileExists(_writingSystem,newRepository);
		}

		[Test]
		public void StoreIDAfterSave_SameAsFileNameWithoutExtension()
		{
			_writingSystem.ISO639 = "en";
			_collection.SaveDefinition(_writingSystem);
			Assert.AreEqual("en", _writingSystem.StoreID);
		}

		[Test]
		public void StoreIDAfterLoad_SameAsFileNameWithoutExtension()
		{
			_writingSystem.ISO639 = "en";
			Assert.AreNotEqual(0, Directory.GetFiles(_testPath, "*.ldml"));
			_collection.SaveDefinition(_writingSystem);
			var newStore = new LdmlInFolderWritingSystemRepository(_testPath);
			WritingSystemDefinition ws2 = newStore.Get("en");
			Assert.AreEqual(Path.GetFileNameWithoutExtension(Directory.GetFiles(_testPath, "*.ldml")[0]), ws2.StoreID);
		}

		[Test]
		public void UpdatesFileNameWhenISOChanges()
		{
			_writingSystem.ISO639 = "en";
			_collection.SaveDefinition(_writingSystem);
			string path = Path.Combine(_collection.PathToWritingSystems, "en.ldml");
			Assert.IsTrue(File.Exists(path));
			WritingSystemDefinition ws2 = _collection.Get(_writingSystem.StoreID);
			ws2.ISO639 = "de";
			Assert.AreEqual("en", ws2.StoreID);
			_collection.SaveDefinition(ws2);
			Assert.AreEqual("de", ws2.StoreID);
			Assert.IsFalse(File.Exists(path));
			path = Path.Combine(_collection.PathToWritingSystems, "de.ldml");
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void MakesNewFileIfNeeded()
		{

			_writingSystem.ISO639 = "en";
			_collection.SaveDefinition(_writingSystem);
			AssertThatXmlIn.File(PathToWS).HasAtLeastOneMatchForXpath("ldml/identity/language[@type='en']");
		}

		[Test]
		public void CanAddVariantToLDMLUsingSameWS()
		{

			_collection.SaveDefinition(_writingSystem);
			_writingSystem.Variant = "1901";
			_collection.SaveDefinition(_writingSystem);
			AssertThatXmlIn.File(PathToWS).HasAtLeastOneMatchForXpath("ldml/identity/variant[@type='1901']");
		}

		[Test]
		public void CanAddVariantToExistingLDML()
		{

			_writingSystem.ISO639 = "en";
			_writingSystem.Abbreviation = "bl";//crucially, abbreviation isn't part of the name of the file
			_collection.SaveDefinition(_writingSystem);

			var newCollection = new LdmlInFolderWritingSystemRepository(_testPath);
			WritingSystemDefinition ws2 = newCollection.Get(_writingSystem.StoreID);
			ws2.Variant = "x-piglatin";
			_collection.SaveDefinition(ws2);
			string path = Path.Combine(_collection.PathToWritingSystems, _collection.GetFileName(ws2));
			AssertThatXmlIn.File(path ).HasAtLeastOneMatchForXpath( "ldml/identity/variant[@type='x-piglatin']");
			AssertThatXmlIn.File(path ).HasAtLeastOneMatchForXpath( "ldml/special/palaso:abbreviation[@value='bl']", _namespaceManager);
		}

		[Test]
		public void CanReadVariant()
		{
			_writingSystem.ISO639 = "en";
			_writingSystem.Variant = "x-piglatin";
			_collection.SaveDefinition(_writingSystem);

			var newCollection = new LdmlInFolderWritingSystemRepository(_testPath);
			WritingSystemDefinition ws2 = newCollection.Get(_writingSystem.StoreID);
			Assert.AreEqual("x-piglatin", ws2.Variant);
		}

		[Test]
		public void CanSaveAndReadDefaultFont()
		{
			_writingSystem.ISO639 = "en";
			_writingSystem.DefaultFontName = "Courier";
			_collection.SaveDefinition(_writingSystem);

			var newCollection = new LdmlInFolderWritingSystemRepository(_testPath);
			WritingSystemDefinition ws2 = newCollection.Get("en");
			Assert.AreEqual("Courier", ws2.DefaultFontName);
		}

		[Test]
		public void CanSaveAndReadKeyboardName()
		{
			_writingSystem.ISO639 = "en";
			_writingSystem.Keyboard = "Thai";
			_collection.SaveDefinition(_writingSystem);

			var newCollection = new LdmlInFolderWritingSystemRepository(_testPath);
			WritingSystemDefinition ws2 = newCollection.Get("en");
			Assert.AreEqual("Thai", ws2.Keyboard);
		}

		[Test]
		public void CanSaveAndReadRightToLeft()
		{
			_writingSystem.ISO639 = "en";
			Assert.IsFalse(_writingSystem.RightToLeftScript);
			_writingSystem.RightToLeftScript = true;
			Assert.IsTrue(_writingSystem.RightToLeftScript);
			_collection.SaveDefinition(_writingSystem);

			var newCollection = new LdmlInFolderWritingSystemRepository(_testPath);
			WritingSystemDefinition ws2 = newCollection.Get("en");
			Assert.IsTrue(ws2.RightToLeftScript);
		}

		[Test]
		public void CanSaveAndReadIsUnicode()
		{
			_writingSystem.ISO639 = "en";
			Assert.IsFalse(_writingSystem.IsLegacyEncoded);
			_writingSystem.IsLegacyEncoded = true;
			Assert.IsTrue(_writingSystem.IsLegacyEncoded);
			_collection.SaveDefinition(_writingSystem);

			var newCollection = new LdmlInFolderWritingSystemRepository(_testPath);
			WritingSystemDefinition ws2 = newCollection.Get("en");
			Assert.IsTrue(ws2.IsLegacyEncoded);
		}

		[Test]
		public void IsLegacyEncoded_FalseByDefault()
		{
			_writingSystem.ISO639 = "en";
			_collection.SaveDefinition(_writingSystem);

			var newCollection = new LdmlInFolderWritingSystemRepository(_testPath);
			WritingSystemDefinition ws2 = newCollection.Get("en");
			Assert.IsFalse(ws2.IsLegacyEncoded);
		}

		[Test]
		public void CanRemoveVariant()
		{
			_writingSystem.ISO639 = "en";
			_writingSystem.Variant = "x-piglatin";
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

			_writingSystem.ISO639 = "en";
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

			_writingSystem.ISO639 = "en";
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
			_writingSystem.ISO639 = "en";
			_collection.SaveDefinition(_writingSystem);
			string path = Path.Combine(_collection.PathToWritingSystems, _collection.GetFileName(_writingSystem));
			Assert.IsTrue(File.Exists(path));
			_collection.Remove(_writingSystem.ISO639);
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
			_writingSystem.ISO639 = "en";
			_collection.SaveDefinition(_writingSystem);
			_collection.Remove(_writingSystem.StoreID);
			AssertFileIsInTrash(_writingSystem);
			WritingSystemDefinition ws2 = new WritingSystemDefinition {ISO639 = "en"};
			_collection.SaveDefinition(ws2);
			_collection.Remove(ws2.StoreID);
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
			_writingSystem.ISO639 = "en";
			Assert.IsTrue(_writingSystem.Modified);
		}

		[Test]
		public void MarkedAsNotModifiedWhenLoaded()
		{
			_writingSystem.ISO639 = "en";
			_collection.SaveDefinition(_writingSystem);
			var newCollection = new LdmlInFolderWritingSystemRepository(_testPath);
			WritingSystemDefinition ws2 = newCollection.Get(_writingSystem.StoreID);
			Assert.IsFalse(ws2.Modified);
		}

		[Test]
		public void MarkedAsNotModifiedWhenSaved()
		{
			_writingSystem.ISO639 = "en";
			Assert.IsTrue(_writingSystem.Modified);
			_collection.SaveDefinition(_writingSystem);
			Assert.IsFalse(_writingSystem.Modified);
			_writingSystem.ISO639 = "de";
			Assert.IsTrue(_writingSystem.Modified);
		}

		[Test]
		public void SystemWritingSystemProvider_Set_WritingSystemsAreIncludedInStore()
		{
			_collection.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			IEnumerable<WritingSystemDefinition> list = _collection.WritingSystemDefinitions;
			Assert.IsTrue(ContainsLanguageWithName(list, "English"));
		}

		[Test]
		public void DefaultLanguageNotAddedIfInTrash()
		{
			_collection.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			IEnumerable<WritingSystemDefinition> list = _collection.WritingSystemDefinitions;
			Assert.IsTrue(ContainsLanguageWithName(list, "English"));
			IList<WritingSystemDefinition> list2 = new List<WritingSystemDefinition>(_collection.WritingSystemDefinitions);
			WritingSystemDefinition ws2 = list2[0];
			_collection.Remove(ws2.ISO639);

			var repository = new LdmlInFolderWritingSystemRepository(_testPath);
		  //  repository.DontAddDefaultDefinitions = false;
			repository.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			Assert.IsFalse(ContainsLanguageWithName(repository.WritingSystemDefinitions, "English"));

		}

		[Test]
		public void Constructor_LdmlFolderStoreContainsMultipleFilesThatOnLoadDescribeWritingSystemsWithIdenticalRFC5646Tags_Throws()
		{
			File.WriteAllText(Path.Combine(_testPath, "de-Zxxx-x-audio.ldml"), GetLdmlFileContent("de-Zxxx-x-audio", "", "", ""));
			File.WriteAllText(Path.Combine(_testPath, "inconsistent-filename.ldml"), GetLdmlFileContent("de", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag));

			Assert.Throws<ApplicationException>(() => _collection = new LdmlInFolderWritingSystemRepository(_testPath));
		}

		[Test]
		//This is not really a problem, but it would be nice if the file were made consistant. So make we will make them run it through the migrator, which they should be using anyway.
		public void Constructor_LdmlFolderStoreContainsInconsistentlyNamedFile_Throws()
		{
			File.WriteAllText(Path.Combine(_testPath, "tpi-Zxxx-x-audio.ldml"), GetLdmlFileContent("de", "latn", "ch", "1901"));
			Assert.Throws<ApplicationException>(() => new LdmlInFolderWritingSystemRepository(_testPath));
		}

		private string GetLdmlFileContent(string language, string script, string region, string variant)
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