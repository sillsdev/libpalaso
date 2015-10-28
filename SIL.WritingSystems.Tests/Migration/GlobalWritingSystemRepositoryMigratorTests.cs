using System;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.WritingSystems.Migration;

namespace SIL.WritingSystems.Tests.Migration
{
	[TestFixture]
	public class GlobalWritingSystemRepositoryMigratorTests
	{

		class TestEnvironment : IDisposable
		{
			private readonly string[] _ldmlFileNames;

			private readonly TemporaryFolder _baseFolder = new TemporaryFolder("GlobalWritingSystemRepositoryMigratorTests");

			public TestEnvironment()
			{
				_ldmlFileNames = Directory.GetFiles(GlobalWritingSystemRepositoryMigrator.LdmlPathPre0);
				NamespaceManager = new XmlNamespaceManager(new NameTable());
				NamespaceManager.AddNamespace("sil", "urn://www.sil.org/ldml/0.1");
				NamespaceManager.AddNamespace("palaso", "urn://palaso.org/ldmlExtensions/v1");
			}

			private XmlNamespaceManager NamespaceManager { get; set; }

			public string BasePath { get { return _baseFolder.Path; } }

			public static void WriteLdmlFile(string language, string bogus, int version)
			{
				string filePath = GlobalWritingSystemRepositoryMigrator.LdmlPathPre0;
				if (!Directory.Exists(filePath))
					Directory.CreateDirectory(filePath);
				filePath = Path.Combine(filePath, String.Format("{0}.ldml", language));
				string content = string.Empty;
				if (version == 0)
					content = LdmlContentForTests.Version0Bogus(language, "", "", "", bogus);
				if (version == LdmlDataMapper.CurrentLdmlVersion)
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
						LdmlDataMapper.CurrentLdmlVersion.ToString()
					);
					return filePath;
				}
			}

			public int GetFileVersion(string fileName)
			{
				var versionReader = new WritingSystemLdmlVersionGetter();
				string filePath = Path.Combine(MigratedLdmlFolder, fileName);
				return versionReader.GetFileVersion(filePath);
			}

			private static void DeleteFilesInFolderExcept(string path, string[] fileNames)
			{
				foreach (string fileName in Directory.GetFiles(path))
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
				DeleteFilesInFolderExcept(GlobalWritingSystemRepositoryMigrator.LdmlPathPre0, _ldmlFileNames);
				_baseFolder.Dispose();
			}
		}

		[Test]
		public void Migrate_WithOldLdmlFile_Migrates()
		{
			using (var e = new TestEnvironment())
			{
				TestEnvironment.WriteLdmlFile("qaa-x-bogus", "bogus", 0);
				var m = new GlobalWritingSystemRepositoryMigrator(e.BasePath);
				m.Migrate();

				Assert.AreEqual(LdmlDataMapper.CurrentLdmlVersion, e.GetFileVersion("qaa-x-bogus.ldml"));
			}
		}

		[Test]
		public void Migrate_WithCurrentVersionLdmlFile_FileCopiedToRepo()
		{
			using (var e = new TestEnvironment())
			{
				TestEnvironment.WriteLdmlFile("qaa-x-bogus", "bogus", LdmlDataMapper.CurrentLdmlVersion);
				var m = new GlobalWritingSystemRepositoryMigrator(e.BasePath);
				m.Migrate();

				Assert.AreEqual(LdmlDataMapper.CurrentLdmlVersion, e.GetFileVersion("qaa-x-bogus.ldml"));
			}
		}
	}
}
