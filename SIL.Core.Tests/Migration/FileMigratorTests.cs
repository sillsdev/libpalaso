using System;
using System.IO;
using NUnit.Framework;
using SIL.Migration;
using SIL.TestUtilities;

namespace SIL.Tests.Migration
{
	[TestFixture]
	public class FileMigratorTests
	{
		private class EnvironmentForTest : TemporaryFolder
		{
			public EnvironmentForTest() : base("FileMigratorTests")
			{
			}

			public static string Xsl1To2
			{
				get
				{
					return
						@"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
  <!-- don't do anything to other versions -->
  <xsl:template match=""configuration[@version='1']"">
	<configuration version=""2"">
	  <xsl:apply-templates mode =""identity""/>
	</configuration>
  </xsl:template>

  <xsl:template match=""@*|node()"" mode=""identity"">
	<xsl:copy>
	  <xsl:apply-templates select=""@*|node()""  mode=""identity""/>
	</xsl:copy>
  </xsl:template>
</xsl:stylesheet>
";
				}
			}

			public static string Xsl2To3
			{
				get
				{
					return @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
  <!-- don't do anything to other versions -->
  <xsl:template match=""configuration[@version='2']"">
	<configuration version=""3"">
	  <xsl:apply-templates mode =""identity""/>
	</configuration>
  </xsl:template>

  <xsl:template match=""@*|node()"" mode=""identity"">
	<xsl:copy>
	  <xsl:apply-templates select=""@*|node()""  mode=""identity""/>
	</xsl:copy>
  </xsl:template>
</xsl:stylesheet>
";

				}
			}

			public static string XmlVersion1
			{
				get
				{
					return @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration version='1'>
  <blah />
</configuration>
".Replace("'", "\"");
				}
			}

			public static string XmlNoVersion
			{
				get
				{
					return @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration>
  <blah />
</configuration>
".Replace("'", "\"");
				}
			}

			public void WriteTestFile(string content)
			{
				TestFilePath = GetPathForNewTempFile(false);
				File.WriteAllText(TestFilePath, content);
			}

			public string TestFilePath { get; private set; }

		}

		private class VersionStrategyThatsGood : IFileVersion
		{
			private readonly int _actualVersion;
			private int _goodToVersion;

			public VersionStrategyThatsGood(int actualVersion, int goodToVersion)
			{
				_actualVersion = actualVersion;
				_goodToVersion = goodToVersion;
			}
			public int GetFileVersion(string filePath)
			{
				return _actualVersion;
			}

			public int StrategyGoodToVersion
			{
				get { return _goodToVersion; }
			}
		}

		private class VersionStrategyThatThrows : VersionStrategyThatsGood
		{
			public VersionStrategyThatThrows(int goodToVersion) : base(0, goodToVersion)
			{
			}

			public new int GetFileVersion(string source)
			{
				throw new ApplicationException("GetFileVersion shouldn't be called");
			}
		}

		private class XslStringMigrator : XslMigrationStrategy
		{
			private string _xsl;

			public XslStringMigrator(int fromVersion, int toVersion, string xsl) :
				base(fromVersion, toVersion, null)
			{
				_xsl = xsl;
			}

			protected override TextReader OpenXslStream(string xslSource)
			{
				return new StringReader(_xsl);
			}
		}


		[Test]
		public void FileVersion_OneStrategy_Correct()
		{
			var migrator = new FileMigrator(10, "somefile");
			migrator.AddVersionStrategy(new VersionStrategyThatsGood(8, 10));
			Assert.That(migrator.GetFileVersion(), Is.EqualTo(8));
		}

		[Test]
		public void FileVersion_TwoStragies_UsesHigherStrategyFirst()
		{
			var migrator = new FileMigrator(10, "somefile");
			migrator.AddVersionStrategy(new VersionStrategyThatsGood(8, 10));
			migrator.AddVersionStrategy(new VersionStrategyThatThrows(2));
			Assert.That(migrator.GetFileVersion(), Is.EqualTo(8));
		}

		[Test]
		public void FileVersion_TwoStragiesSort_UsesHigherStrategyFirst()
		{
			var migrator = new FileMigrator(10, "somefile");
			migrator.AddVersionStrategy(new VersionStrategyThatThrows(2));
			migrator.AddVersionStrategy(new VersionStrategyThatsGood(8, 10));
			Assert.That(migrator.GetFileVersion(), Is.EqualTo(8));
		}

		[Test]
		public void NeedsMigration_WithDifferentFileVersion_True()
		{
			var migrator = new FileMigrator(10, "somefile");
			migrator.AddVersionStrategy(new VersionStrategyThatsGood(8, 10));
			Assert.That(migrator.NeedsMigration(), Is.True);
		}

		[Test]
		public void NeedsMigration_WithSameVersion_False()
		{
			var migrator = new FileMigrator(10, "somefile");
			migrator.AddVersionStrategy(new VersionStrategyThatsGood(10, 10));
			Assert.That(migrator.NeedsMigration(), Is.False);
		}

		[Test]
		public void Migrate_WithBackupFileInTheWay_DoesntThrow()
		{
			using (var e = new EnvironmentForTest())
			{
				e.WriteTestFile(EnvironmentForTest.XmlVersion1);
				var migrator = new FileMigrator(3, e.TestFilePath);
				File.Copy(migrator.SourceFilePath, migrator.BackupFilePath); // Place the backup file in the way.
				migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
				migrator.AddMigrationStrategy(new XslStringMigrator(1, 2, EnvironmentForTest.Xsl1To2));
				migrator.AddMigrationStrategy(new XslStringMigrator(2, 3, EnvironmentForTest.Xsl2To3));

				migrator.Migrate();
			}
		}

		[Test]
		public void Migrate_UsingXslAndXml_ArrivesAtVersion3()
		{
			using (var e = new EnvironmentForTest())
			{
				e.WriteTestFile(EnvironmentForTest.XmlVersion1);
				var migrator = new FileMigrator(3, e.TestFilePath);
				migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
				migrator.AddMigrationStrategy(new XslStringMigrator(1, 2, EnvironmentForTest.Xsl1To2));
				migrator.AddMigrationStrategy(new XslStringMigrator(2, 3, EnvironmentForTest.Xsl2To3));

				migrator.Migrate();

				AssertThatXmlIn.File(e.TestFilePath).HasAtLeastOneMatchForXpath("configuration[@version='3']");
				AssertThatXmlIn.File(e.TestFilePath).HasAtLeastOneMatchForXpath("/configuration/blah");
			}
		}

		[Test]
		public void Migrate_MissingMigrationStrategy_ThrowsAndSourceFileSameVersion()
		{
			using (var e = new EnvironmentForTest())
			{
				e.WriteTestFile(EnvironmentForTest.XmlVersion1);
				var migrator = new FileMigrator(3, e.TestFilePath);
				migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
				migrator.AddMigrationStrategy(new XslStringMigrator(1, 2, EnvironmentForTest.Xsl1To2));

				Assert.Throws<InvalidOperationException>(migrator.Migrate);

				AssertThatXmlIn.File(e.TestFilePath).HasAtLeastOneMatchForXpath("configuration[@version='1']");
				AssertThatXmlIn.File(e.TestFilePath).HasAtLeastOneMatchForXpath("/configuration/blah");
			}
		}

		[Test]
		public void GetFileVersion_DataHasVersion_CorrectVersion()
		{
			using (var e = new EnvironmentForTest())
			{
				e.WriteTestFile(EnvironmentForTest.XmlVersion1);

				var migrator = new FileMigrator(7, e.TestFilePath);
				migrator.AddVersionStrategy(new XPathVersion(1, "/configuration/@version"));

				Assert.AreEqual(1, migrator.GetFileVersion(e.TestFilePath));
			}
		}

		[Test]
		public void GetFileVersion_FileHasNoVersionAndNoDefaultStrategy_Throws()
		{
			using (var e = new EnvironmentForTest())
			{
				e.WriteTestFile(EnvironmentForTest.XmlNoVersion);

				var migrator = new FileMigrator(1, e.TestFilePath);
				migrator.AddVersionStrategy(new XPathVersion(1, "/configuration/@version"));

				Assert.Throws<ApplicationException>(
					() => migrator.GetFileVersion(e.TestFilePath)
				);
			}
		}

		[Test]
		public void GetFileVersion_FileHasNoVersionUsingDefaultStrategy_ReturnsZero()
		{
			using (var e = new EnvironmentForTest())
			{
				e.WriteTestFile(EnvironmentForTest.XmlNoVersion);

				var migrator = new FileMigrator(1, e.TestFilePath);
				migrator.AddVersionStrategy(new XPathVersion(1, "/configuration/@version"));
				migrator.AddVersionStrategy(new DefaultVersion(0, 0));

				Assert.AreEqual(0, migrator.GetFileVersion(e.TestFilePath));
			}
		}

		[Test]
		public void Migrate_MissingMigrationStrategy_LeavesWipFiles()
		{
			using (var e = new EnvironmentForTest())
			{
				e.WriteTestFile(EnvironmentForTest.XmlVersion1);
				var migrator = new FileMigrator(3, e.TestFilePath);
				migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
				migrator.AddMigrationStrategy(new XslStringMigrator(1, 2, EnvironmentForTest.Xsl1To2));

				Assert.Throws<InvalidOperationException>(() => migrator.Migrate());

				var files = Directory.GetFiles(e.Path);
				Assert.That(files, Has.Some.SamePath(e.TestFilePath));
				Assert.That(files, Has.Some.SamePath(e.TestFilePath + ".bak"));
				Assert.That(files, Has.Some.SamePath(e.TestFilePath + ".Migrate_1_2"));
			}
		}

		[Test]
		// No wip files left behind
		public void Migrate_Succeeds_CleansUpWipFilesAndBackupFiles()
		{
			using (var e = new EnvironmentForTest())
			{
				e.WriteTestFile(EnvironmentForTest.XmlVersion1);
				var migrator = new FileMigrator(3, e.TestFilePath);
				migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
				migrator.AddMigrationStrategy(new XslStringMigrator(1, 2, EnvironmentForTest.Xsl1To2));
				migrator.AddMigrationStrategy(new XslStringMigrator(2, 3, EnvironmentForTest.Xsl2To3));

				migrator.Migrate();

				var files = Directory.GetFiles(e.Path);

				Assert.That(files.Length, Is.EqualTo(1));
				Assert.That(files, Has.Some.SamePath(e.TestFilePath));
			}
		}

	}
}
