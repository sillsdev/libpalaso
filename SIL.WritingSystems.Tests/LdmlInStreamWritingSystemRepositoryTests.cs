using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class LdmlInXmlWritingSystemRepositoryInterfaceTests : IWritingSystemRepositoryTests
	{
		public override IWritingSystemRepository CreateNewStore()
		{
			return new LdmlInXmlWritingSystemRepository();
		}
	}

	[TestFixture]
	public class LdmlInXmlWritingSystemRepositoryTests
	{
		private string _testFilePath;
		private LdmlInXmlWritingSystemRepository _writingSystemRepository;
		private WritingSystemDefinition _writingSystem;

		[SetUp]
		public void Setup()
		{
			_writingSystem = new WritingSystemDefinition();
			_testFilePath = Path.GetTempFileName();
			_writingSystemRepository = new LdmlInXmlWritingSystemRepository();
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
			LdmlDataMapper adaptor = new LdmlDataMapper();
			foreach (WritingSystemDefinition ws in writingSystems)
			{
				adaptor.Write(xmlWriter, ws, null);
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();
		}

		private void WriteAllDefinitionsToFile(string filePath, LdmlInXmlWritingSystemRepository repository)
		{
			XmlWriter xmlWriter = new XmlTextWriter(filePath, System.Text.Encoding.UTF8);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("someroot");
			repository.SaveAllDefinitions(xmlWriter);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();
		}

		[Test]
		public void SaveAllToXmlReaderReadAsFile_ReadsBackCorrect()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition();
			ws1.Language = "en";
			_writingSystemRepository.Set(ws1);

			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2.Language = "fr";
			_writingSystemRepository.Set(ws2);
			Assert.AreEqual(2, _writingSystemRepository.Count);

			XmlWriter xmlWriter = new XmlTextWriter(_testFilePath, System.Text.Encoding.UTF8);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("someroot");
			_writingSystemRepository.SaveAllDefinitions(xmlWriter);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();

			LdmlInXmlWritingSystemRepository testRepository = new LdmlInXmlWritingSystemRepository();
			Assert.AreEqual(0, testRepository.Count);
			testRepository.LoadAllDefinitions(_testFilePath);
			Assert.AreEqual(2, testRepository.Count);
		}

		[Test]
		public void SaveAllToXmlReaderReadAsXmlReader_ReadsBackCorrect()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition();
			ws1.Language = "en";
			_writingSystemRepository.Set(ws1);

			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2.Language = "fr";
			_writingSystemRepository.Set(ws2);
			Assert.AreEqual(2, _writingSystemRepository.Count);

			XmlWriter xmlWriter = new XmlTextWriter(_testFilePath, System.Text.Encoding.UTF8);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("someroot");
			_writingSystemRepository.SaveAllDefinitions(xmlWriter);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();

			XmlReader xmlReader = new XmlTextReader(new StreamReader(_testFilePath));
			xmlReader.ReadToFollowing("writingsystems");

			LdmlInXmlWritingSystemRepository testRepository = new LdmlInXmlWritingSystemRepository();
			Assert.AreEqual(0, testRepository.Count);
			testRepository.LoadAllDefinitions(xmlReader);
			Assert.AreEqual(2, testRepository.Count);
			xmlReader.Close();

		}

#if false
		[Test]
		public void SavesWhenNotPreexisting()
		{
			_writingSystemRepository.StoreDefinition(_writingSystem);
			AssertWritingSystemFileExists(_writingSystem);
		}

		private void AssertWritingSystemFileExists(WritingSystemDefinition writingSystem)
		{
			AssertWritingSystemFileExists(writingSystem, _writingSystemRepository);
		}
		private void AssertWritingSystemFileExists(WritingSystemDefinition writingSystem, LdmlInXmlWritingSystemRepository repository)
		{
			string path = repository.PathToWritingSystem(writingSystem);
			Assert.IsTrue(File.Exists(path));
		}
		[Test]
		public void FileNameWhenNothingKnown()
		{
			Assert.AreEqual("unknown.ldml", _writingSystemRepository.GetFileName(_writingSystem));
		}

		[Test]
		public void FileNameWhenOnlyHaveIso()
		{

			_writingSystem.ISO = "en";
			Assert.AreEqual("en.ldml", _writingSystemRepository.GetFileName(_writingSystem));
		}
		[Test]
		public void FileNameWhenHaveIsoAndRegion()
		{

			_writingSystem.ISO = "en";
			_writingSystem.Region = "us";
			Assert.AreEqual("en-us.ldml", _writingSystemRepository.GetFileName(_writingSystem));
		}

		[Test]
		public void SavesWhenPreexisting()
		{
			_writingSystem.ISO = "en";
			_writingSystemRepository.StoreDefinition(_writingSystem);
			_writingSystemRepository.StoreDefinition(_writingSystem);

			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			_writingSystem.ISO = "en";
			_writingSystemRepository.StoreDefinition(ws2);
		}



		[Test]
		public void RegressionWhereUnchangedDefDeleted()
		{
			_writingSystem.ISO = "blah";
			_writingSystemRepository.StoreDefinition(_writingSystem);
			WritingSystemDefinition ws2 = _writingSystemRepository.LoadDefinition("blah");
			_writingSystemRepository.StoreDefinition(ws2);
			AssertWritingSystemFileExists(_writingSystem);
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			var newRepository = new LdmlInXmlWritingSystemRepository();
			newRepository.StoreDefinition(_writingSystem);
			AssertWritingSystemFileExists(_writingSystem, newRepository);
		}

		[Test]
		public void UpdatesFileNameWhenISOChanges()
		{
			_writingSystem.ISO = "1";
			_writingSystemRepository.StoreDefinition(_writingSystem);
			string path = Path.Combine(_writingSystemRepository.PathToWritingSystems, "1.ldml");
			Assert.IsTrue(File.Exists(path));
			WritingSystemDefinition ws2 = _writingSystemRepository.LoadDefinition("1");
			ws2.ISO = "2";
			_writingSystemRepository.StoreDefinition(ws2);
			Assert.IsFalse(File.Exists(path));
			path = Path.Combine(_writingSystemRepository.PathToWritingSystems, "2.ldml");
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void MakesNewFileIfNeeded()
		{

			_writingSystem.ISO = "blah";
			_writingSystemRepository.StoreDefinition(_writingSystem);
			AssertXmlFile.AtLeastOneMatch(PathToWS, "ldml/identity/language[@type='blah']");
		}

		[Test]
		public void CanSetVariantToLDMLUsingSameWS()
		{

			_writingSystemRepository.StoreDefinition(_writingSystem);
			_writingSystem.Variant = "piglatin";
			_writingSystemRepository.StoreDefinition(_writingSystem);
			AssertXmlFile.AtLeastOneMatch(PathToWS, "ldml/identity/variant[@type='piglatin']");
		}

		[Test]
		public void CanSetVariantToExistingLDML()
		{

			_writingSystem.ISO = "blah";
			_writingSystem.Abbreviation = "bl";//crucially, abbreviation isn't part of the name of the file
			_writingSystemRepository.StoreDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2 = _writingSystemRepository.LoadDefinition("blah");
			ws2.Variant = "piglatin";
			_writingSystemRepository.StoreDefinition(ws2);
			string path = Path.Combine(_writingSystemRepository.PathToWritingSystems, _writingSystemRepository.GetFileName(ws2));
			AssertXmlFile.AtLeastOneMatch(path, "ldml/identity/variant[@type='piglatin']");
			AssertXmlFile.AtLeastOneMatch(path, "ldml/special/palaso:abbreviation[@value='bl']", LdmlAdaptor.MakeNameSpaceManager());
		}

		[Test]
		public void CanReadVariant()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Variant = "piglatin";
			_writingSystemRepository.StoreDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _writingSystemRepository.LoadDefinition("en-piglatin");
			Assert.AreEqual("piglatin", ws2.Variant);
		}

		[Test]
		public void CanSaveAndReadDefaultFont()
		{
			_writingSystem.ISO = "en";
			_writingSystem.DefaultFontName = "Courier";
			_writingSystemRepository.StoreDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _writingSystemRepository.LoadDefinition("en");
			Assert.AreEqual("Courier", ws2.DefaultFontName);
		}

		[Test]
		public void CanSaveAndReadKeyboardName()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Keyboard = "Thai";
			_writingSystemRepository.StoreDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _writingSystemRepository.LoadDefinition("en");
			Assert.AreEqual("Thai", ws2.Keyboard);
		}

		[Test]
		public void CanSaveAndReadRightToLeft()
		{
			_writingSystem.ISO = "en";
			Assert.IsFalse(_writingSystem.RightToLeftScript);
			_writingSystem.RightToLeftScript = true;
			Assert.IsTrue(_writingSystem.RightToLeftScript);
			_writingSystemRepository.StoreDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _writingSystemRepository.LoadDefinition("en");
			Assert.IsTrue(ws2.RightToLeftScript);
		}

		[Test]
		public void CanRemoveVariant()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Variant = "piglatin";
			_writingSystemRepository.StoreDefinition(_writingSystem);
			string path = _writingSystemRepository.PathToWritingSystem(_writingSystem);

			AssertXmlFile.AtLeastOneMatch(path, "ldml/identity/variant");
			_writingSystem.Variant = string.Empty;
			_writingSystemRepository.StoreDefinition(_writingSystem);
			TestUtilities.AssertXPathIsNull(PathToWS, "ldml/identity/variant");
		}


		[Test]
		public void CanRemoveAbbreviation()
		{

			_writingSystem.ISO = "en";
			_writingSystem.Abbreviation = "abbrev";
			_writingSystemRepository.StoreDefinition(_writingSystem);
			string path = _writingSystemRepository.PathToWritingSystem(_writingSystem);
			AssertXmlFile.AtLeastOneMatch(path, "ldml/special/palaso:abbreviation", LdmlAdaptor.MakeNameSpaceManager());
			_writingSystem.Abbreviation = string.Empty;
			_writingSystemRepository.StoreDefinition(_writingSystem);
			TestUtilities.AssertXPathIsNull(PathToWS, "ldml/special/palaso:abbreviation", LdmlAdaptor.MakeNameSpaceManager());
		}

		[Test]
		public void WritesAbbreviationToLDML()
		{

			_writingSystem.ISO = "blah";
			_writingSystem.Abbreviation = "bl";
			_writingSystemRepository.StoreDefinition(_writingSystem);
			AssertXmlFile.AtLeastOneMatch(PathToWS, "ldml/special/palaso:abbreviation[@value='bl']", LdmlAdaptor.MakeNameSpaceManager());
		}

		private string PathToWS
		{
			get
			{
				return _writingSystemRepository.PathToWritingSystem(_writingSystem);
			}
		}

		[Test]
		public void CanDeleteFileThatIsNotInTrash()
		{
			_writingSystem.ISO = "blah";
			_writingSystemRepository.StoreDefinition(_writingSystem);
			string path = Path.Combine(_writingSystemRepository.PathToWritingSystems, _writingSystemRepository.GetFileName(_writingSystem));
			Assert.IsTrue(File.Exists(path));
			_writingSystemRepository.DeleteDefinition(_writingSystem);
			Assert.IsFalse(File.Exists(path));
			AssertFileIsInTrash(_writingSystem);
		}

		private void AssertFileIsInTrash(WritingSystemDefinition definition)
		{
			string path = Path.Combine(_writingSystemRepository.PathToWritingSystems, "trash");
			path = Path.Combine(path, _writingSystemRepository.GetFileName(definition));
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void CanDeleteFileMatchingOneThatWasPreviouslyTrashed()
		{
			_writingSystem.ISO = "blah";
			_writingSystemRepository.StoreDefinition(_writingSystem);
			_writingSystemRepository.DeleteDefinition(_writingSystem);
			AssertFileIsInTrash(_writingSystem);
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2.ISO = "blah";
			_writingSystemRepository.StoreDefinition(ws2);
			_writingSystemRepository.DeleteDefinition(ws2);
			string path = Path.Combine(_writingSystemRepository.PathToWritingSystems, _writingSystemRepository.GetFileName(_writingSystem));
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
			_writingSystemRepository.StoreDefinition(_writingSystem);
			WritingSystemDefinition ws2 = _writingSystemRepository.LoadDefinition("blah");
			Assert.IsFalse(ws2.Modified);
		}
		[Test]
		public void MarkedAsNotModifiedWhenSaved()
		{
			_writingSystem.ISO = "bla";
			Assert.IsTrue(_writingSystem.Modified);
			_writingSystemRepository.StoreDefinition(_writingSystem);
			Assert.IsFalse(_writingSystem.Modified);
			_writingSystem.ISO = "foo";
			Assert.IsTrue(_writingSystem.Modified);
		}

		[Test]
		public void LoadDefinitionCanFabricateEnglish()
		{
			_writingSystemRepository.DontSetDefaultDefinitions = false;
			Assert.AreEqual("English", _writingSystemRepository.LoadDefinition("en-Latn").LanguageName);
		}
		[Test]
		public void DefaultDefinitionListIncludesActiveOSLanguages()
		{
			_writingSystemRepository.DontSetDefaultDefinitions = false;
			_writingSystemRepository.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			IList<WritingSystemDefinition> list = _writingSystemRepository.WritingSystemDefinitions;
			Assert.IsTrue(ContainsLanguageWithName(list, "test"));
		}

		[Test]
		public void DefaultLanguageNotSetedIfInTrash()
		{
			_writingSystemRepository.DontSetDefaultDefinitions = false;
			_writingSystemRepository.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			IList<WritingSystemDefinition> list = _writingSystemRepository.WritingSystemDefinitions;
			Assert.IsTrue(ContainsLanguageWithName(list, "test"));
			WritingSystemDefinition ws2 = _writingSystemRepository.WritingSystemDefinitions[0];
			Assert.IsNotNull(ws2);
			_writingSystemRepository.DeleteDefinition(ws2);

			var repository = new LdmlInXmlWritingSystemRepository(_testDir);
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

		class DummyWritingSystemProvider : IEnumerable<WritingSystemDefinition>
		{
			#region IEnumerable<WritingSystemDefinition> Members

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