using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;
using SIL.WritingSystems.Migration;

namespace SIL.WritingSystems.Tests.Migration
{
	[TestFixture]
	public class GlobalWritingSystemRepositoryMigratorTests
	{

		class TestEnvironment : IDisposable
		{
			private readonly string[] _palasoFileNames;
			private readonly string[] _flexFileNames;

			private readonly TemporaryFolder _baseFolder = new TemporaryFolder("GlobalWritingSystemRepositoryMigratorTests");

			public TestEnvironment()
			{
				_palasoFileNames = Directory.GetFiles(GlobalWritingSystemRepositoryMigrator.PalasoLdmlPathPre0);
				_flexFileNames = Directory.GetFiles(GlobalWritingSystemRepositoryMigrator.FlexLdmlPathPre0);
				NamespaceManager = new XmlNamespaceManager(new NameTable());
				NamespaceManager.AddNamespace("sil", "urn://www.sil.org/ldml/0.1");
				NamespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			}

			public XmlNamespaceManager NamespaceManager { get; private set; }

			public string BasePath { get { return _baseFolder.Path; } }

			public void OnMigrateCallback(int toVersion, IEnumerable<LdmlMigrationInfo> migrationInfo)
			{
				MigrationInfo = new List<LdmlMigrationInfo>(migrationInfo);
			}

			public IEnumerable<LdmlMigrationInfo> MigrationInfo { get; private set; }

			public static void WriteFlexFile(string language, string bogus, int version)
			{
				string filePath = GlobalWritingSystemRepositoryMigrator.FlexLdmlPathPre0;
				filePath = Path.Combine(filePath, String.Format("{0}.ldml", language));
				string content = string.Empty;
				if (version == 0)
					content = LdmlContentForTests.Version0Bogus(language, "", "", "", bogus);
				else if (version == WritingSystemDefinition.LatestWritingSystemDefinitionVersion)
					// Make up a custom element
					content = LdmlContentForTests.CurrentVersion(language, "", "", "", string.Format("\n\t<{0} />",bogus));
				File.WriteAllText(filePath, content);
			}

			public static void WritePalasoFile(string language, string bogus, int version)
			{
				string filePath = GlobalWritingSystemRepositoryMigrator.PalasoLdmlPathPre0;
				filePath = Path.Combine(filePath, String.Format("{0}.ldml", language));
				string content = string.Empty;
				if (version == 0)
					content = LdmlContentForTests.Version0Bogus(language, "", "", "", bogus);
				if (version == WritingSystemDefinition.LatestWritingSystemDefinitionVersion)
					content = LdmlContentForTests.CurrentVersion(language, "", "", "", string.Format("\n\t<{0} />",bogus));
				File.WriteAllText(filePath, content);
			}

			private string MigratedLdmlFolder
			{
				get
				{
					string filePath = _baseFolder.Path;
					filePath = Path.Combine(
						filePath,
						WritingSystemDefinition.LatestWritingSystemDefinitionVersion.ToString()
					);
					return filePath;
				}
			}

			public string MigratedLdml(string ldmlFileName)
			{
				string filePath = Path.Combine(MigratedLdmlFolder, ldmlFileName);
				return File.ReadAllText(filePath);
			}

			public int GetFileVersion(string fileName)
			{
				var versionReader = new WritingSystemLdmlVersionGetter();
				string filePath = Path.Combine(MigratedLdmlFolder, fileName);
				return versionReader.GetFileVersion(filePath);
			}

			private static void DeleteFilesInFolderExcept(string path, IEnumerable<string> fileNames)
			{
				foreach (var fileName in Directory.GetFiles(path))
				{
					if (!fileNames.Contains(fileName))
					{
						string filePath = Path.Combine(path, fileName);
						File.Delete(filePath);
					}
				}
			}

			public void Dispose()
			{
				// Delete all files not already present
				DeleteFilesInFolderExcept(GlobalWritingSystemRepositoryMigrator.PalasoLdmlPathPre0, _palasoFileNames);
				DeleteFilesInFolderExcept(GlobalWritingSystemRepositoryMigrator.FlexLdmlPathPre0, _flexFileNames);
				_baseFolder.Dispose();
			}
		}

		[Test]
		public void Migrate_WithOldPalsoFile_Migrates()
		{
			using (var e = new TestEnvironment())
			{
				TestEnvironment.WritePalasoFile("qaa-x-bogus", "bogus", 0);
				var m = new GlobalWritingSystemRepositoryMigrator(e.BasePath, e.OnMigrateCallback);
				m.Migrate();

				Assert.AreEqual(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, e.GetFileVersion("qaa-x-bogus.ldml"));
			}
		}

		[Test]
		public void Migrate_WithOldFlexFile_Migrates()
		{
			using (var e = new TestEnvironment())
			{
				TestEnvironment.WriteFlexFile("qaa-x-bogus", "bogus", 0);
				var m = new GlobalWritingSystemRepositoryMigrator(e.BasePath, e.OnMigrateCallback);
				m.Migrate();

				Assert.AreEqual(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, e.GetFileVersion("qaa-x-bogus.ldml"));
			}
		}

		[Test]
		public void Migrate_WithCurrentFlexFile_FileCopiedToRepo()
		{
			using (var e = new TestEnvironment())
			{
				TestEnvironment.WriteFlexFile("qaa-x-bogus", "bogus", WritingSystemDefinition.LatestWritingSystemDefinitionVersion);
				var m = new GlobalWritingSystemRepositoryMigrator(e.BasePath, e.OnMigrateCallback);
				m.Migrate();

				Assert.AreEqual(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, e.GetFileVersion("qaa-x-bogus.ldml"));
			}
		}

		[Test]
		public void Migrate_PalsoAndFlexHaveSameFileName_PrefersFlex()
		{
			using (var e = new TestEnvironment())
			{
				TestEnvironment.WritePalasoFile("qaa-x-bogus", "bogusPalaso", 0);
				TestEnvironment.WriteFlexFile("qaa-x-bogus", "bogusFlex", 0);
				var m = new GlobalWritingSystemRepositoryMigrator(e.BasePath, e.OnMigrateCallback);
				m.Migrate();

				Assert.AreEqual(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, e.GetFileVersion("qaa-x-bogus.ldml"));
				AssertThatXmlIn.String(e.MigratedLdml("qaa-x-bogus.ldml")).HasAtLeastOneMatchForXpath(
					"ldml[not(bogusPalaso)]", e.NamespaceManager);
				AssertThatXmlIn.String(e.MigratedLdml("qaa-x-bogus.ldml")).HasAtLeastOneMatchForXpath(
					"ldml/bogusFlex", e.NamespaceManager);
			}
		}
	}
}
