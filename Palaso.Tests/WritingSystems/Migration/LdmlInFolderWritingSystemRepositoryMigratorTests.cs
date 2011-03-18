using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Tests.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using Palaso.TestUtilities;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration;

namespace Palaso.Tests.WritingSystems.Migration
{
	[TestFixture]
	public class LdmlInFolderWritingSystemRepositoryMigratorTests
	{
		private class TestEnvironment : IDisposable
		{
			private TemporaryFolder _folderContainingLdml;

			public TestEnvironment()
			{
				FolderContainingLdml = new TemporaryFolder("WsCollectionForTesting");
			}

			public TemporaryFolder FolderContainingLdml
			{
				get { return _folderContainingLdml; }
				set { _folderContainingLdml = value; }
			}

			public void CreateLdmlFileWithContent(string fileName, string contentToWrite)
			{
				TempFile pathToWs = FolderContainingLdml.GetNewTempFile(true);
				File.WriteAllText(pathToWs.Path, contentToWrite);
				pathToWs.MoveTo(Path.Combine(FolderContainingLdml.Path, fileName));
			}

			public void Dispose()
			{
				FolderContainingLdml.Delete();
			}
		}

		[Test]
		public void Migrate_LdmlContainsWritingSystemThatIsVersion0_MigratedToLatest()
		{
			using (var environment = new TestEnvironment())
			{
				environment.CreateLdmlFileWithContent("en-Zxxx-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("en", "Zxxx", "", "x-audio"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.FolderContainingLdml.Path);
				migrator.Migrate();
				Assert.AreEqual(1, migrator.GetFileVersion());
			}
		}

		[Test]
		public void Migrate_LdmlContainsWritingSystemThatIsVersion0_NeedsMigratingIsTrue()
		{
			using (var environment = new TestEnvironment())
			{
				environment.CreateLdmlFileWithContent("en-Zxxx-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("en", "Zxxx", "", "x-audio"));
				environment.CreateLdmlFileWithContent("en.ldml", LdmlFileContentForTests.Version1LdmlFile);
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.FolderContainingLdml.Path);
				Assert.IsTrue(migrator.NeedsMigration());
			}
		}

		[Test]
		public void Migrate_WritingSystemRepositoryContainsWsThatWouldBeMigratedToDuplicateOfExistingWs_DuplicateWsAreDisambiguated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.CreateLdmlFileWithContent("en-Zxxx-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("en", "Zxxx", "", "x-audio"));
				environment.CreateLdmlFileWithContent("en-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "x-audio"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.FolderContainingLdml.Path);
				migrator.Migrate();
				string pathToFile1 = Path.Combine(environment.FolderContainingLdml.Path, "en-Zxxx-x-audio.ldml");
				string pathToFileDuplicate = Path.Combine(environment.FolderContainingLdml.Path, "en-Zxxx-x-audio-dupl1.ldml");
				Assert.True(File.Exists(pathToFile1));
				Assert.True(File.Exists(pathToFileDuplicate));
				AssertThatXmlIn.File(pathToFile1).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(pathToFile1).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(pathToFile1).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio']");
				AssertThatXmlIn.File(pathToFileDuplicate).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(pathToFileDuplicate).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(pathToFileDuplicate).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio-dupl1']");
			}
		}

		[Test]
		public void Migrate_WritingSystemRepositoryContainsWsThatWouldBeMigratedToCaseInsensitiveDuplicateOfExistingWs_DuplicateWsAreDisambiguated()
		{
			using (var environment = new TestEnvironment())
			{
				environment.CreateLdmlFileWithContent("en-Zxxx-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("en", "Zxxx", "", "x-audio"));
				environment.CreateLdmlFileWithContent("en-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("eN", "", "", "x-AuDio"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.FolderContainingLdml.Path);
				migrator.Migrate();
				string pathToFile1 = Path.Combine(environment.FolderContainingLdml.Path, "en-Zxxx-x-audio.ldml");
				string pathToFileDuplicate = Path.Combine(environment.FolderContainingLdml.Path, "eN-Zxxx-x-AuDio-dupl1.ldml");
				Assert.True(File.Exists(pathToFile1));
				Assert.True(File.Exists(pathToFileDuplicate));
				AssertThatXmlIn.File(pathToFile1).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(pathToFile1).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(pathToFile1).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio']");
				AssertThatXmlIn.File(pathToFileDuplicate).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='eN']");
				AssertThatXmlIn.File(pathToFileDuplicate).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(pathToFileDuplicate).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-AuDio-dupl1']");
			}
		}

		[Test]
		public void Migrate_RepoContainsWsThatWouldBeMigratedToCaseInsensitiveDuplicateOfExistingWswithBogusFileName_WritingSystemWithValidRfcTagBeforeMigrationIsNotTheDuplicate()
		{
			using (var environment = new TestEnvironment())
			{
				environment.CreateLdmlFileWithContent("bogus.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("eN", "ZxXx", "", "x-AuDio"));
				environment.CreateLdmlFileWithContent("en-Zxxx-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "x-audio"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.FolderContainingLdml.Path);
				migrator.Migrate();
				string pathToFile1 = Path.Combine(environment.FolderContainingLdml.Path, "en-Zxxx-x-audio.ldml");
				string pathToFileDuplicate = Path.Combine(environment.FolderContainingLdml.Path, "eN-Zxxx-x-AuDio-dupl1.ldml");
				Assert.True(File.Exists(pathToFile1));
				Assert.True(File.Exists(pathToFileDuplicate));
				AssertThatXmlIn.File(pathToFile1).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='eN']");
				AssertThatXmlIn.File(pathToFile1).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='ZxXx']");
				AssertThatXmlIn.File(pathToFile1).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-AuDio']");
				AssertThatXmlIn.File(pathToFileDuplicate).HasAtLeastOneMatchForXpath("/ldml/identity/language[@type='en']");
				AssertThatXmlIn.File(pathToFileDuplicate).HasAtLeastOneMatchForXpath("/ldml/identity/script[@type='Zxxx']");
				AssertThatXmlIn.File(pathToFileDuplicate).HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type='x-audio-dupl1']");
			}
		}
	}
}
