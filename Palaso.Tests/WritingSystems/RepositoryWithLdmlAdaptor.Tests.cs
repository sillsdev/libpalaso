using System.IO;
using NUnit.Framework;

namespace Palaso.Tests
{

	[TestFixture]
	public class RepositoryWithLdmlAdaptorTests
	{
		private string _testDir;
		private WritingSystemRepository _repository;
		private WritingSystemDefinition _writingSystem;

		[SetUp]
		public void Setup()
		{
			_writingSystem = new WritingSystemDefinition();
			_testDir = Palaso.Tests.TestUtilities.GetTempTestDirectory();
			_repository = new WritingSystemRepository(_testDir);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_testDir,true);
		}

		[Test]
		public void SavesWhenNotPreexisting()
		{
			_repository.SaveDefinition(_writingSystem);
			AssertWritingSystemFileExists(_writingSystem);
		}

		private void AssertWritingSystemFileExists(WritingSystemDefinition _writingSystem)
		{
		   AssertWritingSystemFileExists(_writingSystem,_repository);
		}
		private void AssertWritingSystemFileExists(WritingSystemDefinition _writingSystem, WritingSystemRepository repository)
		{
			string path = Path.Combine(repository.PathToWritingSystems, _repository.GetFileName(_writingSystem));
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
		public void SavesWhenDirectoryNotFound()
		{
			WritingSystemRepository newRepository = new WritingSystemRepository(Path.Combine(_testDir, "newguy"));
			newRepository.SaveDefinition(_writingSystem);
			AssertWritingSystemFileExists(_writingSystem,newRepository);
		}

		[Test]
		public void UpdatesFileNameWhenComponentsChange()
		{
			_writingSystem.ISO = "en";
			_repository.SaveDefinition(_writingSystem);
			string path = Path.Combine(_repository.PathToWritingSystems, "en.ldml");
			Assert.IsTrue(File.Exists(path));
			_writingSystem.ISO = "blah";
			_writingSystem.Region = "foo";
			_repository.SaveDefinition(_writingSystem);
			Assert.IsFalse(File.Exists(path));
			 path = Path.Combine(_repository.PathToWritingSystems, "blah-foo.ldml");
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void MakesNewFileIfNeeded()
		{

			_writingSystem.ISO = "blah";
			_repository.SaveDefinition(_writingSystem);
			string path = Path.Combine(_repository.PathToWritingSystems, _repository.GetFileName(_writingSystem));
			TestUtilities.AssertXPathNotNull(path, "ldml/identity/language[@type='blah']");
		}

		[Test]
		public void CanAddVariantToLDMLUsingSameWS()
		{

			_repository.SaveDefinition(_writingSystem);
			_writingSystem.Variant = "piglatin";
			_repository.SaveDefinition(_writingSystem);
			string path = Path.Combine(_repository.PathToWritingSystems, _repository.GetFileName(_writingSystem));
			TestUtilities.AssertXPathNotNull(path, "ldml/identity/variant[@type='piglatin']");
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
		 public void CanRemoveVariant()
		 {
			 _writingSystem.ISO = "en";
			 _writingSystem.Variant = "piglatin";
			 _repository.SaveDefinition(_writingSystem);
			 string path = Path.Combine(_repository.PathToWritingSystems, _repository.GetFileName(_writingSystem));
			 TestUtilities.AssertXPathNotNull(path, "ldml/identity/variant");
			 _writingSystem.Variant = string.Empty;
			 _repository.SaveDefinition(_writingSystem);
				path = Path.Combine(_repository.PathToWritingSystems, _repository.GetFileName(_writingSystem));
			 TestUtilities.AssertXPathIsNull(path, "ldml/identity/variant");
		 }


		[Test]
		public void CanRemoveAbbreviation()
		{

			_writingSystem.ISO = "en";
			_writingSystem.Abbreviation = "abbrev";
			_repository.SaveDefinition(_writingSystem);
			string path = Path.Combine(_repository.PathToWritingSystems, _repository.GetFileName(_writingSystem));
			TestUtilities.AssertXPathNotNull(path, "ldml/special/palaso:abbreviation", LdmlAdaptor.MakeNameSpaceManager());
			_writingSystem.Abbreviation = string.Empty;
			_repository.SaveDefinition(_writingSystem);
			path = Path.Combine(_repository.PathToWritingSystems, _repository.GetFileName(_writingSystem));
			TestUtilities.AssertXPathIsNull(path, "ldml/special/palaso:abbreviation", LdmlAdaptor.MakeNameSpaceManager());
		}

		[Test]
		public void WritesAbbreviationToLDML()
		{

			_writingSystem.ISO = "blah";
			_writingSystem.Abbreviation = "bl";
			_repository.SaveDefinition(_writingSystem);
			string path = Path.Combine(_repository.PathToWritingSystems, _repository.GetFileName(_writingSystem));
			TestUtilities.AssertXPathNotNull(path, "ldml/special/palaso:abbreviation[@value='bl']", LdmlAdaptor.MakeNameSpaceManager());
		}

		[Test, Ignore()]
		public void ReadsAbbreviationFromLDML()
		{
		}

		[Test, Ignore()]
		public void CanAddModifyVariantInLDML()
		{

		}
	}

}