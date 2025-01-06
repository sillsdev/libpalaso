using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;
using Is = SIL.TestUtilities.NUnitExtensions.Is;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class WritingSystemOrphanFinderTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly TemporaryFolder _folder = new TemporaryFolder("WritingSystemOrphanFinderTests");
			private readonly TempFile _file;

			public TestEnvironment(string id1, string id2)
			{
				WritingSystemRepository = new TestLdmlInFolderWritingSystemRepository(WritingSystemsPath);
				_file = _folder.GetNewTempFile(true);
				File.WriteAllText(_file.Path, $"|{id1}||{id1}||{id2}|");
			}

			private string WritingSystemsPath => _folder.Combine("WritingSystems");

			public LdmlInFolderWritingSystemRepository WritingSystemRepository { get; }

			public void Dispose()
			{
				_file.Dispose();
				_folder.Dispose();
			}

			public IEnumerable<string> GetIdsFromFile
			{
				get
				{
					var fileContent = File.ReadAllText(_file.Path);
					foreach(var id in fileContent.Split(new []{'|'},StringSplitOptions.RemoveEmptyEntries).Distinct())
					{
						yield return id;
					}
				}
			}

			public void ReplaceIdInFile(string oldid, string newid)
			{
				var fileContent = File.ReadAllText(_file.Path);
				fileContent = fileContent.Replace("|" + oldid + "|", "|" + newid + "|");
				File.WriteAllText(_file.Path, fileContent);
			}

			public string FileContent => File.ReadAllText(_file.Path);
		}

		[Test]
		public void FindOrphans_NoOrphansFound_WritingSystemRepoAndFileUntouched()
		{
			using (var e = new TestEnvironment("en", "en"))
			{
				var englishWs = new WritingSystemDefinition("en");
				e.WritingSystemRepository.Set(englishWs);
				WritingSystemOrphanFinder.FindOrphans(e.GetIdsFromFile, e.ReplaceIdInFile, e.WritingSystemRepository);
				Assert.That(e.WritingSystemRepository.Count, Is.EqualTo(1));
				Assert.That(e.WritingSystemRepository.Get("en"), Is.EqualTo(englishWs));
				Assert.That(e.FileContent, Is.EqualTo("|en||en||en|"));
			}
		}

		[Test]
		public void FindOrphans_OrphanFoundIsValidRfcTag_WritingSystemIsAddedToWritingSystemRepoAndFileUntouched()
		{
			using (var e = new TestEnvironment("en", "de"))
			{
				var englishWs = new WritingSystemDefinition("en");
				e.WritingSystemRepository.Set(englishWs);
				WritingSystemOrphanFinder.FindOrphans(e.GetIdsFromFile, e.ReplaceIdInFile, e.WritingSystemRepository);
				Assert.That(e.WritingSystemRepository.Count, Is.EqualTo(2));
				Assert.That(e.WritingSystemRepository.Get("en"), Is.EqualTo(englishWs));
				Assert.That(e.WritingSystemRepository.Get("de"), Is.Not.Null);
				Assert.That(e.FileContent, Is.EqualTo("|en||en||de|"));
			}
		}

		[Test]
		public void FindOrphans_OrphanFoundIsNotValidRfcTag_OrphanIsMadeConformAndIsAddedWritingSystemRepoAndWritingSystemIsChangedInFile()
		{
			using (var e = new TestEnvironment("en", "bogusws"))
			{
				var englishWs = new WritingSystemDefinition("en");
				e.WritingSystemRepository.Set(englishWs);
				WritingSystemOrphanFinder.FindOrphans(e.GetIdsFromFile, e.ReplaceIdInFile, e.WritingSystemRepository);
				Assert.That(e.WritingSystemRepository.Count, Is.EqualTo(2));
				Assert.That(e.WritingSystemRepository.Get("en"), Is.EqualTo(englishWs));
				Assert.That(e.WritingSystemRepository.Get("qaa-x-bogusws"), Is.Not.Null);
				Assert.That(e.FileContent, Is.EqualTo("|en||en||qaa-x-bogusws|"));
			}
		}

		[Test]
		public void FindOrphans_OrphanFoundIsNotValidRfcTagButWritingSystemRepoKnowsAboutChange_WritingSystemIsChangedInFile()
		{
			using (var e = new TestEnvironment("en", "bogusws"))
			{
				var englishWs = new WritingSystemDefinition("en");
				e.WritingSystemRepository.Set(englishWs);
				e.WritingSystemRepository.Save();
				englishWs.Variants.Add("new");
				e.WritingSystemRepository.Set(englishWs);
				e.WritingSystemRepository.Save();
				WritingSystemOrphanFinder.FindOrphans(e.GetIdsFromFile, e.ReplaceIdInFile, e.WritingSystemRepository);
				Assert.That(e.WritingSystemRepository.Count, Is.EqualTo(2));
				Assert.That(e.WritingSystemRepository.Get("en-x-new"), Is.EqualTo(englishWs));
				Assert.That(e.WritingSystemRepository.Get("qaa-x-bogusws"), Is.Not.Null);
				Assert.That(e.FileContent, Is.EqualTo("|en-x-new||en-x-new||qaa-x-bogusws|"));
			}
		}

		[Test]
		public void FindOrphans_StaticListOfIds_DuplicateIsCreated()
		{
			using (var e = new TestEnvironment("boglang-Zxxx-bogusws", "boglang-bogusws-Zxxx"))
			{
				var wss = new List<string>(e.GetIdsFromFile);
				WritingSystemOrphanFinder.FindOrphans(wss, e.ReplaceIdInFile, e.WritingSystemRepository);
				Assert.That(e.WritingSystemRepository.Count, Is.EqualTo(2));
				Assert.That(e.WritingSystemRepository.Get("qaa-Zxxx-x-boglang-bogusws"), Is.Not.Null);
				Assert.That(e.WritingSystemRepository.Get("qaa-Zxxx-x-boglang-bogusws-dupl0"), Is.Not.Null);
				Assert.That(e.FileContent, Is.EqualTo("|qaa-Zxxx-x-boglang-bogusws||qaa-Zxxx-x-boglang-bogusws||qaa-Zxxx-x-boglang-bogusws-dupl0|"));
			}
		}

	}
}
