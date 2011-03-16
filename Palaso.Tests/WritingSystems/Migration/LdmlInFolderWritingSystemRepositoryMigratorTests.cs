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
		private class Environment:IDisposable
		{
			private TemporaryFolder _folderContainingLdml;

			public Environment()
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
			using (var environment = new Environment())
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
			using (var environment = new Environment())
			{
				environment.CreateLdmlFileWithContent("en-Zxxx-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("en", "Zxxx", "", "x-audio"));
				environment.CreateLdmlFileWithContent("en.ldml", LdmlFileContentForTests.Version1LdmlFile);
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.FolderContainingLdml.Path);
				Assert.IsTrue(migrator.NeedsMigration());
			}
		}

		[Test]
		public void Migrate_LdmlContainsWritingSystemThatIsLatestVersionButFileNameIsNotidenticalToRfcTag_WhatToDo_Throw()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Migrate_WritingSystemRepositoryContainsWsThatWouldBeMigratedToDuplicateOfExistingWs_DuplicateWsAreDisambiguated()
		{
			using (var environment = new Environment())
			{
				environment.CreateLdmlFileWithContent("en-Zxxx-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("en", "Zxxx", "", "x-audio"));
				environment.CreateLdmlFileWithContent("en-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("en", "", "", "x-audio"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.FolderContainingLdml.Path);
				migrator.Migrate();
				Assert.True(File.Exists("en-Zxxx-x-audio.ldml"));
				Assert.True(File.Exists("en-Zxxx-x-audio-dupl1.ldml"));
				AssertThatXmlIn.File("en-Zxxx-x-audio-dupl1.ldml").HasAtLeastOneMatchForXpath("/ldml/identity/script[@type=en");
				AssertThatXmlIn.File("en-Zxxx-x-audio-dupl1.ldml").HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type=x-audio-dupl1");
			}
		}

		[Test]
		public void Migrate_WritingSystemRepositoryContainsWsThatWouldBeMigratedToCaseInsensitiveDuplicateOfExistingWs_DuplicateWsAreDisambiguated()
		{
			using (var environment = new Environment())
			{
				environment.CreateLdmlFileWithContent("en-Zxxx-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("en", "Zxxx", "", "x-audio"));
				environment.CreateLdmlFileWithContent("en-x-audio.ldml", LdmlFileContentForTests.CreateVersion0LdmlContent("eN", "", "", "x-AuDio"));
				var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(environment.FolderContainingLdml.Path);
				migrator.Migrate();
				Assert.True(File.Exists("en-Zxxx-x-audio.ldml"));
				Assert.True(File.Exists("eN-Zxxx-x-AuDio-dupl1.ldml"));
				AssertThatXmlIn.File("en-Zxxx-x-audio-dupl1.ldml").HasAtLeastOneMatchForXpath("/ldml/identity/script[@type=eN");
				AssertThatXmlIn.File("eN-Zxxx-x-AuDio-dupl1.ldml").HasAtLeastOneMatchForXpath("/ldml/identity/variant[@type=x-AuDio-dupl1");
			}
		}
	}
}
