using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Palaso.Migration;
using Palaso.TestUtilities;

namespace Palaso.Tests.Migration
{
	[TestFixture]
	public class MigratorTests
	{
		private class EnvironmentForTest : IDisposable
		{
			public string Xsl1To2
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

			public string Xsl2To3
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

			public string XmlVersion1
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

			public string XmlVersion0
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

			public void Dispose()
			{
			}
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
			public int GetFileVersion(string source)
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
			var migrator = new Migrator(10, "somefile");
			migrator.AddVersionStrategy(new VersionStrategyThatsGood(8, 10));
			Assert.That(migrator.GetFileVersion(), Is.EqualTo(8));
		}

		[Test]
		public void FileVersion_TwoStragies_UsesHigherStrategyFirst()
		{
			var migrator = new Migrator(10, "somefile");
			migrator.AddVersionStrategy(new VersionStrategyThatsGood(8, 10));
			migrator.AddVersionStrategy(new VersionStrategyThatThrows(2));
			Assert.That(migrator.GetFileVersion(), Is.EqualTo(8));
		}

		[Test]
		public void FileVersion_TwoStragiesSort_UsesHigherStrategyFirst()
		{
			var migrator = new Migrator(10, "somefile");
			migrator.AddVersionStrategy(new VersionStrategyThatThrows(2));
			migrator.AddVersionStrategy(new VersionStrategyThatsGood(8, 10));
			Assert.That(migrator.GetFileVersion(), Is.EqualTo(8));
		}

		[Test]
		public void NeedsMigration_WithDifferentFileVersion_True()
		{
			var migrator = new Migrator(10, "somefile");
			migrator.AddVersionStrategy(new VersionStrategyThatsGood(8, 10));
			Assert.That(migrator.NeedsMigration(), Is.True);
		}

		[Test]
		public void NeedsMigration_WithSameVersion_False()
		{
			var migrator = new Migrator(10, "somefile");
			migrator.AddVersionStrategy(new VersionStrategyThatsGood(10, 10));
			Assert.That(migrator.NeedsMigration(), Is.False);
		}

		[Test]
		public void Migrate_WithBackupFileInTheWay_DoesntThrow()
		{
			using (var e = new EnvironmentForTest())
			{
				using (var folder = new TemporaryFolder("MigratorTests"))
				{
					using (var sourceFile = folder.GetNewTempFile(true))
					{
						File.WriteAllText(sourceFile.Path, e.XmlVersion1);
						var migrator = new Migrator(3, sourceFile.Path);
						File.Copy(migrator.SourceFilePath, migrator.BackupFilePath); // Place the backup file in the way.
						migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
						migrator.AddMigrationStrategy(new XslStringMigrator(1, 2, e.Xsl1To2));
						migrator.AddMigrationStrategy(new XslStringMigrator(2, 3, e.Xsl2To3));

						migrator.Migrate();
					}
				}
			}
		}

		[Test]
		public void Migrate_UsingXslAndXml_ArrviesAtVersion3()
		{
			using (var e = new EnvironmentForTest())
			{
				using (var folder = new TemporaryFolder("MigratorTests"))
				{
					using (var sourceFile = folder.GetNewTempFile(true))
					{
						File.WriteAllText(sourceFile.Path, e.XmlVersion1);
						var migrator = new Migrator(3, sourceFile.Path);
						migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
						migrator.AddMigrationStrategy(new XslStringMigrator(1, 2, e.Xsl1To2));
						migrator.AddMigrationStrategy(new XslStringMigrator(2, 3, e.Xsl2To3));

						migrator.Migrate();

						AssertThatXmlIn.File(sourceFile.Path).HasAtLeastOneMatchForXpath("configuration[@version='3']");
						AssertThatXmlIn.File(sourceFile.Path).HasAtLeastOneMatchForXpath("/configuration/blah");
					}
				}
			}
		}

		[Test]
		public void Migrate_MissingMigrationStrategy_ThrowsAndSourceFileSameVersion()
		{
			using (var e = new EnvironmentForTest())
			{
				using (var folder = new TemporaryFolder("MigratorTests"))
				{
					using (var sourceFile = folder.GetNewTempFile(true))
					{
						File.WriteAllText(sourceFile.Path, e.XmlVersion1);
						var migrator = new Migrator(3, sourceFile.Path);
						migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
						migrator.AddMigrationStrategy(new XslStringMigrator(1, 2, e.Xsl1To2));

						Assert.Throws<InvalidOperationException>(() => migrator.Migrate());

						AssertThatXmlIn.File(sourceFile.Path).HasAtLeastOneMatchForXpath("configuration[@version='1']");
						AssertThatXmlIn.File(sourceFile.Path).HasAtLeastOneMatchForXpath("/configuration/blah");
					}
				}
			}
		}

		[Test]
		public void MaximumVersionThatFileCanBeMigratedTo_NoMigratorForConsecutiveMigrationToHighestPossibleVersion_ReturnsVersionOfHighestAchievableVersion()
		{
			using (var e = new EnvironmentForTest())
			{
				int highestAchievableVersion = 3;
				using (var folder = new TemporaryFolder("MigratorTests"))
				{
					using (var sourceFile = folder.GetNewTempFile(true))
					{
						File.WriteAllText(sourceFile.Path, e.XmlVersion1);
						var migrator = new Migrator(7, sourceFile.Path);
						migrator.AddVersionStrategy(new XPathVersion(highestAchievableVersion, "/configuration/@version"));
						migrator.AddMigrationStrategy(new XslStringMigrator(1, highestAchievableVersion, e.Xsl1To2));
						migrator.AddMigrationStrategy(new XslStringMigrator(highestAchievableVersion+2, 7, e.Xsl1To2));

						Assert.AreEqual(highestAchievableVersion, migrator.MaximumVersionThatFileCanBeMigratedTo);
					}
				}
			}
		}

		[Test]
		public void MaximumVersionThatFileCanBeMigratedTo_FileIsOfVersionThatCanNotBeMigrated_ReturnsVersionOfFile()
		{
			using (var e = new EnvironmentForTest())
			{
				using (var folder = new TemporaryFolder("MigratorTests"))
				{
					using (var sourceFile = folder.GetNewTempFile(true))
					{
						File.WriteAllText(sourceFile.Path, e.XmlVersion1);
						var migrator = new Migrator(7, sourceFile.Path);
						migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
						migrator.AddMigrationStrategy(new XslStringMigrator(5, 7, e.Xsl1To2));

						Assert.AreEqual(1, migrator.MaximumVersionThatFileCanBeMigratedTo);
					}
				}
			}
		}

		[Test]
		public void MaximumVersionThatFileCanBeMigratedTo_MultipleConsecutiveMigratorsFromfileVersionExist_ReturnsVersionOfHighestAchievableVersion()
		{
			using (var e = new EnvironmentForTest())
			{
				using (var folder = new TemporaryFolder("MigratorTests"))
				{
					using (var sourceFile = folder.GetNewTempFile(true))
					{
						File.WriteAllText(sourceFile.Path, e.XmlVersion1);
						var migrator = new Migrator(7, sourceFile.Path);
						migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
						migrator.AddMigrationStrategy(new XslStringMigrator(1, 5, e.Xsl1To2));
						migrator.AddMigrationStrategy(new XslStringMigrator(5, 7, e.Xsl1To2));

						Assert.AreEqual(7, migrator.MaximumVersionThatFileCanBeMigratedTo);
					}
				}
			}
		}

		[Test]
		public void MaximumVersionThatFileCanBeMigratedTo_FileIsOfVersionThatCanNotBeIdentified_ReturnsMinusOne()
		{
			using (var e = new EnvironmentForTest())
			{
				using (var folder = new TemporaryFolder("MigratorTests"))
				{
					using (var sourceFile = folder.GetNewTempFile(true))
					{
						File.WriteAllText(sourceFile.Path, e.XmlVersion0);
						var migrator = new Migrator(7, sourceFile.Path);
						migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
						migrator.AddMigrationStrategy(new XslStringMigrator(1, 2, e.Xsl1To2));

						Assert.AreEqual(-1, migrator.MaximumVersionThatFileCanBeMigratedTo);
					}
				}
			}
			throw new NotImplementedException("Not sure what to do. Return -1, throw?");
		}

		[Test]
		public void Migrate_MissingMigrationStrategy_LeavesWipFiles()
		{
			using (var e = new EnvironmentForTest())
			{
				using (var folder = new TemporaryFolder("MigratorTests"))
				{
					using (var sourceFile = folder.GetNewTempFile(true))
					{
						File.WriteAllText(sourceFile.Path, e.XmlVersion1);
						var migrator = new Migrator(3, sourceFile.Path);
						migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
						migrator.AddMigrationStrategy(new XslStringMigrator(1, 2, e.Xsl1To2));

						Assert.Throws<InvalidOperationException>(() => migrator.Migrate());

						var files = Directory.GetFiles(folder.Path);
						Assert.That(files, Has.Some.SamePath(sourceFile.Path));
						Assert.That(files, Has.Some.SamePath(sourceFile.Path + ".bak"));
						Assert.That(files, Has.Some.SamePath(sourceFile.Path + ".Migrate_1_2"));
					}
				}
			}
		}

		[Test]
		// No wip files left behind
		public void Migrate_Succeeds_CleansUpWipFilesAndBackupFiles()
		{
			using (var e = new EnvironmentForTest())
			{
				using (var folder = new TemporaryFolder("MigratorTests"))
				{
					using (var sourceFile = folder.GetNewTempFile(true))
					{
						File.WriteAllText(sourceFile.Path, e.XmlVersion1);
						var migrator = new Migrator(3, sourceFile.Path);
						migrator.AddVersionStrategy(new XPathVersion(3, "/configuration/@version"));
						migrator.AddMigrationStrategy(new XslStringMigrator(1, 2, e.Xsl1To2));
						migrator.AddMigrationStrategy(new XslStringMigrator(2, 3, e.Xsl2To3));

						migrator.Migrate();

						var files = Directory.GetFiles(folder.Path);

						Assert.That(files.Length, Is.EqualTo(1));
						Assert.That(files, Has.Some.SamePath(sourceFile.Path));

					}
				}
			}
		}

	}
}
