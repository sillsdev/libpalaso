using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;
using SIL.WritingSystems.Migration;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

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

			public static readonly string LdmlV0 = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='' />
		<generation date='0001-01-01T00:00:00' />
		<language type='{0}' />
	</identity>
	<collations />
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation value='{1}' />
		<palaso:defaultFontFamily value='Arial' />
		<palaso:defaultFontSize value='12' />
	</special>
</ldml>".Replace('\'', '"');

			public static readonly string LdmlV1 = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='' />
		<generation date='0001-01-01T00:00:00' />
		<language type='{0}' />
	</identity>
	<collations />
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation value='{1}' />
		<palaso:defaultFontFamily value='Arial' />
		<palaso:defaultFontSize value='12' />
		<palaso:version value='1' />
	</special>
</ldml>".Replace('\'', '"');

// TODO: Where to put abbreviation {1} ?
			public static readonly string LdmlV3 = @"<?xml version='1.0' encoding='utf-8'?>
<ldml xmlns:sil='urn://www.sil.org/ldml/0.1'>
	<identity>
		<version number='$Revision$'/>
		<generation date='$Date$'/>
		<language type='{0}'/>
		<special>
			<sil:identity />
		</special>
	</identity>
</ldml>".Replace('\'', '"');

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

			public void OnMigrateCallback(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo)
			{
				MigrationInfo = new List<LdmlVersion0MigrationStrategy.MigrationInfo>(migrationInfo);
			}

			public IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> MigrationInfo { get; private set; }

			public static void WriteFlexFile(string language, string abbreviation, string template)
			{
				string filePath = GlobalWritingSystemRepositoryMigrator.FlexLdmlPathPre0;
				filePath = Path.Combine(filePath, String.Format("{0}.ldml", language));
				File.WriteAllText(filePath, String.Format(template, language, abbreviation));
			}

			public static void WritePalasoFile(string language, string abbreviation, string template)
			{
				string filePath = GlobalWritingSystemRepositoryMigrator.PalasoLdmlPathPre0;
				filePath = Path.Combine(filePath, String.Format("{0}.ldml", language));
				File.WriteAllText(filePath, String.Format(template, language, abbreviation));
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
				TestEnvironment.WritePalasoFile("qaa-x-bogus", "bogus", TestEnvironment.LdmlV0);
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
				TestEnvironment.WriteFlexFile("qaa-x-bogus", "bogus", TestEnvironment.LdmlV0);
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
				TestEnvironment.WriteFlexFile("qaa-x-bogus", "bogus", TestEnvironment.LdmlV1);
				var m = new GlobalWritingSystemRepositoryMigrator(e.BasePath, e.OnMigrateCallback);
				m.Migrate();

				Assert.AreEqual(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, e.GetFileVersion("qaa-x-bogus.ldml"));
			}
		}

// Does this test go away now?  abbreviation isn't written to ldml anymore
#if WS_FIX
		[Test]
		public void Migrate_PalsoAndFlexHaveSameFileName_PrefersFlex()
		{
			using (var e = new TestEnvironment())
			{
				TestEnvironment.WritePalasoFile("qaa-x-bogus", "bogusPalaso", TestEnvironment.LdmlV0);
				TestEnvironment.WriteFlexFile("qaa-x-bogus", "bogusFlex", TestEnvironment.LdmlV0);
				var m = new GlobalWritingSystemRepositoryMigrator(e.BasePath, e.OnMigrateCallback);
				m.Migrate();

				Assert.AreEqual(WritingSystemDefinition.LatestWritingSystemDefinitionVersion, e.GetFileVersion("qaa-x-bogus.ldml"));
				AssertThatXmlIn.String(e.MigratedLdml("qaa-x-bogus.ldml")).HasAtLeastOneMatchForXpath(
					"ldml/special/palaso:abbreviation[@value='bogusFlex']",
					e.NamespaceManager
				);
			}
		}
#endif

	}
}
