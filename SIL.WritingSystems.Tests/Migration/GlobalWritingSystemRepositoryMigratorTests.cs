using System;
using System.IO;
using NUnit.Framework;
using SIL.PlatformUtilities;
using SIL.TestUtilities;
using SIL.WritingSystems.Migration;

namespace SIL.WritingSystems.Tests.Migration
{
	[TestFixture]
	public class GlobalWritingSystemRepositoryMigratorTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly TemporaryFolder _baseFolder = new TemporaryFolder("GlobalWritingSystemRepositoryMigratorTests");
			private readonly TemporaryFolder _ldmlPathVersion0 = new TemporaryFolder("LdmlPathVersion0");
			private readonly TemporaryFolder _oldLinuxBaseFolder = new TemporaryFolder("GlobalWritingSystemRepositoryMigratorTestsOldLinux");

			public TestEnvironment()
			{
				GlobalWritingSystemRepositoryMigrator.LdmlPathVersion0 = _ldmlPathVersion0.Path;
				GlobalWritingSystemRepositoryMigrator.LdmlPathLinuxVersion2 = _oldLinuxBaseFolder.Path;
			}

			public string BasePath { get { return _baseFolder.Path; } }

			public void WriteVersion0LdmlFile(string language)
			{
				string filePath = Path.Combine(GlobalWritingSystemRepositoryMigrator.LdmlPathVersion0, String.Format("{0}.ldml", language));
				string content = LdmlContentForTests.Version0(language, "", "", "");
				File.WriteAllText(filePath, content);
			}

			public void WriteVersion1LdmlFile(string language)
			{
				string folderPath = Path.Combine(Platform.IsUnix ? GlobalWritingSystemRepositoryMigrator.LdmlPathLinuxVersion2 : _baseFolder.Path, "1");
				Directory.CreateDirectory(folderPath);
				string filePath = Path.Combine(folderPath, String.Format("{0}.ldml", language));
				string content = LdmlContentForTests.Version1(language, "", "", "");
				File.WriteAllText(filePath, content);
			}

			public void WriteVersion2LdmlFile(string language)
			{
				string folderPath = Path.Combine(Platform.IsUnix ? GlobalWritingSystemRepositoryMigrator.LdmlPathLinuxVersion2 : _baseFolder.Path, "2");
				Directory.CreateDirectory(folderPath);
				string filePath = Path.Combine(folderPath, String.Format("{0}.ldml", language));
				string content = LdmlContentForTests.Version2(language, "", "", "");
				File.WriteAllText(filePath, content);
			}

			private string MigratedLdmlFolder
			{
				get
				{
					string filePath = _baseFolder.Path;
					filePath = Path.Combine(
						filePath,
						LdmlDataMapper.CurrentLdmlLibraryVersion.ToString()
					);
					return filePath;
				}
			}

			public int GetMigratedFileVersion(string fileName)
			{
				var versionReader = new WritingSystemLdmlVersionGetter();
				string filePath = Path.Combine(MigratedLdmlFolder, fileName);
				return versionReader.GetFileVersion(filePath);
			}

			public void Dispose()
			{
				_oldLinuxBaseFolder.Dispose();
				_ldmlPathVersion0.Dispose();
				_baseFolder.Dispose();
				GlobalWritingSystemRepositoryMigrator.LdmlPathVersion0 = GlobalWritingSystemRepositoryMigrator.DefaultLdmlPathVersion0;
				GlobalWritingSystemRepositoryMigrator.LdmlPathLinuxVersion2 = GlobalWritingSystemRepositoryMigrator.DefaultLdmlPathLinuxVersion2;
			}
		}

		[Test]
		public void Migrate_WithVersion0LdmlFile_Migrates()
		{
			using (var e = new TestEnvironment())
			{
				e.WriteVersion0LdmlFile("en");
				var m = new GlobalWritingSystemRepositoryMigrator(e.BasePath);
				m.Migrate();

				Assert.That(e.GetMigratedFileVersion("en.ldml"), Is.EqualTo(LdmlDataMapper.CurrentLdmlLibraryVersion));
			}
		}

		[Test]
		public void Migrate_WithVersion1LdmlFile_Migrates()
		{
			using (var e = new TestEnvironment())
			{
				e.WriteVersion1LdmlFile("en");
				var m = new GlobalWritingSystemRepositoryMigrator(e.BasePath);
				m.Migrate();

				Assert.That(e.GetMigratedFileVersion("en.ldml"), Is.EqualTo(LdmlDataMapper.CurrentLdmlLibraryVersion));
			}
		}

		[Test]
		public void Migrate_WithVersion2LdmlFile_Migrates()
		{
			using (var e = new TestEnvironment())
			{
				e.WriteVersion2LdmlFile("en");
				var m = new GlobalWritingSystemRepositoryMigrator(e.BasePath);
				m.Migrate();

				Assert.That(e.GetMigratedFileVersion("en.ldml"), Is.EqualTo(LdmlDataMapper.CurrentLdmlLibraryVersion));
			}
		}
	}
}
