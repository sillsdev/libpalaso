using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Palaso;
using Palaso.Tests;

namespace WritingSystemSetup.Tests
{
	/// <summary>
	/// These are tests about things related to the file system,
	/// not the contents of the ldml file.
	/// </summary>
	[TestFixture]
	public class WritingSystemFileTests
	{
		private string _testDir;
		private WritingSystemRepository _repository;
		private WritingSystemDefinition _writingSystem;
		private LdmlAdaptor _adaptor;


		[SetUp]
		public void Setup()
		{
			_writingSystem = new WritingSystemDefinition();
			_adaptor = new LdmlAdaptor(_writingSystem);
			_testDir = Palaso.Tests.TestUtilities.GetTempTestDirectory();
			_repository = new WritingSystemRepository(_testDir);
		}

		[TearDown]
		public void TearDown()
		{
			//Palaso.Tests.TestUtilities.DeleteFolderThatMayBeInUse(_testDir);
			Directory.Delete(_testDir,true);
		}

		[Test]
		public void SavesWhenNotPreexisting()
		{
			_adaptor.SaveToRepository(_repository);
			AssertWritingSystemFileExists(_writingSystem);
		}

		private void AssertWritingSystemFileExists(WritingSystemDefinition _writingSystem)
		{
		   AssertWritingSystemFileExists(_writingSystem,_repository);
		}
		private void AssertWritingSystemFileExists(WritingSystemDefinition _writingSystem, WritingSystemRepository repository)
		{
			string path = Path.Combine(repository.PathToWritingSystems, _adaptor.FileName);
			Assert.IsTrue(File.Exists(path));
		}
		[Test]
		public void FileNameWhenNothingKnown()
		{
			Assert.AreEqual("unknown.ldml", _adaptor.FileName);
		}

		[Test]
		public void FileNameWhenOnlyHaveIso()
		{

			_writingSystem.ISO = "en";
			Assert.AreEqual("en.ldml", _adaptor.FileName);
		}
		[Test]
		public void FileNameWhenHaveIsoAndRegion()
		{

			_writingSystem.ISO = "en";
			_writingSystem.Region = "us";
			Assert.AreEqual("en-us.ldml", _adaptor.FileName);
		}

		[Test]
		public void SavesWhenPreexisting()
		{
			_writingSystem.ISO = "en";
			_adaptor.SaveToRepository(_repository);
			_adaptor.SaveToRepository(_repository);

			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			LdmlAdaptor adaptor2 = new LdmlAdaptor(ws2);
			_writingSystem.ISO = "en";
			adaptor2.SaveToRepository(_repository);
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			WritingSystemRepository newGuy = new WritingSystemRepository(Path.Combine(_testDir, "newguy"));

			_adaptor.SaveToRepository(newGuy);
			AssertWritingSystemFileExists(_writingSystem,newGuy);
		}

		[Test]
		public void UpdatesFileNameWhenComponentsChange()
		{
			_writingSystem.ISO = "en";
			_adaptor.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, "en.ldml");
			Assert.IsTrue(File.Exists(path));
			_writingSystem.ISO = "blah";
			_writingSystem.Region = "foo";
			_adaptor.SaveToRepository(_repository);
			Assert.IsFalse(File.Exists(path));
			 path = Path.Combine(_repository.PathToWritingSystems, "blah-foo.ldml");
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void MakesNewFileIfNeeded()
		{

			_writingSystem.ISO = "blah";
			_adaptor.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, _adaptor.FileName);
			TestUtilities.AssertXPathNotNull(path, "ldml/identity/language[@type='blah']");
		}

		[Test]
		public void CanAddVariantToLDMLUsingSameWS()
		{

			_adaptor.SaveToRepository(_repository);
			_writingSystem.Variant = "piglatin";
			_adaptor.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, _adaptor.FileName);
			TestUtilities.AssertXPathNotNull(path, "ldml/identity/variant[@type='piglatin']");
		}

		[Test]
		public void CanAddVariantToExistingLDML()
		{

			_writingSystem.ISO = "blah";
			_writingSystem.Abbreviation = "bl";//crucially, abbreviation isn't part of the name of the file
			_adaptor.SaveToRepository(_repository);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			LdmlAdaptor adaptor2 = new LdmlAdaptor(ws2);
			adaptor2.Load(_repository, "blah");
			ws2.Variant = "piglatin";
			adaptor2.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, adaptor2.FileName);
			TestUtilities.AssertXPathNotNull(path, "ldml/identity/variant[@type='piglatin']");
			TestUtilities.AssertXPathNotNull(path, "ldml/special/palaso:abbreviation[@value='bl']", LdmlAdaptor.MakeNameSpaceManager());
		}

		[Test]
		public void CanReadVariant()
		{
			_writingSystem.ISO = "en";
			_writingSystem.Variant = "piglatin";
			_adaptor.SaveToRepository(_repository);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			LdmlAdaptor adaptor2 = new LdmlAdaptor(ws2);
			adaptor2.Load(_repository, "en-piglatin");
			Assert.AreEqual("piglatin", ws2.Variant);
		 }

		 [Test]
		 public void CanRemoveVariant()
		 {

			 _writingSystem.ISO = "en";
			 _writingSystem.Variant = "piglatin";
			 _adaptor.SaveToRepository(_repository);
			 string path = Path.Combine(_repository.PathToWritingSystems, _adaptor.FileName);
			 TestUtilities.AssertXPathNotNull(path, "ldml/identity/variant");
			 _writingSystem.Variant = string.Empty;
			 _adaptor.SaveToRepository(_repository);
				path = Path.Combine(_repository.PathToWritingSystems, _adaptor.FileName);
			 TestUtilities.AssertXPathIsNull(path, "ldml/identity/variant");
		 }


		[Test]
		public void CanRemoveAbbreviation()
		{

			_writingSystem.ISO = "en";
			_writingSystem.Abbreviation = "abbrev";
			_adaptor.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, _adaptor.FileName);
			TestUtilities.AssertXPathNotNull(path, "ldml/special/palaso:abbreviation", LdmlAdaptor.MakeNameSpaceManager());
			_writingSystem.Abbreviation = string.Empty;
			_adaptor.SaveToRepository(_repository);
			path = Path.Combine(_repository.PathToWritingSystems, _adaptor.FileName);
			TestUtilities.AssertXPathIsNull(path, "ldml/special/palaso:abbreviation", LdmlAdaptor.MakeNameSpaceManager());
		}

		[Test]
		public void WritesAbbreviationToLDML()
		{

			_writingSystem.ISO = "blah";
			_writingSystem.Abbreviation = "bl";
			_adaptor.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, _adaptor.FileName);
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