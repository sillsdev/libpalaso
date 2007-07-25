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

		[SetUp]
		public void Setup()
		{
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
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SaveToRepository(_repository);
			AssertWritingSystemFileExists(ws);
		}

		private void AssertWritingSystemFileExists(WritingSystemDefinition ws)
		{
		   AssertWritingSystemFileExists(ws,_repository);
		}
		private void AssertWritingSystemFileExists(WritingSystemDefinition ws, WritingSystemRepository repository)
		{
			string path = Path.Combine(repository.PathToWritingSystems, ws.FileName);
			Assert.IsTrue(File.Exists(path));
		}
		[Test]
		public void FileNameWhenNothingKnown()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.AreEqual("unknown.ldml", ws.FileName);
		}

		[Test]
		public void FileNameWhenOnlyHaveIso()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "en";
			Assert.AreEqual("en.ldml", ws.FileName);
		}
		[Test]
		public void FileNameWhenHaveIsoAndRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "en";
			ws.Region = "us";
			Assert.AreEqual("en-us.ldml", ws.FileName);
		}

		[Test]
		public void SavesWhenPreexisting()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "en";
			ws.SaveToRepository(_repository);
			ws.SaveToRepository(_repository);

			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws.ISO = "en";
			ws.SaveToRepository(_repository);
		}

		[Test]
		public void SavesWhenDirectoryNotFound()
		{
			WritingSystemRepository newGuy = new WritingSystemRepository(Path.Combine(_testDir, "newguy"));
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SaveToRepository(newGuy);
			AssertWritingSystemFileExists(ws,newGuy);
		}

		[Test]
		public void UpdatesFileNameWhenComponentsChange()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "en";
			ws.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, "en.ldml");
			Assert.IsTrue(File.Exists(path));
			ws.ISO = "blah";
			ws.Region = "foo";
			ws.SaveToRepository(_repository);
			Assert.IsFalse(File.Exists(path));
			 path = Path.Combine(_repository.PathToWritingSystems, "blah-foo.ldml");
			Assert.IsTrue(File.Exists(path));
		}

		[Test]
		public void MakesNewFileIfNeeded()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "blah";
			ws.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, ws.FileName);
			TestUtilities.AssertXPathNotNull(path, "ldml/identity/language[@type='blah']");
		}

		[Test]
		public void CanAddVariantToLDMLUsingSameWS()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SaveToRepository(_repository);
			ws.Variant = "piglatin";
			ws.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, ws.FileName);
			TestUtilities.AssertXPathNotNull(path, "ldml/identity/variant[@type='piglatin']");
		}

		[Test]
		public void CanAddVariantToExistingLDML()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "blah";
			ws.Abbreviation = "bl";//crucially, abbreviation isn't part of the name of the file
			ws.SaveToRepository(_repository);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = new WritingSystemDefinition(_repository, "blah");
			ws2.Variant = "piglatin";
			ws2.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, ws2.FileName);
			TestUtilities.AssertXPathNotNull(path, "ldml/identity/variant[@type='piglatin']");
			TestUtilities.AssertXPathNotNull(path, "ldml/special/palaso:abbreviation[@value='bl']",WritingSystemDefinition.MakeNameSpaceManager());
		}

		[Test]
		public void CanReadVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "en";
			ws.Variant = "piglatin";
			ws.SaveToRepository(_repository);

			//here, the task is not to overwrite what was in ther already
			WritingSystemDefinition ws2 = new WritingSystemDefinition(_repository, "en-piglatin");
			Assert.AreEqual("piglatin", ws2.Variant);
		 }

		 [Test]
		 public void CanRemoveVariant()
		 {
			 WritingSystemDefinition ws = new WritingSystemDefinition();
			 ws.ISO = "en";
			 ws.Variant = "piglatin";
			 ws.SaveToRepository(_repository);
			 string path = Path.Combine(_repository.PathToWritingSystems, ws.FileName);
			 TestUtilities.AssertXPathNotNull(path, "ldml/identity/variant");
			 ws.Variant = string.Empty;
			 ws.SaveToRepository(_repository);
				path = Path.Combine(_repository.PathToWritingSystems, ws.FileName);
			 TestUtilities.AssertXPathIsNull(path, "ldml/identity/variant");
		 }


		[Test]
		public void CanRemoveAbbreviation()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "en";
			ws.Abbreviation = "abbrev";
			ws.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, ws.FileName);
			TestUtilities.AssertXPathNotNull(path, "ldml/special/palaso:abbreviation", WritingSystemDefinition.MakeNameSpaceManager());
			ws.Abbreviation = string.Empty;
			ws.SaveToRepository(_repository);
			path = Path.Combine(_repository.PathToWritingSystems, ws.FileName);
			TestUtilities.AssertXPathIsNull(path, "ldml/special/palaso:abbreviation", WritingSystemDefinition.MakeNameSpaceManager());
		}

		[Test]
		public void WritesAbbreviationToLDML()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "blah";
			ws.Abbreviation = "bl";
			ws.SaveToRepository(_repository);
			string path = Path.Combine(_repository.PathToWritingSystems, ws.FileName);
			TestUtilities.AssertXPathNotNull(path, "ldml/special/palaso:abbreviation[@value='bl']", WritingSystemDefinition.MakeNameSpaceManager());
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