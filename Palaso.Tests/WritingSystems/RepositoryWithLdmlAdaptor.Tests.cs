using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class RepositoryWithLdmlAdaptorTests
	{
		private string _testDir;
		private LdmlInFolderWritingSystemRepository _repository;
		private WritingSystemDefinition _writingSystem;

		[SetUp]
		public void Setup()
		{
			_writingSystem = new WritingSystemDefinition();
			_testDir = Palaso.Tests.TestUtilities.GetTempTestDirectory();
			_repository = new LdmlInFolderWritingSystemRepository(_testDir);
			_repository.DontAddDefaultDefinitions = true;
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_testDir,true);
		}

		[Test]
		public void FindExistingWritingSystems()
		{
			_writingSystem.ISO = "one";
			_repository.SaveDefinition(_writingSystem);
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2.ISO = "two";
			_repository.SaveDefinition(ws2);

			Assert.AreEqual(2, _repository.WritingSystemDefinitions.Count);
		}

		[Test]
		public void SavesWhenNotPreexisting()
		{
			_repository.SaveDefinition(_writingSystem);
			AssertWritingSystemFileExists(_writingSystem);
		}

		private void AssertWritingSystemFileExists(WritingSystemDefinition writingSystem)
		{
			AssertWritingSystemFileExists(writingSystem,_repository);
		}
		private void AssertWritingSystemFileExists(WritingSystemDefinition writingSystem, LdmlInFolderWritingSystemRepository repository)
		{
			string path = repository.PathToWritingSystem(writingSystem);
			Assert.IsTrue(File.Exists(path));
		}
		[Test]
		public void FileNameWhenNothingKnown()
		{
			Assert.AreEqual("unknown.ldml", _repository.GetFileName(_writingSystem));
		}

		[Test]
		public void FileNameWhenOnlyHaveIso()
		{

			_writingSystem.ISO = "en";
			Assert.AreEqual("en.ldml", _repository.GetFileName(_writingSystem));
		}
		[Test]
		public void FileNameWhenHaveIsoAndRegion()
		{

			_writingSystem.ISO = "en";
			_writingSystem.Region = "us";
			Assert.AreEqual("en-us.ldml", _repository.GetFileName(_writingSystem));
		}

		[Test]
		public void SavesWhenPreexisting()
		{
			_writingSystem.ISO = "en";
			_repository.SaveDefinition(_writingSystem);
			_repository.SaveDefinition(_writingSystem);

			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			_writingSystem.ISO = "en";
			_repository.SaveDefinition(ws2);
		}



		[Test]
		public void RegressionWhereUnchangedDefDeleted()
		{
			_writingSystem.ISO = "blah";
			_repository.SaveDefinition(_writingSystem);
			WritingSystemDefinition ws2 = _repository.LoadDefinition("blah");
			_repository.SaveDefinition(ws2);
			AssertWritingSystemFileExists(_writingSystem);
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			LdmlInFolderWritingSystemRepository newRepository = new LdmlInFolderWritingSystemRepository(Path.Combine(_testDir, "newguy"));
			newRepository.SaveDefinition(_writingSystem);
			AssertWritingSystemFileExists(_writingSystem,newRepository);
		}

		[Test]
		public void UpdatesFileNameWhenISOChanges()
		{
			_writingSystem.ISO = "1";
			_repository.SaveDefinition(_writingSystem);
			string path = Path.Combine(_repository.PathToWritingSystems, "1.ldml");
			Assert.IsTrue(File.Exists(path));
			WritingSystemDefinition ws2 = _repository.LoadDefinition("1");
			ws2.ISO = "2";
			_repository.SaveDefinition(ws2);
			Assert.IsFalse(File.Exists(path));
			path = Path.Combine(_repository.PathToWritingSystems, "2.ldml");
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void MakesNewFileIfNeeded()
		{

			_writingSystem.ISO = "blah";
			_repository.SaveDefinition(_writingSystem);
			TestUtilities.AssertXPathNotNull(PathToWS, "ldml/identity/language[@type='blah']");
		}

		[Test]
		public void CanAddVariantToLDMLUsingSameWS()
		{

			_repository.SaveDefinition(_writingSystem);
			_writingSystem.Variant = "piglatin";
			_repository.SaveDefinition(_writingSystem);
			TestUtilities.AssertXPathNotNull(PathToWS, "ldml/identity/variant[@type='piglatin']");
		}

		[Test]
		public void CanAddVariantToExistingLDML()
		{

			_writingSystem.ISO = "blah";
			_writingSystem.Abbreviation = "bl";//crucially, abbreviation isn't part of the name of the file
			_repository.SaveDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2 = _repository.LoadDefinition("blah");
			ws2.Variant = "piglatin";
			_repository.SaveDefinition(ws2);
			string path = Path.Combine(_repository.PathToWritingSystems, _repository.GetFileName(ws2));
			TestUtilities.AssertXPathNotNull(path, "ldml/identity/variant[@type='piglatin']");
			TestUtilities.AssertXPathNotNull(path, "ldml/special/palaso:abbreviation[@value='bl']", LdmlAdaptor.MakeNameSpaceManager());
		}

		[Test]
		public void CanReadVariant()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Variant = "piglatin";
			_repository.SaveDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 =_repository.LoadDefinition("en-piglatin");
			Assert.AreEqual("piglatin", ws2.Variant);
		}

		[Test]
		public void CanSaveAndReadDefaultFont()
		{
			_writingSystem.ISO = "en";
			_writingSystem.DefaultFontName = "Courier";
			_repository.SaveDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _repository.LoadDefinition("en");
			Assert.AreEqual("Courier", ws2.DefaultFontName);
		}

		[Test]
		public void CanSaveAndReadKeyboardName()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Keyboard = "Thai";
			_repository.SaveDefinition(_writingSystem);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = _repository.LoadDefinition("en");
			Assert.AreEqual("Thai", ws2.Keyboard);
		}

		[Test]
		public void CanRemoveVariant()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Variant = "piglatin";
			_repository.SaveDefinition(_writingSystem);
			string path = _repository.PathToWritingSystem(_writingSystem);

			TestUtilities.AssertXPathNotNull(path, "ldml/identity/variant");
			_writingSystem.Variant = string.Empty;
			_repository.SaveDefinition(_writingSystem);
			TestUtilities.AssertXPathIsNull(PathToWS, "ldml/identity/variant");
		}


		[Test]
		public void CanRemoveAbbreviation()
		{

			_writingSystem.ISO = "en";
			_writingSystem.Abbreviation = "abbrev";
			_repository.SaveDefinition(_writingSystem);
			string path = _repository.PathToWritingSystem(_writingSystem);
			TestUtilities.AssertXPathNotNull(path, "ldml/special/palaso:abbreviation", LdmlAdaptor.MakeNameSpaceManager());
			_writingSystem.Abbreviation = string.Empty;
			_repository.SaveDefinition(_writingSystem);
			TestUtilities.AssertXPathIsNull(PathToWS, "ldml/special/palaso:abbreviation", LdmlAdaptor.MakeNameSpaceManager());
		}

		[Test]
		public void WritesAbbreviationToLDML()
		{

			_writingSystem.ISO = "blah";
			_writingSystem.Abbreviation = "bl";
			_repository.SaveDefinition(_writingSystem);
			TestUtilities.AssertXPathNotNull(PathToWS, "ldml/special/palaso:abbreviation[@value='bl']", LdmlAdaptor.MakeNameSpaceManager());
		}

		private string PathToWS
		{
			get
			{
				return _repository.PathToWritingSystem(_writingSystem);
			}
		}

		[Test]
		public void CanDeleteFile()
		{
			_writingSystem.ISO = "blah";
			_repository.SaveDefinition(_writingSystem);
			_repository.DeleteDefinition(_writingSystem);
			string path = Path.Combine(_repository.PathToWritingSystems, _repository.GetFileName(_writingSystem));
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
			_repository.SaveDefinition(_writingSystem);
			WritingSystemDefinition ws2 = _repository.LoadDefinition("blah");
			Assert.IsFalse(ws2.Modified);
		}
		[Test]
		public void MarkedAsNotModifiedWhenSaved()
		{
			_writingSystem.ISO = "bla";
			Assert.IsTrue(_writingSystem.Modified);
			_repository.SaveDefinition(_writingSystem);
			Assert.IsFalse(_writingSystem.Modified);
			_writingSystem.ISO = "foo";
			Assert.IsTrue(_writingSystem.Modified);
		}

		[Test]
		public void LoadDefinitionCanFabricateEnglish()
		{
			_repository.DontAddDefaultDefinitions = false;
			Assert.AreEqual("English",_repository.LoadDefinition("en-Latn").LanguageName);
		}
		[Test]
		public void DefaultDefinitionListIncludesActiveOSLanguages()
		{
			_repository.DontAddDefaultDefinitions = false;
			_repository.SystemWritingSystemProvider = new DummyWritingSystemProvider();
			IList<WritingSystemDefinition> list = _repository.WritingSystemDefinitions;
			Assert.IsTrue(ContainsLanguageWithName(list, "test"));
		}

		private bool ContainsLanguageWithName(IList<WritingSystemDefinition> list, string name)
		{
			foreach (WritingSystemDefinition definition in list)
			{
				if(definition.LanguageName == name)
					return true;
			}
			return false;
		}

		class DummyWritingSystemProvider : IWritingSystemProvider
		{
			#region IWritingSystemProvider Members

			public IEnumerable<WritingSystemDefinition> ActiveOSLanguages()
			{
				yield return new WritingSystemDefinition("tst", "", "", "", "test", "");
			}

			#endregion
		}
	}
}