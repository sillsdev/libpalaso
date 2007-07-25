using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Palaso;

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
	}

}