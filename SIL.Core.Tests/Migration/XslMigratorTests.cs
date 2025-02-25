using System;
using System.IO;
using NUnit.Framework;
using SIL.IO;
using SIL.Migration;
using SIL.TestUtilities;

namespace SIL.Tests.Migration
{
	[TestFixture]
	public class XslMigratorTests
	{

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
		public void Constructor_FromLessThanTo_Throws()
		{
			Assert.Throws<ArgumentException>(
				() => { _ = new XslStringMigrator(5, 4, null); }
			);
		}

		[Test]
		public void Migrate_TestData_TransformsOk()
		{
			string xsl = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
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

			string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration version='1'>
  <blah />
</configuration>
".Replace("'", "\"");

			using (var sourceFile = new TempFile(xml))
			{
				using (var destinationFile = new TempFile())
				{
					var migrator = new XslStringMigrator(1, 2, xsl);
					migrator.Migrate(sourceFile.Path, destinationFile.Path);
					AssertThatXmlIn.File(destinationFile.Path).HasAtLeastOneMatchForXpath("configuration[@version='2']");
					AssertThatXmlIn.File(destinationFile.Path).HasAtLeastOneMatchForXpath("/configuration/blah");
				}
			}
		}
	}
}
