using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using NUnit.Framework;

using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class LdmlInXmlWritingSystemStoreInterfaceTests : IWritingSystemStoreTests
	{
		public override IWritingSystemStore CreateNewStore()
		{
			return new LdmlInXmlWritingSystemStore();
		}
	}

	[TestFixture]
	public class LdmlInXmlWritingSystemCollectionTests
	{
		private string _testFilePath;
		private LdmlInXmlWritingSystemStore _writingSystemStore;
		private WritingSystemDefinition _writingSystem;

		[SetUp]
		public void Setup()
		{
			_writingSystem = new WritingSystemDefinition();
			_testFilePath = Path.GetTempFileName();
			_writingSystemStore = new LdmlInXmlWritingSystemStore();
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(_testFilePath);
		}

		private void WriteLdmlFile(string filePath, IEnumerable<WritingSystemDefinition> writingSystems)
		{
			XmlWriter xmlWriter = new XmlTextWriter(filePath, System.Text.Encoding.UTF8);
			xmlWriter.WriteStartDocument();

			xmlWriter.WriteStartElement("someroot");
			xmlWriter.WriteStartElement("writingsystems");
			LdmlAdaptor adaptor = new LdmlAdaptor();
			foreach (WritingSystemDefinition ws in writingSystems)
			{
				adaptor.Write(xmlWriter, ws, null);
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();
		}

		private void WriteAllDefinitionsToFile(string filePath, LdmlInXmlWritingSystemStore store)
		{
			XmlWriter xmlWriter = new XmlTextWriter(filePath, System.Text.Encoding.UTF8);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("someroot");
			store.SaveAllDefinitions(xmlWriter);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();
		}

		[Test]
		public void SaveAllToXmlReaderReadAsFile_ReadsBackCorrect()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition();
			ws1.ISO = "en";
			_writingSystemStore.Set(ws1);

			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2.ISO = "fr";
			_writingSystemStore.Set(ws2);
			Assert.AreEqual(2, _writingSystemStore.Count);

			XmlWriter xmlWriter = new XmlTextWriter(_testFilePath, System.Text.Encoding.UTF8);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("someroot");
			_writingSystemStore.SaveAllDefinitions(xmlWriter);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();

			LdmlInXmlWritingSystemStore testStore = new LdmlInXmlWritingSystemStore();
			Assert.AreEqual(0, testStore.Count);
			testStore.LoadAllDefinitions(_testFilePath);
			Assert.AreEqual(2, testStore.Count);
		}

		[Test]
		public void SaveAllToXmlReaderReadAsXmlReader_ReadsBackCorrect()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition();
			ws1.ISO = "en";
			_writingSystemStore.Set(ws1);

			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2.ISO = "fr";
			_writingSystemStore.Set(ws2);
			Assert.AreEqual(2, _writingSystemStore.Count);

			XmlWriter xmlWriter = new XmlTextWriter(_testFilePath, System.Text.Encoding.UTF8);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("someroot");
			_writingSystemStore.SaveAllDefinitions(xmlWriter);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();

			XmlReader xmlReader = new XmlTextReader(new StreamReader(_testFilePath));
			xmlReader.ReadToFollowing("writingsystems");

			LdmlInXmlWritingSystemStore testStore = new LdmlInXmlWritingSystemStore();
			Assert.AreEqual(0, testStore.Count);
			testStore.LoadAllDefinitions(xmlReader);
			Assert.AreEqual(2, testStore.Count);
			xmlReader.Close();

		}

#if false
		[Test]
		public void SavesWhenNotPreexisting()
		{
			_writingSystemStore.StoreDefinition(_writingSystem);
			AssertWritingSystemFileExists(_writingSystem);
		}

		private void AssertWritingSystemFileExists(WritingSystemDefinition writingSystem)
		{
			AssertWritingSystemFileExists(writingSystem, _writingSystemStore);
		}
		private void AssertWritingSystemFileExists(WritingSystemDefinition writingSystem, LdmlInStreamWritingSystemStore repository)
		{
			string path = repository.PathToWritingSystem(writingSystem);
			Assert.IsTrue(File.Exists(path));
		}
		[Test]
		public void FileNameWhenNothingKnown()
		{
			Assert.AreEqual("unknown.ldml", _writingSystemStore.GetFileName(_writingSystem));
		}

		[Test]
		public void FileNameWhenOnlyHaveIso()
		{

			_writingSystem.ISO = "en";
			Assert.AreEqual("en.ldml", _writingSystemStore.GetFileName(_writingSystem));
		}
		[Test]
		public void FileNameWhenHaveIsoAndRegion()
		{

			_writingSystem.ISO = "en";
			_writingSystem.Region = "us";
			Assert.AreEqual("en-us.ldml", _writingSystemStore.GetFileName(_writingSystem));
		}

		[Test]
		public void SavesWhenPreexisting()
		{
			_writingSystem.ISO = "en";
			_writingSystemStore.StoreDefinition(_writingSystem);
			_writingSystemStore.StoreDefinition(_writingSystem);

			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			_writingSystem.ISO = "en";
			_writingSystemStore.StoreDefinition(ws2);
		}



		[Test]
		public void RegressionWhereUnchangedDefDeleted()
		{
			_writingSystem.ISO = "blah";
			_writingSystemStore.StoreDefinition(_writingSystem);
			WritingSystemDefinition ws2 = _writingSystemStore.LoadDefinition("blah");
			_writingSystemStore.StoreDefinition(ws2);
			AssertWritingSystemFileExists(_writingSystem);
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			LdmlInStreamWritingSystemStore newRepository = new LdmlInStreamWritingSystemStore(Path.Combine(_testDir, "newguy"));
			newRepository.StoreDefinition(_writingSystem);
			AssertWritingSystemFileExists(_writingSystem, newRepository);
		}

		[Test]
		public void UpdatesFileNameWhenISOChanges()
		{
			_writingSystem.ISO = "1";
			_writingSystemStore.StoreDefinition(_writingSystem);
			string path = Path.Combine(_writingSystemStore.PathToWritingSystems, "1.ldml");
			Assert.IsTrue(File.Exists(path));
			WritingSystemDefinition ws2 = _writingSystemStore.LoadDefinition("1");
			ws2.ISO = "2";
			_writingSystemStore.StoreDefinition(ws2);
			Assert.IsFalse(File.Exists(path));
			path = Path.Combine(_writingSystemStore.PathToWritingSystems, "2.ldml");
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void MakesNewFileIfNeeded()
		{

			_writingSystem.ISO = "blah";
			_writingSystemStore.StoreDefinition(_writingSystem);
			AssertXmlFile.AtLeastOneMatch(PathToWS, "ldml/identity/language[@type='blah']");
		}

		[Test]
		public void CanSetVariantToLDMLUsingSameWS()
		{

			_writingSystemStore.StoreDefinition(_writingSystem);
			_writingSystem.Variant = "piglatin";
			_writingSystemStore.StoreDefinition(_writingSystem);
			AssertXmlFile.AtLeastOneMatch(PathToWS, "ldml/identity/variant[@type='piglatin']");
		}

		[Test]
		public void CanSetVariantToExistingLDML()
		{

			_writingSystem.ISO = "blah";
			_writingSystem.Abbreviation = "bl";//crucially, abbreviation isn't part of the name of the file
			_writingSystemStore.StoreDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2 = _writingSystemStore.LoadDefinition("blah");
			ws2.Variant = "piglatin";
			_writingSystemStore.StoreDefinition(ws2);
			string path = Path.Combine(_writingSystemStore.PathToWritingSystems, _writingSystemStore.GetFileName(ws2));
			AssertXmlFile.AtLeastOneMatch(path, "ldml/identity/variant[@type='piglatin']");
			AssertXmlFile.AtLeastOneMatch(path, "ldml/special/palaso:abbreviation[@value='bl']", LdmlAdaptor.MakeNameSpaceManager());
		}

		[Test]
		public void CanReadVariant()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Variant = "piglatin";
			_writingSystemStore.StoreDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _writingSystemStore.LoadDefinition("en-piglatin");
			Assert.AreEqual("piglatin", ws2.Variant);
		}

		[Test]
		public void CanSaveAndReadDefaultFont()
		{
			_writingSystem.ISO = "en";
			_writingSystem.DefaultFontName = "Courier";
			_writingSystemStore.StoreDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _writingSystemStore.LoadDefinition("en");
			Assert.AreEqual("Courier", ws2.DefaultFontName);
		}

		[Test]
		public void CanSaveAndReadKeyboardName()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Keyboard = "Thai";
			_writingSystemStore.StoreDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _writingSystemStore.LoadDefinition("en");
			Assert.AreEqual("Thai", ws2.Keyboard);
		}

		[Test]
		public void CanSaveAndReadRightToLeft()
		{
			_writingSystem.ISO = "en";
			Assert.IsFalse(_writingSystem.RightToLeftScript);
			_writingSystem.RightToLeftScript = true;
			Assert.IsTrue(_writingSystem.RightToLeftScript);
			_writingSystemStore.StoreDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _writingSystemStore.LoadDefinition("en");
			Assert.IsTrue(ws2.RightToLeftScript);
		}

		[Test]
		public void CanRemoveVariant()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Variant = "piglatin";
			_writingSystemStore.StoreDefinition(_writingSystem);
			string path = _writingSystemStore.PathToWritingSystem(_writingSystem);

			AssertXmlFile.AtLeastOneMatch(path, "ldml/identity/variant");
			_writingSystem.Variant = string.Empty;
			_writingSystemStore.StoreDefinition(_writingSystem);
			TestUtilities.AssertXPathIsNull(PathToWS, "ldml/identity/variant");
		}


		[Test]
		public void CanRemoveAbbreviation()
		{

			_writingSystem.ISO = "en";
			_writingSystem.Abbreviation = "abbrev";
			_writingSystemStore.StoreDefinition(_writingSystem);
			string path = _writingSystemStore.PathToWritingSystem(_writingSystem);
			AssertXmlFile.AtLeastOneMatch(path, "ldml/special/palaso:abbreviation", LdmlAdaptor.MakeNameSpaceManager());
			_writingSystem.Abbreviation = string.Empty;
			_writingSystemStore.StoreDefinition(_writingSystem);
			TestUtilities.AssertXPathIsNull(PathToWS, "ldml/special/palaso:abbreviation", LdmlAdaptor.MakeNameSpaceManager());
		}

		[Test]
		public void WritesAbbreviationToLDML()
		{

			_writingSystem.ISO = "blah";
			_writingSystem.Abbreviation = "bl";
			_writingSystemStore.StoreDefinition(_writingSystem);
			AssertXmlFile.AtLeastOneMatch(PathToWS, "ldml/special/palaso:abbreviation[@value='bl']", LdmlAdaptor.MakeNameSpaceManager());
		}

		private string PathToWS
		{
			get
			{
				return _writingSystemStore.PathToWritingSystem(_writingSystem);
			}
		}

		[Test]
		public void CanDeleteFileThatIsNotInTrash()
		{
			_writingSystem.ISO = "blah";
			_writingSystemStore.StoreDefinition(_writingSystem);
			string path = Path.Combine(_writingSystemStore.PathToWritingSystems, _writingSystemStore.GetFileName(_writingSystem));
			Assert.IsTrue(File.Exists(path));
			_writingSystemStore.DeleteDefinition(_writingSystem);
			Assert.IsFalse(File.Exists(path));
			AssertFileIsInTrash(_writingSystem);
		}

		private void AssertFileIsInTrash(WritingSystemDefinition definition)
		{
			string path = Path.Combine(_writingSystemStore.PathToWritingSystems, "trash");
			path = Path.Combine(path, _writingSystemStore.GetFileName(definition));
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void CanDeleteFileMatchingOneThatWasPreviouslyTrashed()
		{
			_writingSystem.ISO = "blah";
			_writingSystemStore.StoreDefinition(_writingSystem);
			_writingSystemStore.DeleteDefinition(_writingSystem);
			AssertFileIsInTrash(_writingSystem);
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2.ISO = "blah";
			_writingSystemStore.StoreDefinition(ws2);
			_writingSystemStore.DeleteDefinition(ws2);
			string path = Path.Combine(_writingSystemStore.PathToWritingSystems, _writingSystemStore.GetFileName(_writingSystem));
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
			_writingSystemStore.StoreDefinition(_writingSystem);
			WritingSystemDefinition ws2 = _writingSystemStore.LoadDefinition("blah");
			Assert.IsFalse(ws2.Modified);
		}
		[Test]
		public void MarkedAsNotModifiedWhenSaved()
		{
			_writingSystem.ISO = "bla";
			Assert.IsTrue(_writingSystem.Modified);
			_writingSystemStore.StoreDefinition(_writingSystem);
			Assert.IsFalse(_writingSystem.Modified);
			_writingSystem.ISO = "foo";
			Assert.IsTrue(_writingSystem.Modified);
		}

		[Test]
		public void LoadDefinitionCanFabricateEnglish()
		{
			_writingSystemStore.DontSetDefaultDefinitions = false;
			Assert.AreEqual("English", _writingSystemStore.LoadDefinition("en-Latn").LanguageName);
		}
		[Test]
		public void DefaultDefinitionListIncludesActiveOSLanguages()
		{
			_writingSystemStore.DontSetDefaultDefinitions = false;
			_writingSystemStore.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			IList<WritingSystemDefinition> list = _writingSystemStore.WritingSystemDefinitions;
			Assert.IsTrue(ContainsLanguageWithName(list, "test"));
		}

		[Test]
		public void DefaultLanguageNotSetedIfInTrash()
		{
			_writingSystemStore.DontSetDefaultDefinitions = false;
			_writingSystemStore.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			IList<WritingSystemDefinition> list = _writingSystemStore.WritingSystemDefinitions;
			Assert.IsTrue(ContainsLanguageWithName(list, "test"));
			WritingSystemDefinition ws2 = _writingSystemStore.WritingSystemDefinitions[0];
			Assert.IsNotNull(ws2);
			_writingSystemStore.DeleteDefinition(ws2);

			Palaso.WritingSystems.LdmlInStreamWritingSystemStore repository = new LdmlInStreamWritingSystemStore(_testDir);
			repository.DontSetDefaultDefinitions = false;
			repository.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			Assert.IsFalse(ContainsLanguageWithName(repository.WritingSystemDefinitions, "test"));

		}

		private bool ContainsLanguageWithName(IList<WritingSystemDefinition> list, string name)
		{
			foreach (WritingSystemDefinition definition in list)
			{
				if (definition.LanguageName == name)
					return true;
			}
			return false;
		}

		class DummyWritingSystemProvider : IWritingSystemProvider
		{
			#region IWritingSystemProvider Members

			public IEnumerable<WritingSystemDefinition> ActiveOSLanguages
			{
				get
				{
					yield return new WritingSystemDefinition("tst", "", "", "", "test", "", false);
				}
			}

			#endregion
		}
#endif
	}
}